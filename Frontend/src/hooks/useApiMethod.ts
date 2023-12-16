import { useCallback, useReducer } from 'react';
import { useNavigate } from 'react-router-dom';
import { REACT_APP_BACKEND_URL } from '../config';
import { pathnames } from '../constants';
import { ApiContract } from '../types/apiContracts';
import { useCommunist } from './useCommunist';

interface ApiMethodState<ResponseData = any> {
  process: {
    loading: boolean;
    error: string | null;
  };
  data: ResponseData | null;
}

const initialState: ApiMethodState = {
  process: {
    loading: false,
    error: null,
  },
  data: null,
};

type ApiMethodAction = {
  name: 'startLoad';
} | {
  name: 'setData';
  payload: any;
} | {
  name: 'setError';
  payload: string;
};

const apiMethodReducer = (state: ApiMethodState, action: ApiMethodAction): ApiMethodState => {
  switch (action.name) {
    case 'startLoad':
      return {
        process: {
          loading: true,
          error: null,
        },
        data: null,
      };
    case 'setError':
      return {
        ...state,
        process: {
          loading: false,
          error: action.payload
        }
      };
    case 'setData':
      return {
        process: {
          loading: false,
          error: null,
        },
        data: action.payload
      };
    default:
      return state;
  }
};
const unauthorizedHttpCode = 401;

const createUrlParam = (name: string, value: string) =>
  `${encodeURIComponent(name)}=${encodeURIComponent(value)}`;

const createFetchUrl = (apiContract: ApiContract) => {
  if (apiContract.urlParams) {
    const params =
      Object.entries(apiContract.urlParams)
        .map(([paramName, paramValue]) => {
          if (Array.isArray(paramValue)) {
            return paramValue.map(val => createUrlParam(paramName, val)).join('&');
          }
          return createUrlParam(paramName, paramValue);
        })
        .join('&');
    return `${REACT_APP_BACKEND_URL}${apiContract.baseUrl}?${params}`;
  }
  return `${REACT_APP_BACKEND_URL}${apiContract.baseUrl}`;
};

const createFetchRequestInit = (apiContract: ApiContract) => {
  if (apiContract.method === 'GET') {
    return undefined;
  }
  return {
    method: apiContract.method,
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(apiContract.body),
  };
};

type AnyObject = Record<string, any>;

const getResponseContent = async (response: Response): Promise<AnyObject | string> => {
  const contentType = response.headers.get('content-type');
  if (!contentType || !contentType.includes('application/json')) {
    return await response.text();
  }
  return await response.json();
};

const getResponseError = (
  response: Response,
  responseContent: AnyObject | string,
  apiContract: ApiContract
) => {
  if (
    typeof responseContent === 'string' ||
    !responseContent.message
  ) {
    return `${apiContract.method} ${apiContract.baseUrl} ${response.status}`;
  }
  return responseContent.message;
}

export const useApiMethod = <ResponseData, RequestData = AnyObject>(apiContractCall: (data: RequestData) => ApiContract) => {
  const [apiMethodState, dispatch] = useReducer(apiMethodReducer, initialState);
  const navigate = useNavigate();
  const { deleteCommunist } = useCommunist();

  const fetchData = useCallback(async (requestData: RequestData) => {
    dispatch({ name: 'startLoad' });
    const apiContract = apiContractCall(requestData);
    try {
      const response = await fetch(
        createFetchUrl(apiContract),
        createFetchRequestInit(apiContract),
      );
      if (response.status === unauthorizedHttpCode) {
        deleteCommunist();
        navigate(pathnames.home.replace(':redirect?', ''));
        return;
      }

      const responseData = await getResponseContent(response);
      if (!response.ok) {
        const errorMessage = getResponseError(response, responseData, apiContract);
        throw new Error(errorMessage);
      }
      dispatch({ name: 'setData', payload: responseData });
    } catch (err: any) {
      dispatch({
        name: 'setError',
        payload: err.message || `Failed to fetch ${apiContract.method} ${apiContract.baseUrl}`,
      });
    }
  }, [apiContractCall, deleteCommunist, navigate]);

  return {
    apiMethodState: apiMethodState as ApiMethodState<ResponseData>,
    fetchData,
  };
};
