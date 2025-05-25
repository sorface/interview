export interface Roadmap {
  id: string;
  name: string;
  order: number;
  items: RoadmapItem[];
}

export interface RoadmapItem {
  id: string;
  type: RoadmapItemType;
  name?: string;
  questionTreeId?: string;
  order: number;
}

export enum RoadmapItemType {
  Milestone = 'Milestone',
  QuestionTree = 'QuestionTree',
  VerticalSplit = 'VerticalSplit',
}
