import { Fragment, FunctionComponent } from 'react';
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
}

export const ChatMessage: FunctionComponent<ChatMessageProps> = ({
  nickname,
  message,
  createdAt,
  avatar,
}) => {
  const messageClassName = useThemeClassName({
    [Theme.Dark]: 'bg-dark-history-hover',
    [Theme.Light]: 'bg-grey1',
  });

  return (
    <Fragment>
      <div
        className={`${messageClassName} flex flex-col py-0.25 px-0.5 rounded-0.5`}
      >
        <div className='flex items-center'>
          <Typography size='xs'>
            <UserAvatar nickname={nickname} src={avatar} size='xs' />
          </Typography>
          <Gap sizeRem={0.25} horizontal />
          <Typography size='s' bold>{nickname}</Typography>
        </div>
        <Gap sizeRem={0.25} />
        <Typography size='s'>{message}</Typography>
        <div className='text-right'>
          <Typography size='xs' secondary>{formatTime(new Date(createdAt))}</Typography>
        </div>
      </div>
      <Gap sizeRem={0.25} />
    </Fragment>
  );
};
