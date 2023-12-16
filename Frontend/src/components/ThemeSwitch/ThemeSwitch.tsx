import { FunctionComponent, useContext } from 'react';
import { Theme, ThemeContext } from '../../context/ThemeContext';
import { Localization } from '../../localization';

import './ThemeSwitch.css';

const themeLocalization: Record<Theme, string> = {
  [Theme.System]: Localization.ThemeSystem,
  [Theme.Light]: Localization.ThemeLight,
  [Theme.Dark]: Localization.ThemeDark,
};

export const ThemeSwitch: FunctionComponent = () => {
  const { themeInSetting, setTheme } = useContext(ThemeContext);

  return (
    <div className='theme-switch'>
      <div>{Localization.Theme}:</div>
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
