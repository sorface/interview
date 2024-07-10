import { FunctionComponent, ReactNode } from 'react';
import ModalInternal from 'react-modal';

import './Modal.css';
import { ThemedIcon } from '../../pages/Room/components/ThemedIcon/ThemedIcon';
import { IconNames } from '../../constants';

interface ModalProps {
  open: boolean;
  contentLabel: string;
  onClose: () => void;
  children: ReactNode;
}

export const Modal: FunctionComponent<ModalProps> = ({
  open,
  contentLabel,
  children,
  onClose,
}) => {
  return (
    <ModalInternal
      isOpen={open}
      contentLabel={contentLabel}
      appElement={document.getElementById('root') || undefined}
      className="modal"
      onRequestClose={onClose}
      style={{
        overlay: {
          backgroundColor: 'var(--page-overlay-color)',
          zIndex: 999,
        },
      }}
    >
      <div className="modal-header">
        <h3>{contentLabel}</h3>
        <button className="modal-close" onClick={onClose}>
          <ThemedIcon name={IconNames.Close} />
        </button>
      </div>
      <div className="modal-content">
        {children}
      </div>
    </ModalInternal>
  );
};
