import { Fragment, FunctionComponent, useContext } from 'react';
import { LocalizationContext } from '../../context/LocalizationContext';
import { CalendarDay } from './CalendarDay';
import { Typography } from '../Typography/Typography';
import { Gap } from '../Gap/Gap';
import { Icon } from '../../pages/Room/components/Icon/Icon';
import { IconNames } from '../../constants';
import { useThemeClassName } from '../../hooks/useThemeClassName';
import { Theme } from '../../context/ThemeContext';

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

const getMonthName = (monthStartDate: Date, locale: string) => {
  return monthStartDate.toLocaleDateString(locale, { month: 'long' })
};

const getDaysInMonth = (monthStartDate: Date) => {
  return new Date(monthStartDate.getFullYear(), monthStartDate.getMonth() + 1, 0).getDate();
}

const getWeekDays = (locale: string) => {
  const baseDate = new Date(Date.UTC(2017, 0, 2));
  const weekDays = [];
  for (let i = 0; i < 7; i++) {
    weekDays.push(baseDate.toLocaleDateString(locale, { weekday: 'short' }));
    baseDate.setDate(baseDate.getDate() + 1);
  }
  return weekDays;
}

const chunkArray = <T extends any>(array: T[], chunkSize: number) => {
  const chunks: T[][] = [];
  for (let i = 0; i < array.length; i += chunkSize) {
    chunks.push(array.slice(i, i + chunkSize));
  }
  return chunks;
};

const unshiftNull = <T extends any>(array: T[], count: number) => {
  const nullArray = Array.from({ length: count }).map(() => null);
  return [...nullArray, ...array];
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
  const startMonthShift = monthStartDate.getDay() === 0 ? 6 : monthStartDate.getDay() - 1;
  const daysInMonth =
    Array.from({ length: getDaysInMonth(monthStartDate) })
      .map((_, index) => {
        const date = new Date(monthStartDate);
        date.setDate(index + 1);
        return date;
      });
  const unshiftedDaysInMonth = unshiftNull(daysInMonth, startMonthShift);
  const daysChunks = chunkArray(unshiftedDaysInMonth, 7);
  const filledItemsStartDates: Array<number | undefined> = filledItems.map(filledItem => {
    const date = new Date(filledItem.getFullYear(), filledItem.getMonth(), filledItem.getDate());
    return date.valueOf();
  });

  return (
    <div className='w-fit h-fit p-0.25 select-none bg-wrap rounded-1.125'>
      <div className='capitalize flex justify-between px-0.5 py-0.375 h-2 items-center'>
        <div className='cursor-pointer opacity-0.5' onClick={onMonthBackClick}>
          <Icon name={IconNames.ChevronBack} size='s' />
        </div>
        <Typography size='l' bold>
          {getMonthName(monthStartDate, lang)}
        </Typography>
        <div className='cursor-pointer opacity-0.5' onClick={onMonthForwardClick}>
          <Icon name={IconNames.ChevronForward} size='s' />
        </div>
      </div>
      <div className={`flex items-center h-1.375 border-b-1 border-b-solid ${daysThemedClassName}`}>
        {days.map((day, dayIndex) => (
          <Fragment key={day}>
            <div className='capitalize w-1.875'>
              <Typography size='xs'>
                {day}
              </Typography>
            </div>
            {dayIndex !== days.length - 1 && (<Gap sizeRem={0.625} horizontal />)}
          </Fragment>
        ))}
      </div>
      <Gap sizeRem={0.25} />
      {daysChunks.map((daysChunk, index) => (
        <Fragment key={`daysChunk${index}`}>
          <div className={`flex ${loading ? 'opacity-0.5' : ''}`}>
            {daysChunk.map((day, dayIndex) => (
              <Fragment key={day ? day.valueOf() : `null-day${dayIndex}`}>
                <CalendarDay
                  day={day}
                  currentDate={currentDate}
                  selectedDay={selectedDay}
                  filledItemsStartDates={filledItemsStartDates}
                  onClick={() => day && onDayClick(day)}
                />
                {dayIndex !== daysChunk.length - 1 && (<Gap sizeRem={0.625} horizontal />)}
              </Fragment>
            ))}
          </div>
          {index !== daysChunks.length - 1 && (<Gap sizeRem={0.625} />)}
        </Fragment>
      ))}
    </div>
  );
};
