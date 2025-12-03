import { DropResult } from "react-beautiful-dnd";
import { useCallback, useEffect } from "react";

interface DragDropHandlerProps {
  onBasicInfoOrderChange: (newOrder: string[]) => void;
  onCompanyInfoOrderChange: (newOrder: string[]) => void;
  basicInfoItemOrder: string[];
  companyInfoItemOrder: string[];
  basicInfoVisibleItems: string[];
  companyInfoVisibleItems: string[];
}

export const useDragDropHandler = ({
  onBasicInfoOrderChange,
  onCompanyInfoOrderChange,
  basicInfoItemOrder,
  companyInfoItemOrder,
  basicInfoVisibleItems,
  companyInfoVisibleItems,
}: DragDropHandlerProps) => {

  // Fix for iOS: Add/remove body class during drag
  useEffect(() => {
    const handleDragStart = () => {
      document.body.classList.add('dragging');
    };

    const handleDragEndCleanup = () => {
      document.body.classList.remove('dragging');
      // Force reflow to fix iOS transform issues
      document.body.offsetHeight;
    };

    // Listen for drag events
    window.addEventListener('dragstart', handleDragStart);
    window.addEventListener('dragend', handleDragEndCleanup);
    window.addEventListener('touchend', handleDragEndCleanup);

    return () => {
      window.removeEventListener('dragstart', handleDragStart);
      window.removeEventListener('dragend', handleDragEndCleanup);
      window.removeEventListener('touchend', handleDragEndCleanup);
      document.body.classList.remove('dragging');
    };
  }, []);

  const handleDragEnd = useCallback((result: DropResult) => {
    // Clean up dragging state
    document.body.classList.remove('dragging');

    // Force reflow for iOS
    if (typeof window !== 'undefined') {
      window.requestAnimationFrame(() => {
        document.body.offsetHeight;
      });
    }

    if (!result.destination) {
      return;
    }

    const { source, destination } = result;
    if (
      source.index === destination.index &&
      source.droppableId === destination.droppableId
    ) {
      return;
    }

    const droppableId = destination.droppableId;

    if (droppableId === "basicInfo-items") {
      const sourceItemKey = result.draggableId.replace("basicInfo-", "");
      
      const visibleItems = Array.from(basicInfoVisibleItems);
      
      const sourceVisibleIndex = visibleItems.indexOf(sourceItemKey);
      if (sourceVisibleIndex === -1) {
        return;
      }
      
      const [reorderedItem] = visibleItems.splice(sourceVisibleIndex, 1);
      visibleItems.splice(destination.index, 0, reorderedItem);
      
     
      const visibleSet = new Set(visibleItems);
      const nonVisibleItems = basicInfoItemOrder.filter(item => !visibleSet.has(item));
      const newOrder = [...visibleItems, ...nonVisibleItems];
      
      onBasicInfoOrderChange(newOrder);
    } else if (droppableId === "companyInfo-items") {
      const sourceItemKey = result.draggableId.replace("companyInfo-", "");
      
      const visibleItems = Array.from(companyInfoVisibleItems);
      
      const sourceVisibleIndex = visibleItems.indexOf(sourceItemKey);
      if (sourceVisibleIndex === -1) {  
        return;
      }
      
      const [reorderedItem] = visibleItems.splice(sourceVisibleIndex, 1);
      visibleItems.splice(destination.index, 0, reorderedItem);
      
      const visibleSet = new Set(visibleItems);
      const nonVisibleItems = companyInfoItemOrder.filter(item => !visibleSet.has(item));
      const newOrder = [...visibleItems, ...nonVisibleItems];

      onCompanyInfoOrderChange(newOrder);
    }

    // Additional cleanup for iOS
    setTimeout(() => {
      document.body.classList.remove('dragging');
    }, 100);
  }, [
    basicInfoItemOrder,
    companyInfoItemOrder,
    basicInfoVisibleItems,
    companyInfoVisibleItems,
    onBasicInfoOrderChange,
    onCompanyInfoOrderChange,
  ]);

  return { handleDragEnd };
};

