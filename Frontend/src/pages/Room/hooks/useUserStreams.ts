import { useCallback, useEffect, useRef, useState } from 'react';
import { useCanvasStream } from './useCanvasStream';

export interface Devices {
  mic: MediaDeviceInfo[];
  camera: MediaDeviceInfo[];
}

const videoConstraints = {
  width: 370,
  height: 300,
  frameRate: 15,
};

const audioConstraints = {
  channelCount: 1,
  sampleRate: 16000,
  sampleSize: 16,
  volume: 1,
  noiseSuppression: true,
  echoCancellation: true,
  autoGainControl: true,
};

const getDevices = async () =>
  await navigator.mediaDevices.enumerateDevices();

const micDeviceKind = 'audioinput';
const cameraDeviceKind = 'videoinput';

const enableDisableUserTrack = (stream: MediaStream, enabled: boolean) =>
  stream.getTracks().forEach(track => track.enabled = enabled);

export const useUserStreams = () => {
  const [devices, setDevices] = useState<Devices>({ camera: [], mic: [] });
  const [selectedCameraId, setSelectedCameraId] = useState<MediaDeviceInfo['deviceId']>();
  const [selectedMicId, setSelectedMicId] = useState<MediaDeviceInfo['deviceId']>();
  const [micEnabled, setMicEnabled] = useState(true);
  const [cameraEnabled, setCameraEnabled] = useState(true);
  const [backgroundRemoveEnabled, setBackgroundRemoveEnabled] = useState(true);
  const [userAudioStream, setUserAudioStream] = useState<MediaStream | null>(null);
  const [userAudioProcessedStream, setUserAudioProcessedStream] = useState<MediaStream | null>(null);
  const [userVideoStream, setUserVideoStream] = useState<MediaStream | null>(null);
  const userVideoStreamRef = useRef<MediaStream | null>(null);
  const canvasMediaStream = useCanvasStream({
    width: videoConstraints.width,
    height: videoConstraints.height,
    frameRate: videoConstraints.frameRate,
    cameraStream: cameraEnabled ? userVideoStream : null,
    backgroundRemoveEnabled,
  });

  const requestDevices = useCallback(async () => {
    navigator.mediaDevices.getUserMedia({ video: true, audio: true });
  }, []);

  const updateDevices = useCallback(async () => {
    const allDevices = await getDevices();
    const micDevices = allDevices.filter(device => device.kind === micDeviceKind);
    const cameraDevices = allDevices.filter(device => device.kind === cameraDeviceKind);
    setDevices({
      camera: cameraDevices,
      mic: micDevices,
    });
  }, []);

  useEffect(() => {
    if (!selectedCameraId) {
      return;
    }
    const updateUserVideoStream = async () => {
      const videoRequest = {
        ...videoConstraints,
        deviceId: selectedCameraId,
      };
      if (!videoRequest) {
        return;
      }
      const newUserStream = await navigator.mediaDevices.getUserMedia({
        video: videoRequest,
      });
      setUserVideoStream(newUserStream);
      userVideoStreamRef.current = newUserStream;
    };
    updateUserVideoStream();

    return () => {
      userVideoStreamRef.current?.getTracks().forEach(track => track.stop());
    };
  }, [selectedCameraId]);

  useEffect(() => {
    if (!selectedMicId) {
      return;
    }
    let newUserStream: MediaStream | null = null;
    const updateUserAudioStream = async () => {
      const micRequest = {
        ...audioConstraints,
        deviceId: selectedMicId,
      };
      if (!micRequest) {
        return;
      }
      newUserStream = await navigator.mediaDevices.getUserMedia({
        audio: micRequest,
      });
      setUserAudioStream(newUserStream);
    };
    updateUserAudioStream();

    return () => {
      newUserStream?.getTracks().forEach(track => track.stop());
    };
  }, [selectedMicId]);

  useEffect(() => {
    if (!userAudioStream) {
      return;
    }
    const audioCtx = new AudioContext();
    const source = audioCtx.createMediaStreamSource(userAudioStream);

    const compressor = audioCtx.createDynamicsCompressor();
    compressor.threshold.setValueAtTime(-15, audioCtx.currentTime);
    compressor.knee.setValueAtTime(25, audioCtx.currentTime);
    compressor.ratio.setValueAtTime(18, audioCtx.currentTime);
    compressor.attack.setValueAtTime(0, audioCtx.currentTime);
    compressor.release.setValueAtTime(0.25, audioCtx.currentTime);

    const filter = audioCtx.createBiquadFilter();
    filter.type = 'peaking';
    filter.frequency.setValueAtTime(2300, audioCtx.currentTime);
    filter.Q.setValueAtTime(1, audioCtx.currentTime);

    const filterHigh = audioCtx.createBiquadFilter();
    filterHigh.type = 'lowpass';
    filterHigh.frequency.setValueAtTime(7000, audioCtx.currentTime);
    filterHigh.gain.setValueAtTime(6, audioCtx.currentTime);

    const dest = audioCtx.createMediaStreamDestination();
    source.connect(compressor);
    compressor.connect(filter);
    filter.connect(filterHigh);
    filterHigh.connect(dest);
    setUserAudioProcessedStream(dest.stream);

    return () => {
      source.disconnect(compressor);
    };
  }, [userAudioStream]);

  const disableVideo = useCallback(() => {
    const userVideoStreamRefCurrent = userVideoStreamRef.current;
    if (!userVideoStreamRefCurrent) {
      console.warn('disableVideo userVideoStream not found');
      return;
    }

    userVideoStreamRefCurrent.getTracks().forEach(track => track.stop());
  }, []);

  const enableVideo = useCallback(async () => {
    if (!selectedCameraId) {
      console.warn('enableVideo selectedDevices not found');
      return;
    }
    const videoRequest = {
      ...videoConstraints,
      deviceId: selectedCameraId,
    };
    const newUserStream = await navigator.mediaDevices.getUserMedia({
      video: videoRequest,
    });
    setUserVideoStream(newUserStream);
    userVideoStreamRef.current = newUserStream;
  }, [selectedCameraId]);

  useEffect(() => {
    if (cameraEnabled) {
      enableVideo();
      return;
    }
    disableVideo();
    return;
  }, [cameraEnabled, enableVideo, disableVideo]);

  useEffect(() => {
    if (!userAudioProcessedStream) {
      return;
    }
    enableDisableUserTrack(userAudioProcessedStream, micEnabled);
  }, [userAudioProcessedStream, micEnabled]);

  return {
    userAudioStream: userAudioProcessedStream,
    userVideoStream: canvasMediaStream,
    devices,
    selectedCameraId,
    selectedMicId,
    cameraEnabled,
    micEnabled,
    backgroundRemoveEnabled,
    setCameraEnabled,
    setSelectedCameraId,
    setSelectedMicId,
    setMicEnabled,
    requestDevices,
    setBackgroundRemoveEnabled,
    updateDevices,
  };
};
