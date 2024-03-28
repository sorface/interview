import { Dispatch, FunctionComponent, ReactNode, SetStateAction, createContext, useEffect, useState } from 'react'

export enum LocalizationLang {
  ru = 'ru',
  en = 'en',
};

const defaultLang = LocalizationLang.en;

interface LocalizationContextType {
  lang: LocalizationLang;
  setLang: Dispatch<SetStateAction<LocalizationLang>>;
}

const localStorageKey = 'localization';

const readFromStorage = () =>
  localStorage.getItem(localStorageKey);

const saveToStorage = (lang: LocalizationLang) =>
  localStorage.setItem(localStorageKey, String(lang));

const validateLang = (lang: string | null) => {
  if (lang && Object.values(LocalizationLang).includes(lang as LocalizationLang)) {
    return lang as LocalizationLang;
  }
  return null;
}

const getLangInSetting = (): LocalizationLang | null => {
  const langeFromStorage = readFromStorage();
  const validLang = validateLang(langeFromStorage);
  if (validLang) {
    return validLang;
  }

  return null;
};

export const getLangInBrowser = () => {
  const langInBrowser = navigator.language;
  const validLang = validateLang(langInBrowser);
  if (validLang) {
    return validLang;
  }

  return defaultLang;
};

export const LocalizationContext = createContext<LocalizationContextType>({
  lang: getLangInSetting() || getLangInBrowser(),
  setLang: () => { },
});

interface LocalizationProviderProps {
  children: ReactNode;
}

export const LocalizationProvider: FunctionComponent<LocalizationProviderProps> = ({
  children,
}) => {
  const [lang, setLang] = useState<LocalizationLang>(getLangInSetting() || getLangInBrowser());

  useEffect(() => {
    saveToStorage(lang);
  }, [lang])

  return (
    <LocalizationContext.Provider value={{ lang, setLang }}>
      {children}
    </LocalizationContext.Provider>
  )
};
