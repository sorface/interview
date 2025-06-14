import React, {
  FunctionComponent,
  useCallback,
  useContext,
  useEffect,
  useState,
  MouseEvent,
} from 'react';
import { Link } from 'react-router-dom';
import {
  GetRoomCalendarParams,
  GetRoomPageParams,
  roomsApiDeclaration,
} from '../../apiDeclarations';
import { aiExpertNickname, IconNames } from '../../constants';
import { AuthContext } from '../../context/AuthContext';
import { useApiMethod } from '../../hooks/useApiMethod';
import {
  Room,
  RoomCalendarItem,
  RoomParticipant,
  RoomStatus,
  RoomWtithType,
} from '../../types/room';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../localization';
import { ItemsGrid } from '../../components/ItemsGrid/ItemsGrid';
import { Icon } from '../Room/components/Icon/Icon';
import { RoomCreate } from '../RoomCreate/RoomCreate';
import { PageHeader } from '../../components/PageHeader/PageHeader';
import { Button } from '../../components/Button/Button';
import { Gap } from '../../components/Gap/Gap';
import { Tag, TagState } from '../../components/Tag/Tag';
import { RoomDateAndTime } from '../../components/RoomDateAndTime/RoomDateAndTime';
import { RoomParticipants } from '../../components/RoomParticipants/RoomParticipants';
import { Calendar } from '../../components/Calendar/Calendar';
import { RoomsHistory } from '../../components/RoomsHistory/RoomsHistory';
import { getRoomLink } from '../../utils/getRoomLink';
import { Loader } from '../../components/Loader/Loader';
import { Typography } from '../../components/Typography/Typography';
import { useThemedAiAvatar } from '../../hooks/useThemedAiAvatar';
import {
  SwitcherButton,
  SwitcherButtonContent,
} from '../../components/SwitcherButton/SwitcherButton';
import { RoomsView, useSavedRoomsView } from '../../hooks/useSavedRoomsView';
import { useThemeClassName } from '../../hooks/useThemeClassName';
import { Theme } from '../../context/ThemeContext';

import './Rooms.css';

const aiParticipant: RoomParticipant = {
  id: 'fakeId',
  nickname: aiExpertNickname,
  userType: 'Expert',
  roles: ['User'],
  roomId: 'roomId',
  twitchIdentity: 'twitchIdentity',
  type: 'Expert',
  userId: 'userId',
  avatar: '',
};

const pageSize = 30;
const initialPageNumber = 1;
const searchDebounceMs = 300;
const now = new Date();
const initialMonthStartDate = new Date(now.getFullYear(), now.getMonth());

const addMonthsToDate = (date: Date, count: number) => {
  const newDate = new Date(date);
  newDate.setMonth(date.getMonth() + count);
  return newDate;
};

const getDayEndValue = (date: Date) => {
  const result = new Date(date);
  result.setHours(23);
  result.setMinutes(59);
  result.setSeconds(59);
  return result;
};

const getMonthEndDate = (startMonthDate: Date) => {
  const endDate = new Date(
    startMonthDate.getFullYear(),
    startMonthDate.getMonth() + 1,
    0,
    23,
    59,
    59,
  );
  return endDate;
};

export enum RoomsPageMode {
  Home,
  Current,
  Closed,
}

interface RoomsProps {
  mode: RoomsPageMode;
}

