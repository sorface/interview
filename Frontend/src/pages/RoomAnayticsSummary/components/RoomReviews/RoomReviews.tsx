import React, { ChangeEvent, FormEvent, FunctionComponent, useCallback, useContext, useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { AuthContext } from '../../../../context/AuthContext';
import { checkAdmin } from '../../../../utils/checkAdmin';
import { useApiMethod } from '../../../../hooks/useApiMethod';
import { AddRoomReviewBody, GetRoomReviewsParams, UpdateRoomReviewsParams, roomReviewApiDeclaration } from '../../../../apiDeclarations';
import { ProcessWrapper } from '../../../../components/ProcessWrapper/ProcessWrapper';
import { Paginator } from '../../../../components/Paginator/Paginator';
import { Room, RoomReview } from '../../../../types/room';
import { SubmitField } from '../../../../components/SubmitField/SubmitField';
import { Field } from '../../../../components/FieldsBlock/Field';
import { LocalizationKey } from '../../../../localization';
import { useLocalizationCaptions } from '../../../../hooks/useLocalizationCaptions';

import './RoomReviews.css';

const pageSize = 10;
const initialPageNumber = 1;
const valueFieldName = 'reviewText';

interface RoomReviewsProps {
  roomId: Room['id'];
}

export const RoomReviews: FunctionComponent<RoomReviewsProps> = ({ roomId }) => {
  let { id } = useParams();
  const auth = useContext(AuthContext);
  const admin = checkAdmin(auth);
  const localizationCaptions = useLocalizationCaptions();
  const [pageNumber, setPageNumber] = useState(initialPageNumber);
  const { apiMethodState: roomReviewsState, fetchData: fetchReviews } = useApiMethod<RoomReview[], GetRoomReviewsParams>(roomReviewApiDeclaration.getPage);
  const { process: { loading, error }, data: roomReviews } = roomReviewsState;
  const { apiMethodState: updatingReviewState, fetchData: fetchUpdateRoomReview } = useApiMethod<RoomReview, UpdateRoomReviewsParams>(roomReviewApiDeclaration.update);

  const {
    process: { loading: updatingLoading, error: updatingError },
    data: updatedRoomReviewId,
  } = updatingReviewState;
  const [editingRoomReview, setEditingRoomReview] = useState<RoomReview | null>(null);

  const {
    apiMethodState: addRoomReviewState,
    fetchData: fetchAddRoomReview,
  } = useApiMethod<RoomReview, AddRoomReviewBody>(roomReviewApiDeclaration.addReview);
  const { process: { loading: addRoomReviewLoading, error: addRoomReviewError }, data: addedRoomReview } = addRoomReviewState;

  const roomReviewsSafe = roomReviews || [];

  useEffect(() => {
    if (!id) {
      throw new Error('Room id not found');
    }
    fetchReviews({
      'Page.PageSize': pageSize,
      'Page.PageNumber': pageNumber,
      'Filter.RoomId': id,
    });
  }, [id, fetchReviews, pageNumber]);

  useEffect(() => {
    if (!id) {
      throw new Error('Room id not found');
    }
    if (updatedRoomReviewId) {
      fetchReviews({
        'Page.PageSize': pageSize,
        'Page.PageNumber': pageNumber,
        'Filter.RoomId': id,
      });
    }
  }, [id, updatedRoomReviewId, pageNumber, fetchReviews]);

  useEffect(() => {
    if (!id) {
      throw new Error('Room id not found');
    }
    if (addedRoomReview) {
      fetchReviews({
        'Page.PageSize': pageSize,
        'Page.PageNumber': pageNumber,
        'Filter.RoomId': id,
      });
    }
  }, [id, addedRoomReview, pageNumber, fetchReviews]);

  const handleNextPage = useCallback(() => {
    setPageNumber(pageNumber + 1);
  }, [pageNumber]);

  const handlePrevPage = useCallback(() => {
    setPageNumber(pageNumber - 1);
  }, [pageNumber]);

  const handleRoomReviewEdit = useCallback((roomReview: RoomReview) => () => {
    setEditingRoomReview(roomReview);
  }, []);

  const handleRoomReviewEditClose = useCallback(() => {
    setEditingRoomReview(null);
  }, []);

  const handleRoomReviewDelete = useCallback((roomReview: RoomReview) => () => {
    fetchUpdateRoomReview({
      id: roomReview.id,
      review: roomReview.review,
      state: 'Closed',
    });
  }, [fetchUpdateRoomReview]);

  const handleEditingRoomReviewValueChange = useCallback((event: ChangeEvent<HTMLTextAreaElement>) => {
    if (!editingRoomReview) {
      console.error('handleEditingRoomReviewValueChange without editingRoomReview');
      return;
    }
    setEditingRoomReview({
      ...editingRoomReview,
      review: event.target.value,
    });
  }, [editingRoomReview]);

  const handleEditingRoomReviewSubmit = useCallback(() => {
    if (!editingRoomReview) {
      console.error('handleEditingRoomReviewSubmit without editingRoomReview');
      return;
    }
    fetchUpdateRoomReview({
      id: editingRoomReview.id,
      review: editingRoomReview.review,
      state: editingRoomReview.state,
    });
    setEditingRoomReview(null);
  }, [editingRoomReview, fetchUpdateRoomReview]);

  const createRoomReviewItem = useCallback((roomReview: RoomReview) => (
    <li key={roomReview.id}>
      {roomReview.id === editingRoomReview?.id ? (
        <div className="roomReview-item">
          <textarea
            value={editingRoomReview.review}
            onInput={handleEditingRoomReviewValueChange}
          />
          <button
            className="roomReview-edit-button"
            onClick={handleEditingRoomReviewSubmit}
          >
            ✔️
          </button>
          <button
            className="roomReview-delete-button"
            onClick={handleRoomReviewEditClose}
          >
            ✖
          </button>
        </div>
      ) : (
        <div className="roomReview-item">
          <div className='roomReview-item-review'>
            <div>{roomReview.review}</div>
            <div className='roomReview-item-review-user'>
              {localizationCaptions[LocalizationKey.WithLove]}, {roomReview.user.nickname}.
            </div>
          </div>
          {(admin || auth?.id === roomReview.user.id) && (
            <>
              <button
                onClick={handleRoomReviewEdit(roomReview)}
              >
                🖊️
              </button>
              <button
                className="roomReview-delete-button"
                onClick={handleRoomReviewDelete(roomReview)}
              >
                ❌
              </button>
            </>
          )}
        </div>
      )}

    </li>
  ), [
    admin,
    auth?.id,
    editingRoomReview,
    localizationCaptions,
    handleRoomReviewDelete,
    handleRoomReviewEdit,
    handleRoomReviewEditClose,
    handleEditingRoomReviewValueChange,
    handleEditingRoomReviewSubmit,
  ]);

  const handleSubmit = useCallback(async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const form = event.target as HTMLFormElement;
    const data = new FormData(form);
    const reviewText = data.get(valueFieldName);
    if (!reviewText) {
      return;
    }
    fetchAddRoomReview({
      roomId,
      review: String(reviewText),
    });
  }, [roomId, fetchAddRoomReview]);


  return (
    <ProcessWrapper
      loading={loading || updatingLoading}
      error={error || updatingError}
      loaders={Array.from({ length: pageSize }, () => ({}))}
    >
      <>
        <Field className='roomReviews'>
          <h3>{localizationCaptions[LocalizationKey.Reviews]}:</h3>
          <ul className="roomReviews-list">
            {roomReviewsSafe.map(createRoomReviewItem)}
          </ul>
          <Paginator
            pageNumber={pageNumber}
            prevDisabled={pageNumber === initialPageNumber}
            nextDisabled={roomReviewsSafe.length !== pageSize}
            onPrevClick={handlePrevPage}
            onNextClick={handleNextPage}
          />
        </Field>
        <Field>
          <div className='roomReview'>
            <form onSubmit={handleSubmit}>
              <label htmlFor="reviewText">{localizationCaptions[LocalizationKey.AddReview]}:</label>
              <textarea id="reviewText" placeholder={localizationCaptions[LocalizationKey.AddReviewPlaceholder]} name={valueFieldName}></textarea>
              {addRoomReviewLoading && (<div>Sending room review...</div>)}
              {addRoomReviewError && (<div>Error sending room review</div>)}
              <SubmitField caption={localizationCaptions[LocalizationKey.Send]} />
            </form>
          </div>
        </Field>
      </>
    </ProcessWrapper>
  );
};
