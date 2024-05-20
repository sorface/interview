import { FunctionComponent, useContext } from 'react';
import { LocalizationKey } from '../../localization';
import { LocalizationContext, LocalizationLang } from '../../context/LocalizationContext';
import { LocalizationCaption } from '../LocalizationCaption/LocalizationCaption';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';

export const LangSwitch: FunctionComponent = () => {
  const { lang, setLang } = useContext(LocalizationContext);
  const localizationCaptions = useLocalizationCaptions();

  const langLocalization = {
    [LocalizationLang.en]: localizationCaptions[LocalizationKey.LocalizationLangEn],
    [LocalizationLang.ru]: localizationCaptions[LocalizationKey.LocalizationLangRu],
  };

  return (
    <div className='setting-switch'>
      <div><LocalizationCaption captionKey={LocalizationKey.Language} />:</div>
      {Object.entries(LocalizationLang).map(([_, langValue]) => (
        <div key={langValue}>
          <input
            type="checkbox"
            id={langValue}
            checked={lang === langValue}
            onChange={() => setLang(langValue)}
          />
          <label htmlFor={langValue}>{langLocalization[langValue] || langValue}</label>
        </div>
      ))}
    </div>
  )
};
