import React, { FunctionComponent } from 'react';
import { RoadmapItem } from '../../../types/roadmap';
import { SvgRoadmapMilestone } from './SvgRoadmapMilestone';
import { SvgRoadmapTree } from './SvgRoadmapTree';

interface SvgRoadmapProps {
  items: RoadmapItem[];
  handleCreateRoom: (treeId: string, treeName: string) => void;
  onRoomAlreadyExists: (roomId: string) => void;
}

export const SvgRoadmap: FunctionComponent<SvgRoadmapProps> = ({
  items,
  handleCreateRoom,
  onRoomAlreadyExists,
}) => {
  const getNextItem = (() => {
    let currIndex = 0;
    return () => items[currIndex++];
  })();

  const handleClick = (item?: RoadmapItem) => {
    if (!item || !item.questionTreeId || !item.name) {
      return;
    }
    if (item.roomId) {
      onRoomAlreadyExists(item.roomId);
      return;
    }
    handleCreateRoom(item.questionTreeId, item.name);
  };

  return (
    <svg
      width="1200"
      height="600"
      viewBox="80 110 1150 600"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
      className="w-full"
    >
      <SvgRoadmapMilestone x={548} y={144} item={getNextItem()} />
      <SvgRoadmapTree
        x={940}
        y={60}
        item={getNextItem()}
        onClick={handleClick}
      />
      <path
        d="M736.5 168C836 168 836 84 935.5 84"
        stroke="#838AA4"
        strokeWidth="2"
        strokeLinecap="round"
        strokeLinejoin="bevel"
        strokeDasharray="9 9"
      />
      <SvgRoadmapTree
        x={940}
        y={126}
        item={getNextItem()}
        onClick={handleClick}
      />
      <path
        d="M935.896 151.846C818.112 204.834 822.112 190.834 736.362 169.105"
        stroke="#838AA4"
        strokeWidth="2"
        strokeLinecap="round"
        strokeLinejoin="bevel"
        strokeDasharray="9 9"
      />
      <SvgRoadmapMilestone x={548} y={282} item={getNextItem()} />
      <SvgRoadmapTree
        x={84}
        y={60}
        item={getNextItem()}
        onClick={handleClick}
      />
      <SvgRoadmapTree
        x={84}
        y={126}
        item={getNextItem()}
        onClick={handleClick}
      />
      <SvgRoadmapTree
        x={84}
        y={192}
        item={getNextItem()}
        onClick={handleClick}
      />
      <SvgRoadmapTree
        x={84}
        y={258}
        item={getNextItem()}
        onClick={handleClick}
      />
      <SvgRoadmapTree
        x={84}
        y={324}
        item={getNextItem()}
        onClick={handleClick}
      />
      <SvgRoadmapTree
        x={84}
        y={390}
        item={getNextItem()}
        onClick={handleClick}
      />
      <SvgRoadmapTree
        x={84}
        y={456}
        item={getNextItem()}
        onClick={handleClick}
      />
      <SvgRoadmapTree
        x={84}
        y={522}
        item={getNextItem()}
        onClick={handleClick}
      />
      <SvgRoadmapTree
        x={84}
        y={588}
        item={getNextItem()}
        onClick={handleClick}
      />
      <path
        d="M543.94 304.06C388.978 230.006 468.98 134.005 312.478 149.555"
        stroke="#838AA4"
        strokeWidth="2"
        strokeLinecap="round"
        strokeLinejoin="bevel"
        strokeDasharray="9 9"
      />
      <path
        d="M543.646 304.862C409.686 269.861 409.686 181.86 312.266 214.568"
        stroke="#838AA4"
        strokeWidth="2"
        strokeLinecap="round"
        strokeLinejoin="bevel"
        strokeDasharray="9 9"
      />
      <path
        d="M544.093 303.766C427.628 237.177 507.631 77.1774 312.497 83.8463"
        stroke="#838AA4"
        strokeWidth="2"
        strokeLinecap="round"
        strokeLinejoin="bevel"
        strokeDasharray="9 9"
      />
      <path
        d="M543.512 306.32C377.482 318.174 377.481 358.174 312.453 348.652"
        stroke="#838AA4"
        strokeWidth="2"
        strokeLinecap="round"
        strokeLinejoin="bevel"
        strokeDasharray="9 9"
      />
      <path
        d="M543.522 306.453C387.297 322.247 467.298 482.249 312.136 415.772"
        stroke="#838AA4"
        strokeWidth="2"
        strokeLinecap="round"
        strokeLinejoin="bevel"
        strokeDasharray="9 9"
      />
      <path
        d="M543.959 307.98C421.185 368.136 501.187 528.138 312.367 481.088"
        stroke="#838AA4"
        strokeWidth="2"
        strokeLinecap="round"
        strokeLinejoin="bevel"
        strokeDasharray="9 9"
      />
      <path
        d="M543.99 308.042C467.913 346.768 547.916 634.77 312.22 547.562"
        stroke="#838AA4"
        strokeWidth="2"
        strokeLinecap="round"
        strokeLinejoin="bevel"
        strokeDasharray="9 9"
      />
      <path
        d="M543.527 306.492C503.351 310.909 583.354 662.911 312.425 612.818"
        stroke="#838AA4"
        strokeWidth="2"
        strokeLinecap="round"
        strokeLinejoin="bevel"
        strokeDasharray="9 9"
      />
      <path
        d="M312.098 280.141C364.595 256.33 364.594 280.329 543.543 305.376"
        stroke="#838AA4"
        strokeWidth="2"
        strokeLinecap="round"
        strokeLinejoin="bevel"
        strokeDasharray="9 9"
      />
      <SvgRoadmapMilestone x={548} y={402} item={getNextItem()} />
      <SvgRoadmapTree
        x={956}
        y={222}
        item={getNextItem()}
        onClick={handleClick}
      />
      <path
        d="M736.5 426C844 426 844 246 951.5 246"
        stroke="#838AA4"
        strokeWidth="2"
        strokeLinecap="round"
        strokeLinejoin="bevel"
        strokeDasharray="9 9"
      />
      <SvgRoadmapTree
        x={956}
        y={306}
        item={getNextItem()}
        onClick={handleClick}
      />
      <path
        d="M951.5 330C844 330 844 426 736.5 426"
        stroke="#838AA4"
        strokeWidth="2"
        strokeLinecap="round"
        strokeLinejoin="bevel"
        strokeDasharray="9 9"
      />
      <SvgRoadmapTree
        x={956}
        y={390}
        item={getNextItem()}
        onClick={handleClick}
      />
      <path
        d="M951.5 414C844 414 844 426 736.5 426"
        stroke="#838AA4"
        strokeWidth="2"
        strokeLinecap="round"
        strokeLinejoin="bevel"
        strokeDasharray="9 9"
      />
      <SvgRoadmapTree
        x={956}
        y={474}
        item={getNextItem()}
        onClick={handleClick}
      />
      <path
        d="M951.5 498C844 498 844 426 736.5 426"
        stroke="#838AA4"
        strokeWidth="2"
        strokeLinecap="round"
        strokeLinejoin="bevel"
        strokeDasharray="9 9"
      />
      <SvgRoadmapMilestone x={546} y={546} item={getNextItem()} />
      <path
        d="M640 196.5V277.5"
        stroke="#838AA4"
        strokeWidth="2"
        strokeLinecap="round"
        strokeLinejoin="bevel"
      />
      <path
        d="M640 334.5V397.5"
        stroke="#838AA4"
        strokeWidth="2"
        strokeLinecap="round"
        strokeLinejoin="bevel"
      />
      <path
        d="M640 454.5V541.5"
        stroke="#838AA4"
        strokeWidth="2"
        strokeLinecap="round"
        strokeLinejoin="bevel"
      />
      <SvgRoadmapTree
        x={988}
        y={570}
        item={getNextItem()}
        onClick={handleClick}
      />
      <path
        d="M738.434 569.229C872.508 545.894 872.508 569.894 983.595 593.081"
        stroke="#838AA4"
        strokeWidth="2"
        strokeLinecap="round"
        strokeLinejoin="bevel"
        strokeDasharray="9 9"
      />
      <SvgRoadmapTree
        x={988}
        y={644}
        item={getNextItem()}
        onClick={handleClick}
      />
      <path
        d="M983.5 668C861 668 861 570 738.5 570"
        stroke="#838AA4"
        strokeWidth="2"
        strokeLinecap="round"
        strokeLinejoin="bevel"
        strokeDasharray="9 9"
      />
    </svg>
  );
};
