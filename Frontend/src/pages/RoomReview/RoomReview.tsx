import { ChangeEvent, Fragment, FunctionComponent, useContext, useEffect, useState } from 'react';
import toast from 'react-hot-toast';
import { useParams } from 'react-router-dom';
import { PageHeader } from '../../components/PageHeader/PageHeader';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../localization';
import { useApiMethod } from '../../hooks/useApiMethod';
import { MyRoomQuestionEvaluation, Room, RoomQuestion } from '../../types/room';
import {
  AddRoomReviewBody,
  GetRoomQuestionsBody,
  roomQuestionApiDeclaration,
  roomQuestionEvaluationApiDeclaration,
  roomReviewApiDeclaration,
  roomsApiDeclaration,
} from '../../apiDeclarations';
import { Loader } from '../../components/Loader/Loader';
import { Gap } from '../../components/Gap/Gap';
import { RoomDateAndTime } from '../../components/RoomDateAndTime/RoomDateAndTime';
import { RoomInfoColumn } from './components/RoomInfoColumn/RoomInfoColumn';
import { Typography } from '../../components/Typography/Typography';
import { RoomReview as RoomReviewType } from '../../types/room';
import { RoomParticipants } from '../../components/RoomParticipants/RoomParticipants';
import { Button } from '../../components/Button/Button';
import { IconNames, roomReviewMaxLength, toastSuccessOptions } from '../../constants';
import { InfoBlock } from '../../components/InfoBlock/InfoBlock';
import { AuthContext } from '../../context/AuthContext';
import { Textarea } from '../../components/Textarea/Textarea';
import { RoomReviewQuestionEvaluation } from './components/RoomReviewQuestionEvaluation/RoomReviewQuestionEvaluation';
import { Modal } from '../../components/Modal/Modal';
import { ModalFooter } from '../../components/ModalFooter/ModalFooter';
import { ModalWarningContent } from '../../components/ModalWarningContent/ModalWarningContent';

export const sortMyRoomReviews = (r1: MyRoomQuestionEvaluation, r2: MyRoomQuestionEvaluation) =>
  r1.order - r2.order;

