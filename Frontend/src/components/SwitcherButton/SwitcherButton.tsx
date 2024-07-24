import { FunctionComponent } from 'react';
import { Button } from '../Button/Button';

import './SwitcherButton.css';

interface SwitcherButtonProps {
  captions: [string, string];
  activeIndex: 0 | 1;
  variant?: 'alternative';
  onClick?: (index: number) => void;
}

export const SwitcherButton: FunctionComponent<SwitcherButtonProps> = ({
  captions,
  activeIndex,
  variant,
  onClick,
}) => {
  const nonActiveVariant = variant === 'alternative' ? 'invertedAlternative' : 'inverted';
  return (
    <div className='switcher-button flex'>
      {captions.map((caption, index) => (
        <Button
          key={caption}
          variant={index === activeIndex ? 'invertedActive' : nonActiveVariant}
          onClick={() => { onClick?.(index); }}
        >
          {caption}
        </Button>
      ))}
    </div>
  );
};
