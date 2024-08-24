import { useCallback, useEffect, useReducer, useState } from 'react';
import { REACT_APP_BACKEND_URL } from '../config';
import { User } from '../types/user';
import { useRefresh } from './useRefresh';

interface GetMeState {
  process: {
    loading: boolean;
    code: number | null;
    error: string | null;
  };
  user: User | null;
}

const initialState: GetMeState = {
  process: {
    loading: false,
    code: null,
    error: null,
  },
  user: null,
};

type GetMeAction = {
  name: 'startLoad';
} | {
  name: 'setUser';
  payload: User;
} | {
  name: 'setError';
  payload: string;
} | {
  name: 'setCode';
  payload: number;
};

const getMeReducer = (state: GetMeState, action: GetMeAction): GetMeState => {
  switch (action.name) {
    case 'startLoad':
      return {
        process: {
          loading: true,
          error: null,
          code: null,
        },
        user: null,
      };
    case 'setError':
      return {
        ...state,
        process: {
          ...state.process,
          loading: false,
          error: action.payload
        }
      };
    case 'setUser':
      return {
        process: {
          ...state.process,
          loading: false,
          error: null,
        },
        user: action.payload
      };
    case 'setCode':
      return {
        ...state,
        process: {
          ...state.process,
          code: action.payload,
        },
      };
    default:
      return state;
  }
};

export const useGetMeApi = () => {
  const { refreshState: { process: { refreshCode, refreshError } }, refresh } = useRefresh();
  const [getMeState, dispatch] = useReducer(getMeReducer, initialState);
  const [requestTrigger, setRequestTrigger] = useState<number | null>(null);

  useEffect(() => {
    if (!requestTrigger) {
      return;
    }
    refresh();
  }, [requestTrigger, refresh]);

  const fetchLoadMe = useCallback(async () => {
    dispatch({ name: 'startLoad' });
    try {
      const response = await fetch(`${REACT_APP_BACKEND_URL}/users/self`, {
        credentials: 'include'
      });
      dispatch({
        name: 'setCode',
        payload: response.status,
      });
      if (!response.ok) {
        throw new Error('UserApi error');
      }
      const responseJson = await response.json();
      dispatch({ name: 'setUser', payload: responseJson });
    } catch (err: any) {
      dispatch({
        name: 'setError',
        payload: err.message || 'Failed to get me',
      });
    }
  }, []);

  useEffect(() => {
    if (!refreshCode && !refreshError) {
      return;
    }
    fetchLoadMe();
  }, [refreshCode, refreshError, fetchLoadMe]);

  const loadMe = useCallback(() => {
    setRequestTrigger(Date.now());
  }, []);

  return {
    getMeState,
    loadMe,
  };
};
