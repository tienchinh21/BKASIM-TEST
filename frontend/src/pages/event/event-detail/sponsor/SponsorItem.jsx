import React, { memo } from "react";
import { Box, useNavigate } from "zmp-ui";
import { FaArrowRightLong } from "react-icons/fa6";
import { openWebview } from "zmp-sdk";
import ViewTextRender from "../../../../components/renderview/ViewTextRender";
import { GlobalStyles } from "../../../../store/styles/GlobalStyles";

const SponsorItem = ({ sponsor }) => {
  const navigate = useNavigate();

  const openURL = async (link) => {
    try {
      await openWebview({
        url: link,
        config: {
          style: "bottomSheet",
          leftButton: "back",
        },
      });
    } catch (error) {
      console.log(error);
    }
  };

  return (
    <Box
      flex
      flexDirection="column"
      style={{
        minWidth: "180px",
        maxWidth: "180px",
        border: "1px solid #eee",
        borderRadius: GlobalStyles.borderRadius.sm,
        padding: GlobalStyles.spacing.sm,
        backgroundColor: "var(--background-color)",
        overflow: "hidden",
        gap: GlobalStyles.spacing.xs,
      }}
      onClick={() => navigate(`/detail-sponsor/${sponsor?.id}`)}
    >
      <img
        src={sponsor?.sponsorImage}
        alt={sponsor?.sponsorImage}
        style={{
          width: "100%",
          maxHeight: 150,
          objectFit: "cover",
          borderRadius: GlobalStyles.borderRadius.xs,
        }}
      />
      <Box
        style={{
          fontWeight: "bold",
          display: "-webkit-box",
          WebkitBoxOrient: "vertical",
          WebkitLineClamp: 2,
          overflow: "hidden",
          textOverflow: "ellipsis",
          wordBreak: "break-word",
          whiteSpace: "normal",
        }}
      >
        {sponsor?.sponsorName}
      </Box>
      <ViewTextRender
        dataText={sponsor?.sponsorIntroduction}
        style={{
          display: "-webkit-box",
          WebkitBoxOrient: "vertical",
          WebkitLineClamp: 3,
          overflow: "hidden",
          textOverflow: "ellipsis",
          wordBreak: "break-word",
          whiteSpace: "normal",
          minHeight: "65px",
          maxHeight: "65px",
          fontSize: 14,
        }}
      />
      <Box
        style={{
          padding: "6px 16px",
          backgroundColor: "var(--primary-color)",
          color: "#fff",
          borderRadius: 999,
          textDecoration: "none",
          fontSize: 14,
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
        }}
        onClick={(e) => {
          e.stopPropagation();
          navigate(`/detail-sponsor/${sponsor?.id}`, {
            state: {
              id: sponsor?.id,
            },
          });
        }}
      >
        Xem thêm
        <Box
          className="divCenter"
          style={{
            borderRadius: "50%",
            border: "1px solid #fff",
            padding: 4,
          }}
        >
          <FaArrowRightLong size={12} />
        </Box>
      </Box>
    </Box>
  );
};

export default memo(SponsorItem);
