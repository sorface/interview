import React, { FunctionComponent, ReactElement } from 'react';
import { RouteProps, Navigate, useLocation, } from 'react-router-dom';
import { pathnames } from '../constants';

type PrivateRouteProps = RouteProps & {
  allowed: boolean;
  children: ReactElement<any, any> | null;
};

export const ProtectedRoute: FunctionComponent<PrivateRouteProps> = ({
  allowed,
  children,
}) => {
  const location = useLocation();
  if (!allowed) {
    return <Navigate to={pathnames.home.replace(':redirect?', encodeURIComponent(location.pathname))} replace />;
  }

  return children;
};
