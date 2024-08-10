import { ChangeEvent, Fragment, FunctionComponent, useContext, useEffect, useState } from 'react';
import toast from 'react-hot-toast';
import { useParams } from 'react-router-dom';
import { PageHeader } from '../../components/PageHeader/PageHeader';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../localization';
import { useApiMethod } from '../../hooks/useApiMethod';
import { Room, RoomQuestion } from '../../types/room';
import { AddRoomReviewBody, GetRoomQuestionsBody, roomQuestionApiDeclaration, roomReviewApiDeclaration, roomsApiDeclaration } from '../../apiDeclarations';
import { Loader } from '../../components/Loader/Loader';
import { Gap } from '../../components/Gap/Gap';
import { RoomDateAndTime } from '../../components/RoomDateAndTime/RoomDateAndTime';
import { RoomInfoColumn } from './components/RoomInfoColumn/RoomInfoColumn';
import { Typography } from '../../components/Typography/Typography';
import { QuestionItem } from '../../components/QuestionItem/QuestionItem';
import { Question } from '../../types/question';
import { RoomReview as RoomReviewType } from '../../types/room';
import { RoomQuestionEvaluation } from '../Room/components/RoomQuestionEvaluation/RoomQuestionEvaluation';
import { RoomParticipants } from '../../components/RoomParticipants/RoomParticipants';
import { Button } from '../../components/Button/Button';
import { toastSuccessOptions } from '../../constants';
import { InfoBlock } from '../../components/InfoBlock/InfoBlock';
import { AuthContext } from '../../context/AuthContext';
import { checkAdmin } from '../../utils/checkAdmin';

const createFakeQuestion = (roomQuestion: RoomQuestion): Question => ({
  ...roomQuestion,
  tags: [],
  answers: [],
  codeEditor: null,
});

export const RoomReview: FunctionComponent = () => {
  const auth = useContext(AuthContext);
  const admin = checkAdmin(auth);
  const { id } = useParams();
  const localizationCaptions = useLocalizationCaptions();
  const [roomReviewValue, setRoomReviewValue] = useState('');

  const { apiMethodState, fetchData } = useApiMethod<Room, Room['id']>(roomsApiDeclaration.getById);
  const { process: { loading, error }, data: room } = apiMethodState;

  const {
    apiMethodState: addRoomReviewState,
    fetchData: fetchAddRoomReview,
  } = useApiMethod<RoomReviewType, AddRoomReviewBody>(roomReviewApiDeclaration.addReview);
  const { process: { loading: addRoomReviewLoading, error: addRoomReviewError }, data: addedRoomReview } = addRoomReviewState;

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

  const {
    apiMethodState: apiRoomCloseMethodState,
    fetchData: fetchRoomClose,
  } = useApiMethod<unknown, Room['id']>(roomsApiDeclaration.close);
  const {
    process: {
      error: roomCloseError,
      code: roomCloseCode,
    },
  } = apiRoomCloseMethodState;

  const examinee = room?.participants.find(
    participant => participant.type === 'Examinee'
  );

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

  useEffect(() => {
    if (roomCloseCode !== 200) {
      return;
    }
    toast.success(localizationCaptions[LocalizationKey.Saved], toastSuccessOptions);
  }, [roomCloseCode, localizationCaptions]);

  useEffect(() => {
    if (!addedRoomReview) {
      return;
    }
    toast.success(localizationCaptions[LocalizationKey.Saved], toastSuccessOptions);
  }, [addedRoomReview, localizationCaptions]);

  useEffect(() => {
    if (!roomCloseError) {
      return;
    }
    toast.error(roomCloseError);
  }, [roomCloseError]);

  useEffect(() => {
    if (!addRoomReviewError) {
      return;
    }
    toast.error(addRoomReviewError);
  }, [addRoomReviewError]);

  const handleCloseRoom = () => {
    if (!id) {
      throw new Error('Room id not found');
    }
    fetchRoomClose(id);
  };

  const handleSaveReview = () => {
    if (!id) {
      throw new Error('Room id not found');
    }
    fetchAddRoomReview({
      roomId: id,
      review: roomReviewValue,
    });
  };

  const handleReviewChange = (event: ChangeEvent<HTMLTextAreaElement>) => {
    setRoomReviewValue(event.target.value);
  };

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
      <InfoBlock className='text-left flex justify-between'>
        {error && (
          <Typography size='m'>{localizationCaptions[LocalizationKey.Error]}: {error}</Typography>
        )}
        <RoomInfoColumn
          header={localizationCaptions[LocalizationKey.Examinee]}
          conent={examinee ? <span className='capitalize'>{examinee.nickname}</span> : localizationCaptions[LocalizationKey.NotFound]}
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
                typographySize='m'
                scheduledStartTime={room.scheduledStartTime}
                timer={room.timer}
                mini
              />
            }
          />
        )}
      </InfoBlock>
      <Gap sizeRem={0.5} />
      <InfoBlock className='text-left flex flex-col'>
        <Typography size='s' bold>{localizationCaptions[LocalizationKey.CandidateOpinion]}</Typography>
        <Gap sizeRem={1} />
        <textarea
          className='h-3.625'
          value={roomReviewValue}
          onInput={handleReviewChange}
        />
      </InfoBlock>
      <Gap sizeRem={0.5} />
      <InfoBlock className='text-left'>
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
      </InfoBlock>
      <Gap sizeRem={1.75} />
      <div className='flex justify-end'>
        {admin && (
          <>
            <Button
              onClick={handleCloseRoom}
            >
              {localizationCaptions[LocalizationKey.CloseRoom]}
            </Button>
            <Gap sizeRem={1} horizontal />
          </>
        )}
        <Button
          variant='active'
          onClick={handleSaveReview}
        >
          {addRoomReviewLoading ? (
            <Loader />
          ) : (
            localizationCaptions[LocalizationKey.Save]
          )}

        </Button>
      </div>
      <Gap sizeRem={0.75} />
    </>
  );
};
