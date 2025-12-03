import React from "react";
import { Draggable, Droppable } from "react-beautiful-dnd";
import { GripVertical } from "lucide-react";
import { Image } from "antd";
import { CompanyInfo } from "../../types/memberDetail";

interface CompanyInfoCardProps {
  companyInfo?: CompanyInfo;
  itemOrder: string[];
  isDragMode: boolean;
  themeColor?: string;
}

const CompanyInfoCard: React.FC<CompanyInfoCardProps> = ({
  companyInfo,
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

  const fieldMap: Record<string, { key: keyof CompanyInfo; label: string }> = {
    logo: { key: "companyLogo", label: "Logo công ty" },
    fullName: { key: "companyFullName", label: "Tên công ty" },
    brandName: { key: "companyBrandName", label: "Thương hiệu" },
    taxCode: { key: "taxCode", label: "Mã số thuế" },
    businessField: { key: "businessField", label: "Lĩnh vực" },
    businessType: { key: "businessType", label: "Loại hình" },
    address: { key: "headquartersAddress", label: "Địa chỉ trụ sở" },
    website: { key: "companyWebsite", label: "Website" },
    phone: { key: "companyPhoneNumber", label: "SĐT công ty" },
    email: { key: "companyEmail", label: "Email công ty" },
    representative: { key: "legalRepresentative", label: "Người đại diện" },
    position: { key: "legalRepresentativePosition", label: "Chức vụ" },
    regNumber: {
      key: "businessRegistrationNumber",
      label: "Số giấy chứng nhận",
    },
    regDate: { key: "businessRegistrationDate", label: "Ngày cấp" },
    regPlace: { key: "businessRegistrationPlace", label: "Nơi cấp" },
  };

  const renderItem = (itemKey: string, index: number) => {
    const field = fieldMap[itemKey];
    if (!field || !companyInfo?.[field.key]) return null;

    let content = null;

    if (itemKey === "logo") {
      content = (
        <div style={{ textAlign: "center" }}>
          <div style={labelStyle}>{field.label}</div>
          <div
            style={{
              display: "flex",
              justifyContent: "center",
              marginTop: "8px",
            }}
          >
            <Image
              src={companyInfo.companyLogo}
              alt={field.label}
              width={80}
              height={80}
              style={{
                borderRadius: "8px",
                border: "2px solid rgba(255, 255, 255, 0.2)",
                cursor: "pointer",
              }}
              preview={{
                mask: <span style={{ color: "#fff" }}>Xem chi tiết</span>,
                maskStyle: {
                  background: "rgba(0, 0, 0, 0.5)",
                  borderRadius: "4px",
                },
              }}
            />
          </div>
        </div>
      );
    } else {
      content = (
        <>
          <div style={labelStyle}>{field.label}</div>
          <div style={valueStyle}>{companyInfo[field.key]}</div>
        </>
      );
    }

    return (
      <Draggable
        key={itemKey}
        draggableId={`companyInfo-${itemKey}`}
        index={index}
        isDragDisabled={!isDragMode}
      >
        {(provided, snapshot) => {
          const draggableStyle = provided.draggableProps.style;
          const style = {
            ...itemStyle,
            opacity: snapshot.isDragging ? 0.5 : 1,
            display: "flex",
            alignItems: itemKey === "logo" ? "flex-start" : "center",
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
    const field = fieldMap[itemKey];
    return field && !!companyInfo?.[field.key];
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
        Thông tin công ty
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

      <Droppable droppableId="companyInfo-items" isDropDisabled={!isDragMode}>
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

export default CompanyInfoCard;
