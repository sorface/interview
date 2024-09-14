import { RoomQuestionListItem, RoomQuestion } from '../types/room';

export const sortRoomQuestion = (qestion1: RoomQuestionListItem | RoomQuestion, qestion2: RoomQuestionListItem | RoomQuestion) =>
  qestion1.order - qestion2.order;
