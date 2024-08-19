import { Fragment, FunctionComponent, useEffect, useState } from 'react';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { useParams } from 'react-router-dom';
import { useApiMethod } from '../../hooks/useApiMethod';
import { Analytics, AnalyticsUserReview } from '../../types/analytics';
import { roomsApiDeclaration } from '../../apiDeclarations';
import { Room, RoomQuestion } from '../../types/room';
import { InfoBlock } from '../../components/InfoBlock/InfoBlock';
import { PageHeader } from '../../components/PageHeader/PageHeader';
import { LocalizationKey } from '../../localization';
import { Loader } from '../../components/Loader/Loader';
import { Gap } from '../../components/Gap/Gap';
import { CircularProgress } from '../../components/CircularProgress/CircularProgress';
import { RoomInfoColumn } from '../RoomReview/components/RoomInfoColumn/RoomInfoColumn';
import { RoomDateAndTime } from '../../components/RoomDateAndTime/RoomDateAndTime';
import { RoomParticipants } from '../../components/RoomParticipants/RoomParticipants';
import { Typography } from '../../components/Typography/Typography';
import { QuestionItem } from '../../components/QuestionItem/QuestionItem';
import { Question } from '../../types/question';
import { ReviewUserOpinion } from './components/ReviewUserOpinion/ReviewUserOpinion';
import { ReviewUserGrid } from './components/ReviewUserGrid/ReviewUserGrid';
import { Modal } from '../../components/Modal/Modal';
import { User } from '../../types/user';

const createFakeQuestion = (roomQuestion: RoomQuestion): Question => ({
  ...roomQuestion,
  tags: [],
  answers: [],
  codeEditor: null,
});

const generateUserOpinion = (userReview: AnalyticsUserReview) => ({
  id: userReview.userId,
  nickname: userReview.nickname,
  participantType: userReview.participantType,
  evaluation: {
    mark: userReview.averageMark,
    review: userReview.comment,
  }
});

const getAllUsers = (data: Analytics) => {
  const users: Map<User['id'], AnalyticsUserReview> = new Map();
  data.userReview.forEach(userReview => {
    users.set(userReview.userId, userReview);
  });
  return users;
};

export const RoomAnaytics: FunctionComponent = () => {
  const localizationCaptions = useLocalizationCaptions();
  const { id } = useParams();
  const [openedQuestionDetails, setOpenedQuestionDetails] = useState('');

  const { apiMethodState, fetchData } = useApiMethod<Analytics, Room['id']>(roomsApiDeclaration.analytics);
  const { data, process: { loading, error } } = apiMethodState;

  const {
    apiMethodState: roomApiMethodState,
    fetchData: fetchRoom,
  } = useApiMethod<Room, Room['id']>(roomsApiDeclaration.getById);
  const {
    process: { loading: roomLoading, error: roomError },
    data: room,
  } = roomApiMethodState;

  const totalError = error || roomError;

  const allUsers = data ? getAllUsers(data) : new Map<User['id'], AnalyticsUserReview>();

  const examinee = room?.participants.find(
    participant => participant.type === 'Examinee'
  );

  useEffect(() => {
    if (!id) {
      throw new Error('Room id not found');
    }
    fetchData(id);
    fetchRoom(id);
  }, [id, fetchData, fetchRoom]);

  const handleQuestionClick = (question: Question) => {
    setOpenedQuestionDetails(question.id);
  };

  const handleQuestionDetailsClose = () => {
    setOpenedQuestionDetails('');
  };

  if (loading || roomLoading) {
    return (
      <Loader />
    );
  }

  return (
    <>
      <Modal
        open={!!openedQuestionDetails}
        contentLabel={localizationCaptions[LocalizationKey.QuestionAnswerDetails]}
        onClose={handleQuestionDetailsClose}
      >
        <ReviewUserGrid>
          {data?.questions.find(question => question.id === openedQuestionDetails)?.users
            .filter(questionUser => allUsers.get(questionUser.id)?.participantType === 'Expert')
            .map(questionUser => {
              if (!questionUser.evaluation) {
                return undefined;
              }
              return (
                <ReviewUserOpinion
                  key={questionUser.id}
                  user={questionUser}
                  allUsers={allUsers}
                />
              );
            })}
        </ReviewUserGrid>
        <Gap sizeRem={1} />
      </Modal>

      <PageHeader title={`${localizationCaptions[LocalizationKey.RoomReviewPageName]} ${room?.name}`} />
      <Gap sizeRem={1} />
      {totalError && (
        <div className='text-left'>
          <Typography size='m' error>{localizationCaptions[LocalizationKey.Error]}: {totalError}</Typography>
        </div>
      )}
      <div className='flex text-left'>
        <InfoBlock className='flex-1'>
          <div className='flex'>
            {room?.scheduledStartTime && (
              <RoomInfoColumn
                header={localizationCaptions[LocalizationKey.RoomDateAndTime]}
                conent={
                  <RoomDateAndTime
                    typographySize='s'
                    scheduledStartTime={room.scheduledStartTime}
                    timer={room.timer}
                    mini
                  />
                }
                mini
              />
            )}
            <RoomInfoColumn
              header={localizationCaptions[LocalizationKey.Examinee]}
              conent={examinee ? <span className='capitalize'>{examinee.nickname}</span> : localizationCaptions[LocalizationKey.NotFound]}
              mini
            />
          </div>
          <Gap sizeRem={2} />
          <RoomInfoColumn
            header={localizationCaptions[LocalizationKey.RoomParticipants]}
            conent={<RoomParticipants participants={room?.participants || []} />}
            mini
          />
        </InfoBlock>
        <Gap sizeRem={0.5} horizontal />
        <InfoBlock className='px-5 flex flex-col items-center'>
          <Typography size='m' bold>
            {localizationCaptions[LocalizationKey.AverageCandidateMark]}
          </Typography>
          <Gap sizeRem={1} />
          {!!data && (
            <CircularProgress
              value={data.averageMark * 10}
              caption={data.averageMark.toFixed(1)}
              size='m'
            />
          )}
        </InfoBlock>
      </div>
      <Gap sizeRem={0.5} />
      <InfoBlock className='text-left'>
        <Typography size='xl' bold>
          {localizationCaptions[LocalizationKey.OpinionsAndMarks]}
          <Gap sizeRem={2} />
          <ReviewUserGrid>
            {data?.userReview
              .filter(userReview => allUsers.get(userReview.userId)?.participantType === 'Expert')
              .map((userReview) => (
                <ReviewUserOpinion
                  key={userReview.userId}
                  user={generateUserOpinion(userReview)}
                  allUsers={allUsers}
                />
              ))}
          </ReviewUserGrid>
        </Typography>
      </InfoBlock>
      <Gap sizeRem={0.5} />
      <InfoBlock className='text-left'>
        <Typography size='xl' bold>
          {localizationCaptions[LocalizationKey.MarksForQuestions]}
        </Typography>
        <Gap sizeRem={2} />
        {data?.questions.map((question, index, questions) => {
          return (
            <Fragment key={question.id}>
              <QuestionItem
                question={createFakeQuestion(question)}
                mark={question.averageMark}
                onClick={handleQuestionClick}
              />
              {index !== questions.length - 1 && (<Gap sizeRem={0.25} />)}
            </Fragment>
          );
        })}
      </InfoBlock>
    </>
  );
};
