import { ChangeEvent, FunctionComponent, useEffect, useState } from 'react';
import { IconNames } from '../../../../constants';
import { ThemedIcon } from '../ThemedIcon/ThemedIcon';

interface DeviceSelectProps {
  devices: MediaDeviceInfo[];
  icon?: IconNames;
  onSelect: (deviceId: MediaDeviceInfo['deviceId']) => void;
}

export const DeviceSelect: FunctionComponent<DeviceSelectProps> = ({
  devices,
  icon,
  onSelect,
}) => {
  const [value, setValue] = useState<string>();

  useEffect(() => {
    const newValue = devices[0]?.deviceId;
    setValue(newValue);
    onSelect(newValue);
  }, [devices, onSelect]);

  const handleChange = (event: ChangeEvent<HTMLSelectElement>) => {
    const selectedDeviceId = event.target.value;
    const selectedDevice = devices.find(device => device.deviceId === selectedDeviceId);
    if (!selectedDevice) {
      console.warn(`empty selectedDevice: ${selectedDeviceId}`);
      return;
    }
    setValue(selectedDeviceId);
    onSelect(selectedDeviceId);
  };

  return (
    <>
      {icon && (
        <div className='pr-0.25'>
          <ThemedIcon name={icon} />
        </div>
      )}
      <select
        value={value}
        className='w-full'
        onChange={handleChange}
      >
        {devices.map(device => (
          <option
            key={device.deviceId}
            value={device.deviceId}
          >
            {device.label}
          </option>
        ))}
      </select>
    </>
  );
};
