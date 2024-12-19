import React, { FunctionComponent } from 'react';

type InfoBlockProps = React.ButtonHTMLAttributes<HTMLDivElement> & {
  className: string;
};

export const InfoBlock: FunctionComponent<InfoBlockProps> = (props) => {
  return (
    <div
      {...props}
      className={`bg-wrap px-1.5 py-1.75 rounded-0.75 ${props.className}`}
    />
  );
};
