import { FunctionComponent, ReactNode } from 'react';
import { Typography } from '../Typography/Typography';
import { PageHeaderUserAvatar } from '../PageHeaderUserAvatar/PageHeaderUserAvatar';
import { PageHeaderSearch, PageHeaderSearchProps } from '../PageHeaderSearch/PageHeaderSearch';

interface PageHeaderProps {
  title: string;
  children?: ReactNode;
  searchValue?: PageHeaderSearchProps['searchValue'];
  onSearchChange?: PageHeaderSearchProps['onSearchChange'];
}

export const PageHeader: FunctionComponent<PageHeaderProps> = ({
  title,
  children,
  searchValue,
  onSearchChange,
}) => {
  return (
    <div className='flex items-center h-4 py-0.5'>
      <h1 className='m-0'>
        <Typography size='xl' bold>
          {title}
        </Typography>
      </h1>
      <div className={`ml-auto flex items-center ${children ? 'pr-2' : ''}`}>
        {!!(typeof searchValue === 'string' && onSearchChange) && (
          <div className='pr-0.25'>
            <PageHeaderSearch
              searchValue={searchValue}
              onSearchChange={onSearchChange}
            />
          </div>
        )}
        <PageHeaderUserAvatar />
      </div>
      <div className='flex'>{children}</div>
    </div>
  )
};
