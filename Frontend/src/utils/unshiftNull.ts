export const unshiftNull = <T>(array: T[], count: number) => {
  const nullArray = Array.from({ length: count }).map(() => null);
  return [...nullArray, ...array];
};
