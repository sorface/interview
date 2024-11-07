import { useCallback, useContext, useEffect, useRef, useState } from 'react';
import Peer from 'simple-peer';
import toast from 'react-hot-toast';
import { User, UserType } from '../../../../../types/user';
import { usePeerStream } from '../../../hooks/usePeerStream';
import { AuthContext } from '../../../../../context/AuthContext';
import { createAudioAnalyser, frequencyBinCount } from '../utils/createAudioAnalyser';
import { getAverageVolume } from '../utils/getAverageVolume';
import { ParsedWsMessage } from '../../../utils/parseWsMessage';
import { checkIsAudioStream } from '../utils/checkIsAudioStream';
import { useLocalizationCaptions } from '../../../../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../../../../localization';

interface UseVideoChatProps {
  viewerMode: boolean;
  lastWsMessageParsed: ParsedWsMessage | null;
  userAudioStream: MediaStream | null;
  userVideoStream: MediaStream | null;
  sendWsMessage: (message: string) => void;
  playJoinRoomSound: () => void;
}

export interface PeerMeta {
  peerID: string;
  nickname: string;
  avatar: string;
  peer: Peer.Instance;
  targetUserId: string;
  participantType: UserType;
  screenShare: boolean;
}

const audioVolumeThreshold = 10.0;
const updateLoudedUserTimeout = 5000;

const removeDuplicates = (peersRef: React.MutableRefObject<PeerMeta[]>, newPeerMeta: PeerMeta) =>
  peersRef.current.filter(peer =>
    peer.peerID !== newPeerMeta.peerID ? true : peer.screenShare !== newPeerMeta.screenShare
  );

const getAllUsers = (data: PeerMeta[], auth: User | null) => {
  const users: Map<User['id'], Pick<User, 'nickname' | 'avatar'>> = new Map();
  data.forEach(peer => {
    users.set(peer.peerID, {
      nickname: peer.nickname,
      avatar: peer.avatar,
    });
  });
  if (auth) {
    users.set(auth.id, {
      nickname: auth.nickname,
      avatar: auth.avatar,
    });
  }
  return users;
};

