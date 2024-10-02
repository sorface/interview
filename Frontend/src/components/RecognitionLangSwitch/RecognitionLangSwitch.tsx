import { ChangeEventHandler, FunctionComponent, useContext } from 'react';
import { LocalizationKey } from '../../localization';
import { LocalizationContext, LocalizationLang } from '../../context/LocalizationContext';
import { LocalizationCaption } from '../LocalizationCaption/LocalizationCaption';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { Typography } from '../Typography/Typography';

export const RecognitionLangSwitch: FunctionComponent = () => {
  const { recognitionLang, setRecognitionLang } = useContext(LocalizationContext);
  const localizationCaptions = useLocalizationCaptions();

  const langLocalization = {
    [LocalizationLang.en]: localizationCaptions[LocalizationKey.LocalizationLangEn],
    [LocalizationLang.ru]: localizationCaptions[LocalizationKey.LocalizationLangRu],
  };

  const handleRecognitionLangChange: ChangeEventHandler<HTMLSelectElement> = (e) => {
    setRecognitionLang(e.target.value as LocalizationLang);
  };

  return (
    <>
      <div className='text-left flex items-center'>
        <Typography size='m'><LocalizationCaption captionKey={LocalizationKey.RecognitionLanguage} />:</Typography>
      </div>
      <select className='w-full' value={recognitionLang} onChange={handleRecognitionLangChange}>
        {Object.entries(LocalizationLang)?.map(([_, langValue]) => (
          <option key={langValue} value={langValue}>{langLocalization[langValue]}</option>
        ))}
      </select>
    </>
  )
};
