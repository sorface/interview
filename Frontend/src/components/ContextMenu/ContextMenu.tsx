import React, { FunctionComponent, ReactNode } from 'react';
import { Icon } from '../../pages/Room/components/Icon/Icon';
import { IconNames } from '../../constants';
import { Dropdown, DropdownProps } from '../Dropdown/Dropdown';
import { Typography } from '../Typography/Typography';
import { ButtonProps } from '../Button/Button';
import { useThemeClassName } from '../../hooks/useThemeClassName';
import { Theme } from '../../context/ThemeContext';

export interface ContextMenuProps {
  translateRem: { x: number; y: number };
  toggleContent?: DropdownProps['toggleContent'];
  contentClassName?: string;
  useButton?: DropdownProps['useButton'];
  buttonVariant?: ButtonProps['variant'];
  variant?: 'alternative';
  children: ReactNode;
}

const ContextMenuComponent: FunctionComponent<ContextMenuProps> = ({
  translateRem,
  toggleContent,
  contentClassName,
  useButton,
  buttonVariant,
  variant,
  children,
}) => {
  const defaultToggleContent = <Icon name={IconNames.EllipsisVertical} />;
  const contentThemedClassName = useThemeClassName({
    [Theme.Dark]: variant === 'alternative' ? 'bg-modal-bg' : 'bg-wrap',
    [Theme.Light]: 'bg-wrap',
  });

  return (
    <Dropdown
      toggleContent={toggleContent || defaultToggleContent}
      useButton={useButton}
      buttonVariant={buttonVariant}
      contentClassName={contentClassName ?? 'w-[13.75rem] rounded-[0.75rem]'}
      toggleClassName="flex"
      contentStyle={{
        transform: `translate(${translateRem.x}rem, ${translateRem.y}rem)`,
      }}
    >
      <div className={`${contentThemedClassName} py-[0.5rem]`}>{children}</div>
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
      className="cursor-pointer hover:bg-form px-[1rem] py-[0.5rem]"
      onClick={onClick}
    >
      <Typography size="m">{title}</Typography>
    </div>
  );
};

export const ContextMenu = Object.assign(ContextMenuComponent, {
  Item: ContextMenuItem,
});
