import React, {
  ChangeEvent,
  FunctionComponent,
  useCallback,
  useEffect,
  useState,
} from 'react';
import { useNavigate } from 'react-router-dom';
import {
  categoriesApiDeclaration,
  CreateRoomBody,
  GetCategoriesParams,
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
import { Category } from '../../types/category';

const aiRoom = true;
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

const parseRoomDate = (roomFields: RoomFields) => {
  const roomDateStart = new Date(roomFields.date);
  const roomStartTime = roomFields.startTime.split(':');
  roomDateStart.setHours(parseInt(roomStartTime[0]));
  roomDateStart.setMinutes(parseInt(roomStartTime[1]));
  const roomEndTime = roomFields.endTime.split(':');
  const roomDateEnd = new Date(roomDateStart);
  if (roomEndTime.length > 1) {
    roomDateEnd.setHours(parseInt(roomEndTime[0]));
    roomDateEnd.setMinutes(parseInt(roomEndTime[1]));
  }
  if (roomDateEnd < roomDateStart) {
    roomDateEnd.setDate(roomDateEnd.getDate() + 1);
  }
  const duration = (roomDateEnd.getTime() - roomDateStart.getTime()) / 1000;
  return { roomDateStart, duration };
};

const formatDuration = (durationSec: number) => {
  const hours = Math.floor(durationSec / (60 * 60));
  const minutes = Math.floor((durationSec / 60) % 60);
  return `${padTime(hours)}:${padTime(minutes)}`;
};

const getNextHourDate = () => {
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
  categoryId: string;
  date: string;
  startTime: string;
  endTime: string;
};

interface RoomCreateProps {
  editRoomId: string | null;
  open: boolean;
  onClose: () => void;
}

export const RoomCreate: FunctionComponent<RoomCreateProps> = ({
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

  const {
    apiMethodState: rootCategoriesState,
    fetchData: fetchRootCategories,
  } = useApiMethod<Category[], GetCategoriesParams>(
    categoriesApiDeclaration.getPage,
  );
  const {
    process: { loading: rootCategoriesLoading, error: rootCategoriesError },
    data: rootCategories,
  } = rootCategoriesState;

  const [roomFields, setRoomFields] = useState<RoomFields>({
    name: '',
    categoryId: '',
    date: formatDate(new Date()),
    startTime: aiRoom ? formatTime(new Date()) : '',
    endTime: aiRoom ? formatTime(getNextHourDate()) : '',
  });
  const parsedRoomDate = parseRoomDate(roomFields);
  const [creationStep, setCreationStep] = useState<CreationStep>(
    CreationStep.Step1,
  );
  const [uiError, setUiError] = useState('');
  const modalTitle = editRoomId
    ? localizationCaptions[LocalizationKey.EditRoom]
    : localizationCaptions[LocalizationKey.NewRoom];

  const totalLoading = loading || loadingRoom || loadingRoomEdit;
  const totalError = error || errorRoom || errorRoomEdit || uiError;

  useEffect(() => {
    fetchRootCategories({
      name: '',
      PageNumber: 1,
      PageSize: 30,
      showOnlyWithoutParent: true,
    });
  }, [fetchRootCategories]);

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

  const getUiError = () => {
    if (!roomFields.categoryId) {
      return localizationCaptions[LocalizationKey.EmptyRoomCategoryError];
    }
    if (!roomFields.name) {
      return localizationCaptions[LocalizationKey.EmptyRoomNameError];
    }
    if (!roomFields.startTime) {
      return localizationCaptions[LocalizationKey.RoomEmptyStartTimeError];
    }
    if (!roomFields.date) {
      return localizationCaptions[LocalizationKey.RoomEmptyStartTimeError];
    }
    const roomDateStart = new Date(roomFields.date);
    const roomStartTime = roomFields.startTime.split(':');
    roomDateStart.setHours(parseInt(roomStartTime[0]));
    roomDateStart.setMinutes(parseInt(roomStartTime[1]));
    if (
      roomDateStart.getTime() <
      Date.now() - 1000 * 60 * roomStartTimeShiftMinutes
    ) {
      return localizationCaptions[
        LocalizationKey.RoomStartTimeMustBeGreaterError
      ];
    }
    if (!roomFields.categoryId) {
      return localizationCaptions[LocalizationKey.RoomEmptyCategoryError];
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
        categoryId: roomFields.categoryId,
        name: roomFields.name,
        scheduleStartTime: parsedRoomDate.roomDateStart.toISOString(),
        durationSec: parsedRoomDate.duration,
      });
    } else {
      fetchData({
        name: roomFields.name,
        categoryId: roomFields.categoryId,
        experts: [],
        examinees: [],
        tags: [],
        accessType: RoomAccessType.Private,
        scheduleStartTime: parsedRoomDate.roomDateStart.toISOString(),
        duration: parsedRoomDate.duration,
      });
    }
  };

  const handleChangeField =
    (fieldName: keyof RoomFields) => (e: ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
      setRoomFields({
        ...roomFields,
        [fieldName]: e.target.value,
      });
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

  const stepView = {
    [CreationStep.Step1]: (
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
            {localizationCaptions[aiRoom ? LocalizationKey.RoomNamePromptAi : LocalizationKey.RoomNamePrompt]}
          </Typography>
        </div>
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
                  value={roomFields.date}
                  type="date"
                  required
                  className="mr-0.5"
                  onChange={handleChangeField('date')}
                />
                <input
                  id="roomTimeStart"
                  name={startTimeFieldName}
                  value={roomFields.startTime}
                  type="time"
                  required
                  className="mr-0.5"
                  onChange={handleChangeField('startTime')}
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
                    {formatDuration(parsedRoomDate.duration)}
                  </Typography>
                )}
              </RoomCreateField.Content>
            </RoomCreateField.Wrapper>
          </>
        )}
        <Gap sizeRem={2} />

        <RoomCreateField.Wrapper className="w-full max-w-15.75">
          <RoomCreateField.Label>
            <label htmlFor="rootCategory">
              <Typography size="m" bold>
                {localizationCaptions[LocalizationKey.Category]}
              </Typography>
            </label>
          </RoomCreateField.Label>
          <RoomCreateField.Content>
            {rootCategoriesError && (
              <Typography error size='m'>{rootCategoriesError}</Typography>
            )}
            {rootCategoriesLoading ? (
              <Loader />
            ) : (
              <select
                id="rootCategory"
                className="w-full"
                value={roomFields.categoryId}
                onChange={handleChangeField('categoryId')}
              >
                <option value="">
                  {localizationCaptions[LocalizationKey.NotSelected]}
                </option>
                {rootCategories?.map((rootCategory) => (
                  <option key={rootCategory.id} value={rootCategory.id}>
                    {rootCategory.name}
                  </option>
                ))}
              </select>
            )}
          </RoomCreateField.Content>
        </RoomCreateField.Wrapper>
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
          roomInvitesData={roomInvitesData}
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
        {renderStatus()}
        {stepView[creationStep]}
      </div>
    </ModalWithProgressWarning>
  );
};
