import React, { useEffect, useState } from 'react';
import { TreeNode } from 'versatile-tree';
import { TreeController } from './hooks/useTreeController';
import { useApiMethod } from '../../hooks/useApiMethod';
import { Question } from '../../types/question';
import { questionsApiDeclaration } from '../../apiDeclarations';
import { Typography } from '../Typography/Typography';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../localization';
import { Icon } from '../../pages/Room/components/Icon/Icon';
import { IconNames } from '../../constants';
import { Button } from '../Button/Button';
import { RoomQuestionsSelector } from '../../pages/RoomCreate/RoomQuestionsSelector/RoomQuestionsSelector';
import { RoomQuestionListItem } from '../../types/room';
import { Modal } from '../Modal/Modal';

export interface BasicTreeNodeTitleComponentProps {
  node: TreeNode;
  treeController: TreeController;
}

export const BasicTreeNodeTitleComponent = ({
  node,
  treeController,
}: BasicTreeNodeTitleComponentProps) => {
  const localizationCaptions = useLocalizationCaptions();
  const [questionsSelectorOpen, setQuestionsSelectorOpen] = useState(false);
  const { apiMethodState: getQuestionState, fetchData: fetchQuestion } =
    useApiMethod<Question, Question['id']>(questionsApiDeclaration.get);
  const {
    process: { loading: questionLoading, error: questionError },
    data: question,
  } = getQuestionState;

  const questionId = node.getData().question?.id;

  useEffect(() => {
    if (!questionId) {
      return;
    }
    fetchQuestion(questionId);
  }, [questionId]);

  const handleQuestionsSelectorOpen = () => {
    setQuestionsSelectorOpen(true);
  };

  const handleQuestionsSelectorClose = () => {
    setQuestionsSelectorOpen(false);
  };

  const handleQuestionsSelectorSave = (questions: RoomQuestionListItem[]) => {
    treeController.mutations.updateNode(node, {
      question: { id: questions[0].id, value: questions[0].value },
    });
    setQuestionsSelectorOpen(false);
  };

  return (
    <>
      <div className="flex items-center">
        {questionLoading && (
          <Typography size="m">
            {localizationCaptions[LocalizationKey.Loading]}
          </Typography>
        )}
        {question && <Typography size="m">{question.value}</Typography>}
        {questionError && (
          <Typography size="m" error>
            {questionError}
          </Typography>
        )}
        {!questionId && <Typography size="m">EMPTY</Typography>}
        <Button variant="text" onClick={handleQuestionsSelectorOpen}>
          <Icon name={IconNames.Settings} />
        </Button>
      </div>
      {questionsSelectorOpen && (
        <Modal
          open={questionsSelectorOpen}
          onClose={handleQuestionsSelectorClose}
          contentLabel=""
        >
          <RoomQuestionsSelector
            preSelected={[]}
            onCancel={handleQuestionsSelectorClose}
            onSave={handleQuestionsSelectorSave}
          />
        </Modal>
      )}
    </>
  );
};
