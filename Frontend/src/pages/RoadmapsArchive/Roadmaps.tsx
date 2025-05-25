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
import { ActionModal } from '../../components/ActionModal/ActionModal';

const pageSize = 30;
const initialPageNumber = 1;

export const RoadmapsArchive: FunctionComponent = () => {
  const auth = useContext(AuthContext);
  const admin = checkAdmin(auth);
  const localizationCaptions = useLocalizationCaptions();
  const roadmapItemThemedClassName = useThemeClassName({
    [Theme.Dark]: 'hover:bg-dark-history-hover',
    [Theme.Light]:
      'hover:bg-blue-light hover:border hover:border-solid border-blue-main',
  });

  const [pageNumber, setPageNumber] = useState(initialPageNumber);
  const { apiMethodState, fetchData } = useApiMethod<
    Roadmap[],
    GetRoadmapsParams
  >(roadmapTreeApiDeclaration.getPageArchived);

  const { apiMethodState: unarchiveRoadmapState, fetchData: unarchiveRoadmap } =
    useApiMethod<unknown, string>(roadmapTreeApiDeclaration.unarchive);
  const {
    process: { loading: unarchiveLoading, error: unarchiveError },
    data: unarchivedRoadmap,
  } = unarchiveRoadmapState;

  const {
    process: { loading, error },
    data: roadmaps,
  } = apiMethodState;

  useEffect(() => {
    fetchData({
      PageSize: pageSize,
      PageNumber: pageNumber,
    });
  }, [pageNumber, unarchivedRoadmap, fetchData]);

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
                <ActionModal
                  openButtonCaption="📁"
                  error={unarchiveError}
                  loading={unarchiveLoading}
                  title={localizationCaptions[LocalizationKey.Unarchive]}
                  loadingCaption={
                    localizationCaptions[LocalizationKey.ArchiveLoading]
                  }
                  onAction={() => {
                    unarchiveRoadmap(roadmap.id);
                  }}
                />
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
      <ItemsGrid
        currentData={roadmaps}
        loading={loading}
        error={error}
        triggerResetAccumData={`${unarchivedRoadmap}`}
        renderItem={createRoadmapItem}
        nextPageAvailable={false}
        handleNextPage={handleNextPage}
      />
    </>
  );
};
