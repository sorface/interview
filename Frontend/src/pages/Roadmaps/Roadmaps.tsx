import React, {
  FunctionComponent,
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
import { Link } from 'react-router-dom';

const pageSize = 30;
const initialPageNumber = 1;

export const Roadmaps: FunctionComponent = () => {
  const auth = useContext(AuthContext);
  const admin = checkAdmin(auth);
  const localizationCaptions = useLocalizationCaptions();
  const [pageNumber, setPageNumber] = useState(initialPageNumber);
  const { apiMethodState, fetchData } = useApiMethod<
    Roadmap[],
    PaginationUrlParams
  >(roadmapTreeApiDeclaration.getPage);

  const {
    process: { loading, error },
    data: roadmaps,
  } = apiMethodState;

  useEffect(() => {
    fetchData({
      PageSize: pageSize,
      PageNumber: pageNumber,
    });
  }, [pageNumber, fetchData]);

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
      <Typography size="xl">{JSON.stringify(roadmaps)}</Typography>
    </>
  );
};
