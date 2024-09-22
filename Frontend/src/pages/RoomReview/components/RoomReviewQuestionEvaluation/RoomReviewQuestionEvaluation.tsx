import { FunctionComponent, useEffect, useState } from 'react';
import { useApiMethod } from '../../../../hooks/useApiMethod';
import { RoomQuestionEvaluation, RoomQuestionEvaluationValue } from '../../../Room/components/RoomQuestionEvaluation/RoomQuestionEvaluation';
import { MyRoomQuestionEvaluation, MyRoomQuestionEvaluation as RoomQuestionEvaluationType } from '../../../../types/room'
import { MergeRoomQuestionEvaluationBody, roomQuestionEvaluationApiDeclaration } from '../../../../apiDeclarations';
import { Typography } from '../../../../components/Typography/Typography';
import { Icon } from '../../../Room/components/Icon/Icon';
import { useLocalizationCaptions } from '../../../../hooks/useLocalizationCaptions';
import { IconNames } from '../../../../constants';
import { LocalizationKey } from '../../../../localization';
import { Loader } from '../../../../components/Loader/Loader';
import { QuestionItem } from '../../../../components/QuestionItem/QuestionItem';
import { Question } from '../../../../types/question';
import { Gap } from '../../../../components/Gap/Gap';

interface RoomReviewQuestionEvaluationProps {
  roomId: string;
  questionEvaluations: MyRoomQuestionEvaluation;
  readOnly?: boolean;
  onDetailsOpen: () => void;
}

const mergeRoomQuestionEvaluationDebounceMs = 1000;
const defaultRoomQuestionEvaluation: RoomQuestionEvaluationValue = {
  mark: 0,
  review: '',
};

const createFakeQuestion = (roomQuestion: MyRoomQuestionEvaluation): Question => ({
  ...roomQuestion,
  tags: [],
  answers: [],
  codeEditor: null,
  category: {
    id: '',
    name: '',
    parentId: '',
  },
});

export const RoomReviewQuestionEvaluation: FunctionComponent<RoomReviewQuestionEvaluationProps> = ({
  roomId,
  questionEvaluations,
  readOnly,
  onDetailsOpen,
}) => {
  const localizationCaptions = useLocalizationCaptions();
  const [roomQuestionEvaluation, setRoomQuestionEvaluation] = useState<RoomQuestionEvaluationValue | null>(null);
  const actualRoomQuestionEvaluation = roomQuestionEvaluation || questionEvaluations.evaluation || defaultRoomQuestionEvaluation;
  const doNotRateChecked = actualRoomQuestionEvaluation.mark === 0;

  const {
    apiMethodState: apiMergeRoomQuestionEvaluationState,
    fetchData: mergeRoomQuestionEvaluation,
  } = useApiMethod<RoomQuestionEvaluationType, MergeRoomQuestionEvaluationBody>(roomQuestionEvaluationApiDeclaration.merge);
  const {
    data: mergedRoomQuestionEvaluation,
    process: {
      loading: loadingMergeRoomQuestionEvaluation,
      error: errorMergeRoomQuestionEvaluation,
    },
  } = apiMergeRoomQuestionEvaluationState;

  useEffect(() => {
    if (!roomQuestionEvaluation) {
      return;
    }
    const requestTimeout = setTimeout(() => {
      mergeRoomQuestionEvaluation({
        ...roomQuestionEvaluation,
        questionId: questionEvaluations.id,
        roomId: roomId,
        review: roomQuestionEvaluation.review || '',
        mark: roomQuestionEvaluation.mark || null,
      });
    }, mergeRoomQuestionEvaluationDebounceMs);

    return () => {
      clearTimeout(requestTimeout);
    };
  }, [roomQuestionEvaluation, questionEvaluations, roomId, mergeRoomQuestionEvaluation]);

  const handleDoNotRateCheck = () => {
    setRoomQuestionEvaluation({
      mark: 0,
      review: '',
    });
  };

  const handleRoomQuestionEvaluationChange = (newValue: RoomQuestionEvaluationValue) => {
    setRoomQuestionEvaluation(newValue);
  };

  return (
    <QuestionItem
      openedByDefault
      question={createFakeQuestion(questionEvaluations)}
      checked={doNotRateChecked}
      checkboxLabel={<Typography size='m' bold>{localizationCaptions[LocalizationKey.DoNotRate]}</Typography>}
      onCheck={handleDoNotRateCheck}
    >
      <RoomQuestionEvaluation
        readOnly={readOnly}
        value={actualRoomQuestionEvaluation}
        validateComment
        onChange={handleRoomQuestionEvaluationChange}
      />
      <div className='text-left h-1.125'>
        {mergedRoomQuestionEvaluation && (
          <Typography size='s'>
            <Icon name={IconNames.CheckmarkDone} />
            {localizationCaptions[LocalizationKey.Saved]}
          </Typography>
        )}
        {loadingMergeRoomQuestionEvaluation && (<Loader />)}
        {errorMergeRoomQuestionEvaluation && (
          <Typography size='s' error>{localizationCaptions[LocalizationKey.Error]}: {errorMergeRoomQuestionEvaluation}</Typography>
        )}
      </div>
      <Gap sizeRem={1.5} />
      <div
        className='text-right cursor-pointer'
        onClick={onDetailsOpen}
      >
        <Typography size='s' secondary>
          {localizationCaptions[LocalizationKey.QuestionAnswerDetails]}
        </Typography>
      </div>
    </QuestionItem>
  );
};
