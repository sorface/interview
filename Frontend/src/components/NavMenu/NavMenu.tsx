import React, { FunctionComponent, ReactNode } from 'react';
import { NavLink } from 'react-router-dom';
import { FieldsBlock } from '../FieldsBlock/FieldsBlock';
import { ThemeSwitchMini } from '../ThemeSwitchMini/ThemeSwitchMini';

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

export interface NavMenuProps {
  item: MenuItem;
}

export const NavMenu: FunctionComponent<NavMenuProps> = ({ item }) => {
  return (
    <FieldsBlock className="nav-menu">
      <nav>
        {createMenuItem(item)}
      </nav>
      <ThemeSwitchMini />
    </FieldsBlock>
  );
};
