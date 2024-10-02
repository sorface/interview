import { ChangeEventHandler, FunctionComponent, useContext } from 'react';
import { Theme, ThemeContext } from '../../context/ThemeContext';
import { LocalizationKey } from '../../localization';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { Typography } from '../Typography/Typography';

export const ThemeSwitch: FunctionComponent = () => {
  const { themeInSetting, setTheme } = useContext(ThemeContext);
  const localizationCaptions = useLocalizationCaptions();
  const themeLocalization: Record<Theme, string> = {
    [Theme.System]: localizationCaptions[LocalizationKey.ThemeSystem],
    [Theme.Light]: localizationCaptions[LocalizationKey.ThemeLight],
    [Theme.Dark]: localizationCaptions[LocalizationKey.ThemeDark],
  };

  const handleThemeChange: ChangeEventHandler<HTMLSelectElement> = (e) => {
    setTheme(e.target.value as Theme);
  };

  return (
    <>
      <div className='text-left flex items-center'>
        <Typography size='m'>{localizationCaptions[LocalizationKey.Theme]}:</Typography>
      </div>
      <select className='w-full' value={themeInSetting} onChange={handleThemeChange}>
        {Object.entries(Theme)?.map(([_, themeValue]) => (
          <option key={themeValue} value={themeValue}>{themeLocalization[themeValue]}</option>
        ))}
      </select>
    </>
  )
};
