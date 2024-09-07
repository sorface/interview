import { FunctionComponent, useState } from 'react';
import { Modal, ModalProps } from '../Modal/Modal';
import { ModalWarningContent } from '../ModalWarningContent/ModalWarningContent';
import { IconNames } from '../../constants';
import { ModalFooter } from '../ModalFooter/ModalFooter';
import { Button } from '../Button/Button';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../localization';

interface ModalWithProgressWarningProps extends ModalProps {
  warningCaption: string;
}

export const ModalWithProgressWarning: FunctionComponent<ModalWithProgressWarningProps> = ({
  warningCaption,
  ...modalProps
}) => {
  const localizationCaptions = useLocalizationCaptions();
  const [warningOpen, setWarningOpen] = useState(false);

  const handleRequestClose = () => {
    setWarningOpen(true);
  };

  const handleRequestWarningClose = () => {
    setWarningOpen(false);
  };

  const handleRequestCloselWithoutSave = () => {
    setWarningOpen(false);
    modalProps.onClose();
  };

  return (
    <>
      <Modal
        open={warningOpen}
        contentLabel=''
        onClose={handleRequestCloselWithoutSave}
      >
        <ModalWarningContent
          captionLine1={warningCaption}
          captionLine2={`${localizationCaptions[LocalizationKey.CloselWithoutSave]}?`}
          iconName={IconNames.None}
          dangerIcon
        />
        <ModalFooter>
          <Button
            onClick={handleRequestWarningClose}
          >
            {localizationCaptions[LocalizationKey.Stay]}
          </Button>
          <Button
            variant='active'
            onClick={handleRequestCloselWithoutSave}
          >
            {localizationCaptions[LocalizationKey.CloselWithoutSave]}
          </Button>
        </ModalFooter>
      </Modal>
      <Modal
        {...modalProps}
        open={warningOpen ? false : modalProps.open}
        onRequestClose={handleRequestClose}
      />
    </>
  );
};
