import React, { FunctionComponent } from 'react';
import { Typography } from '../Typography/Typography';
import { useThemeClassName } from '../../hooks/useThemeClassName';
import { Theme } from '../../context/ThemeContext';
import { Icon } from '../../pages/Room/components/Icon/Icon';
import { IconNames } from '../../constants';

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
  const notCurrentMonth = day?.getMonth() !== currentDate.getMonth();
  const filledThemedClassName = useThemeClassName({
    [Theme.Dark]: 'bg-dark-dark-button',
    [Theme.Light]: 'bg-blue-light',
  });
  const notCurrentMonthThemedClassName = useThemeClassName({
    [Theme.Dark]: 'bg-dark-grey4',
    [Theme.Light]: 'bg-grey-active',
  });
  const currentDayThemedClassName = useThemeClassName({
    [Theme.Dark]: 'bg-blue-dark text-white',
    [Theme.Light]: 'bg-blue-main text-white',
  });
  const closeThemedClassName = useThemeClassName({
    [Theme.Dark]: 'bg-dark-red',
    [Theme.Light]: 'bg-red text-white',
  });
  const selected = selectedDay && day?.valueOf() === selectedDay?.valueOf();
  const filledClassName = filledItemsStartDates.includes(day?.valueOf())
    ? filledThemedClassName
    : '';
  const currentDayClassName =
    day?.valueOf() === currentDate.valueOf() ? currentDayThemedClassName : '';
  const selectedDayClassName = selected
    ? 'border border-solid border-button-border'
    : '';
  const notCurrentMonthClassName = notCurrentMonth
    ? notCurrentMonthThemedClassName
    : '';

  return (
    <>
      <div
        className={`flex items-center justify-center w-2.5 h-2.5 ${notCurrentMonth ? '' : 'rounded-full'} cursor-pointer box-border ${currentDayClassName || filledClassName || notCurrentMonthClassName} ${selectedDayClassName}`}
        onClick={onClick}
      >
        <Typography size="m">{day?.getDate() || ''}</Typography>
        {selected && (
          <div
            className={`${closeThemedClassName} absolute translate-x-0.75-y--0.75 w-1 h-1 flex items-center justify-center rounded-full`}
          >
            <Icon name={IconNames.Close} inheritFontSize size="s" />
          </div>
        )}
      </div>
    </>
  );
};
