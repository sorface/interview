import React, {
  Fragment,
  FunctionComponent,
  ReactNode,
  useContext,
} from 'react';
import { matchPath, NavLink } from 'react-router-dom';
import { IconNames, pathnames } from '../../constants';
import { VITE_APP_NAME } from '../../config';
import { LocalizationCaption } from '../LocalizationCaption/LocalizationCaption';
import { LocalizationKey } from '../../localization';
import { ThemeSwitchMini } from '../ThemeSwitchMini/ThemeSwitchMini';
import { Typography } from '../Typography/Typography';
import { Gap } from '../Gap/Gap';
import { PageHeaderUserAvatar } from '../PageHeaderUserAvatar/PageHeaderUserAvatar';
import { useThemeClassName } from '../../hooks/useThemeClassName';
import { Theme } from '../../context/ThemeContext';
import { DeviceContext } from '../../context/DeviceContext';
import { LangSwitch } from '../LangSwitch/LangSwitch';
import { Icon } from '../../pages/Room/components/Icon/Icon';

interface NavMenu2Props {
  admin: boolean;
}

interface MenuItem {
  path: string;
  target?: string;
  logo?: string;
  icon?: IconNames;
  caption: ReactNode;
}

export const NavMenu: FunctionComponent<NavMenu2Props> = ({ admin }) => {
  const device = useContext(DeviceContext);
  const navmenuThemeClassName = useThemeClassName({
    [Theme.Dark]: 'bg-dark-dark2 border-dark-dark1',
    [Theme.Light]: 'bg-white border-nav-menu-border-light',
  });

  const items: Array<MenuItem | null> = [
    {
      path: pathnames.roadmaps,
      logo: '/logo192.png',
      caption: VITE_APP_NAME,
    },
    {
      path: pathnames.roadmaps,
      caption: (
        <LocalizationCaption captionKey={LocalizationKey.RoadmapsPageName} />
      ),
    },
    admin
      ? {
          path: pathnames.roadmapsArchive,
          caption: (
            <LocalizationCaption
              captionKey={LocalizationKey.RoadmapsArchivePageName}
            />
          ),
        }
      : null,
    {
      path: pathnames.questionsRootCategories,
      caption: (
        <LocalizationCaption captionKey={LocalizationKey.QuestionsPageName} />
      ),
    },
    {
      path: pathnames.highlightRooms,
      caption: (
        <LocalizationCaption
          captionKey={LocalizationKey.HighlightsRoomsPageName}
        />
      ),
    },
    {
      path: 'https://t.me/sorface_event',
      target: '_blank',
      icon: IconNames.PaperPlane,
      caption: <LocalizationCaption captionKey={LocalizationKey.News} />,
    },
    admin
      ? {
          path: pathnames.categories,
          caption: (
            <LocalizationCaption
              captionKey={LocalizationKey.CategoriesPageName}
            />
          ),
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
          target={item.target}
          className={`flex items-center no-underline ${firstItem ? 'pr-[2rem]' : 'pr-[2.25rem]'} ${active ? 'underline' : ''}`}
        >
          {item.icon && <Icon name={item.icon} />}
          {item.logo && (
            <img
              className="w-[1.25rem] h-[1.25rem] rounded-[0.25rem]"
              src={item.logo}
              alt="site logo"
            />
          )}
          <Gap sizeRem={0.5} horizontal />
          <Typography size={firstItem ? 'xl' : 'm'} bold={firstItem || active}>
            {item.caption}
          </Typography>
        </NavLink>
      </Fragment>
    );
  };

  return (
    <nav
      className={`flex w-full text-nowrap border-b-[1px] ${device === 'Desktop' ? 'px-[2.5rem]' : 'px-[1rem]'} py-[0.625rem] ${navmenuThemeClassName}`}
    >
      <div className="flex items-center overflow-y-auto py-[0.5rem]">
        {items.map((item, index) =>
          item ? createMenuItem(item, index) : undefined,
        )}
      </div>
      <div className="ml-auto flex items-center">
        <LangSwitch elementType="button" />
        <ThemeSwitchMini variant="button" />
        <Gap sizeRem={device === 'Desktop' ? 1 : 0.15} horizontal />
        <PageHeaderUserAvatar size="m" />
      </div>
    </nav>
  );
};
