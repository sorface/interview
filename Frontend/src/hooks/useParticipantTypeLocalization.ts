import { LocalizationKey } from '../localization';
import { UserType } from '../types/user';
import { useLocalizationCaptions } from './useLocalizationCaptions';

export const useParticipantTypeLocalization = () => {
  const localizationCaptions = useLocalizationCaptions();

  const participantTypeLocalization: { [key in UserType]: string } = {
    Viewer: localizationCaptions[LocalizationKey.Viewer],
    Examinee: localizationCaptions[LocalizationKey.Examinee],
    Expert: localizationCaptions[LocalizationKey.Expert],
  };

  const localizeParticipantType = (type: UserType) => participantTypeLocalization[type];

  return localizeParticipantType;
};
