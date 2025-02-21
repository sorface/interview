export const enum TreeNodeType {
  Empty = 'Empty',
  Question = 'Question',
}

export interface TreeNode {
  id: string;
  question: {
    id: string | null;
    value: string;
  } | null;
  type: TreeNodeType;
  order: number;
  parentQuestionSubjectTreeId: string | null;
}

export interface TreeMeta {
  id: string;
  name: string;
}
