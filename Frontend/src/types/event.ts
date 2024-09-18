import { UserType, Role } from './user';

export interface Event {
  id: string;
  type: string;
  participantTypes: UserType[];
  roles: Role[];
}

export type EventsSearch = Record<string, Array<{
  id: string;
  createdById: string;
  trype: string;
  payload: string;
}>>;
