import React from 'react';
import { render, screen } from '@testing-library/react';
import { VITE_NAME } from '../../config';
import { Home } from './Home';

describe('Home', () => {
  test('renders app name', () => {
    render(<Home />);
    const apNameElement = screen.getByRole('heading', {
      name: VITE_NAME,
    });
    expect(apNameElement).toBeDefined();
  });
});
