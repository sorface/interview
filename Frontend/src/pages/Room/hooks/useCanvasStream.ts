import {
  useCallback,
  useContext,
  useEffect,
  useRef,
  useState,
  Dispatch,
  SetStateAction,
} from 'react';
import { GpuBuffer, Results } from '@mediapipe/selfie_segmentation';
import { useSelfieSegmentation } from './useSelfieSegmentation';
import { AuthContext } from '../../../context/AuthContext';

interface UseCanvasStreamParams {
  enabled: boolean;
  width: number;
  height: number;
  frameRate: number;
  cameraStream: MediaStream | null;
  backgroundRemoveEnabled: boolean;
}

const loadAvatar = (
  url: string,
  setAvatar: Dispatch<SetStateAction<HTMLImageElement | null>>,
): void => {
  const image = new Image();
  image.crossOrigin = 'anonymous';
  image.onload = () => setAvatar(image);
  image.src = url;
};

const fillNoCamera = (
  context: CanvasRenderingContext2D,
  nickname: string,
  avatar: HTMLImageElement | null,
) => {
  const canvasWidth = context.canvas.width;
  const canvasHeight = context.canvas.height;

  context.fillStyle = 'black';
  context.fillRect(0, 0, canvasWidth, canvasHeight);

  const drawAvatar = (img: HTMLImageElement) => {
    const scale = Math.min(canvasWidth / img.width, canvasHeight / img.height);
    const x = (canvasWidth - img.width * scale) / 2;
    const y = (canvasHeight - img.height * scale) / 2;

    context.drawImage(img, x, y, img.width * scale, img.height * scale);
  };

  const drawNickname = () => {
    const rectHeight = 24;
    const paddingY = canvasHeight / 2 - rectHeight + 36;
    const x = Math.round(canvasWidth / 2);

    context.fillStyle = 'white';
    context.textAlign = 'center';
    context.font = 'bold 26px sans-serif';
    context.fillText(nickname, x, paddingY, context.canvas.width);
  };

  if (avatar) {
    drawAvatar(avatar);
  } else {
    drawNickname();
  }
};

export const useCanvasStream = ({
  enabled,
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
  const [avatar, setAvatar] = useState<HTMLImageElement | null>(null);
  const requestRef = useRef<number>();
  const backgroundRemoveEnabledRef = useRef(backgroundRemoveEnabled);
  const videoLoadedRef = useRef(false);
  const lastSegmentationMaskRef = useRef<GpuBuffer | null>(null);
  const lastSegmentationResultProcessedRef = useRef(true);

  const userNickname = auth?.nickname ?? 'no camera';

  const onResults = useCallback(
    (results: Results) => {
      if (!context || !cameraStream || !video) {
        return;
      }
      lastSegmentationMaskRef.current = results.segmentationMask;
      lastSegmentationResultProcessedRef.current = true;
    },
    [context, cameraStream, video],
  );

  const selfieSegmentationEnabled = Boolean(cameraStream && video);
  const selfieSegmentation = useSelfieSegmentation(
    selfieSegmentationEnabled,
    onResults,
  );

  useEffect(() => {
    backgroundRemoveEnabledRef.current = backgroundRemoveEnabled;
  }, [backgroundRemoveEnabled]);

  useEffect(() => {
    if (!cameraStream || !video) {
      return;
    }
    videoLoadedRef.current = false;
    video.onloadeddata = () => {
      videoLoadedRef.current = true;
    };
    video.srcObject = cameraStream;
    video.play();
    lastSegmentationResultProcessedRef.current = true;
    lastSegmentationMaskRef.current = null;
  }, [video, cameraStream]);

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
    canvasContext.imageSmoothingEnabled = true;
    const stream = canvas.captureStream(frameRate);
    const videoTrack = stream.getVideoTracks()[0];
    videoTrack.enabled = true;
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
        const canvasElement = context.canvas;
        context.clearRect(0, 0, canvasElement.width, canvasElement.height);
        context.save();
        context.filter = 'blur(0)';

        if (
          lastSegmentationMaskRef.current &&
          backgroundRemoveEnabledRef.current
        ) {
          context.drawImage(
            lastSegmentationMaskRef.current,
            0,
            0,
            canvasElement.width,
            canvasElement.height,
          );
        }

        context.scale(-1, 1);
        if (backgroundRemoveEnabledRef.current) {
          context.globalCompositeOperation = 'source-in';
          context.drawImage(
            video,
            0,
            0,
            -canvasElement.width,
            canvasElement.height,
          );
        }

        context.globalCompositeOperation = 'destination-atop';
        if (backgroundRemoveEnabledRef.current) {
          context.filter = 'blur(8px)';
        }

        context.drawImage(
          video,
          0,
          0,
          -canvasElement.width,
          canvasElement.height,
        );
        context.scale(1, 1);

        context.restore();

        if (
          lastSegmentationResultProcessedRef.current &&
          selfieSegmentation &&
          videoLoadedRef.current
        ) {
          lastSegmentationResultProcessedRef.current = false;
          selfieSegmentation.send({ image: video });
        }
      } else {
        fillNoCamera(context, userNickname, avatar);
      }
      requestRef.current = requestAnimationFrame(triggerCanvasUpdate);
    };
    requestRef.current = requestAnimationFrame(triggerCanvasUpdate);
    return () => {
      if (requestRef.current) {
        cancelAnimationFrame(requestRef.current);
      }
    };
  }, [context, video, avatar, userNickname, cameraStream, selfieSegmentation]);

  useEffect(() => {
    if (auth?.avatar) loadAvatar(auth?.avatar, setAvatar);
  }, [auth?.avatar]);

  return canvasMediaStream;
};
