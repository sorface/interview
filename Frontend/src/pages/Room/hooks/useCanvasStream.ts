import {
  useCallback,
  useContext,
  useEffect,
  useRef,
  useState,
  Dispatch,
  SetStateAction,
} from 'react';
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
  const backgroundRemoveEnabledRef = useRef(false);
  backgroundRemoveEnabledRef.current = backgroundRemoveEnabled;

  const userNickname = auth?.nickname ?? 'no camera';

  const onResults = useCallback(
    (results: Results) => {
      if (!context || !cameraStream || !video) {
        return;
      }
      const canvasElement = context.canvas;
      context.clearRect(0, 0, canvasElement.width, canvasElement.height);
      context.save();
      context.filter = 'blur(0)';

      context.drawImage(
        results.segmentationMask,
        0,
        0,
        canvasElement.width,
        canvasElement.height,
      );
      context.globalCompositeOperation = 'source-in';
      context.drawImage(
        results.image,
        0,
        0,
        canvasElement.width,
        canvasElement.height,
      );
      context.globalCompositeOperation = 'destination-atop';
      context.filter = 'blur(8px)';

      context.drawImage(
        results.image,
        0,
        0,
        canvasElement.width,
        canvasElement.height,
      );

      context.restore();
    },
    [context, cameraStream, video],
  );

  const selfieSegmentationEnabled = Boolean(
    backgroundRemoveEnabled && cameraStream && video,
  );
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
        if (
          video &&
          cameraStream &&
          selfieSegmentation &&
          backgroundRemoveEnabledRef.current
        ) {
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
        cameraStream.getTracks().forEach((track) => track.stop());
      });
    };
  }, [video, context, width, height, cameraStream, selfieSegmentation]);

  useEffect(() => {
    const canvas = document.createElement('canvas');
    canvas.width = width;
    canvas.height = height;
    const canvasContext = canvas.getContext('2d');
    if (!canvasContext) {
      throw new Error('Failed to get context for blank stream');
    }
    setContext(canvasContext);
  }, [width, height]);

  useEffect(() => {
    if (!enabled || !context) {
      return;
    }
    const newVideo = document.createElement('video');
    newVideo.width = width;
    newVideo.height = height;
    newVideo.muted = true;
    newVideo.autoplay = true;
    newVideo.playsInline = true;
    const stream = context.canvas.captureStream(frameRate);
    const videoTrack = stream.getVideoTracks()[0];
    videoTrack.enabled = true;
    fillNoCamera(context, userNickname, avatar);
    setVideo(newVideo);
    setMediaStream(new MediaStream([videoTrack]));
  }, [frameRate, height, width, userNickname, enabled, avatar, context]);

  useEffect(() => {
    if (!context) {
      return;
    }
    const triggerCanvasUpdate = () => {
      if (cameraStream && video) {
        if (!backgroundRemoveEnabled) {
          context.drawImage(
            video,
            0,
            0,
            context.canvas.width,
            context.canvas.height,
          );
        }
        requestRef.current = requestAnimationFrame(triggerCanvasUpdate);
        return;
      }
      fillNoCamera(context, userNickname, avatar);
      requestRef.current = requestAnimationFrame(triggerCanvasUpdate);
    };
    requestRef.current = requestAnimationFrame(triggerCanvasUpdate);
    return () => {
      if (requestRef.current) {
        cancelAnimationFrame(requestRef.current);
      }
    };
  }, [
    context,
    video,
    cameraStream,
    userNickname,
    avatar,
    backgroundRemoveEnabled,
  ]);

  useEffect(() => {
    if (auth?.avatar) loadAvatar(auth?.avatar, setAvatar);
  }, [auth?.avatar]);

  return canvasMediaStream;
};
