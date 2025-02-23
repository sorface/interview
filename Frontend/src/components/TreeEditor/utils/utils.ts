export const key = (...values: (string | number)[]): string => {
  return values.map((val) => `${val}`).join('-');
};

export const uuid = (): string => {
  return crypto.randomUUID();
};
