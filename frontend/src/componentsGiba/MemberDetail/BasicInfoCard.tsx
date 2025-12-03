import React from "react";
import { Draggable, Droppable } from "react-beautiful-dnd";
import { GripVertical } from "lucide-react";
import { BasicInfo, RatingInfo } from "../../types/memberDetail";

interface BasicInfoCardProps {
  basicInfo?: BasicInfo;
  ratingInfo?: RatingInfo;
  itemOrder: string[];
  isDragMode: boolean;
  themeColor?: string;
}

const BasicInfoCard: React.FC<BasicInfoCardProps> = ({
  basicInfo,
  ratingInfo,
  itemOrder,
  isDragMode,
  themeColor = "#1877F2",
}) => {
  const addOpacity = (color: string, opacity: number) => {
    if (color.startsWith("#")) {
      const hex = color.slice(1);
      const r = parseInt(hex.substring(0, 2), 16);
      const g = parseInt(hex.substring(2, 4), 16);
      const b = parseInt(hex.substring(4, 6), 16);
      return `rgba(${r}, ${g}, ${b}, ${opacity})`;
    }
    return color;
  };
  const itemStyle = {
    background: "#f0f2f5",
    borderRadius: "8px",
    padding: "12px 16px",
  };

  const labelStyle = {
    color: "#65676b",
    fontSize: "13px",
    fontWeight: "600" as const,
    marginBottom: "4px",
  };

  const valueStyle = {
    color: "#050505",
    fontSize: "15px",
    fontWeight: "400" as const,
    lineHeight: "1.5",
  };

  const renderItem = (itemKey: string, index: number) => {
    let content : React.ReactNode = null;

    if (itemKey === "dayOfBirth" && basicInfo?.dayOfBirth) {
      content = (
        <>
          <div style={labelStyle}>Ngày sinh</div>
          <div style={valueStyle}>{basicInfo.dayOfBirth}</div>
        </>
      );
    } else if (itemKey === "address" && basicInfo?.address) {
      content = (
        <>
          <div style={labelStyle}>Địa chỉ</div>
          <div style={valueStyle}>{basicInfo.address}</div>
        </>
      );
    } else if (
      itemKey === "rating" &&
      ratingInfo &&
      ratingInfo.totalRatings > 0
    ) {
      content = (
        <>
          <div style={labelStyle}>Đánh giá</div>
          <div style={valueStyle}>
            {ratingInfo.averageRating}/5 ({ratingInfo.totalRatings} đánh giá)
          </div>
        </>
      );
    }

    if (!content) return null;

    return (
      <Draggable
        key={itemKey}
        draggableId={`basicInfo-${itemKey}`}
        index={index}
        isDragDisabled={!isDragMode}
      >
        {(provided, snapshot) => {
          const draggableStyle = provided.draggableProps.style;
          const style = {
            ...itemStyle,
            opacity: snapshot.isDragging ? 0.5 : 1,
            display: "flex",
            alignItems: "center",
            gap: "12px",
            transition: snapshot.isDragging ? "none" : "opacity 0.2s ease",
            transform: draggableStyle?.transform || "none",
            ...draggableStyle,
          };

          return (
            <div
              ref={provided.innerRef}
              {...provided.draggableProps}
              style={style}
            >
              <div
                {...provided.dragHandleProps}
                style={{
                  flexShrink: 0,
                  opacity: isDragMode ? 1 : 0,
                  width: "16px",
                  cursor: isDragMode
                    ? snapshot.isDragging
                      ? "grabbing"
                      : "grab"
                    : "default",
                  pointerEvents: isDragMode ? "auto" : "none",
                }}
              >
                <GripVertical size={16} color="#fff" opacity={0.6} />
              </div>
              <div style={{ flex: 1 }}>{content}</div>
            </div>
          );
        }}
      </Draggable>
    );
  };

  const hasItem = (itemKey: string) => {
    if (itemKey === "dayOfBirth") return !!basicInfo?.dayOfBirth;
    if (itemKey === "address") return !!basicInfo?.address;
    if (itemKey === "rating") return ratingInfo && ratingInfo.totalRatings > 0;
    return false;
  };

  const visibleItems = itemOrder.filter(hasItem);

  if (visibleItems.length === 0) return null;

  return (
    <div
      style={{
        background: "#fff",
        borderRadius: "8px",
        padding: "16px",
        boxShadow: "0 1px 2px rgba(0, 0, 0, 0.1)",
        borderLeft: `4px solid ${themeColor}`,
      }}
    >
      <h2
        style={{
          fontSize: "20px",
          fontWeight: "700",
          color: themeColor,
          marginBottom: "16px",
          display: "flex",
          alignItems: "center",
          gap: "8px",
        }}
      >
        <div
          style={{
            width: "4px",
            height: "20px",
            background: themeColor,
            borderRadius: "2px",
          }}
        />
        Thông tin cá nhân
      </h2>

      {isDragMode && (
        <div
          style={{
            background: "#fff7e6",
            padding: "8px 12px",
            borderRadius: "6px",
            marginBottom: "12px",
            fontSize: "13px",
            color: "#faad14",
            fontWeight: "500",
            border: "1px solid #ffe58f",
          }}
        >
          Kéo để sắp xếp các mục
        </div>
      )}

      <Droppable droppableId="basicInfo-items" isDropDisabled={!isDragMode}>
        {(provided) => (
          <div
            ref={provided.innerRef}
            {...provided.droppableProps}
            style={{ display: "flex", flexDirection: "column", gap: "12px" }}
          >
            {visibleItems.map((item, index) => renderItem(item, index))}
            {provided.placeholder}
          </div>
        )}
      </Droppable>
    </div>
  );
};

export default BasicInfoCard;
