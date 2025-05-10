import React, { FunctionComponent } from 'react';

interface HomeInfoBlockProps {
  title: string;
  info: string;
  iamgeSrc: string;
  reverseRow?: boolean;
}

export const HomeInfoBlock: FunctionComponent<HomeInfoBlockProps> = ({
  title,
  info,
  iamgeSrc,
  reverseRow,
}) => {
  const infoBlock = (
    <div className="w-5/6 sm:w-1/2 p-6">
      <h3 className="text-3xl text-gray-800 font-bold leading-none mb-3">
        {title}
      </h3>
      <p className="text-gray-600 mb-8">{info}</p>
    </div>
  );

  const imageBlock = (
    <div className="w-full sm:w-1/2 p-6">
      <img src={iamgeSrc} />
    </div>
  );

  return (
    <div className="flex flex-wrap text-left">
      {reverseRow ? imageBlock : infoBlock}
      {reverseRow ? infoBlock : imageBlock}
    </div>
  );
};
