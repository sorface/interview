import { ChangeEventHandler, useContext } from 'react';
import { LocalizationKey } from '../../localization';
import {
  LocalizationContext,
  LocalizationLang,
} from '../../context/LocalizationContext';
import { LocalizationCaption } from '../LocalizationCaption/LocalizationCaption';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { Typography } from '../Typography/Typography';
import {
  SwitcherButton,
  SwitcherButtonContent,
} from '../SwitcherButton/SwitcherButton';

interface LangSwitchProps {
  elementType?: 'switcherButton' | 'select';
}

export const LangSwitch = ({
  elementType = 'select',
}: LangSwitchProps): JSX.Element => {
  const { lang, setLang } = useContext(LocalizationContext);
  const localizationCaptions = useLocalizationCaptions();

  const langLocalization = {
    [LocalizationLang.en]:
      localizationCaptions[LocalizationKey.LocalizationLangEn],
    [LocalizationLang.ru]:
      localizationCaptions[LocalizationKey.LocalizationLangRu],
  };

  const handleLangChange: ChangeEventHandler<HTMLSelectElement> = (e) => {
    setLang(e.target.value as LocalizationLang);
  };

  const select = (
    <>
      <div className="text-left flex items-center">
        <Typography size="m">
          <LocalizationCaption captionKey={LocalizationKey.Language} />:
        </Typography>
      </div>
      <select className="w-full" value={lang} onChange={handleLangChange}>
        {Object.entries(LocalizationLang)?.map(([_, langValue]) => (
          <option key={langValue} value={langValue}>
            {langLocalization[langValue]}
          </option>
        ))}
      </select>
    </>
  );

  const switcherButton = () => {
    const items: [SwitcherButtonContent, SwitcherButtonContent] = [
      {
        id: LocalizationLang.en,
        content: LocalizationLang.en.toLocaleUpperCase(),
      },
      {
        id: LocalizationLang.ru,
        content: LocalizationLang.ru.toLocaleUpperCase(),
      },
    ];

    return (
      <SwitcherButton
        items={items}
        activeIndex={items.findIndex((i) => i.id === lang) as 0 | 1}
        variant="alternative"
        onClick={(activeIndex) =>
          setLang(items[activeIndex].id as LocalizationLang)
        }
      />
    );
  };

  const elements = {
    select: select,
    switcherButton: switcherButton(),
  };

  return elements[elementType];
};
