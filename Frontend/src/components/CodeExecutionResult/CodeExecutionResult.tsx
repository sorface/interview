import React, { FunctionComponent, useState } from 'react';
import {
  ExecuteCodeResult,
  ExpectResult,
} from '../../utils/executeCodeWithExpect';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../localization';
import { Gap } from '../Gap/Gap';
import { Button } from '../Button/Button';
import { Tag, TagState } from '../Tag/Tag';
import { CodeExecutionResultInfo } from './CodeExecutionResultInfo';
import { Typography } from '../Typography/Typography';

interface CodeExecutionResultProps {
  expectResult: ExecuteCodeResult;
}

export const CodeExecutionResult: FunctionComponent<
  CodeExecutionResultProps
> = ({ expectResult }) => {
  const localizationCaptions = useLocalizationCaptions();
  const [activeResultIndex, setActiveResultIndex] = useState(0);
  const activeResult: ExpectResult | undefined =
    expectResult.results[activeResultIndex];
  const expectResultsPassed = expectResult.results.every(
    (expectResult) => expectResult.passed,
  );
  const noData = expectResult.results.length === 0;
  const statusLocalizationKey = expectResultsPassed
    ? LocalizationKey.ExpectsExecuteResultsPassed
    : LocalizationKey.ExpectsExecuteResultsNotPassed;

  const handleActiveResultIndexChange = (newActiveResult: number) => () => {
    setActiveResultIndex(newActiveResult);
  };

  if (expectResult.error) {
    return (
      <div>
        <Typography size="l" error>
          {expectResult.error}
        </Typography>
      </div>
    );
  }

  if (noData) {
    return (
      <div>
        <Typography size="l">
          {localizationCaptions[LocalizationKey.NoData]}
        </Typography>
      </div>
    );
  }

  return (
    <div>
      <div className="flex items-center">
        {expectResult.results.map((result, index) => (
          <Button
            key={result.id}
            onClick={handleActiveResultIndexChange(index)}
            variant={
              activeResultIndex === index ? 'invertedActive' : 'inverted'
            }
          >
            <div
              className={`w-0.5 h-0.5 rounded-full ${result.passed ? 'bg-green' : 'bg-red'}`}
            />
            <Gap sizeRem={0.5} horizontal />
            <div>
              {localizationCaptions[LocalizationKey.Test]} {index + 1}
            </div>
          </Button>
        ))}
        <Gap sizeRem={2} horizontal />
        <Tag
          state={
            expectResultsPassed ? TagState.Pending : TagState.WaitingForAction
          }
          typographySize="m"
          typographySemibold
        >
          {localizationCaptions[statusLocalizationKey]}
        </Tag>
      </div>
      <Gap sizeRem={1.5} />
      <CodeExecutionResultInfo
        title={localizationCaptions[LocalizationKey.ExpectsExecuteInput]}
        subtitle={JSON.stringify(activeResult?.arguments[0])}
      />
      <Gap sizeRem={0.25} />
      <CodeExecutionResultInfo
        title={
          localizationCaptions[LocalizationKey.ExpectsExecuteOutputExpected]
        }
        subtitle={JSON.stringify(activeResult?.arguments[1])}
      />
      <Gap sizeRem={0.25} />
      <CodeExecutionResultInfo
        title={localizationCaptions[LocalizationKey.ExpectsExecuteOutput]}
        subtitle={JSON.stringify(activeResult?.arguments[2])}
      />
    </div>
  );
};
