import { FunctionComponent, useState } from 'react';
import { CodeEditorLang, Question, QuestionAnswer } from '../../types/question';
import { ThemedIcon } from '../../pages/Room/components/ThemedIcon/ThemedIcon';
import { IconNames } from '../../constants';
import { Typography } from '../Typography/Typography';
import { Accordion } from '../Accordion/Accordion';
import { CodeEditor } from '../CodeEditor/CodeEditor';
import { Gap } from '../Gap/Gap';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../localization';
import { ContextMenu, ContextMenuProps } from '../ContextMenu/ContextMenu';
import { Button } from '../Button/Button';

interface QuestionItemProps {
  question: Question;
  checked?: boolean;
  primary?: boolean;
  contextMenu?: ContextMenuProps;
  onCheck?: (newValue: boolean) => void;
  onRemove?: (question: Question) => void;
}

export const QuestionItem: FunctionComponent<QuestionItemProps> = ({
  question,
  checked,
  primary,
  contextMenu,
  onCheck,
  onRemove,
}) => {
  const localizationCaptions = useLocalizationCaptions();
  const hasCheckbox = typeof checked === 'boolean';
  const accordionDisabled = question.answers.length === 0;
  const [selectedAnswer, setSelectedAnswer] = useState<QuestionAnswer | null>(
    question.answers ? question.answers[0] : null
  );

  const handleCheckboxChange = () => {
    onCheck?.(!checked);
  };

  const handleRemove = () => {
    onRemove?.(question);
  };

  const title = (
    <>
      <div className={`${!accordionDisabled ? 'px-0.75' : ''}`}>
        <Typography size='m' bold>
          {question.value}
        </Typography>
      </div>
      <div className='ml-auto'>
        {contextMenu && <ContextMenu {...contextMenu} buttonVariant='text' />}
        {hasCheckbox && <input type='checkbox' checked={checked} onChange={handleCheckboxChange} />}
        {onRemove && (
          <span onClick={handleRemove} className='cursor-pointer'>
            <ThemedIcon name={IconNames.Trash} size='small' />
          </span>
        )}
      </div>
    </>
  );

  return (
    <Accordion
      title={title}
      disabled={accordionDisabled}
      className={`${primary ? 'bg-wrap' : 'bg-form'} rounded-0.75 py-1.25 px-1.5`}
      classNameTitle='flex items-center'
    >
      {question.codeEditor && (
        <>
          <Gap sizeRem={1.5} />
          <CodeEditor
            language={question.codeEditor.lang}
            languages={[question.codeEditor.lang]}
            value={question.codeEditor.content}
            readOnly
            scrollBeyondLastLine={false}
            alwaysConsumeMouseWheel={false}
            className='h-32.25'
          />
        </>

      )}
      <Gap sizeRem={1.5} />
      {!!question.answers.length && (
        <>
          <Typography size='m' bold>
            {localizationCaptions[LocalizationKey.QuestionAnswerOptions]}
          </Typography>
          <Gap sizeRem={1} />
        </>
      )}
      {question.answers.map(answer => (
        <Button
          key={answer.id}
          variant={answer === selectedAnswer ? 'active' : undefined}
          className='mr-0.25'
          onClick={() => setSelectedAnswer(answer)}
        >
          <Typography size='m'>
            {answer.title}
          </Typography>
        </Button>
      ))}
      {!!selectedAnswer && (
        <CodeEditor
          language={(selectedAnswer.codeEditor && question.codeEditor) ? question.codeEditor.lang : CodeEditorLang.Plaintext}
          languages={[(selectedAnswer.codeEditor && question.codeEditor) ? question.codeEditor.lang : CodeEditorLang.Plaintext]}
          value={selectedAnswer.content}
          readOnly
          scrollBeyondLastLine={false}
          alwaysConsumeMouseWheel={false}
          className='h-32.25'
        />
      )}
    </Accordion>
  );
};
