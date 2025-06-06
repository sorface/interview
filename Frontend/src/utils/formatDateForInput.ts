import { padTime } from './padTime';

export const formatDateForInput = (value: Date) => {
  const month = padTime(value.getMonth() + 1);
  const date = padTime(value.getDate());
  return `${value.getFullYear()}-${month}-${date}`;
};
