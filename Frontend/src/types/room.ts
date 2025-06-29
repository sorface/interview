import { DragNDropListItem } from '../components/DragNDropList/DragNDropList';
import { CodeEditorLang, Question, QuestionAnswer } from './question';
import { QuestionsTree } from './questionsTree';
import { Tag } from './tag';
import { User, UserType } from './user';

export type RoomStatus = 'New' | 'Active' | 'Review' | 'Close' | 'Expire';

export enum RoomAccessType {
  Public = 'Public',
  Private = 'Private',
}

export interface Room {
  id: string;
  name: string;
  participants: RoomParticipant[];
  owner: {
    id: User['id'];
    nickname: User['nickname'];
  };
  tags: Tag[];
  status: RoomStatus;
  questions: RoomQuestionListItem[];
  scheduledStartTime: string;
  questionTree?: QuestionsTree;
  timer?: {
    durationSec: number;
    startTime?: string;
  };
}

export type RoomWtithType = Room & {
  type: 'AI' | 'Standard';
};

export interface RoomCalendarItem {
  minScheduledStartTime: string;
  statuses: RoomStatus[];
}

export type RoomQuestionListItem = Question & DragNDropListItem;

export interface RoomParticipant extends User {
  type: UserType;
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
  activeQuestion: Question;
  codeEditor: {
    enabled: boolean;
    content: string;
  };
  states: RoomStateAdditional[];
}

export interface RoomReview {
  id: string;
  user: User;
  roomId: Room['id'];
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

export type RoomQuestionState = 'Open' | 'Closed' | 'Active';

export interface RoomQuestion {
  id: string;
  state: RoomQuestionState;
  value: string;
  order: number;
  answers?: QuestionAnswer[];
}

export interface RoomQuestionEvaluation {
  id: string;
  mark?: number | null;
  review?: string;
}

export interface MyRoomQuestionEvaluation {
  id: string;
  value: string;
  order: number;
  evaluation?: RoomQuestionEvaluation;
}

export interface RoomQuestionAnswer {
  codeEditor?: {
    content: string;
    lang: CodeEditorLang;
  };
  details: Array<{
    answerCodeEditorContent: string;
    endActiveDate: string;
    startActiveDate: string;
    transcription: Array<{
      createdAt: string;
      id: string;
      payload: string;
      user: {
        id: string;
        nickname: string;
      };
    }>;
  }>;
}

export interface RoomBusinessAnalytic {
  ai: RoomBusinessAnalyticTypeItem[];
  standard: RoomBusinessAnalyticTypeItem[];
}

export interface RoomBusinessAnalyticTypeItem {
  date: string;
  status: Array<{
    name: RoomStatus;
    count: number;
  }>;
}
