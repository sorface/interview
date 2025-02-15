import React, {
  FunctionComponent,
  useContext,
  useEffect,
  useState,
} from 'react';
import { Navigate, useParams } from 'react-router-dom';
import { pathnames } from '../../constants';
import { AuthContext } from '../../context/AuthContext';
import { HomeAction } from './components/HomeContent/HomeAction';
import { LocalizationKey } from '../../localization';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { Gap } from '../../components/Gap/Gap';
import { Typography } from '../../components/Typography/Typography';
import { LangSwitch } from '../../components/LangSwitch/LangSwitch';
import { VITE_NAME } from '../../config';
import {
  AiAssistant,
  AiAssistantLoadingVariant,
  AiAssistantScriptName,
} from '../Room/components/AiAssistant/AiAssistant';
import { Canvas } from '@react-three/fiber';

export const Home: FunctionComponent = () => {
  const auth = useContext(AuthContext);
  const { redirect } = useParams();
  const localizationCaptions = useLocalizationCaptions();
  const [aiAssistantScript, setAiAssistantScript] = useState(
    AiAssistantScriptName.Idle,
  );
  const [aiAssistantClicked, setAiAssistantClicked] = useState(false);

  const handleAiAssistantClick = () => {
    setAiAssistantScript(AiAssistantScriptName.Idle);
    setAiAssistantClicked(true);
  };

  useEffect(() => {
    if (
      aiAssistantClicked &&
      aiAssistantScript === AiAssistantScriptName.Idle
    ) {
      setAiAssistantScript(AiAssistantScriptName.Login);
    }
  }, [aiAssistantScript, aiAssistantClicked]);

  if (auth && redirect) {
    return <Navigate to={redirect} replace />;
  }

  if (auth) {
    return <Navigate to={pathnames.highlightRooms} replace />;
  }

  return (
    <>
      <div className="flex flex-1 flex-col justify-center items-center">
        <div
          style={{ width: 291, height: 291 }}
          className="cursor-pointer"
          onClick={handleAiAssistantClick}
        >
          <Canvas shadows camera={{ position: [0, 0.05, 2.2], fov: 38 }}>
            <AiAssistant
              currentScript={aiAssistantScript}
              loading
              loadingVariant={AiAssistantLoadingVariant.Wide}
              trackMouse
            />
          </Canvas>
        </div>
        <Typography size="xxxl" bold>
          {VITE_NAME}
        </Typography>
        <Gap sizeRem={0.625} />
        <div className="whitespace-break-spaces">
          <Typography size="l">
            {localizationCaptions[LocalizationKey.HomeDescription]}
          </Typography>
        </div>
        <Gap sizeRem={1.05} />
        <HomeAction />
      </div>
      <div className="flex justify-center">
        <LangSwitch elementType="switcherButton" />
      </div>
    </>
  );
};
