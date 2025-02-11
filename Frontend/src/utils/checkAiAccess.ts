import { VITE_AI_ACCESS } from '../config';
import { User } from '../types/user';

const aiAccessNicknames = VITE_AI_ACCESS.split(',');

export const checkAiAccess = (user: User | null) =>
  !!user && aiAccessNicknames.includes(user.nickname);
