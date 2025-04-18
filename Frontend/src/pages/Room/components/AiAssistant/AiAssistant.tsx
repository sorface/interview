import React, {
  FunctionComponent,
  memo,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useRef,
  useState,
} from 'react';
import { GroupProps, useFrame } from '@react-three/fiber';
import {
  BufferGeometry,
  Material,
  Mesh,
  NormalBufferAttributes,
  Object3DEventMap,
} from 'three';
import { LocalizationContext } from '../../../../context/LocalizationContext';
import { Theme, ThemeContext } from '../../../../context/ThemeContext';

const vertexshader = `
  uniform float u_time;

  vec3 mod289(vec3 x)
  {
    return x - floor(x * (1.0 / 289.0)) * 289.0;
  }
      
  vec4 mod289(vec4 x)
  {
    return x - floor(x * (1.0 / 289.0)) * 289.0;
  }

  vec4 permute(vec4 x)
  {
    return mod289(((x*34.0)+10.0)*x);
  }
  
  vec4 taylorInvSqrt(vec4 r)
  {
    return 1.79284291400159 - 0.85373472095314 * r;
  }
  
  vec3 fade(vec3 t) {
    return t*t*t*(t*(t*6.0-15.0)+10.0);
  }

  // Classic Perlin noise, periodic variant
  float pnoise(vec3 P, vec3 rep)
  {
    vec3 Pi0 = mod(floor(P), rep); // Integer part, modulo period
    vec3 Pi1 = mod(Pi0 + vec3(1.0), rep); // Integer part + 1, mod period
    Pi0 = mod289(Pi0);
    Pi1 = mod289(Pi1);
    vec3 Pf0 = fract(P); // Fractional part for interpolation
    vec3 Pf1 = Pf0 - vec3(1.0); // Fractional part - 1.0
    vec4 ix = vec4(Pi0.x, Pi1.x, Pi0.x, Pi1.x);
    vec4 iy = vec4(Pi0.yy, Pi1.yy);
    vec4 iz0 = Pi0.zzzz;
    vec4 iz1 = Pi1.zzzz;

    vec4 ixy = permute(permute(ix) + iy);
    vec4 ixy0 = permute(ixy + iz0);
    vec4 ixy1 = permute(ixy + iz1);

    vec4 gx0 = ixy0 * (1.0 / 7.0);
    vec4 gy0 = fract(floor(gx0) * (1.0 / 7.0)) - 0.5;
    gx0 = fract(gx0);
    vec4 gz0 = vec4(0.5) - abs(gx0) - abs(gy0);
    vec4 sz0 = step(gz0, vec4(0.0));
    gx0 -= sz0 * (step(0.0, gx0) - 0.5);
    gy0 -= sz0 * (step(0.0, gy0) - 0.5);

    vec4 gx1 = ixy1 * (1.0 / 7.0);
    vec4 gy1 = fract(floor(gx1) * (1.0 / 7.0)) - 0.5;
    gx1 = fract(gx1);
    vec4 gz1 = vec4(0.5) - abs(gx1) - abs(gy1);
    vec4 sz1 = step(gz1, vec4(0.0));
    gx1 -= sz1 * (step(0.0, gx1) - 0.5);
    gy1 -= sz1 * (step(0.0, gy1) - 0.5);

    vec3 g000 = vec3(gx0.x,gy0.x,gz0.x);
    vec3 g100 = vec3(gx0.y,gy0.y,gz0.y);
    vec3 g010 = vec3(gx0.z,gy0.z,gz0.z);
    vec3 g110 = vec3(gx0.w,gy0.w,gz0.w);
    vec3 g001 = vec3(gx1.x,gy1.x,gz1.x);
    vec3 g101 = vec3(gx1.y,gy1.y,gz1.y);
    vec3 g011 = vec3(gx1.z,gy1.z,gz1.z);
    vec3 g111 = vec3(gx1.w,gy1.w,gz1.w);

    vec4 norm0 = taylorInvSqrt(vec4(dot(g000, g000), dot(g010, g010), dot(g100, g100), dot(g110, g110)));
    g000 *= norm0.x;
    g010 *= norm0.y;
    g100 *= norm0.z;
    g110 *= norm0.w;
    vec4 norm1 = taylorInvSqrt(vec4(dot(g001, g001), dot(g011, g011), dot(g101, g101), dot(g111, g111)));
    g001 *= norm1.x;
    g011 *= norm1.y;
    g101 *= norm1.z;
    g111 *= norm1.w;

    float n000 = dot(g000, Pf0);
    float n100 = dot(g100, vec3(Pf1.x, Pf0.yz));
    float n010 = dot(g010, vec3(Pf0.x, Pf1.y, Pf0.z));
    float n110 = dot(g110, vec3(Pf1.xy, Pf0.z));
    float n001 = dot(g001, vec3(Pf0.xy, Pf1.z));
    float n101 = dot(g101, vec3(Pf1.x, Pf0.y, Pf1.z));
    float n011 = dot(g011, vec3(Pf0.x, Pf1.yz));
    float n111 = dot(g111, Pf1);

    vec3 fade_xyz = fade(Pf0);
    vec4 n_z = mix(vec4(n000, n100, n010, n110), vec4(n001, n101, n011, n111), fade_xyz.z);
    vec2 n_yz = mix(n_z.xy, n_z.zw, fade_xyz.y);
    float n_xyz = mix(n_yz.x, n_yz.y, fade_xyz.x); 
    return 2.2 * n_xyz;
  }

  uniform float u_frequency;

  void main() {
      float noise = 3.0 * pnoise(position + u_time, vec3(10.0));
      float displacement = (u_frequency / 30.) * (noise / 10.);
      vec3 newPosition = position + normal * displacement;
      gl_Position = projectionMatrix * modelViewMatrix * vec4(newPosition, 1.0);
  }
`;

