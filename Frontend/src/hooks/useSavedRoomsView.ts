import { useEffect, useState } from 'react';

export enum RoomsView {
  Grid = 'grid',
  List = 'list',
}

const defaultView = RoomsView.Grid;

const localStorageKey = 'roomsView';

const readFromStorage = () => localStorage.getItem(localStorageKey);

const saveToStorage = (roomsView: RoomsView) =>
  localStorage.setItem(localStorageKey, String(roomsView));

const validateTheme = (roomsView: string | null) => {
  if (roomsView && Object.values(RoomsView).includes(roomsView as RoomsView)) {
    return roomsView as RoomsView;
  }
  return null;
};

const getSavedRoomsView = (): RoomsView => {
  const roomsViewFromStorage = readFromStorage();
  const validRoomsView = validateTheme(roomsViewFromStorage);
  if (validRoomsView) {
    return validRoomsView;
  }

  return defaultView;
};

export const useSavedRoomsView = () => {
  const [view, setView] = useState<RoomsView>(getSavedRoomsView());

  useEffect(() => {
    saveToStorage(view);
  }, [view]);

  return { view, setView };
};
