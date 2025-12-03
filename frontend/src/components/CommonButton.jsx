import React from "react";
import { Box } from "zmp-ui";
import { GlobalStyles } from "../store/styles/GlobalStyles";

const CommonButton = ({
  text,
  onClick,
  secondButton = false,
  isFixedBottom = false,
  variant = "primary", // "primary", "outline", "disabled"
  isDisabled = false,
  style = {},
  leftIcon,
  rightIcon,
  ...props
}) => {
  const isBtnDisabled = variant === "disabled" || isDisabled;

  const baseStyle = {
    fontSize: GlobalStyles.fontSize.md,
    width: "100%",
    height: 35,
    borderRadius: GlobalStyles.borderRadius.xs,
    fontWeight: "bold",
    textAlign: "center",
  };

  const variantStyle = {
    primary: {
      background: "#333",
      color: "var(--second-color)",
      border: "1px solid #333",
    },
    outline: {
      background: "var(--second-color)",
      color: "#333",
      border: "1px solid #333",
    },
    disabled: {
      background: "#E0E0E0",
      color: "#A0A0A0",
      cursor: "not-allowed",
      pointerEvents: "none",
      border: "none",
      opacity: 0.7,
    },
  };

  const buttonStyle = {
    ...baseStyle,
    ...(variantStyle[variant] || variantStyle["primary"]),
    ...style,
  };

  const ButtonElement = (
    <Box
      className="divCenter"
      style={{
        ...buttonStyle,
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        gap: leftIcon || rightIcon ? "8px" : "0",
      }}
      onClick={!isBtnDisabled ? onClick : undefined}
      {...props}
    >
      {leftIcon && (
        <span
          style={{
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
          }}
        >
          {leftIcon}
        </span>
      )}
      <span>{text}</span>
      {rightIcon && (
        <span
          style={{
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
          }}
        >
          {rightIcon}
        </span>
      )}
    </Box>
  );

  if (isFixedBottom) {
    return (
      <Box
        style={{
          position: "fixed",
          bottom: 0,
          left: 0,
          right: 0,
          width: "100%",
          zIndex: 100,
          height: 65,
          background: "var(--fourth-background-color)",
          padding: "10px 14px",
          paddingBottom: 20,
        }}
      >
        {ButtonElement}
      </Box>
    );
  }

  return ButtonElement;
};

export default CommonButton;
