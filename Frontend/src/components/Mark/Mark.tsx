import React, { FunctionComponent, useEffect, useState } from 'react';
import { LocalizationKey } from '../../localization';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';

import './Mark.css';

interface MarkProps {
  likes: number;
  dislikes: number;
}

export const Mark: FunctionComponent<MarkProps> = ({
  likes,
  dislikes,
}) => {
  const localizationCaptions = useLocalizationCaptions();
  const [markWithComment, setMarkWithComment] = useState<string>(localizationCaptions[LocalizationKey.MarkNotCalculated]);
  const [markPostfix, setMarkPostfix] = useState<string>(localizationCaptions[LocalizationKey.MarkNotCalculated]);

  useEffect(() => {
    const getMarkWithComment = (mark: number) => {
      const markInt = ~~mark;
      const markParts = mark.toString().split('.');
      const markFirstDecimal = markParts.length < 2 ? 0 : +markParts[1][0];
      if (markFirstDecimal >= 8) {
        return `${markInt + 1} ${localizationCaptions[LocalizationKey.MarkWithMinus]}.`;
      }
      if (markFirstDecimal > 5) {
        return `${markInt} ${localizationCaptions[LocalizationKey.MarkWithPlus]}.`;
      }
      return `${localizationCaptions[LocalizationKey.MarkAveragePrefix]} ${markInt} ${localizationCaptions[LocalizationKey.MarkAverage]}.`;
    };

    const getMarkPostfix = (mark: number) => {
      if (mark > 4) {
        return localizationCaptions[LocalizationKey.MarkPostfixCool];
      }
      if (mark >= 3) {
        return localizationCaptions[LocalizationKey.MarkPostfixAverage];
      }
      return localizationCaptions[LocalizationKey.MarkPostfixBad];
    };

    const totalCount = likes + dislikes;
    const mark = likes / totalCount * 10 / 2;
    const newMarkWithComment = getMarkWithComment(mark);
    const markPostfix = getMarkPostfix(mark);
    setMarkWithComment(newMarkWithComment);
    setMarkPostfix(markPostfix);
  }, [likes, dislikes, localizationCaptions]);

  return (
    <div className="mark">
      <div>{markWithComment}</div>
      <div className="mark-postfix">{localizationCaptions[LocalizationKey.MarkSmmary]}. {markPostfix}</div>
    </div>
  );
};
