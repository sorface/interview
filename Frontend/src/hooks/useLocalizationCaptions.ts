import { useContext } from 'react';
import { LocalizationCaptions } from '../localization';
import { LocalizationContext } from '../context/LocalizationContext';

export const useLocalizationCaptions = () => {
  const { lang } = useContext(LocalizationContext);

  return LocalizationCaptions[lang];
};
