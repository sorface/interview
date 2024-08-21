import { useCallback, useReducer } from 'react';
import { REACT_APP_BACKEND_URL } from '../config';
import { getCookie } from '../utils/getCookie';

interface RefreshState {
  process: {
    refreshLoading: boolean;
    refreshError: string | null;
    refreshCode: number | null;
  };
}

const initialState: RefreshState = {
  process: {
    refreshLoading: false,
    refreshError: null,
    refreshCode: null,
  },
};

type RefreshAction = {
  name: 'startLoad';
} | {
  name: 'setError';
  payload: string;
} | {
  name: 'setCode';
  payload: number;
};

const refreshCookieName = 'ate_t';
const refreshDeadlineShiftMinutes = 30;

const refreshReducer = (state: RefreshState, action: RefreshAction): RefreshState => {
  switch (action.name) {
    case 'startLoad':
      return {
        process: {
          refreshLoading: true,
          refreshError: null,
          refreshCode: null,
        },
      };
    case 'setError':
      return {
        ...state,
        process: {
          refreshLoading: false,
          refreshCode: null,
          refreshError: action.payload
        }
      };
    case 'setCode':
      return {
        ...state,
        process: {
          ...state.process,
          refreshCode: action.payload,
        },
      };
    default:
      return state;
  }
};

export const useRefresh = () => {
  const [refreshState, dispatch] = useReducer(refreshReducer, initialState);

  const refresh = useCallback(async () => {
    dispatch({ name: 'startLoad' });
    try {
      const response = await fetch(`${REACT_APP_BACKEND_URL}/refresh`, {
        method: 'POST',
        credentials: 'include'
      });
      if (!response.ok) {
        throw new Error('refresh error');
      }
      dispatch({
        name: 'setCode',
        payload: response.status,
      });
    } catch (err: any) {
      dispatch({
        name: 'setError',
        payload: err.message || 'Failed to refresh',
      });
    }
  }, []);

  const checkRefreshNecessity = useCallback(() => {
    try {
      const refreshValue = getCookie(refreshCookieName);
      if (!refreshValue) {
        return true;
      }
      const refreshDeadline = new Date(parseInt(refreshValue) * 1000);
      refreshDeadline.setMinutes(
        refreshDeadline.getMinutes() - refreshDeadlineShiftMinutes
      );
      if (Date.now() > refreshDeadline.getTime()) {
        return true;
      }
    } catch {
      return true;
    }
    return false;
  }, []);

  return {
    refreshState,
    checkRefreshNecessity,
    refresh,
  };
};
