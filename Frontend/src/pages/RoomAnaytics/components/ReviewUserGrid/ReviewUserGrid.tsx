import React, { FunctionComponent, ReactNode, useContext } from 'react';
import { DeviceContext } from '../../../../context/DeviceContext';

interface ReviewUserGridProps {
  children: ReactNode;
}

export const ReviewUserGrid: FunctionComponent<ReviewUserGridProps> = ({
  children,
}) => {
  const device = useContext(DeviceContext);
  return (
    <div
      className={`grid ${device === 'Desktop' ? 'grid-cols-3' : 'grid-cols-1'} gap-[3rem]`}
    >
      {children}
    </div>
  );
};
