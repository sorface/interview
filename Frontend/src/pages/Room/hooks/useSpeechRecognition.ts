import { useContext, useEffect, useRef, useState } from 'react';
import { LocalizationContext } from '../../../context/LocalizationContext';

interface UseSpeechRecognitionParams {
  recognitionEnabled: boolean;
  onVoiceRecognition: (transcript: string) => void;
}

const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;

export const useSpeechRecognition = ({
  recognitionEnabled,
  onVoiceRecognition,
}: UseSpeechRecognitionParams) => {
  const { recognitionLang } = useContext(LocalizationContext);
  const recognition = useRef(SpeechRecognition ? new SpeechRecognition() : null);
  const [recognitionNotSupported, setRecognitionNotSupported] = useState(false);

  useEffect(() => {
    if (!recognition.current) {
      setRecognitionNotSupported(true);
    }
  }, []);

  useEffect(() => {
    const recog = recognition.current;
    return () => {
      recog?.stop();
    };
  }, []);

  useEffect(() => {
    const recog = recognition.current;
    if (!recog) {
      return;
    }
    recog.lang = recognitionLang;
    recog.continuous = true;
    recog.onend = () => {
      if (recognitionEnabled) {
        recog.start();
      }
    }

    return () => {
      recog.onend = null;
    }
  }, [recognitionEnabled, recognitionLang]);

  useEffect(() => {
    const recog = recognition.current;
    if (!recog) {
      return;
    }
    try {
      if (recognitionEnabled) {
        recog.stop();
      }
      recog.lang = recognitionLang;
      recog.start();
    } catch (error) {
      console.warn(error);
    }

    return () => {
      try {
        if (recognitionEnabled) {
          recog.stop();
        }
      } catch (error) {
        console.warn(error);
      }
    }
  }, [recognitionEnabled, recognitionLang]);

  useEffect(() => {
    const recog = recognition.current;
    if (!recog) {
      return;
    }
    recog.onresult = (event) => {
      for (let i = event.resultIndex; i < event.results.length; i++) {
        const res = event.results[i][0];
        onVoiceRecognition(res.transcript);
      }
    };

    return () => {
      recog.onresult = null;
    };
  }, [onVoiceRecognition]);

  useEffect(() => {
    const recog = recognition.current;
    if (!recog) {
      return;
    }
    try {
      if (recognitionEnabled) {
        recog.start();
      } else {
        recog.stop();
      }
    } catch (error) {
      console.warn(error);
    }
  }, [recognitionEnabled]);

  return {
    recognitionNotSupported,
  }
};
