export interface IonIconProps {
  name: string;
}

declare global {
  namespace JSX {
    interface IntrinsicElements {
      ['ion-icon']: IonIconProps;
    }
  }
}
