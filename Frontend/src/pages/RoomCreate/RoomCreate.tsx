import React, {
  ChangeEvent,
  Fragment,
  FunctionComponent,
  useCallback,
  useEffect,
  useState,
} from 'react';
import { useNavigate } from 'react-router-dom';
import {
  CreateRoomBody,
  GetPageQuestionsTreeResponse,
  GetQuestionsTreesParams,
  questionTreeApiDeclaration,
  RoomEditBody,
  RoomIdParam,
  roomInviteApiDeclaration,
  roomsApiDeclaration,
} from '../../apiDeclarations';
import { Field } from '../../components/FieldsBlock/Field';
import { Loader } from '../../components/Loader/Loader';
import { IconNames } from '../../constants';
import { useApiMethod } from '../../hooks/useApiMethod';
import { LocalizationKey } from '../../localization';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import {
  Room,
  RoomAccessType,
  RoomInvite,
  RoomQuestionListItem,
} from '../../types/room';
import { ModalFooter } from '../../components/ModalFooter/ModalFooter';
import { Gap } from '../../components/Gap/Gap';
import { Typography } from '../../components/Typography/Typography';
import { Icon } from '../Room/components/Icon/Icon';
import { RoomInvitations } from '../../components/RoomInvitations/RoomInvitations';
import { RoomCreateField } from './RoomCreateField/RoomCreateField';
import { ModalWithProgressWarning } from '../../components/ModalWithProgressWarning/ModalWithProgressWarning';
import { Button } from '../../components/Button/Button';
import { padTime } from '../../utils/padTime';
import { Question } from '../../types/question';
import { QuestionItem } from '../../components/QuestionItem/QuestionItem';
import { SwitcherButton } from '../../components/SwitcherButton/SwitcherButton';
import { DragNDropList } from '../../components/DragNDropList/DragNDropList';
import { RoomQuestionsSelector } from './RoomQuestionsSelector/RoomQuestionsSelector';
import { sortRoomQuestion } from '../../utils/sortRoomQestions';
import { TreeViewer } from '../../components/TreeViewer/TreeViewer';
import { QuestionsTree } from '../../types/questionsTree';
import { mapInvitesForAiRoom } from '../../utils/mapInvitesForAiRoom';

const nameFieldName = 'roomName';
const dateFieldName = 'roomDate';
const startTimeFieldName = 'roomStartTime';
const endTimeFieldName = 'roomEndTime';

const roomStartTimeShiftMinutes = 15;

const formatDate = (value: Date) => {
  const month = padTime(value.getMonth() + 1);
  const date = padTime(value.getDate());
  return `${value.getFullYear()}-${month}-${date}`;
};

const formatTime = (value: Date) => {
  const hours = padTime(value.getHours());
  const minutes = padTime(value.getMinutes());
  return `${hours}:${minutes}`;
};

const parseScheduledStartTime = (
  scheduledStartTime: string,
  durationSec?: number,
) => {
  const parsed = new Date(scheduledStartTime);
  const parsedDuration = new Date(scheduledStartTime);
  if (durationSec) {
    parsedDuration.setSeconds(parsedDuration.getSeconds() + durationSec);
  }
  return {
    date: formatDate(parsed),
    startTime: formatTime(parsed),
    ...(durationSec && { endTime: formatTime(parsedDuration) }),
  };
};

const getRoomDuration = (roomFields: RoomFields) => {
  const roomEndTime = roomFields.endTime.split(':');
  const roomDateEnd = new Date(roomFields.scheduleStartTime);
  if (roomEndTime.length > 1) {
    roomDateEnd.setHours(parseInt(roomEndTime[0]));
    roomDateEnd.setMinutes(parseInt(roomEndTime[1]));
  }
  if (roomDateEnd < roomFields.scheduleStartTime) {
    roomDateEnd.setDate(roomDateEnd.getDate() + 1);
  }
  const duration =
    (roomDateEnd.getTime() - roomFields.scheduleStartTime.getTime()) / 1000;
  return duration;
};

const formatDuration = (durationSec: number) => {
  const hours = Math.floor(durationSec / (60 * 60));
  const minutes = Math.floor((durationSec / 60) % 60);
  return `${padTime(hours)}:${padTime(minutes)}`;
};

const getInitialRoomStartDate = () => {
  const currDate = new Date();
  currDate.setMinutes(currDate.getMinutes() + 15);
  return currDate;
};

