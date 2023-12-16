import { UserType, Role } from './user';

export interface Event {
  id: string;
  type: string;
  participantTypes: UserType[];
  roles: Role[];
}

export type EventsSearch = Record<string, Array<{
  trype: string;
  payload: string;
}>>;
