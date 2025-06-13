import { AnyObject } from '../types/anyObject';
import { deepEqual } from './deepEqual';

const expectCode = `
  return new Promise((res) => {
  const __expectCalls = [];
  const __consoleLogs = [];
  let __expectConsoleCalled = false;
  let __expectedConsoleLogs = [];

  const originalConsoleLog = console.log;

  const finishConsoleLogSpy = () => {
    console.log = originalConsoleLog;
    __consoleLogs.forEach((log, index) =>
      __expectCalls.push([log, __expectedConsoleLogs[index]])
    );
    res(__expectCalls);
  };

  console.log = (arg) => {
    __consoleLogs.push(arg);
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
    __expectConsoleCalled = true;
    __expectedConsoleLogs = expectedCalls;
    setTimeout(() => {
      finishConsoleLogSpy();
    }, 333);
  };
`;

const expectCallsReturnCode = `
  if (!__expectConsoleCalled) {
    console.log = originalConsoleLog;
    res(__expectCalls);
  }
});
`;

const expectCodeForIframeStart = `
  <script>
    const __expectCalls = [];
    const expectValue = (value1, value2) => {
      __expectCalls.push([value1, value2]);
    };
  </script>
`;

const expectCodeForIframeEnd = `
  <script>
    window.parent.postMessage(__expectCalls, '*');
  </script>
`;

export type ExecuteCodeArg = number | string | AnyObject;

export interface ExecuteCodeResult {
  results: ExpectResult[];
  error?: string;
}

export type ExpectResult = {
  id: number;
  arguments:
    | [ExecuteCodeArg, ExecuteCodeArg, ExecuteCodeArg]
    | [ExecuteCodeArg, ExecuteCodeArg];
  passed: boolean;
};

export const computEexecuteResult = (
  expectResult: Array<ExecuteCodeArg[]>,
): ExecuteCodeResult => {
  try {
    return {
      results: expectResult.map((res) => {
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
    console.error(error);
    return {
      results: [],
      error: error instanceof Error ? error.message : String(error),
    };
  }
};

export const executeCodeWithExpect = async (
  code: string | undefined,
): Promise<ExecuteCodeResult> => {
  try {
    const executeCodeWithExpect = new Function(
      expectCode + code + expectCallsReturnCode,
    ) as () => Promise<Array<ExecuteCodeArg[]>>;

    const executeResult = await executeCodeWithExpect();
    return computEexecuteResult(executeResult);
  } catch (error) {
    console.error(error);
    return {
      results: [],
      error: error instanceof Error ? error.message : String(error),
    };
  }
};

export const getSrcForIframe = (code: string) => {
  const resultCode = expectCodeForIframeStart + code + expectCodeForIframeEnd;
  return `data:text/html;charset=utf-8,${encodeURIComponent(resultCode)}`;
};
