import { LocalizationKey } from '../../../localization';
import { getTreeProgress } from './getTreeProgress';

const levels = [
  LocalizationKey.RoadmapLevel0,
  LocalizationKey.RoadmapLevel1,
  LocalizationKey.RoadmapLevel2,
  LocalizationKey.RoadmapLevel3,
];

export const getRoadmapProgress = (treeIds: string[]) => {
  const doneTreesPerLevel = Math.floor(treeIds.length / levels.length);
  const doneCount = treeIds.filter(
    (treeId) => getTreeProgress(treeId) === 100,
  ).length;
  const level = Math.trunc(doneCount / doneTreesPerLevel);
  const levelProgressPercent = Math.trunc(
    ((doneCount % doneTreesPerLevel) / doneTreesPerLevel) * 100,
  );
  const levelCaption = levels[level];

  return { level, levelCaption, levelProgressPercent };
};
