import React, { FunctionComponent, useEffect, useState } from 'react';
import { Localization } from '../../localization';

import './Mark.css';

interface MarkProps {
  likes: number;
  dislikes: number;
}

export const Mark: FunctionComponent<MarkProps> = ({
  likes,
  dislikes,
}) => {
  const [markWithComment, setMarkWithComment] = useState<string>(Localization.MarkNotCalculated);
  const [markPostfix, setMarkPostfix] = useState<string>(Localization.MarkNotCalculated);

  useEffect(() => {
    const getMarkWithComment = (mark: number) => {
      const markInt = ~~mark;
      const markParts = mark.toString().split('.');
      const markFirstDecimal = markParts.length < 2 ? 0 : +markParts[1][0];
      if (markFirstDecimal >= 8) {
        return `${markInt + 1} ${Localization.MarkWithMinus}.`;
      }
      if (markFirstDecimal > 5) {
        return `${markInt} ${Localization.MarkWithPlus}.`;
      }
      return `${Localization.MarkAveragePrefix} ${markInt} ${Localization.MarkAverage}.`;
    };

    const getMarkPostfix = (mark: number) => {
      if (mark > 4) {
        return Localization.MarkPostfixCool;
      }
      if (mark >= 3) {
        return Localization.MarkPostfixAverage;
      }
      return Localization.MarkPostfixBad;
    };

    const totalCount = likes + dislikes;
    const mark = likes / totalCount * 10 / 2;
    const newMarkWithComment = getMarkWithComment(mark);
    const markPostfix = getMarkPostfix(mark);
    setMarkWithComment(newMarkWithComment);
    setMarkPostfix(markPostfix);
  }, [likes, dislikes]);

  return (
    <div className="mark">
      <div>{markWithComment}</div>
      <div className="mark-postfix">{Localization.MarkSmmary}. {markPostfix}</div>
    </div>
  );
};
