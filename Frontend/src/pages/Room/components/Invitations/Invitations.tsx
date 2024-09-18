import { FunctionComponent } from 'react';
import { generatePath } from 'react-router-dom';
import toast from 'react-hot-toast';
import { useLocalizationCaptions } from '../../../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../../../localization';
import { RoomInvite } from '../../../../types/room';
import { IconNames, inviteParamName, pathnames } from '../../../../constants';
import { UserType } from '../../../../types/user';
import { Icon } from '../Icon/Icon';
import { Loader } from '../../../../components/Loader/Loader';
import { Button } from '../../../../components/Button/Button';
import { Gap } from '../../../../components/Gap/Gap';
import { Modal } from '../../../../components/Modal/Modal';
import { Typography } from '../../../../components/Typography/Typography';
import { useParticipantTypeLocalization } from '../../../../hooks/useParticipantTypeLocalization';

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
  const localizeParticipantType = useParticipantTypeLocalization();

  const handleCopyToClipboard = (link: string) => {
    window.navigator.clipboard.writeText(link);
    toast.success(localizationCaptions[LocalizationKey.CopiedToClipboard]);
  };

  return (
    <Modal
      open={open}
      contentLabel={localizationCaptions[LocalizationKey.Invitations]}
      onClose={onRequestClose}
    >
      <div>
        {roomInvitesLoading && (
          <div className='invitations-modal-item'>
            <Typography size='m'>
              {localizationCaptions[LocalizationKey.RoomInvitesLoading]}...
            </Typography>
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
              <Typography size='m'>{localizeParticipantType(roomInvite.participantType)} ({roomInvite.used}/{roomInvite.max}):</Typography>
              <Gap sizeRem={0.75} />
              <div className='invitations-modal-item-link-field'>
                <Button variant='active2' onClick={() => handleCopyToClipboard(inviteUrlDispaly)}>
                  <Icon name={IconNames.Link} />
                  <Gap sizeRem={0.5} horizontal />
                  {localizationCaptions[LocalizationKey.InviteViaLink]}
                </Button>
                <Button variant='text' onClick={() => onGenerateInvite(roomInvite.participantType)}><Icon name={IconNames.Refresh} /></Button>
              </div>
            </div>
          );
        })}
        <Button
          className='invitations-modal-refresh-all'
          variant='active2'
          onClick={onGenerateAllInvites}
        >
          {localizationCaptions[LocalizationKey.RefreshAll]}<Icon name={IconNames.Refresh} />
        </Button>
      </div>
    </Modal>
  );
};
