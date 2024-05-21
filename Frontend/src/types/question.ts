import { Tag } from './tag';

export enum QuestionType {
  Public = 'Public',
  Private = 'Private',
}

export interface Question {
  id: string;
  value: string;
  tags: Tag[];
}
