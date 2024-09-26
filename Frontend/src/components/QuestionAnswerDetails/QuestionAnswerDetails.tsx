import { FunctionComponent, useContext, useEffect, useState } from 'react';
import { useApiMethod } from '../../hooks/useApiMethod';
import { GetAnswerParams, roomQuestionApiDeclaration } from '../../apiDeclarations';
import { Loader } from '../Loader/Loader';
import { Typography } from '../Typography/Typography';
import { RoomQuestionAnswer } from '../../types/room';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../localization';
import { Gap } from '../Gap/Gap';
import { SwitcherButton } from '../SwitcherButton/SwitcherButton';
import { CodeEditor } from '../CodeEditor/CodeEditor';
import { CodeEditorLang } from '../../types/question';
import { Theme, ThemeContext } from '../../context/ThemeContext';
import { Button } from '../Button/Button';
import { ChatMessage } from '../../pages/Room/components/VideoChat/ChatMessage';
import { User } from '../../types/user';

interface QuestionAnswerDetailsProps {
  roomId: string;
  questionId: string;
  questionTitle: string;
  allUsers: Map<User['id'], Pick<User, 'nickname' | 'avatar'>>;
}

export const QuestionAnswerDetails: FunctionComponent<QuestionAnswerDetailsProps> = ({
  roomId,
  questionId,
  questionTitle,
  allUsers,
}) => {
  const localizationCaptions = useLocalizationCaptions();
  const { themeInUi } = useContext(ThemeContext);
  const { apiMethodState, fetchData } = useApiMethod<RoomQuestionAnswer, GetAnswerParams>(roomQuestionApiDeclaration.getAnswer);
  const { process: { loading, error }, data } = apiMethodState;
  const [codeQuestionTab, setCodeQuestionTab] = useState<0 | 1>(0);
  const answerCodeEditorContent = data?.details[data?.details.length - 1]?.answerCodeEditorContent;
  const hasTranscriptions = !!(data?.details.some(details => !!details.transcription.length));
  const hasCodeEditorContent = !!(typeof data?.codeEditor === 'string');
  const codeEditorValue = (codeQuestionTab === 0) && hasCodeEditorContent ?
    data?.codeEditor?.content || '' :
    answerCodeEditorContent;

  useEffect(() => {
    if (!roomId || !questionId) {
      return;
    }
    fetchData({
      roomId,
      questionId,
    });
  }, [roomId, questionId, fetchData]);

  if (loading) {
    return (
      <Loader />
    );
  }

  if (error) {
    return (
      <Typography error size='m'>{error}</Typography>
    );
  }

  return (
    <div className='text-left flex flex-col'>
      {(data?.codeEditor || answerCodeEditorContent) && (
        <>
          <Typography size='xl' bold>
            {questionTitle}
          </Typography>
          <Gap sizeRem={2.25} />
          {hasCodeEditorContent ? (
            <>
              <SwitcherButton
                items={[
                  {
                    id: 1,
                    content: localizationCaptions[LocalizationKey.QuestionCode],
                  },
                  {
                    id: 2,
                    content: localizationCaptions[LocalizationKey.AnswerCode],
                  },
                ]}
                activeIndex={codeQuestionTab}
                {...(themeInUi === Theme.Dark && {
                  variant: 'alternative',
                })}
                onClick={setCodeQuestionTab}
              />
            </>
          ) : (
            <Button
              variant='invertedActive'
              className='w-fit'
            >
              {localizationCaptions[LocalizationKey.AnswerCode]}
            </Button>
          )}
          <Gap sizeRem={1} />
          <div className='h-32.25'>
            <CodeEditor
              language={data.codeEditor?.lang || CodeEditorLang.Plaintext}
              languages={[data.codeEditor?.lang || CodeEditorLang.Plaintext]}
              readOnly
              alwaysConsumeMouseWheel={false}
              scrollBeyondLastLine={false}
              value={codeEditorValue}
            />
          </div>
          <Gap sizeRem={2.25} />
          <Typography size='m' bold>
            {localizationCaptions[LocalizationKey.QurstionTranscription]}
          </Typography>
          <Gap sizeRem={1} />
        </>
      )}
      <div className='flex flex-col'>
        {!hasTranscriptions && (
          <div className='text-center'>
            <Typography size='m' secondary>
              {localizationCaptions[LocalizationKey.NoData]}
            </Typography>
            <Gap sizeRem={1.5} />
          </div>
        )}
        {data?.details.map(detail => detail.transcription.map((transcription, index, allTranscriptions) => (
          <ChatMessage
            key={transcription.id}
            createdAt={transcription.createdAt}
            message={transcription.payload}
            nickname={transcription.user.nickname}
            avatar={allUsers.get(transcription.user.id)?.avatar}
            removePaggingTop={index === 0}
            stackWithPrevious={allTranscriptions[index - 1]?.user.id === transcription.user.id}
          />
        )))}
      </div>
    </div>
  );
};
