import { useCallback, useEffect, useReducer } from 'react';
import { VITE_BACKEND_URL } from '../config';
import { pathnames, HttpResponseCode } from '../constants';
import { ApiContract } from '../types/apiContracts';
import { useLogout } from './useLogout';
import { useNavigate } from 'react-router-dom';
import { AnyObject } from '../types/anyObject';
import { getDevAuthorization } from '../utils/devAuthorization';

interface ApiMethodState<ResponseData = AnyObject | string> {
  process: {
    loading: boolean;
    code: number | null;
    error: string | null;
  };
  data: ResponseData | null;
}

const initialState: ApiMethodState = {
  process: {
    loading: false,
    code: null,
    error: null,
  },
  data: null,
};

type ApiMethodAction =
  | {
      name: 'startLoad';
    }
  | {
      name: 'setData';
      payload: AnyObject | string;
    }
  | {
      name: 'setError';
      payload: string;
    }
  | {
      name: 'setCode';
      payload: number;
    };

const apiMethodReducer = (
  state: ApiMethodState,
  action: ApiMethodAction,
): ApiMethodState => {
  switch (action.name) {
    case 'startLoad':
      return {
        process: {
          loading: true,
          error: null,
          code: null,
        },
        data: null,
      };
    case 'setError':
      return {
        ...state,
        process: {
          ...state.process,
          loading: false,
          error: action.payload,
        },
      };
    case 'setData':
      return {
        process: {
          ...state.process,
          loading: false,
          error: null,
        },
        data: action.payload,
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

const createUrlParam = (name: string, value: string) =>
  `${encodeURIComponent(name)}=${encodeURIComponent(value)}`;

const createFetchUrl = (apiContract: ApiContract) => {
  if (apiContract.urlParams) {
    const params = Object.entries(apiContract.urlParams)
      .map(([paramName, paramValue]) => {
        if (Array.isArray(paramValue)) {
          return paramValue
            .map((val) => createUrlParam(paramName, val))
            .join('&');
        }
        return createUrlParam(paramName, paramValue);
      })
      .join('&');
    return `${VITE_BACKEND_URL}${apiContract.baseUrl}?${params}`;
  }
  return `${VITE_BACKEND_URL}${apiContract.baseUrl}`;
};

const createFetchRequestInit = (apiContract: ApiContract) => {
  const defaultRequestInit: RequestInit = {
    credentials: 'include',
    method: apiContract.method,
    headers: getDevAuthorization(),
  };

  if (apiContract.method === 'GET') {
    return defaultRequestInit;
  }

  return {
    ...defaultRequestInit,
    headers: {
      ...defaultRequestInit.headers,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(apiContract.body),
  };
};

const getResponseContent = async (
  response: Response,
): Promise<AnyObject | string> => {
  const contentType = response.headers.get('content-type');
  if (!contentType || !contentType.includes('application/json')) {
    return await response.text();
  }
  return await response.json();
};

const getResponseError = (
  response: Response,
  responseContent: AnyObject | string,
  apiContract: ApiContract,
) => {
  if (typeof responseContent === 'string' || !responseContent.message) {
    return `${apiContract.method} ${apiContract.baseUrl} ${response.status}`;
  }
  return responseContent.message;
};

export const useApiMethod = <ResponseData, RequestData = AnyObject>(
  apiContractCall: (data: RequestData) => ApiContract,
) => {
  const navigate = useNavigate();
  const [apiMethodState, dispatch] = useReducer(apiMethodReducer, initialState);
  const { logoutState, logout } = useLogout();
  const {
    process: { logoutError },
  } = logoutState;

  useEffect(() => {
    if (!logoutError) {
      return;
    }
    navigate(pathnames.logoutError);
  }, [logoutError, navigate]);

  const fetchData = useCallback(
    async (requestData: RequestData) => {
      dispatch({ name: 'startLoad' });
      const apiContract = apiContractCall(requestData);
      try {
        const response = await fetch(
          createFetchUrl(apiContract),
          createFetchRequestInit(apiContract),
        );
        dispatch({
          name: 'setCode',
          payload: response.status,
        });
        if (response.status === HttpResponseCode.Unauthorized) {
          logout();
          return;
        }

        const responseData = await getResponseContent(response);
        if (!response.ok) {
          const errorMessage = getResponseError(
            response,
            responseData,
            apiContract,
          );
          throw new Error(errorMessage);
        }
        dispatch({ name: 'setData', payload: responseData });
      } catch (err: unknown) {
        console.error(err);
        dispatch({
          name: 'setError',
          payload:
            err instanceof Error
              ? err.message
              : `Failed to fetch ${apiContract.method} ${apiContract.baseUrl}`,
        });
      }
    },
    [apiContractCall, logout],
  );

  return {
    apiMethodState: apiMethodState as ApiMethodState<ResponseData>,
    fetchData,
  };
};
