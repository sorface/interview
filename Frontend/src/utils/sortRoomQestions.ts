import { RoomQuestionListItem } from '../types/room';

export const sortRoomQestions = (qestion1: RoomQuestionListItem, qestion2: RoomQuestionListItem) =>
  qestion1.order - qestion2.order;
