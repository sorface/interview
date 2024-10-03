import { useEffect, useState } from 'react';
import { SelfieSegmentation, Results } from '@mediapipe/selfie_segmentation';

export const useSelfieSegmentation = (enabled: boolean, onResults: (results: Results) => void) => {
  const [selfieSegmentation, setSelfieSegmentation] = useState<SelfieSegmentation | null>(null);

  useEffect(() => {
    if (selfieSegmentation || !enabled) {
      return;
    }
    const newSelfieSegmentation = new SelfieSegmentation({
      locateFile: (file) => {
        return `https://cdn.jsdelivr.net/npm/@mediapipe/selfie_segmentation@0.1/${file}`;
      }
    });
    newSelfieSegmentation.setOptions({ modelSelection: 1, selfieMode: true });
    newSelfieSegmentation.onResults(onResults);
    setSelfieSegmentation(newSelfieSegmentation);

    return () => {
      newSelfieSegmentation.close();
    };
  }, [onResults, enabled, selfieSegmentation]);

  return selfieSegmentation;
};
