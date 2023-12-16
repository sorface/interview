import { FunctionComponent } from 'react';

import './OpenIcon.css';

interface OpenIconProps {
  sizeRem: number;
}

export const OpenIcon: FunctionComponent<OpenIconProps> = ({
  sizeRem,
}) => {
  const styleSize = `${sizeRem}rem`;
  return (
    <div
      className='open-icon'
      style={{
        width: styleSize,
        height: styleSize,
      }}
    />
  );
};
