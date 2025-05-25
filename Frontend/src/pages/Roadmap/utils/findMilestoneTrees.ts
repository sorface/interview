import { RoadmapItem } from '../../../types/roadmap';

export const findMilestoneTrees = (
  roadmapItems: RoadmapItem[],
  milestoneItemIndex: number,
) => {
  const nextMilestoneIndex = roadmapItems.findIndex(
    (roadmapItem, roadmapItemIndex) => {
      if (roadmapItemIndex <= milestoneItemIndex) {
        return false;
      }
      return roadmapItem.type === 'Milestone';
    },
  );

  return roadmapItems.slice(
    milestoneItemIndex + 1,
    nextMilestoneIndex === -1 ? roadmapItems.length : nextMilestoneIndex,
  );
};
