const getFromEnv = (varName: string) => {
  const value = import.meta.env && import.meta.env[varName];
  if (!value) {
    throw new Error(`import.meta.env.${varName} are not defined`);
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

export const VITE_BACKEND_URL = getFromEnv('VITE_BACKEND_URL');
export const VITE_WS_URL = getFromEnv('VITE_WS_URL');
export const VITE_BUILD_HASH = getFromEnv('VITE_BUILD_HASH');
export const VITE_NAME = getFromEnvOrDefault('VITE_APP_NAME', 'Interview Platform');
export const VITE_FEEDBACK_IFRAME_URL = getFromEnvOrDefault(
  'VITE_FEEDBACK_IFRAME_URL',
  '',
);
