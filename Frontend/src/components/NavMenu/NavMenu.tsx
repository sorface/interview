import React, { FunctionComponent, ReactNode, useContext } from 'react';
import { NavLink } from 'react-router-dom';
import { AuthContext } from '../../context/AuthContext';
import { FieldsBlock } from '../FieldsBlock/FieldsBlock';
import { pathnames } from '../../constants';
import { checkAdmin } from '../../utils/checkAdmin';
import { UserAvatar } from '../UserAvatar/UserAvatar';
import { ThemeSwitchMini } from '../ThemeSwitchMini/ThemeSwitchMini';
import { Localization } from '../../localization';

import './NavMenu.css';

interface MenuItem {
  path: string;
  content: ReactNode;
}

const createMenuItem = (item: MenuItem) => (
  <NavLink
    key={item.path}
    to={item.path}
    className="field-wrap"
  >
    {item.content}
  </NavLink>
);

export const NavMenu: FunctionComponent = () => {
  const auth = useContext(AuthContext);
  const admin = checkAdmin(auth);

  const userContent = auth ?
    (
      <div className='nav-menu-user-content'>
        {auth.avatar && (
          <UserAvatar
            src={auth.avatar}
            nickname={auth.nickname}
          />
        )}
        {auth.nickname}
      </div>
    ) :
    (Localization.UnauthorizedMessage);

  const items: MenuItem[] = admin ? [
    { path: pathnames.home.replace(':redirect?', ''), content: Localization.AppName },
    { path: pathnames.rooms, content: Localization.RoomsPageName },
    { path: pathnames.questions, content: Localization.QuestionsPageName },
    { path: pathnames.session, content: userContent },
  ] : [
    { path: pathnames.home.replace(':redirect?', ''), content: Localization.AppName },
    { path: pathnames.rooms, content: Localization.RoomsPageName },
    { path: pathnames.session, content: userContent },
  ];

  return (
    <FieldsBlock className="nav-menu">
      <nav>
        {items.map(item => createMenuItem(item))}
      </nav>
      <ThemeSwitchMini />
    </FieldsBlock>
  );
};
