import { FunctionComponent } from 'react';

import './UserAvatar.css';

interface UserAvatarProps {
  src?: string;
  nickname: string;
  altarnativeBackgound?: boolean;
  size?: 'xxs' | 'xs' | 's' | 'm' | 'l' | 'xl';
}

const getSizeClassName = (size: UserAvatarProps['size']) => {
  switch (size) {
    case 'xl':
      return 'w-6.875 h-6.875';
    case 'l':
      return 'w-4 h-4';
    case 'm':
      return 'w-2.5 h-2.5';
    case 's':
      return 'w-2.25 h-2.25';
    case 'xs':
      return 'w-1.375 h-1.375 items-center';
    case 'xxs':
      return 'w-1 h-1 items-end';
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
        {nickname[0].toLocaleUpperCase()}
      </div>
    );
  }

  return (
    <img
      src={src}
      className={`user-avatar shrink-0 ${getSizeClassName(size)}`}
      alt={`${nickname} avatar`}
      {...restProps}
    />
  );
};
