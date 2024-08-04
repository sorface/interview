import { FunctionComponent } from 'react';
import { User, UserType } from '../../../../types/user';
import { RoomQuestionEvaluation } from '../../../../types/room';
import { Typography } from '../../../../components/Typography/Typography';
import { UserAvatar } from '../../../../components/UserAvatar/UserAvatar';
import { CircularProgress } from '../../../../components/CircularProgress/CircularProgress';
import { Gap } from '../../../../components/Gap/Gap';
import { useLocalizationCaptions } from '../../../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../../../localization';

interface ReviewUserOpinionProps {
  user: {
    id: User['id'];
    nickname: User['nickname'];
    avatar?: User['avatar'];
    participantType: UserType;
    evaluation: Omit<RoomQuestionEvaluation, 'id'>;
  };
}

export const ReviewUserOpinion: FunctionComponent<ReviewUserOpinionProps> = ({
  user,
}) => {
  const localizationCaptions = useLocalizationCaptions();
  const participantTypeLocalization: { [key in UserType]: string } = {
    Viewer: localizationCaptions[LocalizationKey.Viewer],
    Examinee: localizationCaptions[LocalizationKey.Examinee],
    Expert: localizationCaptions[LocalizationKey.Expert],
  };

  return (
    <div className='text-left'>
      <div className='flex justify-between'>
        <div className='flex'>
          <UserAvatar size='m' nickname={user.nickname} src={user.avatar} />
          <Gap sizeRem={1} horizontal />
          <div className='flex flex-col'>
            <Typography size='m' bold>{user.nickname}</Typography>
            <span className='opacity-0.5'>
              <Typography size='m' bold>{participantTypeLocalization[user.participantType]}</Typography>
            </span>
          </div>
        </div>
        <div>
          <CircularProgress
            value={user.evaluation.mark * 10}
            caption={user.evaluation.mark.toFixed(1)}
            size='s'
          />
        </div>
      </div>
      <Gap sizeRem={1} />
      <Typography size='m'>
        {user.evaluation.review}
      </Typography>
    </div>
  );
};
