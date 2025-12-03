import React from "react";
import { Box } from "zmp-ui";
import { Calendar, MapPin } from "lucide-react";
import { openWebview } from "zmp-sdk";
import ProductDetailPromotionItem from "../../product/ProductDetailPromotionItem";
import ViewTextRender from "../../../components/renderview/ViewTextRender";
import { formatTime, formatDate } from "../../../utils/dateFormatter";

const EventInfo = ({ detail }) => {
  const ggmapWebView = async (link) => {
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

  // Using utility functions from dateFormatter

  return (
    <Box
      style={{
        display: "flex",
        flexDirection: "column",
        gap: 10,
        background: "#fff",
      }}
    >
      <Box
        mb={1}
        style={{
          fontWeight: "bold",
          display: "-webkit-box",
          WebkitBoxOrient: "vertical",
          WebkitLineClamp: 2,
          overflow: "hidden",
          textOverflow: "ellipsis",
          wordBreak: "break-word",
          whiteSpace: "normal",
          fontSize: 18,
        }}
      >
        {detail?.title}
      </Box>

      <Box
        style={{
          display: "flex",
          alignItems: "center",
          gap: 4,
          fontSize: 14,
        }}
      >
        <Calendar size={20} style={{ minWidth: "8%", maxWidth: "8%" }} />
        <Box>
          {`${formatTime(detail?.startTime)} - ${formatTime(
            detail?.endTime
          )}, ${formatDate(detail?.startTime)} - ${formatDate(
            detail?.endTime
          )}`}
        </Box>
      </Box>

      <Box
        style={{
          display: "flex",
          alignItems: "center",
          gap: 4,
          fontSize: 14,
        }}
      >
        <MapPin size={20} style={{ minWidth: "8%", maxWidth: "8%" }} />
        {detail?.type === 2 ? detail?.address : "Online"}
      </Box>

      <Box
        ml={6}
        style={{
          background: "#D9D9D9",
          padding: "4px 20px",
          width: "fit-content",
          borderRadius: 10,
          fontSize: 14,
        }}
        onClick={() => {
          const link =
            detail?.type === 2 ? detail?.googleMapURL : detail?.meetingLink;
          ggmapWebView(link);
        }}
      >
        {detail?.type === 2 ? "Xem Google Map" : "Vào meeting"}
      </Box>

      <ViewTextRender dataText={detail?.content} style={{ marginTop: 8 }} />

      {detail?.gifts?.length > 0 && (
        <Box>
          <Box
            style={{
              display: "flex",
              flexDirection: "column",
            }}
          >
            <Box
              style={{
                display: "flex",
                justifyContent: "space-between",
                alignItems: "center",
              }}
            >
              <Box style={{ fontWeight: "bold" }}>Quà tặng</Box>
              <Box
                style={{
                  fontWeight: 600,
                  background: "var(--primary-background-color)",
                  fontSize: 13,
                  padding: "1px 12px",
                }}
              >
                Nhận tại sự kiện
              </Box>
            </Box>
          </Box>
          {detail?.gifts?.map((item) => (
            <ProductDetailPromotionItem key={item?.id} item={item} />
          ))}
        </Box>
      )}
    </Box>
  );
};

export default EventInfo;
