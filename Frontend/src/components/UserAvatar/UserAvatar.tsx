import React, { FunctionComponent } from 'react';
import { Typography } from '../Typography/Typography';

import './UserAvatar.css';

export interface UserAvatarProps {
  src?: string;
  nickname: string;
  altarnativeBackgound?: boolean;
  size?: 'xxs' | 'xs' | 's' | 'm' | 'l' | 'xl';
}

const getSizeClassName = (size: UserAvatarProps['size']) => {
  switch (size) {
    case 'xl':
      return 'w-[6.875rem] h-[6.875rem]';
    case 'l':
      return 'w-[4rem] h-[4rem]';
    case 'm':
      return 'w-[2.5rem] h-[2.5rem]';
    case 's':
      return 'w-[2.25rem] h-[2.25rem]';
    case 'xs':
      return 'w-[1.375rem] h-[1.375rem] items-center';
    case 'xxs':
      return 'w-[1rem] h-[1rem] items-end';
    default:
      return '';
  }
};

export const UserAvatar: FunctionComponent<UserAvatarProps> = ({
  src,
  nickname,
  altarnativeBackgound,
  size,
  ...restProps
}) => {
  if (!src) {
    return (
      <div
        className={`${altarnativeBackgound ? 'bg-wrap' : 'bg-form'} flex items-center justify-center user-avatar shrink-0 ${getSizeClassName(size)}`}
        {...restProps}
      >
        <Typography size="m">{nickname[0].toLocaleUpperCase()}</Typography>
      </div>
    );
  }

  return (
    <img
      src={src}
      className={`user-avatar block shrink-0 ${getSizeClassName(size)}`}
      alt={`${nickname} avatar`}
      {...restProps}
    />
  );
};
