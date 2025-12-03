import React, { memo } from "react";
import { Box } from "zmp-ui";

const ProductDetailPromotionItem = ({ item }) => (
  <Box
    mt={2}
    style={{
      display: "flex",
      gap: 10,
      borderBottom: "1px solid #ccc",
      padding: "4px 0 10px",
      minHeight: "100px",
    }}
  >
    <img src={item?.images?.[0]} style={{ width: "30%" }} />
    <Box>
      <Box style={{ fontSize: 14, display: "flex", gap: 4 }}>
        <Box style={{ color: "var(--primary-color)", fontWeight: 600 }}>
          Tặng
        </Box>
        {item?.name}
      </Box>
      <Box
        style={{
          display: "flex",
          justifyContent: "start",
          alignItems: "center",
          gap: 10,
        }}
      >
        <Box
          style={{
            color: "var(--primary-color)",
            fontWeight: 600,
          }}
        >
          Free
        </Box>
        <Box
          style={{
            textDecoration: "line-through",
            display: "inline-flex",
            color: "#888",
            fontSize: 12,
            alignItems: "center",
            whiteSpace: "nowrap",
          }}
        >
          {item?.price?.toLocaleString("vi-VN")}
          &nbsp;đ
        </Box>
      </Box>
    </Box>
  </Box>
);

export default memo(ProductDetailPromotionItem);
