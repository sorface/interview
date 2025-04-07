import React, { FunctionComponent, useContext } from 'react';
import { Theme, ThemeContext } from '../../context/ThemeContext';
import { ToggleSwitch } from '../ToggleSwitch/ToggleSwitch';
import { ButtonSwitch } from '../ButtonSwitch/ButtonSwitch';

import './ThemeSwitchMini.css';

interface ThemeSwitchMiniProps {
  className?: string | null;
  variant?: 'switch' | 'button';
}

const getNextTheme = (themeInUi: Theme) =>
  themeInUi === Theme.Light ? Theme.Dark : Theme.Light;

export const ThemeSwitchMini: FunctionComponent<ThemeSwitchMiniProps> = ({
  className,
  variant = 'switch',
}) => {
  const { themeInUi, setTheme } = useContext(ThemeContext);
  const toggled = themeInUi === Theme.Dark;

  const handleSwitch = () => {
    setTheme(getNextTheme(themeInUi));
  };

  const SwitchComponent = variant === 'switch' ? ToggleSwitch : ButtonSwitch;

  return (
    <div className={`theme-switch-mini ${className}`} onClick={handleSwitch}>
      <SwitchComponent toggled={toggled} onToggle={handleSwitch} />
    </div>
  );
};
