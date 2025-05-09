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
      <div className="pt-24">
        <div className="container px-3 mx-auto flex flex-wrap flex-col md:flex-row items-center">
          <div className="flex flex-col w-full md:w-2/5 justify-center items-start text-center md:text-left">
            <h1 className="my-4 text-5xl font-bold leading-tight">
              {VITE_APP_NAME}
            </h1>
            <p className="leading-normal text-2xl mb-8">
              {localizationCaptions[LocalizationKey.HomeDescription]}
            </p>
            <HomeAction />
            <Gap sizeRem={0.15} />
            <div className="whitespace-break-spaces">
              <Typography size="s" secondary>
                {localizationCaptions[LocalizationKey.TermsOfUsageAcceptance]}
                <Link to={pathnames.terms}>
                  {localizationCaptions[LocalizationKey.TermsOfUsage]}
                </Link>
              </Typography>
            </div>
          </div>
          <div className="w-full md:w-3/5 py-6 text-center">
            <div className="w-full flex items-center justify-center md:w-4/5 z-50">
              <div
                style={{
                  width: '291px',
                  height: '291px',
                }}
              />
              <div
                className="relative cursor-pointer"
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
          </div>
        </div>
      </div>
      <Gap sizeRem={2.75} />
      <div className="relative -mt-12 lg:-mt-24">
        <svg
          viewBox="0 0 1428 174"
          version="1.1"
          xmlns="http://www.w3.org/2000/svg"
          xmlnsXlink="http://www.w3.org/1999/xlink"
        >
          <g stroke="none" strokeWidth="1" fill="none" fillRule="evenodd">
            <g
              transform="translate(-2.000000, 44.000000)"
              fill="#FFFFFF"
              fillRule="nonzero"
            >
              <path
                d="M0,0 C90.7283404,0.927527913 147.912752,27.187927 291.910178,59.9119003 C387.908462,81.7278826 543.605069,89.334785 759,82.7326078 C469.336065,156.254352 216.336065,153.6679 0,74.9732496"
                opacity="0.100000001"
              ></path>
              <path
                d="M100,104.708498 C277.413333,72.2345949 426.147877,52.5246657 546.203633,45.5787101 C666.259389,38.6327546 810.524845,41.7979068 979,55.0741668 C931.069965,56.122511 810.303266,74.8455141 616.699903,111.243176 C423.096539,147.640838 250.863238,145.462612 100,104.708498 Z"
                opacity="0.100000001"
              ></path>
              <path
                d="M1046,51.6521276 C1130.83045,29.328812 1279.08318,17.607883 1439,40.1656806 L1439,120 C1271.17211,77.9435312 1140.17211,55.1609071 1046,51.6521276 Z"
                id="Path-4"
                opacity="0.200000003"
              ></path>
            </g>
            <g
              transform="translate(-4.000000, 76.000000)"
              fill="#FFFFFF"
              fillRule="nonzero"
            >
              <path d="M0.457,34.035 C57.086,53.198 98.208,65.809 123.822,71.865 C181.454,85.495 234.295,90.29 272.033,93.459 C311.355,96.759 396.635,95.801 461.025,91.663 C486.76,90.01 518.727,86.372 556.926,80.752 C595.747,74.596 622.372,70.008 636.799,66.991 C663.913,61.324 712.501,49.503 727.605,46.128 C780.47,34.317 818.839,22.532 856.324,15.904 C922.689,4.169 955.676,2.522 1011.185,0.432 C1060.705,1.477 1097.39,3.129 1121.236,5.387 C1161.703,9.219 1208.621,17.821 1235.4,22.304 C1285.855,30.748 1354.351,47.432 1440.886,72.354 L1441.191,104.352 L1.121,104.031 L0.457,34.035 Z"></path>
            </g>
          </g>
        </svg>
      </div>
      <section className="bg-white border-b py-8">
        <div className="container max-w-5xl mx-auto m-8">
          <h2 className="w-full my-2 text-5xl font-bold leading-tight text-center text-gray-800">
            Собеседования с AI
          </h2>
          <div className="w-full mb-4">
            <div className="h-1 mx-auto gradient w-64 opacity-25 my-0 py-0 rounded-t"></div>
          </div>
          <div className="flex flex-wrap">
            <div className="w-5/6 sm:w-1/2 p-6">
              <h3 className="text-3xl text-gray-800 font-bold leading-none mb-3">
                Собеседования по теории
              </h3>
              <p className="text-gray-600 mb-8">
                Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aliquam
                at ipsum eu nunc commodo posuere et sit amet ligula.
              </p>
            </div>
            <div className="w-full sm:w-1/2 p-6">
              <img src="/ai-theory.png" />
            </div>
          </div>
          <div className="flex flex-wrap flex-col-reverse sm:flex-row">
            <div className="w-full sm:w-1/2 p-6 mt-6">
              <img src="/ai-coding.png" />
            </div>
            <div className="w-full sm:w-1/2 p-6 mt-6">
              <div className="align-middle">
                <h3 className="text-3xl text-gray-800 font-bold leading-none mb-3">
                  Собеседования по кодингу
                </h3>
                <p className="text-gray-600 mb-8">
                  Lorem ipsum dolor sit amet, consectetur adipiscing elit.
                  Aliquam at ipsum eu nunc commodo posuere et sit amet ligula.
                </p>
              </div>
            </div>
          </div>
        </div>
      </section>
      <section className="bg-gray-100 py-8">
        <div className="container mx-auto px-2 pt-4 pb-12 text-gray-800">
          <h2 className="w-full my-2 text-5xl font-bold leading-tight text-center text-gray-800">
            Стоимость
          </h2>
          <div className="w-full mb-4">
            <div className="h-1 mx-auto gradient w-64 opacity-25 my-0 py-0 rounded-t"></div>
          </div>
          <div className="flex flex-col sm:flex-row justify-center pt-12 my-12 sm:my-4">
            <div className="flex flex-col w-[25.625rem] lg:w-1/3 mx-auto lg:mx-0 rounded-lg bg-white mt-4 sm:-mt-6 shadow-lg">
              <div className="flex-1 bg-white rounded-t rounded-b-none overflow-hidden shadow">
                <div className="w-full p-8 text-3xl font-bold text-center">
                  Бесплатно
                </div>
                <div className="h-1 w-full gradient my-0 py-0 rounded-t"></div>
                <ul className="w-full text-center text-base font-bold">
                  <li className="border-b py-4">
                    Полный доступ к базе вопросов
                  </li>
                  <li className="border-b py-4">
                    Бесконечное количество собеседований
                  </li>
                </ul>
              </div>
              <div className="flex-none mt-auto bg-white rounded-b rounded-t-none overflow-hidden shadow p-6">
                <div className="flex items-center justify-center">
                  <HomeAction />
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>
      <div className="fixed bottom-0 w-full flex flex-col items-center justify-center">
        <LangSwitch elementType="switcherButton" />
        <Gap sizeRem={0.75} />
      </div>
    </>
  );
};
