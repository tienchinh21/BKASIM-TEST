import React, { useState, memo, useEffect, useRef } from "react";
import { Box } from "zmp-ui";
import "./category.css";

const Category = ({
  list,
  value = "",
  onChange,
  valueChild = "",
  onChangeValueChild,
  containerStyle = {},
  backgroundColor = "#efefef",
}) => {
  const itemRefs = useRef({});
  const [listChild, setListChild] = useState([]);

  useEffect(() => {
    const category = list.find((item) => item.id === value);
    const hasChild = category?.listChild?.length > 0;

    // Scroll vào vị trí được chọn
    if (itemRefs.current[value]) {
      itemRefs.current[value].scrollIntoView({
        behavior: "smooth",
        inline: "center",
        block: "nearest",
      });
    }

    if (value) {
      setListChild(
        hasChild ? [{ id: "", name: "Tất cả" }, ...category.listChild] : []
      );
    } else {
      setListChild([]);
    }
  }, [value, list]);

  return (
    <>
      <Box
        className="hidden-scrollbar-y"
        style={{
          display: "flex",
          fontSize: 14,
          overflowX: "auto",
          whiteSpace: "nowrap",
          flexDirection: "row",
          backgroundColor: backgroundColor,
          ...containerStyle,
        }}
      >
        {list.map((item, index) => (
          <Box
            key={index}
            id={item.id}
            ref={(el) => (itemRefs.current[item.id] = el)}
            className={`item-category ${
              value === item.id ? "item-category-active" : ""
            }`}
            onClick={() => {
              onChange(item.id);
            }}
          >
            {item.name}
          </Box>
        ))}
      </Box>

      {listChild.length > 0 && (
        <Box
          className="hidden-scrollbar-y"
          style={{
            display: "flex",
            fontSize: 14,
            overflowX: "auto",
            whiteSpace: "nowrap",
            flexDirection: "row",
            marginTop: 8,
          }}
        >
          {listChild.map((item, index) => (
            <Box
              key={index}
              id={item.id}
              className={`item-category-child ${
                valueChild === item.id ? "item-category-child-active" : ""
              }`}
              onClick={() => {
                onChangeValueChild(item.id);
              }}
            >
              {item.name}
            </Box>
          ))}
        </Box>
      )}
    </>
  );
};

export default memo(Category);
