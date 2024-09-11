import { FunctionComponent, useCallback, useContext, useEffect, useRef, useState } from 'react';
import Modal from 'react-modal';
import { IconNames } from '../../../../constants';
import { DeviceSelect } from './DeviceSelect';
import { createAudioAnalyser, frequencyBinCount } from './utils/createAudioAnalyser';
import { getAverageVolume } from './utils/getAverageVolume';
import { AuthContext } from '../../../../context/AuthContext';
import { UserAvatar } from '../../../../components/UserAvatar/UserAvatar';
import { Devices } from '../../hooks/useUserStreams';
import { Loader } from '../../../../components/Loader/Loader';
import { LocalizationKey } from '../../../../localization';
import { useLocalizationCaptions } from '../../../../hooks/useLocalizationCaptions';
import { Button } from '../../../../components/Button/Button';
import { Typography } from '../../../../components/Typography/Typography';
import { Gap } from '../../../../components/Gap/Gap';
import { RoomToolsPanel } from '../RoomToolsPanel/RoomToolsPanel';

interface EnterVideoChatModalProps {
  open: boolean;
  viewerMode: boolean;
  loading: boolean;
  roomName?: string;
  devices: Devices;
  error: string | null;
  userVideoStream: MediaStream | null;
  userAudioStream: MediaStream | null;
  micEnabled: boolean;
  cameraEnabled: boolean;
  setSelectedCameraId: React.Dispatch<React.SetStateAction<string | undefined>>;
  setSelectedMicId: React.Dispatch<React.SetStateAction<string | undefined>>;
  onRequestDevices: () => void;
  updateDevices: () => Promise<void>;
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
  error,
  userVideoStream,
  userAudioStream,
  micEnabled,
  cameraEnabled,
  setSelectedCameraId,
  setSelectedMicId,
  onRequestDevices,
  updateDevices,
  onClose,
  onMicSwitch,
  onCameraSwitch,
}) => {
  const auth = useContext(AuthContext);
  const localizationCaptions = useLocalizationCaptions();
  const [screen, setScreen] = useState<Screen>(Screen.Joining);
  const [micVolume, setMicVolume] = useState(0);
  const userVideo = useRef<HTMLVideoElement>(null);
  const requestRef = useRef<number>();
  const updateAnalyserTimeout = useRef(0);
  const audioAnalyser = useRef<AnalyserNode | null>(null);

  useEffect(() => {
    if (viewerMode) {
      return;
    }
    onRequestDevices();
  }, [viewerMode, onRequestDevices]);

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

  const joiningRoomHeader = (
    <div>
      <Typography size='xl' bold>
        {localizationCaptions[LocalizationKey.JoiningRoom]}
      </Typography>
      <Gap sizeRem={2} />
    </div>
  );

  const screens: { [key in Screen]: JSX.Element } = {
    [Screen.Joining]: (
      <>
        <div className='pr-4 w-25 h-25 flex items-center justify-center'>
          {!!(auth?.nickname && auth?.avatar) && (
            <UserAvatar
              nickname={auth.nickname}
              src={auth.avatar}
              size='l'
            />
          )}
        </div>
        <div className='w-20 flex flex-col items-center text-center'>
          {joiningRoomHeader}
          {loading ? (
            <Loader />
          ) : (
            viewerMode ? (
              <Button variant='active' onClick={onClose}>{localizationCaptions[LocalizationKey.Join]}</Button>
            ) : (
              <Button className='w-full' onClick={handleSetupDevices}>{localizationCaptions[LocalizationKey.SetupDevices]}</Button>
            )
          )}
          <Gap sizeRem={1} />
          <Typography size='s'>
            {localizationCaptions[LocalizationKey.CallRecording]}
          </Typography>
        </div>
      </>
    ),
    [Screen.SetupDevices]: (
      <>
        <div className='pr-4'>
          <div className='relative w-25 h-25'>
            <video
              ref={userVideo}
              muted
              autoPlay
              playsInline
              className='w-25 h-25 rounded-1.25'
            >
              Video not supported
            </video>
            <RoomToolsPanel.Wrapper>
              <RoomToolsPanel.ButtonsGroupWrapper noPaddingBottom>
                <RoomToolsPanel.SwitchButton
                  enabled={micEnabled}
                  iconEnabledName={IconNames.MicOn}
                  iconDisabledName={IconNames.MicOff}
                  onClick={onMicSwitch}
                  progress={micVolume / 50}
                />
                <Gap sizeRem={0.125} />
                <RoomToolsPanel.SwitchButton
                  enabled={cameraEnabled}
                  iconEnabledName={IconNames.VideocamOn}
                  iconDisabledName={IconNames.VideocamOff}
                  onClick={onCameraSwitch}
                />
              </RoomToolsPanel.ButtonsGroupWrapper>
            </RoomToolsPanel.Wrapper>
          </div>
        </div>
        <div className='w-20 flex flex-col items-center text-center'>
          {joiningRoomHeader}
          <div >
            <div className='flex items-center'>
              <DeviceSelect
                devices={devices.mic}
                onSelect={handleSelectMic}
                icon={IconNames.MicOn}
              />
            </div>
            <Gap sizeRem={0.5} />
            <div className='flex items-center'>
              <DeviceSelect
                devices={devices.camera}
                onSelect={handleSelectCamera}
                icon={IconNames.VideocamOn}
              />
            </div>
            <Gap sizeRem={2} />
            <Button variant='active' className='w-full' onClick={onClose}>{localizationCaptions[LocalizationKey.Join]}</Button>
            <Gap sizeRem={1} />
            <Typography size='s'>
              {localizationCaptions[LocalizationKey.CallRecording]}
            </Typography>
          </div>
        </div>
      </>
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
      className="enter-videochat-modal flex flex-col h-full"
      style={{
        overlay: {
          backgroundColor: 'var(--form-bg)',
          zIndex: 2,
        },
      }}
    >
      <div className="action-modal-header absolute flex items-center px-0.5 py-0.5 h-4">
        <div className='w-2.375 h-2.375 pr-1'>
          <img className='w-2.375 h-2.375 rounded-0.375' src='/logo192.png' alt='site logo' />
        </div>
        <h3>{roomName}</h3>
      </div>
      <div className='flex items-center justify-center mt-auto mb-auto'>
        {screens[screen]}
      </div>
    </Modal >
  );
};
