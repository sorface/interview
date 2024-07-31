import { FunctionComponent, ReactNode } from 'react';
import { Typography } from '../../../../components/Typography/Typography';
import { Gap } from '../../../../components/Gap/Gap';

interface RoomInfoColumnProps {
  header: string;
  conent: ReactNode;
}

export const RoomInfoColumn: FunctionComponent<RoomInfoColumnProps> = ({
  header,
  conent,
}) => {
  return (
    <div className='flex flex-col w-23.125'>
      <Typography size='s' bold>
        {header}
      </Typography>
      <Gap sizeRem={0.5} />
      <Typography size='s'>
        {conent}
      </Typography>
    </div>
  );
};
