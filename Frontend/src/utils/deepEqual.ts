export const deepEqual = (obj1: unknown, obj2: unknown) => {
  if (obj1 === obj2) return true;

  if (
    obj1 === null ||
    obj2 === null ||
    typeof obj1 !== 'object' ||
    typeof obj2 !== 'object'
  ) {
    return false;
  }

  const firstObj = obj1 as Record<string, unknown>;
  const secondObj = obj2 as Record<string, unknown>;

  const keys1 = Object.keys(firstObj);
  const keys2 = Object.keys(secondObj);

  if (keys1.length !== keys2.length) return false;

  for (const key of keys1) {
    if (!keys2.includes(key)) return false;
    if (!deepEqual(firstObj[key], secondObj[key])) return false;
  }

  return true;
};
