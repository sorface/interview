import { FunctionComponent, useEffect, useRef, useState } from 'react';
import Peer from 'simple-peer';
import { Loader } from '../../../../components/Loader/Loader';

interface VideoChatVideoProps {
  peer: Peer.Instance;
}

export const VideoChatVideo: FunctionComponent<VideoChatVideoProps> = ({
  peer,
}) => {
  const refVideo = useRef<HTMLVideoElement>(null);
  const refAudio = useRef<HTMLVideoElement>(null);
  const [loading, setLoading] = useState(true);

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
      setLoading(false);
    };
    peer.on('stream', handleStream);

    return () => {
      peer.off('stream', handleStream);
    };
  }, [peer]);

  return (
    <>
      {loading ? (
        <div className='flex items-center h-full justify-center'>
          <Loader />
        </div>
      ) : (
        <>
          <video playsInline autoPlay ref={refVideo} className='videochat-video' />
          <audio playsInline autoPlay ref={refAudio} />
        </>
      )}
    </>
  );
};
