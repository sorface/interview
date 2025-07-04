import React, {
  FunctionComponent,
  useCallback,
  useContext,
  useEffect,
  useRef,
  useState,
} from 'react';
import Modal from 'react-modal';
import { IconNames, pathnames } from '../../../../constants';
import { DeviceSelect } from './DeviceSelect';
import {
  createAudioAnalyser,
  frequencyBinCount,
} from './utils/createAudioAnalyser';
import { getAverageVolume } from './utils/getAverageVolume';
import { AuthContext } from '../../../../context/AuthContext';
import { Loader } from '../../../../components/Loader/Loader';
import { LocalizationKey } from '../../../../localization';
import { useLocalizationCaptions } from '../../../../hooks/useLocalizationCaptions';
import { Button } from '../../../../components/Button/Button';
import { Typography } from '../../../../components/Typography/Typography';
import { Gap } from '../../../../components/Gap/Gap';
import { RoomToolsPanel } from '../RoomToolsPanel/RoomToolsPanel';
import { RecognitionLangSwitch } from '../../../../components/RecognitionLangSwitch/RecognitionLangSwitch';
import { Checkbox } from '../../../../components/Checkbox/Checkbox';
import { UserStreamsContext } from '../../context/UserStreamsContext';
import { RoomContext } from '../../context/RoomContext';
import { useThemeClassName } from '../../../../hooks/useThemeClassName';
import { Theme } from '../../../../context/ThemeContext';
import { Link } from 'react-router-dom';

interface EnterVideoChatModalProps {
  aiRoom: boolean;
  open: boolean;
  loading: boolean;
  error: string | null;
  onClose: () => void;
}

const updateAnalyserDelay = 1000 / 30;

const enum Screen {
  JoiningAi,
  Joining,
  SetupDevices,
  Error,
}

export const EnterVideoChatModal: FunctionComponent<
  EnterVideoChatModalProps
