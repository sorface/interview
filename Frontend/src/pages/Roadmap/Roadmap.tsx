import React, { FunctionComponent, useEffect } from 'react';
import { PageHeader } from '../../components/PageHeader/PageHeader';
import { generatePath, useNavigate, useParams } from 'react-router-dom';
import { useApiMethod } from '../../hooks/useApiMethod';
import { Room, RoomAccessType } from '../../types/room';
import { Roadmap as RoadmapType } from '../../types/roadmap';
import {
  CreateRoomBody,
  roadmapTreeApiDeclaration,
  roomsApiDeclaration,
} from '../../apiDeclarations';
import { pathnames } from '../../constants';
import { Loader } from '../../components/Loader/Loader';
import { Typography } from '../../components/Typography/Typography';
import { Milestone } from './components/Milestone';
import { Gap } from '../../components/Gap/Gap';
import { RoadmapProgress } from './components/RoadmapProgress';
import { getRoadmapProgress } from './utils/getRoadmapProgress';
import { findMilestoneTrees } from './utils/findMilestoneTrees';

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
  const navigate = useNavigate();
  const { id } = useParams();
  const roadmapProgress = getRoadmapProgress(progressTreeIds);

  const { apiMethodState, fetchData } = useApiMethod<Room, CreateRoomBody>(
    roomsApiDeclaration.create,
  );
  const {
    process: { loading, error },
    data: createdRoom,
  } = apiMethodState;

  const { apiMethodState: roadmapApiMethodState, fetchData: fetchRoadmap } =
    useApiMethod<RoadmapType, string>(roadmapTreeApiDeclaration.get);

  const {
    process: { loading: roadmapLoading, error: roadmapError },
    data: roadmap,
  } = roadmapApiMethodState;

  const totalLoading = loading || roadmapLoading;
  const totalError = error || roadmapError;

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

  const handleRoomAlreadyExists = (roomId: string) => {
    navigate(generatePath(pathnames.room, { id: roomId }));
  };

  useEffect(() => {
    if (!id) {
      return;
    }
    fetchRoadmap(id);
  }, [id, fetchRoadmap]);

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
          <Typography size="xxxl">{roadmap?.name}</Typography>
          <Gap sizeRem={2.75} />
          <div className="flex flex-col items-center justify-center">
            {totalLoading && <Loader />}

            {totalError && (
              <div>
                <Typography size="m" error>
                  {totalError}
                </Typography>
                <Gap sizeRem={1} />
              </div>
            )}

            {!loading && (
              <div className="w-full max-w-[64rem] flex flex-col">
                {roadmap?.items.map((roadmapItem, roadmapItemIndex) => {
                  if (roadmapItem.type !== 'Milestone') {
                    return;
                  }
                  const { trees, lastMilestone } = findMilestoneTrees(
                    roadmap?.items || [],
                    roadmapItemIndex,
                  );
                  return (
                    <Milestone
                      key={roadmapItem.id}
                      name={roadmapItem.name || ''}
                      arrow={!lastMilestone}
                      trees={trees}
                      onCreateRoom={handleCreateRoom}
                      onRoomAlreadyExists={handleRoomAlreadyExists}
                    />
                  );
                })}
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
