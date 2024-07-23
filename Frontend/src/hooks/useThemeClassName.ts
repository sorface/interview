import { useContext } from 'react';
import { ThemeContext, ThemeInUi } from '../context/ThemeContext';

type UseThemeClassNameProps<T> = {
  [key in ThemeInUi]: T;
};

export const useThemeClassName = <T>(props: UseThemeClassNameProps<T>) => {
  const { themeInUi } = useContext(ThemeContext);
  return props[themeInUi];
};
