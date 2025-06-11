import React, {
  FunctionComponent,
  ReactNode,
  createContext,
  useEffect,
  useState,
} from 'react';

export type Device = 'Mobile' | 'Desktop';

export const DeviceContext = createContext<Device>('Desktop');

interface DeviceProviderProps {
  children: ReactNode;
}

const getDevice = () => (window.innerWidth <= 768 ? 'Mobile' : 'Desktop');

export const DeviceProvider: FunctionComponent<DeviceProviderProps> = ({
  children,
}) => {
  const [device, setDevice] = useState<Device>(getDevice());

  useEffect(() => {
    const handleWindowSizeChange = () => {
      setDevice(getDevice());
    };

    window.addEventListener('resize', handleWindowSizeChange, {
      passive: true,
    });
    return () => {
      window.removeEventListener('resize', handleWindowSizeChange);
    };
  }, []);

  return (
    <DeviceContext.Provider value={device}>{children}</DeviceContext.Provider>
  );
};
