const storageKey = 'DevAuthorization';

export const getDevAuthorization = () => {
  const authorization = localStorage.getItem(storageKey);
  if (!authorization || import.meta.env.MODE !== 'development') {
    return;
  }
  return {
    Authorization: `DevBearer ${authorization}`,
  };
};

export const setDevAuthorization = (value: string) => {
  localStorage.setItem(storageKey, value);
};

export const clearDevAuthorization = () => {
  localStorage.removeItem(storageKey);
};
