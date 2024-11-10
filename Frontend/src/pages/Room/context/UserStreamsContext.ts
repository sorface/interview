import { createContext } from 'react';
import { Devices } from '../hooks/useUserStreams';

export interface UserStreamsContextType {
  userAudioStream: MediaStream | null;
  userVideoStream: MediaStream | null;
  devices: Devices;
  selectedCameraId?: string | null;
  selectedMicId?: string | null;
  cameraEnabled: boolean;
  micEnabled: boolean;
  backgroundRemoveEnabled: boolean;
  setCameraEnabled: (enabled: boolean) => void;
  setSelectedCameraId: (id: string) => void;
  setSelectedMicId: (id: string) => void;
  setMicEnabled: (enabled: boolean) => void;
  requestDevices: () => void;
  setBackgroundRemoveEnabled: (enabled: boolean) => void;
  updateDevices: () => void;
}

const noop = () => { };

const defaultValue: UserStreamsContextType = {
  userAudioStream: null,
  userVideoStream: null,
  devices: { camera: [], mic: [] },
  selectedCameraId: null,
  selectedMicId: null,
  cameraEnabled: false,
  micEnabled: false,
  backgroundRemoveEnabled: false,
  setCameraEnabled: noop,
  setSelectedCameraId: noop,
  setSelectedMicId: noop,
  setMicEnabled: noop,
  requestDevices: noop,
  setBackgroundRemoveEnabled: noop,
  updateDevices: noop,
};

export const UserStreamsContext = createContext<UserStreamsContextType>(defaultValue);
