import React, { FunctionComponent } from 'react';
import { User } from '../../../../types/user';
import { RoomQuestionEvaluation } from '../../../../types/room';
import { Typography } from '../../../../components/Typography/Typography';
import { UserAvatar } from '../../../../components/UserAvatar/UserAvatar';
import { CircularProgress } from '../../../../components/CircularProgress/CircularProgress';
import { Gap } from '../../../../components/Gap/Gap';
import { AnalyticsUserReview } from '../../../../types/analytics';
import { useParticipantTypeLocalization } from '../../../../hooks/useParticipantTypeLocalization';
import { useLocalizationCaptions } from '../../../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../../../localization';

interface DetailedEvaluation {
  recommendation?: string;
  expected?: string;
}

interface ReviewUserOpinionProps {
  user: {
    id: User['id'];
    evaluation?: Partial<Omit<RoomQuestionEvaluation, 'id'>> &
      DetailedEvaluation;
  };
  allUsers: Map<User['id'], AnalyticsUserReview>;
}

export const ReviewUserOpinion: FunctionComponent<ReviewUserOpinionProps> = ({
  user,
  allUsers,
}) => {
  const localizationCaptions = useLocalizationCaptions();
  const localizeParticipantType = useParticipantTypeLocalization();
  const currentUser = allUsers.get(user.id);

  return (
    <div className="text-left">
      <div className="flex justify-between">
        <div className="flex">
          <UserAvatar
            size="m"
            nickname={currentUser?.nickname || ''}
            src={currentUser?.avatar}
          />
          <Gap sizeRem={1} horizontal />
          <div className="flex flex-col break-all">
            <Typography size="m" bold>
              {currentUser?.nickname}
            </Typography>
            <span className="opacity-0.5">
              <Typography size="m" bold>
                {currentUser &&
                  localizeParticipantType(currentUser.participantType)}
              </Typography>
            </span>
          </div>
        </div>
        <div>
          <CircularProgress
            value={
              typeof user.evaluation?.mark === 'number'
                ? user.evaluation.mark * 10
                : null
            }
            caption={
              typeof user.evaluation?.mark === 'number'
                ? user.evaluation.mark.toFixed(1)
                : null
            }
            size="s"
          />
        </div>
      </div>
      <Gap sizeRem={1} />
      <Typography size="m" secondary={!user.evaluation?.review}>
        {user.evaluation?.review ??
          localizationCaptions[LocalizationKey.RoomReviewWaiting]}
      </Typography>
      {user.evaluation?.recommendation && (
        <div className="flex flex-col">
          <Gap sizeRem={1} />
          <Typography size="m" bold>
            {localizationCaptions[LocalizationKey.Recommendation]}
          </Typography>
          <Typography size="m">{user.evaluation.recommendation}</Typography>
        </div>
      )}
      {user.evaluation?.expected && (
        <div className="flex flex-col">
          <Gap sizeRem={1} />
          <Typography size="m" bold>
            {localizationCaptions[LocalizationKey.ExampleOfCorrectAnswer]}
          </Typography>
          <Typography size="m">{user.evaluation.expected}</Typography>
        </div>
      )}
    </div>
  );
};
