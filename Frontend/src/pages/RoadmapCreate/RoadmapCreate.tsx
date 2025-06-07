import React, { FunctionComponent, useEffect, useState } from 'react';
import toast from 'react-hot-toast';
import { PageHeader } from '../../components/PageHeader/PageHeader';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../localization';
import { Button } from '../../components/Button/Button';
import { Gap } from '../../components/Gap/Gap';
import { useApiMethod } from '../../hooks/useApiMethod';
import { Roadmap, RoadmapItem, RoadmapItemType } from '../../types/roadmap';
import {
  roadmapTreeApiDeclaration,
  UpsertRoadmapBody,
} from '../../apiDeclarations';
import { Loader } from '../../components/Loader/Loader';
import { Typography } from '../../components/Typography/Typography';
import { useNavigate, useParams } from 'react-router-dom';
import { Icon } from '../Room/components/Icon/Icon';
import { IconNames, pathnames } from '../../constants';
import { QuestionTreeSelector } from '../Roadmap/components/QuestionTreeSelector';

interface RoadmapCreateProps {
  edit: boolean;
}

const sortByOrder = (item1: RoadmapItem, item2: RoadmapItem) =>
  item1.order - item2.order;

export const RoadmapCreate: FunctionComponent<RoadmapCreateProps> = ({
  edit,
}) => {
  const { id } = useParams();
  const navigate = useNavigate();
  const localizationCaptions = useLocalizationCaptions();
  const [roadmapName, setRoadmapName] = useState('Roadmap');
  const [roadmapOrder, setRoadmapOrder] = useState(0);
  const [roadmapDescription, setRoadmapDescription] = useState<
    string | undefined
  >(undefined);
  const [roadmapImageBase64, setRoadmapImageBase64] = useState<
    string | undefined
  >(undefined);
  const [roadmapItems, setRoadmapItems] = useState<RoadmapItem[]>([]);

  const { apiMethodState, fetchData } = useApiMethod<string, UpsertRoadmapBody>(
    roadmapTreeApiDeclaration.upsert,
  );

  const { apiMethodState: roadmapApiMethodState, fetchData: fetchRoadmap } =
    useApiMethod<Roadmap, string>(roadmapTreeApiDeclaration.get);

  const {
    process: { loading: roadmapLoading, error: roadmapError },
    data: roadmap,
  } = roadmapApiMethodState;

  const {
    process: { loading, error },
    data: upsertedRoadmap,
  } = apiMethodState;

  const totalLoading = roadmapLoading || loading;
  const totalError = error || roadmapError;

  useEffect(() => {
    if (!id || !edit) {
      return;
    }
    fetchRoadmap(id);
  }, [id, edit, fetchRoadmap]);

  useEffect(() => {
    if (!roadmap) {
      return;
    }
    setRoadmapName(roadmap.name);
    setRoadmapOrder(roadmap.order);
    setRoadmapDescription(roadmap.description);
    setRoadmapImageBase64(roadmap.imageBase64);
    const newRoadmapItems = roadmap.items.map((item, index) => ({
      ...item,
      order: index,
    }));
    setRoadmapItems(newRoadmapItems);
  }, [roadmap]);

  const handleUpdateRoadmapItem = (newItem: RoadmapItem) => {
    const newItems = roadmapItems.map((item) => {
      if (item.id !== newItem.id) {
        return item;
      }
      return newItem;
    });
    setRoadmapItems(newItems);
  };

  const handleAddRoadmapItem = () => {
    const newItem: RoadmapItem = {
      id: String(Math.random()),
      type: RoadmapItemType.Milestone,
      name: `Milestone ${roadmapItems.length + 1}`,
      order: roadmapItems.length,
    };
    setRoadmapItems([...roadmapItems, newItem]);
  };

  const handleRemoveRoadmapItem = (id: string) => {
    const newRoadmapItems = roadmapItems
      .filter((item) => item.id !== id)
      .map((item, index) => ({ ...item, order: index }));
    setRoadmapItems(newRoadmapItems);
  };

  const handleUpsert = () => {
    const itemsForRequest: UpsertRoadmapBody['items'] = roadmapItems.map(
      (item) => {
        if (item.type === RoadmapItemType.Milestone) {
          return {
            type: item.type,
            name: item.name,
            order: item.order === 0 ? item.order : -1,
          };
        }
        if (item.type === RoadmapItemType.QuestionTree) {
          return {
            type: item.type,
            questionTreeId: item.questionTreeId,
            order: item.order,
          };
        }
        return item;
      },
    );
    fetchData({
      tags: [],
      id: id || undefined,
      name: roadmapName,
      items: itemsForRequest,
      order: roadmapOrder,
      description: roadmapDescription,
      imageBase64: roadmapImageBase64,
    });
  };

  useEffect(() => {
    if (!upsertedRoadmap) {
      return;
    }
    toast.success(localizationCaptions[LocalizationKey.Saved]);
    navigate(pathnames.roadmaps);
  }, [upsertedRoadmap, localizationCaptions, navigate]);

  return (
    <>
      <PageHeader
        title={localizationCaptions[LocalizationKey.RoadmapCreatePageName]}
      />
      {totalLoading && <Loader />}
      {totalError && (
        <Typography size="m" error>
          {totalError}
        </Typography>
      )}
      <div className="flex flex-col items-center">
        <div>
          <div className="flex flex-col">
            <div className="flex items-center">
              <Typography size="m">Name: </Typography>
              <Gap sizeRem={0.5} horizontal />
              <input
                type="text"
                className="bg-wrap"
                value={roadmapName}
                onChange={(e) => {
                  setRoadmapName(e.target.value as string);
                }}
              />
            </div>
            <Gap sizeRem={0.5} />
            <div className="flex items-center">
              <Typography size="m">Order: </Typography>
              <Gap sizeRem={0.5} horizontal />
              <input
                type="number"
                className="bg-wrap"
                value={roadmapOrder}
                onChange={(e) => {
                  setRoadmapOrder(Number(e.target.value));
                }}
              />
            </div>
            <Gap sizeRem={0.5} />
            <div className="flex items-center">
              <Typography size="m">Description: </Typography>
              <Gap sizeRem={0.5} horizontal />
              <input
                type="text"
                className="bg-wrap"
                value={roadmapDescription || ''}
                onChange={(e) => {
                  setRoadmapDescription(e.target.value as string);
                }}
              />
            </div>
            <Gap sizeRem={0.5} />
            <div className="flex items-center">
              <Typography size="m">Image: </Typography>
              <Gap sizeRem={0.5} horizontal />
              <input
                type="file"
                accept="image/*"
                className="bg-wrap"
                onChange={(e) => {
                  const file = e.target.files?.[0];
                  if (!file) {
                    return;
                  }
                  const fileReader = new FileReader();
                  fileReader.addEventListener('load', (frEvent) => {
                    const result = frEvent.target?.result;
                    if (typeof result === 'string') {
                      setRoadmapImageBase64(result);
                    }
                  });

                  fileReader.readAsDataURL(file);
                }}
              />
              <Gap sizeRem={0.5} horizontal />
              <Button
                variant="invertedActive"
                onClick={() => setRoadmapImageBase64(undefined)}
              >
                Clear
              </Button>
            </div>
            {roadmapImageBase64 && (
              <>
                <Gap sizeRem={0.5} />
                <div className="flex items-center">
                  <Typography size="m">Image preview: </Typography>
                  <Gap sizeRem={0.5} horizontal />
                  <img
                    src={roadmapImageBase64}
                    alt="roadmap image preview"
                    className="w-[16.125rem] h-[8.875rem] object-cover"
                  />
                </div>
              </>
            )}
          </div>
          <Gap sizeRem={1.75} />
          <div className="w-fit flex flex-col">
            {roadmapItems.sort(sortByOrder).map((item) => (
              <div key={item.id} className="flex mb-[0.5rem]">
                <select
                  className="muted w-[18rem] mr-[0.5rem]"
                  value={item.type}
                  onChange={(e) => {
                    handleUpdateRoadmapItem({
                      ...item,
                      type: e.target.value as RoadmapItemType,
                    });
                  }}
                >
                  <option value="Milestone">Milestone</option>
                  <option value="QuestionTree">QuestionTree</option>
                </select>
                {item.type === 'Milestone' && (
                  <input
                    type="text"
                    className="bg-wrap w-[18rem] mr-[0.5rem]"
                    value={item.name}
                    onChange={(e) => {
                      handleUpdateRoadmapItem({
                        ...item,
                        name: e.target.value as string,
                      });
                    }}
                  />
                )}
                {item.type === 'QuestionTree' && (
                  <QuestionTreeSelector
                    selectedTreeId={item.questionTreeId}
                    onSelect={(treeId) => {
                      handleUpdateRoadmapItem({
                        ...item,
                        questionTreeId: treeId,
                      });
                    }}
                  />
                )}
                <input
                  type="number"
                  className="bg-wrap w-[4rem] mr-[0.5rem]"
                  value={item.order}
                  onChange={(e) => {
                    handleUpdateRoadmapItem({
                      ...item,
                      order: Number(e.target.value),
                    });
                  }}
                />
                <Button
                  variant="active2"
                  className="min-w-[5rem]"
                  onClick={() => handleRemoveRoadmapItem(item.id)}
                >
                  <Icon size="s" name={IconNames.Trash} />
                </Button>
              </div>
            ))}
            <Gap sizeRem={0.85} />
            <Button variant="active2" onClick={handleAddRoadmapItem}>
              <Icon name={IconNames.Add} />
            </Button>
            <Gap sizeRem={0.75} />
            <Button variant="active" onClick={handleUpsert}>
              {localizationCaptions[LocalizationKey.Save]}
            </Button>
          </div>
        </div>
      </div>
    </>
  );
};
