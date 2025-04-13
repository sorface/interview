import { Theme } from '../context/ThemeContext';
import { useThemeClassName } from './useThemeClassName';

export const useThemedAiAvatar = () => {
  const themedAiAvatar = useThemeClassName({
    [Theme.Dark]: '/ai-dark.png',
    [Theme.Light]: '/ai-white.png',
  });

  return themedAiAvatar;
};
