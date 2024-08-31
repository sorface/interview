import { FunctionComponent } from 'react';
import { Icon } from '../../pages/Room/components/Icon/Icon';
import { Gap } from '../Gap/Gap';
import { Typography } from '../Typography/Typography';
import { IconNames } from '../../constants';

interface ModalWarningContentProps {
  iconName: IconNames;
  captionLine1: string;
  captionLine2: string;
  dangerIcon?: boolean;
}

export const ModalWarningContent: FunctionComponent<ModalWarningContentProps> = ({
  iconName,
  captionLine1,
  captionLine2,
  dangerIcon,
}) => {
  return (
    <div className='flex'>
      <Icon size='xxl' danger={dangerIcon} name={iconName} />
      <Gap sizeRem={1} horizontal />
      <div className='flex flex-col text-left'>
        <Typography size='xxl' bold>
          {captionLine1}
        </Typography>
        <Gap sizeRem={0.25} />
        <Typography size='l'>
          {captionLine2}
        </Typography>
      </div>
    </div>
  );
};