const fragmentshader = `
  uniform float u_red;
  uniform float u_blue;
  uniform float u_green;
  void main() {
      gl_FragColor = vec4(vec3(u_red, u_green, u_blue), 1. );
  }
`;

export enum AiAssistantScriptName {
  Idle = 'idle',
  Welcome = 'welcome',
  PleaseAnswer = 'please-answer',
  GoodAnswer = 'good-answer',
  NeedTrain = 'need-train',
  Login = 'login',
}

export enum AiAssistantLoadingVariant {
  Normal = 10,
  Wide = 30,
}

interface AiAssistantProps {
  visible?: boolean;
  loading?: boolean;
  loadingVariant?: AiAssistantLoadingVariant;
  trackMouse?: boolean;
  currentScript: AiAssistantScriptName;
}

const AiAssistantComponent: FunctionComponent<
  GroupProps & AiAssistantProps
> = ({
  visible = true,
  loading,
  loadingVariant = AiAssistantLoadingVariant.Normal,
  trackMouse,
  currentScript,
}) => {
  const { lang } = useContext(LocalizationContext);
  const { themeInUi } = useContext(ThemeContext);
  const meshRef = useRef<Mesh<
    BufferGeometry<NormalBufferAttributes>,
    Material | Material[],
    Object3DEventMap
  > | null>(null);
  const [analyser, setAnalyser] = useState<{
    node: AnalyserNode;
    context: AudioContext;
  } | null>(null);
  const analyserDataArrayRef = useRef(new Uint8Array(0));
  const uniformsRef = useRef({
    u_time: { type: 'f', value: 10.0 },
    u_frequency: { type: 'f', value: 0.0 },
    u_red: { type: 'f', value: 0.0 },
    u_green: { type: 'f', value: 0.0 },
    u_blue: { type: 'f', value: 0.0 },
  });
  const audios = useMemo(() => {
    const result: Record<AiAssistantScriptName, HTMLAudioElement> = {
      [AiAssistantScriptName.Idle]: new Audio(),
      [AiAssistantScriptName.Welcome]: new Audio(
        `/audios/${lang}/${AiAssistantScriptName.Welcome}.mp3`,
      ),
      [AiAssistantScriptName.PleaseAnswer]: new Audio(
        `/audios/${lang}/${AiAssistantScriptName.PleaseAnswer}.mp3`,
      ),
      [AiAssistantScriptName.GoodAnswer]: new Audio(
        `/audios/${lang}/${AiAssistantScriptName.GoodAnswer}.mp3`,
      ),
      [AiAssistantScriptName.NeedTrain]: new Audio(
        `/audios/${lang}/${AiAssistantScriptName.NeedTrain}.mp3`,
      ),
      [AiAssistantScriptName.Login]: new Audio(
        `/audios/${lang}/${AiAssistantScriptName.Login}.mp3`,
      ),
    };
    return result;
  }, [lang]);

  useEffect(() => {
    const color =
      themeInUi === Theme.Dark
        ? [6 / 255, 102 / 255, 226 / 255]
        : [6 / 255, 102 / 255, 225 / 255];
    uniformsRef.current.u_red.value = color[0];
    uniformsRef.current.u_green.value = color[1];
    uniformsRef.current.u_blue.value = color[2];
  }, [themeInUi]);

  const initAnalyser = useCallback(() => {
    const context = new AudioContext();
    const analyser = context.createAnalyser();

    analyser.connect(context.destination);

    analyser.fftSize = 256;

    const bufferLength = analyser.frequencyBinCount;
    analyserDataArrayRef.current = new Uint8Array(bufferLength);
    setAnalyser({ node: analyser, context });
  }, []);

  useEffect(() => {
    if (analyser || currentScript === AiAssistantScriptName.Idle) {
      return;
    }
    initAnalyser();
  }, [analyser, currentScript, initAnalyser]);

  const audioSources = useMemo(() => {
    if (!analyser) {
      return null;
    }
    const result: Record<AiAssistantScriptName, MediaElementAudioSourceNode> = {
      [AiAssistantScriptName.Idle]: analyser.context.createMediaElementSource(
        audios[AiAssistantScriptName.Idle],
      ),
      [AiAssistantScriptName.Welcome]:
        analyser.context.createMediaElementSource(
          audios[AiAssistantScriptName.Welcome],
        ),
      [AiAssistantScriptName.PleaseAnswer]:
        analyser.context.createMediaElementSource(
          audios[AiAssistantScriptName.PleaseAnswer],
        ),
      [AiAssistantScriptName.GoodAnswer]:
        analyser.context.createMediaElementSource(
          audios[AiAssistantScriptName.GoodAnswer],
        ),
      [AiAssistantScriptName.NeedTrain]:
        analyser.context.createMediaElementSource(
          audios[AiAssistantScriptName.NeedTrain],
        ),
      [AiAssistantScriptName.Login]: analyser.context.createMediaElementSource(
        audios[AiAssistantScriptName.Login],
      ),
    };
    return result;
  }, [analyser, audios]);

  useEffect(() => {
    if (
      !analyser ||
      !audioSources ||
      currentScript === AiAssistantScriptName.Idle
    ) {
      return;
    }
    audioSources[currentScript].connect(analyser.node);
    return () => {
      audioSources[currentScript].disconnect(analyser.node);
    };
  }, [currentScript, analyser, audioSources]);

  useEffect(() => {
    if (currentScript === AiAssistantScriptName.Idle) {
      return;
    }
    audios[currentScript].play();
  }, [currentScript, audios]);

  useFrame((_, delta) => {
    uniformsRef.current.u_time.value += delta;
    if (analyser) {
      const dataArr = analyserDataArrayRef.current;
      analyser.node.getByteFrequencyData(dataArr);
      let avg = 0;
      for (let i = 0; i < dataArr.length; i++) {
        avg += dataArr[i];
      }
      avg /= dataArr.length;

      uniformsRef.current.u_frequency.value = avg;
      if (avg !== 0) {
        return;
      }
    }
    if (meshRef.current) {
      if (!trackMouse) {
        meshRef.current.rotation.y += delta * 0.025;
        meshRef.current.rotation.x += delta * 0.025;
      }
      if (loading) {
        uniformsRef.current.u_frequency.value =
          loadingVariant + Math.sin(performance.now() / 20000) + 1;
      }
    }
  });

  useEffect(() => {
    if (!trackMouse) {
      return;
    }
    const mousemoveHandler = (e: MouseEvent) => {
      if (!meshRef.current) {
        return;
      }
      const posX = e.clientX / window.innerWidth - 0.5;
      const posY = e.clientY / window.innerHeight - 0.5;
      meshRef.current.rotation.y = -posX * 1.2;
      meshRef.current.rotation.x = -posY * 1.2;
    };

    document.body.addEventListener('mousemove', mousemoveHandler, {
      passive: true,
    });

    return () => {
      document.body.removeEventListener('mousemove', mousemoveHandler);
    };
  }, [trackMouse]);

  if (!visible) {
    return <></>;
  }

  return (
    <mesh
      ref={meshRef}
      // eslint-disable-next-line react/no-unknown-property
      position={[0, -0.5, -6]}
    >
      {/* eslint-disable-next-line react/no-unknown-property */}
      <icosahedronGeometry args={[2, 2]} />
      <shaderMaterial
        // eslint-disable-next-line react/no-unknown-property
        wireframe
        // eslint-disable-next-line react/no-unknown-property
        uniforms={uniformsRef.current}
        // eslint-disable-next-line react/no-unknown-property
        vertexShader={vertexshader}
        // eslint-disable-next-line react/no-unknown-property
        fragmentShader={fragmentshader}
      />
    </mesh>
  );
};

export const AiAssistant = memo(AiAssistantComponent);