const getAiRoomEndDate = () => {
  const currDate = new Date();
  currDate.setHours(currDate.getHours() + 1);
  return currDate;
};

enum CreationStep {
  Step1,
  Step2,
}

type RoomFields = {
  name: string;
  questionTreeId: string;
  scheduleStartTime: Date;
  endTime: string;
};

interface RoomCreateProps {
  aiRoom: boolean;
  editRoomId: string | null;
  open: boolean;
  onClose: () => void;
}

export const RoomCreate: FunctionComponent<RoomCreateProps> = ({
  aiRoom,
  editRoomId,
  open,
  onClose,
}) => {
  const localizationCaptions = useLocalizationCaptions();
  const navigate = useNavigate();
  const { apiMethodState, fetchData } = useApiMethod<Room, CreateRoomBody>(
    roomsApiDeclaration.create,
  );
  const {
    process: { loading, error },
    data: createdRoom,
  } = apiMethodState;

  const { apiMethodState: apiRoomMethodState, fetchData: fetchRoom } =
    useApiMethod<Room, Room['id']>(roomsApiDeclaration.getById);
  const {
    process: { loading: loadingRoom, error: errorRoom },
    data: room,
  } = apiRoomMethodState;

  const { apiMethodState: apiRoomEditMethodState, fetchData: fetchRoomEdit } =
    useApiMethod<Room, RoomEditBody>(roomsApiDeclaration.edit);
  const {
    process: { loading: loadingRoomEdit, error: errorRoomEdit },
    data: editedRoom,
  } = apiRoomEditMethodState;

  const { apiMethodState: apiRoomInvitesState, fetchData: fetchRoomInvites } =
    useApiMethod<RoomInvite[], RoomIdParam>(roomInviteApiDeclaration.get);
  const {
    process: { loading: roomInvitesLoading, error: roomInvitesError },
    data: roomInvitesData,
  } = apiRoomInvitesState;

  const { apiMethodState: questionTreesState, fetchData: fetchQuestionTrees } =
    useApiMethod<GetPageQuestionsTreeResponse, GetQuestionsTreesParams>(
      questionTreeApiDeclaration.getPage,
    );
  const {
    process: { loading: questionTreesLoading, error: questionTreesError },
    data: questionTrees,
  } = questionTreesState;

  const { apiMethodState: questionTreeGetState, fetchData: fetchQuestionTree } =
    useApiMethod<QuestionsTree, string>(questionTreeApiDeclaration.get);
  const {
    process: { loading: questionTreeLoading, error: questionTreeError },
    data: questionTree,
  } = questionTreeGetState;

  const [roomFields, setRoomFields] = useState<RoomFields>({
    name: '',
    questionTreeId: '',
    scheduleStartTime: getInitialRoomStartDate(),
    endTime: aiRoom ? formatTime(getAiRoomEndDate()) : '',
  });
  const roomDuration = getRoomDuration(roomFields);
  const [selectedQuestions, setSelectedQuestions] = useState<
    RoomQuestionListItem[]
  >([]);
  const [creationStep, setCreationStep] = useState<CreationStep>(
    CreationStep.Step1,
  );
  const [questionsView, setQuestionsView] = useState(false);
  const [displayTreeViewer, setDisplayTreeViewer] = useState(false);
  const [uiError, setUiError] = useState('');
  const modalTitle = editRoomId
    ? localizationCaptions[LocalizationKey.EditRoom]
    : localizationCaptions[LocalizationKey.NewRoom];

  const totalLoading = loading || loadingRoom || loadingRoomEdit;
  const totalError = error || errorRoom || errorRoomEdit || uiError;

  useEffect(() => {
    fetchQuestionTrees({
      name: '',
      PageNumber: 1,
      PageSize: 30,
    });
  }, [fetchQuestionTrees]);

  useEffect(() => {
    if (!editRoomId) {
      return;
    }
    fetchRoom(editRoomId);
  }, [editRoomId, fetchRoom]);

  useEffect(() => {
    if (!room) {
      return;
    }
    const parsedScheduledStartTime =
      room.scheduledStartTime &&
      parseScheduledStartTime(room.scheduledStartTime, room.timer?.durationSec);
    setRoomFields((rf) => ({
      ...rf,
      ...room,
      date: parsedScheduledStartTime ? parsedScheduledStartTime.date : '',
      startTime: parsedScheduledStartTime
        ? parsedScheduledStartTime.startTime
        : '',
      endTime: parsedScheduledStartTime
        ? parsedScheduledStartTime.endTime || ''
        : '',
    }));
    setSelectedQuestions(room.questions.sort(sortRoomQuestion));
  }, [room]);

  useEffect(() => {
    if (!createdRoom && !editedRoom) {
      return;
    }
    setCreationStep(CreationStep.Step2);
  }, [createdRoom, editedRoom, localizationCaptions, navigate]);

  useEffect(() => {
    if (!createdRoom && !editedRoom) {
      return;
    }
    fetchRoomInvites({
      roomId: createdRoom?.id || editedRoom?.id || '',
    });
  }, [createdRoom, editedRoom, fetchRoomInvites]);

  useEffect(() => {
    if (!roomFields.questionTreeId) {
      return;
    }
    fetchQuestionTree(roomFields.questionTreeId);
  }, [roomFields.questionTreeId, fetchQuestionTree]);

  useEffect(() => {
    if (questionTreeLoading) {
      setDisplayTreeViewer(false);
      return;
    }
    if (questionTree) {
      setDisplayTreeViewer(true);
      return;
    }
  }, [questionTreeLoading, questionTree]);

  const getUiError = () => {
    if (aiRoom && !roomFields.questionTreeId) {
      return localizationCaptions[LocalizationKey.EmptyRoomQuestionTreeError];
    }
    if (!aiRoom && selectedQuestions.length === 0) {
      return localizationCaptions[LocalizationKey.RoomEmptyQuestionsListError];
    }
    if (!roomFields.name) {
      return localizationCaptions[LocalizationKey.EmptyRoomNameError];
    }
    if (!roomFields.scheduleStartTime) {
      return localizationCaptions[LocalizationKey.RoomEmptyStartTimeError];
    }
    if (
      roomFields.scheduleStartTime.getTime() <
      Date.now() - 1000 * 60 * roomStartTimeShiftMinutes
    ) {
      return localizationCaptions[
        LocalizationKey.RoomStartTimeMustBeGreaterError
      ];
    }
    return '';
  };

  const validateRoomFields = () => {
    const newUiError = getUiError();
    setUiError(newUiError);
    if (newUiError) {
      return false;
    }
    return true;
  };

  const handleCreateRoom = () => {
    if (!validateRoomFields()) {
      return;
    }
    if (editRoomId) {
      fetchRoomEdit({
        id: editRoomId,
        name: roomFields.name,
        ...(aiRoom
          ? {
              questionTreeId: roomFields.questionTreeId,
            }
          : {
              questions: selectedQuestions.map((question, index) => ({
                ...question,
                order: index,
              })),
            }),
        scheduleStartTime: roomFields.scheduleStartTime.toISOString(),
        durationSec: roomDuration,
      });
    } else {
      fetchData({
        name: roomFields.name,
        ...(aiRoom
          ? {
              questionTreeId: roomFields.questionTreeId,
            }
          : {
              questions: selectedQuestions.map((question, index) => ({
                id: question.id,
                order: index,
              })),
            }),
        experts: [],
        examinees: [],
        tags: [],
        accessType: RoomAccessType.Private,
        scheduleStartTime: roomFields.scheduleStartTime.toISOString(),
        duration: roomDuration,
      });
    }
  };

  const handleChangeStartDate = (
    e: ChangeEvent<HTMLInputElement | HTMLSelectElement>,
  ) => {
    const selectedDate = new Date(e.target.value);
    selectedDate.setHours(roomFields.scheduleStartTime.getHours());
    selectedDate.setMinutes(roomFields.scheduleStartTime.getMinutes());
    setRoomFields({
      ...roomFields,
      scheduleStartTime: selectedDate,
    });
  };

  const handleChangeStartTime = (
    e: ChangeEvent<HTMLInputElement | HTMLSelectElement>,
  ) => {
    const [selectedHours, selectedMinutes] = e.target.value.split(':');
    console.log(selectedHours, selectedMinutes);
    const selectedDate = new Date(roomFields.scheduleStartTime);
    selectedDate.setHours(+selectedHours);
    selectedDate.setMinutes(+selectedMinutes);
    setRoomFields({
      ...roomFields,
      scheduleStartTime: selectedDate,
    });
  };

  const handleChangeField =
    (fieldName: keyof RoomFields) =>
    (e: ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
      setRoomFields({
        ...roomFields,
        [fieldName]: e.target.value,
      });
    };

  const handleChangeQuestionTreeField = (
    e: ChangeEvent<HTMLInputElement | HTMLSelectElement>,
  ) => {
    const [id, name] = e.target.value.split('|');
    setRoomFields({
      ...roomFields,
      questionTreeId: id,
      name: name,
    });
  };

  const handleQuestionsSave = (questions: RoomQuestionListItem[]) => {
    setSelectedQuestions(questions);
    setQuestionsView(false);
  };

  const handleQuestionRemove = (question: Question) => {
    const newSelectedQuestions = selectedQuestions.filter(
      (q) => q.id !== question.id,
    );
    setSelectedQuestions(newSelectedQuestions);
  };

  const handleQuestionsViewOpen = () => {
    setQuestionsView(true);
  };

  const handleQuestionsViewClose = () => {
    setQuestionsView(false);
  };

  const renderStatus = useCallback(() => {
    if (totalError) {
      return (
        <>
          <Typography size="m" error>
            <div className="flex">
              <Icon name={IconNames.Information} />
              <Gap sizeRem={0.25} horizontal />
              {totalError}
            </div>
          </Typography>
          <Gap sizeRem={1.25} />
        </>
      );
    }
    if (totalLoading) {
      return (
        <Field>
          <Loader />
        </Field>
      );
    }
    return <></>;
  }, [totalError, totalLoading]);

  const renderQuestionItem = (
    question: RoomQuestionListItem,
    index: number,
  ) => (
    <Fragment>
      <div className="flex w-full">
        <div className="flex flex-col w-1.75 text-right text-grey2">
          <Gap sizeRem={1} />
          <Typography size="xxl" bold>
            {index + 1}
          </Typography>
        </div>
        <Gap sizeRem={1} horizontal />
        <div className="flex-1">
          <QuestionItem question={question} onRemove={handleQuestionRemove} />
        </div>
      </div>
      {index !== selectedQuestions.length - 1 && <Gap sizeRem={0.25} />}
    </Fragment>
  );

  const stepView = {
    [CreationStep.Step1]: (
      <>
        {!aiRoom && (
          <>
            <RoomCreateField.Wrapper>
              <RoomCreateField.Label>
                <label htmlFor="roomName">
                  <Typography size="m" bold>
                    {localizationCaptions[LocalizationKey.RoomName]}
                  </Typography>
                </label>
              </RoomCreateField.Label>
              <RoomCreateField.Content className="flex flex-1">
                <input
                  id="roomName"
                  name={nameFieldName}
                  value={roomFields.name}
                  onChange={handleChangeField('name')}
                  className="flex-1"
                  type="text"
                  required
                />
              </RoomCreateField.Content>
            </RoomCreateField.Wrapper>
            <div className="px-1">
              <Typography size="s">
                {localizationCaptions[LocalizationKey.RoomNamePrompt]}
              </Typography>
            </div>
          </>
        )}
        {!aiRoom && (
          <>
            <Gap sizeRem={1.5} />
            <RoomCreateField.Wrapper>
              <RoomCreateField.Label>
                <label htmlFor="roomDate">
                  <Typography size="m" bold>
                    {localizationCaptions[LocalizationKey.RoomDateAndTime]}
                  </Typography>
                </label>
              </RoomCreateField.Label>
              <RoomCreateField.Content className="flex items-center">
                <input
                  id="roomDate"
                  name={dateFieldName}
                  value={formatDate(roomFields.scheduleStartTime)}
                  type="date"
                  required
                  className="mr-0.5"
                  onChange={handleChangeStartDate}
                />
                <input
                  id="roomTimeStart"
                  name={startTimeFieldName}
                  value={formatTime(roomFields.scheduleStartTime)}
                  type="time"
                  required
                  className="mr-0.5"
                  onChange={handleChangeStartTime}
                />
                <span className="mr-0.5">
                  <Typography size="s">-</Typography>
                </span>
                <input
                  id="roomTimeEnd"
                  name={endTimeFieldName}
                  value={roomFields.endTime}
                  type="time"
                  onChange={handleChangeField('endTime')}
                />
                <Gap sizeRem={1.5} horizontal />
                {!!roomFields.endTime && (
                  <Typography size="m">
                    {localizationCaptions[LocalizationKey.RoomDuration]}:{' '}
                    {formatDuration(roomDuration)}
                  </Typography>
                )}
              </RoomCreateField.Content>
            </RoomCreateField.Wrapper>
            <Gap sizeRem={2} />
          </>
        )}

        {!aiRoom && (
          <RoomCreateField.Wrapper>
            <RoomCreateField.Label className="self-start">
              <Typography size="m" bold>
                {localizationCaptions[LocalizationKey.RoomQuestions]}
              </Typography>
            </RoomCreateField.Label>
            <RoomCreateField.Content>
              <Gap sizeRem={0.5} />
              <DragNDropList
                items={selectedQuestions}
                renderItem={renderQuestionItem}
                onItemsChange={setSelectedQuestions}
              />
              {!!selectedQuestions.length && <Gap sizeRem={1.5} />}
              <Button variant="active2" onClick={handleQuestionsViewOpen}>
                <Icon size="s" name={IconNames.Add} />
                <Gap sizeRem={0.5} horizontal />
                {localizationCaptions[LocalizationKey.AddRoomQuestions]}
              </Button>
            </RoomCreateField.Content>
          </RoomCreateField.Wrapper>
        )}

        {aiRoom && (
          <RoomCreateField.Wrapper className="w-full">
            <div className="">
              <label htmlFor="questionTree">
                <Typography size="m" bold>
                  {localizationCaptions[LocalizationKey.QuestionTree]}
                </Typography>
              </label>
              <Gap sizeRem={0.5} />
            </div>
            <RoomCreateField.Content>
              {questionTreesError && (
                <Typography error size="m">
                  {questionTreesError}
                </Typography>
              )}
              {questionTreesLoading ? (
                <Loader />
              ) : (
                <div>
                  <select
                    id="questionTree"
                    className="w-full"
                    defaultValue={roomFields.questionTreeId}
                    onChange={handleChangeQuestionTreeField}
                  >
                    <option value="">
                      {localizationCaptions[LocalizationKey.NotSelected]}
                    </option>
                    {questionTrees?.data.map((questionTree) => (
                      <option
                        key={questionTree.id}
                        value={`${questionTree.id}|${questionTree.name}`}
                      >
                        {questionTree.name}
                      </option>
                    ))}
                  </select>
                  {questionTreeError && (
                    <Typography error size="m">
                      {questionTreeError}
                    </Typography>
                  )}
                  {displayTreeViewer && questionTree && (
                    <>
                      <Gap sizeRem={0.75} />
                      <div style={{ width: '100%', height: '500px' }}>
                        <TreeViewer tree={questionTree.tree} />
                      </div>
                    </>
                  )}
                </div>
              )}
            </RoomCreateField.Content>
          </RoomCreateField.Wrapper>
        )}

        <ModalFooter>
          <Button onClick={onClose}>
            {localizationCaptions[LocalizationKey.Cancel]}
          </Button>
          <Button variant="active" onClick={handleCreateRoom}>
            {localizationCaptions[LocalizationKey.Continue]}
          </Button>
        </ModalFooter>
      </>
    ),
    [CreationStep.Step2]: (
      <>
        <RoomInvitations
          roomId={createdRoom?.id || editedRoom?.id || ''}
          roomInvitesData={
            !aiRoom
              ? roomInvitesData
              : mapInvitesForAiRoom(roomInvitesData || [])
          }
          roomInvitesError={roomInvitesError}
          roomInvitesLoading={roomInvitesLoading}
        />
        <ModalFooter>
          <Button variant="active" onClick={onClose}>
            {localizationCaptions[LocalizationKey.Continue]}
          </Button>
        </ModalFooter>
      </>
    ),
  };

  return (
    <ModalWithProgressWarning
      warningCaption={
        localizationCaptions[LocalizationKey.CurrentRoomNotBeSaved]
      }
      contentLabel={modalTitle}
      open={open}
      wide
      onClose={onClose}
    >
      <div className="text-left">
        {!aiRoom && !questionsView && (
          <>
            <SwitcherButton
              items={[
                {
                  id: 1,
                  content:
                    localizationCaptions[LocalizationKey.CreateRoomStep1],
                },
                {
                  id: 2,
                  content:
                    localizationCaptions[LocalizationKey.CreateRoomStep2],
                },
              ]}
              activeIndex={creationStep}
              activeVariant="invertedActive"
              nonActiveVariant="inverted"
            />
            <Gap sizeRem={2.25} />
          </>
        )}
        {renderStatus()}
        {questionsView ? (
          <RoomQuestionsSelector
            preSelected={selectedQuestions}
            onCancel={handleQuestionsViewClose}
            onSave={handleQuestionsSave}
          />
        ) : (
          stepView[creationStep]
        )}
      </div>
    </ModalWithProgressWarning>
  );
};
