import { FunctionComponent, ReactNode } from 'react';

interface RoomCreateFieldProps {
  className?: string;
  children?: ReactNode;
}

export const Wrapper: FunctionComponent<RoomCreateFieldProps> = ({
  className,
  children,
}) => {
  return (
    <div className={`flex items-center ${className}`}>
      {children}
    </div>
  );
};

export const Label: FunctionComponent<RoomCreateFieldProps> = ({
  className,
  children,
}) => {
  return (
    <div className={`w-7.5 pr-0.5 ${className}`}>
      {children}
    </div>
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
