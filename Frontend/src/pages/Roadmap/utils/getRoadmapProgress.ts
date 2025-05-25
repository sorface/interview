import { LocalizationKey } from '../../../localization';
import { getTreeProgress } from './getTreeProgress';

const levels = [
  LocalizationKey.RoadmapLevel0,
  LocalizationKey.RoadmapLevel1,
  LocalizationKey.RoadmapLevel2,
  LocalizationKey.RoadmapLevel3,
];

export const getRoadmapProgress = (treeIds: Array<string | undefined>) => {
  if (treeIds.length === 0) {
    return { level: 0, levelCaption: levels[0], levelProgressPercent: 0 };
  }
  const doneTreesPerLevel = Math.floor(treeIds.length / levels.length);
  const doneCount = treeIds.filter(
    (treeId) => getTreeProgress(treeId || '') === 100,
  ).length;
  const level = Math.trunc(doneCount / doneTreesPerLevel);
  const levelProgressPercent = Math.trunc((levels.length / level) * 10);
  const levelCaption = levels[level];

  return { level, levelCaption, levelProgressPercent };
};
