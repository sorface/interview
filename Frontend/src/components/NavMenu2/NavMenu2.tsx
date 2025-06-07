import React, { Fragment, FunctionComponent, ReactNode } from 'react';
import { matchPath, NavLink } from 'react-router-dom';
import { IconNames, pathnames } from '../../constants';
import { VITE_APP_NAME } from '../../config';
import { LocalizationCaption } from '../LocalizationCaption/LocalizationCaption';
import { LocalizationKey } from '../../localization';
import { Icon } from '../../pages/Room/components/Icon/Icon';
import { ThemeSwitchMini } from '../ThemeSwitchMini/ThemeSwitchMini';
import { Typography } from '../Typography/Typography';
import { Gap } from '../Gap/Gap';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { PageHeaderUserAvatar } from '../PageHeaderUserAvatar/PageHeaderUserAvatar';
import { useThemeClassName } from '../../hooks/useThemeClassName';
import { Theme } from '../../context/ThemeContext';

interface NavMenu2Props {
  admin: boolean;
}

interface MenuItem {
  path: string;
  logo?: string;
  caption: ReactNode;
  icon: IconNames;
}

export const NavMenu2: FunctionComponent<NavMenu2Props> = ({ admin }) => {
  const localizationCaptions = useLocalizationCaptions();
  const navmenuThemeClassName = useThemeClassName({
    [Theme.Dark]: 'border-dark-dark2',
    [Theme.Light]: 'border-nav-menu-border-light',
  });

  const items: Array<MenuItem | null> = [
    {
      path: pathnames.roadmaps,
      logo: '/logo192.png',
      caption: VITE_APP_NAME,
      icon: IconNames.Golf,
    },
    {
      path: pathnames.roadmaps,
      caption: (
        <LocalizationCaption captionKey={LocalizationKey.RoadmapsPageName} />
      ),
      icon: IconNames.Golf,
    },
    admin
      ? {
          path: pathnames.roadmapsArchive,
          caption: (
            <LocalizationCaption
              captionKey={LocalizationKey.RoadmapsPageName}
            />
          ),
          icon: IconNames.Golf,
        }
      : null,
    {
      path: pathnames.questions,
      caption: (
        <LocalizationCaption captionKey={LocalizationKey.QuestionsPageName} />
      ),
      icon: IconNames.Chat,
    },
    {
      path: pathnames.highlightRooms,
      caption: (
        <LocalizationCaption
          captionKey={LocalizationKey.HighlightsRoomsPageName}
        />
      ),
      icon: IconNames.Cube,
    },
    admin
      ? {
          path: pathnames.categories,
          caption: (
            <LocalizationCaption
              captionKey={LocalizationKey.CategoriesPageName}
            />
          ),
          icon: IconNames.Clipboard,
        }
      : null,
    admin
      ? {
          path: pathnames.categoriesArchive,
          caption: (
            <LocalizationCaption
              captionKey={LocalizationKey.CategoriesArchive}
            />
          ),
          icon: IconNames.Clipboard,
        }
      : null,
    admin
      ? {
          path: pathnames.questionTrees,
          caption: (
            <LocalizationCaption
              captionKey={LocalizationKey.QuestionTreesPageName}
            />
          ),
          icon: IconNames.Expand,
        }
      : null,
    admin
      ? {
          path: pathnames.businessAnalytic,
          caption: (
            <LocalizationCaption
              captionKey={LocalizationKey.BusinessAnalyticPageName}
            />
          ),
          icon: IconNames.Cube,
        }
      : null,
  ];

  const createMenuItem = (item: MenuItem, index: number) => {
    const firstItem = index === 0;
    const active = !!matchPath(window.location.pathname, item.path);
    return (
      <Fragment key={`${item.path}${item.logo}`}>
        <NavLink
          key={item.path}
          to={item.path}
          className={`flex items-center no-underline ${firstItem ? 'pr-[2rem]' : 'pr-[2.25rem]'}`}
        >
          {item.logo ? (
            <img
              className="w-[1.25rem] h-[1.25rem] rounded-[0.25rem]"
              src={item.logo}
              alt="site logo"
            />
          ) : (
            <Icon name={item.icon} />
          )}
          <Gap sizeRem={0.5} horizontal />
          <Typography size="m" bold={firstItem || active}>
            {item.caption}
          </Typography>
        </NavLink>
      </Fragment>
    );
  };

  return (
    <nav
      className={`flex w-full text-nowrap border-b-[1px] px-[2.5rem] py-[1.25rem] ${navmenuThemeClassName}`}
    >
      <div className="flex items-center overflow-y-auto">
        {items.map((item, index) =>
          item ? createMenuItem(item, index) : undefined,
        )}
      </div>
      <div className="ml-auto flex items-center">
        <Typography size="m">
          {localizationCaptions[LocalizationKey.ThemeDark]}
        </Typography>
        <Gap sizeRem={0.25} horizontal />
        <ThemeSwitchMini />
        <Gap sizeRem={0.75} horizontal />
        <PageHeaderUserAvatar />
      </div>
    </nav>
  );
};
