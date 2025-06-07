import React, { FunctionComponent, useContext, useEffect } from 'react';
import { generatePath, useNavigate, useParams } from 'react-router-dom';
import { useApiMethod } from '../../hooks/useApiMethod';
import { Room, RoomAccessType } from '../../types/room';
import { RoadmapItem, Roadmap as RoadmapType } from '../../types/roadmap';
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
import { SvgRoadmap } from './components/SvgRoadmap';
import { DeviceContext } from '../../context/DeviceContext';

const roomDuration = 3600;
const svgRoadmapFingerprint = 'MQQMQQQQQQQQQMQQQQMQQ';
const getRoadmapFingerprint = (items: RoadmapItem[]) => {
  const fingerprint = items
    .map((item) => {
      return item.type[0];
    })
    .join('');
  return fingerprint;
};

export const Roadmap: FunctionComponent = () => {
  const navigate = useNavigate();
  const { id } = useParams();
  const device = useContext(DeviceContext);

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
  const svgRoadmap =
    device === 'Desktop' &&
    getRoadmapFingerprint(roadmap?.items || []) === svgRoadmapFingerprint;

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
      <Gap sizeRem={2.25} />
      <div className={`flex ${device === 'Mobile' ? 'flex-col' : ''}`}>
        <div className="flex-1">
          <div className="flex flex-col">
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
              <div className="w-full max-w-[60rem] flex flex-col items-start">
                <Typography size="xxxl" bold>
                  {roadmap?.name}
                </Typography>
                {device === 'Mobile' && (
                  <>
                    <Gap sizeRem={1} />
                    <div className="w-full">
                      <RoadmapProgress {...roadmapProgress} />
                    </div>
                  </>
                )}
                <Gap sizeRem={device === 'Desktop' ? 2.25 : 0.75} />
                {svgRoadmap ? (
                  <SvgRoadmap
                    items={roadmap?.items || []}
                    handleCreateRoom={handleCreateRoom}
                    onRoomAlreadyExists={handleRoomAlreadyExists}
                  />
                ) : (
                  roadmap?.items.map((roadmapItem, roadmapItemIndex) => {
                    if (roadmapItem.type !== 'Milestone') {
                      return;
                    }
                    return (
                      <Milestone
                        key={roadmapItem.id}
                        name={roadmapItem.name || ''}
                        trees={findMilestoneTrees(
                          roadmap?.items || [],
                          roadmapItemIndex,
                        )}
                        onCreateRoom={handleCreateRoom}
                        onRoomAlreadyExists={handleRoomAlreadyExists}
                      />
                    );
                  })
                )}
              </div>
            )}
          </div>
        </div>
        {device === 'Desktop' && (
          <>
            <Gap sizeRem={1} horizontal />
            <div className="flex w-full max-w-[18rem] justify-center">
              <div className="w-full max-w-[26rem]">
                <RoadmapProgress {...roadmapProgress} />
              </div>
            </div>
          </>
        )}
      </div>
    </>
  );
};
