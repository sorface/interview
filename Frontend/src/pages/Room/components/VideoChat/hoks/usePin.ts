import { useState } from 'react';

export const usePin = (order?: number) => {
  const [pin, setPin] = useState(false);

  const handlePin = () => setPin((prev) => !prev);

  const pinOrder = pin && -100;
  const defaultOrder = order || 2;
  const orderSafe = pinOrder || defaultOrder;

  return { pin, handlePin, orderSafe };
};
