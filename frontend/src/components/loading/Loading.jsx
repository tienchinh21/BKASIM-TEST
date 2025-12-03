import React from "react";
import { Box, Spinner } from "zmp-ui";

const Loading = () => {
  return (
    <Box
      style={{
        position: "absolute",
        top: 0,
        bottom: 0,
        left: 0,
        right: 0,
        display: "flex",
        justifyContent: "center",
        alignItems: "center",
      }}
    >
      <Spinner size="small" color="#fff" />
    </Box>
  );
};

export default Loading;
