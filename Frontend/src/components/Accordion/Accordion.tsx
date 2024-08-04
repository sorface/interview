import { FunctionComponent, ReactNode, useState } from 'react';
import { ThemedIcon } from '../../pages/Room/components/ThemedIcon/ThemedIcon';
import { IconNames } from '../../constants';

interface AccordionProps {
  title: ReactNode;
  className?: string;
  classNameTitle?: string;
  disabled?: boolean;
  children?: ReactNode;
  onClick?: () => void;
}

export const Accordion: FunctionComponent<AccordionProps> = ({
  title,
  className,
  classNameTitle,
  disabled,
  children,
  onClick,
}) => {
  const [expanded, setExpanded] = useState(false);
  const cursorPointer = !disabled || onClick;

  const handleOnClick = () => {
    if (onClick) {
      onClick();
      return;
    }
    setExpanded(!expanded);
  };

  return (
    <div className={className}>
      <div
        className={`${classNameTitle} ${cursorPointer ? 'cursor-pointer' : ''}`}
        onClick={disabled && !onClick ? undefined : handleOnClick}
      >
        {!disabled && (
          <span className={`cursor-pointer h-1.125 ${expanded ? 'rotate-90' : ''}`}>
            <ThemedIcon name={IconNames.ChevronForward} size='small' />
          </span>
        )}
        {title}
      </div>
      <div>
        {expanded && children}
      </div>
    </div>
  );
};
