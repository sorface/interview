import { FunctionComponent, useContext } from 'react';
import { Theme, ThemeContext } from '../../context/ThemeContext';
import { IconNames } from '../../constants';
import { ThemedIcon } from '../../pages/Room/components/ThemedIcon/ThemedIcon';

import './ThemeSwitchMini.css';

interface ThemeSwitchMiniProps {
  className?: string | null;
}

const getNextTheme = (themeInUi: Theme) =>
themeInUi === Theme.Light ? Theme.Dark : Theme.Light;

export const ThemeSwitchMini: FunctionComponent<ThemeSwitchMiniProps> = ({
  className,
}) => {
  const { themeInUi, setTheme } = useContext(ThemeContext);
  const iconName = themeInUi === Theme.Light ? IconNames.ThemeSwitchDark : IconNames.ThemeSwitchLight;

  const handleSwitch = () => {
    setTheme(getNextTheme(themeInUi));
  }

  return (
    <div
      className={`theme-switch-mini ${className}`}
      onClick={handleSwitch}
    >
      <ThemedIcon name={iconName} />
    </div>
  );
};
