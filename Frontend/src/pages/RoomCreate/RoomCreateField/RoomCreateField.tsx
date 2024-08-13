import { FunctionComponent, ReactNode } from 'react';
import { Gap } from '../../../components/Gap/Gap';

interface RoomCreateFieldProps {
  className?: string;
  children?: ReactNode;
}

export const Wrapper: FunctionComponent<RoomCreateFieldProps> = ({
  className,
  children,
}) => {
  return (
    <div className={`flex flex-col ${className}`}>
      {children}
    </div>
  );
};

export const Label: FunctionComponent<RoomCreateFieldProps> = ({
  className,
  children,
}) => {
  return (
    <>
      <div className={`w-7.5 pr-0.5 ${className}`}>
        {children}
      </div>
      <Gap sizeRem={0.5} />
    </>
  );
};

export const Content: FunctionComponent<RoomCreateFieldProps> = ({
  className,
  children,
}) => {
  return (
    <div className={`flex-1 ${className}`}>
      {children}
    </div>
  );
};

const RoomCreateField = {
  Wrapper,
  Label,
  Content,
};

export { RoomCreateField };
