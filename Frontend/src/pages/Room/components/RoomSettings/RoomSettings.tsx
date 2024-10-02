import { FunctionComponent } from 'react';
import { Modal } from '../../../../components/Modal/Modal';
import { useLocalizationCaptions } from '../../../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../../../localization';
import { RecognitionLangSwitch } from '../../../../components/RecognitionLangSwitch/RecognitionLangSwitch';
import { Gap } from '../../../../components/Gap/Gap';
import { LangSwitch } from '../../../../components/LangSwitch/LangSwitch';
import { ThemeSwitch } from '../../../../components/ThemeSwitch/ThemeSwitch';

interface RoomSettingsProps {
  open: boolean;
  onRequestClose: () => void;
}

export const RoomSettings: FunctionComponent<RoomSettingsProps> = ({
  open,
  onRequestClose,
}) => {
  const localizationCaptions = useLocalizationCaptions();

  return (
    <Modal
      open={open}
      contentLabel={localizationCaptions[LocalizationKey.Settings]}
      onClose={onRequestClose}
    >
      <div className='flex-1 flex flex-col items-center'>
        <div className='w-full max-w-29.25 grid grid-cols-settings-list gap-y-1'>
          <ThemeSwitch />
          <LangSwitch />
          <RecognitionLangSwitch />
        </div>
      </div>
      <Gap sizeRem={2} />
    </Modal>
  );
};
