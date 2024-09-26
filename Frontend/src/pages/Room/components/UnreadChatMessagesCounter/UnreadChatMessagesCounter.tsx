import { FunctionComponent } from 'react';
import { useThemeClassName } from '../../../../hooks/useThemeClassName';
import { Theme } from '../../../../context/ThemeContext';

interface UnreadChatMessagesCounterProps {
  value: number;
}

export const UnreadChatMessagesCounter: FunctionComponent<UnreadChatMessagesCounterProps> = ({
  value,
}) => {
  const themedClassName = useThemeClassName({
    [Theme.Dark]: 'bg-dark-active',
    [Theme.Light]: 'bg-grey2',
  });
  return (
    <div className={`${themedClassName} w-1 h-1 p-0.25 rounded-0.75`}>
      {value}
    </div>
  );
};
