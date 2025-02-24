import { AnyObject } from '../types/anyObject';
import { deepEqual } from './deepEqual';

const expectCode = `
  const __expectCalls = [];
  const expect = (result, expected) => {
    __expectCalls.push([result, expected]);
  };
`;

const expectCallsReturnCode = `
  return __expectCalls;
`;

type Arg = number | string | AnyObject;

export interface ExecuteCodeResult {
  results: ExpectResults;
  error?: string;
}

export type ExpectResults = Array<{
  id: number;
  arguments: [Arg, Arg];
  passed: boolean;
}>;

export const executeCodeWithExpect = (
  code: string | undefined,
): ExecuteCodeResult => {
  try {
    const executeCodeWithExpect = new Function(
      expectCode + code + expectCallsReturnCode,
    ) as () => Array<Arg[]>;

    const executeResult = executeCodeWithExpect();

    return {
      results: executeResult.map(([result, expect]) => {
        const passed = deepEqual(result, expect);
        return {
          id: Math.random(),
          arguments: [result, expect],
          passed,
        };
      }),
    };
  } catch (error) {
    return {
      results: [],
      error: error instanceof Error ? error.message : String(error),
    };
  }
};
