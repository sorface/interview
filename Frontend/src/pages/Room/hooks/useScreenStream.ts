import { useCallback, useState } from 'react';

const handleRequestScreenStream = async () => {
  let captureStream = null;

  try {
    captureStream = await navigator.mediaDevices.getDisplayMedia({
      audio: false,
      // @ts-expect-error experimental api
      selfBrowserSurface: 'exclude',
    });
  } catch (err) {
    console.error(`requestScreenStream error: ${err}`);
  }
  return captureStream;
};

export const useScreenStream = () => {
  const [screenStream, setScreenStream] = useState<MediaStream | null>(null);

  const requestScreenStream = useCallback(async () => {
    const requestedScreenStream = await handleRequestScreenStream();
    setScreenStream(requestedScreenStream);
  }, []);

  return {
    screenStream,
    requestScreenStream,
  };
};
