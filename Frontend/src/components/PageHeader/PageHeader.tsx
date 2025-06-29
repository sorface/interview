import React, { FunctionComponent, ReactNode } from 'react';
import { Typography } from '../Typography/Typography';
import {
  PageHeaderSearch,
  PageHeaderSearchProps,
} from '../PageHeaderSearch/PageHeaderSearch';
import { PageHeaderNotifications } from '../PageHeaderNotifications/PageHeaderNotifications';

interface PageHeaderProps {
  title: string;
  actionItem?: ReactNode;
  notifications?: boolean;
  overlapping?: boolean;
  children?: ReactNode;
  searchValue?: PageHeaderSearchProps['searchValue'];
  onSearchChange?: PageHeaderSearchProps['onSearchChange'];
}

export const PageHeader: FunctionComponent<PageHeaderProps> = ({
  title,
  actionItem,
  notifications,
  overlapping,
  children,
  searchValue,
  onSearchChange,
}) => {
  return (
    <div
      className={`flex items-center shrink-0 h-[4rem] my-[0.5rem] ${overlapping ? 'fixed right-[1rem]' : ''}`}
    >
      <h1 className="m-0">
        <Typography size="xxl" semibold>
          {title}
        </Typography>
      </h1>
      <div
        className={`ml-auto flex items-center ${children ? 'pr-[2rem]' : ''}`}
      >
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
      </div>
      <div className="flex">{children}</div>
    </div>
  );
};
