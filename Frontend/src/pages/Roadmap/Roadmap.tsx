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

const progressTreeIds = [
  '655c5171-a0ea-4858-bb14-fb53325178d5',
  'b15b714c-93df-4c65-b100-eaa6f62594d0',
  'b15b714c-93df-4c65-b100-eaa6f62594de',
  '1',
  '2',
  '3',
  '4',
  '5',
  '6',
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
                        id: 'Архитектура клиент-сервер',
                        name: 'Архитектура клиент-сервер',
                      },
                      {
                        id: 'Протокол HTTP',
                        name: 'Протокол HTTP',
                      },
                    ]}
                    onCreateRoom={handleCreateRoom}
                  />

                  <Milestone
                    name="Основы"
                    trees={[
                      {
                        id: 'Основы',
                        name: 'Основы',
                      },
                      {
                        id: 'Переменные',
                        name: 'Переменные',
                      },
                      {
                        id: 'Функции',
                        name: 'Функции',
                      },
                      {
                        id: 'Область видимости',
                        name: 'Область видимости',
                      },
                      {
                        id: 'Условия, циклы',
                        name: 'Условия, циклы',
                      },
                      {
                        id: 'Лексическое окружение',
                        name: 'Лексическое окружение',
                      },
                      {
                        id: 'Контекст выполнения',
                        name: 'Контекст выполнения',
                      },
                      {
                        id: 'Асинхронность',
                        name: 'Асинхронность',
                      },
                    ]}
                    onCreateRoom={handleCreateRoom}
                  />
                </div>
                <Gap sizeRem={2} horizontal />
                <div className="w-full">
                  <Milestone
                    name="Базовые задачи"
                    arrow
                    trees={[
                      {
                        id: '9c59607a-0036-4154-8246-2a3873b8d890',
                        name: 'Основные задачи',
                      },
                    ]}
                    onCreateRoom={handleCreateRoom}
                  />
                  <Milestone
                    name="Творческие задачи"
                    trees={[
                      {
                        id: 'e54916ca-6103-4c65-927f-010b8cccf8c7',
                        name: 'Задачи с фкнкциями',
                      },
                      {
                        id: '9c59607a-0036-4154-8246-2a3873b8d890',
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
