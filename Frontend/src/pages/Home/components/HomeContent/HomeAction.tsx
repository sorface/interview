import React, { FunctionComponent } from 'react';
import { pathnames } from '../../../../constants';
import { REACT_APP_BACKEND_URL } from '../../../../config';
import { User } from '../../../../types/user';
import { Link } from 'react-router-dom';
import { Localization } from '../../../../localization';

interface HomeActionProps {
  auth: User | null;
}

export const HomeAction: FunctionComponent<HomeActionProps> = ({
  auth,
}) => {
  if (auth) {
    return (
      <Link to={pathnames.rooms}>
        <button>
          {Localization.ToRooms}
        </button>
      </Link>
    );
  }
  return (
    <a href={`${REACT_APP_BACKEND_URL}/login/sorface?redirectUri=${encodeURIComponent(window.location.href)}`}>
      <button>{Localization.Login}</button>
    </a>
  );
};
