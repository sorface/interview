import { ComponentProps, FunctionComponent } from 'react';
import { Tooltip as ReactTooltip } from 'react-tooltip';
import { useThemeClassName } from '../../hooks/useThemeClassName';
import { Theme } from '../../context/ThemeContext';

type TooltipProps = ComponentProps<typeof ReactTooltip>;

export const Tooltip: FunctionComponent<TooltipProps> = (props) => {
  const tooltipThemedBackground = useThemeClassName({
    [Theme.Dark]: 'var(--dark-dark1)',
    [Theme.Light]: 'var(--dark)',
  });

  return (
    <ReactTooltip
      style={{ backgroundColor: tooltipThemedBackground, zIndex: 999 }}
      {...props}
    />
  );
};
