import { FunctionComponent, ReactElement } from 'react';

import './ModalFooter.css';
import { Gap } from '../Gap/Gap';

interface ModalFooterProps {
  children: ReactElement | ReactElement[];
}

export const ModalFooter: FunctionComponent<ModalFooterProps> = ({
  children,
}) => {
  return (
    <div className='modal-footer bottom-0 sticky'>
      <Gap sizeRem={1.25} />
      <div className='flex justify-end modal-footer-content'>
        {children}
      </div>
      <Gap sizeRem={1.25} />
    </div>
  )
};
