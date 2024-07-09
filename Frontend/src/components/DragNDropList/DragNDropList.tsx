import { FunctionComponent, useRef, useState } from 'react';

import './DragNDropList.css';

export interface DragNDropListItem {
  id: string;
  value: string;
  order: number;
}

interface DragNDropListProps {
  items: DragNDropListItem[];
  onItemsChange: (items: DragNDropListItem[]) => void;
}

export const DragNDropList: FunctionComponent<DragNDropListProps> = ({
  items,
  onItemsChange,
}) => {
  const [draggingItem, setDraggingItem] = useState<DragNDropListItem | null>(null);
  const containerRef = useRef<HTMLDivElement | null>(null);
  const previewContainerRef = useRef<HTMLDivElement | null>(null);
  const previewElRef = useRef<HTMLDivElement | null>(null);

  const handleDragStart = (e: React.DragEvent<HTMLDivElement>, item: DragNDropListItem) => {
    setDraggingItem(item);
    e.dataTransfer.setData('text/plain', '');
    const crt = (e.target as HTMLDivElement).cloneNode(true) as HTMLDivElement;
    if (containerRef.current) {
      crt.style.width = `${containerRef.current.offsetWidth}px`;
    }
    if (previewContainerRef.current) {
      previewElRef.current = crt;
      previewContainerRef.current.appendChild(crt);
    }
    e.dataTransfer.setDragImage(crt, e.nativeEvent.offsetX, e.nativeEvent.offsetY);
  };

  const handleDragEnd = () => {
    setDraggingItem(null);
    if (previewContainerRef.current && previewElRef.current) {
      previewContainerRef.current.removeChild(previewElRef.current);
    }
  };

  const dropItem = (targetItem: DragNDropListItem) => {
    if (!draggingItem) return;

    const currentIndex = items.indexOf(draggingItem);
    const targetIndex = items.indexOf(targetItem);

    if (currentIndex !== -1 && targetIndex !== -1) {
      const newItems = [...items];
      newItems.splice(currentIndex, 1);
      newItems.splice(targetIndex, 0, draggingItem);
      onItemsChange(newItems);
    }
  };

  const handleDragOver = (e: React.DragEvent<HTMLDivElement>, targetItem: DragNDropListItem) => {
    e.preventDefault();
    if (!draggingItem || targetItem === draggingItem) {
      return;
    }
    dropItem(targetItem);
  };

  const handleDrop = (_: React.DragEvent<HTMLDivElement>, targetItem: DragNDropListItem) => {
    dropItem(targetItem);
  };

  return (
    <div className="drag-n-drop-list" ref={containerRef}>
      <div ref={previewContainerRef} style={{ position: 'absolute', left: '-9999px', top: '-9999px' }}></div>
      {items.map(item => (
        <div
          key={item.id}
          draggable="true"
          className=
          {`list-item ${item === draggingItem ?
            'dragging' : ''
            }`}
          onDragStart={(e) =>
            handleDragStart(e, item)}
          onDragEnd={handleDragEnd}
          onDragOver={(e) => handleDragOver(e, item)}
          onDrop={(e) => handleDrop(e, item)}
        >
          <div className="list-item-details">
            <span>{item.value}</span>
          </div>
        </div>
      ))}
    </div>
  );
};
