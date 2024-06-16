import { FunctionComponent, ReactElement, memo } from 'react';
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
  if (error) {
    return (
      <Field>
        <div>{errorPrefix || <LocalizationCaption captionKey={LocalizationKey.Error} />}: {error}</div>
      </Field>
    );
  }

  if (loading) {
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