export const Rooms: FunctionComponent<RoomsProps> = ({ mode }) => {
  const auth = useContext(AuthContext);
  const localizationCaptions = useLocalizationCaptions();
  const themedAiAvatar = useThemedAiAvatar();
  const { view, setView } = useSavedRoomsView();
  const [pageNumber, setPageNumber] = useState(initialPageNumber);
  const { apiMethodState, fetchData } = useApiMethod<
    RoomWtithType[],
    GetRoomPageParams
  >(roomsApiDeclaration.getPage);
  const {
    process: { loading, error },
    data: rooms,
  } = apiMethodState;
  const {
    apiMethodState: roomsHistoryApiMethodState,
    fetchData: fetchRoomsHistory,
  } = useApiMethod<Room[], GetRoomPageParams>(roomsApiDeclaration.getPage);
  const {
    process: { loading: loadingRoomsHistory, error: errorRoomsHistory },
    data: roomsHistory,
  } = roomsHistoryApiMethodState;
  const {
    apiMethodState: roomsCalendarApiMethodState,
    fetchData: fetchRoomsCalendar,
  } = useApiMethod<RoomCalendarItem[], GetRoomCalendarParams>(
    roomsApiDeclaration.calendar,
  );
  const {
    process: { loading: loadingRoomsCalendar, error: errorRoomsCalendar },
    data: roomsCalendar,
  } = roomsCalendarApiMethodState;
  const [searchValueInput, setSearchValueInput] = useState('');
  const [searchValue, setSearchValue] = useState('');
  const closed = mode === RoomsPageMode.Closed;
  const [createEditModalOpened, setCreateEditModalOpened] = useState(false);
  const [editingRoomId, setEditingRoomId] = useState<Room['id'] | null>(null);
  const [roomsUpdateTrigger, setRoomsUpdateTrigger] = useState(0);
  const [monthStartDate, setMonthStartDate] = useState(initialMonthStartDate);
  const currentDate = new Date(
    now.getFullYear(),
    now.getMonth(),
    now.getDate(),
  );
  const roomDates = roomsCalendar
    ? roomsCalendar.map(
        (roomCalendar) => new Date(roomCalendar.minScheduledStartTime),
      )
    : [];
  const [selectedDay, setSelectedDay] = useState<Date | null>(null);
  const triggerResetAccumData = `${roomsUpdateTrigger}${searchValue}${mode}${selectedDay}`;
  const roomItemThemedClassName = useThemeClassName({
    [Theme.Dark]: 'hover:bg-dark-history-hover',
    [Theme.Light]:
      'hover:bg-blue-light hover:border hover:border-solid border-blue-main',
  });

  const getPageTitle = () => {
    switch (mode) {
      case RoomsPageMode.Home:
        return localizationCaptions[LocalizationKey.HighlightsRoomsPageName];
      case RoomsPageMode.Current:
        return localizationCaptions[LocalizationKey.CurrentRoomsPageName];
      case RoomsPageMode.Closed:
        return localizationCaptions[LocalizationKey.ClosedRoomsPageName];
      default:
        return '';
    }
  };

  const updateRooms = useCallback(() => {
    const statuses: RoomStatus[] = closed
      ? ['Close']
      : ['New', 'Active', 'Review'];
    const startValue = selectedDay ? selectedDay.toISOString() : null;
    const endValue = selectedDay
      ? getDayEndValue(selectedDay).toISOString()
      : null;
    fetchData({
      PageSize: pageSize,
      PageNumber: pageNumber,
      Name: searchValue,
      Participants: [auth?.id || ''],
      Statuses: statuses,
      ...(startValue && { StartValue: startValue }),
      ...(endValue && { EndValue: endValue }),
    });
  }, [pageNumber, searchValue, auth?.id, closed, selectedDay, fetchData]);

  useEffect(() => {
    updateRooms();
  }, [updateRooms, roomsUpdateTrigger, mode]);

  useEffect(() => {
    if (!monthStartDate || mode !== RoomsPageMode.Home) {
      return;
    }
    const monthEndDate = getMonthEndDate(monthStartDate);
    fetchRoomsCalendar({
      RoomStatus: ['New', 'Active', 'Review'],
      TimeZoneOffset: new Date().getTimezoneOffset() * -1,
      StartDateTime: monthStartDate.toISOString(),
      EndDateTime: monthEndDate.toISOString(),
    });
  }, [roomsUpdateTrigger, mode, monthStartDate, fetchRoomsCalendar]);

  useEffect(() => {
    setPageNumber(initialPageNumber);
  }, [triggerResetAccumData]);

  useEffect(() => {
    if (mode !== RoomsPageMode.Home) {
      return;
    }
    fetchRoomsHistory({
      PageSize: pageSize,
      PageNumber: 1,
      Name: '',
      Participants: [auth?.id || ''],
      Statuses: ['Close'],
      dateSort: 'Desc',
    });
  }, [mode, auth?.id, fetchRoomsHistory]);

  useEffect(() => {
    const searchTimeout = setTimeout(() => {
      setSearchValue(searchValueInput);
    }, searchDebounceMs);

    return () => {
      clearTimeout(searchTimeout);
    };
  }, [searchValueInput]);

  const handleNextPage = useCallback(() => {
    setPageNumber(pageNumber + 1);
  }, [pageNumber]);

  const handleOpenCreateModalClassic = () => {
    setCreateEditModalOpened(true);
  };

  const handleOpenEditModal = (roomId: Room['id']) => (e: MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setEditingRoomId(roomId);
    setCreateEditModalOpened(true);
  };

  const handleCloseCreateEditModal = () => {
    setCreateEditModalOpened(false);
    setEditingRoomId(null);
    setRoomsUpdateTrigger((t) => t + 1);
  };

  const handleMonthBackClick = () => {
    setMonthStartDate(addMonthsToDate(monthStartDate, -1));
  };

  const handleMonthForwardClick = () => {
    setMonthStartDate(addMonthsToDate(monthStartDate, 1));
  };

  const handleDayClick = (day: Date) => {
    if (selectedDay?.valueOf() === day.valueOf()) {
      setSelectedDay(null);
      return;
    }
    setSelectedDay(day);
  };

  const createRoomItem = (room: RoomWtithType) => {
    const roomStatusCaption: Record<Room['status'], string> = {
      New: localizationCaptions[LocalizationKey.RoomStatusNew],
      Active: localizationCaptions[LocalizationKey.RoomStatusActive],
      Review: localizationCaptions[LocalizationKey.RoomStatusReview],
      Close: localizationCaptions[LocalizationKey.RoomStatusClose],
      Expire: localizationCaptions[LocalizationKey.RoomStatusExpire],
    };
    const tagStates: Record<Room['status'], TagState> = {
      New: TagState.Waiting,
      Active: TagState.Pending,
      Review: TagState.WaitingForAction,
      Close: TagState.Closed,
      Expire: TagState.Closed,
    };

    const roomLink = getRoomLink(room);
    const expertInRoom = !!room.participants.find(
      (roomParticipant) =>
        roomParticipant.type === 'Expert' && roomParticipant.id === auth?.id,
    );
    const canEditInStatus = room.status === 'New' || room.status === 'Active';
    const aiRoom = room.type === 'AI';

    if (view === RoomsView.List) {
      return (
        <div
          key={room.id}
          className={`room-item-wrapper ${roomItemThemedClassName}`}
        >
          <li>
            <Link to={roomLink}>
              <div className="room-item">
                <div className="room-name">{room.name}</div>
                {room.scheduledStartTime && (
                  <RoomDateAndTime
                    typographySize="s"
                    scheduledStartTime={room.scheduledStartTime}
                    timer={room.timer}
                    col
                    mini
                  />
                )}
                <div className="room-status-wrapper w-fit">
                  <Tag state={tagStates[room.status]}>
                    {roomStatusCaption[room.status]}
                  </Tag>
                </div>
                <RoomParticipants
                  participants={[
                    ...room.participants,
                    ...(room.type === 'AI'
                      ? [
                          {
                            ...aiParticipant,
                            avatar: themedAiAvatar,
                          },
                        ]
                      : []),
                  ]}
                />
                <div className="room-action-links">
                  {!aiRoom && expertInRoom && canEditInStatus && (
                    <>
                      <div
                        className="rotate-90"
                        onClick={handleOpenEditModal(room.id)}
                      >
                        <Icon
                          size="s"
                          secondary
                          hover
                          name={IconNames.Options}
                        />
                      </div>
                    </>
                  )}
                </div>
              </div>
            </Link>
          </li>
        </div>
      );
    }

    return (
      <div
        key={room.id}
        className={`room-item-wrapper ${roomItemThemedClassName}`}
      >
        <li>
          <Link to={roomLink}>
            <div className="room-item">
              <div className="room-status-wrapper">
                <Tag state={tagStates[room.status]}>
                  {roomStatusCaption[room.status]}
                </Tag>
                <Gap sizeRem={1.5} />
                <div className="room-action-links">
                  {!aiRoom && expertInRoom && canEditInStatus && (
                    <>
                      <div
                        className="rotate-90"
                        onClick={handleOpenEditModal(room.id)}
                      >
                        <Icon
                          size="s"
                          secondary
                          hover
                          name={IconNames.Options}
                        />
                      </div>
                    </>
                  )}
                </div>
              </div>
              <div className="room-name">{room.name}</div>
              {room.scheduledStartTime && (
                <>
                  <Gap sizeRem={0.75} />
                  <RoomDateAndTime
                    typographySize="s"
                    scheduledStartTime={room.scheduledStartTime}
                    timer={room.timer}
                  />
                </>
              )}
              <Gap sizeRem={1.75} />
              <RoomParticipants
                participants={[
                  ...room.participants,
                  ...(room.type === 'AI'
                    ? [
                        {
                          ...aiParticipant,
                          avatar: themedAiAvatar,
                        },
                      ]
                    : []),
                ]}
              />
            </div>
          </Link>
        </li>
      </div>
    );
  };

  const headerActionSwitcherItems: [
    SwitcherButtonContent,
    SwitcherButtonContent,
  ] = [
    {
      id: RoomsView.Grid,
      content: <Icon name={IconNames.Grid} />,
    },
    {
      id: RoomsView.List,
      content: <Icon name={IconNames.Menu} />,
    },
  ];

  const headerActionItems = (
    <div className="flex">
      <SwitcherButton
        items={headerActionSwitcherItems}
        activeIndex={view === RoomsView.Grid ? 0 : 1}
        activeVariant="active2"
        nonActiveVariant="invertedAlternative"
        mini
        onClick={(activeIndex) =>
          setView(activeIndex === 0 ? RoomsView.Grid : RoomsView.List)
        }
      />
      <Gap sizeRem={2} horizontal />
    </div>
  );

  return (
    <>
      <PageHeader
        title={getPageTitle()}
        notifications
        searchValue={searchValueInput}
        actionItem={headerActionItems}
        onSearchChange={setSearchValueInput}
      >
        <Button
          variant="active"
          className="h-[2.5rem]"
          aria-hidden
          onClick={handleOpenCreateModalClassic}
        >
          <Icon name={IconNames.Add} />
          {localizationCaptions[LocalizationKey.CreateRoom]}
        </Button>
      </PageHeader>
      <div className="rooms-page flex-1 overflow-auto">
        {createEditModalOpened && (
          <RoomCreate
            aiRoom={false}
            editRoomId={editingRoomId || null}
            open={createEditModalOpened}
            onClose={handleCloseCreateEditModal}
          />
        )}
        <div className="flex overflow-auto h-full">
          <div
            className={`flex-1 overflow-auto h-full ${view === RoomsView.Grid ? 'grid-view' : 'list-view'}`}
          >
            <ItemsGrid
              currentData={rooms}
              loading={loading}
              error={error}
              triggerResetAccumData={triggerResetAccumData}
              loaderClassName="room-item-wrapper room-item-loader"
              renderItem={createRoomItem}
              nextPageAvailable={false}
              handleNextPage={handleNextPage}
            />
            {!!(
              !loading &&
              mode !== RoomsPageMode.Closed &&
              rooms?.length === 0 &&
              pageNumber === 1
            ) && (
              <>
                <Gap sizeRem={2.25} />
                <div className="flex justify-center">
                  <Button
                    className="h-[2.5rem] text-grey3"
                    aria-hidden
                    onClick={handleOpenCreateModalClassic}
                  >
                    <Icon name={IconNames.Add} />
                    {localizationCaptions[LocalizationKey.CreateRoom]}
                  </Button>
                </div>
              </>
            )}
          </div>
          {mode === RoomsPageMode.Home && (
            <div className="flex overflow-auto">
              <Gap sizeRem={1} horizontal />
              <div className="w-[17.5rem] flex flex-col overflow-auto">
                {!!errorRoomsCalendar && (
                  <Typography size="m" error>
                    <div className="text-left flex items-center">
                      <Icon name={IconNames.Information} />
                      <Gap sizeRem={0.25} horizontal />
                      <div>
                        {localizationCaptions[LocalizationKey.Error]}:{' '}
                        {errorRoomsCalendar}
                      </div>
                    </div>
                  </Typography>
                )}
                <Calendar
                  loading={loadingRoomsCalendar}
                  monthStartDate={monthStartDate}
                  currentDate={currentDate}
                  filledItems={roomDates}
                  selectedDay={selectedDay}
                  onMonthBackClick={handleMonthBackClick}
                  onMonthForwardClick={handleMonthForwardClick}
                  onDayClick={handleDayClick}
                />
                <Gap sizeRem={0.5} />
                {!!errorRoomsHistory && (
                  <Typography size="m" error>
                    <div className="flex items-center">
                      <Icon name={IconNames.Information} />
                      <Gap sizeRem={0.25} horizontal />
                      <div>
                        {localizationCaptions[LocalizationKey.Error]}:{' '}
                        {errorRoomsHistory}
                      </div>
                    </div>
                  </Typography>
                )}
                {!roomsHistory || loadingRoomsHistory ? (
                  <div className="flex justify-center items-center w-full h-full bg-wrap rounded-[1.125rem]">
                    <Loader />
                  </div>
                ) : (
                  <RoomsHistory rooms={roomsHistory} />
                )}
              </div>
            </div>
          )}
        </div>
      </div>
    </>
  );
};
