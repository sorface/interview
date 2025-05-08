import React, {
  FunctionComponent,
  useContext,
  useEffect,
  useState,
} from 'react';
import { Link, Navigate, useParams } from 'react-router-dom';
import { pathnames } from '../../constants';
import { AuthContext } from '../../context/AuthContext';
import { HomeAction } from './components/HomeAction';
import { LocalizationKey } from '../../localization';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { Gap } from '../../components/Gap/Gap';
import { Typography } from '../../components/Typography/Typography';
import { LangSwitch } from '../../components/LangSwitch/LangSwitch';
import { VITE_APP_NAME } from '../../config';
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
    return <Navigate to={pathnames.roadmapJs} replace />;
  }

  return (
    <>
      <div className="flex flex-1 flex-col justify-center items-center">
        <div className="w-full flex items-center justify-center">
          <div
            style={{
              width: '291px',
              height: '291px',
            }}
          />
          <div
            className="absolute cursor-pointer"
            style={{ width: '256px', height: '256px' }}
            onClick={handleAiAssistantClick}
          >
            <Canvas shadows camera={{ position: [0, 0.12, 1.5], fov: 38 }}>
              <AiAssistant
                currentScript={aiAssistantScript}
                loading
                loadingVariant={AiAssistantLoadingVariant.Wide}
                trackMouse
              />
            </Canvas>
          </div>
        </div>
        <Typography size="xxxl" bold>
          {VITE_APP_NAME}
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
      <div className="flex flex-col items-center">
        <div className="whitespace-break-spaces">
          <Typography size="s">
            {localizationCaptions[LocalizationKey.TermsOfUsageAcceptance]}
            <Link to={pathnames.terms}>
              {localizationCaptions[LocalizationKey.TermsOfUsage]}
            </Link>
          </Typography>
        </div>
        <Gap sizeRem={0.5} />
        <LangSwitch elementType="switcherButton" />
      </div>
    </>
  );
};
