import { FunctionComponent, useEffect, useRef } from 'react';
import Peer from 'simple-peer';

interface VideoChatVideoProps {
  peer: Peer.Instance;
}

export const VideoChatVideo: FunctionComponent<VideoChatVideoProps> = ({
  peer,
}) => {
  const ref = useRef<HTMLVideoElement>(null);

  useEffect(() => {
    const handleStream = (stream: MediaStream) => {
      if (ref.current) {
        ref.current.srcObject = stream;
      }
    };
    peer.on('stream', handleStream);

    return () => {
      peer.off('stream', handleStream);
    };
  }, [peer]);

  return (
    <video playsInline autoPlay ref={ref} className='videochat-video' />
  );
};
