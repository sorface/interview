import { MouseEvent, FunctionComponent, ReactNode, useState } from 'react';
import { CodeEditorLang, Question, QuestionAnswer } from '../../types/question';
import { Icon } from '../../pages/Room/components/Icon/Icon';
import { IconNames } from '../../constants';
import { Typography } from '../Typography/Typography';
import { Accordion } from '../Accordion/Accordion';
import { CodeEditor } from '../CodeEditor/CodeEditor';
import { Gap } from '../Gap/Gap';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../localization';
import { ContextMenu, ContextMenuProps } from '../ContextMenu/ContextMenu';
import { Button } from '../Button/Button';
import { CircularProgress } from '../CircularProgress/CircularProgress';
import { Checkbox } from '../Checkbox/Checkbox';

interface QuestionItemProps {
  question: Question;
  checked?: boolean;
  checkboxLabel?: ReactNode;
  mark?: number;
  primary?: boolean;
  contextMenu?: Omit<ContextMenuProps, 'toggleContent'>;
  bgSelected?: boolean;
  children?: ReactNode;
  onCheck?: (newValue: boolean) => void;
  onRemove?: (question: Question) => void;
  onClick?: (question: Question) => void;
}

export const QuestionItem: FunctionComponent<QuestionItemProps> = ({
  question,
  checked,
  checkboxLabel,
  mark,
  primary,
  contextMenu,
  bgSelected,
  children,
  onCheck,
  onRemove,
  onClick,
}) => {
  const localizationCaptions = useLocalizationCaptions();
  const hasCheckbox = typeof checked === 'boolean';
  const accordionDisabled =
    question.answers.length === 0 &&
    !question.codeEditor &&
    !children;
  const [selectedAnswer, setSelectedAnswer] = useState<QuestionAnswer | null>(
    question.answers ? question.answers[0] : null
  );

  const handleCheckboxChange = () => {
    onCheck?.(!checked);
  };

  const handleCheckboxAreaClick = (e: MouseEvent) => {
    e.stopPropagation();
  };

  const handleRemove = () => {
    onRemove?.(question);
  };

  const handleOnClick = () => {
    onClick?.(question);
  };

  const title = (
    <>
      {typeof mark === 'number' && (
        <>
          <CircularProgress
            size='s'
            value={mark * 10}
            caption={mark.toFixed(1)}
          />
          <Gap sizeRem={1.5} horizontal />
        </>
      )}
      <div className={`${!accordionDisabled ? 'px-0.75' : ''}`}>
        <Typography size='m' bold>
          {question.value}
        </Typography>
      </div>
      <div className='ml-auto'>
        {contextMenu && (
          <div onClick={handleCheckboxAreaClick}>
            <ContextMenu
              {...contextMenu}
              toggleContent={(
                <div className='text-grey2'>
                  <Icon size='s' name={IconNames.EllipsisVertical} />
                </div>
              )}
              buttonVariant='text'
            />
          </div>
        )}
        {hasCheckbox && (
          <div onClick={handleCheckboxAreaClick}>
            <Checkbox
              id={`questionCheckbox${question.id}`}
              checked={checked}
              label={checkboxLabel && (checkboxLabel)}
              onChange={handleCheckboxChange}
            />
          </div>
        )}
        {onRemove && (
          <span onClick={handleRemove} className='cursor-pointer text-grey3'>
            <Icon size='s' name={IconNames.Trash} />
          </span>
        )}
        {onClick && (
          <span className='opacity-0.5'>
            <Icon name={IconNames.ChevronForward} />
          </span>
        )}
      </div>
    </>
  );

  return (
    <Accordion
      title={title}
      disabled={accordionDisabled}
      className={`${primary ? 'bg-wrap' : 'bg-form'} ${bgSelected ? 'bg-blue-light' : ''} rounded-0.75 py-1.25 px-1.5`}
      classNameTitle='flex items-center'
      onClick={onClick ? handleOnClick : undefined}
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
      {children}
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
