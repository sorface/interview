import { FunctionComponent, ReactNode } from 'react';
import { Gap } from '../../../../components/Gap/Gap';
import { Typography } from '../../../../components/Typography/Typography';

interface QuestionCreateFieldProps {
  className?: string;
  children?: ReactNode;
}

export const Wrapper: FunctionComponent<QuestionCreateFieldProps> = ({
  className,
  children,
}) => {
  return (
    <div className={`flex flex-col ${className}`}>
      {children}
    </div>
  );
};

export const Label: FunctionComponent<QuestionCreateFieldProps> = ({
  className,
  children,
}) => {
  return (
    <>
      <div className={`${className}`}>
        <Typography size='m' bold>
          {children}
        </Typography>
      </div>
      <Gap sizeRem={0.5} />
    </>
  );
};

export const Content: FunctionComponent<QuestionCreateFieldProps> = ({
  className,
  children,
}) => {
  return (
    <div className={`flex-1 ${className}`}>
      {children}
    </div>
  );
};

const QuestionCreateField = {
  Wrapper,
  Label,
  Content,
};

export { QuestionCreateField };
