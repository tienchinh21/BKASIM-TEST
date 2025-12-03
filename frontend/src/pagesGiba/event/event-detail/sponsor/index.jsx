import React, { memo } from "react";
import { Box } from "zmp-ui";
import SponsorItem from "./SponsorItem";

const Sponsor = ({ tiers }) => {
  return (
    <Box>
      <Box flex style={{ alignItems: "center", gap: 10, margin: "16px 0 8px" }}>
        <img
          src={tiers?.tierImage}
          alt={tiers?.tierImage}
          height={20}
          width={20}
        />
        <Box style={{ fontWeight: "bold" }}>Nhà tài trợ {tiers?.tierName}</Box>
      </Box>
      <Box
        className="hidden-scrollbar"
        style={{
          display: "flex",
          flexWrap: "nowrap",
          overflowX: "auto",
          whiteSpace: "nowrap",
          gap: 16,
        }}
      >
        {tiers?.sponsors?.map((sponsor, index) => (
          <SponsorItem key={index} sponsor={sponsor} />
        ))}
      </Box>
    </Box>
  );
};

export default memo(Sponsor);
