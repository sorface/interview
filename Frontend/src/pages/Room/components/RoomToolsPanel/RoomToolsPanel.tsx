import { FunctionComponent, ReactNode } from 'react'
import { IconNames } from '../../../../constants';
import { Button } from '../../../../components/Button/Button';
import { Loader } from '../../../../components/Loader/Loader';
import { ThemedIcon } from '../ThemedIcon/ThemedIcon';

interface WrapperProps {
  children: ReactNode;
}

const Wrapper: FunctionComponent<WrapperProps> = ({
  children,
}) => {
  return (
    <div
      className='absolute p-0.625 w-2.5 rounded-1.25 bg-dark-0.5 backdrop-blur'
      style={{
        right: '0.5rem',
        bottom: '0.5rem',
      }}
    >
      {children}
    </div>
  );
};

interface SwitchButtonProps {
  enabled: boolean;
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
  iconDisabledName,
  iconEnabledName,
  loading,
  htmlDisabled,
  roundedTop,
  roundedBottom,
  progress,
  onClick,
}) => {
  const iconName = enabled ? iconEnabledName : iconDisabledName;

  return (
    <div>
      <Button
        variant='toolsPanel'
        disabled={htmlDisabled}
        className={`w-2.5 h-2.5 z-1 ${roundedTop ? 'rounded-t-0.75' : ''} ${roundedBottom ? 'rounded-b-0.75' : ''}`}
        onClick={onClick}
      >
        {loading ? (
          <Loader />
        ) : (
          <div className='absolute'>
            <ThemedIcon name={iconName} />
          </div>
        )}
        {typeof progress === 'number' && (
          <div
            className='relative w-full h-full bg-grey3 opacity-0.5 origin-bottom'
            style={{
              transform: `scaleY(${Math.min(Math.max(progress, 0.01), 1).toFixed(2)})`,
            }}
          />)}
      </Button>
    </div>
  );
};

export const RoomToolsPanel = {
  Wrapper,
  SwitchButton,
};
