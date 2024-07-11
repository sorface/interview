export interface IonIconProps {
  name: string;
  size?: string;
}

declare global {
  namespace JSX {
    interface IntrinsicElements {
      ['ion-icon']: IonIconProps;
    }
  }
}
