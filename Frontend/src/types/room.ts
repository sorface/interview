import { Question } from './question';
import { Tag } from './tag';
import { User, UserType } from './user';

export type RoomStatus = 'New' | 'Active' | 'Review' | 'Close';

export enum RoomAccessType {
  Public = 'Public',
  Private = 'Private',
}

export interface Room {
  id: string;
  name: string;
  participants: User[];
  tags: Tag[];
  roomStatus: RoomStatus;
  timer: {
    durationSec: number;
    startTime?: string;
  };
}

export type RoomStateType = 'CodeEditor';

export type RoomStateAdditionalStatefulPayload = {
  value: string | boolean;
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

export interface RoomInvite {
  inviteId: string;
  participantType: UserType;
  max: number;
  used: number;
}

export interface RoomParticipant {
  id: string;
  roomId: Room['id'];
  userId: User['id'];
  userType: UserType;
}

export type RoomQuestionState =
  'Open' |
  'Closed' |
  'Active';


export interface RoomQuestion {
  id: string;
  state: RoomQuestionState;
  value: string;
  order: number;
}
