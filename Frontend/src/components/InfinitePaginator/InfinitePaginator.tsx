import React, { FunctionComponent, useEffect, useRef } from 'react';

import './InfinitePaginator.css';

interface InfinitePaginatorProps {
  onNextPage: () => void;
}

export const InfinitePaginator: FunctionComponent<InfinitePaginatorProps> = ({
  onNextPage,
}) => {
  const triggerElRef = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    const triggerElRefCurrent = triggerElRef.current;
    const observer = new IntersectionObserver((entries) => {
      const target = entries[0];
      if (target.isIntersecting) {
        onNextPage();
      }
    });

    if (triggerElRefCurrent) {
      observer.observe(triggerElRefCurrent);
    }

    return () => {
      if (triggerElRefCurrent) {
        observer.unobserve(triggerElRefCurrent);
      }
    };
  }, [triggerElRef, onNextPage]);

  return (
    <div ref={triggerElRef} className="infinite-paginator"></div>
  );
};
