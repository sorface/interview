import React, {
  ChangeEvent,
  FunctionComponent,
  useEffect,
  useState,
} from 'react';
import { PageHeader } from '../../components/PageHeader/PageHeader';
import { useApiMethod } from '../../hooks/useApiMethod';
import {
  RoomBusinessAnalyticParams,
  roomsApiDeclaration,
} from '../../apiDeclarations';
import { RoomBusinessAnalytic } from '../../types/room';
import { BusinessAnalyticRoomTable } from './BusinessAnalyticRoomTable';
import { Typography } from '../../components/Typography/Typography';
import { Gap } from '../../components/Gap/Gap';
import { Loader } from '../../components/Loader/Loader';
import { formatDateForInput } from '../../utils/formatDateForInput';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../localization';

const getInitialStartDate = () => {
  const date = new Date();
  date.setMonth(date.getMonth() - 1);
  return formatDateForInput(date);
};

const getInitialEndDate = () => {
  return formatDateForInput(new Date());
};

export const BusinessAnalytic: FunctionComponent = () => {
  const localizationCaptions = useLocalizationCaptions();
  const [startDate, setStartDate] = useState(getInitialStartDate());
  const [endDate, setEndDate] = useState(getInitialEndDate());
  const { apiMethodState, fetchData } = useApiMethod<
    RoomBusinessAnalytic,
    RoomBusinessAnalyticParams
  >(roomsApiDeclaration.businessAnalytic);
  const {
    process: { loading, error },
    data: businessAnalytic,
  } = apiMethodState;

  useEffect(() => {
    if (!startDate || !endDate) {
      return;
    }
    fetchData({
      startDate: new Date(startDate).toISOString(),
      endDate: new Date(endDate).toISOString(),
      dateSort: 'Desc',
    });
  }, [startDate, endDate, fetchData]);

  const handleChangeStartDate = (e: ChangeEvent<HTMLInputElement>) => {
    setStartDate(e.target.value);
  };

  const handleChangeEndDate = (e: ChangeEvent<HTMLInputElement>) => {
    setEndDate(e.target.value);
  };

  return (
    <>
      <PageHeader
        title={localizationCaptions[LocalizationKey.BusinessAnalyticPageName]}
      />
      <div className="flex justify-center">
        <div className="w-fit">
          <div className="flex items-center">
            <input
              id="roomDate"
              value={startDate}
              type="date"
              required
              className="mr-[0.5rem]"
              onChange={handleChangeStartDate}
            />
            <Gap sizeRem={0.25} horizontal />
            <Typography size="m">-</Typography>
            <Gap sizeRem={0.5} horizontal />
            <input
              id="roomDate"
              value={endDate}
              type="date"
              required
              className="mr-[0.5rem]"
              onChange={handleChangeEndDate}
            />
          </div>
          <Gap sizeRem={1.75} />
          <div className="flex flex-col items-center">
            {loading && <Loader />}
            {error && (
              <Typography size="m" error>
                {error}
              </Typography>
            )}
            {businessAnalytic &&
              Object.keys(businessAnalytic).map((businessAnalyticKey) => (
                <div key={businessAnalyticKey}>
                  <div className="capitalize">
                    <Typography size="l" bold>
                      {businessAnalyticKey}
                    </Typography>
                  </div>
                  <Gap sizeRem={0.25} />
                  <table>
                    <thead>
                      <tr>
                        <th>Date</th>
                        <th>New</th>
                        <th>Active</th>
                        <th>Review</th>
                        <th>Close</th>
                      </tr>
                    </thead>
                    <tbody>
                      {Object.keys(businessAnalytic).map(
                        (businessAnalyticKey) => (
                          <BusinessAnalyticRoomTable
                            key={businessAnalyticKey}
                            items={
                              businessAnalytic[
                                businessAnalyticKey as keyof RoomBusinessAnalytic
                              ]
                            }
                          />
                        ),
                      )}
                    </tbody>
                  </table>
                  <Gap sizeRem={2.15} />
                </div>
              ))}
          </div>
        </div>
      </div>
    </>
  );
};
