import React, { FunctionComponent, useCallback, useContext, useEffect, useState } from 'react';
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
import { RoomsSearch } from '../../components/RoomsSearch/RoomsSearch';
import { ButtonLink } from '../../components/ButtonLink/ButtonLink';
import { RoomsFilter } from '../../components/RoomsFilter/RoomsFilter';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../localization';
import { ItemsGrid } from '../../components/ItemsGrid/ItemsGrid';
import { ThemedIcon } from '../Room/components/ThemedIcon/ThemedIcon';
import { UserAvatar } from '../../components/UserAvatar/UserAvatar';

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

  useEffect(() => {
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

  const createRoomItem = useCallback((room: Room) => {
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
                    <Link
                      to={`${pathnames.roomsParticipants.replace(':id', room.id)}`}
                      className='room-edit-participants-link'
                    >
                      <ThemedIcon name={IconNames.Settings} />
                    </Link>
                  )}
                </div>
              </div>
              <div className='room-name'>
                {room.name}
              </div>
              <div className='room-participants'>
                {room.users.map(roomParticipant => (
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
  }, [admin, localizationCaptions]);

  return (
    <MainContentWrapper className='rooms-page'>
      <Field>
        <RoomsSearch
          searchValue={searchValueInput}
          onSearchChange={setSearchValueInput}
        />
      </Field>
      <Field>
        <div className='room-actions'>
          <RoomsFilter
            participating={participating}
            closed={closed}
            onParticipatingChange={setParticipating}
            onClosedChange={setClosed}
          />
          <ButtonLink
            path={pathnames.roomsCreate}
            caption="+"
          />
        </div>
      </Field>
      <ProcessWrapper
        loading={false}
        error={error}
      >
        <ItemsGrid
          currentData={rooms || []}
          loading={loading}
          loaderClassName='room-item-wrapper room-item-loader'
          renderItem={createRoomItem}
          nextPageAvailable={rooms?.length === pageSize}
          handleNextPage={handleNextPage}
        />
      </ProcessWrapper>
    </MainContentWrapper>
  );
};
