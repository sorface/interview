import React, { FunctionComponent } from 'react';

import './Wave.css';

export const Wave: FunctionComponent = () => {
  return (
    <svg width="80" height="60" viewBox="5 0 80 60">
      <path
        className="waveAnimated"
        fill="none"
        stroke="var(--text)"
        strokeWidth="4"
        strokeLinecap="round"
        d="M 0 37.5 c 7.684299348848887 0 7.172012725592294 -15 15 -15 s 7.172012725592294 15 15 15 s 7.172012725592294 -15 15 -15 s 7.172012725592294 15 15 15 s 7.172012725592294 -15 15 -15 s 7.172012725592294 15 15 15 s 7.172012725592294 -15 15 -15 s 7.172012725592294 15 15 15 s 7.172012725592294 -15 15 -15 s 7.172012725592294 15 15 15 s 7.172012725592294 -15 15 -15 s 7.172012725592294 15 15 15 s 7.172012725592294 -15 15 -15 s 7.172012725592294 15 15 15 s 7.172012725592294 -15 15 -15"
      />
    </svg>
  );
};
