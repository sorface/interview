import { FunctionComponent, ReactNode } from 'react';
import { ThemedIcon } from '../../pages/Room/components/ThemedIcon/ThemedIcon';
import { IconNames } from '../../constants';
import { Dropdown, DropdownProps } from '../Dropdown/Dropdown';
import { Typography } from '../Typography/Typography';

export interface ContextMenuProps {
  position?: 'left' | 'right';
  toggleContent?: DropdownProps['toggleContent'];
  useButton?: DropdownProps['useButton'];
  children: ReactNode;
}

const ContextMenuComponent: FunctionComponent<ContextMenuProps> = ({
  position,
  toggleContent,
  useButton,
  children,
}) => {
  const defaultToggleContent = <ThemedIcon name={IconNames.EllipsisVertical} />;
  return (
    <Dropdown
      toggleContent={toggleContent || defaultToggleContent}
      useButton={useButton}
      contentClassName={`w-13.75 rounded-0.75 ${position === 'right' ? 'translate-x--10.375-y-0.25' : ''}`}
    >
      <div className='bg-wrap py-0.5'>
        {children}
      </div>
    </Dropdown>
  );
};

export interface ContextMenuItemProps {
  title: string;
  onClick: () => void;
}

const ContextMenuItem: FunctionComponent<ContextMenuItemProps> = ({
  title,
  onClick,
}) => {
  return (
    <div
      className='cursor-pointer hover:bg-form px-1 py-0.5'
      onClick={onClick}
    >
      <Typography size='m'>
        {title}
      </Typography>
    </div>
  );
};

export const ContextMenu = Object.assign(ContextMenuComponent, {
  Item: ContextMenuItem,
});
