import { useEffect, useRef, useState } from 'react';

const compareObjects = (obj1: object, obj2: object) =>
  JSON.stringify(obj1) === JSON.stringify(obj2);

export const useAccumData = <T extends object>(currentData: T[] | null) => {
  const [allData, setAllData] = useState<T[]>([]);
  const allDataRef = useRef<T[]>([]);

  useEffect(() => {
    if (!currentData || currentData.length === 0) {
      return;
    }
    const sameData = compareObjects(
      currentData[currentData.length - 1],
      allDataRef.current[allDataRef.current.length - 1]
    );
    if (sameData) {
      return;
    }
    allDataRef.current = [
      ...allDataRef.current,
      ...currentData,
    ];
    setAllData(allDataRef.current);
  }, [currentData]);

  return allData as T[];
};
