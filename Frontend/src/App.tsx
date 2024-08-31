import React, { FunctionComponent, useEffect } from 'react';
import { BrowserRouter } from 'react-router-dom';
import { Toaster } from 'react-hot-toast';
import { AppRoutes } from './routes/AppRoutes';
import { AuthContext } from './context/AuthContext';
import { useGetMeApi } from './hooks/useGetMeApi';
import { Loader } from './components/Loader/Loader';
import { ThemeProvider } from './context/ThemeContext';
import { LocalizationProvider } from './context/LocalizationContext';
import { useLogout } from './hooks/useLogout';
import { unauthorizedHttpCode } from './constants';
import { LoadingAccountError } from './components/LoadingAccountError/LoadingAccountError';

import './App.css';

export const App: FunctionComponent = () => {
  const { getMeState, loadMe } = useGetMeApi();
  const { logout } = useLogout();
  const { process: { loading, error, code }, user } = getMeState;
  const userWillLoad = !user && !error;

  useEffect(() => {
    loadMe();
  }, [loadMe]);

  const handlePageReset = () => {
    logout();
  };

  const renderMainContent = () => {
    if (loading || userWillLoad) {
      return (
        <div className='h-dvh flex items-center justify-center'>
          <Loader />
        </div>
      )
    }
    if (error && code !== unauthorizedHttpCode) {
      return (
        <LoadingAccountError onAccountReset={handlePageReset} />
      );
    }
    return (
      <AppRoutes user={user} />
    );
  };

  return (
    <BrowserRouter>
      <Toaster
        position="top-center"
        toastOptions={{
          duration: 3000,
          style: {
            background: 'var(--bg)',
            color: 'var(--text)',
          },
          success: {
            style: {
              background: 'var(--bg-success)',
              color: 'var(--text)',
            },
          },
          error: {
            style: {
              background: 'var(--red)',
              color: 'var(--text)',
            },
          },
        }}
      />
      <ThemeProvider>
        <LocalizationProvider>
          <AuthContext.Provider value={user}>
            <div className="App-container">
              {renderMainContent()}
            </div>
          </AuthContext.Provider>
        </LocalizationProvider>
      </ThemeProvider>
    </BrowserRouter>
  );
};
