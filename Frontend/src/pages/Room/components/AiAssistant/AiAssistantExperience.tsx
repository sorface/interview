import { FunctionComponent, memo } from "react";
import { Environment, useTexture } from "@react-three/drei";
import { useThree } from "@react-three/fiber";
import { AiAssistant } from "./AiAssistant";
import { Transcript } from "../../../../types/transcript";
import { useLocalizationCaptions } from "../../../../hooks/useLocalizationCaptions";
import { LocalizationKey } from "../../../../localization";

export interface AiAssistantExperienceProps {
  lastTranscription: Transcript | undefined;
}

const logoUrl = '/logo192.png';

const cleanUpTranscription = (value: string) =>
  value.trim().replace(/[.,?!]/g, '').toLocaleLowerCase();

const AiAssistantExperienceComponent: FunctionComponent<AiAssistantExperienceProps> = ({
  lastTranscription,
}) => {
  const texture = useTexture(logoUrl);
  const viewport = useThree((state) => state.viewport);
  const localizationCaptions = useLocalizationCaptions();

  const welcomeMessage = lastTranscription ?
    cleanUpTranscription(lastTranscription.value) === cleanUpTranscription(localizationCaptions[LocalizationKey.AiAssistantWelcomePrompt]) :
    false;

  return (
    <>
      <Environment preset='sunset' />
      <AiAssistant position={[0, -3, 5]} rotation={[-Math.PI / 2, 0, 0]} scale={2} speaking={welcomeMessage} />
      <mesh position={[0, 0, 0.25]} >
        <planeGeometry args={[viewport.width, viewport.height]} />
        <meshBasicMaterial map={texture} />
      </mesh>
    </>
  );
};

export const AiAssistantExperience = memo(AiAssistantExperienceComponent);
