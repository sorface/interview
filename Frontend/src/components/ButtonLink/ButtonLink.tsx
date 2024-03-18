import { FunctionComponent } from 'react';
import { Link } from 'react-router-dom';

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
      <button className="button-link">{caption}</button>
    </Link>
  )
};
