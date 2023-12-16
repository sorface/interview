import { Theme } from '../../../../../context/ThemeContext';

const colorsLight = [
  '#CC6600', // orange
  '#009933', // green
  '#6633CC', // purple
  '#CC0000', // red
];

const colorsDark = [
  '#CC9966', // orange
  '#99CC99', // green
  '#9999CC', // purple
  '#CC6666', // red
];

export const stringToColor = (str: string, themeInUi: Theme) => {
  const colors = themeInUi === Theme.Light ? colorsLight : colorsDark;
  let hash = 0;
  str.split('').forEach(char => {
    hash = char.charCodeAt(0) + ((hash << 5) - hash)
  });
  return colors[Math.abs(hash) % colors.length];
};
