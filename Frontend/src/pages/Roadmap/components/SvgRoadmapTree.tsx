import React, { FunctionComponent } from 'react';
import { useThemeClassName } from '../../../hooks/useThemeClassName';
import { Theme } from '../../../context/ThemeContext';
import { RoadmapItem } from '../../../types/roadmap';

interface SvgRoadmapTreeProps {
  x: number;
  y: number;
  item?: RoadmapItem;
  onClick: (item?: RoadmapItem) => void;
}

export const SvgRoadmapTree: FunctionComponent<SvgRoadmapTreeProps> = ({
  x,
  y,
  item,
  onClick,
}) => {
  const fill = useThemeClassName({
    [Theme.Dark]: 'var(--dark-active)',
    [Theme.Light]: 'var(--red-light)',
  });
  const stroke = useThemeClassName({
    [Theme.Dark]: 'var(--dark-grey4)',
    [Theme.Light]: 'var(--text)',
  });

  const width = 224;
  const height = 48;

  return (
    <g className="cursor-pointer" onClick={() => onClick(item)}>
      <rect x={x} y={y} width={width} height={height} rx="3" fill={fill} />
      <rect
        x={x}
        y={y}
        width={width}
        height={height}
        rx="3"
        stroke={stroke}
        strokeOpacity="0.28"
        strokeWidth="2"
        strokeLinecap="round"
      />
      <text
        x={x + width / 2}
        y={y + height / 2}
        fill="var(--text)"
        fontSize="1rem"
        fontWeight={400}
        textAnchor="middle"
        dominantBaseline="middle"
      >
        {item?.name}
      </text>
    </g>
  );
};
