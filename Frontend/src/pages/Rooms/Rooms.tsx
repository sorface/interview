import React, { FunctionComponent, useCallback, useContext, useEffect, useState } from 'react';
import { Link, generatePath } from 'react-router-dom';
import { GetRoomPageParams, roomsApiDeclaration } from '../../apiDeclarations';
import { Field } from '../../components/FieldsBlock/Field';
import { MainContentWrapper } from '../../components/MainContentWrapper/MainContentWrapper';
import { Paginator } from '../../components/Paginator/Paginator';
import { pathnames } from '../../constants';
import { AuthContext } from '../../context/AuthContext';
import { useApiMethod } from '../../hooks/useApiMethod';
import { Room, RoomStatus } from '../../types/room';
import { checkAdmin } from '../../utils/checkAdmin';
import { ProcessWrapper, skeletonTransitionMs } from '../../components/ProcessWrapper/ProcessWrapper';
import { TagsView } from '../../components/TagsView/TagsView';
import { RoomsSearch } from '../../components/RoomsSearch/RoomsSearch';
import { ButtonLink } from '../../components/ButtonLink/ButtonLink';
import { HeaderField } from '../../components/HeaderField/HeaderField';
import { RoomsFilter } from '../../components/RoomsFilter/RoomsFilter';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../localization';

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
  const [loadersCount, setLoadersCount] = useState(0);
  const loaders = Array.from({ length: loadersCount || 1 }, () => ({ height: '4rem' }));
  const roomsSafe = rooms || [];

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
    if (loadersCount || !roomsSafe.length || loading) {
      return;
    }
    const loadersCountTimeout = setTimeout(() => {
      setLoadersCount(roomsSafe.length);
    }, skeletonTransitionMs);

    return () => {
      clearTimeout(loadersCountTimeout);
    };

  }, [roomsSafe.length, loading, loadersCount]);

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

  const handlePrevPage = useCallback(() => {
    setPageNumber(pageNumber - 1);
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
      <HeaderField>
        <RoomsSearch
          searchValue={searchValueInput}
          onSearchChange={setSearchValueInput}
        />
      </HeaderField>
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
        loading={loading}
        error={error}
        loaders={loaders}
      >
        <>
          <ul className="rooms-list">
            {(rooms && !rooms.length) ? (
              <Field>
                <div className="rooms-list-no-data">{localizationCaptions[LocalizationKey.NoRecords]}</div>
              </Field>
            ) : (
              roomsSafe.map(createRoomItem)
            )}
          </ul>
        </>
      </ProcessWrapper>
      <Paginator
        pageNumber={pageNumber}
        prevDisabled={loading || (pageNumber === initialPageNumber)}
        nextDisabled={loading || (roomsSafe.length !== pageSize)}
        onPrevClick={handlePrevPage}
        onNextClick={handleNextPage}
      />
    </MainContentWrapper>
  );
};
