import { FunctionComponent } from 'react';
import { Icon } from '../Icon/Icon';
import { IconNames } from '../../../../constants';
import { Loader } from '../../../../components/Loader/Loader';
import { Button } from '../../../../components/Button/Button';

import './SwitchButton.css';

interface SwitchButtonProps {
  enabled: boolean;
  iconEnabledName: IconNames;
  iconDisabledName: IconNames;
  disabledColor?: boolean;
  subCaption?: string;
  loading?: boolean;
  counter?: number;
  htmlDisabled?: boolean;
  onClick: () => void;
}

export const SwitchButton: FunctionComponent<SwitchButtonProps> = ({
  enabled,
  iconDisabledName,
  iconEnabledName,
  disabledColor,
  subCaption,
  loading,
  counter,
  htmlDisabled,
  onClick,
}) => {
  const iconName = enabled ? iconEnabledName : iconDisabledName;

  return (
    <div className="switch-button-container">
      <Button
        variant='text'
        disabled={htmlDisabled}
        className={`switch-button ${(!enabled && disabledColor) ? 'switch-button-disabled' : ''}`}
        onClick={onClick}
      >
        {loading ? (
          <Loader />
        ) : (
          <Icon name={iconName} />
        )}
        {!!counter && (
          <div className="switch-button-counter-wrapper">
            <div className="switch-button-counter">
              {counter > 99 ? '99+' : counter}
            </div>
          </div>
        )}
      </Button>
      {!!subCaption && (
        <span className="switch-button-subcaption">{subCaption}</span>
      )}
    </div>
  );
};
