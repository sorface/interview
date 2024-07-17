import { FunctionComponent } from 'react';
import { Link } from 'react-router-dom';
import { Button } from '../Button/Button';

import './ButtonLink.css';

export interface ButtonLinkPorps {
  path: string;
  caption: string;
}

export const ButtonLink: FunctionComponent<ButtonLinkPorps> = ({
  path,
  caption,
}) => {
  return (
    <Link to={path}>
      <Button className="button-link">{caption}</Button>
    </Link>
  )
};
