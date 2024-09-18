import { Fragment, FunctionComponent } from 'react';
import { generatePath } from 'react-router-dom';
import toast from 'react-hot-toast';
import { RoomInvite } from '../../types/room';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../localization';
import { Icon } from '../../pages/Room/components/Icon/Icon';
import { IconNames, inviteParamName, pathnames } from '../../constants';
import { Loader } from '../Loader/Loader';
import { RoomCreateField } from '../../pages/RoomCreate/RoomCreateField/RoomCreateField';
import { Typography } from '../Typography/Typography';
import { Gap } from '../Gap/Gap';
import { Button } from '../Button/Button';
import { useParticipantTypeLocalization } from '../../hooks/useParticipantTypeLocalization';

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

  const localizeParticipantType = useParticipantTypeLocalization();

  const handleCopyToClipboard = (link: string) => {
    window.navigator.clipboard.writeText(link);
    toast.success(localizationCaptions[LocalizationKey.CopiedToClipboard]);
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
                  <Typography size='m' bold>{localizeParticipantType(roomInvite.participantType)}</Typography>
                  <Gap sizeRem={0.5} />
                  <Button
                    variant='active2'
                    onClick={() => handleCopyToClipboard(inviteUrlDispaly)}
                  >
                    <Icon name={IconNames.Link} />
                    <Gap sizeRem={0.5} horizontal />
                    {localizationCaptions[LocalizationKey.InviteViaLink]}
                  </Button>
                </div>
                {!lastItem && <Gap sizeRem={1.5} />}
              </Fragment>
            );
          })}
        </RoomCreateField.Content>
      </RoomCreateField.Wrapper>
    </div>
  );
};
