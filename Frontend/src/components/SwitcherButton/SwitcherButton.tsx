import { FunctionComponent, ReactNode } from 'react';
import { Button } from '../Button/Button';

import './SwitcherButton.css';

type ActiveIndex = 0 | 1;

interface SwitcherButtonContent {
  id: string | number;
  content: ReactNode;
}

interface SwitcherButtonProps {
  items: [SwitcherButtonContent, SwitcherButtonContent];
  activeIndex: ActiveIndex;
  variant?: 'alternative';
  disabled?: boolean;
  onClick?: (index: ActiveIndex) => void;
}

export const SwitcherButton: FunctionComponent<SwitcherButtonProps> = ({
  items,
  activeIndex,
  variant,
  disabled,
  onClick,
}) => {
  const nonActiveVariant = variant === 'alternative' ? 'invertedAlternative' : 'inverted';
  const disabledClassName = disabled ? 'cursor-not-allowed' : '';
  return (
    <div className={`switcher-button flex ${disabledClassName}`}>
      {items.map((item, index) => (
        <Button
          key={item.id}
          variant={index === activeIndex ? 'invertedActive' : nonActiveVariant}
          onClick={() => { onClick?.(index as ActiveIndex); }}
        >
          {item.content}
        </Button>
      ))}
    </div>
  );
};
