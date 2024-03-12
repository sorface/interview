import { Tag } from './tag';

export interface Question {
  id: string;
  value: string;
  tags: Tag[];
}
