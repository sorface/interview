import { FunctionComponent } from 'react';

interface GapProps {
  sizeRem: number;
  horizontal?: boolean;
}

export const Gap: FunctionComponent<GapProps> = ({
  sizeRem,
  horizontal,
}) => {
  return (
    <div
      className='shrink-0'
      style={{ [horizontal ? 'width' : 'height']: `${sizeRem}rem` }}
    />
  );
};
