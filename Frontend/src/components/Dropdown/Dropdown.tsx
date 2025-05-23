import React, {
  FunctionComponent,
  ReactNode,
  useEffect,
  useRef,
  useState,
} from 'react';
import { Button, ButtonProps } from '../Button/Button';
import { Icon } from '../../pages/Room/components/Icon/Icon';
import { IconNames } from '../../constants';

export interface DropdownProps {
  toggleContent: ReactNode;
  toggleClassName?: string;
  toggleIcon?: boolean;
  contentClassName?: string;
  contentStyle?: React.CSSProperties | undefined;
  useButton?: boolean;
  buttonVariant?: ButtonProps['variant'];
  children: ReactNode;
}

export const Dropdown: FunctionComponent<DropdownProps> = ({
  toggleContent,
  toggleClassName,
  toggleIcon,
  contentClassName,
  contentStyle,
  useButton,
  buttonVariant,
  children,
}) => {
  const containerRef = useRef<HTMLDivElement>(null);
  const [open, setOpen] = useState(false);

  const handleToggle = () => {
    setOpen(!open);
  };

  useEffect(() => {
    const handler = (e: MouseEvent) => {
      if (!e.target || !open) {
        return;
      }
      const containerRefTarget = containerRef.current?.contains(
        e.target as HTMLElement,
      );
      if (!containerRefTarget) {
        setOpen(false);
      }
    };

    window.addEventListener('click', handler);
    return () => {
      window.removeEventListener('click', handler);
    };
  });

  return (
    <div ref={containerRef} className="relative">
      {useButton ? (
        <Button
          aria-expanded={open}
          variant={buttonVariant}
          className={toggleClassName}
          onClick={handleToggle}
        >
          {toggleContent}
          {toggleIcon && (
            <span
              className={`cursor-pointer h-[1.125rem] ${open ? 'rotate-90' : ''}`}
            >
              <Icon size="s" name={IconNames.ChevronForward} />
            </span>
          )}
        </Button>
      ) : (
        <div
          aria-expanded={open}
          className={toggleClassName}
          onClick={handleToggle}
        >
          {toggleContent}
        </div>
      )}
      {open && (
        <div
          className={`${contentClassName} absolute overflow-auto z-50 translate-y-[0.25rem] shadow-dark-medium`}
          style={contentStyle}
        >
          {children}
        </div>
      )}
    </div>
  );
};
