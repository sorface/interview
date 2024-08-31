import { FunctionComponent } from 'react';
import { Typography } from '../Typography/Typography';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../localization';
import { Button } from '../Button/Button';
import { Gap } from '../Gap/Gap';

interface LoadingAccountErrorProps {
  onAccountReset: () => void;
}

export const LoadingAccountError: FunctionComponent<LoadingAccountErrorProps> = ({
  onAccountReset,
}) => {
  const localizationCaptions = useLocalizationCaptions();

  return (
    <div className='h-dvh flex flex-col items-center justify-center'>
      <Typography size='xxl'>
        {localizationCaptions[LocalizationKey.LoadingAccountErrorTitle]}
      </Typography>
      <Gap sizeRem={1} />
      <Typography size='m' secondary>
        {localizationCaptions[LocalizationKey.LoadingAccountError]}
      </Typography>
      <Typography size='m' secondary>
        {localizationCaptions[LocalizationKey.WeAwareOfProblem]}
      </Typography>
      <Gap sizeRem={1} />
      <Button onClick={onAccountReset}>
        <Typography size='m' secondary>
          {localizationCaptions[LocalizationKey.LogOut]}
        </Typography>
      </Button>
    </div>
  );
};
