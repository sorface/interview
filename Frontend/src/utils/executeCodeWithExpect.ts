import { AnyObject } from '../types/anyObject';
import { deepEqual } from './deepEqual';

const expectCode = `
  return new Promise((res) => {
  const __expectCalls = [];
  const __consoleLogs = [];
  let __expectedConsoleLogs = [];

  const originalConsoleLog = console.log;
  console.log = (arg) => {
    __consoleLogs.push(arg);
    if (__expectedConsoleLogs.length === __consoleLogs.length) {
      console.log = originalConsoleLog;
      __expectedConsoleLogs.forEach((expectedLog, index) =>
        __expectCalls.push([__consoleLogs[index], expectedLog])
      );
      res(__expectCalls);
    }
  };

  const expect = (fn, ...argsWithExpected) => {
    const args = [...argsWithExpected];
    args.length--;
    const expected = argsWithExpected[argsWithExpected.length - 1];
    const result = args.reduce((accum, val) => {
      if (accum === undefined) {
        return fn(...val);
      }
      return accum(...val);
    }, undefined);
    __expectCalls.push([args, result, expected]);
  };

  const expectValue = (value1, value2) => {
    __expectCalls.push([value1, value2]);
  };

  const expectConsole = (expectedCalls) => {
    __expectedConsoleLogs = expectedCalls;
  };
`;

const expectCallsReturnCode = `
  if (__expectedConsoleLogs.length === 0) {
    console.log = originalConsoleLog;
    res(__expectCalls);
  }
});
`;

type Arg = number | string | AnyObject;

export interface ExecuteCodeResult {
  results: ExpectResult[];
  error?: string;
}

export type ExpectResult = {
  id: number;
  arguments: [Arg, Arg, Arg] | [Arg, Arg];
  passed: boolean;
};

export const executeCodeWithExpect = async (
  code: string | undefined,
): Promise<ExecuteCodeResult> => {
  try {
    const executeCodeWithExpect = new Function(
      expectCode + code + expectCallsReturnCode,
    ) as () => Promise<Array<Arg[]>>;

    const executeResult = await executeCodeWithExpect();

    return {
      results: executeResult.map((res) => {
        const withoutArgs = res.length === 2;
        const [args, result, expect] = res;
        const passed = withoutArgs
          ? deepEqual(args, result)
          : deepEqual(result, expect);
        return {
          id: Math.random(),
          arguments: withoutArgs ? [args, result] : [args, result, expect],
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
