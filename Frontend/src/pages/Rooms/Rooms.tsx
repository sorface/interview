import React, { FunctionComponent, useCallback, useContext, useEffect, useState } from 'react';
import { Link, generatePath } from 'react-router-dom';
import { GetRoomPageParams, roomsApiDeclaration } from '../../apiDeclarations';
import { Field } from '../../components/FieldsBlock/Field';
import { MainContentWrapper } from '../../components/MainContentWrapper/MainContentWrapper';
import { pathnames } from '../../constants';
import { AuthContext } from '../../context/AuthContext';
import { useApiMethod } from '../../hooks/useApiMethod';
import { Room, RoomStatus } from '../../types/room';
import { checkAdmin } from '../../utils/checkAdmin';
import { ProcessWrapper } from '../../components/ProcessWrapper/ProcessWrapper';
import { TagsView } from '../../components/TagsView/TagsView';
import { RoomsSearch } from '../../components/RoomsSearch/RoomsSearch';
import { ButtonLink } from '../../components/ButtonLink/ButtonLink';
import { RoomsFilter } from '../../components/RoomsFilter/RoomsFilter';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../localization';
import { ItemsGrid } from '../../components/ItemsGrid/ItemsGrid';

import './Rooms.css';

const pageSize = 10;
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
      <Field key={room.id}>
        <li>
          <div className='room-item'>
            <div className='room-link'>
              <Link to={roomLink} >
                {room.name}
              </Link>
              <div className='room-status'>
                {roomStatusCaption[room.roomStatus]}
              </div>
            </div>
            <div className="room-tags">
              <TagsView
                placeHolder={localizationCaptions[LocalizationKey.NoTags]}
                tags={room.tags}
              />
            </div>
            <div className='room-action-links'>
              <Link
                to={roomLink}
                className='room-join-link'
              >
                {localizationCaptions[LocalizationKey.Join]}
              </Link>
              {admin && (
                <Link
                  to={`${pathnames.roomsParticipants.replace(':id', room.id)}`}
                  className='room-edit-participants-link'
                >
                  {localizationCaptions[LocalizationKey.EditParticipants]}
                </Link>
              )}
            </div>
          </div>
        </li>
      </Field>
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
          renderItem={createRoomItem}
          nextPageAvailable={rooms?.length === pageSize}
          handleNextPage={handleNextPage}
        />
      </ProcessWrapper>
    </MainContentWrapper>
  );
};
