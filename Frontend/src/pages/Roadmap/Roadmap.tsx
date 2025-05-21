import React, { FunctionComponent, useEffect } from 'react';
import { PageHeader } from '../../components/PageHeader/PageHeader';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { generatePath, useNavigate } from 'react-router-dom';
import { useApiMethod } from '../../hooks/useApiMethod';
import { Room, RoomAccessType } from '../../types/room';
import { CreateRoomBody, roomsApiDeclaration } from '../../apiDeclarations';
import { pathnames } from '../../constants';
import { Loader } from '../../components/Loader/Loader';
import { Typography } from '../../components/Typography/Typography';
import { LocalizationKey } from '../../localization';
import { Milestone } from './components/Milestone';
import { Gap } from '../../components/Gap/Gap';
import { RoadmapProgress } from './components/RoadmapProgress';
import { getRoadmapProgress } from './utils/getRoadmapProgress';

const roomDuration = 3600;
export const notAvailableId = 'notAvailable';

const progressTreeIds = [
  '7727b396-c2c8-423f-8a69-c29a2a126c64',
  'cd9a7aef-6a13-4369-8925-277db6ef4504',
  '5314ccc7-9cd9-46cc-afb6-5fbaf32c1d6a',
  'acb971a7-add1-4fcf-b158-4b14f54aeb54',
  '91d092dd-a2af-4b08-9e93-fb59c702cdbb',
  '1e4d88c2-ea15-4484-95f9-d2730e53ed67',
  '9ffb195f-ea3a-4cae-abd9-013bda0d8ab0',
  '55430493-cb5f-44bc-afc8-947bc06d7c76',
  '9608b396-973b-4864-9103-9378fbb832c6',
  'fd75ec8a-65c3-4104-ab40-f7cb96c268f7',
  '1a43d56a-1f15-49ee-8f49-06b85352dea2',
  '1fe41bb5-3424-481b-9577-651bc6392c20',
];

export const Roadmap: FunctionComponent = () => {
  const localizationCaptions = useLocalizationCaptions();
  const navigate = useNavigate();
  const roadmapProgress = getRoadmapProgress(progressTreeIds);

  const { apiMethodState, fetchData } = useApiMethod<Room, CreateRoomBody>(
    roomsApiDeclaration.create,
  );
  const {
    process: { loading, error },
    data: createdRoom,
  } = apiMethodState;

  const handleCreateRoom = (treeId: string, treeName: string) => {
    const roomStartDate = new Date();
    roomStartDate.setMinutes(roomStartDate.getMinutes() + 15);

    fetchData({
      name: treeName,
      questionTreeId: treeId,
      experts: [],
      examinees: [],
      tags: [],
      accessType: RoomAccessType.Private,
      scheduleStartTime: roomStartDate.toISOString(),
      duration: roomDuration,
    });
  };

  useEffect(() => {
    if (!createdRoom) {
      return;
    }
    navigate(generatePath(pathnames.room, { id: createdRoom.id }));
  }, [createdRoom, navigate]);

  return (
    <>
      <PageHeader title="" />

      <div className="flex">
        <div className="flex-1">
          <Typography size="xxxl">
            {localizationCaptions[LocalizationKey.RoadmapJsPageName]}
          </Typography>
          <Gap sizeRem={2.75} />
          <div className="flex flex-col items-center justify-center">
            {loading && <Loader />}

            {error && (
              <div>
                <Typography size="m" error>
                  {error}
                </Typography>
                <Gap sizeRem={1} />
              </div>
            )}

            {!loading && (
              <div className="w-full max-w-[64rem] flex">
                <div className="w-full">
                  <Milestone
                    name="Сеть"
                    arrow
                    trees={[
                      {
                        id: notAvailableId,
                        name: 'Архитектура клиент-сервер',
                      },
                      {
                        id: notAvailableId,
                        name: 'Протокол HTTP',
                      },
                    ]}
                    onCreateRoom={handleCreateRoom}
                  />

                  <Milestone
                    name="Основы"
                    trees={[
                      {
                        id: '1fe41bb5-3424-481b-9577-651bc6392c20',
                        name: 'Основы',
                      },
                      {
                        id: '1a43d56a-1f15-49ee-8f49-06b85352dea2',
                        name: 'Типы данных',
                      },
                      {
                        id: 'fd75ec8a-65c3-4104-ab40-f7cb96c268f7',
                        name: 'Переменные',
                      },
                      {
                        id: '9608b396-973b-4864-9103-9378fbb832c6',
                        name: 'Операторы',
                      },
                      {
                        id: '55430493-cb5f-44bc-afc8-947bc06d7c76',
                        name: 'Условия, циклы',
                      },
                      {
                        id: '9ffb195f-ea3a-4cae-abd9-013bda0d8ab0',
                        name: 'Сборщик мусора',
                      },
                      {
                        id: '1e4d88c2-ea15-4484-95f9-d2730e53ed67',
                        name: 'Функции',
                      },
                      {
                        id: '91d092dd-a2af-4b08-9e93-fb59c702cdbb',
                        name: 'Область видимости',
                      },
                      {
                        id: 'acb971a7-add1-4fcf-b158-4b14f54aeb54',
                        name: 'Лексическое окружение',
                      },
                      {
                        id: '5314ccc7-9cd9-46cc-afb6-5fbaf32c1d6a',
                        name: 'Контекст выполнения',
                      },
                    ]}
                    onCreateRoom={handleCreateRoom}
                  />
                </div>
                <Gap sizeRem={2} horizontal />
                <div className="w-full">
                  <Milestone
                    name="Задачи с собеседований"
                    arrow
                    trees={[
                      {
                        id: 'cd9a7aef-6a13-4369-8925-277db6ef4504',
                        name: 'Задачи с собеседований 1',
                      },
                      {
                        id: '7727b396-c2c8-423f-8a69-c29a2a126c64',
                        name: 'Задачи с собеседований 2',
                      },
                    ]}
                    onCreateRoom={handleCreateRoom}
                  />
                  <Milestone
                    name="Творческие задачи"
                    trees={[
                      {
                        id: notAvailableId,
                        name: 'Задачи с функциями',
                      },
                      {
                        id: notAvailableId,
                        name: 'Задачи с классами',
                      },
                    ]}
                    onCreateRoom={handleCreateRoom}
                  />
                </div>
              </div>
            )}
          </div>
        </div>
        <Gap sizeRem={1} horizontal />
        <div className="flex w-full max-w-[14rem] justify-center">
          <div className="w-full max-w-[26rem]">
            <RoadmapProgress {...roadmapProgress} />
          </div>
        </div>
      </div>
    </>
  );
};
