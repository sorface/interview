import React, { FunctionComponent, useContext } from 'react';
import { Typography } from '../Typography/Typography';
import { useThemeClassName } from '../../hooks/useThemeClassName';
import { Theme } from '../../context/ThemeContext';
import { Icon } from '../../pages/Room/components/Icon/Icon';
import { IconNames } from '../../constants';
import { LocalizationContext } from '../../context/LocalizationContext';
import { getMonthName } from '../../utils/getMonthName';

interface CalendarDayProps {
  day: Date | null;
  monthStartDate: Date;
  currentDate: Date;
  selectedDay: Date | null;
  filledItemsStartDates: Array<number | undefined>;
  onClick: () => void;
}

export const CalendarDay: FunctionComponent<CalendarDayProps> = ({
  day,
  monthStartDate,
  currentDate,
  selectedDay,
  filledItemsStartDates,
  onClick,
}) => {
  const { lang } = useContext(LocalizationContext);
  const notCurrentMonth = day?.getMonth() !== monthStartDate.getMonth();
  const filledThemedClassName = useThemeClassName({
    [Theme.Dark]: 'bg-dark-dark-button',
    [Theme.Light]: 'bg-blue-light',
  });
  const notCurrentMonthThemedClassName = useThemeClassName({
    [Theme.Dark]: 'text-grey3 bg-dark-dark-button',
    [Theme.Light]: 'text-grey3 bg-grey-active',
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
  const currentDay = day?.valueOf() === currentDate.valueOf();
  const currentDayClassName = currentDay ? currentDayThemedClassName : '';
  const selectedDayClassName = selected
    ? 'border border-solid border-button-border'
    : '';
  const notCurrentMonthClassName = notCurrentMonth
    ? notCurrentMonthThemedClassName
    : '';

  return (
    <>
      <div
        className={`relative flex items-center justify-center w-[2.5rem] h-[2.5rem] p-[0.3125rem] ${notCurrentMonth ? '' : 'rounded-full'} cursor-pointer box-border ${notCurrentMonthClassName}`}
        onClick={onClick}
      >
        {day?.getDate() === 1 && !currentDay && !selected && (
          <div className="absolute leading-[0rem]" style={{ top: 0 }}>
            <Typography size="xs" secondary>
              {getMonthName(day, lang)}
            </Typography>
          </div>
        )}
        <div
          className={`w-full h-full flex items-center justify-center rounded-full ${currentDayClassName || filledClassName || notCurrentMonthClassName} ${selectedDayClassName}`}
        >
          <Typography size="m">{day?.getDate() || ''}</Typography>
          {selected && (
            <div
              className={`${closeThemedClassName} absolute translate-x-[0.75rem] translate-y-[-0.75rem] w-[1rem] h-[1rem] flex items-center justify-center rounded-full`}
            >
              <Icon name={IconNames.Close} inheritFontSize size="s" />
            </div>
          )}
        </div>
      </div>
    </>
  );
};
