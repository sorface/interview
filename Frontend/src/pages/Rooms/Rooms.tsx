import React, { FunctionComponent, useCallback, useContext, useEffect, useState, MouseEvent } from 'react';
import { Link, generatePath } from 'react-router-dom';
import { GetRoomPageParams, roomsApiDeclaration } from '../../apiDeclarations';
import { MainContentWrapper } from '../../components/MainContentWrapper/MainContentWrapper';
import { IconNames, pathnames } from '../../constants';
import { AuthContext } from '../../context/AuthContext';
import { useApiMethod } from '../../hooks/useApiMethod';
import { Room, RoomStatus } from '../../types/room';
import { checkAdmin } from '../../utils/checkAdmin';
import { ProcessWrapper } from '../../components/ProcessWrapper/ProcessWrapper';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../localization';
import { ItemsGrid } from '../../components/ItemsGrid/ItemsGrid';
import { ThemedIcon } from '../Room/components/ThemedIcon/ThemedIcon';
import { RoomCreate } from '../RoomCreate/RoomCreate';
import { PageHeader } from '../../components/PageHeader/PageHeader';
import { Button } from '../../components/Button/Button';
import { Gap } from '../../components/Gap/Gap';
import { Tag, TagState } from '../../components/Tag/Tag';
import { RoomDateAndTime } from '../../components/RoomDateAndTime/RoomDateAndTime';
import { RoomParticipants } from '../../components/RoomParticipants/RoomParticipants';

import './Rooms.css';

const pageSize = 30;
const initialPageNumber = 1;
const searchDebounceMs = 300;

export enum RoomsPageMode {
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
  const admin = checkAdmin(auth);
  const localizationCaptions = useLocalizationCaptions();
  const [pageNumber, setPageNumber] = useState(initialPageNumber);
  const { apiMethodState, fetchData } = useApiMethod<Room[], GetRoomPageParams>(roomsApiDeclaration.getPage);
  const { process: { loading, error }, data: rooms } = apiMethodState;
  const [searchValueInput, setSearchValueInput] = useState('');
  const [searchValue, setSearchValue] = useState('');
  const closed = mode === RoomsPageMode.Closed;
  const [createEditModalOpened, setCreateEditModalOpened] = useState(false);
  const [editingRoomId, setEditingRoomId] = useState<Room['id'] | null>(null);
  const [roomsUpdateTrigger, setRoomsUpdateTrigger] = useState(0);
  const pageTitle = mode === RoomsPageMode.Current ?
    localizationCaptions[LocalizationKey.CurrentRoomsPageName] :
    localizationCaptions[LocalizationKey.ClosedRoomsPageName];

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

    const roomSummary =
      room.roomStatus === 'Review' ||
      room.roomStatus === 'Close';
    const roomLink = roomSummary ?
      generatePath(pathnames.roomReview, { id: room.id }) :
      generatePath(pathnames.room, { id: room.id });

    return (
      <div key={room.id} className='room-item-wrapper'>
        <li>
          <Link to={roomLink} >
            <div className='room-item'>
              <div className='room-status-wrapper'>
                <Tag state={tagStates[room.roomStatus]}>
                  {roomStatusCaption[room.roomStatus]}
                </Tag>
                <Gap sizeRem={1.5} />
                <div className='room-action-links'>
                  {admin && (
                    <>
                      <div
                        className='room-edit-participants-link rotate-90'
                        onClick={handleOpenEditModal(room.id)}
                      >
                        <ThemedIcon name={IconNames.Options} />
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
        title={pageTitle}
        searchValue={searchValueInput}
        onSearchChange={setSearchValueInput}
      >
        <Button variant='active' className='h-2.5' onClick={handleOpenCreateModal}>
          <ThemedIcon name={IconNames.Add} />
          {localizationCaptions[LocalizationKey.CreateRoom]}
        </Button>
      </PageHeader>
      <MainContentWrapper className='rooms-page'>
        {createEditModalOpened && (
          <RoomCreate
            editRoomId={editingRoomId || null}
            open={createEditModalOpened}
            onClose={handleCloseCreateEditModal} />
        )}
        <ProcessWrapper
          loading={false}
          error={error}
        >
          <ItemsGrid
            currentData={rooms}
            loading={loading}
            triggerResetAccumData={`${roomsUpdateTrigger}${searchValue}${mode}${closed}`}
            loaderClassName='room-item-wrapper room-item-loader'
            renderItem={createRoomItem}
            nextPageAvailable={rooms?.length === pageSize}
            handleNextPage={handleNextPage}
          />
        </ProcessWrapper>
      </MainContentWrapper>
    </>
  );
};
