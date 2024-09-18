import { useCallback } from 'react';
import { LocalizationKey } from '../localization';
import { UserType } from '../types/user';
import { useLocalizationCaptions } from './useLocalizationCaptions';

export const useParticipantTypeLocalization = () => {
  const localizationCaptions = useLocalizationCaptions();

  const localizeParticipantType = useCallback((type: UserType) => {
    const participantTypeLocalization: { [key in UserType]: string } = {
      Viewer: localizationCaptions[LocalizationKey.Viewer],
      Examinee: localizationCaptions[LocalizationKey.Examinee],
      Expert: localizationCaptions[LocalizationKey.Expert],
    };
    return participantTypeLocalization[type];
  }, [localizationCaptions]);

  return localizeParticipantType;
};
