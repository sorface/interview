import { FunctionComponent, useContext, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { UserAvatar } from '../UserAvatar/UserAvatar';
import { AuthContext } from '../../context/AuthContext';
import { ContextMenu } from '../ContextMenu/ContextMenu';
import { Typography } from '../Typography/Typography';
import { Gap } from '../Gap/Gap';
import { Icon } from '../../pages/Room/components/Icon/Icon';
import { IconNames, pathnames } from '../../constants';
import { Button } from '../Button/Button';
import { useLogout } from '../../hooks/useLogout';

export const PageHeaderUserAvatar: FunctionComponent = () => {
  const auth = useContext(AuthContext);
  const navigate = useNavigate();
  const { logout, logoutState: { process: { logoutCode } } } = useLogout();

  useEffect(() => {
    if (!logoutCode || logoutCode !== 200) {
      return;
    }
    navigate(pathnames.home.replace(':redirect?', ''));
  }, [logoutCode, navigate]);

  return (
    <>
      <ContextMenu
        position='right'
        toggleContent={
          <div className='cursor-pointer'>
            <UserAvatar
              nickname={auth?.nickname || ''}
              src={auth?.avatar || ''}
              size='m'
              altarnativeBackgound
            />
          </div>
        }
      >
        <div className='flex flex-col items-center py-1.5'>
          <UserAvatar
            nickname={auth?.nickname || ''}
            src={auth?.avatar || ''}
            size='l'
          />
          <Gap sizeRem={0.5} />
          <Typography size='m' bold>{auth?.nickname}</Typography>
          <Gap sizeRem={1.5} />
          <div className='flex'>
            <div className='pr-1.5'>
              <Link to={pathnames.session}>
                <Button variant='text'>
                  <Icon name={IconNames.Settings} />
                </Button>
              </Link>
            </div>
            <div>
              <Button variant='text' onClick={logout}>
                <Icon name={IconNames.Exit} />
              </Button>
            </div>
          </div>
        </div>
      </ContextMenu>

    </>
  );
};
