import React, { FunctionComponent, useState } from 'react';
import { PageHeader } from '../../components/PageHeader/PageHeader';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../localization';
import { CodeEditor } from '../../components/CodeEditor/CodeEditor';
import { CodeEditorLang } from '../../types/question';

interface RoadmapCreateProps {}

export const RoadmapCreate: FunctionComponent<RoadmapCreateProps> = ({}) => {
  const localizationCaptions = useLocalizationCaptions();
  const [editorValue, setEditorValue] = useState<string | undefined>('');

  return (
    <>
      <PageHeader
        title={localizationCaptions[LocalizationKey.RoadmapCreatePageName]}
      />
      <CodeEditor
        language={CodeEditorLang.Json}
        languages={[CodeEditorLang.Json]}
        value={editorValue}
        onChange={setEditorValue}
      />
    </>
  );
};
