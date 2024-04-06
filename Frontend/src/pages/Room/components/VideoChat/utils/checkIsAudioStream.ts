export const checkIsAudioStream = (stream: MediaStream): boolean => {
  return !!stream.getAudioTracks().length;
};
