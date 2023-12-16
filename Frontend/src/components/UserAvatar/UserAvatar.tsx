import { FunctionComponent } from 'react';

import './UserAvatar.css';

interface UserAvatarProps {
  src: string;
  nickname: string;
}

export const UserAvatar: FunctionComponent<UserAvatarProps> = ({
  src,
  nickname,
  ...restProps
}) => {
  return (
    <img
      src={src}
      className='user-avatar'
      alt={`${nickname} avatar`}
      {...restProps}
    />
  );
};
