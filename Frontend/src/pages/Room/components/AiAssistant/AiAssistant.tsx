import React, { FunctionComponent, memo, useContext, useEffect, useMemo, useRef } from 'react'
import { useAnimations, useFBX, useGLTF } from '@react-three/drei'
import { GroupProps, useFrame, useLoader } from '@react-three/fiber'
import { FileLoader, Group, MathUtils, Object3DEventMap } from 'three';
import { LocalizationContext } from '../../../../context/LocalizationContext';

const modelUrl = '/models/661ad1c4a60647ec5c68e19b.glb';
const animationUrl = '/animations/Idle.fbx';

const corresponding = {
  A: 'viseme_PP',
  B: 'viseme_kk',
  C: 'viseme_I',
  D: 'viseme_AA',
  E: 'viseme_O',
  F: 'viseme_U',
  G: 'viseme_FF',
  H: 'viseme_TH',
  X: 'viseme_PP',
};

interface HrAvatarProps {
  speaking: boolean;
}

const AiAssistantComponent:FunctionComponent<GroupProps & HrAvatarProps> = (props) => {
  const { lang } = useContext(LocalizationContext);
  const { nodes, materials } = useGLTF(modelUrl);
  const { animations: idleAnimation } = useFBX(animationUrl);
  const animation = 'Idle';
  const script = 'hi-all';
  const audio = useMemo(() => new Audio(`/audios/${lang}/${script}.mp3`), [script, lang]);
  const jsonFile = useLoader(FileLoader, `/audios/${lang}/${script}.json`);
  const lipsync = JSON.parse(String(jsonFile));

  idleAnimation[0].name = animation;

  const group = useRef<Group<Object3DEventMap> | null>(null);
  const { actions } = useAnimations(
    [idleAnimation[0]],
    group
  );

  useFrame(() => {
    const currentAudioTime = audio.currentTime;
    if (audio.paused || audio.ended) {
      return;
    }

    const smoothMorphTarget = true;
    const morphTargetSmoothing = true;

    Object.values(corresponding).forEach((value) => {
      if (!smoothMorphTarget) {
        // @ts-ignore
        nodes.Wolf3D_Head.morphTargetInfluences[
          // @ts-ignore
          nodes.Wolf3D_Head.morphTargetDictionary[value]
        ] = 0;
        // @ts-ignore
        nodes.Wolf3D_Teeth.morphTargetInfluences[
          // @ts-ignore
          nodes.Wolf3D_Teeth.morphTargetDictionary[value]
        ] = 0;
      } else {
        // @ts-ignore
        nodes.Wolf3D_Head.morphTargetInfluences[
          // @ts-ignore
          nodes.Wolf3D_Head.morphTargetDictionary[value]
        ] = MathUtils.lerp(
          // @ts-ignore
          nodes.Wolf3D_Head.morphTargetInfluences[
            // @ts-ignore
            nodes.Wolf3D_Head.morphTargetDictionary[value]
          ],
          0,
          // @ts-ignore
          morphTargetSmoothing
        );

        // @ts-ignore
        nodes.Wolf3D_Teeth.morphTargetInfluences[
          // @ts-ignore
          nodes.Wolf3D_Teeth.morphTargetDictionary[value]
        ] = MathUtils.lerp(
          // @ts-ignore
          nodes.Wolf3D_Teeth.morphTargetInfluences[
            // @ts-ignore
            nodes.Wolf3D_Teeth.morphTargetDictionary[value]
          ],
          0,
          // @ts-ignore
          morphTargetSmoothing
        );
      }
    });

    for (let i = 0; i < lipsync.mouthCues.length; i++) {
      const mouthCue = lipsync.mouthCues[i];
      if (
        currentAudioTime >= mouthCue.start &&
        currentAudioTime <= mouthCue.end
      ) {
        if (!smoothMorphTarget) {
          // @ts-ignore
          nodes.Wolf3D_Head.morphTargetInfluences[
            // @ts-ignore
            nodes.Wolf3D_Head.morphTargetDictionary[
              // @ts-ignore
              corresponding[mouthCue.value]
            ]
          ] = 1;
          // @ts-ignore
          nodes.Wolf3D_Teeth.morphTargetInfluences[
            // @ts-ignore
            nodes.Wolf3D_Teeth.morphTargetDictionary[
              // @ts-ignore
              corresponding[mouthCue.value]
            ]
          ] = 1;
        } else {
          // @ts-ignore
          nodes.Wolf3D_Head.morphTargetInfluences[
            // @ts-ignore
            nodes.Wolf3D_Head.morphTargetDictionary[
              // @ts-ignore
              corresponding[mouthCue.value]
            ]
          ] = MathUtils.lerp(
            // @ts-ignore
            nodes.Wolf3D_Head.morphTargetInfluences[
              // @ts-ignore
              nodes.Wolf3D_Head.morphTargetDictionary[
                // @ts-ignore
                corresponding[mouthCue.value]
              ]
            ],
            1,
            // @ts-ignore
            morphTargetSmoothing
          );
          // @ts-ignore
          nodes.Wolf3D_Teeth.morphTargetInfluences[
            // @ts-ignore
            nodes.Wolf3D_Teeth.morphTargetDictionary[
              // @ts-ignore
              corresponding[mouthCue.value]
            ]
          ] = MathUtils.lerp(
            // @ts-ignore
            nodes.Wolf3D_Teeth.morphTargetInfluences[
              // @ts-ignore
              nodes.Wolf3D_Teeth.morphTargetDictionary[
                // @ts-ignore
                corresponding[mouthCue.value]
              ]
            ],
            1,
            // @ts-ignore
            morphTargetSmoothing
          );
        }

        break;
      }
    }
  });

  const playAudio = true;
  useEffect(() => {
    if (playAudio) {
      audio.play();
    } else {
      audio.pause();
    }
  }, [playAudio, script, audio]);

  useEffect(() => {
    actions[animation]?.play();
  }, [animation, actions]);

  useEffect(() => {
    if (!props.speaking) {
      audio.pause();
      return;
    }
    audio.play();
  }, [props.speaking, audio]);

  return (
    <group {...props} ref={group} dispose={null}>
      <primitive object={nodes.Hips} />
      {/* @ts-ignore */}
      <skinnedMesh geometry={nodes.Wolf3D_Body.geometry} material={materials.Wolf3D_Body} skeleton={nodes.Wolf3D_Body.skeleton} />
      {/* @ts-ignore */}
      <skinnedMesh geometry={nodes.Wolf3D_Outfit_Bottom.geometry} material={materials.Wolf3D_Outfit_Bottom} skeleton={nodes.Wolf3D_Outfit_Bottom.skeleton} />
      {/* @ts-ignore */}
      <skinnedMesh geometry={nodes.Wolf3D_Outfit_Footwear.geometry} material={materials.Wolf3D_Outfit_Footwear} skeleton={nodes.Wolf3D_Outfit_Footwear.skeleton} />
      {/* @ts-ignore */}
      <skinnedMesh geometry={nodes.Wolf3D_Outfit_Top.geometry} material={materials.Wolf3D_Outfit_Top} skeleton={nodes.Wolf3D_Outfit_Top.skeleton} />
      {/* @ts-ignore */}
      <skinnedMesh geometry={nodes.Wolf3D_Hair.geometry} material={materials.Wolf3D_Hair} skeleton={nodes.Wolf3D_Hair.skeleton} />
      {/* @ts-ignore */}
      <skinnedMesh name="EyeLeft" geometry={nodes.EyeLeft.geometry} material={materials.Wolf3D_Eye} skeleton={nodes.EyeLeft.skeleton} morphTargetDictionary={nodes.EyeLeft.morphTargetDictionary} morphTargetInfluences={nodes.EyeLeft.morphTargetInfluences} />
      {/* @ts-ignore */}
      <skinnedMesh name="EyeRight" geometry={nodes.EyeRight.geometry} material={materials.Wolf3D_Eye} skeleton={nodes.EyeRight.skeleton} morphTargetDictionary={nodes.EyeRight.morphTargetDictionary} morphTargetInfluences={nodes.EyeRight.morphTargetInfluences} />
      {/* @ts-ignore */}
      <skinnedMesh name="Wolf3D_Head" geometry={nodes.Wolf3D_Head.geometry} material={materials.Wolf3D_Skin} skeleton={nodes.Wolf3D_Head.skeleton} morphTargetDictionary={nodes.Wolf3D_Head.morphTargetDictionary} morphTargetInfluences={nodes.Wolf3D_Head.morphTargetInfluences} />
      {/* @ts-ignore */}
      <skinnedMesh name="Wolf3D_Teeth" geometry={nodes.Wolf3D_Teeth.geometry} material={materials.Wolf3D_Teeth} skeleton={nodes.Wolf3D_Teeth.skeleton} morphTargetDictionary={nodes.Wolf3D_Teeth.morphTargetDictionary} morphTargetInfluences={nodes.Wolf3D_Teeth.morphTargetInfluences} />
    </group>
  )
}

export const AiAssistant = memo(AiAssistantComponent);
