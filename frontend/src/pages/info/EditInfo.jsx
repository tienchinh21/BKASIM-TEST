import React, { useEffect, useState, useLayoutEffect } from "react";
import { useRecoilState, useRecoilValue } from "recoil";

import { Page, Box } from "zmp-ui";
const EditInfo = () => {

  return (
    <Page
      hideScrollbar={true}
      style={{
        paddingTop: 50,
        background: "var(--primary-background-color)",
        overflowY: "auto",
      }}
    >
      <Spin spinning={spinning} fullscreen />
      <Box
        style={{
          display: "flex",
          flexDirection: "column",
          alignItems: "center",
        }}
      >
        hi
       
      </Box>
    </Page>
  );
};

export default EditInfo;
