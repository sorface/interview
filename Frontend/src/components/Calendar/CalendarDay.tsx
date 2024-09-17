import { FunctionComponent } from 'react';
import { Typography } from '../Typography/Typography';
import { useThemeClassName } from '../../hooks/useThemeClassName';
import { Theme } from '../../context/ThemeContext';

interface CalendarDayProps {
  day: Date | null;
  currentDate: Date;
  selectedDay: Date | null;
  filledItemsStartDates: Array<number | undefined>;
  onClick: () => void;
}

export const CalendarDay: FunctionComponent<CalendarDayProps> = ({
  day,
  currentDate,
  selectedDay,
  filledItemsStartDates,
  onClick,
}) => {
  const filledThemedClassName = useThemeClassName({
    [Theme.Dark]: 'bg-dark-grey4',
    [Theme.Light]: 'bg-blue-light',
  });
  const currentDayThemedClassName = useThemeClassName({
    [Theme.Dark]: 'bg-blue-dark text-white',
    [Theme.Light]: 'bg-blue-main text-white',
  });
  const filledClassName = filledItemsStartDates.includes(day?.valueOf()) ? filledThemedClassName : '';
  const currentDayClassName = day?.valueOf() === currentDate.valueOf() ? currentDayThemedClassName : '';
  const selectedDayClassName = selectedDay && (day?.valueOf() === selectedDay?.valueOf()) ? 'border border-solid border-button-border' : '';

  return (
    <div
      className={`flex items-center justify-center w-1.875 h-1.875 rounded-full cursor-pointer box-border ${currentDayClassName || filledClassName} ${selectedDayClassName}`}
      onClick={onClick}
    >
      <Typography size='m'>
        {day?.getDate() || ''}
      </Typography>
    </div>
  );
};
