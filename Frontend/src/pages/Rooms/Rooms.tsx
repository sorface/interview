import React, { FunctionComponent, useCallback, useContext, useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { GetRoomPageParams, roomsApiDeclaration } from '../../apiDeclarations';
import { Field } from '../../components/FieldsBlock/Field';
import { MainContentWrapper } from '../../components/MainContentWrapper/MainContentWrapper';
import { Paginator } from '../../components/Paginator/Paginator';
import { pathnames } from '../../constants';
import { AuthContext } from '../../context/AuthContext';
import { useApiMethod } from '../../hooks/useApiMethod';
import { Room, RoomStatus } from '../../types/room';
import { checkAdmin } from '../../utils/checkAdmin';
import { ProcessWrapper } from '../../components/ProcessWrapper/ProcessWrapper';
import { TagsView } from '../../components/TagsView/TagsView';
import { RoomsSearch } from '../../components/RoomsSearch/RoomsSearch';
import { Localization } from '../../localization';
import { ButtonLink } from '../../components/ButtonLink/ButtonLink';
import { HeaderField } from '../../components/HeaderField/HeaderField';
import { RoomsFilter } from '../../components/RoomsFilter/RoomsFilter';

import './Rooms.css';

const roomStatusCaption: Record<Room['roomStatus'], string> = {
  New: Localization.RoomStatusNew,
  Active: Localization.RoomStatusActive,
  Review: Localization.RoomStatusReview,
  Close: Localization.RoomStatusClose,
};

const pageSize = 10;
const initialPageNumber = 1;

export const Rooms: FunctionComponent = () => {
  const auth = useContext(AuthContext);
  const admin = checkAdmin(auth);
  const [pageNumber, setPageNumber] = useState(initialPageNumber);
  const { apiMethodState, fetchData } = useApiMethod<Room[], GetRoomPageParams>(roomsApiDeclaration.getPage);
  const { process: { loading, error }, data: rooms } = apiMethodState;
  const [searchValue, setSearchValue] = useState('');
  const [participating, setParticipating] = useState(false);
  const [closed, setClosed] = useState(false);
  const loaders = Array.from({ length: pageSize }, () => ({ height: '4.46rem' }));
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

  const handleNextPage = useCallback(() => {
    setPageNumber(pageNumber + 1);
  }, [pageNumber]);

  const handlePrevPage = useCallback(() => {
    setPageNumber(pageNumber - 1);
  }, [pageNumber]);

  const createRoomItem = useCallback((room: Room) => {
    const roomSummary =
      room.roomStatus === 'Review' ||
      room.roomStatus === 'Close';
    const roomLink = roomSummary ?
      pathnames.roomAnalyticsSummary.replace(':id', room.id) :
      pathnames.room.replace(':id', room.id);

    return (
      <li key={room.id}>
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
              placeHolder={Localization.NoTags}
              tags={room.tags}
            />
          </div>
          <div className='room-action-links'>
            <Link
              to={roomLink}
              className='room-join-link'
            >
              {Localization.Join}
            </Link>
            {admin && (
              <Link
                to={`${pathnames.roomsParticipants.replace(':id', room.id)}`}
                className='room-edit-participants-link'
              >
                {Localization.EditParticipants}
              </Link>
            )}
          </div>
        </div>
      </li>
    );
  }, [admin]);

  return (
    <MainContentWrapper className='rooms-page'>
      <HeaderField>
        <RoomsSearch
          searchValue={searchValue}
          onSearchChange={setSearchValue}
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
          <Field>
            <ul className="rooms-list">
              {roomsSafe.length === 0 ? (
                <div className="rooms-list-no-data">{Localization.NoRecords}</div>
              ) : (
                roomsSafe.map(createRoomItem)
              )}
            </ul>
          </Field>
          <Paginator
            pageNumber={pageNumber}
            prevDisabled={pageNumber === initialPageNumber}
            nextDisabled={roomsSafe.length !== pageSize}
            onPrevClick={handlePrevPage}
            onNextClick={handleNextPage}
          />
        </>
      </ProcessWrapper>
    </MainContentWrapper>
  );
};
