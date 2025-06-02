import React, { Fragment, FunctionComponent } from 'react';
import {
  RoomBusinessAnalyticTypeItem,
  RoomBusinessAnalyticTypeItemStatus,
} from '../../types/room';

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
          <th>{item.date}</th>
          <th>{getStatusCount(item, 'New')}</th>
          <th>{getStatusCount(item, 'Active')}</th>
          <th>{getStatusCount(item, 'Review')}</th>
          <th>{getStatusCount(item, 'Close')}</th>
        </tr>
      ))}
    </Fragment>
  );
};
