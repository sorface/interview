const getFromEnv = (varName: string) => {
  const value = process.env && process.env[varName];
  if (!value) {
    throw new Error(`process.env.${varName} are not defined`);
  }
  return value;
};

export const REACT_APP_BACKEND_URL = getFromEnv('REACT_APP_BACKEND_URL');
export const REACT_APP_WS_URL = getFromEnv('REACT_APP_WS_URL');
export const REACT_APP_BUILD_HASH = getFromEnv('REACT_APP_BUILD_HASH');
