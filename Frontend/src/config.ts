const getFromEnv = (varName: string) => {
  const value = process.env && process.env[varName];
  if (!value) {
    throw new Error(`process.env.${varName} are not defined`);
  }
  return value;
};

const getFromEnvOrDefault = (varName: string, defaultValue: string) => {
  try {
    return getFromEnv(varName);
  } catch {
    return defaultValue;
  }
};

export const REACT_APP_BACKEND_URL = getFromEnv('REACT_APP_BACKEND_URL');
export const REACT_APP_WS_URL = getFromEnv('REACT_APP_WS_URL');
export const REACT_APP_BUILD_HASH = getFromEnv('REACT_APP_BUILD_HASH');
export const REACT_APP_FEEDBACK_IFRAME_URL = getFromEnvOrDefault(
  'REACT_APP_FEEDBACK_IFRAME_URL',
  '',
);
