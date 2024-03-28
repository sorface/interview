import React, { Fragment, FunctionComponent, useCallback, useContext, useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { MainContentWrapper } from '../../components/MainContentWrapper/MainContentWrapper';
import { HeaderWithLink } from '../../components/HeaderWithLink/HeaderWithLink';
import { pathnames } from '../../constants';
import { Field } from '../../components/FieldsBlock/Field';
import { useApiMethod } from '../../hooks/useApiMethod';
import { roomsApiDeclaration } from '../../apiDeclarations';
import { AnalyticsQuestionsExpert, AnalyticsSummary } from '../../types/analytics';
import { Room as RoomType } from '../../types/room';
import { Mark } from '../../components/Mark/Mark';
import { ProcessWrapper } from '../../components/ProcessWrapper/ProcessWrapper';
import { RoomReviews } from './components/RoomReviews/RoomReviews';
import { AuthContext } from '../../context/AuthContext';
import { checkAdmin } from '../../utils/checkAdmin';
import { ActionModal } from '../../components/ActionModal/ActionModal';
import { LocalizationKey } from '../../localization';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';

import './RoomAnayticsSummary.css';

export const RoomAnayticsSummary: FunctionComponent = () => {
  const auth = useContext(AuthContext);
  const admin = checkAdmin(auth);
  const localizationCaptions = useLocalizationCaptions();
  let { id } = useParams();
  const { apiMethodState, fetchData } = useApiMethod<AnalyticsSummary, RoomType['id']>(roomsApiDeclaration.analyticsSummary);
  const { data, process: { loading, error } } = apiMethodState;

  const {
    apiMethodState: roomApiMethodState,
    fetchData: fetchRoom,
  } = useApiMethod<RoomType, RoomType['id']>(roomsApiDeclaration.getById);
  const {
    process: { loading: roomLoading, error: roomError },
    data: room,
  } = roomApiMethodState;

  const {
    apiMethodState: apiRoomCloseMethodState,
    fetchData: fetchRoomClose,
  } = useApiMethod<unknown, RoomType['id']>(roomsApiDeclaration.close);
  const {
    process: { loading: roomCloseLoading, error: roomCloseError },
  } = apiRoomCloseMethodState;

  const {
    apiMethodState: apiRoomStartReviewMethodState,
    fetchData: fetchRoomStartReview,
  } = useApiMethod<unknown, RoomType['id']>(roomsApiDeclaration.startReview);
  const {
    process: { loading: roomStartReviewLoading, error: roomStartReviewError },
  } = apiRoomStartReviewMethodState;

  const displayedReactions = ['Like', 'Dislike'];
  const displayedReactionsView = [localizationCaptions[LocalizationKey.LikeTable], localizationCaptions[LocalizationKey.DislikeTable]];
  const [totalLikesDislikes, setTotalLikesDislikes] = useState({ likes: 0, dislikes: 0 });
  const [totalMarkError, setTotalMarkError] = useState('');
  const loaders = [
    {},
    { height: '3.5rem' },
    {},
  ];

  useEffect(() => {
    if (!id) {
      throw new Error('Room id not found');
    }
    fetchData(id);
    fetchRoom(id);
  }, [id, fetchData, fetchRoom]);

  useEffect(() => {
    if (!data?.questions) {
      return;
    }
    const getExpertReactionsCount = (expert: AnalyticsQuestionsExpert, reactionType: string) =>
      expert.reactionsSummary.find(reaction => reaction.type === reactionType)?.count || 0;

    const expertReactionsSummary = data.questions.reduce((totalAcc, question) => {
      if (!question.experts) {
        return { ...totalAcc };
      }
      const expertSummary = question.experts.reduce((expertAcc, expert) => ({
        likes: expertAcc.likes + getExpertReactionsCount(expert, 'Like'),
        dislikes: expertAcc.dislikes + getExpertReactionsCount(expert, 'Dislike'),
      }), { likes: 0, dislikes: 0 });
      return {
        likes: totalAcc.likes + expertSummary.likes,
        dislikes: totalAcc.dislikes + expertSummary.dislikes,
      }
    }, { likes: 0, dislikes: 0 });
    if (!expertReactionsSummary.likes || !expertReactionsSummary.dislikes) {
      setTotalMarkError(localizationCaptions[LocalizationKey.FailedToCalculateMark]);
      return;
    }
    setTotalLikesDislikes(expertReactionsSummary);
  }, [data, localizationCaptions]);

  const handleCloseRoom = useCallback(() => {
    if (!id) {
      throw new Error('Room id not found');
    }
    fetchRoomClose(id);
  }, [id, fetchRoomClose]);

  const handleStartReviewRoom = useCallback(() => {
    if (!id) {
      throw new Error('Room id not found');
    }
    fetchRoomStartReview(id);
  }, [id, fetchRoomStartReview]);

  return (
    <MainContentWrapper className="room-anaytics-summary">
      <HeaderWithLink
        title={localizationCaptions[LocalizationKey.RoomAnayticsSummary]}
        linkVisible={true}
        path={pathnames.rooms}
        linkCaption="<"
        linkFloat="left"
      />
      <ProcessWrapper
        loading={loading || roomLoading}
        error={error || roomError}
        loaders={loaders}
      >
        <>
        {!!(admin && room?.roomStatus === 'Review') && (
            <Field>
              <ActionModal
                title={localizationCaptions[LocalizationKey.CloseRoomModalTitle]}
                openButtonCaption={localizationCaptions[LocalizationKey.CloseRoom]}
                loading={roomCloseLoading}
                loadingCaption={localizationCaptions[LocalizationKey.CloseRoomLoading]}
                error={roomCloseError}
                onAction={handleCloseRoom}
              />
            </Field>
          )}
          {!!(admin && room?.roomStatus === 'Close') && (
            <Field>
              <ActionModal
                title={localizationCaptions[LocalizationKey.StartReviewRoomModalTitle]}
                openButtonCaption={localizationCaptions[LocalizationKey.StartReviewRoom]}
                loading={roomStartReviewLoading}
                loadingCaption={localizationCaptions[LocalizationKey.CloseRoomLoading]}
                error={roomStartReviewError}
                onAction={handleStartReviewRoom}
              />
            </Field>
          )}
          <Field>
            <h3>{room?.name}</h3>
          </Field>
          <Field>
            <h3>{localizationCaptions[LocalizationKey.MarkSmmary]}:</h3>
            <div>
              {totalMarkError ? totalMarkError : (<Mark {...totalLikesDislikes} />)}
            </div>
          </Field>
          <Field>
            <h3>{localizationCaptions[LocalizationKey.QuestionsSummary]}:</h3>
            <table className='anaytics-table'>
              <thead>
                <tr>
                  <th>{localizationCaptions[LocalizationKey.Question]}</th>
                  <th></th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {data?.questions.map(question => (
                  <Fragment key={question.id}>
                    <tr>
                      <td className="question-cell">
                        {question.value}
                      </td>
                      {displayedReactions.map((reaction, reactionIndex) => (
                        <td key={reaction}>{displayedReactionsView[reactionIndex]}</td>
                      ))}
                    </tr>
                    {question.experts && question.experts.map(expert => (
                      <tr key={`${question.id}${expert.id}`} className="user-row">
                        <td>{expert.nickname}</td>
                        {displayedReactions.map(displayedReaction => (
                          <td key={`expert-${displayedReaction}`}>
                            {expert.reactionsSummary.find(
                              reactionSummary => reactionSummary.type === displayedReaction
                            )?.count || 0}
                          </td>
                        ))}
                      </tr>
                    ))}
                    {question.viewers && question.viewers.map(viewer => (
                      <tr key={`${question.id}-viewer`} className="user-row">
                        <td>{localizationCaptions[LocalizationKey.Viewers]}</td>
                        {displayedReactions.map(displayedReaction => (
                          <td key={`viewer-${displayedReaction}`}>
                            {viewer.reactionsSummary.find(
                              reactionSummary => reactionSummary.type === displayedReaction
                            )?.count || 0}
                          </td>
                        ))}
                      </tr>
                    ))}
                  </Fragment>
                ))}
              </tbody>
            </table>
          </Field>
          <RoomReviews roomId={id || ''} />
        </>
      </ProcessWrapper>
    </MainContentWrapper>
  );
};
