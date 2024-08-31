import { FunctionComponent, useContext, useEffect } from 'react';
import { AuthContext } from '../../context/AuthContext';
import { useNavigate, generatePath } from 'react-router-dom';
import { pathnames } from '../../constants';
import { Typography } from '../../components/Typography/Typography';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../localization';

export const LogoutError: FunctionComponent = () => {
  const auth = useContext(AuthContext);
  const localizationCaptions = useLocalizationCaptions();
  const navigate = useNavigate();

  useEffect(() => {
    if (!auth) {
      return;
    }
    navigate(generatePath(pathnames.home, { redirect: null }));
  }, [auth, navigate]);

  return (
    <div className='h-full flex flex-col items-center justify-center'>
      <Typography size='m' secondary>
        {localizationCaptions[LocalizationKey.LoadingAccountError]}
      </Typography>
      <Typography size='m' secondary>
        {localizationCaptions[LocalizationKey.WeAwareOfProblem]}
      </Typography>
    </div>
  );
};
