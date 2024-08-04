import { FunctionComponent, ReactNode } from 'react';

interface ReviewUserGridProps {
  children: ReactNode;
}

export const ReviewUserGrid: FunctionComponent<ReviewUserGridProps> = ({
  children,
}) => {
  return (
    <div className='grid grid-cols-3 gap-12'>
      {children}
    </div>
  );
};