export const useVideoChat = ({
  viewerMode,
  lastWsMessageParsed,
  userAudioStream,
  userVideoStream,
  sendWsMessage,
  playJoinRoomSound,
}: UseVideoChatProps) => {
  const auth = useContext(AuthContext);
  const localizationCaptions = useLocalizationCaptions();

  const [peers, setPeers] = useState<PeerMeta[]>([]);
  const { peerToStream, addPeerStream, removePeerStream } = usePeerStream();
  const updateLouderUserTimeout = useRef(0);
  const allUsers = getAllUsers(peers, auth);
  // screenShare
  // const screenSharePeer = peers.find(peer => peer.screenShare);
  const peersRef = useRef<PeerMeta[]>([]);
  const userIdToAudioAnalyser = useRef<Record<string, AnalyserNode>>({});
  const requestRef = useRef<number>();
  const louderUserId = useRef(auth?.id || '');
  const [videoOrder, setVideoOrder] = useState<Record<string, number>>({
    [auth?.id || '']: 1,
  });

  const createPeer = useCallback((to: string, forViewer?: boolean, screenShare?: boolean) => {
    if (viewerMode) {
      sendWsMessage(JSON.stringify({
        Type: 'sending signal',
        Value: JSON.stringify({
          To: to,
          Signal: 'fake-viewer-signal',
          ScreenShare: false,
        }),
      }));
      return new Peer();
    }

    const streams: MediaStream[] = [];
    userAudioStream && streams.push(userAudioStream);
    // ScreenShare
    // if (screenShare) {
    //   screenStream && streams.push(screenStream);
    // } else {
    userVideoStream && streams.push(userVideoStream);
    // }

    const peer = new Peer({
      initiator: true,
      trickle: false,
      streams,
      ...((forViewer || screenShare) && {
        offerOptions: {
          offerToReceiveAudio: false,
          offerToReceiveVideo: false,
        },
      }),
    });

    peer.on('signal', signal => {
      sendWsMessage(JSON.stringify({
        Type: 'sending signal',
        Value: JSON.stringify({
          To: to,
          Signal: JSON.stringify(signal),
          ScreenShare: screenShare,
        }),
      }));
    });

    return peer;
  }, [userAudioStream, userVideoStream, viewerMode, sendWsMessage]);

  useEffect(() => {
    const frequencyData = new Uint8Array(frequencyBinCount);
    let prevTime = performance.now();
    const updateAudioAnalyser = () => {
      const time = performance.now();
      const delta = time - prevTime;
      let newLouderUserId = '';
      let louderVolume = -1;
      const result: Record<string, number> = {};
      for (const [userId, analyser] of Object.entries(userIdToAudioAnalyser.current)) {
        analyser.getByteFrequencyData(frequencyData);
        const averageVolume = getAverageVolume(frequencyData);
        if (averageVolume < audioVolumeThreshold) {
          continue;
        }
        if (averageVolume > louderVolume) {
          newLouderUserId = userId;
          louderVolume = averageVolume;
        }
        result[userId] = averageVolume;
      }

      if (updateLouderUserTimeout.current > 0) {
        updateLouderUserTimeout.current -= delta;
        prevTime = time;
        requestRef.current = requestAnimationFrame(updateAudioAnalyser);
        return;
      }

      if (newLouderUserId && newLouderUserId !== louderUserId.current) {
        updateLouderUserTimeout.current = updateLoudedUserTimeout;
        setVideoOrder({
          [newLouderUserId]: 1,
          [louderUserId.current]: 2,
        });
        louderUserId.current = newLouderUserId;
      }
      prevTime = time;

      requestRef.current = requestAnimationFrame(updateAudioAnalyser);
    };
    requestRef.current = requestAnimationFrame(updateAudioAnalyser);

    return () => {
      if (requestRef.current) {
        cancelAnimationFrame(requestRef.current);
      }
    };

  }, [auth, louderUserId]);

  const addPeer = useCallback((incomingSignal: Peer.SignalData, callerID: string, screenShare?: boolean) => {
    const streams: MediaStream[] = [];
    if (!screenShare) {
      userAudioStream && streams.push(userAudioStream);
      userVideoStream && streams.push(userVideoStream);
    }

    const peer = new Peer({
      initiator: false,
      trickle: false,
      streams,
    });

    peer.on('signal', signal => {
      sendWsMessage(JSON.stringify({
        Type: 'returning signal',
        Value: JSON.stringify({
          To: callerID,
          Signal: JSON.stringify(signal),
          ScreenShare: screenShare,
        }),
      }));
    });

    peer.signal(incomingSignal);

    return peer;
  }, [userAudioStream, userVideoStream, sendWsMessage]);

  useEffect(() => {
    if (!lastWsMessageParsed || !auth) {
      return;
    }
    try {
      // ScreenShare
      // const screenShare = !!(parsedPayload?.ScreenShare);
      const screenShare = false;
      switch (lastWsMessageParsed?.Type) {
        case 'all users':
          lastWsMessageParsed.Value.forEach(userInChat => {
            if (userInChat.Id === auth.id) {
              return;
            }
            const peer = createPeer(userInChat.Id);
            const newPeerMeta: PeerMeta = {
              peerID: userInChat.Id,
              nickname: userInChat.Nickname,
              avatar: userInChat.Avatar,
              targetUserId: userInChat.Id,
              participantType: userInChat.ParticipantType,
              peer,
              screenShare: false,
            };

            addPeerStream(newPeerMeta.peerID, peer, false);
            peer.on('stream', (stream) => {
              const audioStream = checkIsAudioStream(stream);
              if (!audioStream) {
                return;
              }
              userIdToAudioAnalyser.current[newPeerMeta.targetUserId] = createAudioAnalyser(stream);
            });

            peersRef.current.push(newPeerMeta)
          });
          setPeers([...peersRef.current]);
          break;
        case 'user joined':
          const fromUser = lastWsMessageParsed.Value.From;
          if (!viewerMode && fromUser.ParticipantType === 'Viewer') {
            const peer = createPeer(fromUser.Id, true);
            const newPeerMeta: PeerMeta = {
              peerID: fromUser.Id,
              nickname: fromUser.Nickname,
              avatar: fromUser.Avatar,
              targetUserId: fromUser.Id,
              participantType: fromUser.ParticipantType,
              peer,
              screenShare: false,
            };

            addPeerStream(newPeerMeta.peerID, peer, false);
            peer.on('stream', (stream) => {
              const audioStream = checkIsAudioStream(stream);
              if (!audioStream) {
                return;
              }
              userIdToAudioAnalyser.current[newPeerMeta.targetUserId] = createAudioAnalyser(stream);
            });

            peersRef.current = removeDuplicates(peersRef, newPeerMeta);

            peersRef.current.push(newPeerMeta);
            setPeers([...peersRef.current]);
            break;
          }
          if (viewerMode && fromUser.ParticipantType === 'Viewer') {
            const peer = new Peer();
            const newPeerMeta: PeerMeta = {
              peerID: fromUser.Id,
              nickname: fromUser.Nickname,
              avatar: fromUser.Avatar,
              targetUserId: fromUser.Id,
              participantType: fromUser.ParticipantType,
              peer,
              screenShare: false,
            };
            peersRef.current = removeDuplicates(peersRef, newPeerMeta);
            peersRef.current.push(newPeerMeta);
            setPeers([...peersRef.current]);
            break;
          }
          if (viewerMode || screenShare) {
            const peer = addPeer(JSON.parse(lastWsMessageParsed.Value.Signal), fromUser.Id, screenShare);
            addPeerStream(fromUser.Id, peer, true);
            const newPeerMeta: PeerMeta = {
              peerID: fromUser.Id,
              nickname: fromUser.Nickname,
              avatar: fromUser.Avatar,
              targetUserId: fromUser.Id,
              participantType: fromUser.ParticipantType,
              peer,
              screenShare,
            };
            peersRef.current = removeDuplicates(peersRef, newPeerMeta);
            peersRef.current.push(newPeerMeta);

            peer.on('stream', (stream) => {
              const audioStream = checkIsAudioStream(stream);
              if (!audioStream || screenShare) {
                return;
              }
              userIdToAudioAnalyser.current[newPeerMeta.targetUserId] = createAudioAnalyser(stream);
            });
            setPeers([...peersRef.current]);
            break;
          }
          const peer = addPeer(JSON.parse(lastWsMessageParsed.Value.Signal), fromUser.Id);
          addPeerStream(fromUser.Id, peer, true);
          const newPeerMeta: PeerMeta = {
            peerID: fromUser.Id,
            nickname: fromUser.Nickname,
            avatar: fromUser.Avatar,
            targetUserId: fromUser.Id,
            participantType: fromUser.ParticipantType,
            peer,
            screenShare,
          };
          peersRef.current = removeDuplicates(peersRef, newPeerMeta);
          peersRef.current.push(newPeerMeta);

          peer.on('stream', (stream) => {
            const audioStream = checkIsAudioStream(stream);
            if (!audioStream || screenShare) {
              return;
            }
            userIdToAudioAnalyser.current[newPeerMeta.targetUserId] = createAudioAnalyser(stream);
          });
          setPeers([...peersRef.current]);
          break;
        case 'user left':
          const leftUserId = lastWsMessageParsed.Value.Id;
          const leftUserPeer = peersRef.current.find(p => p.targetUserId === leftUserId);
          if (leftUserPeer) {
            removePeerStream(leftUserPeer.peerID);
            leftUserPeer.peer.destroy();
          }
          const peersAfterLeft = peersRef.current.filter(p => p.targetUserId !== leftUserId);
          peersRef.current = peersAfterLeft;
          setPeers([...peersRef.current]);
          setVideoOrder({
            [auth.id]: 1,
            [louderUserId.current]: 2,
          });
          break;
        case 'receiving returned signal':
          const item = peersRef.current.find(p =>
            p.peerID === lastWsMessageParsed.Value.From && (screenShare ? p.screenShare : true)
          );
          if (item) {
            item.peer.signal(lastWsMessageParsed.Value.Signal);
          }
          break;
        default:
          break;
      }
    } catch (err) {
      console.error('parse ws message error: ', err);
    }
  }, [auth, lastWsMessageParsed, viewerMode, addPeer, createPeer, addPeerStream, removePeerStream]);

  // ScreenShare  
  // useEffect(() => {
  //   if (screenStream && auth?.id) {
  //     try {
  //       peersRef.current.forEach(peer => {
  //         if (peer.targetUserId === auth.id) {
  //           return;
  //         }
  //         const newPeer = createPeer(peer.targetUserId, false, true);
  //         const newPeerMeta: PeerMeta = {
  //           peerID: peer.targetUserId,
  //           nickname: peer.nickname,
  //           avatar: peer.avatar,
  //           targetUserId: peer.targetUserId,
  //           participantType: peer.participantType,
  //           peer: newPeer,
  //           screenShare: true,
  //         };

  //         peersRef.current.push(newPeerMeta);
  //         setPeers([...peersRef.current]);
  //       });
  //     } catch (e) {
  //       console.error('add screenStream error: ', e);
  //     }
  //   }
  // }, [auth, screenStream, createPeer]);

  useEffect(() => {
    if (!lastWsMessageParsed) {
      return;
    }
    try {
      switch (lastWsMessageParsed?.Type) {
        case 'user joined':
          const fromUser = lastWsMessageParsed.Value.From;
          toast.success(
            `${fromUser.Nickname} ${localizationCaptions[LocalizationKey.UserConnectedToRoom]}`
          );
          playJoinRoomSound();
          break;
        default:
          break;
      }
    } catch (err) {
      console.error('parse ws message error: ', err);
    }
  }, [lastWsMessageParsed, localizationCaptions, playJoinRoomSound]);

  useEffect(() => {
    if (!userAudioStream || !auth?.id) {
      return;
    }
    try {
      userIdToAudioAnalyser.current[auth.id] = createAudioAnalyser(userAudioStream);
    } catch { }
  }, [userAudioStream, auth?.id]);

  return {
    peers,
    videoOrder,
    peerToStream,
    allUsers,
  };
};
