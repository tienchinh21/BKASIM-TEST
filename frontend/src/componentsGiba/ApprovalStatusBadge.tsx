import React from "react";
import { Box, Text } from "zmp-ui";

interface ApprovalStatusBadgeProps {
  status: number | null;
  size?: "small" | "medium" | "large";
  showIcon?: boolean;
}

const ApprovalStatusBadge: React.FC<ApprovalStatusBadgeProps> = ({ 
  status, 
  size = "medium",
  showIcon = true 
}) => {
  if (status === null || status === undefined) return null;

  const getStatusConfig = () => {
    switch (status) {
      case 0:
        return {
          text: "Chờ duyệt",
          bg: "#FFF3CD",
          color: "#856404",
          borderColor: "#FFEAA7",
          icon: "⏳"
        };
      case 1:
        return {
          text: "Đã duyệt",
          bg: "#D4EDDA",
          color: "#155724",
          borderColor: "#C3E6CB",
          icon: "✓"
        };
      case 2:
        return {
          text: "Từ chối",
          bg: "#F8D7DA",
          color: "#721C24",
          borderColor: "#F5C6CB",
          icon: "✕"
        };
      default:
        return {
          text: "Không xác định",
          bg: "#E2E3E5",
          color: "#383D41",
          borderColor: "#D6D8DB",
          icon: "?"
        };
    }
  };

  const config = getStatusConfig();
  
  const sizeConfig = {
    small: {
      padding: "4px 8px",
      fontSize: "11px",
      borderRadius: "8px",
    },
    medium: {
      padding: "6px 12px",
      fontSize: "12px",
      borderRadius: "10px",
    },
    large: {
      padding: "8px 16px",
      fontSize: "14px",
      borderRadius: "12px",
    }
  };

  const currentSize = sizeConfig[size];

  return (
    <Box
      style={{
        display: "inline-flex",
        alignItems: "center",
        justifyContent: "center",
        gap: showIcon ? "6px" : "0",
        padding: currentSize.padding,
        backgroundColor: config.bg,
        borderRadius: currentSize.borderRadius,
        border: `1px solid ${config.borderColor}`,
        whiteSpace: "nowrap",
      }}
    >
      {showIcon && (
        <span style={{ fontSize: currentSize.fontSize, lineHeight: "1" }}>
          {config.icon}
        </span>
      )}
      <Text
        style={{
          color: config.color,
          fontWeight: "600",
          fontSize: currentSize.fontSize,
          lineHeight: "1",
          margin: 0,
        }}
      >
        {config.text}
      </Text>
    </Box>
  );
};

export default ApprovalStatusBadge;
