import { FunctionComponent, ReactNode, useState } from 'react';
import { ThemedIcon } from '../../pages/Room/components/ThemedIcon/ThemedIcon';
import { IconNames } from '../../constants';

interface AccordionProps {
  title: ReactNode;
  className?: string;
  classNameTitle?: string;
  disabled?: boolean;
  children?: ReactNode;
}

export const Accordion: FunctionComponent<AccordionProps> = ({
  title,
  className,
  classNameTitle,
  disabled,
  children,
}) => {
  const [expanded, setExpanded] = useState(false);

  const handleExpandCollapse = () => {
    setExpanded(!expanded);
  };

  return (
    <div className={className}>
      <div className={classNameTitle}>
        {!disabled && (
          <span onClick={handleExpandCollapse} className={`cursor-pointer h-1.125 ${expanded ? 'rotate-90' : ''}`}>
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
