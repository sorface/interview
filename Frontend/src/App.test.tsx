import React from 'react';
import { render, screen } from '@testing-library/react';
import { App } from './App';
import { Localization } from './localization';

describe('App', () => {
  test('renders app name', () => {
    render(<App />);
    const apNameElement = screen.getByRole('link', { name: Localization.AppName });
    expect(apNameElement).toBeInTheDocument();
  });
});
