import { FunctionComponent, ReactNode } from 'react';
import { Icon } from '../../pages/Room/components/Icon/Icon';
import { IconNames } from '../../constants';
import { Dropdown, DropdownProps } from '../Dropdown/Dropdown';
import { Typography } from '../Typography/Typography';
import { ButtonProps } from '../Button/Button';

export interface ContextMenuProps {
  position?: 'bottom-left' | 'bottom-right' | 'left';
  toggleContent?: DropdownProps['toggleContent'];
  useButton?: DropdownProps['useButton'];
  buttonVariant?: ButtonProps['variant'];
  children: ReactNode;
}

const ContextMenuComponent: FunctionComponent<ContextMenuProps> = ({
  position,
  toggleContent,
  useButton,
  buttonVariant,
  children,
}) => {
  const defaultToggleContent = <Icon name={IconNames.EllipsisVertical} />;
  const positionClassName =
    position === 'bottom-right' ?
      'translate-x--11.375-y-0.25' :
      position === 'left' ?
        'translate-x--14.25-y--6.75' :
        '';

  return (
    <Dropdown
      toggleContent={toggleContent || defaultToggleContent}
      useButton={useButton}
      buttonVariant={buttonVariant}
      contentClassName={`w-13.75 rounded-0.75 ${positionClassName}`}
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
