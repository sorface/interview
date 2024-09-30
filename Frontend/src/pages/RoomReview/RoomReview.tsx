import { ChangeEvent, Fragment, FunctionComponent, useContext, useEffect, useState } from 'react';
import toast from 'react-hot-toast';
import { generatePath, Navigate, useParams } from 'react-router-dom';
import { PageHeader } from '../../components/PageHeader/PageHeader';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../localization';
import { useApiMethod } from '../../hooks/useApiMethod';
import { MyRoomQuestionEvaluation, Room } from '../../types/room';
import {
  CompleteRoomReviewsBody,
  UpsertRoomReviewsBody,
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
import { HttpResponseCode, IconNames, pathnames, roomReviewMaxLength } from '../../constants';
import { InfoBlock } from '../../components/InfoBlock/InfoBlock';
import { AuthContext } from '../../context/AuthContext';
import { Textarea } from '../../components/Textarea/Textarea';
import { RoomReviewQuestionEvaluation } from './components/RoomReviewQuestionEvaluation/RoomReviewQuestionEvaluation';
import { Modal } from '../../components/Modal/Modal';
import { ModalFooter } from '../../components/ModalFooter/ModalFooter';
import { ModalWarningContent } from '../../components/ModalWarningContent/ModalWarningContent';
import { QuestionAnswerDetails } from '../../components/QuestionAnswerDetails/QuestionAnswerDetails';
import { Icon } from '../Room/components/Icon/Icon';
import { User } from '../../types/user';

const upsertRoomReviewDebounceMs = 1000;
export const sortMyRoomReviews = (r1: MyRoomQuestionEvaluation, r2: MyRoomQuestionEvaluation) =>
  r1.order - r2.order;

const getAllUsers = (data: Room) => {
  const users: Map<User['id'], Pick<User, 'nickname' | 'avatar'>> = new Map();
  data.participants.forEach(participant => {
    users.set(participant.id, {
      nickname: participant.nickname,
      avatar: participant.avatar,
    });
  });
  return users;
};

export const RoomReview: FunctionComponent = () => {
  const auth = useContext(AuthContext);
  const { id } = useParams();
  const localizationCaptions = useLocalizationCaptions();
  const [roomReviewValue, setRoomReviewValue] = useState<string | null>(null);
  const [saveModalOpen, setSaveModalOpen] = useState(false);
  const [closeModalOpen, setCloseModalOpen] = useState(false);
  const [openedAnswerDetailsId, setOpenedAnswerDetailsId] = useState<string | null>(null);

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
    apiMethodState: completeRoomReviewState,
    fetchData: fetchCompleteRoomReview,
  } = useApiMethod<RoomReviewType, CompleteRoomReviewsBody>(roomReviewApiDeclaration.complete);
  const { process: { loading: completeRoomReviewLoading, error: completeRoomReviewError }, data: completedRoomReview } = completeRoomReviewState;

  const {
    apiMethodState: upsertRoomReviewState,
    fetchData: fetchUpsertRoomReview,
  } = useApiMethod<RoomReviewType, UpsertRoomReviewsBody>(roomReviewApiDeclaration.upsert);
  const { process: { loading: upsertRoomReviewLoading, error: upsertRoomReviewError }, data: upsertedRoomReview } = upsertRoomReviewState;

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

  const canWriteReview = participant ? participant.type === 'Expert' : true;
  const reviewCompleted = myRoomReview ? myRoomReview.state === 'Closed' : false;
  const shouldRedirectToAnalytics = (
    (!canWriteReview || reviewCompleted) ||
    (completedRoomReview || roomCloseCode === HttpResponseCode.Ok)
  );
  const totalError = error || myQuestionEvaluationsError || myRoomReviewError;
  const examinee = room?.participants.find(
    participant => participant.type === 'Examinee'
  );
  const allUsers = room ? getAllUsers(room) : new Map<User['id'], Pick<User, 'nickname' | 'avatar'>>();

  useEffect(() => {
    if (!id) {
      throw new Error('Room id not found');
    }
    fetchData(id);
    fetchMyRoomReview(id);
    fetchMyRoomQuestionEvaluations(id);
  }, [id, fetchData, fetchMyRoomReview, fetchMyRoomQuestionEvaluations]);

  useEffect(() => {
    if (roomCloseCode !== HttpResponseCode.Ok) {
      return;
    }
    handleCloseCloseModal();
    toast.success(localizationCaptions[LocalizationKey.Saved]);
  }, [roomCloseCode, localizationCaptions]);

  useEffect(() => {
    if (!completedRoomReview) {
      return;
    }
    handleCloseSaveModal();
    toast.success(localizationCaptions[LocalizationKey.Saved]);
  }, [id, completedRoomReview, localizationCaptions, fetchMyRoomReview]);

  useEffect(() => {
    if (!roomCloseError) {
      return;
    }
    toast.error(roomCloseError);
  }, [roomCloseError]);

  useEffect(() => {
    if (!completeRoomReviewError) {
      return;
    }
    toast.error(completeRoomReviewError);
  }, [completeRoomReviewError]);

  useEffect(() => {
    if (typeof roomReviewValue !== 'string') {
      return;
    }
    const requestTimeout = setTimeout(() => {
      if (!id) {
        throw new Error('Room id not found');
      }
      fetchUpsertRoomReview({
        roomId: id,
        review: roomReviewValue,
      });
    }, upsertRoomReviewDebounceMs);

    return () => {
      clearTimeout(requestTimeout);
    };
  }, [roomReviewValue, id, fetchUpsertRoomReview]);

  const handleCloseRoom = () => {
    if (!id) {
      throw new Error('Room id not found');
    }
    fetchRoomClose(id);
  };

  const handleCompleteReview = () => {
    if (!id) {
      throw new Error('Room id not found');
    }
    fetchCompleteRoomReview({
      roomId: id,
    });
  };

  const handleReviewChange = (event: ChangeEvent<HTMLTextAreaElement>) => {
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

  const handleDetailsOpen = (questionId: string) => {
    setOpenedAnswerDetailsId(questionId);
  };

  const handleDetailsClose = () => {
    setOpenedAnswerDetailsId(null);
  };

  if (shouldRedirectToAnalytics) {
    return <Navigate to={generatePath(pathnames.roomAnalytics, { id })} replace />
  }

  if (loading || !room || myQuestionEvaluationsLoading || myRoomReviewLoading) {
    return (
      <>
      <PageHeader title={localizationCaptions[LocalizationKey.RoomReviewPageName]} />
      <Loader />
      </>
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
        <InfoBlock className='text-left flex flex-col'>
          <Typography size='s' bold>{localizationCaptions[LocalizationKey.CandidateOpinion]}</Typography>
          <Gap sizeRem={1} />
          <Textarea
            className='h-3.625'
            maxLength={roomReviewMaxLength}
            showMaxLength={true}
            value={roomReviewValue ?? myRoomReview?.review ?? ''}
            onInput={handleReviewChange}
          />
          {upsertedRoomReview && (
            <Typography size='s'>
              <Icon name={IconNames.CheckmarkDone} />
              {localizationCaptions[LocalizationKey.Saved]}
            </Typography>
          )}
          {upsertRoomReviewLoading && (<Loader />)}
          {upsertRoomReviewError && (
            <Typography size='s' error>{localizationCaptions[LocalizationKey.Error]}: {upsertRoomReviewError}</Typography>
          )}
        </InfoBlock>
        <Gap sizeRem={0.5} />
        <InfoBlock className='text-left'>
          <Typography size='xl' bold>{localizationCaptions[LocalizationKey.CandidateMarks]}</Typography>
          <Gap sizeRem={2} />
          {myQuestionEvaluations && myQuestionEvaluations.sort(sortMyRoomReviews).map((questionEvaluations, index, allMyQuestionEvaluations) => (
            <Fragment key={questionEvaluations.id}>
              <RoomReviewQuestionEvaluation
                roomId={room.id}
                questionEvaluations={questionEvaluations}
                onDetailsOpen={() => handleDetailsOpen(questionEvaluations.id)}
              />
              {index !== allMyQuestionEvaluations.length - 1 && (<Gap sizeRem={0.25} />)}
            </Fragment>
          ))}
        </InfoBlock>
        <Gap sizeRem={1.75} />
        <Modal
          open={!!openedAnswerDetailsId}
          contentLabel={localizationCaptions[LocalizationKey.QuestionAnswerDetails]}
          onClose={handleDetailsClose}
        >
          <QuestionAnswerDetails
            roomId={room.id}
            questionId={openedAnswerDetailsId || ''}
            questionTitle={myQuestionEvaluations?.find(e => e.id === openedAnswerDetailsId)?.value || ''}
            allUsers={allUsers}
          />
          <ModalFooter>
            <Button onClick={handleDetailsClose}>
              {localizationCaptions[LocalizationKey.Close]}
            </Button>
          </ModalFooter>
        </Modal>
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
            <Button onClick={handleCompleteReview} variant='active'>
              {completeRoomReviewLoading ? (
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
          <Button
            variant='active'
            onClick={handleOpenSaveModal}
          >
            {completeRoomReviewLoading ? (
              <Loader />
            ) : (
              localizationCaptions[LocalizationKey.RoomReviewSave]
            )}
          </Button>
        </div>
        <Gap sizeRem={0.75} />
      </div>
    </>
  );
};
