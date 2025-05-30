import React, {
  MouseEvent,
  FunctionComponent,
  ReactNode,
  useState,
} from 'react';
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
import { useThemeClassName } from '../../hooks/useThemeClassName';
import { Theme } from '../../context/ThemeContext';

interface QuestionItemProps {
  question: Question;
  openedByDefault?: boolean;
  checked?: boolean;
  checkboxLabel?: ReactNode;
  mark?: number;
  categoryName?: string;
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
  openedByDefault,
  checked,
  checkboxLabel,
  mark,
  categoryName,
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
    question.answers.length === 0 && !question.codeEditor && !children;
  const [selectedAnswer, setSelectedAnswer] = useState<QuestionAnswer | null>(
    question.answers ? question.answers[0] : null,
  );
  const contextMenuIconClassName = useThemeClassName({
    [Theme.Dark]: 'text-dark-grey4',
    [Theme.Light]: 'text-grey2',
  });

  const accordionClassName = useThemeClassName({
    [Theme.Dark]: `${bgSelected ? 'bg-dark-dark2' : primary ? 'bg-dark-dark2' : 'bg-dark-disable'}`,
    [Theme.Light]: `${bgSelected ? '!bg-blue-light' : primary ? 'bg-white' : 'bg-grey1'}`,
  });

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
      {(typeof mark === 'number' || mark === null) && (
        <>
          <CircularProgress
            size="s"
            value={typeof mark === 'number' ? mark * 10 : null}
            caption={typeof mark === 'number' ? mark.toFixed(1) : null}
          />
          <Gap sizeRem={1.5} horizontal />
        </>
      )}
      <div
        className={`flex items-baseline ${!accordionDisabled ? 'px-[0.75rem]' : ''}`}
      >
        <Typography size="m" bold>
          {question.value}
        </Typography>
        {categoryName && (
          <>
            <Gap sizeRem={1.15} horizontal />
            <Typography size="m" secondary>
              {question.category.name}
            </Typography>
          </>
        )}
      </div>
      <div className="ml-auto flex items-center h-[1.3125rem]">
        {contextMenu && (
          <div onClick={handleCheckboxAreaClick}>
            <ContextMenu
              {...contextMenu}
              toggleContent={
                <div className={contextMenuIconClassName}>
                  <Icon size="s" name={IconNames.EllipsisVertical} />
                </div>
              }
              buttonVariant="text"
            />
          </div>
        )}
        {hasCheckbox && (
          <div onClick={handleCheckboxAreaClick}>
            <Checkbox
              id={`questionCheckbox${question.id}`}
              checked={checked}
              label={checkboxLabel && checkboxLabel}
              onChange={handleCheckboxChange}
            />
          </div>
        )}
        {onRemove && (
          <span onClick={handleRemove} className="cursor-pointer text-grey3">
            <Icon size="s" name={IconNames.Trash} />
          </span>
        )}
        {onClick && (
          <span className="opacity-50">
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
      openedByDefault={openedByDefault}
      className={`${accordionClassName} rounded-[0.75rem] py-[1.25rem] px-[1.5rem]`}
      classNameTitle="flex items-center"
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
            className="h-[32.25rem]"
          />
        </>
      )}
      <Gap sizeRem={1.5} />
      {!!question.answers.length && (
        <>
          <Typography size="m" bold>
            {localizationCaptions[LocalizationKey.QuestionAnswerOptions]}
          </Typography>
          <Gap sizeRem={1} />
        </>
      )}
      {children}
      {question.answers.map((answer) => (
        <Button
          key={answer.id}
          variant={answer === selectedAnswer ? 'active' : undefined}
          className="mr-[0.25rem]"
          onClick={() => setSelectedAnswer(answer)}
        >
          <Typography size="m">{answer.title}</Typography>
        </Button>
      ))}
      {!!selectedAnswer && (
        <>
          <Gap sizeRem={1} />
          <CodeEditor
            language={
              selectedAnswer.codeEditor && question.codeEditor
                ? question.codeEditor.lang
                : CodeEditorLang.Plaintext
            }
            languages={[
              selectedAnswer.codeEditor && question.codeEditor
                ? question.codeEditor.lang
                : CodeEditorLang.Plaintext,
            ]}
            value={selectedAnswer.content}
            readOnly
            scrollBeyondLastLine={false}
            alwaysConsumeMouseWheel={false}
            className="h-[32.25rem]"
          />
        </>
      )}
    </Accordion>
  );
};
