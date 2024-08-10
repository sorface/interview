import React, { FunctionComponent, useCallback, useContext, useEffect, useState, MouseEvent } from 'react';
import { Link } from 'react-router-dom';
import { GetRoomPageParams, roomsApiDeclaration } from '../../apiDeclarations';
import { IconNames } from '../../constants';
import { AuthContext } from '../../context/AuthContext';
import { useApiMethod } from '../../hooks/useApiMethod';
import { Room, RoomStatus } from '../../types/room';
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

import './Rooms.css';

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

export enum RoomsPageMode {
  Home,
  Current,
  Closed,
}

interface RoomsProps {
  mode: RoomsPageMode;
}

export const Rooms: FunctionComponent<RoomsProps> = ({
  mode,
}) => {
  const auth = useContext(AuthContext);
  const localizationCaptions = useLocalizationCaptions();
  const [pageNumber, setPageNumber] = useState(initialPageNumber);
  const { apiMethodState, fetchData } = useApiMethod<Room[], GetRoomPageParams>(roomsApiDeclaration.getPage);
  const { process: { loading, error }, data: rooms } = apiMethodState;
  const { apiMethodState: roomsHistoryApiMethodState, fetchData: fetchRoomsHistory } = useApiMethod<Room[], GetRoomPageParams>(roomsApiDeclaration.getPage);
  const { process: { loading: loadingRoomsHistory, error: errorRoomsHistory }, data: roomsHistory } = roomsHistoryApiMethodState;
  const [searchValueInput, setSearchValueInput] = useState('');
  const [searchValue, setSearchValue] = useState('');
  const closed = mode === RoomsPageMode.Closed;
  const [createEditModalOpened, setCreateEditModalOpened] = useState(false);
  const [editingRoomId, setEditingRoomId] = useState<Room['id'] | null>(null);
  const [roomsUpdateTrigger, setRoomsUpdateTrigger] = useState(0);
  const [monthStartDate, setMonthStartDate] = useState(initialMonthStartDate);
  const currentDate = new Date(now.getFullYear(), now.getMonth(), now.getDate());
  const roomDates = rooms ? rooms.map(room => new Date(room.scheduledStartTime)) : [];

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
    const statuses: RoomStatus[] = closed ? ['Close'] : ['New', 'Active', 'Review'];
    fetchData({
      PageSize: pageSize,
      PageNumber: pageNumber,
      Name: searchValue,
      Participants: [auth?.id || ''],
      Statuses: statuses,
    });
  }, [pageNumber, searchValue, auth?.id, closed, fetchData]);

  useEffect(() => {
    updateRooms();
  }, [updateRooms, roomsUpdateTrigger]);

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

  const handleOpenCreateModal = () => {
    setCreateEditModalOpened(true);
  }

  const handleOpenEditModal = (roomId: Room['id']) => (e: MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setEditingRoomId(roomId);
    setCreateEditModalOpened(true);
  }

  const handleCloseCreateEditModal = () => {
    setCreateEditModalOpened(false);
    setEditingRoomId(null);
    setRoomsUpdateTrigger((t) => t + 1);
  }

  const handleMonthBackClick = () => {
    setMonthStartDate(addMonthsToDate(monthStartDate, -1));
  };

  const handleMonthForwardClick = () => {
    setMonthStartDate(addMonthsToDate(monthStartDate, 1));
  };

  const createRoomItem = (room: Room) => {
    const roomStatusCaption: Record<Room['roomStatus'], string> = {
      New: localizationCaptions[LocalizationKey.RoomStatusNew],
      Active: localizationCaptions[LocalizationKey.RoomStatusActive],
      Review: localizationCaptions[LocalizationKey.RoomStatusReview],
      Close: localizationCaptions[LocalizationKey.RoomStatusClose],
    };
    const tagStates: Record<Room['roomStatus'], TagState> = {
      New: TagState.Waiting,
      Active: TagState.Pending,
      Review: TagState.WaitingForAction,
      Close: TagState.Closed,
    };

    const roomLink = getRoomLink(room);
    const expertInRoom = !!room.participants.find(
      roomParticipant => roomParticipant.type === 'Expert' && roomParticipant.id === auth?.id
    );

    return (
      <div key={room.id} className='room-item-wrapper'>
        <li>
          <Link to={roomLink}>
            <div className='room-item'>
              <div className='room-status-wrapper'>
                <Tag state={tagStates[room.roomStatus]}>
                  {roomStatusCaption[room.roomStatus]}
                </Tag>
                <Gap sizeRem={1.5} />
                <div className='room-action-links'>
                  {expertInRoom && (
                    <>
                      <div
                        className='room-edit-participants-link rotate-90'
                        onClick={handleOpenEditModal(room.id)}
                      >
                        <Icon name={IconNames.Options} />
                      </div>
                    </>
                  )}
                </div>
              </div>
              <div className='room-name'>
                {room.name}
              </div>
              {room.scheduledStartTime && (
                <>
                  <Gap sizeRem={0.75} />
                  <RoomDateAndTime
                    typographySize='s'
                    scheduledStartTime={room.scheduledStartTime}
                    timer={room.timer}
                  />
                </>
              )}
              <Gap sizeRem={1.75} />
              <RoomParticipants participants={room.participants} />
            </div>
          </Link>
        </li>
      </div>
    );
  };

  return (
    <>
      <PageHeader
        title={getPageTitle()}
        searchValue={searchValueInput}
        onSearchChange={setSearchValueInput}
      >
        <Button variant='active' className='h-2.5' onClick={handleOpenCreateModal}>
          <Icon name={IconNames.Add} />
          {localizationCaptions[LocalizationKey.CreateRoom]}
        </Button>
      </PageHeader>
      <div className='rooms-page flex-1 overflow-auto'>
        {createEditModalOpened && (
          <RoomCreate
            editRoomId={editingRoomId || null}
            open={createEditModalOpened}
            onClose={handleCloseCreateEditModal} />
        )}
        <div className='flex overflow-auto h-full'>
          <div className='flex-1 overflow-auto h-full'>
            <ItemsGrid
              currentData={rooms}
              loading={loading}
              error={error}
              triggerResetAccumData={`${roomsUpdateTrigger}${searchValue}${mode}${closed}`}
              loaderClassName='room-item-wrapper room-item-loader'
              renderItem={createRoomItem}
              nextPageAvailable={false}
              handleNextPage={handleNextPage}
            />
          </div>
          {mode === RoomsPageMode.Home && (
            <div className='flex overflow-auto'>
              <Gap sizeRem={1} horizontal />
              <div className='flex flex-col overflow-auto'>
                <Calendar
                  monthStartDate={monthStartDate}
                  currentDate={currentDate}
                  filledItems={roomDates}
                  onMonthBackClick={handleMonthBackClick}
                  onMonthForwardClick={handleMonthForwardClick}
                />
                <Gap sizeRem={0.5} />
                {!!errorRoomsHistory && (
                  <Typography size='m' error>
                    <div className='flex items-center'>
                      <Icon name={IconNames.Information} />
                      <Gap sizeRem={0.25} horizontal />
                      <div>
                        {localizationCaptions[LocalizationKey.Error]}: {errorRoomsHistory}
                      </div>
                    </div>
                  </Typography>
                )}
                {!!(!roomsHistory || loadingRoomsHistory) ? (
                  <div className='flex justify-center items-center w-full h-full bg-wrap rounded-1.125'>
                    <Loader />
                  </div>
                ) : (
                  <RoomsHistory
                    rooms={roomsHistory}
                  />
                )}
              </div>
            </div>
          )}
        </div>
      </div>
    </>
  );
};
