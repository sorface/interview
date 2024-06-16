import { FunctionComponent, useCallback, useState } from 'react';
import { generatePath } from 'react-router-dom';
import Modal from 'react-modal';
import { useLocalizationCaptions } from '../../../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../../../localization';
import { RoomInvite } from '../../../../types/room';
import { IconNames, inviteParamName, pathnames } from '../../../../constants';
import { UserType } from '../../../../types/user';
import { ThemedIcon } from '../ThemedIcon/ThemedIcon';
import { Loader } from '../../../../components/Loader/Loader';

import './Invitations.css';

interface InvitationsProps {
  roomId: string;
  roomInvitesData: RoomInvite[] | null;
  roomInvitesLoading: boolean;
  roomInvitesError: string | null;
  onOpen: () => void;
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
  roomId,
  roomInvitesLoading,
  roomInvitesError,
  roomInvitesData,
  onOpen,
  onGenerateInvite,
  onGenerateAllInvites,
}) => {
  const [modalOpen, setModalOpen] = useState(false);
  const localizationCaptions = useLocalizationCaptions();

  const participantTypeLocalization: { [key in UserType]: string } = {
    Viewer: localizationCaptions[LocalizationKey.Viewer],
    Examinee: localizationCaptions[LocalizationKey.Examinee],
    Expert: localizationCaptions[LocalizationKey.Expert],
  };

  const handleOpenModal = useCallback(() => {
    onOpen();
    setModalOpen(true);
  }, [onOpen]);

  const handleCloseModal = useCallback(() => {
    setModalOpen(false);
  }, []);

  const handleCopyToClipboard = (link: string) => {
    window.navigator.clipboard.writeText(link);
  };

  return (
    <>
      <button
        className='invitations-open'
        onClick={handleOpenModal}
      >
        <ThemedIcon name={IconNames.Settings} />
      </button>
      <Modal
        isOpen={modalOpen}
        contentLabel={localizationCaptions[LocalizationKey.Invitations]}
        appElement={document.getElementById('root') || undefined}
        className="action-modal"
        onRequestClose={handleCloseModal}
        style={{
          overlay: {
            backgroundColor: 'rgba(0, 0, 0, 0.75)',
          },
        }}
      >
        <div className="action-modal-header">
          <h3>{localizationCaptions[LocalizationKey.Invitations]}</h3>
          <button onClick={handleCloseModal}>X</button>
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
                <div className='invitations-modal-item-link-field'>
                  <input
                    type='text'
                    readOnly
                    className='invitations-modal-item-link'
                    value={inviteUrlDispaly}
                    onChange={() => { }}
                  />
                  <div className='invitations-modal-item-link-ations'>
                    <button onClick={() => handleCopyToClipboard(inviteUrlDispaly)}><ThemedIcon name={IconNames.Clipboard} /></button>
                    <button onClick={() => onGenerateInvite(roomInvite.participantType)}><ThemedIcon name={IconNames.Refresh} /></button>
                  </div>
                </div>
              </div>
            );
          })}
          <button
            className='invitations-modal-refresh-all'
            onClick={onGenerateAllInvites}
          >
            {localizationCaptions[LocalizationKey.RefreshAll]}<ThemedIcon name={IconNames.Refresh} />
          </button>
        </div>
      </Modal>
    </>
  );
};
