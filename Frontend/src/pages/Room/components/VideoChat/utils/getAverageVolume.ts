export const getAverageVolume = (data: Uint8Array) => {
  const length = data.length;
  let sum = 0;
  for (let i = 0; i < length; i++) {
    sum += data[i];
  }
  return sum / length;
};
