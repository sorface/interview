import { useEffect, useMemo } from 'react';

export const useAudioProcessedStream = (
  userAudioStream: MediaStream | null,
) => {
  const audioInstruments = useMemo(() => {
    const audioCtx = new AudioContext();
    const dest = audioCtx.createMediaStreamDestination();

    const compressor = audioCtx.createDynamicsCompressor();
    compressor.threshold.setValueAtTime(-15, audioCtx.currentTime);
    compressor.knee.setValueAtTime(25, audioCtx.currentTime);
    compressor.ratio.setValueAtTime(18, audioCtx.currentTime);
    compressor.attack.setValueAtTime(0, audioCtx.currentTime);
    compressor.release.setValueAtTime(0.25, audioCtx.currentTime);

    const filter = audioCtx.createBiquadFilter();
    filter.type = 'peaking';
    filter.frequency.setValueAtTime(2300, audioCtx.currentTime);
    filter.Q.setValueAtTime(1, audioCtx.currentTime);

    const filterHigh = audioCtx.createBiquadFilter();
    filterHigh.type = 'lowpass';
    filterHigh.frequency.setValueAtTime(7000, audioCtx.currentTime);
    filterHigh.gain.setValueAtTime(6, audioCtx.currentTime);

    compressor.connect(filter);
    filter.connect(filterHigh);
    filterHigh.connect(dest);

    return { audioCtx, dest, compressor };
  }, []);

  useEffect(() => {
    if (!userAudioStream) {
      return;
    }
    const source =
      audioInstruments.audioCtx.createMediaStreamSource(userAudioStream);
    source.connect(audioInstruments.compressor);

    return () => {
      source.disconnect(audioInstruments.compressor);
    };
  }, [audioInstruments, userAudioStream]);

  return userAudioStream ? audioInstruments.dest.stream : null;
};
