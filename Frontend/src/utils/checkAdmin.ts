import { User } from '../types/user';

export const checkAdmin = (user: User | null) =>
  !!user && user.roles.includes('Admin');
