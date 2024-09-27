import { FunctionComponent } from 'react';
import { useThemeClassName } from '../../../../hooks/useThemeClassName';
import { Theme } from '../../../../context/ThemeContext';
import { Typography } from '../../../../components/Typography/Typography';

interface UnreadChatMessagesCounterProps {
  value: number | string;
}

export const UnreadChatMessagesCounter: FunctionComponent<UnreadChatMessagesCounterProps> = ({
  value,
}) => {
  const themedClassName = useThemeClassName({
    [Theme.Dark]: 'bg-dark-red',
    [Theme.Light]: 'bg-red text-white',
  });
  return (
    <div className={`${themedClassName} flex items-center justify-center w-0.75 h-0.75 p-0.25 rounded-0.75`}>
      <Typography size='s'>
        {value}
      </Typography>
    </div>
  );
};
