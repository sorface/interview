export const getTreeProgress = (treeId: string): number => {
  const defaultValue = 0;
  try {
    const storageData = localStorage.getItem(treeId);
    if (!storageData) {
      return defaultValue;
    }
    const parsed = JSON.parse(storageData);
    if (!parsed) {
      return defaultValue;
    }
    if (typeof parsed?.all !== 'number' || typeof parsed?.closed !== 'number') {
      return defaultValue;
    }
    return ~~((parsed.closed / parsed.all) * 100);
  } catch {
    return defaultValue;
  }
};
