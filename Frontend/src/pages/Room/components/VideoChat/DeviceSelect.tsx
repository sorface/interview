import { ChangeEvent, FunctionComponent, useEffect, useState } from 'react';
import { IconNames } from '../../../../constants';
import { Icon } from '../Icon/Icon';

interface DeviceSelectProps {
  devices: MediaDeviceInfo[];
  icon?: IconNames;
  localStorageKey: string;
  onSelect: (deviceId: MediaDeviceInfo['deviceId']) => void;
}

export const DeviceSelect: FunctionComponent<DeviceSelectProps> = ({
  devices,
  icon,
  localStorageKey,
  onSelect,
}) => {
  const [value, setValue] = useState<string>();

  useEffect(() => {
    const deviceIdFromStorage = localStorage.getItem(localStorageKey);
    const deviceFromStorageAvailable =
      deviceIdFromStorage &&
      devices.find(device => device.deviceId === deviceIdFromStorage);
    const newValue = deviceFromStorageAvailable ? deviceIdFromStorage : devices[0]?.deviceId;
    setValue(newValue);
    onSelect(newValue);
  }, [devices, localStorageKey, onSelect]);

  const handleChange = (event: ChangeEvent<HTMLSelectElement>) => {
    const selectedDeviceId = event.target.value;
    const selectedDevice = devices.find(device => device.deviceId === selectedDeviceId);
    if (!selectedDevice) {
      console.warn(`empty selectedDevice: ${selectedDeviceId}`);
      return;
    }
    setValue(selectedDeviceId);
    onSelect(selectedDeviceId);
    localStorage.setItem(localStorageKey, selectedDeviceId);
  };

  return (
    <>
      {icon && (
        <div className='pr-0.25'>
          <Icon name={icon} />
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
