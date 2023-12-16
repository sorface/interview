import { FunctionComponent, useCallback, useState } from 'react';
import Modal from 'react-modal';
import { Localization } from '../../localization';

import './ActionModal.css';

interface ActionModalProps {
  title: string;
  openButtonCaption: string;
  loading: boolean;
  loadingCaption: string;
  error: string | null;
  onAction: () => void;
}

export const ActionModal: FunctionComponent<ActionModalProps> = ({
  title,
  openButtonCaption,
  loading,
  error,
  loadingCaption,
  onAction,
}) => {
  const [modalOpen, setModalOpen] = useState(false);

  const handleOpenModal = useCallback(() => {
    setModalOpen(true);
  }, []);

  const handleCloseModal = useCallback(() => {
    setModalOpen(false);
  }, []);

  const onCallAction = useCallback(() => {
    handleCloseModal();
    onAction();
  }, [handleCloseModal, onAction]);

  if (loading) {
    return (<div>{loadingCaption}...</div>);
  }

  if (error) {
    return (<div>{Localization.Error}: {error}</div>);
  }

  return (
    <>
      <button
        onClick={handleOpenModal}
      >
        {openButtonCaption}
      </button>
      <Modal
        isOpen={modalOpen}
        contentLabel={Localization.CloseRoom}
        appElement={document.getElementById('root') || undefined}
        className="action-modal"
        onRequestClose={handleCloseModal}
        style={{
          overlay: {
            backgroundColor: 'rgba(0, 0, 0, 0.75)',
          },
        }}
      >
        <div className="action-modal-header">
          <h3>{title}</h3>
          <button onClick={handleCloseModal}>X</button>
        </div>
        <div className="action-modal-content">
          <button onClick={onCallAction}>{Localization.Yes}</button>
          <button onClick={handleCloseModal}>{Localization.No}</button>
        </div>
      </Modal>
    </>
  );
};
