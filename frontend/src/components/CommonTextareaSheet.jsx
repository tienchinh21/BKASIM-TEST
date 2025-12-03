import React, { useState, useImperativeHandle, forwardRef } from "react";
import { Box, Input, Sheet } from "zmp-ui";
import { FaTrash } from "react-icons/fa";
import CommonButton from "./CommonButton";
import { RiCloseLargeLine } from "react-icons/ri";
import { GlobalStyles } from "../store/styles/GlobalStyles";
import { TrashIcon } from "./Icons/SVGIcon";
import usePlatform from "../hooks/usePlatform";

const CloseButton = ({ onClose }) => (
  <Box
    className="divCenter"
    onClick={onClose}
    style={{
      position: "absolute",
      top: 10,
      right: 10,
      width: 28,
      height: 28,
      borderRadius: "50%",
      zIndex: 10,
    }}
  >
    <RiCloseLargeLine size={18} />
  </Box>
);
const CommonTextareaSheet = forwardRef(
  (
    {
      title = "Nội dung",
      placeholder = "Nhập nội dung",
      maxLength = 1000,
      confirmText = "Xong",
      handleConfirm,
    },
    ref
  ) => {
    const [visible, setVisible] = useState(false);
    const [value, setValue] = useState("");
    const platform = usePlatform();

    const open = (defaultValue = "") => {
      setValue(defaultValue);
      setVisible(true);
    };

    const close = () => setVisible(false);

    useImperativeHandle(ref, () => ({ open }));

    const onSubmit = () => {
      if (handleConfirm) handleConfirm(value);
      close();
    };

    return (
      <Sheet
        visible={visible}
        onClose={close}
        height={platform === "IOS" ? 380 : 330}
        autoHeight
        style={{ padding: "14px" }}
        maskClosable
      >
        <CloseButton onClose={close} />

        <Box
          flex
          alignItems="center"
          style={{
            paddingTop: GlobalStyles.padding.sm,
            justifyContent: "space-between",
          }}
        >
          <Box style={{ fontWeight: "bold", fontSize: 16 }}>{title}</Box>
          {value && (
            <Box
              className="divCenter"
              onClick={() => {
                setValue("");
              }}
              style={{
                cursor: "pointer",
                padding: 4,
              }}
            >
              <TrashIcon />
            </Box>
          )}
        </Box>

        <Input.TextArea
          placeholder={placeholder}
          value={value}
          maxLength={maxLength}
          onChange={(e) => setValue(e.target.value)}
          rows={4}
          style={{
            marginTop: GlobalStyles.spacing.sm,
            padding: GlobalStyles.spacing.sm,
            border: "1px solid #ccc",
            borderRadius: GlobalStyles.borderRadius.sm,
          }}
        />

        <Box
          style={{
            display: "flex",
            justifyContent: "flex-end",
            marginTop: 4,
            fontSize: 12,
            color: "#888",
          }}
        >
          {value.length}/{maxLength}
        </Box>

        <CommonButton
          style={{
            height: 40,
          }}
          text={confirmText}
          onClick={onSubmit}
        />
      </Sheet>
    );
  }
);

export default CommonTextareaSheet;
