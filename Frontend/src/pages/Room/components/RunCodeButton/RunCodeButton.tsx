import React, { FunctionComponent } from 'react';
import { Button } from '../../../../components/Button/Button';
import { useLocalizationCaptions } from '../../../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../../../localization';
import './RunCodeButton.css';
import { compare } from './compare';

interface RunCodeButtonProps {
  stringifyFunction: string | undefined;
}

const NAME_OF_COMPARE_FUNCTION = 'expect';

const extractFunctionAndArgs = (inputString: string) => {
  const startIndex = inputString.indexOf(`(`); // expect( <--
  const bracketToCompare = { '(': ')' };
  const stack = [];
  let lastIndex = 0;

  // цикл чтобы выделить тело функции и ее аргументы
  for (let i = startIndex + 1; i < inputString.length; i++) {
    const current = inputString[i];

    if (current in bracketToCompare) stack.push(current);

    if (current === bracketToCompare['(']) {
      stack.pop();
      lastIndex = i;

      if (stack.length === 0) break;
    }
  }

  return {
    fnBodyWithArguments: inputString.slice(startIndex + 1, lastIndex + 1),
    expectArgument: inputString
      .slice(lastIndex + 2, inputString.lastIndexOf(')'))
      .trim(),
  };
};

export const RunCodeButton: FunctionComponent<RunCodeButtonProps> = ({
  stringifyFunction,
}) => {
  const localizationCaptions = useLocalizationCaptions();

  const handleStart = () => {
    if (stringifyFunction) {
      const compareFunctionIndex = stringifyFunction.indexOf(
        NAME_OF_COMPARE_FUNCTION,
      );
      const functionToCall = stringifyFunction.slice(0, compareFunctionIndex);
      const functionToCompare = stringifyFunction.slice(compareFunctionIndex);
      const functinosToCompare = functionToCompare
        .trim()
        .replaceAll('\r\n', '')
        .split(';')
        .filter((value) => value !== '');

      if (functinosToCompare.length > 1) {
        const result: boolean[] = [];
        functinosToCompare.forEach((expectString) => {
          const functionWithArguments = extractFunctionAndArgs(expectString);
          const mergeFunctionBodyWithCall =
            functionToCall + `` + functionWithArguments.fnBodyWithArguments;

          const compareResults = compare(
            eval(mergeFunctionBodyWithCall),
            functionWithArguments.expectArgument,
          );
          result.push(compareResults);
        });
        console.log(result);
      } else {
        const functionWithArguments = extractFunctionAndArgs(functionToCompare);
        const mergeFunctionBodyWithCall =
          functionToCall + `` + functionWithArguments.fnBodyWithArguments;

        const compareResults = compare(
          eval(mergeFunctionBodyWithCall),
          functionWithArguments.expectArgument,
        );

        console.log(compareResults);
      }
    }
  };

  return (
    <Button
      className="run-code-button"
      variant="inverted"
      onClick={handleStart}
      disabled={!stringifyFunction?.length}
    >
      {localizationCaptions[LocalizationKey.Run]}
    </Button>
  );
};
