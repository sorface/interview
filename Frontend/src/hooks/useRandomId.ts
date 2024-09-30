import { useState } from 'react';
import { randomId as generateRandomId } from '../utils/randomId';

export const useRandomId = () => {
    const [randomId] = useState(generateRandomId());
    return randomId;
};
