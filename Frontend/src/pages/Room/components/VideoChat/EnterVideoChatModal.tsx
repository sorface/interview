import { FunctionComponent, useCallback, useContext, useEffect, useRef, useState } from 'react';
import Modal from 'react-modal';
import { Link } from 'react-router-dom';
import { IconNames, pathnames } from '../../../../constants';
import { DeviceSelect } from './DeviceSelect';
import { createAudioAnalyser, frequencyBinCount } from './utils/createAudioAnalyser';
import { getAverageVolume } from './utils/getAverageVolume';
import { SwitchButton } from './SwitchButton';
import { AuthContext } from '../../../../context/AuthContext';
import { UserAvatar } from '../../../../components/UserAvatar/UserAvatar';
import { Devices } from '../../hooks/useUserStreams';
import { Loader } from '../../../../components/Loader/Loader';
import { LocalizationKey } from '../../../../localization';
import { useLocalizationCaptions } from '../../../../hooks/useLocalizationCaptions';

import './EnterVideoChatModal.css';

interface EnterVideoChatModalProps {
  open: boolean;
  viewerMode: boolean;
  loading: boolean;
  roomName?: string;
  devices: Devices;
  setSelectedCameraId: React.Dispatch<React.SetStateAction<string | undefined>>,
  setSelectedMicId: React.Dispatch<React.SetStateAction<string | undefined>>,
  updateDevices: () => Promise<void>;
  error: string | null;
  userVideoStream: MediaStream | null;
  userAudioStream: MediaStream | null;
  micEnabled: boolean;
  cameraEnabled: boolean;
  onClose: () => void;
  onMicSwitch: () => void;
  onCameraSwitch: () => void;
}

const updateAnalyserDelay = 1000 / 30;

const enum Screen {
  Joining,
  SetupDevices,
  Error,
}

