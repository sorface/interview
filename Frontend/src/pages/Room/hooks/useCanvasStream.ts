import { useCallback, useContext, useEffect, useRef, useState } from 'react';
import { Results } from '@mediapipe/selfie_segmentation';
import { Camera } from '@mediapipe/camera_utils';
import { useSelfieSegmentation } from './useSelfieSegmentation';
import { AuthContext } from '../../../context/AuthContext';

interface UseCanvasStreamParams {
  width: number;
  height: number;
  frameRate: number;
  cameraStream: MediaStream | null;
  backgroundRemoveEnabled: boolean;
}

const fillNoCamera = (context: CanvasRenderingContext2D, nickname: string | null) => {
  context.fillStyle = 'black';
  context.fillRect(0, 0, context.canvas.width, context.canvas.height);

  const rectHeight = 24;
  const paddingY = context.canvas.height / 2 - rectHeight + 36;
  const x = Math.round(context.canvas.width / 2);
  context.fillStyle = 'white';
  context.textAlign = 'center';
  context.font = 'bold 26px sans-serif';
  context.fillText(nickname || 'no camera', x, paddingY, context.canvas.width);
};

export const useCanvasStream = ({
  width,
  height,
  frameRate,
  cameraStream,
  backgroundRemoveEnabled,
}: UseCanvasStreamParams) => {
  const auth = useContext(AuthContext);
  const [context, setContext] = useState<CanvasRenderingContext2D | null>(null);
  const [canvasMediaStream, setMediaStream] = useState(new MediaStream());
  const [video, setVideo] = useState<HTMLVideoElement | null>(null);
  const requestRef = useRef<number>();
  const backgroundRemoveEnabledRef = useRef(false);
  backgroundRemoveEnabledRef.current = backgroundRemoveEnabled;

  const onResults = useCallback((results: Results) => {
    if (!context || !cameraStream || !video) {
      return;
    }
    const canvasElement = context.canvas;
    context.clearRect(0, 0, canvasElement.width, canvasElement.height);
    context.save();
    context.filter = 'blur(0)';

    context.drawImage(results.segmentationMask, 0, 0, canvasElement.width, canvasElement.height);
    context.globalCompositeOperation = 'source-in';
    context.drawImage(results.image, 0, 0, canvasElement.width, canvasElement.height);
    context.globalCompositeOperation = 'destination-atop';
    context.filter = 'blur(8px)'

    context.drawImage(results.image, 0, 0, canvasElement.width, canvasElement.height);

    context.restore();
  }, [context, cameraStream, video]);

  const selfieSegmentationEnabled = Boolean(backgroundRemoveEnabled && cameraStream && video);
  const selfieSegmentation = useSelfieSegmentation(
    selfieSegmentationEnabled,
    onResults,
  );

  useEffect(() => {
    if (!video || !cameraStream || !context) {
      return;
    }
    const newCamera = new Camera(video, {
      onFrame: async () => {
        if (video && cameraStream && selfieSegmentation && backgroundRemoveEnabledRef.current) {
          try {
            await selfieSegmentation.send({ image: video });
          } catch (err) {
            console.warn(err);
          }
        }
      },
      width: video.width,
      height: video.height,
    });
    newCamera.start();

    return () => {
      newCamera.stop().then(() => {
        cameraStream.getTracks().forEach(track => track.stop());
      });
    };
  }, [video, context, width, height, cameraStream, selfieSegmentation, auth?.nickname]);

  useEffect(() => {
    const canvas = document.createElement('canvas');
    const newVideo = document.createElement('video');
    newVideo.width = canvas.width = width;
    newVideo.height = canvas.height = height;
    newVideo.muted = true;
    newVideo.autoplay = true;
    newVideo.playsInline = true;
    const canvasContext = canvas.getContext('2d');
    if (!canvasContext) {
      throw new Error('Failed to get context for blank stream');
    }
    const stream = canvas.captureStream(frameRate);
    const videoTrack = stream.getVideoTracks()[0];
    videoTrack.enabled = true;
    fillNoCamera(canvasContext, auth?.nickname || null);
    setVideo(newVideo);
    setContext(canvasContext);
    setMediaStream(new MediaStream([videoTrack]));
  }, [frameRate, height, width, auth?.nickname]);

  useEffect(() => {
    if (!context) {
      return;
    }
    const triggerCanvasUpdate = () => {
      if (cameraStream && video) {
        if (!backgroundRemoveEnabled) {
          context.drawImage(video, 0, 0, context.canvas.width, context.canvas.height);
        }
        requestRef.current = requestAnimationFrame(triggerCanvasUpdate);
        return;
      }
      fillNoCamera(context, auth?.nickname || null);
      requestRef.current = requestAnimationFrame(triggerCanvasUpdate);
    };
    requestRef.current = requestAnimationFrame(triggerCanvasUpdate);
    return () => {
      if (requestRef.current) {
        cancelAnimationFrame(requestRef.current);
      }
    };
  }, [context, video, cameraStream, auth?.nickname, backgroundRemoveEnabled]);

  return canvasMediaStream;
};
