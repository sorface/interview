import { useCallback, useReducer } from 'react';
import { useLocalizationCaptions } from '../../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../../localization';

export const enum VoiceRecognitionCommand {
  None,
  LetsStart,
  RateMe,
}

interface VoiceRecognitionAccumState {
  value: string;
  command: VoiceRecognitionCommand;
}

const getInitialState = (): VoiceRecognitionAccumState => ({
  value: '',
  command: VoiceRecognitionCommand.None,
});

type VoiceRecognitionAccumAction =
  | {
      name: 'reset';
    }
  | {
      name: 'addTranscript';
      payload: { value: string; command: VoiceRecognitionCommand };
    };

const checkIsCommandTranscript = (transcript: string, command: string[]) => {
  const words = encodeRussianSymbols(transcript.toLowerCase().replaceAll('.', '')).split(' ');
  const firstWordIndex = words.indexOf(encodeRussianSymbols(command[0]));
  
  if (firstWordIndex === -1) {
    return false;
  }

  return words[firstWordIndex + 1] === encodeRussianSymbols(command[1]);
};

const encodeRussianSymbols = (value: string) => {
  if (!value) return '';

  return value.replaceAll('ё', 'е');
}

const apiMethodReducer = (
  state: VoiceRecognitionAccumState,
  action: VoiceRecognitionAccumAction,
): VoiceRecognitionAccumState => {
  switch (action.name) {
    case 'reset':
      return getInitialState();
    case 'addTranscript':
      return {
        command: action.payload.command,
        value: state.value + action.payload.value,
      };
    default:
      return state;
  }
};

export const useVoiceRecognitionAccum = () => {
  const [apiMethodState, dispatch] = useReducer(
    apiMethodReducer,
    getInitialState(),
  );
  const localizationCaptions = useLocalizationCaptions();

  const resetVoiceRecognitionAccum = useCallback(() => {
    dispatch({
      name: 'reset',
    });
  }, []);

  const addVoiceRecognitionAccumTranscript = useCallback(
    (transcript: string) => {
      const rateMeCommandLocalized =
        localizationCaptions[LocalizationKey.RateMeCommand].split(' ');
      const letsStartCommandLocalized =
        localizationCaptions[LocalizationKey.LetsBeginCommand].split(' ');
      dispatch({
        name: 'addTranscript',
        payload: {
          command: checkIsCommandTranscript(transcript, rateMeCommandLocalized)
            ? VoiceRecognitionCommand.RateMe
            : checkIsCommandTranscript(transcript, letsStartCommandLocalized)
              ? VoiceRecognitionCommand.LetsStart
              : VoiceRecognitionCommand.None,
          value: transcript,
        },
      });
    },
    [localizationCaptions],
  );

  return {
    recognitionAccum: apiMethodState.value,
    recognitionCommand: apiMethodState.command,
    resetVoiceRecognitionAccum,
    addVoiceRecognitionAccumTranscript,
  };
};
