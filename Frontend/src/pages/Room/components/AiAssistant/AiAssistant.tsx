import React, {
  FunctionComponent,
  memo,
  useContext,
  useEffect,
  useMemo,
  useRef,
} from 'react';
import { useAnimations, useFBX, useGLTF } from '@react-three/drei';
import { GroupProps, useFrame, useLoader } from '@react-three/fiber';
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

const AiAssistantComponent: FunctionComponent<GroupProps & HrAvatarProps> = (
  props,
) => {
  const { lang } = useContext(LocalizationContext);
  const { nodes, materials } = useGLTF(modelUrl);
  const { animations: idleAnimation } = useFBX(animationUrl);
  const animation = 'Idle';
  const script = 'hi-all';
  const audio = useMemo(
    () => new Audio(`/audios/${lang}/${script}.mp3`),
    [script, lang],
  );
  const jsonFile = useLoader(FileLoader, `/audios/${lang}/${script}.json`);
  const lipsync = JSON.parse(String(jsonFile));

  idleAnimation[0].name = animation;

  const group = useRef<Group<Object3DEventMap> | null>(null);
  const { actions } = useAnimations([idleAnimation[0]], group);

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
          morphTargetSmoothing,
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
          morphTargetSmoothing,
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
            morphTargetSmoothing,
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
            morphTargetSmoothing,
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
      <skinnedMesh
        // @ts-ignore
        geometry={nodes.Wolf3D_Body.geometry}
        material={materials.Wolf3D_Body}
        // @ts-ignore
        skeleton={nodes.Wolf3D_Body.skeleton}
      />
      <skinnedMesh
        // @ts-ignore
        geometry={nodes.Wolf3D_Outfit_Bottom.geometry}
        // @ts-ignore
        material={materials.Wolf3D_Outfit_Bottom}
        // @ts-ignore
        skeleton={nodes.Wolf3D_Outfit_Bottom.skeleton}
      />
      <skinnedMesh
        // @ts-ignore
        geometry={nodes.Wolf3D_Outfit_Footwear.geometry}
        material={materials.Wolf3D_Outfit_Footwear}
        // @ts-ignore
        skeleton={nodes.Wolf3D_Outfit_Footwear.skeleton}
      />
      <skinnedMesh
        // @ts-ignore
        geometry={nodes.Wolf3D_Outfit_Top.geometry}
        material={materials.Wolf3D_Outfit_Top}
        // @ts-ignore
        skeleton={nodes.Wolf3D_Outfit_Top.skeleton}
      />
      <skinnedMesh
        // @ts-ignore
        geometry={nodes.Wolf3D_Hair.geometry}
        material={materials.Wolf3D_Hair}
        // @ts-ignore
        skeleton={nodes.Wolf3D_Hair.skeleton}
      />
      <skinnedMesh
        name="EyeLeft"
        // @ts-ignore
        geometry={nodes.EyeLeft.geometry}
        material={materials.Wolf3D_Eye}
        // @ts-ignore
        skeleton={nodes.EyeLeft.skeleton}
        // @ts-ignore
        morphTargetDictionary={nodes.EyeLeft.morphTargetDictionary}
        // @ts-ignore
        morphTargetInfluences={nodes.EyeLeft.morphTargetInfluences}
      />
      <skinnedMesh
        name="EyeRight"
        // @ts-ignore
        geometry={nodes.EyeRight.geometry}
        material={materials.Wolf3D_Eye}
        // @ts-ignore
        skeleton={nodes.EyeRight.skeleton}
        // @ts-ignore
        morphTargetDictionary={nodes.EyeRight.morphTargetDictionary}
        // @ts-ignore
        morphTargetInfluences={nodes.EyeRight.morphTargetInfluences}
      />
      <skinnedMesh
        name="Wolf3D_Head"
        // @ts-ignore
        geometry={nodes.Wolf3D_Head.geometry}
        material={materials.Wolf3D_Skin}
        // @ts-ignore
        skeleton={nodes.Wolf3D_Head.skeleton}
        // @ts-ignore
        morphTargetDictionary={nodes.Wolf3D_Head.morphTargetDictionary}
        // @ts-ignore
        morphTargetInfluences={nodes.Wolf3D_Head.morphTargetInfluences}
      />
      <skinnedMesh
        name="Wolf3D_Teeth"
        // @ts-ignore
        geometry={nodes.Wolf3D_Teeth.geometry}
        material={materials.Wolf3D_Teeth}
        // @ts-ignore
        skeleton={nodes.Wolf3D_Teeth.skeleton}
        // @ts-ignore
        morphTargetDictionary={nodes.Wolf3D_Teeth.morphTargetDictionary}
        // @ts-ignore
        morphTargetInfluences={nodes.Wolf3D_Teeth.morphTargetInfluences}
      />
    </group>
  );
};

export const AiAssistant = memo(AiAssistantComponent);
