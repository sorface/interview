import React, { FormEvent, FunctionComponent, useCallback, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import { ChangeParticipantStatusBody, roomParticipantApiDeclaration, roomsApiDeclaration } from '../../apiDeclarations';
import { Field } from '../../components/FieldsBlock/Field';
import { Loader } from '../../components/Loader/Loader';
import { MainContentWrapper } from '../../components/MainContentWrapper/MainContentWrapper';
import { SubmitField } from '../../components/SubmitField/SubmitField';
import { useApiMethod } from '../../hooks/useApiMethod';
import { Room } from '../../types/room';
import { LocalizationKey } from '../../localization';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';

const userFieldName = 'user';
const userTypeFieldName = 'userType';

export const RoomParticipants: FunctionComponent = () => {
  const localizationCaptions = useLocalizationCaptions();
  let { id } = useParams();
  const { apiMethodState, fetchData } = useApiMethod<Room, Room['id']>(roomsApiDeclaration.getById);
  const { process: { loading, error }, data: room } = apiMethodState;

  const {
    apiMethodState: changeParticipantStatusState,
    fetchData: changeParticipantStatusFetch,
  } = useApiMethod<object, ChangeParticipantStatusBody>(roomParticipantApiDeclaration.changeParticipantStatus);
  const {
    process: { loading: changeParticipantStatusLoading, error: changeParticipantStatusError },
    data: changeParticipantStatusData,
  } = changeParticipantStatusState;

  useEffect(() => {
    if (!id) {
      throw new Error('Room id not found');
    }
    fetchData(id);
  }, [id, fetchData]);

  const handleSubmit = useCallback(async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!id) {
      throw new Error('Room id not found');
    }
    const form = event.target as HTMLFormElement;
    const data = new FormData(form);
    const userId = data.get(userFieldName);
    if (!userId) {
      return;
    }
    if (typeof userId !== 'string') {
      throw new Error('userNickname field type error');
    }
    const userType = data.get(userTypeFieldName);
    if (!userType) {
      return;
    }
    if (typeof userType !== 'string') {
      throw new Error('userType field type error');
    }
    changeParticipantStatusFetch({
      roomId: id,
      userId,
      userType,
    });
  }, [id, changeParticipantStatusFetch]);

  const renderMainContent = useCallback(() => {
    if (loading || !room) {
      return (
        <Field>
          <Loader />
        </Field>
      )
    }

    if (error) {
      <Field>
        <div>Error: {error}</div>
      </Field>
    }

    return (
      <form action="" onSubmit={handleSubmit}>
        <Field>
          <select name={userFieldName}>
            {room.participants.map(participant => (
              <option key={participant.id} value={participant.id}>{participant.nickname}</option>
            ))}
          </select>
          <select name={userTypeFieldName}>
            <option value="Viewer">{localizationCaptions[LocalizationKey.Viewer]}</option>
            <option value="Expert">{localizationCaptions[LocalizationKey.Expert]}</option>
            <option value="Examinee">{localizationCaptions[LocalizationKey.Examinee]}</option>
          </select>
        </Field>
        <SubmitField caption={localizationCaptions[LocalizationKey.Save]} />
        {changeParticipantStatusLoading && (
          <Field><div>Changing participant status...</div></Field>
        )}
        {changeParticipantStatusError && (
          <Field><div>Changing participant status error: {changeParticipantStatusError}</div></Field>
        )}
        {changeParticipantStatusData && (
          <Field><div>Successfully changed participant status</div></Field>
        )}
      </form>
    )
  }, [
    loading,
    changeParticipantStatusLoading,
    error,
    changeParticipantStatusError,
    room,
    changeParticipantStatusData,
    localizationCaptions,
    handleSubmit,
  ]);

  return (
    <MainContentWrapper>
      {renderMainContent()}
    </MainContentWrapper>
  );
};
