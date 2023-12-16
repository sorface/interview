import { Question } from './question';
import { Tag } from './tag';
import { User, UserType } from './user';

export type RoomStatus = 'New' | 'Active' | 'Review' | 'Close';

export interface Room {
  id: string;
  name: string;
  twitchChannel: string;
  questions: Question[];
  users: User[];
  tags: Tag[];
  roomStatus: RoomStatus;
}

export type RoomStateType = 'CodeEditor';

export type RoomStateAdditionalStatefulPayload = {
  enabled: boolean;
};

interface RoomStateAdditional {
  type: RoomStateType;
  payload: string;
}

export interface RoomState {
  id: Room['id'];
  name: Room['name'];
  likeCount: number;
  dislikeCount: number;
  codeEditorContent: string;
  activeQuestion: Question;
  states: RoomStateAdditional[];
}

export interface RoomReview {
  id: string;
  user: User;
  roomId: Room['id'],
  review: string;
  state: 'Open' | 'Closed';
}

export interface RoomParticipant {
  id: string;
  roomId: Room['id'];
  userId: User['id'];
  userType: UserType;
}
