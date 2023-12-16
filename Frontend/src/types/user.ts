export type Role = 'User' | 'Admin';

export interface User {
  id: string;
  nickname: string;
  twitchIdentity: string;
  avatar?: string;
  roles: Role[];
}

export type UserType = 'Viewer' | 'Expert' | 'Examinee';
