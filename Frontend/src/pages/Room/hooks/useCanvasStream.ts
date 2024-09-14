import { useCallback, useContext, useEffect, useRef, useState } from 'react';
import { Results } from '@mediapipe/selfie_segmentation';
import { Camera } from '@mediapipe/camera_utils';
import { useSelfieSegmentation } from './useSelfieSegmentation';
import { AuthContext } from '../../../context/AuthContext';

interface UseCanvasStreamParams {
  enabled: boolean;
  width: number;
  height: number;
  frameRate: number;
  cameraStream: MediaStream | null;
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
  enabled,
  width,
  height,
  frameRate,
  cameraStream,
}: UseCanvasStreamParams) => {
  const auth = useContext(AuthContext);
  const [context, setContext] = useState<CanvasRenderingContext2D | null>(null);
  const [canvasMediaStream, setMediaStream] = useState(new MediaStream());
  const [video, setVideo] = useState<HTMLVideoElement | null>(null);
  const [backgroundImage, setBackgroundImage] = useState<HTMLImageElement | null>(null);
  const requestRef = useRef<number>();

  const onResults = useCallback((results: Results) => {
    if (!context || !cameraStream || !video) {
      return;
    }
    const canvasElement = context.canvas;
    context.save();
    context.clearRect(0, 0, canvasElement.width, canvasElement.height);

    context.drawImage(results.image, 0, 0, canvasElement.width, canvasElement.height);

    context.globalCompositeOperation = 'destination-in';
    context.drawImage(results.segmentationMask, 0, 0, canvasElement.width, canvasElement.height);

    if (backgroundImage) {
      context.globalCompositeOperation = 'destination-over';
      context.drawImage(backgroundImage, 0, 0, canvasElement.width, canvasElement.height);
    }

    context.restore();
  }, [context, cameraStream, video, backgroundImage]);

  const selfieSegmentation = useSelfieSegmentation(onResults);

  useEffect(() => {
    if (backgroundImage) {
      return;
    }
    const newImg = new Image();
    newImg.onload = () => {
      setBackgroundImage(newImg);
    };
    newImg.src = '/logo512.png';
  }, [backgroundImage]);

  useEffect(() => {
    if (!video || !cameraStream || !context) {
      return;
    }
    const newCamera = new Camera(video, {
      onFrame: async () => {
        if (video && cameraStream && selfieSegmentation) {
          await selfieSegmentation.send({ image: video });
        }
      },
      width: video.width,
      height: video.height,
    });
    newCamera.start();

    return () => {
      newCamera.stop().then(() => {
        cameraStream.getTracks().forEach(track => track.stop());
        fillNoCamera(context, auth?.nickname || null);
      });
    };
  }, [video, context, width, height, cameraStream, selfieSegmentation, auth?.nickname]);


  useEffect(() => {
    if (!enabled) {
      return;
    }
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
  }, [frameRate, height, width, enabled, auth?.nickname]);

  useEffect(() => {
    if (!context) {
      return;
    }
    const triggerCanvasUpdate = () => {
      if (cameraStream && video) {
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
  }, [context, video, cameraStream, auth?.nickname]);

  useEffect(() => {
    if (cameraStream && video && context) {
      video.srcObject = cameraStream;
      video.play();
      return;
    }
    if (!context) {
      return;
    }
    fillNoCamera(context, auth?.nickname || null);
  }, [cameraStream, context, video, auth?.nickname]);

  return canvasMediaStream;
};
