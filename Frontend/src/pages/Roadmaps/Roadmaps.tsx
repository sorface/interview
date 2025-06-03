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
  PaginationUrlParams,
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
import { Gap } from '../../components/Gap/Gap';

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
  const { apiMethodState, fetchData } = useApiMethod<
    Roadmap[],
    PaginationUrlParams
  >(roadmapTreeApiDeclaration.getPage);
  const {
    process: { loading, error },
    data: roadmaps,
  } = apiMethodState;

  const { apiMethodState: archiveRoadmapState, fetchData: archiveRoadmap } =
    useApiMethod<unknown, string>(roadmapTreeApiDeclaration.archive);
  const {
    process: { loading: archiveLoading, error: archiveError },
    data: archivedRoadmap,
  } = archiveRoadmapState;

  useEffect(() => {
    fetchData({
      PageSize: pageSize,
      PageNumber: pageNumber,
    });
  }, [pageNumber, archivedRoadmap, fetchData]);

  const handleNextPage = useCallback(() => {
    setPageNumber(pageNumber + 1);
  }, [pageNumber]);

  const createRoadmapItem = (roadmap: Roadmap) => {
    const roadmapLink = generatePath(pathnames.roadmap, { id: roadmap.id });
    const roadmapEditLink = generatePath(pathnames.roadmapEdit, {
      id: roadmap.id,
    });

    return (
      <li key={roadmap.id}>
        <Link to={roadmapLink} className="no-underline">
          <div
            className={`${roadmapItemThemedClassName} bg-wrap p-[1rem] mb-[0.25rem] flex flex-col rounded-[0.5rem]`}
          >
            <img
              className="object-cover"
              alt={`roadmap ${roadmap.name} image`}
              src={roadmap.imageBase64 || 'roadmap-placeholder.png'}
            />
            <Gap sizeRem={0.75} />
            <Typography size="m" semibold>
              {roadmap.name}
            </Typography>
            <Gap sizeRem={0.25} />
            <Typography size="s" secondary>
              {roadmap.description ||
                `${localizationCaptions[LocalizationKey.Learn]} ${roadmap.name}.`}
            </Typography>
            {admin && (
              <div className="flex ml-auto">
                <Link to={roadmapEditLink}>
                  <Button>
                    <Icon size="m" name={IconNames.Settings} />
                  </Button>
                </Link>
                <ActionModal
                  openButtonCaption="📁"
                  error={archiveError}
                  loading={archiveLoading}
                  title={localizationCaptions[LocalizationKey.Archive]}
                  loadingCaption={
                    localizationCaptions[LocalizationKey.ArchiveLoading]
                  }
                  onAction={() => {
                    archiveRoadmap(roadmap.id);
                  }}
                />
              </div>
            )}
          </div>
        </Link>
      </li>
    );
  };

  return (
    <>
      <PageHeader title="" overlapping>
        {admin && (
          <Link to={pathnames.roadmapCreate}>
            <Button variant="active" className="h-[2.5rem]" aria-hidden>
              <Icon name={IconNames.Add} />
              {localizationCaptions[LocalizationKey.Create]}
            </Button>
          </Link>
        )}
      </PageHeader>
      <Gap sizeRem={2.25} />
      <div className="text-left flex flex-col">
        <Typography size="xxxl" semibold>
          {localizationCaptions[LocalizationKey.RoadmapsPageName]}
        </Typography>
        <Gap sizeRem={0.75} />
        <Typography size="s" semibold secondary>
          {localizationCaptions[LocalizationKey.RoadmapsPageDescription]}
        </Typography>
      </div>
      <Gap sizeRem={1.75} />
      <ItemsGrid
        currentData={roadmaps}
        loading={loading}
        error={error}
        triggerResetAccumData={`${archivedRoadmap}`}
        className="grid gap-[0.5rem] grid-cols-grid-view"
        renderItem={createRoadmapItem}
        nextPageAvailable={false}
        handleNextPage={handleNextPage}
      />
    </>
  );
};
