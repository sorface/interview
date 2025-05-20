export interface Category {
  id: string;
  name: string;
  parent?: {
    id: string;
    name: string;
  };
  order: number;
}
