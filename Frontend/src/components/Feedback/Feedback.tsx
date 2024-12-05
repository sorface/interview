import { FunctionComponent, useState } from 'react';
import { IconNames } from '../../constants';
import { Typography } from '../Typography/Typography';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../localization';
import { Gap } from '../Gap/Gap';
import { Icon } from '../../pages/Room/components/Icon/Icon';
import { REACT_APP_FEEDBACK_IFRAME_URL } from '../../config';
import { useThemeClassName } from '../../hooks/useThemeClassName';
import { Theme } from '../../context/ThemeContext';

export const Feedback: FunctionComponent = () => {
  const localizationCaptions = useLocalizationCaptions();
  const [open, setOpen] = useState(false);

  const themedClassName = useThemeClassName({
    [Theme.Dark]: 'bg-dark-disable',
    [Theme.Light]: 'bg-grey1 border border-solid border-button !border-b-none',
  });

  const handleOpenClose = () => {
    setOpen(!open);
  };

  return (
    <div
      className={`absolute z-1 ${themedClassName} right-1 bottom-0 bg-wrap px-1.25 rounded-t-0.75`}
    >
      {open && <Gap sizeRem={1} />}
      <div
        className="cursor-pointer flex items-center"
        role="button"
        onClick={handleOpenClose}
      >
        <Typography size="m">
          {localizationCaptions[LocalizationKey.Feedback]}
        </Typography>
        <Gap sizeRem={0.25} horizontal />
        {!open && (
          <div className="h-1.25 rotate-180">
            <Icon size="s" name={IconNames.ChevronDown} />
          </div>
        )}
        {open && (
          <div className="h-1.25 ml-auto">
            <Icon size="m" name={IconNames.ChevronDown} />
          </div>
        )}
      </div>
      {open && (
        <>
          <Gap sizeRem={0.25} />
          <div className="flex w-25 h-32.25">
            <iframe
              title={localizationCaptions[LocalizationKey.Feedback]}
              className="flex-1 border-none"
              src={REACT_APP_FEEDBACK_IFRAME_URL}
            ></iframe>
          </div>
        </>
      )}
    </div>
  );
};
