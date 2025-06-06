import React, { FunctionComponent } from 'react';
import { Typography } from '../Typography/Typography';
import { Gap } from '../Gap/Gap';
import { useThemeClassName } from '../../hooks/useThemeClassName';
import { Theme } from '../../context/ThemeContext';

interface CodeExecutionResultInfoProps {
  title: string;
  subtitle: string;
}

export const CodeExecutionResultInfo: FunctionComponent<
  CodeExecutionResultInfoProps
> = ({ title, subtitle }) => {
  const themedClassName = useThemeClassName({
    [Theme.Dark]: 'bg-dark-dark2',
    [Theme.Light]: 'bg-dark-white',
  });

  return (
    <div
      className={`flex flex-col text-left rounded-[1.125rem] px-[1rem] py-[0.5rem] ${themedClassName}`}
    >
      <Typography size="m" semibold>
        {title}
      </Typography>
      <Gap sizeRem={0.5} />
      <Typography size="m">{subtitle}</Typography>
    </div>
  );
};
