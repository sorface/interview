export const getMonthName = (monthStartDate: Date, locale: string) => {
  return monthStartDate.toLocaleDateString(locale, { month: 'long' });
};
