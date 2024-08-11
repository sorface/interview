import { FunctionComponent } from 'react';
import { Typography } from '../Typography/Typography';
import { useThemeClassName } from '../../hooks/useThemeClassName';
import { Theme } from '../../context/ThemeContext';

interface CalendarDayProps {
  day: Date | null;
  currentDate: Date;
  filledItemsStartDates: Array<number | undefined>;
}

export const CalendarDay: FunctionComponent<CalendarDayProps> = ({
  day,
  currentDate,
  filledItemsStartDates,
}) => {
  const filledThemedClassName = useThemeClassName({
    [Theme.Dark]: 'bg-dark-blue-0.25',
    [Theme.Light]: 'bg-blue-light',
  });
  const currentDayThemedClassName = useThemeClassName({
    [Theme.Dark]: 'bg-blue-dark text-white',
    [Theme.Light]: 'bg-blue-main text-white',
  });
  const filledClassName = filledItemsStartDates.includes(day?.valueOf()) ? filledThemedClassName : '';
  const currentDayClassName = day?.valueOf() === currentDate.valueOf() ? currentDayThemedClassName : '';

  return (
    <div className={`flex items-center justify-center w-1.875 h-1.875 rounded-full ${filledClassName} ${currentDayClassName}`}>
      <Typography size='m'>
        {day?.getDate() || ''}
      </Typography>
    </div>
  );
};
