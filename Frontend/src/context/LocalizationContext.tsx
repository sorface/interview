import { Dispatch, FunctionComponent, ReactNode, SetStateAction, createContext, useEffect, useState } from 'react'

export enum LocalizationLang {
  ru = 'ru',
  en = 'en',
};

const defaultLang = LocalizationLang.en;

interface LocalizationContextType {
  lang: LocalizationLang;
  recognitionLang: LocalizationLang;
  setLang: Dispatch<SetStateAction<LocalizationLang>>;
  setRecognitionLang: Dispatch<SetStateAction<LocalizationLang>>;
}

const localizationLocalStorageKey = 'localization';
const recognitionLocalStorageKey = 'recognition';

const readFromStorage = (key: string) =>
  localStorage.getItem(key);

const saveToStorage = (key: string, lang: LocalizationLang) =>
  localStorage.setItem(key, String(lang));

const validateLang = (lang: string | null) => {
  if (lang && Object.values(LocalizationLang).includes(lang as LocalizationLang)) {
    return lang as LocalizationLang;
  }
  return null;
}

const getLangInSetting = (key: string): LocalizationLang | null => {
  const langeFromStorage = readFromStorage(key);
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
  lang: getLangInSetting(localizationLocalStorageKey) || getLangInBrowser(),
  recognitionLang: getLangInSetting(recognitionLocalStorageKey) || getLangInBrowser(),
  setLang: () => { },
  setRecognitionLang: () => { },
});

interface LocalizationProviderProps {
  children: ReactNode;
}

export const LocalizationProvider: FunctionComponent<LocalizationProviderProps> = ({
  children,
}) => {
  const [lang, setLang] = useState<LocalizationLang>(getLangInSetting(localizationLocalStorageKey) || getLangInBrowser());
  const [recognitionLang, setRecognitionLang] = useState<LocalizationLang>(getLangInSetting(recognitionLocalStorageKey) || getLangInBrowser());

  useEffect(() => {
    saveToStorage(localizationLocalStorageKey, lang);
  }, [lang]);

  useEffect(() => {
    saveToStorage(recognitionLocalStorageKey, recognitionLang);
  }, [recognitionLang]);

  return (
    <LocalizationContext.Provider value={{ lang, recognitionLang, setLang, setRecognitionLang }}>
      {children}
    </LocalizationContext.Provider>
  )
};
