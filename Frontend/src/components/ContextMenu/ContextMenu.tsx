import React, { FunctionComponent, ReactNode } from 'react';
import { Icon } from '../../pages/Room/components/Icon/Icon';
import { IconNames } from '../../constants';
import { Dropdown, DropdownProps } from '../Dropdown/Dropdown';
import { Typography } from '../Typography/Typography';
import { ButtonProps } from '../Button/Button';

export interface ContextMenuProps {
  translateRem: { x: number; y: number };
  toggleContent?: DropdownProps['toggleContent'];
  contentClassName?: string;
  useButton?: DropdownProps['useButton'];
  buttonVariant?: ButtonProps['variant'];
  children: ReactNode;
}

const ContextMenuComponent: FunctionComponent<ContextMenuProps> = ({
  translateRem,
  toggleContent,
  contentClassName,
  useButton,
  buttonVariant,
  children,
}) => {
  const defaultToggleContent = <Icon name={IconNames.EllipsisVertical} />;

  return (
    <Dropdown
      toggleContent={toggleContent || defaultToggleContent}
      useButton={useButton}
      buttonVariant={buttonVariant}
      contentClassName={contentClassName ?? 'w-13.75 rounded-0.75'}
      toggleClassName="flex"
      contentStyle={{
        transform: `translate(${translateRem.x}rem, ${translateRem.y}rem)`,
      }}
    >
      <div className="bg-wrap py-0.5">{children}</div>
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
    <div className="cursor-pointer hover:bg-form px-1 py-0.5" onClick={onClick}>
      <Typography size="m">{title}</Typography>
    </div>
  );
};

export const ContextMenu = Object.assign(ContextMenuComponent, {
  Item: ContextMenuItem,
});
