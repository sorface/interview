import { useEffect, useRef, useState } from 'react';

interface UseCanvasStreamParams {
  enabled: boolean;
  width: number;
  height: number;
  frameRate: number;
  cameraStream: MediaStream | null;
}

const fillLines = (context: CanvasRenderingContext2D, width: number, height: number) => {
  const lines = ['white', 'yellow', 'aqua', 'lime', 'fuchsia', 'red', 'blue'];
  const lineWidth = Math.round(width / lines.length);
  for (let i = lines.length; i--;) {
    const lineColor = lines[i];
    context.fillStyle = lineColor;
    context.fillRect(i * lineWidth, 0, lineWidth, height);
  }
};

const fillNoCamera = (context: CanvasRenderingContext2D, width: number, height: number) => {
  context.fillStyle = 'black';
  const paddingX = 14;
  const rectHeight = 24;
  const paddingY = height / 2 - rectHeight;
  context.fillRect(paddingX, paddingY, width - paddingX * 2, height - paddingY * 2);
  context.fillStyle = 'white';
  context.font = 'bold 36px sans-serif';
  context.fillText('NO CAMERA', paddingX * 2 + 10, paddingY + 36);
};

export const useCanvasStream = ({
  enabled,
  width,
  height,
  frameRate,
  cameraStream,
}: UseCanvasStreamParams) => {
  const [context, setContext] = useState<CanvasRenderingContext2D | null>(null);
  const [canvasMediaStream, setMediaStream] = useState(new MediaStream());
  const [video, setVideo] = useState<HTMLVideoElement | null>(null);
  const requestRef = useRef<number>();

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
    fillLines(canvasContext, width, height);
    fillNoCamera(canvasContext, width, height);
    setVideo(newVideo);
    setContext(canvasContext);
    setMediaStream(new MediaStream([videoTrack]));
  }, [frameRate, height, width, enabled]);

  useEffect(() => {
    if (!context) {
      return;
    }
    const triggerCanvasUpdate = () => {
      if (cameraStream && video) {
        context.drawImage(video, 0, 0, video.width, video.height);
      } else {
        context.fillRect(0, 0, 1, 1);
      }
      requestRef.current = requestAnimationFrame(triggerCanvasUpdate);
    };
    requestRef.current = requestAnimationFrame(triggerCanvasUpdate);
    return () => {
      if (requestRef.current) {
        cancelAnimationFrame(requestRef.current);
      }
    };
  }, [context, video, cameraStream]);

  useEffect(() => {
    if (cameraStream && video && context) {
      video.srcObject = cameraStream;
      video.play();
      return;
    }
    if (!context) {
      return;
    }
    fillLines(context, width, height);
    fillNoCamera(context, width, height);
  }, [cameraStream, context, video, width, height]);

  return canvasMediaStream;
};
