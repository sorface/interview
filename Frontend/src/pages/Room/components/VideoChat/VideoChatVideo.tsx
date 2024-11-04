import { FunctionComponent, useEffect, useRef } from 'react';
import { Loader } from '../../../../components/Loader/Loader';

interface VideoChatVideoProps {
  loaded?: boolean;
  cover?: boolean;
  blurBg?: boolean;
  audioStream?: MediaStream;
  videoStream?: MediaStream;
}

export const VideoChatVideo: FunctionComponent<VideoChatVideoProps> = ({
  loaded,
  cover,
  blurBg,
  audioStream,
  videoStream,
}) => {
  const loading = typeof loaded === 'boolean' ? !loaded : false;
  const refVideo = useRef<HTMLVideoElement>(null);
  const refVideoBgBlur = useRef<HTMLVideoElement>(null);
  const refAudio = useRef<HTMLVideoElement>(null);

  useEffect(() => {
    if (audioStream && refAudio.current) {
      refAudio.current.srcObject = audioStream;
    }
  }, [audioStream]);

  useEffect(() => {
    if (!videoStream) {
      return;
    }
    if (refVideo.current) {
      refVideo.current.srcObject = videoStream;
    }
    if (refVideoBgBlur.current) {
      refVideoBgBlur.current.srcObject = videoStream;
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
        {blurBg && (
          <video
            ref={refVideoBgBlur}
            className='absolute w-full h-full object-cover blur-lg'
            muted
            autoPlay
            playsInline
          >
            Video not supported
          </video>
        )}
        <video playsInline autoPlay ref={refVideo} className={`videochat-video relative ${cover ? 'object-cover' : ''}`} />
        <audio playsInline autoPlay ref={refAudio} />
      </>
    </>
  );
};
