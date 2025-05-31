import React, { FunctionComponent } from 'react';
import { useThemeClassName } from '../../../hooks/useThemeClassName';
import { Theme } from '../../../context/ThemeContext';
import { RoadmapItem } from '../../../types/roadmap';

interface SvgRoadmapMilestoneProps {
  x: number;
  y: number;
  item?: RoadmapItem;
}

export const SvgRoadmapMilestone: FunctionComponent<
  SvgRoadmapMilestoneProps
> = ({ x, y, item }) => {
  const fill = useThemeClassName({
    [Theme.Dark]: 'var(--dark-grey4)',
    [Theme.Light]: 'var(--green-light)',
  });
  const stroke = useThemeClassName({
    [Theme.Dark]: 'var(--dark-closed-light)',
    [Theme.Light]: 'var(--grey3)',
  });

  const width = 184;
  const height = 48;

  return (
    <g>
      <rect x={x} y={y} width={width} height={height} rx="3" fill={fill} />
      <rect
        x={x}
        y={y}
        width={width}
        height={height}
        rx="3"
        stroke={stroke}
        strokeWidth="2"
        strokeLinecap="round"
      />
      <text
        x={x + width / 2}
        y={y + height / 2}
        fill="var(--text)"
        fontSize="1rem"
        fontWeight={700}
        textAnchor="middle"
        dominantBaseline="middle"
      >
        {item?.name}
      </text>
    </g>
  );
};
