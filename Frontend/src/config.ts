const getFromEnv = (varName: string): string => {
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
export const VITE_APP_NAME = getFromEnv('VITE_APP_NAME');
export const VITE_FEEDBACK_IFRAME_URL = getFromEnvOrDefault(
  'VITE_FEEDBACK_IFRAME_URL',
  '',
);
export const VITE_AI_API = getFromEnv('VITE_AI_API');
