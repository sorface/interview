import { FunctionComponent } from 'react';
import { generatePath } from 'react-router-dom';
import Modal from 'react-modal';
import toast from 'react-hot-toast';
import { useLocalizationCaptions } from '../../../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../../../localization';
import { RoomInvite } from '../../../../types/room';
import { IconNames, inviteParamName, pathnames, toastSuccessOptions } from '../../../../constants';
import { UserType } from '../../../../types/user';
import { Icon } from '../Icon/Icon';
import { Loader } from '../../../../components/Loader/Loader';
import { Button } from '../../../../components/Button/Button';
import { Gap } from '../../../../components/Gap/Gap';

import './Invitations.css';

interface InvitationsProps {
  open: boolean;
  roomId: string;
  roomInvitesData: RoomInvite[] | null;
  roomInvitesLoading: boolean;
  roomInvitesError: string | null;
  onRequestClose: () => void;
  onGenerateInvite: (participantType: UserType) => void;
  onGenerateAllInvites: () => void;
}

const sortInvites = (a: RoomInvite, b: RoomInvite) => {
  if (a.participantType > b.participantType) {
    return 1;
  }
  if (a.participantType < b.participantType) {
    return -1;
  }
  return 0;
};

export const Invitations: FunctionComponent<InvitationsProps> = ({
  open,
  roomId,
  roomInvitesLoading,
  roomInvitesError,
  roomInvitesData,
  onRequestClose,
  onGenerateInvite,
  onGenerateAllInvites,
}) => {
  const localizationCaptions = useLocalizationCaptions();

  const participantTypeLocalization: { [key in UserType]: string } = {
    Viewer: localizationCaptions[LocalizationKey.Viewer],
    Examinee: localizationCaptions[LocalizationKey.Examinee],
    Expert: localizationCaptions[LocalizationKey.Expert],
  };

  const handleCopyToClipboard = (link: string) => {
    window.navigator.clipboard.writeText(link);
    toast.success(localizationCaptions[LocalizationKey.CopiedToClipboard], toastSuccessOptions);
  };

  return (
    <Modal
      isOpen={open}
      contentLabel={localizationCaptions[LocalizationKey.Invitations]}
      appElement={document.getElementById('root') || undefined}
      className="action-modal"
      onRequestClose={onRequestClose}
      style={{
        overlay: {
          backgroundColor: 'rgba(0, 0, 0, 0.75)',
          zIndex: 999,
        },
      }}
    >
      <div className="action-modal-header">
        <h3>{localizationCaptions[LocalizationKey.Invitations]}</h3>
        <Button className='min-w-fit' onClick={onRequestClose}>X</Button>
      </div>
      <div>
        {roomInvitesLoading && (
          <div className='invitations-modal-item'>
            {localizationCaptions[LocalizationKey.RoomInvitesLoading]}...
            <Loader />
          </div>
        )}
        {roomInvitesError && (
          <div className='invitations-modal-item'>{localizationCaptions[LocalizationKey.Error]}: {roomInvitesError}</div>
        )}
        {!roomInvitesLoading && roomInvitesData?.sort(sortInvites).map(roomInvite => {
          const invitePath = generatePath(pathnames.room, {
            id: roomId,
            [inviteParamName]: roomInvite.inviteId,
          });
          const inviteUrlDispaly = `${window.location.origin}${invitePath}`;
          return (
            <div key={roomInvite.inviteId} className='invitations-modal-item'>
              <div className='invitations-modal-item-participantType'>{participantTypeLocalization[roomInvite.participantType]} ({roomInvite.used}/{roomInvite.max}):</div>
              <Gap sizeRem={0.25} />
              <div className='invitations-modal-item-link-field'>
                <div className='invitations-modal-item-link-ations'>
                  <Button onClick={() => handleCopyToClipboard(inviteUrlDispaly)}>
                    <Icon name={IconNames.Link} />
                    {localizationCaptions[LocalizationKey.InviteViaLink]}
                  </Button>
                  <Button onClick={() => onGenerateInvite(roomInvite.participantType)}><Icon name={IconNames.Refresh} /></Button>
                </div>
              </div>
            </div>
          );
        })}
        <Button
          className='invitations-modal-refresh-all'
          onClick={onGenerateAllInvites}
        >
          {localizationCaptions[LocalizationKey.RefreshAll]}<Icon name={IconNames.Refresh} />
        </Button>
      </div>
    </Modal>
  );
};
