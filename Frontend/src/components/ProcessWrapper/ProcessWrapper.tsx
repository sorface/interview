import { FunctionComponent, ReactElement, memo, useEffect, useState } from 'react';
import { Field } from '../FieldsBlock/Field';
import { Loader } from '../Loader/Loader';
import { LocalizationKey } from '../../localization';
import { LocalizationCaption } from '../LocalizationCaption/LocalizationCaption';

import './ProcessWrapper.css';

export const skeletonTransitionMs = 300;

export interface LoaderStyle {
  height?: string;
}

export interface ProcessWrapperProps {
  loading: boolean;
  loadingPrefix?: string;
  loaders?: LoaderStyle[];
  error: string | null;
  errorPrefix?: string;
  children: ReactElement<any, any> | null;
}

const ProcessWrapperComponent: FunctionComponent<ProcessWrapperProps> = ({
  loading,
  loadingPrefix,
  loaders,
  error,
  errorPrefix,
  children,
}) => {
  const [skeletonTransition, setSkeletonTransition] = useState(false);
  const [showLoading, setShowLoading] = useState(false);
  const [firstLoading, setFirstLoading] = useState(true);

  useEffect(() => {
    if (loading) {
      return;
    }
    const loaderWrapper = document.querySelectorAll('.process-wrapper-loader');
    loaderWrapper?.forEach(el => el.classList.add('fadeOut'));
    setFirstLoading(false);
    setSkeletonTransition(true);
  }, [loading]);

  useEffect(() => {
    if (!skeletonTransition) {
      return;
    }
    const transitionTimeout = setTimeout(() => {
      setSkeletonTransition(false);
    }, skeletonTransitionMs);

    return () => {
      clearTimeout(transitionTimeout);
    };
  }, [skeletonTransition]);

  useEffect(() => {
    if (!loading) {
      return;
    }
    setShowLoading(true);
  }, [loading]);

  useEffect(() => {
    if (skeletonTransition || firstLoading) {
      return;
    }
    setShowLoading(false);
  }, [skeletonTransition, firstLoading]);

  if (error) {
    return (
      <Field>
        <div>{errorPrefix || <LocalizationCaption captionKey={LocalizationKey.Error} />}: {error}</div>
      </Field>
    );
  }

  if (showLoading || loading) {
    return (
      <>
        {
          (loaders || [{}]).map((loader, index) => (
            <Field key={`loader${index}`}>
              <div
                style={{ height: loader.height || '1.8rem' }}
                className="process-wrapper-loader"
              >
                {!!loadingPrefix && index === 0 ? (
                  <div>{loadingPrefix}</div>
                ) : (
                  <Loader />
                )}
              </div>
            </Field>
          ))
        }
      </>
    );
  }

  return children;
};

export const ProcessWrapper = memo(ProcessWrapperComponent);
