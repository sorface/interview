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

export const Roadmap: FunctionComponent = () => {
  const navigate = useNavigate();
  const { id } = useParams();

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
  const progressTreeIds =
    roadmap?.items.map((item) => item.questionTreeId).filter(Boolean) || [];
  const roadmapProgress = getRoadmapProgress(progressTreeIds);

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
