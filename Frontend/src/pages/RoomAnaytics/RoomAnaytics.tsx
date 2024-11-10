import { Fragment, FunctionComponent, useContext, useEffect, useState } from 'react';
import toast from 'react-hot-toast';
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
import { RoomAnayticsDetails } from './components/RoomAnayticsDetails/RoomAnayticsDetails';
import { UserAvatar } from '../../components/UserAvatar/UserAvatar';
import { HttpResponseCode, IconNames } from '../../constants';
import { AuthContext } from '../../context/AuthContext';
import { Button } from '../../components/Button/Button';
import { ModalWarningContent } from '../../components/ModalWarningContent/ModalWarningContent';
import { ModalFooter } from '../../components/ModalFooter/ModalFooter';

const updateDataTimeoutMs = 10000;

const createFakeQuestion = (roomQuestion: RoomQuestion): Question => ({
  ...roomQuestion,
  tags: [],
  answers: [],
  codeEditor: null,
  category: {
    id: '',
    name: '',
    parentId: '',
  },
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
  const auth = useContext(AuthContext);
  const localizationCaptions = useLocalizationCaptions();
  const { id } = useParams();
  const [openedQuestionDetails, setOpenedQuestionDetails] = useState('');
  const [closeModalOpen, setCloseModalOpen] = useState(false);
  const [loadedData, setLoadedData] = useState<Analytics | null>(null);

  const { apiMethodState, fetchData } = useApiMethod<Analytics, Room['id']>(roomsApiDeclaration.analytics);
  const { data, process: { loading, error, code } } = apiMethodState;

  const {
    apiMethodState: roomApiMethodState,
    fetchData: fetchRoom,
  } = useApiMethod<Room, Room['id']>(roomsApiDeclaration.getById);
  const {
    process: { loading: roomLoading, error: roomError },
    data: room,
  } = roomApiMethodState;

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

  const averageMarkOrNull = loadedData?.averageMark ?? null;

  const viewNotAllowed = code === HttpResponseCode.Forbidden;

  const totalError = (!viewNotAllowed && error) || roomError;

  const allUsers = loadedData ? getAllUsers(loadedData) : new Map<User['id'], AnalyticsUserReview>();

  const examinees = room?.participants.filter(
    participant => participant.type === 'Examinee'
  );

  const openedQuestionDetailsTitle = data?.questions.find(question => question.id === openedQuestionDetails)?.value;

  useEffect(() => {
    if (!id) {
      throw new Error('Room id not found');
    }
    fetchData(id);
    fetchRoom(id);
  }, [id, fetchData, fetchRoom]);

  useEffect(() => {
    if (roomCloseCode !== HttpResponseCode.Ok) {
      return;
    }
    handleCloseCloseModal();
    toast.success(localizationCaptions[LocalizationKey.Saved]);
  }, [roomCloseCode, localizationCaptions]);

  useEffect(() => {
    if (!roomCloseError) {
      return;
    }
    toast.error(roomCloseError);
  }, [roomCloseError]);

  useEffect(() => {
    if (!id) {
      throw new Error('Room id not found');
    }
    if (!loadedData || loadedData.completed) {
      return;
    }
    const timeout = setTimeout(() => {
      fetchData(id);
    }, updateDataTimeoutMs);

    return () => {
      clearTimeout(timeout);
    };
  }, [id, loadedData, fetchData]);

  useEffect(() => {
    if (!data) {
      return;
    }
    setLoadedData(data);
  }, [data]);

  const handleQuestionClick = (question: Question) => {
    setOpenedQuestionDetails(question.id);
  };

  const handleQuestionDetailsClose = () => {
    setOpenedQuestionDetails('');
  };

  const handleOpenCloseModal = () => {
    setCloseModalOpen(true);
  };

  const handleCloseCloseModal = () => {
    setCloseModalOpen(false);
  };

  const handleCloseRoom = () => {
    if (!id) {
      throw new Error('Room id not found');
    }
    fetchRoomClose(id);
  };

  return (
    <>
      <Modal
        open={!!openedQuestionDetails}
        contentLabel={`${localizationCaptions[LocalizationKey.QuestionAnswerDetails]} "${openedQuestionDetailsTitle}"`}
        onClose={handleQuestionDetailsClose}
      >
        <RoomAnayticsDetails
          allUsers={allUsers}
          data={loadedData}
          openedQuestionDetails={openedQuestionDetails}
          roomId={room?.id}
        />
        <Gap sizeRem={1} />
      </Modal>

      <PageHeader title={`${localizationCaptions[LocalizationKey.RoomReviewPageName]} ${room?.name}`} />
      <Gap sizeRem={1} />
      {totalError && (
        <div className='text-left'>
          <Typography size='m' error>{localizationCaptions[LocalizationKey.Error]}: {totalError}</Typography>
        </div>
      )}
      {roomLoading ? (
        <InfoBlock className='h-9.375 flex items-center justify-center'>
          <Loader />
        </InfoBlock>
      ) : (
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
                conent={
                  examinees?.length ? (
                    <div className='flex items-center'>
                      {examinees.map(examinee => (
                        <Fragment>
                          <Typography size='xs'>
                            <div className='flex'>
                              <UserAvatar nickname={examinee.nickname} size='xxs' src={examinee.avatar} />
                            </div>
                          </Typography>
                          <Gap sizeRem={0.25} horizontal />
                          <span className='capitalize'>{examinee.nickname}</span>
                          <Gap sizeRem={1} horizontal />
                        </Fragment>
                      ))}
                    </div>
                  ) :
                    localizationCaptions[LocalizationKey.NotFound]
                }
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
          {!viewNotAllowed && (
            <>
              <Gap sizeRem={0.5} horizontal />
              <InfoBlock className='px-5 flex flex-col items-center'>
                <Typography size='m' bold>
                  {localizationCaptions[LocalizationKey.AverageCandidateMark]}
                </Typography>
                <Gap sizeRem={1} />
                <CircularProgress
                  value={averageMarkOrNull ? averageMarkOrNull * 10 : null}
                  caption={averageMarkOrNull ? averageMarkOrNull.toFixed(1) : null}
                  size='m'
                />
              </InfoBlock>
            </>
          )}
        </div>
      )}
      <Gap sizeRem={0.5} />
      {viewNotAllowed && (
        <InfoBlock className='text-left'>
          <Typography size='m' bold>{localizationCaptions[LocalizationKey.NotEnoughRights]}</Typography>
        </InfoBlock>
      )}
      {!viewNotAllowed && (
        <>
          <InfoBlock className='text-left'>
            {(!loadedData && loading) ? (
              <div className='h-9.375 flex items-center justify-center'>
                <Loader />
              </div>
            ) : (
              <>
                <Typography size='xl' bold>
                  {localizationCaptions[LocalizationKey.OpinionsAndMarks]}
                  <Gap sizeRem={2} />
                  <ReviewUserGrid>
                    {loadedData?.userReview
                      .filter(userReview => allUsers.get(userReview.userId)?.participantType === 'Expert')
                      .filter(userReview => data?.completed ? typeof userReview.averageMark === 'number' : true)
                      .map((userReview) => (
                        <ReviewUserOpinion
                          key={userReview.userId}
                          user={generateUserOpinion(userReview)}
                          allUsers={allUsers}
                        />
                      ))}
                  </ReviewUserGrid>
                </Typography>
              </>
            )}
          </InfoBlock>
          <Gap sizeRem={0.5} />
          <InfoBlock className='text-left'>
            {(!loadedData && loading) ? (
              <div className='h-4 flex items-center justify-center'>
                <Loader />
              </div>
            ) : (
              <>
                <Typography size='xl' bold>
                  {localizationCaptions[LocalizationKey.MarksForQuestions]}
                </Typography>
                <Gap sizeRem={2} />
                {loadedData?.questions.map((question, index, questions) => {
                  return (
                    <Fragment key={question.id}>
                      <QuestionItem
                        question={createFakeQuestion(question)}
                        mark={question.averageMark ?? null}
                        onClick={handleQuestionClick}
                      />
                      {index !== questions.length - 1 && (<Gap sizeRem={0.25} />)}
                    </Fragment>
                  );
                })}
              </>
            )}
          </InfoBlock>
        </>
      )}
      {(loadedData?.completed === false && room && room?.owner.id === auth?.id) && (
        <>
          <Gap sizeRem={1.75} />
          <div className='flex justify-end'>
            <Button
              onClick={handleOpenCloseModal}
            >
              {localizationCaptions[LocalizationKey.CloseRoomWithoutReview]}
            </Button>
            <Gap sizeRem={1} horizontal />
          </div>
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
        </>
      )}
    </>
  );
};
