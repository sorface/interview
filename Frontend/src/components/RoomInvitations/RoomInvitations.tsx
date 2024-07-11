import { Fragment, FunctionComponent } from 'react';
import { generatePath } from 'react-router-dom';
import toast from 'react-hot-toast';
import { RoomInvite } from '../../types/room';
import { UserType } from '../../types/user';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../localization';
import { ThemedIcon } from '../../pages/Room/components/ThemedIcon/ThemedIcon';
import { IconNames, inviteParamName, pathnames, toastSuccessOptions } from '../../constants';
import { Loader } from '../Loader/Loader';
import { RoomCreateField } from '../../pages/RoomCreate/RoomCreateField/RoomCreateField';
import { Typography } from '../Typography/Typography';
import { Gap } from '../Gap/Gap';

interface RoomInvitationsProps {
  roomId: string;
  roomInvitesData: RoomInvite[] | null;
  roomInvitesLoading: boolean;
  roomInvitesError: string | null;
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

export const RoomInvitations: FunctionComponent<RoomInvitationsProps> = ({
  roomId,
  roomInvitesLoading,
  roomInvitesError,
  roomInvitesData,
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
    <div>
      {roomInvitesLoading && (
        <div>
          {localizationCaptions[LocalizationKey.RoomInvitesLoading]}...
          <Loader />
        </div>
      )}
      {roomInvitesError && (
        <div>{localizationCaptions[LocalizationKey.Error]}: {roomInvitesError}</div>
      )}
      <RoomCreateField.Wrapper>
        <RoomCreateField.Label className='self-start'>
          <Typography size='m' bold>{localizationCaptions[LocalizationKey.RoomParticipants]}</Typography>
        </RoomCreateField.Label>
        <RoomCreateField.Content>
          {!roomInvitesLoading && roomInvitesData?.sort(sortInvites).map((roomInvite, index) => {
            const lastItem = index === roomInvitesData.length - 1;
            const invitePath = generatePath(pathnames.room, {
              id: roomId,
              [inviteParamName]: roomInvite.inviteId,
            });
            const inviteUrlDispaly = `${window.location.origin}${invitePath}`;

            return (
              <Fragment key={roomInvite.inviteId}>
                <div>
                  <Typography size='m'>{participantTypeLocalization[roomInvite.participantType]}</Typography>
                  <Gap sizeRem={0.5} />
                  <button
                    onClick={() => handleCopyToClipboard(inviteUrlDispaly)}>
                    <ThemedIcon name={IconNames.Link} />
                    {localizationCaptions[LocalizationKey.InviteViaLink]}
                  </button>
                </div>
                {!lastItem && <Gap sizeRem={0.5} />}
              </Fragment>
            );
          })}
        </RoomCreateField.Content>
      </RoomCreateField.Wrapper>
    </div>
  );
};
