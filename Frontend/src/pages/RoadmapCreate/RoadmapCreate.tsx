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
import { Roadmap } from '../../types/roadmap';
import { roadmapTreeApiDeclaration } from '../../apiDeclarations';
import { Loader } from '../../components/Loader/Loader';
import { Typography } from '../../components/Typography/Typography';

interface RoadmapCreateProps {}

export const RoadmapCreate: FunctionComponent<RoadmapCreateProps> = ({}) => {
  const localizationCaptions = useLocalizationCaptions();
  const [editorValue, setEditorValue] = useState<string | undefined>('');

  const { apiMethodState, fetchData } = useApiMethod<string, Partial<Roadmap>>(
    roadmapTreeApiDeclaration.upsert,
  );

  const {
    process: { loading, error },
    data: upsertedRoadmap,
  } = apiMethodState;

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
        {localizationCaptions[LocalizationKey.Create]}
      </Button>
      <Gap sizeRem={0.85} />
      <CodeEditor
        language={CodeEditorLang.Json}
        languages={[CodeEditorLang.Json]}
        value={editorValue}
        onChange={setEditorValue}
      />
    </>
  );
};
