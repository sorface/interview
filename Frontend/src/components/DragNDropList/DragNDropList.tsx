import { ReactElement, useRef, useState } from 'react';

export interface DragNDropListItem {
  id: string;
  value: string;
  order: number;
}

interface DragNDropListProps<T extends DragNDropListItem> {
  items: T[];
  renderItem: (item: T, index: number) => ReactElement;
  onItemsChange: (items: T[]) => void;
}

export const DragNDropList = <T extends DragNDropListItem>({
  items,
  renderItem,
  onItemsChange,
}: DragNDropListProps<T>) => {
  const [draggingItem, setDraggingItem] = useState<T | null>(null);
  const containerRef = useRef<HTMLDivElement | null>(null);
  const previewContainerRef = useRef<HTMLDivElement | null>(null);
  const previewElRef = useRef<HTMLDivElement | null>(null);

  const handleDragStart = (e: React.DragEvent<HTMLDivElement>, item: T) => {
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

  const dropItem = (targetItem: T) => {
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

  const handleDragOver = (e: React.DragEvent<HTMLDivElement>, targetItem: T) => {
    e.preventDefault();
    if (!draggingItem || targetItem === draggingItem) {
      return;
    }
    dropItem(targetItem);
  };

  const handleDrop = (_: React.DragEvent<HTMLDivElement>, targetItem: T) => {
    dropItem(targetItem);
  };

  return (
    <div ref={containerRef}>
      <div ref={previewContainerRef} style={{ position: 'absolute', left: '-9999px', top: '-9999px' }}></div>
      {items.map((item, index) => (
        <div
          key={item.id}
          draggable="true"
          className={`cursor-move ${item === draggingItem ? 'opacity-0' : ''}`}
          onDragStart={(e) =>
            handleDragStart(e, item)}
          onDragEnd={handleDragEnd}
          onDragOver={(e) => handleDragOver(e, item)}
          onDrop={(e) => handleDrop(e, item)}
        >
          {renderItem(item, index)}
        </div>
      ))}
    </div>
  );
};
