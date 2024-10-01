import { ChangeEventHandler, FunctionComponent, useContext } from 'react';
import { LocalizationKey } from '../../localization';
import { LocalizationContext, LocalizationLang } from '../../context/LocalizationContext';
import { LocalizationCaption } from '../LocalizationCaption/LocalizationCaption';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { Typography } from '../Typography/Typography';

export const LangSwitch: FunctionComponent = () => {
  const { lang, setLang } = useContext(LocalizationContext);
  const localizationCaptions = useLocalizationCaptions();

  const langLocalization = {
    [LocalizationLang.en]: localizationCaptions[LocalizationKey.LocalizationLangEn],
    [LocalizationLang.ru]: localizationCaptions[LocalizationKey.LocalizationLangRu],
  };

  const handleLangChange: ChangeEventHandler<HTMLSelectElement> = (e) => {
    setLang(e.target.value as LocalizationLang);
  };

  return (
    <>
      <div className='text-left flex items-center'>
        <Typography size='m'><LocalizationCaption captionKey={LocalizationKey.Language} />:</Typography>
      </div>
      <select id="rootCategory" className='w-full' value={lang} onChange={handleLangChange}>
        {Object.entries(LocalizationLang)?.map(([_, langValue]) => (
          <option key={langValue} value={langValue}>{langLocalization[langValue]}</option>
        ))}
      </select>
    </>
  )
};