> = ({ aiRoom, open, loading, error, onClose }) => {
  const auth = useContext(AuthContext);
  const localizationCaptions = useLocalizationCaptions();
  const { viewerMode, room } = useContext(RoomContext);
  const {
    userAudioStream,
    userVideoStream,
    micEnabled,
    cameraEnabled,
    backgroundRemoveEnabled,
    devices,
    setMicEnabled,
    setCameraEnabled,
    setSelectedCameraId,
    setSelectedMicId,
    requestDevices,
    updateDevices,
    setBackgroundRemoveEnabled,
  } = useContext(UserStreamsContext);
  const [screen, setScreen] = useState<Screen>(Screen.Joining);
  const [micVolume, setMicVolume] = useState(0);
  const userVideo = useRef<HTMLVideoElement>(null);
  const requestRef = useRef<number>();
  const updateAnalyserTimeout = useRef(0);
  const audioAnalyser = useRef<AnalyserNode | null>(null);
  const joinPreviewThemedClassName = useThemeClassName({
    [Theme.Dark]: 'bg-dark-dark2',
    [Theme.Light]: 'bg-grey2',
  });

  useEffect(() => {
    if (!aiRoom) {
      return;
    }
    setScreen(Screen.JoiningAi);
  }, [aiRoom]);

  useEffect(() => {
    if (viewerMode || loading) {
      return;
    }
    if (aiRoom) {
      return;
    }
    requestDevices();
  }, [viewerMode, loading, aiRoom, requestDevices]);

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
      userVideo.current.play();
    }
  }, [userVideoStream, screen]);

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

  const handleSelectMic = useCallback(
    (deviceId: MediaDeviceInfo['deviceId']) => {
      setSelectedMicId(deviceId);
    },
    [setSelectedMicId],
  );

  const handleSelectCamera = useCallback(
    (deviceId: MediaDeviceInfo['deviceId']) => {
      setSelectedCameraId(deviceId);
    },
    [setSelectedCameraId],
  );

  const handleMicSwitch = () => {
    setMicEnabled(!micEnabled);
  };

  const handleCameraSwitch = () => {
    setCameraEnabled(!cameraEnabled);
  };

  const handleBackgroundRemoveSwitch = () => {
    setBackgroundRemoveEnabled(!backgroundRemoveEnabled);
  };

  const joiningRoomHeader = (
    <div>
      <Typography size="xl" bold>
        {localizationCaptions[LocalizationKey.JoiningRoom]}
      </Typography>
    </div>
  );

  const screens: { [key in Screen]: JSX.Element } = {
    [Screen.JoiningAi]: (
      <>
        <div className="w-[20rem] flex flex-col items-center text-center">
          {joiningRoomHeader}
          <Gap sizeRem={2} />
          <div className="text-left w-full">
            <Typography size="m" semibold>
              {localizationCaptions[LocalizationKey.AllowAccessToMicrophone]}
            </Typography>
          </div>
          <Gap sizeRem={0.25} />
          <div className="w-full max-w-[29.25rem] grid grid-cols-settings-list gap-y-[1rem]">
            <RecognitionLangSwitch />
          </div>
          <Gap sizeRem={0.25} />
          <div className="text-left w-full">
            <Typography size="s" secondary>
              {
                localizationCaptions[
                  LocalizationKey.PleaseSelectRecognitionLanguage
                ]
              }
            </Typography>
          </div>
          <Gap sizeRem={2} />
          <Button variant="active" className="w-full" onClick={onClose}>
            {localizationCaptions[LocalizationKey.Join]}
          </Button>
          <Gap sizeRem={1} />
          <Typography size="s" secondary>
            {localizationCaptions[LocalizationKey.CallRecording]}
          </Typography>
        </div>
      </>
    ),
    [Screen.Joining]: (
      <>
        <div className="pr-[4rem]">
          <div className="relative w-[37rem] h-[28rem]">
            <div
              className={`flex items-center justify-center w-[37rem] h-[28rem] rounded-[1.25rem] ${joinPreviewThemedClassName}`}
            >
              <Typography size="xxl">{auth?.nickname}</Typography>
            </div>
            <RoomToolsPanel.Wrapper>
              <RoomToolsPanel.ButtonsGroupWrapper noPaddingBottom>
                <RoomToolsPanel.SwitchButton
                  enabled={false}
                  htmlDisabled
                  danger
                  iconEnabledName={IconNames.MicOn}
                  iconDisabledName={IconNames.MicOff}
                  onClick={() => {}}
                  progress={micVolume / 50}
                />
                <Gap sizeRem={0.125} />
                <RoomToolsPanel.SwitchButton
                  enabled={false}
                  htmlDisabled
                  danger
                  iconEnabledName={IconNames.VideocamOn}
                  iconDisabledName={IconNames.VideocamOff}
                  onClick={() => {}}
                />
              </RoomToolsPanel.ButtonsGroupWrapper>
            </RoomToolsPanel.Wrapper>
          </div>
          <Gap sizeRem={0.75} />
          <div className="invisible">
            <Typography size="m">
              <Checkbox
                id="webcam-background-remove"
                label={
                  localizationCaptions[LocalizationKey.WebcamBackgroundBlur]
                }
                disabled
                checked={false}
                onChange={() => {}}
              />
            </Typography>
          </div>
        </div>
        <div className="w-[20rem] flex flex-col items-center text-center">
          {joiningRoomHeader}
          <Gap sizeRem={2} />
          {loading ? (
            <Loader />
          ) : viewerMode ? (
            <Button variant="active" onClick={onClose}>
              {localizationCaptions[LocalizationKey.Join]}
            </Button>
          ) : (
            <Button
              variant="active"
              className="w-full"
              onClick={handleSetupDevices}
            >
              {localizationCaptions[LocalizationKey.SetupDevices]}
            </Button>
          )}
        </div>
      </>
    ),
    [Screen.SetupDevices]: (
      <>
        <div className="pr-[4rem]">
          <div className="relative w-[37rem] h-[28rem]">
            <video
              ref={userVideo}
              muted
              autoPlay
              playsInline
              className="w-[37rem] h-[28rem] rounded-[1.25rem] object-cover"
            >
              Video not supported
            </video>
            <RoomToolsPanel.Wrapper>
              <RoomToolsPanel.ButtonsGroupWrapper noPaddingBottom>
                <RoomToolsPanel.SwitchButton
                  enabled={micEnabled}
                  iconEnabledName={IconNames.MicOn}
                  iconDisabledName={IconNames.MicOff}
                  onClick={handleMicSwitch}
                  progress={micVolume / 50}
                />
                <Gap sizeRem={0.125} />
                <RoomToolsPanel.SwitchButton
                  enabled={cameraEnabled}
                  iconEnabledName={IconNames.VideocamOn}
                  iconDisabledName={IconNames.VideocamOff}
                  onClick={handleCameraSwitch}
                />
              </RoomToolsPanel.ButtonsGroupWrapper>
            </RoomToolsPanel.Wrapper>
          </div>
          <Gap sizeRem={0.75} />
          <Typography size="m">
            <Checkbox
              id="webcam-background-remove"
              label={localizationCaptions[LocalizationKey.WebcamBackgroundBlur]}
              checked={backgroundRemoveEnabled}
              onChange={handleBackgroundRemoveSwitch}
            />
          </Typography>
        </div>
        <div className="w-[20rem] flex flex-col items-center text-center">
          {joiningRoomHeader}
          <Gap sizeRem={2} />
          <div>
            <div className="flex items-center">
              <DeviceSelect
                devices={devices.mic}
                localStorageKey="defalutMic"
                onSelect={handleSelectMic}
                icon={IconNames.MicOn}
              />
            </div>
            <Gap sizeRem={0.5} />
            <div className="flex items-center">
              <DeviceSelect
                devices={devices.camera}
                localStorageKey="defalutCamera"
                onSelect={handleSelectCamera}
                icon={IconNames.VideocamOn}
              />
            </div>
            <Gap sizeRem={1.25} />
            <div className="w-full max-w-[29.25rem] grid grid-cols-settings-list gap-y-[1rem]">
              <RecognitionLangSwitch />
            </div>
            <Gap sizeRem={0.25} />
            <div className="text-left">
              <Typography size="s" secondary>
                {
                  localizationCaptions[
                    LocalizationKey.PleaseSelectRecognitionLanguage
                  ]
                }
              </Typography>
            </div>
            <Gap sizeRem={2} />
            <Button variant="active" className="w-full" onClick={onClose}>
              {localizationCaptions[LocalizationKey.Join]}
            </Button>
            <Gap sizeRem={1} />
            <Typography size="s" secondary>
              {localizationCaptions[LocalizationKey.CallRecording]}
            </Typography>
          </div>
        </div>
      </>
    ),
    [Screen.Error]: <div>{error}</div>,
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
      <Link
        to={aiRoom ? pathnames.roadmaps : pathnames.highlightRooms}
        className="no-underline"
      >
        <div className="action-modal-header absolute flex items-center h-[2.375rem] mt-[1.3125rem] ml-[0.5rem]">
          <div className="w-[2.375rem] h-[2.375rem] mr-[1rem]">
            <img
              className="w-[2.375rem] h-[2.375rem] rounded-[0.375rem]"
              src="/logo192.png"
              alt="site logo"
            />
          </div>
          <h3>
            <Typography size="xl" semibold>
              {room?.name}
            </Typography>
          </h3>
        </div>
      </Link>
      <div className="flex items-center justify-center mt-auto mb-auto">
        {loading ? <Loader /> : screens[screen]}
      </div>
    </Modal>
  );
};
