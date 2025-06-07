import React, { Fragment, FunctionComponent } from 'react';
import {
  RoomBusinessAnalyticTypeItem,
  RoomBusinessAnalyticTypeItemStatus,
} from '../../types/room';
import { Typography } from '../../components/Typography/Typography';

interface BusinessAnalyticRoomTableProps {
  items: RoomBusinessAnalyticTypeItem[];
}

const getStatusCount = (
  item: RoomBusinessAnalyticTypeItem,
  status: RoomBusinessAnalyticTypeItemStatus,
) => {
  const itemWithStatus = item.status.find(
    (itemStatus) => itemStatus.name === status,
  );
  if (!itemWithStatus) {
    return 0;
  }
  return itemWithStatus.count;
};

export const BusinessAnalyticRoomTable: FunctionComponent<
  BusinessAnalyticRoomTableProps
> = ({ items }) => {
  return (
    <Fragment>
      {items.map((item) => (
        <tr key={item.date}>
          <th>
            <Typography size="m">{item.date}</Typography>
          </th>
          <th>
            <Typography size="m">{getStatusCount(item, 'New')}</Typography>
          </th>
          <th>
            <Typography size="m">{getStatusCount(item, 'Active')}</Typography>
          </th>
          <th>
            <Typography size="m">{getStatusCount(item, 'Review')}</Typography>
          </th>
          <th>
            <Typography size="m">{getStatusCount(item, 'Close')}</Typography>
          </th>
        </tr>
      ))}
    </Fragment>
  );
};
