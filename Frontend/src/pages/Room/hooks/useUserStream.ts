import { useEffect, useState } from 'react';

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
  volume: 1
};

export const useUserStream = ({
  selectedDevices,
}: UseUserStreamParams) => {
  const [userStream, setUserStream] = useState<MediaStream | null>(null);

  useEffect(() => {
    if (!selectedDevices) {
      return;
    }
    const updateUserStream = async () => {
      const videoRequest = selectedDevices.camera?.deviceId && {
        ...videoConstraints,
        deviceId: selectedDevices.camera?.deviceId,
      };
      const micRequest = selectedDevices.mic?.deviceId && {
        ...audioConstraints,
        deviceId: selectedDevices.mic?.deviceId,
      };
      const newUserStream = await navigator.mediaDevices.getUserMedia({
        video: videoRequest || undefined,
        audio: micRequest || undefined,
      });
      setUserStream(newUserStream);
    };
    updateUserStream();
  }, [selectedDevices]);

  return {
    userStream,
  }
};
