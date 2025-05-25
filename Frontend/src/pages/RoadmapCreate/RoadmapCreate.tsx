import React, { FunctionComponent, useEffect, useState } from 'react';
import toast from 'react-hot-toast';
import { PageHeader } from '../../components/PageHeader/PageHeader';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../localization';
import { CodeEditor } from '../../components/CodeEditor/CodeEditor';
import { CodeEditorLang } from '../../types/question';
import { Button } from '../../components/Button/Button';
import { Gap } from '../../components/Gap/Gap';
import { useApiMethod } from '../../hooks/useApiMethod';
import { Roadmap, RoadmapItem, RoadmapItemType } from '../../types/roadmap';
import { roadmapTreeApiDeclaration } from '../../apiDeclarations';
import { Loader } from '../../components/Loader/Loader';
import { Typography } from '../../components/Typography/Typography';
import { useParams } from 'react-router-dom';
import { Icon } from '../Room/components/Icon/Icon';
import { IconNames } from '../../constants';
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
  const localizationCaptions = useLocalizationCaptions();
  const [editorValue, setEditorValue] = useState<string | undefined>('');
  const [roadmapItems, setRoadmapItems] = useState<RoadmapItem[]>([]);
  console.log('roadmapItems: ', roadmapItems);

  const { apiMethodState, fetchData } = useApiMethod<string, Partial<Roadmap>>(
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

  useEffect(() => {
    if (!id || !edit) {
      return;
    }
    fetchRoadmap(id);
  }, [id, edit]);

  useEffect(() => {
    if (!roadmap) {
      return;
    }
    setEditorValue(JSON.stringify(roadmap));
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
      name: 'test',
      order: roadmapItems.length,
      type: 'Milestone',
      questionTreeId: '',
    };
    setRoadmapItems([...roadmapItems, newItem]);
  };

  const handleUpsert = () => {
    if (!editorValue) {
      return;
    }
    fetchData(JSON.parse(editorValue));
  };

  useEffect(() => {
    if (!upsertedRoadmap) {
      return;
    }
    toast.success(localizationCaptions[LocalizationKey.Saved]);
  }, [upsertedRoadmap]);

  return (
    <>
      <PageHeader
        title={localizationCaptions[LocalizationKey.RoadmapCreatePageName]}
      />
      {loading && <Loader />}
      {error && (
        <Typography size="m" error>
          {error}
        </Typography>
      )}
      <Button variant="active" onClick={handleUpsert}>
        {localizationCaptions[LocalizationKey.Save]}
      </Button>
      <Gap sizeRem={1.75} />
      {roadmapItems.sort(sortByOrder).map((item) => (
        <div key={item.id} className="flex">
          <select
            className="w-full muted"
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
            <option value="VerticalSplit">VerticalSplit</option>
          </select>
          {item.type === 'Milestone' && (
            <input
              type="text"
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
                handleUpdateRoadmapItem({ ...item, questionTreeId: treeId });
              }}
            />
          )}
          <input
            type="number"
            value={item.order}
            onChange={(e) => {
              handleUpdateRoadmapItem({
                ...item,
                order: Number(e.target.value),
              });
            }}
          />
        </div>
      ))}
      <Gap sizeRem={0.85} />
      <Button variant="active" onClick={handleAddRoadmapItem}>
        <Icon name={IconNames.Add} />
      </Button>
    </>
  );
};
