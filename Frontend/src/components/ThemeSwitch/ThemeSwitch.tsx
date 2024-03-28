import { FunctionComponent, useContext } from 'react';
import { Theme, ThemeContext } from '../../context/ThemeContext';
import { LocalizationKey } from '../../localization';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';

export const ThemeSwitch: FunctionComponent = () => {
  const { themeInSetting, setTheme } = useContext(ThemeContext);
  const localizationCaptions = useLocalizationCaptions();
  const themeLocalization: Record<Theme, string> = {
    [Theme.System]: localizationCaptions[LocalizationKey.ThemeSystem],
    [Theme.Light]: localizationCaptions[LocalizationKey.ThemeLight],
    [Theme.Dark]: localizationCaptions[LocalizationKey.ThemeDark],
  };

  return (
    <div className='setting-switch'>
      <div>{localizationCaptions[LocalizationKey.Theme]}:</div>
      {Object.entries(Theme).map(([_, themeValue]) => (
        <div key={themeValue}>
          <input
            type="checkbox"
            id={themeValue}
            checked={themeInSetting === themeValue}
            onChange={() => setTheme(themeValue)}
          />
          <label htmlFor={themeValue}>{themeLocalization[themeValue]}</label>
        </div>
      ))}
    </div>
  )
};
