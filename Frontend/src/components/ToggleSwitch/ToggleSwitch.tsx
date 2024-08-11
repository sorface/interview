import React, { FunctionComponent } from 'react';

import './ToggleSwitch.css';

interface ToggleSwitchProps {
  toggled: boolean;
  onToggle: () => void;
}

export const ToggleSwitch: FunctionComponent<ToggleSwitchProps> = ({
  toggled,
  onToggle,
}) => {
  return (
    <label className="toggle-switch">
      <input type="checkbox" checked={toggled} onChange={onToggle} />
      <span className="slider" />
    </label>
  );
};
