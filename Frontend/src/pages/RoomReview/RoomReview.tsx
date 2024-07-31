import { Fragment, FunctionComponent, useEffect } from 'react';
import { PageHeader } from '../../components/PageHeader/PageHeader';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../localization';
import { useApiMethod } from '../../hooks/useApiMethod';
import { Room, RoomQuestion } from '../../types/room';
import { GetRoomQuestionsBody, roomQuestionApiDeclaration, roomsApiDeclaration } from '../../apiDeclarations';
import { useParams } from 'react-router-dom';
import { Loader } from '../../components/Loader/Loader';
import { Gap } from '../../components/Gap/Gap';
import { RoomDateAndTime } from '../../components/RoomDateAndTime/RoomDateAndTime';
import { RoomInfoColumn } from './components/RoomInfoColumn/RoomInfoColumn';
import { Typography } from '../../components/Typography/Typography';
import { QuestionItem } from '../../components/QuestionItem/QuestionItem';
import { Question } from '../../types/question';
import { RoomQuestionEvaluation } from '../Room/components/RoomQuestionEvaluation/RoomQuestionEvaluation';
import { RoomParticipants } from '../../components/RoomParticipants/RoomParticipants';

const createFakeQuestion = (roomQuestion: RoomQuestion): Question => ({
  ...roomQuestion,
  tags: [],
  answers: [],
  codeEditor: null,
});

export const RoomReview: FunctionComponent = () => {
  const { id } = useParams();
  const localizationCaptions = useLocalizationCaptions();

  const { apiMethodState, fetchData } = useApiMethod<Room, Room['id']>(roomsApiDeclaration.getById);
  const { process: { loading, error }, data: room } = apiMethodState;

  const {
    apiMethodState: apiRoomQuestions,
    fetchData: getRoomQuestions,
  } = useApiMethod<Array<RoomQuestion>, GetRoomQuestionsBody>(roomQuestionApiDeclaration.getRoomQuestions);
  const {
    data: roomQuestions,
    process: {
      error: errorRoomQuestions,
      loading: loadingRoomQuestions,
    }
  } = apiRoomQuestions;

  useEffect(() => {
    if (!id) {
      throw new Error('Room id not found');
    }
    fetchData(id);
    getRoomQuestions({
      RoomId: id,
      States: ['Closed'],
    });
  }, [id, fetchData, getRoomQuestions]);

  if (loading || loadingRoomQuestions || !room || !roomQuestions) {
    return (
      <Loader />
    );
  }

  return (
    <>
      <PageHeader title={localizationCaptions[LocalizationKey.RoomReviewPageName]} />
      <h2 className='text-left'>{room.name}</h2>
      <Gap sizeRem={1} />
      <div className='text-left flex justify-between bg-wrap px-1.5 py-1.75 rounded-0.75'>
        {error && (
          <Typography size='m'>{localizationCaptions[LocalizationKey.Error]}: {error}</Typography>
        )}
        <RoomInfoColumn
          header={localizationCaptions[LocalizationKey.Examinee]}
          conent={localizationCaptions[LocalizationKey.NotFound]}
        />
        <RoomInfoColumn
          header={localizationCaptions[LocalizationKey.RoomParticipants]}
          conent={<RoomParticipants participants={room.participants} />}
        />
        {room.scheduledStartTime && (
          <RoomInfoColumn
            header={localizationCaptions[LocalizationKey.RoomDateAndTime]}
            conent={
              <RoomDateAndTime
                scheduledStartTime={room.scheduledStartTime}
                timer={room.timer}
                mini
              />
            }
          />
        )}
      </div>
      <Gap sizeRem={0.5} />
      <div className='text-left flex flex-col bg-wrap px-1.5 py-1.75 rounded-0.75'>
        <Typography size='s' bold>{localizationCaptions[LocalizationKey.CandidateOpinion]}</Typography>
        <Gap sizeRem={1} />
        <textarea className='h-3.625' />
      </div>
      <Gap sizeRem={0.5} />
      <div className='text-left bg-wrap px-1.5 py-1.75 rounded-0.75'>
        <Typography size='xl' bold>{localizationCaptions[LocalizationKey.CandidateMarks]}</Typography>
        <Gap sizeRem={2} />
        {errorRoomQuestions && (
          <Typography size='m'>{localizationCaptions[LocalizationKey.Error]}: {errorRoomQuestions}</Typography>
        )}
        {roomQuestions.map((roomQuestion, index) => (
          <Fragment key={roomQuestion.id}>
            <QuestionItem
              question={createFakeQuestion(roomQuestion)}
              checked={false}
              checkboxLabel={<Typography size='m' bold>{localizationCaptions[LocalizationKey.DoNotRate]}</Typography>}
              onCheck={() => { }}
            >
              <RoomQuestionEvaluation
                value={{ mark: 5, review: 'NORM' }}
                onChange={() => { }}
              />
            </QuestionItem>
            {index !== roomQuestions.length - 1 && (<Gap sizeRem={0.25} />)}
          </Fragment>
        ))}
      </div>
    </>
  );
};
