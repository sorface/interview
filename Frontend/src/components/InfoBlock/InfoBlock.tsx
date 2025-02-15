import React, { FunctionComponent } from 'react';

type InfoBlockProps = React.ButtonHTMLAttributes<HTMLDivElement> & {
  className: string;
};

export const InfoBlock: FunctionComponent<InfoBlockProps> = (props) => {
  return (
    <div
      {...props}
      className={`bg-wrap px-[1.5rem] py-[1.75rem] rounded-[0.75rem] ${props.className}`}
    />
  );
};
