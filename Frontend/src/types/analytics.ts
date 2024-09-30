import { Question } from './question';
import { RoomQuestion, RoomQuestionEvaluation } from './room';
import { User, UserType } from './user';

export interface AnalyticsQuestionsUserReactionSummary {
  id: string;
  type: string;
  count: number;
}

export interface AnalyticsQuestionsUserReaction {
  id: string;
  type: string;
}

export interface AnalyticsQuestionsExpert extends User {
  nickname: string;
  reactionsSummary: AnalyticsQuestionsUserReactionSummary[];
}

interface AnalyticsQuestionsViwer {
  reactionsSummary: AnalyticsQuestionsUserReactionSummary[];
}

export interface AnalyticsSummaryQuestions extends Question {
  status: string;
  viewers: AnalyticsQuestionsViwer[] | null;
  experts: AnalyticsQuestionsExpert[] | null;
}

export interface AnalyticsSummary {
  questions: AnalyticsSummaryQuestions[];
}

export interface AnalyticsQuestions extends RoomQuestion {
  averageMark: number;
  users: Array<{
    id: User['id'];
    evaluation?: Omit<RoomQuestionEvaluation, 'id'>;
  }>;
}

export interface AnalyticsUserReview {
  userId: User['id'];
  nickname: User['nickname'];
  avatar?: User['avatar'];
  participantType: UserType;
  averageMark?: number;
  comment: string;
}

export interface Analytics {
  completed: boolean;
  averageMark?: number;
  questions: AnalyticsQuestions[];
  userReview: AnalyticsUserReview[];
}
