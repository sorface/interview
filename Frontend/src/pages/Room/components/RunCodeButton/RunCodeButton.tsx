import React, { FunctionComponent } from 'react';
import { Button } from '../../../../components/Button/Button';
import { useLocalizationCaptions } from '../../../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../../../localization';
import './RunCodeButton.css';

interface RunCodeButtonProps {
  stringifyFunction: string | undefined;
}

export const RunCodeButton: FunctionComponent<RunCodeButtonProps> = ({
  stringifyFunction,
}) => {
  const localizationCaptions = useLocalizationCaptions();

  const handleStart = () => {
    if (stringifyFunction) {
      eval(stringifyFunction);
    }
  };

  return (
    <Button
      className="run-code-button"
      variant="inverted"
      onClick={handleStart}
    >
      {localizationCaptions[LocalizationKey.Run]}
    </Button>
  );
};
