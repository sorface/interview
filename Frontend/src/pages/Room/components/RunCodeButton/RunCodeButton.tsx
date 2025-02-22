import React, { FunctionComponent } from 'react';
import { Button } from '../../../../components/Button/Button';
import { useLocalizationCaptions } from '../../../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../../../localization';
import './RunCodeButton.css';
import { deepEqual } from '../../../../utils/deepEqual';

interface RunCodeButtonProps {
  codeEditorText: string | undefined;
  handleExpectResults: (result: Res) => void;
}

const expectCode = `
const __expectCalls = [];
const expect = (result, expected) => {
  __expectCalls.push([result, expected]);
};`;
const expectCallsReturnCode = `
  return __expectCalls;
`;

export type Res = {
  [idx: number]: {
    arguments: Array<[unknown, unknown]>;
    result: boolean;
  };
};

export const RunCodeButton: FunctionComponent<RunCodeButtonProps> = ({
  codeEditorText,
  handleExpectResults,
}) => {
  const localizationCaptions = useLocalizationCaptions();

  const handleStart = () => {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    const executeCodeWithExpect: () => Array<any[]> = new Function(
      expectCode + codeEditorText + expectCallsReturnCode,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    ) as () => Array<any[]>;

    const res: Res = {};

    executeCodeWithExpect().forEach(([result, expect], idx) => {
      const newExpect = deepEqual(result, expect);
      res[idx] = {
        arguments: [[result, expect]],
        result: newExpect,
      };
    });

    handleExpectResults(res);
  };

  return (
    <Button
      className="run-code-button"
      variant="inverted"
      onClick={handleStart}
      disabled={!codeEditorText?.length}
    >
      {localizationCaptions[LocalizationKey.Run]}
    </Button>
  );
};
