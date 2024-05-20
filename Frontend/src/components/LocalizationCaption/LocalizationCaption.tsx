import { FunctionComponent } from 'react';
import { LocalizationKey } from '../../localization';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';

export interface LocalizationCaptionProps {
  captionKey: LocalizationKey;
}

export const LocalizationCaption: FunctionComponent<LocalizationCaptionProps> = ({
  captionKey,
}) => {
  return (
    <>
      {useLocalizationCaptions()[captionKey]}
    </>
  );
};
