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
  AiAssistantScriptName,
} from '../Room/components/AiAssistant/AiAssistant';
import { Canvas } from '@react-three/fiber';
import { EffectComposer, FXAA } from '@react-three/postprocessing';

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
      setAiAssistantScript(AiAssistantScriptName.Welcome);
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
          style={{ width: 450, height: 450 }}
          className="cursor-pointer"
          onClick={handleAiAssistantClick}
        >
          <Canvas shadows camera={{ position: [0, 0.05, 2.8], fov: 38 }}>
            <EffectComposer>
              <FXAA />
            </EffectComposer>
            <AiAssistant currentScript={aiAssistantScript} loading trackMouse />
          </Canvas>
        </div>
        <Typography size="xxl" bold>
          {VITE_NAME}
        </Typography>
        <Gap sizeRem={0.25} />
        <Typography size="m">
          {localizationCaptions[LocalizationKey.HomeDescription]}
        </Typography>
        <Gap sizeRem={1.25} />
        <HomeAction />
      </div>
      <div className="flex justify-center">
        <LangSwitch elementType="switcherButton" />
      </div>
    </>
  );
};
