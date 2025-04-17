import React, { FunctionComponent, useContext } from 'react';
import { Link } from 'react-router-dom';
import { UserAvatar } from '../UserAvatar/UserAvatar';
import { AuthContext } from '../../context/AuthContext';
import { ContextMenu } from '../ContextMenu/ContextMenu';
import { Typography } from '../Typography/Typography';
import { Gap } from '../Gap/Gap';
import { Icon } from '../../pages/Room/components/Icon/Icon';
import { IconNames, pathnames } from '../../constants';
import { Button } from '../Button/Button';
import { useLogout } from '../../hooks/useLogout';
import {VITE_GATEWAY_LOGOUT_URL, VITE_GATEWAY_POST_LOGOUT_URL} from "../../config";

export const PageHeaderUserAvatar: FunctionComponent = () => {
    const auth = useContext(AuthContext);
    const { logout } = useLogout();

  return (
    <>
      <ContextMenu
        translateRem={{ x: -11.375, y: 0.25 }}
        variant="alternative"
        toggleContent={
          <div className="cursor-pointer">
            <UserAvatar
              nickname={auth?.nickname || ''}
              src={auth?.avatar || ''}
              size="m"
              altarnativeBackgound
            />
          </div>
        }
      >
        <div className="flex flex-col items-center py-[1.5rem]">
          <UserAvatar
            nickname={auth?.nickname || ''}
            src={auth?.avatar || ''}
            size="l"
          />
          <Gap sizeRem={0.5} />
          <Typography size="m" bold>
            {auth?.nickname}
          </Typography>
          <Gap sizeRem={1.5} />
          <div className="flex">
            <div className="pr-[1.5rem]">
              <Link to={pathnames.session}>
                <Button variant="text">
                  <Icon name={IconNames.Settings} />
                </Button>
              </Link>
            </div>
            <div>
                <form method={'POST'}
                      action={`${VITE_GATEWAY_LOGOUT_URL}?redirect-location=${VITE_GATEWAY_POST_LOGOUT_URL}`}>
                    <Button variant="text">
                        <Icon name={IconNames.Exit}/>
                    </Button>
                </form>
            </div>
          </div>
        </div>
      </ContextMenu>
    </>
  );
};
