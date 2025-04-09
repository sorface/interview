import React, { FunctionComponent, useContext } from 'react';
import { LocalizationContext } from '../../context/LocalizationContext';
import { CalendarDay } from './CalendarDay';
import { Typography } from '../Typography/Typography';
import { Icon } from '../../pages/Room/components/Icon/Icon';
import { IconNames } from '../../constants';
import { useThemeClassName } from '../../hooks/useThemeClassName';
import { Theme } from '../../context/ThemeContext';
import { chunkArray } from '../../utils/chunkArray';
import { getMonthName } from '../../utils/getMonthName';

interface CalendarProps {
  loading: boolean;
  monthStartDate: Date;
  currentDate: Date;
  filledItems: Date[];
  selectedDay: Date | null;
  onMonthBackClick: () => void;
  onMonthForwardClick: () => void;
  onDayClick: (day: Date) => void;
}

const getDaysInMonth = (monthStartDate: Date) => {
  return new Date(
    monthStartDate.getFullYear(),
    monthStartDate.getMonth() + 1,
    0,
  ).getDate();
};

const getWeekDays = (locale: string) => {
  const baseDate = new Date(Date.UTC(2017, 0, 2));
  const weekDays = [];
  for (let i = 0; i < 7; i++) {
    weekDays.push(baseDate.toLocaleDateString(locale, { weekday: 'short' }));
    baseDate.setDate(baseDate.getDate() + 1);
  }
  return weekDays;
};

const unshiftPrevMonth = (daysInMonth: Date[], startMonthShift: number) => {
  const result = [...daysInMonth];
  const prevMonthStartDate = new Date(daysInMonth[0]);
  prevMonthStartDate.setMonth(prevMonthStartDate.getMonth() - 1);
  const prevMonthDaysCount = getDaysInMonth(prevMonthStartDate);
  for (let i = 0; i < startMonthShift; i++) {
    const dateToUnshift = new Date(prevMonthStartDate);
    dateToUnshift.setDate(prevMonthDaysCount - i);
    result.unshift(dateToUnshift);
  }
  return result;
};

const pushNextMonth = (days: Date[]) => {
  const pushCount = 7 - (days.length % 7);
  if (pushCount === 7) {
    return days;
  }
  const result = [...days];
  const nextMonthStartDate = new Date(days[days.length - 1]);
  nextMonthStartDate.setDate(1);
  nextMonthStartDate.setMonth(nextMonthStartDate.getMonth() + 1);
  for (let i = 0; i < pushCount; i++) {
    const dateToPush = new Date(nextMonthStartDate);
    dateToPush.setDate(i + 1);
    result.push(dateToPush);
  }
  return result;
};

export const Calendar: FunctionComponent<CalendarProps> = ({
  loading,
  monthStartDate,
  currentDate,
  filledItems,
  selectedDay,
  onMonthBackClick,
  onMonthForwardClick,
  onDayClick,
}) => {
  const { lang } = useContext(LocalizationContext);
  const days = getWeekDays(lang);

  const daysThemedClassName = useThemeClassName({
    [Theme.Dark]: 'border-grey-3',
    [Theme.Light]: 'border-grey-active',
  });
  const startMonthShift =
    monthStartDate.getDay() === 0 ? 6 : monthStartDate.getDay() - 1;
  const daysInMonth = Array.from({
    length: getDaysInMonth(monthStartDate),
  }).map((_, index) => {
    const date = new Date(monthStartDate);
    date.setDate(index + 1);
    return date;
  });
  const unshiftedDaysInMonth = pushNextMonth(
    unshiftPrevMonth(daysInMonth, startMonthShift),
  );
  const daysChunks = chunkArray(unshiftedDaysInMonth, 7);
  const filledItemsStartDates: Array<number | undefined> = filledItems.map(
    (filledItem) => {
      const date = new Date(
        filledItem.getFullYear(),
        filledItem.getMonth(),
        filledItem.getDate(),
      );
      return date.valueOf();
    },
  );

  return (
    <div className="w-fit h-fit select-none bg-wrap rounded-1.125">
      <div className="capitalize flex justify-between px-0.875 py-0.25 h-2 items-center">
        <div className="cursor-pointer opacity-0.5" onClick={onMonthBackClick}>
          <Icon name={IconNames.ChevronBack} size="s" />
        </div>
        <Typography size="l" bold>
          {getMonthName(monthStartDate, lang)}
        </Typography>
        <div
          className="cursor-pointer opacity-0.5"
          onClick={onMonthForwardClick}
        >
          <Icon name={IconNames.ChevronForward} size="s" />
        </div>
      </div>
      <div
        className={`flex items-center h-1.375 border-b-1 border-b-solid ${daysThemedClassName}`}
      >
        {days.map((day) => (
          <div key={day} className="capitalize w-2.5">
            <Typography size="xs">{day}</Typography>
          </div>
        ))}
      </div>
      {daysChunks.map((daysChunk, index) => (
        <div
          key={`daysChunk${index}`}
          className={`flex ${loading ? 'opacity-0.5' : ''}`}
        >
          {daysChunk.map((day, dayIndex) => (
            <CalendarDay
              key={day ? day.valueOf() : `null-day${dayIndex}`}
              day={day}
              monthStartDate={monthStartDate}
              currentDate={currentDate}
              selectedDay={selectedDay}
              filledItemsStartDates={filledItemsStartDates}
              onClick={() => day && onDayClick(day)}
            />
          ))}
        </div>
      ))}
    </div>
  );
};
