const parseJson = (content: string) => {
  return JSON.parse(content);
};

export const tryCompleteJson = (content: string) => {
  const appendix = '":""}';
  for (let i = 0; i <= appendix.length; i++) {
    const currAppendix = appendix.slice(i, appendix.length);
    try {
      return parseJson(content + currAppendix);
      // eslint-disable-next-line no-empty
    } catch {}
  }

  return null;
};
