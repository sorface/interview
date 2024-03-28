import React from 'react';
import { render, screen } from '@testing-library/react';
import { App } from './App';
import { LocalizationKey } from './localization';
import { useLocalizationCaptions } from './hooks/useLocalizationCaptions';

describe('App', () => {
  test('renders app name', () => {
    render(<App />);
    const apNameElement = screen.getByRole('link', { name: useLocalizationCaptions()[LocalizationKey.AppName] });
    expect(apNameElement).toBeInTheDocument();
  });
});
