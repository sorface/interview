import { FunctionComponent, useContext } from "react";
import { IconNames, IconThemePostfix } from "../../../../constants";
import { Theme, ThemeContext } from "../../../../context/ThemeContext";

interface ThemedIconProps {
  name: IconNames;
  size?: 'small' | 'large';
}

export const ThemedIcon: FunctionComponent<ThemedIconProps> = ({
  name,
  size,
}) => {
  const { themeInUi } = useContext(ThemeContext);
  const iconPostfix = themeInUi === Theme.Dark ? IconThemePostfix.Dark : IconThemePostfix.Light;
  return (
    <ion-icon name={`${name}${iconPostfix}`} size={size}></ion-icon>
  );
};
