import React, { FunctionComponent, useContext } from 'react';
import { Modal } from '../../../../components/Modal/Modal';
import { useLocalizationCaptions } from '../../../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../../../localization';
import { RecognitionLangSwitch } from '../../../../components/RecognitionLangSwitch/RecognitionLangSwitch';
import { Gap } from '../../../../components/Gap/Gap';
import { LangSwitch } from '../../../../components/LangSwitch/LangSwitch';
import { ThemeSwitch } from '../../../../components/ThemeSwitch/ThemeSwitch';
import { Checkbox } from '../../../../components/Checkbox/Checkbox';
import { Typography } from '../../../../components/Typography/Typography';
import { UserStreamsContext } from '../../context/UserStreamsContext';
import { DeviceSelect } from '../VideoChat/DeviceSelect';
import { IconNames } from '../../../../constants';

interface RoomSettingsProps {
  open: boolean;
  onRequestClose: () => void;
}

export const RoomSettings: FunctionComponent<RoomSettingsProps> = ({
  open,
  onRequestClose,
}) => {
  const localizationCaptions = useLocalizationCaptions();
  const {
    backgroundRemoveEnabled,
    devices,
    setBackgroundRemoveEnabled,
    setSelectedCameraId,
    setSelectedMicId,
  } = useContext(UserStreamsContext);

  const handleBackgroundRemoveSwitch = () => {
    setBackgroundRemoveEnabled(!backgroundRemoveEnabled);
  };

  return (
    <Modal
      open={open}
      contentLabel={localizationCaptions[LocalizationKey.Settings]}
      onClose={onRequestClose}
    >
      <div className="flex-1 flex flex-col items-center">
        <div className="w-full max-w-29.25 grid grid-cols-settings-list gap-y-1">
          <ThemeSwitch />
          <LangSwitch />
          <RecognitionLangSwitch />
          <div className="text-left flex items-center">
            <Typography size="m">
              {localizationCaptions[LocalizationKey.WebcamBackgroundBlur]}:
            </Typography>
          </div>
          <div className="text-left">
            <Checkbox
              id="webcam-background-remove"
              label=""
              checked={backgroundRemoveEnabled}
              onChange={handleBackgroundRemoveSwitch}
            />
          </div>
        </div>
        <Gap sizeRem={1} />
        <div className="w-full max-w-29.25">
          <div className="flex">
            <DeviceSelect
              devices={devices.mic}
              localStorageKey="defalutMic"
              onSelect={setSelectedMicId}
              icon={IconNames.MicOn}
            />
          </div>
          <Gap sizeRem={1} />
          <div className="flex">
            <DeviceSelect
              devices={devices.camera}
              localStorageKey="defalutCamera"
              onSelect={setSelectedCameraId}
              icon={IconNames.VideocamOn}
            />
          </div>
        </div>
      </div>
      <Gap sizeRem={2} />
    </Modal>
  );
};
