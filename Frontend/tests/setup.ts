import { vi } from 'vitest';

Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: vi.fn().mockImplementation((query) => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: vi.fn(), // deprecated
    removeListener: vi.fn(), // deprecated
    addEventListener: vi.fn(),
    removeEventListener: vi.fn(),
    dispatchEvent: vi.fn(),
  })),
});

vi.mock('react-router-dom', () => ({
  useParams: () => ({}),
  BrowserRouter: vi.fn().mockImplementation((props) => props.children),
  Link: vi.fn().mockImplementation((props) => props.children),
}));
