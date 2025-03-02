import React, { FunctionComponent, ReactNode } from 'react';
import { Typography } from '../../../../components/Typography/Typography';
import { useThemeClassName } from '../../../../hooks/useThemeClassName';
import { Theme } from '../../../../context/ThemeContext';
import { Gap } from '../../../../components/Gap/Gap';

export interface ChatMessageAiProps {
  nickname: string;
  message: string;
  fromAi: boolean;
  children?: ReactNode;
  removePaggingTop?: boolean;
  stackWithPrevious?: boolean;
}

export const ChatMessageAi: FunctionComponent<ChatMessageAiProps> = ({
  nickname,
  message,
  fromAi,
  children,
  removePaggingTop,
  stackWithPrevious,
}) => {
  const messageClassName = useThemeClassName({
    [Theme.Dark]: 'bg-dark-history-hover',
    [Theme.Light]: 'bg-blue-light',
  });
  const currentUserMessageClassName = useThemeClassName({
    [Theme.Dark]: 'bg-dark-active rounded-br-0',
    [Theme.Light]: 'bg-white rounded-br-0',
  });

  return (
    <div className={`flex flex-col ${fromAi ? '' : 'items-end'}`}>
      {!removePaggingTop && <Gap sizeRem={stackWithPrevious ? 0.25 : 1} />}
      {!stackWithPrevious && (
        <>
          <div className="flex items-center">
            <Typography size="xl">{nickname}</Typography>
          </div>
          <Gap sizeRem={0.375} />
        </>
      )}
      <div className={`flex ${children ? '' : 'w-fit'}`}>
        <div
          className={`${fromAi ? messageClassName : currentUserMessageClassName} overflow-auto flex-1 text-left flex flex-col px-0.5 rounded-0.5`}
        >
          <Gap sizeRem={0.5} />
          {children || <Typography size="xxl">{message}</Typography>}
          <Gap sizeRem={0.5} />
        </div>
      </div>
    </div>
  );
};
