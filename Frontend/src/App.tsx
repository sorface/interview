import React, { FunctionComponent, useCallback, useEffect } from 'react';
import { BrowserRouter } from 'react-router-dom';
import { Toaster } from 'react-hot-toast';
import { AppRoutes } from './routes/AppRoutes';
import { AuthContext } from './context/AuthContext';
import { useGetMeApi } from './hooks/useGetMeApi';
import { Loader } from './components/Loader/Loader';
import { MainContentWrapper } from './components/MainContentWrapper/MainContentWrapper';
import { Field } from './components/FieldsBlock/Field';
import { ThemeProvider } from './context/ThemeContext';
import { LocalizationProvider } from './context/LocalizationContext';
import { Button } from './components/Button/Button';
import { useLogout } from './hooks/useLogout';

import './App.css';

export const App: FunctionComponent = () => {
  const { getMeState, loadMe } = useGetMeApi();
  const { logout, logoutState: { process: { logoutCode } } } = useLogout();
  const { process: { loading, error }, user } = getMeState;
  const userWillLoad = !user && !error;

  useEffect(() => {
    loadMe();
  }, [loadMe]);

  const handlePageReset = useCallback(() => {
    logout();
  }, [logout]);

  useEffect(() => {
    if (!logoutCode || logoutCode !== 200) {
      return;
    }
    window.location.reload();
  }, [logoutCode]);

  const renderMainContent = useCallback(() => {
    if (loading || userWillLoad) {
      return (
        <MainContentWrapper>
          <Field>
            <div>Loading user data...</div>
            <Loader />
          </Field>
        </MainContentWrapper>
      )
    }
    if (error) {
      return (
        <MainContentWrapper>
          <Field>
            <div>Failed to get user data: {error}</div>
            <Button onClick={handlePageReset}>Reset page</Button>
          </Field>
        </MainContentWrapper>
      );
    }
    return (
      <AppRoutes user={user} />
    );
  }, [loading, userWillLoad, error, user, handlePageReset]);

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
