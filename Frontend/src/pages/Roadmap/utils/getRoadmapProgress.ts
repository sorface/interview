import { LocalizationKey } from '../../../localization';
import { getTreeProgress } from './getTreeProgress';

const levels = [
  LocalizationKey.RoadmapLevel0,
  LocalizationKey.RoadmapLevel1,
  LocalizationKey.RoadmapLevel2,
  LocalizationKey.RoadmapLevel3,
];

const getDoneTreesPerLevel = (treeIds: Array<string | undefined>) => {
  const doneTreesPerLevel = Math.floor(treeIds.length / levels.length);
  if (doneTreesPerLevel === 0) {
    return 1;
  }
  return doneTreesPerLevel;
};

export const getRoadmapProgress = (treeIds: Array<string | undefined>) => {
  if (treeIds.length === 0) {
    return { level: 0, levelCaption: levels[0], levelProgressPercent: 0 };
  }
  const doneCount = treeIds.filter(
    (treeId) => getTreeProgress(treeId || '') === 100,
  ).length;
  if (doneCount === treeIds.length) {
    return {
      level: levels.length - 1,
      levelCaption: levels[levels.length - 1],
      levelProgressPercent: 100,
    };
  }
  const levelProgressPercent = Math.floor((doneCount / treeIds.length) * 100);
  const doneTreesPerLevel = getDoneTreesPerLevel(treeIds);
  const level = Math.trunc(doneCount / doneTreesPerLevel);
  const levelCaption = levels[level];

  return { levelCaption, levelProgressPercent };
};
