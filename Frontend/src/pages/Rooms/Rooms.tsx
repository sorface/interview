import React, { FunctionComponent, useCallback, useContext, useEffect, useState, MouseEvent } from 'react';
import { Link, generatePath } from 'react-router-dom';
import { GetRoomPageParams, roomsApiDeclaration } from '../../apiDeclarations';
import { Field } from '../../components/FieldsBlock/Field';
import { MainContentWrapper } from '../../components/MainContentWrapper/MainContentWrapper';
import { IconNames, pathnames } from '../../constants';
import { AuthContext } from '../../context/AuthContext';
import { useApiMethod } from '../../hooks/useApiMethod';
import { Room, RoomStatus } from '../../types/room';
import { checkAdmin } from '../../utils/checkAdmin';
import { ProcessWrapper } from '../../components/ProcessWrapper/ProcessWrapper';
import { RoomsFilter } from '../../components/RoomsFilter/RoomsFilter';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../localization';
import { ItemsGrid } from '../../components/ItemsGrid/ItemsGrid';
import { ThemedIcon } from '../Room/components/ThemedIcon/ThemedIcon';
import { UserAvatar } from '../../components/UserAvatar/UserAvatar';
import { RoomCreate } from '../RoomCreate/RoomCreate';

import './Rooms.css';

const pageSize = 30;
const initialPageNumber = 1;
const searchDebounceMs = 300;

export const Rooms: FunctionComponent = () => {
  const auth = useContext(AuthContext);
  const admin = checkAdmin(auth);
  const localizationCaptions = useLocalizationCaptions();
  const [pageNumber, setPageNumber] = useState(initialPageNumber);
  const { apiMethodState, fetchData } = useApiMethod<Room[], GetRoomPageParams>(roomsApiDeclaration.getPage);
  const { process: { loading, error }, data: rooms } = apiMethodState;
  const [searchValueInput, setSearchValueInput] = useState('');
  const [searchValue, setSearchValue] = useState('');
  const [participating, setParticipating] = useState(false);
  const [closed, setClosed] = useState(false);
  const [createEditModalOpened, setCreateEditModalOpened] = useState(false);
  const [editingRoomId, setEditingRoomId] = useState<Room['id'] | null>(null);
  const [roomsUpdateTrigger, setRoomsUpdateTrigger] = useState(0);

  const updateRooms = useCallback(() => {
    const participants = (auth?.id && participating) ? [auth?.id] : [];
    const statuses: RoomStatus[] = closed ? ['Close'] : ['New', 'Active', 'Review'];
    fetchData({
      PageSize: pageSize,
      PageNumber: pageNumber,
      Name: searchValue,
      Participants: participants,
      Statuses: statuses,
    });
  }, [pageNumber, searchValue, auth?.id, participating, closed, fetchData]);

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

    const roomSummary =
      room.roomStatus === 'Review' ||
      room.roomStatus === 'Close';
    const roomLink = roomSummary ?
      generatePath(pathnames.roomAnalyticsSummary, { id: room.id }) :
      generatePath(pathnames.room, { id: room.id });

    return (
      <div key={room.id} className='room-item-wrapper'>
        <li>
          <Link to={roomLink} >
            <div className='room-item'>
              <div className='room-status-wrapper'>
                <div className='room-status'>
                  {roomStatusCaption[room.roomStatus]}
                </div>
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
              <div className='room-participants'>
                {room.participants.map(roomParticipant => (
                  <div className='room-participant'>
                    {roomParticipant.avatar &&
                      <UserAvatar
                        src={roomParticipant.avatar}
                        nickname={roomParticipant.nickname}
                      />
                    }
                  </div>
                ))}
              </div>
            </div>
          </Link>
        </li>
      </div>
    );
  };

  return (
    <MainContentWrapper className='rooms-page'>
      {createEditModalOpened && (
        <RoomCreate
          editRoomId={editingRoomId || null}
          open={createEditModalOpened}
          onClose={handleCloseCreateEditModal} />
      )}
      <Field>
        <div className='room-actions items-center'>
          <RoomsFilter
            participating={participating}
            closed={closed}
            searchValue={searchValueInput}
            onSearchChange={setSearchValueInput}
            onParticipatingChange={setParticipating}
            onClosedChange={setClosed}
          />
          {/* <RoomsSearch
          searchValue={searchValueInput}
          onSearchChange={setSearchValueInput}
        /> */}
          <button className='active' onClick={handleOpenCreateModal}>
            <ThemedIcon name={IconNames.Add} />
            {localizationCaptions[LocalizationKey.CreateRoom]}
          </button>
        </div>
      </Field>
      <ProcessWrapper
        loading={false}
        error={error}
      >
        <ItemsGrid
          currentData={rooms}
          loading={loading}
          triggerResetAccumData={`${roomsUpdateTrigger}${searchValue}${participating}${closed}`}
          loaderClassName='room-item-wrapper room-item-loader'
          renderItem={createRoomItem}
          nextPageAvailable={rooms?.length === pageSize}
          handleNextPage={handleNextPage}
        />
      </ProcessWrapper>
    </MainContentWrapper>
  );
};
