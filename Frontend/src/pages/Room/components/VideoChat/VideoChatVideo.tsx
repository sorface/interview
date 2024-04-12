import { FunctionComponent, useEffect, useRef } from 'react';
import Peer from 'simple-peer';

interface VideoChatVideoProps {
  peer: Peer.Instance;
}

export const VideoChatVideo: FunctionComponent<VideoChatVideoProps> = ({
  peer,
}) => {
  const refVideo = useRef<HTMLVideoElement>(null);
  const refAudio = useRef<HTMLVideoElement>(null);

  useEffect(() => {
    const handleStream = (stream: MediaStream) => {
      const audioStream = !!stream.getAudioTracks().length;
      const videoStream = !!stream.getVideoTracks().length;

      if (videoStream && refVideo.current) {
        refVideo.current.srcObject = stream;
      }

      if (audioStream && refAudio.current) {
        refAudio.current.srcObject = stream;
      }
    };
    peer.on('stream', handleStream);

    return () => {
      peer.off('stream', handleStream);
    };
  }, [peer]);

  return (
    <>
      <video playsInline autoPlay ref={refVideo} className='videochat-video' />
      <audio playsInline autoPlay ref={refAudio} />
    </>
  );
};
