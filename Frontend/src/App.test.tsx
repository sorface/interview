import React from 'react';
import { render, screen } from '@testing-library/react';
import { App } from './App';
import { VITE_NAME } from './config';

describe('App', () => {
  test('renders app name', () => {
    render(<App />);
    const apNameElement = screen.getByRole('link', {
      name: VITE_NAME,
    });
    expect(apNameElement).toBeInTheDocument();
  });
});
