export interface Roadmap {
  id: string;
  name: string;
  order: number;
  imageBase64?: string;
  description?: string;
  items: RoadmapItem[];
}

export interface RoadmapItem {
  id: string;
  type: RoadmapItemType;
  name?: string;
  questionTreeId?: string;
  roomId?: string;
  order: number;
}

export enum RoadmapItemType {
  Milestone = 'Milestone',
  QuestionTree = 'QuestionTree',
}
