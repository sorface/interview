import React from 'react';
import { render, screen } from '@testing-library/react';
import { VITE_APP_NAME } from '../../config';
import { Home } from './Home';

describe('Home', () => {
  test('renders app name', () => {
    render(<Home />);
    const apNameElement = screen.getByRole('heading', {
      name: VITE_APP_NAME,
    });
    expect(apNameElement).toBeDefined();
  });
});
