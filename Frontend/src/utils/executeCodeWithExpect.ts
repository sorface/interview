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

export type ExpectResults = Array<{
  id: number;
  arguments: [Arg, Arg];
  passed: boolean;
}>;

export const executeCodeWithExpect = (code: string | undefined) => {
  const executeCodeWithExpect = new Function(
    expectCode + code + expectCallsReturnCode,
  ) as () => Array<Arg[]>;

  const res: ExpectResults = [];

  executeCodeWithExpect().forEach(([result, expect]) => {
    const passed = deepEqual(result, expect);
    res.push({
      id: Math.random(),
      arguments: [result, expect],
      passed,
    });
  });

  return res;
};
