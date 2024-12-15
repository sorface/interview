import { useCallback, useEffect, useReducer } from 'react';
import { VITE_BACKEND_URL } from '../config';

interface LogoutState {
  process: {
    logoutLoading: boolean;
    logoutError: string | null;
    logoutCode: number | null;
  };
}

const initialState: LogoutState = {
  process: {
    logoutLoading: false,
    logoutError: null,
    logoutCode: null,
  },
};

type LogoutAction =
  | {
    name: 'startLoad';
  }
  | {
    name: 'setError';
    payload: string;
  }
  | {
    name: 'setCode';
    payload: number;
  };

const logoutReducer = (
  state: LogoutState,
  action: LogoutAction,
): LogoutState => {
  switch (action.name) {
    case 'startLoad':
      return {
        process: {
          logoutLoading: true,
          logoutError: null,
          logoutCode: null,
        },
      };
    case 'setError':
      return {
        ...state,
        process: {
          logoutLoading: false,
          logoutCode: null,
          logoutError: action.payload,
        },
      };
    case 'setCode':
      return {
        ...state,
        process: {
          ...state.process,
          logoutCode: action.payload,
        },
      };
    default:
      return state;
  }
};

export const useLogout = () => {
  const [logoutState, dispatch] = useReducer(logoutReducer, initialState);
  const {
    process: { logoutCode },
  } = logoutState;

  useEffect(() => {
    if (!logoutCode || logoutCode !== 200) {
      return;
    }
    window.location.reload();
  }, [logoutCode]);

  const logout = useCallback(async () => {
    dispatch({ name: 'startLoad' });
    try {
      const response = await fetch(`${VITE_BACKEND_URL}/logout`, {
        method: 'POST',
        credentials: 'include',
      });
      if (!response.ok) {
        throw new Error('logout error');
      }
      dispatch({
        name: 'setCode',
        payload: response.status,
      });
    } catch (err: unknown) {
      dispatch({
        name: 'setError',
        payload: err instanceof Error ? err.message : 'Failed to logout',
      });
    }
  }, []);

  return {
    logoutState,
    logout,
  };
};
