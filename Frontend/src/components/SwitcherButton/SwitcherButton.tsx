import { FunctionComponent } from 'react';
import { Button } from '../Button/Button';

import './SwitcherButton.css';

interface SwitcherButtonProps {
  captions: [string, string];
  activeIndex: 0 | 1;
}

export const SwitcherButton: FunctionComponent<SwitcherButtonProps> = ({
  captions,
  activeIndex,
}) => {
  return (
    <div className='switcher-button'>
      {captions.map((caption, index) => (
        <Button
          key={caption}
          variant={index === activeIndex ? 'invertedActive' : 'inverted'}
        >
          {caption}
        </Button>
      ))}
    </div>
  );
};
