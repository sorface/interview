import { AnyObject } from '../types/anyObject';
import { CodeEditorLang } from '../types/question';
import { deepEqual } from './deepEqual';

const expectCodeForIframeStart = `
  <script>
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
      window.parent.postMessage(__expectCalls, '*');
    };

    console.log = (arg) => {
      __consoleLogs.push(arg);
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

  </script>
`;

const expectCodeForIframeEnd = `
  <script>
    if (!__expectConsoleCalled) {
      console.log = originalConsoleLog;
      window.parent.postMessage(__expectCalls, '*');
    }
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

export const getSrcForIframe = (code: string, language?: CodeEditorLang) => {
  let resultCode;
  if (language === CodeEditorLang.Javascript) {
    resultCode =
      expectCodeForIframeStart +
      `<script>${code}</script>` +
      expectCodeForIframeEnd;
  } else {
    resultCode = expectCodeForIframeStart + code + expectCodeForIframeEnd;
  }
  return `data:text/html;charset=utf-8,${encodeURIComponent(resultCode)}`;
};
