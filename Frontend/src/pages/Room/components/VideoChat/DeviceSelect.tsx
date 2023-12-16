import { ChangeEvent, FunctionComponent, useEffect, useState } from 'react';

interface DeviceSelectProps {
  devices: MediaDeviceInfo[];
  onSelect: (deviceId: MediaDeviceInfo['deviceId']) => void;
}

export const DeviceSelect: FunctionComponent<DeviceSelectProps> = ({
  devices,
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
    <select
      value={value}
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
  );
};
