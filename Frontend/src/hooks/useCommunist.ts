import { useCallback } from 'react';

export const useCommunist = () => {
  const cookieName = '_auth';

  const getCommunist = useCallback(() => {
    const name = cookieName + "=";
    const decodedCookie = decodeURIComponent(document.cookie);
    const ca = decodedCookie.split(';');
    for (let i = 0; i < ca.length; i++) {
      let c = ca[i];
      while (c.charAt(0) === ' ') {
        c = c.substring(1);
      }
      if (c.indexOf(name) === 0) {
        return c.substring(name.length, c.length);
      }
    }
    return '';
  }, []);

  const deleteCommunist = useCallback(() => {
    document.cookie = `${cookieName}=; Path=/; Expires=Thu, 01 Jan 1970 00:00:01 GMT;`;
  }, []);

  const resetCommunist = useCallback(() => {
    deleteCommunist();
    window.location.reload();
  }, [deleteCommunist]);

  return {
    getCommunist,
    deleteCommunist,
    resetCommunist,
  }
};
