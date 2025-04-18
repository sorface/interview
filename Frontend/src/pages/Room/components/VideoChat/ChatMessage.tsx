import React, { Fragment, FunctionComponent } from 'react';
import Linkify from 'linkify-react';
import { Typography } from '../../../../components/Typography/Typography';
import { useThemeClassName } from '../../../../hooks/useThemeClassName';
import { Theme } from '../../../../context/ThemeContext';
import { Gap } from '../../../../components/Gap/Gap';
import { formatTime } from '../../../../utils/formatTime';
import { UserAvatar } from '../../../../components/UserAvatar/UserAvatar';

interface ChatMessageProps {
  nickname: string;
  message: string;
  createdAt: string;
  avatar?: string;
  removePaggingTop?: boolean;
  stackWithPrevious?: boolean;
  fromCurrentUser?: boolean;
}

export const ChatMessage: FunctionComponent<ChatMessageProps> = ({
  nickname,
  message,
  createdAt,
  avatar,
  removePaggingTop,
  stackWithPrevious,
  fromCurrentUser,
}) => {
  const messageClassName = useThemeClassName({
    [Theme.Dark]: 'bg-dark-history-hover',
    [Theme.Light]: 'bg-grey1',
  });
  const currentUserMessageClassName = useThemeClassName({
    [Theme.Dark]: 'bg-dark-active rounded-br-none',
    [Theme.Light]: 'bg-blue-light rounded-br-none',
  });

  return (
    <Fragment>
      {!removePaggingTop && <Gap sizeRem={stackWithPrevious ? 0.25 : 1} />}
      <div className="flex justify-end">
        {!fromCurrentUser && (
          <div className={`flex ${stackWithPrevious ? 'invisible' : ''}`}>
            <Typography size="xs">
              <UserAvatar nickname={nickname} src={avatar} size="xs" />
            </Typography>
            <Gap sizeRem={0.25} horizontal />
          </div>
        )}
        <div
          className={`${fromCurrentUser ? currentUserMessageClassName : messageClassName} ${fromCurrentUser ? 'max-w-[12rem]' : ''} overflow-auto flex flex-1 flex-col py-[0.25rem] px-[0.5rem] rounded-[0.5rem]`}
        >
          {!fromCurrentUser && !stackWithPrevious && (
            <div className="flex items-center">
              <Typography size="s" bold>
                {nickname}
              </Typography>
            </div>
          )}
          <Gap sizeRem={0.25} />
          <Typography size="s">
            <Linkify options={{ target: '_blank', rel: 'noopener noreferrer' }}>
              {message}
            </Linkify>
          </Typography>
          <div className="text-right">
            <Typography size="xs" secondary>
              {formatTime(new Date(createdAt))}
            </Typography>
          </div>
        </div>
      </div>
    </Fragment>
  );
};
