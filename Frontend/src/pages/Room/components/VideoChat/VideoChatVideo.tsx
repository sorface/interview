import { FunctionComponent, useEffect, useRef } from 'react';
import { Loader } from '../../../../components/Loader/Loader';

interface VideoChatVideoProps {
  loaded?: boolean;
  audioStream?: MediaStream;
  videoStream?: MediaStream;
}

export const VideoChatVideo: FunctionComponent<VideoChatVideoProps> = ({
  loaded,
  audioStream,
  videoStream,
}) => {
  const loading = typeof loaded === 'boolean' ? !loaded : false;
  const refVideo = useRef<HTMLVideoElement>(null);
  const refAudio = useRef<HTMLVideoElement>(null);

  useEffect(() => {
    if (audioStream && refAudio.current) {
      refAudio.current.srcObject = audioStream;
    }
  }, [audioStream]);

  useEffect(() => {
    if (videoStream && refVideo.current) {
      refVideo.current.srcObject = videoStream;
    }
  }, [videoStream]);

  return (
    <>
      {(!videoStream || loading) && (
        <div className='flex items-center h-full justify-center'>
          <Loader />
        </div>
      )}
      <>
        <video playsInline autoPlay ref={refVideo} className='videochat-video object-cover' />
        <audio playsInline autoPlay ref={refAudio} />
      </>
    </>
  );
};
