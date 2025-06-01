import React, { FunctionComponent } from 'react';
import { useThemeClassName } from '../../../hooks/useThemeClassName';
import { Theme } from '../../../context/ThemeContext';
import { RoadmapItem } from '../../../types/roadmap';
import { getTreeProgress } from '../utils/getTreeProgress';

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
  const treeCompleted = getTreeProgress(item?.questionTreeId || '') === 100;
  const fill = useThemeClassName({
    [Theme.Dark]: 'var(--dark-active)',
    [Theme.Light]: 'var(--red-light)',
  });
  const fillCompleted = useThemeClassName({
    [Theme.Dark]: 'var(--dark-green-light)',
    [Theme.Light]: 'var(--green)',
  });
  const stroke = useThemeClassName({
    [Theme.Dark]: 'var(--dark-closed-light)',
    [Theme.Light]: 'var(--text)',
  });

  const width = 224;
  const height = 48;

  return (
    <g className="cursor-pointer" onClick={() => onClick(item)}>
      <rect
        x={x}
        y={y}
        width={width}
        height={height}
        rx="3"
        fill={treeCompleted ? fillCompleted : fill}
      />
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
      {treeCompleted && (
        <g
          transform={`translate(${x + 12} ${y + 12}) scale(0.0707 0.0707)`}
          fill="var(--text)"
        >
          <path
            d="M152.502,0.001C68.412,0.001,0,68.412,0,152.501s68.412,152.5,152.502,152.5c84.089,0,152.5-68.411,152.5-152.5
            S236.591,0.001,152.502,0.001z M152.502,280.001C82.197,280.001,25,222.806,25,152.501c0-70.304,57.197-127.5,127.502-127.5
            c70.304,0,127.5,57.196,127.5,127.5C280.002,222.806,222.806,280.001,152.502,280.001z"
          />
          <path
            d="M218.473,93.97l-90.546,90.547l-41.398-41.398c-4.882-4.881-12.796-4.881-17.678,0c-4.881,4.882-4.881,12.796,0,17.678
            l50.237,50.237c2.441,2.44,5.64,3.661,8.839,3.661c3.199,0,6.398-1.221,8.839-3.661l99.385-99.385
            c4.881-4.882,4.881-12.796,0-17.678C231.269,89.089,223.354,89.089,218.473,93.97z"
          />
        </g>
      )}
    </g>
  );
};
