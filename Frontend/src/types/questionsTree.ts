import { TreeNodeType } from './tree';

export interface QuestionsTree {
  id: string;
  name: string;
  themeAiDescription?: string;
  rootQuestionSubjectTreeId: string;
  tree: QuestionsTreeNode[];
}

export interface QuestionsTreeNode {
  id: string;
  order: number;
  parentQuestionSubjectTreeId: string;
  type: TreeNodeType;
  question?: {
    id: string;
    value: string;
  };
}
