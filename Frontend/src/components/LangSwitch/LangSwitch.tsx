import React, { ChangeEventHandler, useContext } from 'react';
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
import { Button } from '../Button/Button';

interface LangSwitchProps {
  elementType?: 'switcherButton' | 'select' | 'button';
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
      <select className="w-full muted" value={lang} onChange={handleLangChange}>
        {Object.entries(LocalizationLang)?.map(([, langValue]) => (
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
        activeVariant="invertedActive"
        nonActiveVariant="invertedAlternative"
        onClick={(activeIndex) =>
          setLang(items[activeIndex].id as LocalizationLang)
        }
      />
    );
  };

  const button = () => {
    return (
      <Button
        variant="invertedAlternative"
        className="min-w-[0rem] w-[2.375rem] h-[2.375rem] p-[0rem]"
        onClick={() =>
          setLang(
            lang === LocalizationLang.ru
              ? LocalizationLang.en
              : LocalizationLang.ru,
          )
        }
      >
        <Typography size="s" semibold>
          {lang === LocalizationLang.ru ? 'EN' : 'RU'}
        </Typography>
      </Button>
    );
  };

  const elements = {
    select: select,
    switcherButton: switcherButton(),
    button: button(),
  };

  return elements[elementType];
};
