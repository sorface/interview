import React, { FunctionComponent, ReactNode } from 'react';
import { IconNames } from '../../../../constants';
import { Button } from '../../../../components/Button/Button';
import { Loader } from '../../../../components/Loader/Loader';
import { Icon } from '../Icon/Icon';

interface WrapperProps {
  rightPos?: string;
  bottomPos?: string;
  children: ReactNode;
}

const Wrapper: FunctionComponent<WrapperProps> = ({
  rightPos,
  bottomPos,
  children,
}) => {
  return (
    <div
      className="absolute w-auto flex flex-col p-[0.625rem] rounded-[1.25rem] bg-room-tools-panel-bg backdrop-blur z-10"
      style={{
        right: rightPos || '0.75rem',
        bottom: bottomPos || '0.5rem',
      }}
    >
      {children}
    </div>
  );
};

interface ButtonsGroupWrapperProps {
  noPaddingBottom?: boolean;
  children: ReactNode;
}

const ButtonsGroupWrapper: FunctionComponent<ButtonsGroupWrapperProps> = ({
  noPaddingBottom,
  children,
}) => {
  return (
    <div className={`flex flex-col ${noPaddingBottom ? '' : 'pb-[1.5rem]'}`}>
      {children}
    </div>
  );
};

interface SwitchButtonProps {
  enabled: boolean;
  danger?: boolean;
  alternative?: boolean;
  iconEnabledName: IconNames;
  iconDisabledName: IconNames;
  loading?: boolean;
  htmlDisabled?: boolean;
  roundedTop?: boolean;
  roundedBottom?: boolean;
  progress?: number;
  onClick: () => void;
}

const SwitchButton: FunctionComponent<SwitchButtonProps> = ({
  enabled,
  danger,
  alternative,
  iconDisabledName,
  iconEnabledName,
  loading,
  htmlDisabled,
  progress,
  onClick,
}) => {
  const iconName = enabled ? iconEnabledName : iconDisabledName;

  return (
    <Button
      variant={
        danger
          ? 'toolsPanelDanger'
          : alternative
            ? 'toolsPanelAlternative'
            : 'toolsPanel'
      }
      disabled={htmlDisabled}
      className="w-[2.5rem] h-[2.5rem] z-1 first:rounded-t-[0.75rem] last:rounded-b-[0.75rem]"
      onClick={onClick}
    >
      {loading ? (
        <Loader />
      ) : (
        <div className="absolute z-1 flex">
          <Icon name={iconName} />
        </div>
      )}
      {typeof progress === 'number' && (
        <div
          className="relative w-full h-full bg-grey3 origin-bottom"
          style={{
            transform: `scaleY(${Math.min(Math.max(progress, 0.01), 1).toFixed(2)})`,
          }}
        />
      )}
    </Button>
  );
};

export const RoomToolsPanel = {
  Wrapper,
  ButtonsGroupWrapper,
  SwitchButton,
};
