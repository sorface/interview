import { FunctionComponent } from 'react';
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
        <button
          key={caption}
          className={`${index === activeIndex ? 'inverted' : ''}`}
        >
          {caption}
        </button>
      ))}
    </div>
  );
};
