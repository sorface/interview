import React, { FunctionComponent, ReactNode, useState } from 'react';
import { NavLink } from 'react-router-dom';
import { ThemeSwitchMini } from '../ThemeSwitchMini/ThemeSwitchMini';
import { IconNames, pathnames } from '../../constants';
import { ThemedIcon } from '../../pages/Room/components/ThemedIcon/ThemedIcon';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../localization';
import { LocalizationCaption } from '../LocalizationCaption/LocalizationCaption';

import './NavMenu.css';

interface MenuItem {
  path: string;
  logo?: string;
  caption: ReactNode;
  icon: IconNames;
}

export interface NavMenuProps {
  items: MenuItem[];
}

export const NavMenu: FunctionComponent<NavMenuProps> = ({ items }) => {
  const localizationCaptions = useLocalizationCaptions();
  const [collapsed, setCollapsed] = useState(true);

  const handleMouseEnter = () => {
    setCollapsed(false);
  };

  const handleMouseLeave = () => {
    setCollapsed(true);
  };

  const createMenuItem = (item: MenuItem) => (
    <NavLink
      key={item.path}
      to={item.path}
      className={`nav-menu-item ${item.logo ? 'no-active' : ''} move-transition`}
    >
      {item.logo ? (
        <img className='site-logo' src={item.logo} alt='site logo' />
      ) : (
        <ThemedIcon name={item.icon} />
      )}
      <div
        className='nav-menu-item-caption move-transition'
        style={{ width: collapsed ? '0rem' : 'var(--caption-width)' }}
      >
        {item.caption}
      </div>
    </NavLink>
  );

  return (
    <div className='nav-menu-container'>
      <nav
        className={`nav-menu ${collapsed ? 'collapsed' : ''} move-transition`}
        onMouseEnter={handleMouseEnter}
        onMouseLeave={handleMouseLeave}
      >
        <NavLink
          to={pathnames.home.replace(':redirect?', '')}
          className='nav-menu-item no-active move-transition'
        >
          <img className='site-logo' src='/logo192.png' alt='site logo' />
          <h1
            className='nav-menu-item-caption move-transition'
            style={{ width: collapsed ? '0rem' : 'var(--caption-width)' }}
          >
            <LocalizationCaption captionKey={LocalizationKey.AppName} />
          </h1>
        </NavLink>
        {items.map(createMenuItem)}
        <hr />
        <div className='nav-menu-item move-transition nav-menu-theme-switch'>
          <ThemeSwitchMini />
          <div
            className='nav-menu-item-caption move-transition'
            style={{ width: collapsed ? '0rem' : 'var(--caption-width)' }}
          >
            {localizationCaptions[LocalizationKey.ThemeDark]}
            <ThemedIcon name={IconNames.ThemeSwitchDark} />
          </div>
        </div>
      </nav>
    </div>
  );
};
