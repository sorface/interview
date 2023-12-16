export const limitLength = <T>(arr: Array<T>, length: number): Array<T> => {
  const lengthSurplus = arr.length - length;
  if (lengthSurplus > 0) {
    return arr.slice(lengthSurplus);
  }
  return arr;
};
