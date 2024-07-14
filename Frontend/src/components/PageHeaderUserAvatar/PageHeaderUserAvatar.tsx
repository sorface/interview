import { FunctionComponent, useContext } from 'react';
import { UserAvatar } from '../UserAvatar/UserAvatar';
import { AuthContext } from '../../context/AuthContext';
import { ContextMenu } from '../ContextMenu/ContextMenu';
import { Typography } from '../Typography/Typography';
import { Gap } from '../Gap/Gap';
import { ThemedIcon } from '../../pages/Room/components/ThemedIcon/ThemedIcon';
import { IconNames, pathnames } from '../../constants';
import { Link } from 'react-router-dom';
import { useCommunist } from '../../hooks/useCommunist';

export const PageHeaderUserAvatar: FunctionComponent = () => {
  const auth = useContext(AuthContext);
  const { resetCommunist } = useCommunist();

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
                <button>
                  <ThemedIcon name={IconNames.Settings} />
                </button>
              </Link>
            </div>
            <div>
              <button onClick={resetCommunist}>
                <ThemedIcon name={IconNames.Exit} />
              </button>
            </div>
          </div>
        </div>
      </ContextMenu>

    </>
  );
};
