import { FunctionComponent } from 'react';

interface GapProps {
  sizeRem: number;
}

export const Gap: FunctionComponent<GapProps> = ({
  sizeRem,
}) => {
  return (
    <div style={{ height: `${sizeRem}rem` }}></div>
  );
};
