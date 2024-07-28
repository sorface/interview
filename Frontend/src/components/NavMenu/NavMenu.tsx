import React, { FunctionComponent, ReactElement, ReactNode, useState } from 'react';
import { NavLink, useNavigate, matchPath, useLocation } from 'react-router-dom';
import { ThemeSwitchMini } from '../ThemeSwitchMini/ThemeSwitchMini';
import { IconNames, pathnames } from '../../constants';
import { ThemedIcon } from '../../pages/Room/components/ThemedIcon/ThemedIcon';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../localization';
import { LocalizationCaption } from '../LocalizationCaption/LocalizationCaption';
import { CategoriesList } from '../CategoriesList/CategoriesList';
import { Category } from '../../types/category';
import { Typography } from '../Typography/Typography';
import { REACT_APP_BUILD_HASH } from '../../config';

import './NavMenu.css';

interface MenuItem {
  path: string;
  logo?: string;
  caption: ReactNode;
  icon: IconNames;
  forceActive?: boolean;
  subitem?: ReactElement | null;
  onClick?: React.MouseEventHandler<HTMLAnchorElement>;
}

export interface NavMenuProps {
  admin: boolean;
}

export const NavMenu: FunctionComponent<NavMenuProps> = ({ admin }) => {
  const navigate = useNavigate();
  const location = useLocation();
  const localizationCaptions = useLocalizationCaptions();
  const [collapsed, setCollapsed] = useState(true);
  const [questionsClicked, setQuestionsClicked] = useState(false);
  const [selectedCategory, setSelectedCategory] = useState<Category | null>(null);
  const questionsPath = matchPath(
    { path: pathnames.questions, end: false, },
    location.pathname,
  );

  const handleQuestionsClick: React.MouseEventHandler<HTMLAnchorElement> = (event) => {
    event.preventDefault();
    setQuestionsClicked(!questionsClicked);
  };

  const handleItemClick = () => {
    setSelectedCategory(null);
    setQuestionsClicked(false);
  };

  const handleMouseEnter = () => {
    setCollapsed(false);
  };

  const handleMouseLeave = () => {
    setCollapsed(true);
    setSelectedCategory(null);
    setQuestionsClicked(false);
  };

  const handleCategoryClick = (category: Category) => {
    if (category === selectedCategory) {
      setSelectedCategory(null);
      return;
    }
    setSelectedCategory(category);
  };

  const handleSubCategoryClick = (category: Category) => {
    navigate(pathnames.questions.replace(':category', category.id));
    handleMouseLeave();
  };

  const items: Array<MenuItem | null> = [
    {
      path: pathnames.currentRooms,
      caption: <LocalizationCaption captionKey={LocalizationKey.CurrentRoomsPageName} />,
      icon: IconNames.Cube,
      onClick: handleItemClick,
    },
    {
      path: pathnames.closedRooms,
      caption: <LocalizationCaption captionKey={LocalizationKey.ClosedRoomsPageName} />,
      icon: IconNames.Golf,
      onClick: handleItemClick,
    },
    {
      path: pathnames.questions,
      caption: <LocalizationCaption captionKey={LocalizationKey.QuestionsPageName} />,
      icon: IconNames.Chat,
      forceActive: questionsClicked || !!questionsPath,
      onClick: handleQuestionsClick,
      subitem: questionsClicked ? <CategoriesList showOnlyWithoutParent={true} onCategoryClick={handleCategoryClick} /> : null,
    },
    admin ? {
      path: pathnames.categories,
      caption: <LocalizationCaption captionKey={LocalizationKey.CategoriesPageName} />,
      icon: IconNames.Clipboard,
      onClick: handleItemClick,
    } : null,
  ];

  const createMenuItem = (item: MenuItem) => {
    const noActiveClassName = !item.forceActive && (item.logo || questionsClicked);
    return (
      <>
        <NavLink
          key={item.path}
          to={item.path}
          className={`nav-menu-item ${noActiveClassName ? 'no-active' : ''} ${item.forceActive ? 'active' : ''} move-transition`}
          onClick={item.onClick}
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
        {!!item.subitem && item.subitem}
      </>
    );
  };

  return (
    <>
      {!!selectedCategory && (
        <div className='nav-menu-page-overlay'></div>
      )}
      <div
        className='nav-menu-container relative'
        onMouseEnter={handleMouseEnter}
        onMouseLeave={handleMouseLeave}
      >
        <nav
          className={`nav-menu ${collapsed ? 'collapsed' : ''} move-transition`}
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
          {items.map(item => item ? createMenuItem(item) : undefined)}
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
          <div className='nav-menu-build h-1.125 opacity-0.5'>
            <Typography size='s' >
              {localizationCaptions[LocalizationKey.BuildHash]}: {REACT_APP_BUILD_HASH}
            </Typography>
          </div>
        </nav>
        {!!selectedCategory && (
          <>
            <div className='nav-menu-overlay-gap'></div>
            <div className='nav-menu-overlay'>
              <h4>{selectedCategory.name}</h4>
              <CategoriesList
                parentId={selectedCategory.id}
                onCategoryClick={handleSubCategoryClick}
              />
            </div>
          </>
        )}
      </div>
    </>
  );
};
