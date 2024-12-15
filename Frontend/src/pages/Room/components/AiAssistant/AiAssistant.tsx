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
        // @ts-expect-error does not exist on type

        nodes.Wolf3D_Head.morphTargetInfluences[
          // @ts-expect-error does not exist on type

          nodes.Wolf3D_Head.morphTargetDictionary[value]
        ] = 0;
        // @ts-expect-error does not exist on type

        nodes.Wolf3D_Teeth.morphTargetInfluences[
          // @ts-expect-error does not exist on type

          nodes.Wolf3D_Teeth.morphTargetDictionary[value]
        ] = 0;
      } else {
        // @ts-expect-error does not exist on type

        nodes.Wolf3D_Head.morphTargetInfluences[
          // @ts-expect-error does not exist on type

          nodes.Wolf3D_Head.morphTargetDictionary[value]
        ] = MathUtils.lerp(
          // @ts-expect-error does not exist on type

          nodes.Wolf3D_Head.morphTargetInfluences[
            // @ts-expect-error does not exist on type

            nodes.Wolf3D_Head.morphTargetDictionary[value]
          ],
          0,
          // @ts-expect-error does not exist on type

          morphTargetSmoothing,
        );

        // @ts-expect-error does not exist on type

        nodes.Wolf3D_Teeth.morphTargetInfluences[
          // @ts-expect-error does not exist on type

          nodes.Wolf3D_Teeth.morphTargetDictionary[value]
        ] = MathUtils.lerp(
          // @ts-expect-error does not exist on type

          nodes.Wolf3D_Teeth.morphTargetInfluences[
            // @ts-expect-error does not exist on type

            nodes.Wolf3D_Teeth.morphTargetDictionary[value]
          ],
          0,
          // @ts-expect-error does not exist on type

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
          // @ts-expect-error does not exist on type

          nodes.Wolf3D_Head.morphTargetInfluences[
            // @ts-expect-error does not exist on type

            nodes.Wolf3D_Head.morphTargetDictionary[
              // @ts-expect-error does not exist on type

              corresponding[mouthCue.value]
            ]
          ] = 1;
          // @ts-expect-error does not exist on type

          nodes.Wolf3D_Teeth.morphTargetInfluences[
            // @ts-expect-error does not exist on type

            nodes.Wolf3D_Teeth.morphTargetDictionary[
              // @ts-expect-error does not exist on type

              corresponding[mouthCue.value]
            ]
          ] = 1;
        } else {
          // @ts-expect-error does not exist on type

          nodes.Wolf3D_Head.morphTargetInfluences[
            // @ts-expect-error does not exist on type

            nodes.Wolf3D_Head.morphTargetDictionary[
              // @ts-expect-error does not exist on type

              corresponding[mouthCue.value]
            ]
          ] = MathUtils.lerp(
            // @ts-expect-error does not exist on type

            nodes.Wolf3D_Head.morphTargetInfluences[
              // @ts-expect-error does not exist on type

              nodes.Wolf3D_Head.morphTargetDictionary[
                // @ts-expect-error does not exist on type

                corresponding[mouthCue.value]
              ]
            ],
            1,
            // @ts-expect-error does not exist on type

            morphTargetSmoothing,
          );
          // @ts-expect-error does not exist on type

          nodes.Wolf3D_Teeth.morphTargetInfluences[
            // @ts-expect-error does not exist on type

            nodes.Wolf3D_Teeth.morphTargetDictionary[
              // @ts-expect-error does not exist on type

              corresponding[mouthCue.value]
            ]
          ] = MathUtils.lerp(
            // @ts-expect-error does not exist on type

            nodes.Wolf3D_Teeth.morphTargetInfluences[
              // @ts-expect-error does not exist on type

              nodes.Wolf3D_Teeth.morphTargetDictionary[
                // @ts-expect-error does not exist on type

                corresponding[mouthCue.value]
              ]
            ],
            1,
            // @ts-expect-error does not exist on type
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
    // eslint-disable-next-line react/no-unknown-property
    <group {...props} ref={group} dispose={null}>
      {/* eslint-disable-next-line react/no-unknown-property */}
      <primitive object={nodes.Hips} />
      <skinnedMesh
        // @ts-expect-error does not exist on type
        // eslint-disable-next-line react/no-unknown-property
        geometry={nodes.Wolf3D_Body.geometry}
        // eslint-disable-next-line react/no-unknown-property
        material={materials.Wolf3D_Body}
        // @ts-expect-error does not exist on type
        // eslint-disable-next-line react/no-unknown-property
        skeleton={nodes.Wolf3D_Body.skeleton}
      />
      <skinnedMesh
        // @ts-expect-error does not exist on type
        // eslint-disable-next-line react/no-unknown-property
        geometry={nodes.Wolf3D_Outfit_Bottom.geometry}
        // eslint-disable-next-line react/no-unknown-property
        material={materials.Wolf3D_Outfit_Bottom}
        // @ts-expect-error does not exist on type
        // eslint-disable-next-line react/no-unknown-property
        skeleton={nodes.Wolf3D_Outfit_Bottom.skeleton}
      />
      <skinnedMesh
        // @ts-expect-error does not exist on type
        // eslint-disable-next-line react/no-unknown-property
        geometry={nodes.Wolf3D_Outfit_Footwear.geometry}
        // eslint-disable-next-line react/no-unknown-property
        material={materials.Wolf3D_Outfit_Footwear}
        // @ts-expect-error does not exist on type
        // eslint-disable-next-line react/no-unknown-property
        skeleton={nodes.Wolf3D_Outfit_Footwear.skeleton}
      />
      <skinnedMesh
        // @ts-expect-error does not exist on type
        // eslint-disable-next-line react/no-unknown-property
        geometry={nodes.Wolf3D_Outfit_Top.geometry}
        // eslint-disable-next-line react/no-unknown-property
        material={materials.Wolf3D_Outfit_Top}
        // @ts-expect-error does not exist on type
        // eslint-disable-next-line react/no-unknown-property
        skeleton={nodes.Wolf3D_Outfit_Top.skeleton}
      />
      <skinnedMesh
        // @ts-expect-error does not exist on type
        // eslint-disable-next-line react/no-unknown-property
        geometry={nodes.Wolf3D_Hair.geometry}
        // eslint-disable-next-line react/no-unknown-property
        material={materials.Wolf3D_Hair}
        // @ts-expect-error does not exist on type
        // eslint-disable-next-line react/no-unknown-property
        skeleton={nodes.Wolf3D_Hair.skeleton}
      />
      <skinnedMesh
        name="EyeLeft"
        // @ts-expect-error does not exist on type
        // eslint-disable-next-line react/no-unknown-property
        geometry={nodes.EyeLeft.geometry}
        // eslint-disable-next-line react/no-unknown-property
        material={materials.Wolf3D_Eye}
        // @ts-expect-error does not exist on type
        // eslint-disable-next-line react/no-unknown-property
        skeleton={nodes.EyeLeft.skeleton}
        // @ts-expect-error does not exist on type
        // eslint-disable-next-line react/no-unknown-property
        morphTargetDictionary={nodes.EyeLeft.morphTargetDictionary}
        // @ts-expect-error does not exist on type
        // eslint-disable-next-line react/no-unknown-property
        morphTargetInfluences={nodes.EyeLeft.morphTargetInfluences}
      />
      <skinnedMesh
        name="EyeRight"
        // @ts-expect-error does not exist on type
        // eslint-disable-next-line react/no-unknown-property
        geometry={nodes.EyeRight.geometry}
        // eslint-disable-next-line react/no-unknown-property
        material={materials.Wolf3D_Eye}
        // @ts-expect-error does not exist on type
        // eslint-disable-next-line react/no-unknown-property
        skeleton={nodes.EyeRight.skeleton}
        // @ts-expect-error does not exist on type
        // eslint-disable-next-line react/no-unknown-property
        morphTargetDictionary={nodes.EyeRight.morphTargetDictionary}
        // @ts-expect-error does not exist on type
        // eslint-disable-next-line react/no-unknown-property
        morphTargetInfluences={nodes.EyeRight.morphTargetInfluences}
      />
      <skinnedMesh
        name="Wolf3D_Head"
        // @ts-expect-error does not exist on type
        // eslint-disable-next-line react/no-unknown-property
        geometry={nodes.Wolf3D_Head.geometry}
        // eslint-disable-next-line react/no-unknown-property
        material={materials.Wolf3D_Skin}
        // @ts-expect-error does not exist on type
        // eslint-disable-next-line react/no-unknown-property
        skeleton={nodes.Wolf3D_Head.skeleton}
        // @ts-expect-error does not exist on type
        // eslint-disable-next-line react/no-unknown-property
        morphTargetDictionary={nodes.Wolf3D_Head.morphTargetDictionary}
        // @ts-expect-error does not exist on type
        // eslint-disable-next-line react/no-unknown-property
        morphTargetInfluences={nodes.Wolf3D_Head.morphTargetInfluences}
      />
      <skinnedMesh
        name="Wolf3D_Teeth"
        // @ts-expect-error does not exist on type
        // eslint-disable-next-line react/no-unknown-property
        geometry={nodes.Wolf3D_Teeth.geometry}
        // eslint-disable-next-line react/no-unknown-property
        material={materials.Wolf3D_Teeth}
        // @ts-expect-error does not exist on type
        // eslint-disable-next-line react/no-unknown-property
        skeleton={nodes.Wolf3D_Teeth.skeleton}
        // @ts-expect-error does not exist on type
        // eslint-disable-next-line react/no-unknown-property
        morphTargetDictionary={nodes.Wolf3D_Teeth.morphTargetDictionary}
        // @ts-expect-error does not exist on type
        // eslint-disable-next-line react/no-unknown-property
        morphTargetInfluences={nodes.Wolf3D_Teeth.morphTargetInfluences}
      />
    </group>
  );
};

export const AiAssistant = memo(AiAssistantComponent);
