import { Tag } from './tag';

export interface Question {
  id: string;
  value: string;
  tags: Tag[];
}

export type QuestionState =
  'Open' |
  'Closed' |
  'Active';
