import { generatePath } from 'react-router-dom';
import { Room } from '../types/room';
import { pathnames } from '../constants';

export const getRoomLink = (room: Room) => {
  switch (room.status) {
    case 'New':
    case 'Active':
      return generatePath(pathnames.room, { id: room.id });
    case 'Review':
      return generatePath(pathnames.roomReview, { id: room.id });
    case 'Close':
      return generatePath(pathnames.roomAnalytics, { id: room.id });
    default:
      return '';
  }
};
