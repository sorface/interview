import { FunctionComponent, useContext, useState } from 'react';
import { ReviewUserGrid } from '../ReviewUserGrid/ReviewUserGrid';
import { Analytics, AnalyticsUserReview } from '../../../../types/analytics';
import { ReviewUserOpinion } from '../ReviewUserOpinion/ReviewUserOpinion';
import { SwitcherButton } from '../../../../components/SwitcherButton/SwitcherButton';
import { useLocalizationCaptions } from '../../../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../../../localization';
import { Theme, ThemeContext } from '../../../../context/ThemeContext';
import { QuestionAnswerDetails } from '../../../../components/QuestionAnswerDetails/QuestionAnswerDetails';
import { Gap } from '../../../../components/Gap/Gap';

interface RoomAnayticsDetailsProps {
  data: Analytics | null;
  allUsers: Map<string, AnalyticsUserReview>;
  openedQuestionDetails: string;
  roomId: string | undefined;
}

export const RoomAnayticsDetails: FunctionComponent<RoomAnayticsDetailsProps> = ({
  data,
  allUsers,
  openedQuestionDetails,
  roomId,
}) => {
  const localizationCaptions = useLocalizationCaptions();
  const { themeInUi } = useContext(ThemeContext);
  const [activeTab, setActiveTab] = useState<0 | 1>(0);
  const openedQuestion = data?.questions.find(question => question.id === openedQuestionDetails);

  return (
    <div>
      <SwitcherButton
        captions={[
          localizationCaptions[LocalizationKey.Opinions],
          localizationCaptions[LocalizationKey.Transcription],
        ]}
        activeIndex={activeTab}
        {...(themeInUi === Theme.Dark && {
          variant: 'alternative',
        })}
        onClick={setActiveTab}
      />
      <Gap sizeRem={2.25} />
      {activeTab === 0 && (
        <ReviewUserGrid>
          {openedQuestion?.users
            .filter(questionUser => allUsers.get(questionUser.id)?.participantType === 'Expert')
            .map(questionUser => {
              if (!questionUser.evaluation) {
                return undefined;
              }
              return (
                <ReviewUserOpinion
                  key={questionUser.id}
                  user={questionUser}
                  allUsers={allUsers}
                />
              );
            })}
        </ReviewUserGrid>
      )}
      {(activeTab === 1 && openedQuestion) && (
        <QuestionAnswerDetails
          questionId={openedQuestion.id}
          questionTitle={openedQuestion.value}
          roomId={roomId || ''}
        />
      )}
    </div>
  );
};
