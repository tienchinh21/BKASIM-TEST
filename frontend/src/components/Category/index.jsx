import React, { memo, useRef, useEffect } from "react";
import { Box } from "zmp-ui";
import "./category.css";

const Category = ({
  list,
  value = "",
  onChange,
  containerStyle = {},
  backgroundColor = "#fff",
  disabled = false,
}) => {
  const itemRefs = useRef({});

  useEffect(() => {
    if (value && itemRefs.current[value]) {
      itemRefs.current[value].scrollIntoView({
        behavior: "smooth",
        inline: "center",
        block: "nearest",
      });
    }
  }, [value]);

  return (
    <Box
      className="category-container hidden-scrollbar-y"
      style={{
        backgroundColor: backgroundColor,
        ...containerStyle,
      }}
    >
      {list.map((item) => (
        <div
          key={item.id}
          ref={(el) => (itemRefs.current[item.value] = el)}
          className={`category-item ${
            value === item.value ? "category-item-active" : ""
          } ${item.disabled || disabled ? "category-item-disabled" : ""}`}
          onClick={() => !item.disabled && !disabled && onChange(item.value)}
          style={{
            cursor: item.disabled || disabled ? "not-allowed" : "pointer",
            opacity: item.disabled || disabled ? 0.5 : 1,
          }}
        >
          {item.name}
        </div>
      ))}
    </Box>
  );
};

export default memo(Category);
