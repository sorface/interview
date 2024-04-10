import { useCallback, useEffect, useState } from 'react';

interface Device {
  deviceId?: MediaDeviceInfo['deviceId'];
}

export interface Devices {
  mic?: Device;
  camera?: Device;
}

interface UseUserStreamParams {
  selectedDevices: Devices | null;
}

const videoConstraints = {
  height: 300,
  width: 300,
  frameRate: 15,
};

const audioConstraints = {
  channelCount: 1,
  sampleRate: 16000,
  sampleSize: 16,
  volume: 1,
  noiseSuppression: true,
};

export const useUserStreams = ({
  selectedDevices,
}: UseUserStreamParams) => {
  const [userAudioStream, setUserAudioStream] = useState<MediaStream | null>(null);
  const [userAudioProcessedStream, setUserAudioProcessedStream] = useState<MediaStream | null>(null);
  const [userVideoStream, setUserVideoStream] = useState<MediaStream | null>(null);

  useEffect(() => {
    if (!selectedDevices) {
      return;
    }
    const updateUserAudioStream = async () => {
      const micRequest = selectedDevices.mic?.deviceId && {
        ...audioConstraints,
        deviceId: selectedDevices.mic?.deviceId,
      };
      if (!micRequest) {
        return;
      }
      const newUserStream = await navigator.mediaDevices.getUserMedia({
        audio: micRequest || undefined,
      });
      setUserAudioStream(newUserStream);
    };
    updateUserAudioStream();
  }, [selectedDevices]);

  useEffect(() => {
    if (!userAudioStream) {
      return;
    }
    const audioCtx = new AudioContext();
    const source = audioCtx.createMediaStreamSource(userAudioStream);

    const compressor = audioCtx.createDynamicsCompressor();
    compressor.threshold.setValueAtTime(-50, audioCtx.currentTime);
    compressor.knee.setValueAtTime(40, audioCtx.currentTime);
    compressor.ratio.setValueAtTime(18, audioCtx.currentTime);
    compressor.attack.setValueAtTime(0, audioCtx.currentTime);
    compressor.release.setValueAtTime(0.25, audioCtx.currentTime);

    const filter = audioCtx.createBiquadFilter();
    filter.type = 'highshelf';
    filter.frequency.setValueAtTime(1600, audioCtx.currentTime);
    filter.gain.setValueAtTime(6, audioCtx.currentTime);

    const dest = audioCtx.createMediaStreamDestination();
    source.connect(compressor);
    compressor.connect(filter);
    filter.connect(dest);
    setUserAudioProcessedStream(dest.stream);

    return () => {
      source.disconnect(compressor);
    };
  }, [userAudioStream]);

  useEffect(() => {
    if (!selectedDevices) {
      return;
    }
    const updateUserVideoStream = async () => {
      const videoRequest = selectedDevices.camera?.deviceId && {
        ...videoConstraints,
        deviceId: selectedDevices.camera?.deviceId,
      };
      if (!videoRequest) {
        return;
      }
      const newUserStream = await navigator.mediaDevices.getUserMedia({
        video: videoRequest || undefined,
      });
      setUserVideoStream(newUserStream);
    };
    updateUserVideoStream();
  }, [selectedDevices]);

  const disableVideo = useCallback(() => {
    if (!userVideoStream) {
      console.warn('disableVideo userVideoStream not found');
      return;
    }

    userVideoStream.getTracks().forEach(track => track.stop());
  }, [userVideoStream]);

  const enableVideo = useCallback(async () => {
    if (!selectedDevices) {
      console.warn('enableVideo selectedDevices not found');
      return;
    }
    const videoRequest = selectedDevices.camera?.deviceId && {
      ...videoConstraints,
      deviceId: selectedDevices.camera?.deviceId,
    };
    const newUserStream = await navigator.mediaDevices.getUserMedia({
      video: videoRequest || undefined,
    });
    setUserVideoStream(newUserStream);
  }, [selectedDevices]);

  return {
    userAudioStream: userAudioProcessedStream,
    userVideoStream,
    disableVideo,
    enableVideo,
  }
};
