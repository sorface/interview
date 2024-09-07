import { FunctionComponent, ReactNode } from 'react';
import ModalInternal from 'react-modal';
import { Button } from '../Button/Button';
import { IconNames } from '../../constants';
import { Icon } from '../../pages/Room/components/Icon/Icon';

import './Modal.css';

export interface ModalProps {
  open: boolean;
  contentLabel: string;
  wide?: boolean;
  onClose: () => void;
  onRequestClose?: () => void;
  children: ReactNode;
}

export const Modal: FunctionComponent<ModalProps> = ({
  open,
  contentLabel,
  wide,
  children,
  onClose,
  onRequestClose,
}) => {
  return (
    <ModalInternal
      isOpen={open}
      contentLabel={contentLabel}
      appElement={document.getElementById('root') || undefined}
      className={`modal ${wide ? 'modal-wide' : ''} flex flex-col`}
      onRequestClose={onRequestClose || onClose}
      style={{
        overlay: {
          backgroundColor: 'var(--page-overlay-color)',
          zIndex: 999,
        },
      }}
    >
      <div className="modal-header">
        <h3>{contentLabel}</h3>
        <Button className="modal-close" onClick={onClose}>
          <Icon secondary name={IconNames.Close} />
        </Button>
      </div>
      <div className="modal-content overflow-auto">
        {children}
      </div>
    </ModalInternal>
  );
};
