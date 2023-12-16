import React from 'react';
import { User } from '../types/user';

export const AuthContext = React.createContext<User | null>(null);
