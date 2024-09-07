import { FunctionComponent } from 'react';
import { Button } from '../Button/Button';

import './SwitcherButton.css';

type ActiveIndex = 0 | 1;

interface SwitcherButtonProps {
  captions: [string, string];
  activeIndex: ActiveIndex;
  variant?: 'alternative';
  disabled?: boolean;
  onClick?: (index: ActiveIndex) => void;
}

export const SwitcherButton: FunctionComponent<SwitcherButtonProps> = ({
  captions,
  activeIndex,
  variant,
  disabled,
  onClick,
}) => {
  const nonActiveVariant = variant === 'alternative' ? 'invertedAlternative' : 'inverted';
  const disabledClassName = disabled ? 'cursor-not-allowed' : '';
  return (
    <div className={`switcher-button flex ${disabledClassName}`}>
      {captions.map((caption, index) => (
        <Button
          key={caption}
          variant={index === activeIndex ? 'invertedActive' : nonActiveVariant}
          onClick={() => { onClick?.(index as ActiveIndex); }}
        >
          {caption}
        </Button>
      ))}
    </div>
  );
};