export const RoomReview: FunctionComponent = () => {
  const auth = useContext(AuthContext);
  const { id } = useParams();
  const localizationCaptions = useLocalizationCaptions();
  const [roomReviewValue, setRoomReviewValue] = useState('');
  const [saveModalOpen, setSaveModalOpen] = useState(false);
  const [closeModalOpen, setCloseModalOpen] = useState(false);

  const { apiMethodState, fetchData } = useApiMethod<Room, Room['id']>(roomsApiDeclaration.getById);
  const { process: { loading, error }, data: room } = apiMethodState;

  const {
    apiMethodState: getMyQuestionEvaluationsState,
    fetchData: fetchMyRoomQuestionEvaluations,
  } = useApiMethod<MyRoomQuestionEvaluation[], Room['id']>(roomQuestionEvaluationApiDeclaration.getMy);
  const {
    process: { loading: myQuestionEvaluationsLoading, error: myQuestionEvaluationsError },
    data: myQuestionEvaluations,
  } = getMyQuestionEvaluationsState;

  const {
    apiMethodState: getMyRoomReviewState,
    fetchData: fetchMyRoomReview,
  } = useApiMethod<RoomReviewType, Room['id']>(roomReviewApiDeclaration.getMy);
  const { process: { loading: myRoomReviewLoading, error: myRoomReviewError }, data: myRoomReview } = getMyRoomReviewState;

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
      loading: roomCloseLoading,
      error: roomCloseError,
      code: roomCloseCode,
    },
  } = apiRoomCloseMethodState;

  const participant = room?.participants.find(
    participant => participant.id === auth?.id
  );
  const canWriteReview = participant?.type === 'Expert';
  const totalError = error || myQuestionEvaluationsError || myRoomReviewError;
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
    fetchMyRoomReview(id);
    fetchMyRoomQuestionEvaluations(id);
  }, [id, fetchData, getRoomQuestions, fetchMyRoomReview, fetchMyRoomQuestionEvaluations]);

  useEffect(() => {
    if (roomCloseCode !== 200) {
      return;
    }
    handleCloseCloseModal();
    toast.success(localizationCaptions[LocalizationKey.Saved], toastSuccessOptions);
  }, [roomCloseCode, localizationCaptions]);

  useEffect(() => {
    if (!addedRoomReview) {
      return;
    }
    handleCloseSaveModal();
    toast.success(localizationCaptions[LocalizationKey.Saved], toastSuccessOptions);
    fetchMyRoomReview(id || '');
  }, [id, addedRoomReview, localizationCaptions, fetchMyRoomReview]);

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
    if (myRoomReview) {
      return;
    }
    setRoomReviewValue(event.target.value);
  };

  const handleOpenSaveModal = () => {
    setSaveModalOpen(true);
  };

  const handleCloseSaveModal = () => {
    setSaveModalOpen(false);
  };

  const handleOpenCloseModal = () => {
    setCloseModalOpen(true);
  };

  const handleCloseCloseModal = () => {
    setCloseModalOpen(false);
  };

  if (loading || loadingRoomQuestions || !room || !roomQuestions || myQuestionEvaluationsLoading || myRoomReviewLoading) {
    return (
      <Loader />
    );
  }

  return (
    <>
      <PageHeader title={localizationCaptions[LocalizationKey.RoomReviewPageName]} />
      <div className='overflow-auto'>
        <h2 className='text-left'>{room.name}</h2>
        <Gap sizeRem={1} />
        <InfoBlock className='text-left flex justify-between'>
          {totalError && (
            <Typography size='m'>{localizationCaptions[LocalizationKey.Error]}: {totalError}</Typography>
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
        {myRoomReview && (
          <InfoBlock className='text-left flex flex-col'>
            <Typography size='m' bold>{localizationCaptions[LocalizationKey.RoomReviewAlreadyGiven]}</Typography>
          </InfoBlock>
        )}
        {!canWriteReview && (
          <InfoBlock className='text-left flex flex-col'>
            <Typography size='m' bold>{localizationCaptions[LocalizationKey.RoomReviewWaiting]}</Typography>
          </InfoBlock>
        )}
        {(!myRoomReview && canWriteReview) && (
          <>
            <InfoBlock className='text-left flex flex-col'>
              <Typography size='s' bold>{localizationCaptions[LocalizationKey.CandidateOpinion]}</Typography>
              <Gap sizeRem={1} />
              <Textarea
                className='h-3.625'
                maxLength={roomReviewMaxLength}
                showMaxLength={true}
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
              {myQuestionEvaluations && myQuestionEvaluations.sort(sortMyRoomReviews).map((questionEvaluations, index) => (
                <Fragment key={questionEvaluations.id}>
                  <RoomReviewQuestionEvaluation
                    roomId={room.id}
                    questionEvaluations={questionEvaluations}
                    readOnly={!!myRoomReview}
                  />
                  {index !== roomQuestions.length - 1 && (<Gap sizeRem={0.25} />)}
                </Fragment>
              ))}
            </InfoBlock>
          </>
        )}
        <Gap sizeRem={1.75} />
        <Modal
          contentLabel=''
          open={saveModalOpen}
          onClose={handleCloseSaveModal}
        >
          <ModalWarningContent
            iconName={IconNames.HelpCircle}
            captionLine1={localizationCaptions[LocalizationKey.SaveRoomQuestionEvaluationWarningLine1]}
            captionLine2={localizationCaptions[LocalizationKey.SaveRoomQuestionEvaluationWarningLine2]}
          />
          <ModalFooter>
            <Button onClick={handleCloseSaveModal}>{localizationCaptions[LocalizationKey.Cancel]}</Button>
            <Button onClick={handleSaveReview} variant='active'>
              {addRoomReviewLoading ? (
                <Loader />
              ) : (
                localizationCaptions[LocalizationKey.Save]
              )}
            </Button>
          </ModalFooter>
        </Modal>
        <Modal
          contentLabel=''
          open={closeModalOpen}
          onClose={handleCloseCloseModal}
        >
          <ModalWarningContent
            iconName={IconNames.HelpCircle}
            captionLine1={localizationCaptions[LocalizationKey.CloseRoomWithoutQuestionEvaluationWarningLine1]}
            captionLine2={localizationCaptions[LocalizationKey.CloseRoomWithoutQuestionEvaluationWarningLine2]}
          />
          <ModalFooter>
            <Button onClick={handleCloseCloseModal}>{localizationCaptions[LocalizationKey.Cancel]}</Button>
            <Button onClick={handleCloseRoom} variant='active'>
              {roomCloseLoading ? (
                <Loader />
              ) : (
                localizationCaptions[LocalizationKey.Save]
              )}
            </Button>
          </ModalFooter>
        </Modal>
        <div className='flex justify-end'>
          {(room.owner.id === auth?.id) && (
            <>
              <Button
                onClick={handleOpenCloseModal}
              >
                {localizationCaptions[LocalizationKey.CloseRoomWithoutReview]}
              </Button>
              <Gap sizeRem={1} horizontal />
            </>
          )}
          {(!myRoomReview && canWriteReview) && (
            <Button
              variant='active'
              onClick={handleOpenSaveModal}
            >
              {addRoomReviewLoading ? (
                <Loader />
              ) : (
                localizationCaptions[LocalizationKey.RoomReviewSave]
              )}
            </Button>
          )}
        </div>
        <Gap sizeRem={0.75} />
      </div>
    </>
  );
};
