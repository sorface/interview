import { FunctionComponent, useEffect, useState } from 'react';
import { CodeEditorLang, Question, QuestionAnswer } from '../../types/question';
import { Button } from '../Button/Button';
import { Typography } from '../Typography/Typography';
import { CodeEditor } from '../CodeEditor/CodeEditor';

interface QuestionAnswersProps {
  answers: QuestionAnswer[];
  codeEditor?: Question['codeEditor'];
}

export const QuestionAnswers: FunctionComponent<QuestionAnswersProps> = ({
  answers,
  codeEditor,
}) => {
  const [selectedAnswer, setSelectedAnswer] = useState<QuestionAnswer | null>(null);

  useEffect(() => {
    if (!answers[0]) {
      return;
    }
    setSelectedAnswer(answers[0]);
  }, [answers]);

  return (
    <div className='text-left'>
      {answers.map(answer => (
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
          language={(selectedAnswer.codeEditor && codeEditor) ? codeEditor.lang : CodeEditorLang.Plaintext}
          languages={[(selectedAnswer.codeEditor && codeEditor) ? codeEditor.lang : CodeEditorLang.Plaintext]}
          value={selectedAnswer.content}
          readOnly
          scrollBeyondLastLine={false}
          alwaysConsumeMouseWheel={false}
          className='h-32.25'
        />
      )}
    </div>
  );
};
