import { Dispatch, FunctionComponent, ReactNode, SetStateAction, createContext, useEffect, useState } from 'react'

export enum Theme {
  System = 'System',
  Light = 'Light',
  Dark = 'Dark',
};

export type ThemeInUi = Theme.Dark | Theme.Light;

const defaultTheme = Theme.System;

interface ThemeContextType {
  themeInSetting: Theme;
  themeInUi: ThemeInUi;
  setTheme: Dispatch<SetStateAction<Theme>>;
}

const localStorageKey = 'theme';

const readFromStorage = () =>
  localStorage.getItem(localStorageKey);

const saveToStorage = (theme: Theme) =>
  localStorage.setItem(localStorageKey, String(theme));

const validateTheme = (theme: string | null) => {
  if (theme && Object.values(Theme).includes(theme as Theme)) {
    return theme as Theme;
  }
  return null;
}

const getThemeInSetting = (): Theme => {
  const themeFromStorage = readFromStorage();
  const validTheme = validateTheme(themeFromStorage);
  if (validTheme) {
    return validTheme;
  }

  return defaultTheme;
};

const darkMediaQuery = '(prefers-color-scheme: dark)';

export const getThemeInUi = () => {
  const matchMediaDark = window.matchMedia(darkMediaQuery);
  if (matchMediaDark.matches) {
    return Theme.Dark;
  }
  return Theme.Light;
};

const setDomUiTheme = (theme: Theme) =>
  document.documentElement.dataset.theme = theme;

export const initThemeInUi = (fastApply: boolean) => {
  const themeInSetting = getThemeInSetting();
  const themeInUi = themeInSetting === Theme.System ? getThemeInUi() : themeInSetting;
  if (fastApply) {
    setDomUiTheme(themeInUi);
  }
  return themeInUi;
};

export const ThemeContext = createContext<ThemeContextType>({
  themeInSetting: getThemeInSetting(),
  themeInUi: initThemeInUi(false),
  setTheme: () => { },
});

interface ThemeProviderProps {
  children: ReactNode;
}

export const ThemeProvider: FunctionComponent<ThemeProviderProps> = ({
  children,
}) => {
  const [themeInSetting, setThemeInSetting] = useState<Theme>(getThemeInSetting);
  const [themeInUi, setThemeInUi] = useState<ThemeInUi>(initThemeInUi(true));

  useEffect(() => {
    const timeoutId = setTimeout(() => {
      document.documentElement.dataset.colorTransition = 'enabled';
    }, 500);

    return () => {
      clearTimeout(timeoutId);
    }
  }, []);

  useEffect(() => {
    if (themeInSetting === Theme.System) {
      return;
    }
    setThemeInUi(themeInSetting);
  }, [themeInSetting]);

  useEffect(() => {
    setDomUiTheme(themeInUi);
  }, [themeInUi]);

  useEffect(() => {
    saveToStorage(themeInSetting);
    if (themeInSetting !== Theme.System) {
      setThemeInSetting(themeInSetting);
      return;
    }

    const handleMatches = (matches: boolean) => {
      if (matches) {
        setThemeInUi(Theme.Dark);
      } else {
        setThemeInUi(Theme.Light);
      }
    };

    const matchMediaDark = window.matchMedia(darkMediaQuery);
    handleMatches(matchMediaDark.matches);
    const handleMatchMediaChange = ({ matches }: MediaQueryListEvent) => {
      handleMatches(matches);
    };

    matchMediaDark.addEventListener('change', handleMatchMediaChange);
    return () => {
      matchMediaDark.removeEventListener('change', handleMatchMediaChange);
    };
  }, [themeInSetting]);

  return (
    <ThemeContext.Provider value={{ themeInSetting, themeInUi, setTheme: setThemeInSetting }}>
      {children}
    </ThemeContext.Provider>
  )
};
