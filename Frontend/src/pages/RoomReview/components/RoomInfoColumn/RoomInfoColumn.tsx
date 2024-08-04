import { FunctionComponent, ReactNode } from 'react';
import { Typography } from '../../../../components/Typography/Typography';
import { Gap } from '../../../../components/Gap/Gap';

interface RoomInfoColumnProps {
  header: string;
  conent: ReactNode;
  mini?: boolean;
}

export const RoomInfoColumn: FunctionComponent<RoomInfoColumnProps> = ({
  header,
  conent,
  mini,
}) => {
  return (
    <div className={`flex flex-col ${mini ? 'w-22' : 'w-23.125'}`}>
      <Typography size='s' bold>
        {header}
      </Typography>
      <Gap sizeRem={0.5} />
      <Typography size='m'>
        {conent}
      </Typography>
    </div>
  );
};