export const EnterVideoChatModal: FunctionComponent<EnterVideoChatModalProps> = ({
  open,
  loading,
  viewerMode,
  roomName,
  devices,
  updateDevices,
  setSelectedCameraId,
  setSelectedMicId,
  error,
  userVideoStream,
  userAudioStream,
  micEnabled,
  cameraEnabled,
  onClose,
  onMicSwitch,
  onCameraSwitch,
}) => {
  const auth = useContext(AuthContext);
  const localizationCaptions = useLocalizationCaptions();
  const [screen, setScreen] = useState<Screen>(Screen.Joining);
  const [micVolume, setMicVolume] = useState(0);
  const [settingsEnabled, setSettingsEnabled] = useState(false);
  const userVideo = useRef<HTMLVideoElement>(null);
  const requestRef = useRef<number>();
  const updateAnalyserTimeout = useRef(0);
  const audioAnalyser = useRef<AnalyserNode | null>(null);

  useEffect(() => {
    if (!error) {
      return;
    }
    setScreen(Screen.Error);
  }, [error]);

  useEffect(() => {
    if (!userAudioStream) {
      return;
    }
    try {
      audioAnalyser.current = createAudioAnalyser(userAudioStream);
    } catch {
      console.warn('Failed to create audioAnalyser');
    }
  }, [userAudioStream]);

  useEffect(() => {
    if (!userVideoStream) {
      return;
    }
    if (userVideo.current) {
      userVideo.current.srcObject = userVideoStream;
    }
  }, [userVideoStream]);

  useEffect(() => {
    const frequencyData = new Uint8Array(frequencyBinCount);
    let prevTime = performance.now();
    const updateAudioAnalyser = () => {
      const time = performance.now();
      const delta = time - prevTime;
      if (updateAnalyserTimeout.current > 0) {
        updateAnalyserTimeout.current -= delta;
        prevTime = time;
        requestRef.current = requestAnimationFrame(updateAudioAnalyser);
        return;
      }
      const analyser = audioAnalyser.current;
      if (!analyser) {
        requestRef.current = requestAnimationFrame(updateAudioAnalyser);
        return;
      }
      analyser.getByteFrequencyData(frequencyData);
      const averageVolume = getAverageVolume(frequencyData);
      setMicVolume(averageVolume);

      prevTime = time;
      updateAnalyserTimeout.current = updateAnalyserDelay;
      requestRef.current = requestAnimationFrame(updateAudioAnalyser);
    };
    requestRef.current = requestAnimationFrame(updateAudioAnalyser);

    return () => {
      if (requestRef.current) {
        cancelAnimationFrame(requestRef.current);
      }
    };
  }, []);

  const handleSetupDevices = async () => {
    try {
      updateDevices();
      setScreen(Screen.SetupDevices);
    } catch {
      alert(localizationCaptions[LocalizationKey.UserStreamError]);
    }
  };

  const handleSelectMic = useCallback((deviceId: MediaDeviceInfo['deviceId']) => {
    setSelectedMicId(deviceId);
  }, [setSelectedMicId]);

  const handleSelectCamera = useCallback((deviceId: MediaDeviceInfo['deviceId']) => {
    setSelectedCameraId(deviceId);
  }, [setSelectedCameraId]);

  const handleSwitchSettings = () => {
    setSettingsEnabled(!settingsEnabled);
  };

  const screens: { [key in Screen]: JSX.Element } = {
    [Screen.Joining]: (
      <div className="enter-videochat-info">
        {!!(auth?.nickname && auth?.avatar) && (
          <UserAvatar
            nickname={auth.nickname}
            src={auth.avatar}
          />
        )}
        <p>{localizationCaptions[LocalizationKey.JoinAs]}: {auth?.nickname}</p>
        <div className="enter-videochat-warning">
          <div>{localizationCaptions[LocalizationKey.Warning]}</div>
          <div>{localizationCaptions[LocalizationKey.CallRecording]}</div>
        </div>
        {loading ? (
          <Loader />
        ) : (
          viewerMode ? (
            <button className="active" onClick={onClose}>{localizationCaptions[LocalizationKey.Join]}</button>
          ) : (
            <button onClick={handleSetupDevices}>{localizationCaptions[LocalizationKey.SetupDevices]}</button>
          )
        )}
      </div>
    ),
    [Screen.SetupDevices]: (
      <div>
        <div className="enter-videochat-row">
          <div className="enter-videochat-column">
            <div className={settingsEnabled ? 'enter-videochat-content-container-mini' : 'enter-videochat-content-container'} >
              <video
                ref={userVideo}
                muted
                autoPlay
                playsInline
              >
                Video not supported
              </video>
              <div className="enter-videochat-row switch-row">
                <SwitchButton
                  enabled={micEnabled}
                  iconEnabledName={IconNames.MicOn}
                  iconDisabledName={IconNames.MicOff}
                  onClick={onMicSwitch}
                />
                <SwitchButton
                  enabled={cameraEnabled}
                  iconEnabledName={IconNames.VideocamOn}
                  iconDisabledName={IconNames.VideocamOff}
                  onClick={onCameraSwitch}
                />
                <SwitchButton
                  enabled={!settingsEnabled}
                  iconEnabledName={IconNames.Settings}
                  iconDisabledName={IconNames.Settings}
                  onClick={handleSwitchSettings}
                />
              </div>
              <progress max="50" value={micVolume}>{localizationCaptions[LocalizationKey.Microphone]}: {micVolume}</progress>
            </div >

            <div className="enter-videochat-content-container" style={{ display: settingsEnabled ? 'block' : 'none' }}>
              <div >
                <div>{localizationCaptions[LocalizationKey.Microphone]}</div>
                <DeviceSelect
                  devices={devices.mic}
                  onSelect={handleSelectMic}
                />
                <div>{localizationCaptions[LocalizationKey.Camera]}</div>
                <DeviceSelect
                  devices={devices.camera}
                  onSelect={handleSelectCamera}
                />
              </div>
            </div>

          </div >
        </div >
        <button className="active" onClick={onClose}>{localizationCaptions[LocalizationKey.Join]}</button>
      </div >
    ),
    [Screen.Error]: (
      <div>{error}</div>
    ),
  };

  return (
    <Modal
      isOpen={open}
      contentLabel={localizationCaptions[LocalizationKey.CloseRoom]}
      appElement={document.getElementById('root') || undefined}
      className="action-modal enter-videochat-modal"
      style={{
        overlay: {
          backgroundColor: 'var(--form-bg)',
          zIndex: 2,
        },
      }}
    >
      <div className="action-modal-header">
        <h3>{localizationCaptions[LocalizationKey.JoiningRoom]} {roomName}</h3>
      </div>
      {screens[screen]}
      <Link to={pathnames.rooms} className="enter-videochat-exit">
        {localizationCaptions[LocalizationKey.Exit]}
      </Link>
    </Modal >
  );
};
