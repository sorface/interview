const fftSize = 2048;
export const frequencyBinCount = fftSize / 2;

export const createAudioAnalyser = (stream: MediaStream): AnalyserNode => {
  const audioContext = new AudioContext();
  const analyser = audioContext.createAnalyser();
  analyser.smoothingTimeConstant = 0.7;
  analyser.fftSize = fftSize;
  const source = audioContext.createMediaStreamSource(stream);
  source.connect(analyser);
  return analyser;
};
