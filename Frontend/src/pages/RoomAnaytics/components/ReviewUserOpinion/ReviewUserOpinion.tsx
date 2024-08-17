import { FunctionComponent } from 'react';
import { User, UserType } from '../../../../types/user';
import { RoomQuestionEvaluation } from '../../../../types/room';
import { Typography } from '../../../../components/Typography/Typography';
import { UserAvatar } from '../../../../components/UserAvatar/UserAvatar';
import { CircularProgress } from '../../../../components/CircularProgress/CircularProgress';
import { Gap } from '../../../../components/Gap/Gap';
import { useLocalizationCaptions } from '../../../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../../../localization';
import { AnalyticsUserReview } from '../../../../types/analytics';

interface ReviewUserOpinionProps {
  user: {
    id: User['id'];
    evaluation?: Partial<Omit<RoomQuestionEvaluation, 'id'>>;
  };
  allUsers: Map<User['id'], AnalyticsUserReview>;
}

export const ReviewUserOpinion: FunctionComponent<ReviewUserOpinionProps> = ({
  user,
  allUsers,
}) => {
  const localizationCaptions = useLocalizationCaptions();
  const participantTypeLocalization: { [key in UserType]: string } = {
    Viewer: localizationCaptions[LocalizationKey.Viewer],
    Examinee: localizationCaptions[LocalizationKey.Examinee],
    Expert: localizationCaptions[LocalizationKey.Expert],
  };
  const currentUser = allUsers.get(user.id);

  return (
    <div className='text-left'>
      <div className='flex justify-between'>
        <div className='flex'>
          <UserAvatar size='m' nickname={currentUser?.nickname || ''} src={currentUser?.avatar} />
          <Gap sizeRem={1} horizontal />
          <div className='flex flex-col'>
            <Typography size='m' bold>{currentUser?.nickname}</Typography>
            <span className='opacity-0.5'>
              <Typography size='m' bold>
                {currentUser && participantTypeLocalization[currentUser.participantType]}
              </Typography>
            </span>
          </div>
        </div>
        <div>
          {!!user.evaluation && (
            <CircularProgress
            value={user.evaluation.mark ? user.evaluation.mark * 10 : 0}
            caption={user.evaluation.mark ? user.evaluation.mark.toFixed(1) : 0}
            size='s'
          />
          )}
        </div>
      </div>
      <Gap sizeRem={1} />
      <Typography size='m'>
        {user.evaluation?.review}
      </Typography>
    </div>
  );
};
