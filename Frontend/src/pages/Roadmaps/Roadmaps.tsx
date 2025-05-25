import React, {
  FunctionComponent,
  useCallback,
  useContext,
  useEffect,
  useState,
} from 'react';
import { useApiMethod } from '../../hooks/useApiMethod';
import { Roadmap } from '../../types/roadmap';
import {
  GetRoadmapsParams,
  roadmapTreeApiDeclaration,
} from '../../apiDeclarations';
import { PageHeader } from '../../components/PageHeader/PageHeader';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../localization';
import { Typography } from '../../components/Typography/Typography';
import { Button } from '../../components/Button/Button';
import { Icon } from '../Room/components/Icon/Icon';
import { IconNames, pathnames } from '../../constants';
import { AuthContext } from '../../context/AuthContext';
import { checkAdmin } from '../../utils/checkAdmin';
import { generatePath, Link } from 'react-router-dom';
import { ItemsGrid } from '../../components/ItemsGrid/ItemsGrid';
import { useThemeClassName } from '../../hooks/useThemeClassName';
import { Theme } from '../../context/ThemeContext';
import { Checkbox } from '../../components/Checkbox/Checkbox';

const pageSize = 30;
const initialPageNumber = 1;

export const Roadmaps: FunctionComponent = () => {
  const auth = useContext(AuthContext);
  const admin = checkAdmin(auth);
  const localizationCaptions = useLocalizationCaptions();
  const roadmapItemThemedClassName = useThemeClassName({
    [Theme.Dark]: 'hover:bg-dark-history-hover',
    [Theme.Light]:
      'hover:bg-blue-light hover:border hover:border-solid border-blue-main',
  });

  const [pageNumber, setPageNumber] = useState(initialPageNumber);
  const [archived, setArchived] = useState(false);
  const { apiMethodState, fetchData } = useApiMethod<
    Roadmap[],
    GetRoadmapsParams
  >(roadmapTreeApiDeclaration.getPage);

  const {
    process: { loading, error },
    data: roadmaps,
  } = apiMethodState;

  useEffect(() => {
    fetchData({
      PageSize: pageSize,
      PageNumber: pageNumber,
      Archived: archived,
    });
  }, [pageNumber, archived, fetchData]);

  const handleNextPage = useCallback(() => {
    setPageNumber(pageNumber + 1);
  }, [pageNumber]);

  const createRoadmapItem = (roadmap: Roadmap) => {
    const roadmapLink = generatePath(pathnames.roadmap, { id: roadmap.id });
    const roadmapEditLink = generatePath(pathnames.roadmapEdit, {
      id: roadmap.id,
    });

    return (
      <div key={roadmap.id} className={roadmapItemThemedClassName}>
        <li>
          <div className="flex">
            <Link to={roadmapLink}>{roadmap.name}</Link>
            {admin && (
              <div className="ml-auto">
                <Link to={roadmapEditLink}>
                  <Icon size="m" name={IconNames.Settings} />
                </Link>
              </div>
            )}
          </div>
        </li>
      </div>
    );
  };

  return (
    <>
      <PageHeader
        title={localizationCaptions[LocalizationKey.RoadmapsPageName]}
      >
        {admin && (
          <Link to={pathnames.roadmapCreate}>
            <Button variant="active" className="h-[2.5rem]" aria-hidden>
              <Icon name={IconNames.Add} />
              {localizationCaptions[LocalizationKey.Create]}
            </Button>
          </Link>
        )}
      </PageHeader>
      {admin && (
        <Checkbox
          id='archiveCheckbox'
          checked={archived}
          label='archived'
          onChange={() => setArchived(!archived)}
        />
      )}
      <ItemsGrid
        currentData={roadmaps}
        loading={loading}
        error={error}
        triggerResetAccumData=""
        renderItem={createRoadmapItem}
        nextPageAvailable={false}
        handleNextPage={handleNextPage}
      />
    </>
  );
};
