import { Question } from './question';
import { User } from './user';

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

export interface AnalyticsQuestions extends Question {
  status: string;
  viewers: AnalyticsQuestionsViwer[] | null;
  experts: AnalyticsQuestionsExpert[] | null;
}

export interface AnalyticsSummary {
  questions: AnalyticsQuestions[];
}
