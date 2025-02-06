import { User } from '../types/user';

const aiAccessNicknames = ['masonyan777', 'developerdevpav', 'dsisdead'];

export const checkAiAccess = (user: User | null) =>
  !!user && aiAccessNicknames.includes(user.nickname);
