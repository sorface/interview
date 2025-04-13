import React, { FunctionComponent, ReactNode, useContext } from 'react';
import { Typography } from '../Typography/Typography';
import { PageHeaderUserAvatar } from '../PageHeaderUserAvatar/PageHeaderUserAvatar';
import {
  PageHeaderSearch,
  PageHeaderSearchProps,
} from '../PageHeaderSearch/PageHeaderSearch';
import { AuthContext } from '../../context/AuthContext';
import { PageHeaderNotifications } from '../PageHeaderNotifications/PageHeaderNotifications';

interface PageHeaderProps {
  title: string;
  actionItem?: ReactNode;
  notifications?: boolean;
  children?: ReactNode;
  searchValue?: PageHeaderSearchProps['searchValue'];
  onSearchChange?: PageHeaderSearchProps['onSearchChange'];
}

export const PageHeader: FunctionComponent<PageHeaderProps> = ({
  title,
  actionItem,
  notifications,
  children,
  searchValue,
  onSearchChange,
}) => {
  const auth = useContext(AuthContext);

  return (
    <div className="flex items-center shrink-0 h-[4rem] my-[0.5rem]">
      <h1 className="m-0">
        <Typography size="xl" bold>
          {title}
        </Typography>
      </h1>
      <div className={`ml-auto flex items-center ${children ? 'pr-[2rem]' : ''}`}>
        {actionItem && actionItem}
        {!!(typeof searchValue === 'string' && onSearchChange) && (
          <>
            <div className="pr-[0.25rem]">
              <PageHeaderSearch
                searchValue={searchValue}
                onSearchChange={onSearchChange}
              />
            </div>
            {notifications && (
              <div className="pr-[0.25rem]">
                <PageHeaderNotifications />
              </div>
            )}
          </>
        )}
        {auth && <PageHeaderUserAvatar />}
      </div>
      <div className="flex">{children}</div>
    </div>
  );
};
