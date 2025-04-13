import React, { Fragment, FunctionComponent, ReactNode } from 'react';
import { Button, ButtonProps } from '../Button/Button';

import './SwitcherButton.css';

type ActiveIndex = 0 | 1;

export interface SwitcherButtonContent {
  id: string | number;
  content: ReactNode;
}

interface SwitcherButtonProps {
  items: [SwitcherButtonContent, SwitcherButtonContent];
  activeIndex: ActiveIndex;
  activeVariant: ButtonProps['variant'];
  nonActiveVariant: ButtonProps['variant'];
  mini?: boolean;
  disabled?: boolean;
  onClick?: (index: ActiveIndex) => void;
}

export const SwitcherButton: FunctionComponent<SwitcherButtonProps> = ({
  items,
  activeIndex,
  activeVariant,
  nonActiveVariant,
  mini,
  disabled,
  onClick,
}) => {
  const disabledClassName = disabled ? 'cursor-not-allowed' : '';
  return (
    <div
      className={`switcher-button ${mini ? 'mini' : 'max'} flex ${disabledClassName}`}
    >
      {items.map((item, index) => (
        <Fragment key={item.id}>
          <Button
            className={`${mini ? 'min-w-[0rem]' : ''}`}
            variant={index === activeIndex ? activeVariant : nonActiveVariant}
            onClick={() => {
              onClick?.(index as ActiveIndex);
            }}
          >
            {item.content}
          </Button>
        </Fragment>
      ))}
    </div>
  );
};
