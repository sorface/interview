import { AnyObject } from '../../../types/anyObject';
import { uuid } from '../utils/utils';

export interface TreeControllerOptions {
  idPropertyName: string;
  titlePropertyName: string;
  createNewData: () => AnyObject;
}

const defaultIdPropName = 'id';
const defaultTitlePropName = 'title';
export const defaultTreeControllerOptions: TreeControllerOptions = {
  idPropertyName: defaultIdPropName,
  titlePropertyName: defaultTitlePropName,
  createNewData: () => ({
    [defaultIdPropName]: uuid(),
    [defaultTitlePropName]: '',
  }),
};
