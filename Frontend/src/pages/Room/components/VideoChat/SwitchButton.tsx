import { FunctionComponent } from 'react';
import { ThemedIcon } from '../ThemedIcon/ThemedIcon';
import { IconNames } from '../../../../constants';
import { Loader } from '../../../../components/Loader/Loader';

import './SwitchButton.css';

interface SwitchButtonProps {
  enabled: boolean;
  iconEnabledName: IconNames;
  iconDisabledName: IconNames;
  disabledColor?: boolean;
  subCaption?: string;
  loading?: boolean;
  counter?: number;
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
  onClick,
}) => {
  const iconName = enabled ? iconEnabledName : iconDisabledName;

  return (
    <div className="switch-button-container">
      <button
        className={`switch-button ${(!enabled && disabledColor) ? 'switch-button-disabled' : ''}`}
        onClick={onClick}
      >
        {loading ? (
          <Loader />
        ) : (
          <ThemedIcon name={iconName} />
        )}
        {!!counter && (
          <div className="switch-button-counter-wrapper">
            <div className="switch-button-counter">
              {counter > 99 ? '99+' : counter}
            </div>
          </div>
        )}
      </button>
      {!!subCaption && (
        <span className="switch-button-subcaption">{subCaption}</span>
      )}
    </div>
  );
};
