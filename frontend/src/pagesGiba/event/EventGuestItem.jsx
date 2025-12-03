import React, { memo } from "react";
import { Box } from "zmp-ui";
import DefaultImage from "../../assets/no_image.png";
import {
  IoCalendarSharp,
  IoTimeOutline,
} from "react-icons/io5";
import { FaLocationDot, FaCheck } from "react-icons/fa6";
import { useNavigate } from "react-router-dom";
import CommonButton from "../../components/CommonButton";
import "./event.css";
import { GlobalStyles } from "../../store/styles/GlobalStyles";
import { QRCodeIcon } from "../../components/Icons/SVGIcon";
import dfData from "../../common/DefaultConfig.json";
import { formatDateTime } from "../../utils/dateFormatter";

const EventGuestItem = ({ item, titleMaxLines = 2 }) => {
  const navigate = useNavigate();

  const getBannerUrl = (banner) => {
    if (!banner) return DefaultImage;
    if (banner.startsWith("http://") || banner.startsWith("https://")) {
      return banner;
    }
    if (banner.startsWith("/")) {
      const fullUrl = `${dfData.domain}${banner}`;
      return fullUrl;
    }
    return banner;
  };

  const getStatusStyle = (status) => {
    const baseStyle = {
      color: "white",
      padding: "4px 20px",
      borderRadius: GlobalStyles.borderRadius.xs,
      fontSize: "12px",
      fontWeight: "bold",
      display: "inline-block",
    };

    if (status === 2) {
      return {
        ...baseStyle,
      };
    } else if (status === 3) {
      return {
        ...baseStyle,
        background: "var(--five-background-color)",
      };
    } else {
      return {
        ...baseStyle,
        background: "#CCCCCC",
      };
    }
  };

  const getStatusText = (status, startTime) => {
    if (status === 1 && startTime) {
      const now = new Date();
      const start = new Date(startTime);
      const diffMs = start.getTime() - now.getTime();

      if (diffMs <= 0) return "Sắp diễn ra";

      const diffMinutes = Math.floor(diffMs / (1000 * 60));
      const diffHours = Math.floor(diffMinutes / 60);
      const diffDays = Math.floor(diffHours / 24);

      if (diffDays >= 1) {
        return `${diffDays} ngày nữa`;
      } else if (diffHours >= 1) {
        return `${diffHours} giờ nữa`;
      } else {
        return `${diffMinutes} phút nữa`;
      }
    }

    switch (status) {
      case 2:
        return "Đang diễn ra";
      case 3:
        return "Đã kết thúc";
      default:
        return "";
    }
  };

  const getEventTypeBadge = (type) => {
    if (type === 2) {
      return {
        label: "Công khai",
        bgColor: "#E8F5E9",
        textColor: "#2E7D32",
        borderColor: "#81C784",
      };
    } else {
      return {
        label: "Nội bộ",
        bgColor: "#FFF3E0",
        textColor: "#E65100",
        borderColor: "#FFB74D",
      };
    }
  };

  const renderStatusButton = (item) => {
    // Status 1: Sắp diễn ra
    if (item?.status === 1) {
      return (
        <div
          style={{
            display: "flex",
            gap: "8px",
            flexDirection: "column",
            minHeight: "96px",
            justifyContent: "flex-end",
          }}
        >
          {item?.checkInCode ? (
            <CommonButton
              leftIcon={<FaCheck size={18} color="white" />}
              text="Đã xác nhận tham gia"
              onClick={(e) => {
                e.stopPropagation();
                navigate(`/giba/event-detail/${item?.id}`, {
                  state: { eventData: item, fromGuestList: true },
                });
              }}
              style={{
                background: "#919191",
                border: "none",
                fontSize: 15,
                height: 40,
              }}
            />
          ) : (
            <CommonButton
              leftIcon={<IoTimeOutline size={18} color="white" />}
              text="Đang đợi duyệt"
              onClick={(e) => {
                e.stopPropagation();
                navigate(`/giba/event-detail/${item?.id}`, {
                  state: { eventData: item, fromGuestList: true },
                });
              }}
              style={{
                background: "#94A3B8",
                border: "none",
                fontSize: 15,
                height: 40,
              }}
            />
          )}
        </div>
      );
    }

    // Status 2: Đang diễn ra
    if (item?.status === 2) {
      if (item?.checkInStatus) {
        // Đã check-in
        return (
          <div
            style={{
              minHeight: "96px",
              display: "flex",
              alignItems: "flex-end",
            }}
          >
            <CommonButton
              leftIcon={<FaCheck size={18} color="white" />}
              text="Đã Check-in"
              onClick={(e) => e.stopPropagation()}
              style={{
                background: "#919191",
                border: "none",
                height: 40,
                fontSize: 15,
                width: "100%",
              }}
            />
          </div>
        );
      } else {
        // Chưa check-in
        return (
          <div
            style={{
              display: "flex",
              gap: "8px",
              flexDirection: "column",
              minHeight: "96px",
              justifyContent: "flex-end",
            }}
          >
            {item?.checkInCode ? (
              <CommonButton
                leftIcon={<QRCodeIcon />}
                text="QR Check-in"
                onClick={(e) => {
                  e.stopPropagation();
                  navigate("/check-in", {
                    state: { checkInCode: item?.checkInCode },
                  });
                }}
                style={{
                  background: "#0069F3",
                  border: "none",
                  fontSize: 15,
                  height: 40,
                }}
              />
            ) : (
              <CommonButton
                leftIcon={<IoTimeOutline size={18} />}
                text="Đang đợi duyệt"
                onClick={(e) => {
                  e.stopPropagation();
                  navigate(`/giba/event-detail/${item?.id}`, {
                    state: { eventData: item, fromGuestList: true },
                  });
                }}
                style={{
                  background: "#94A3B8",
                  border: "none",
                  fontSize: 15,
                  height: 40,
                }}
              />
            )}
          </div>
        );
      }
    }

    // Status 3: Đã kết thúc
    if (item?.status === 3) {
      return (
        <div
          style={{
            minHeight: "96px",
            display: "flex",
            alignItems: "flex-end",
          }}
        >
          <CommonButton
            leftIcon={<FaCheck size={18} color="white" />}
            text="Sự kiện đã kết thúc"
            onClick={(e) => e.stopPropagation()}
            style={{
              background: "var(--five-background-color)",
              border: "none",
              fontSize: 15,
              height: 40,
              width: "100%",
            }}
          />
        </div>
      );
    }

    return null;
  };

  const formatDateTimeDisplay = (dateTimeString) => {
    if (!dateTimeString) return "N/A";

    const date = new Date(dateTimeString);
    if (isNaN(date.getTime())) return "N/A";

    const hours = date.getHours().toString().padStart(2, "0");
    const minutes = date.getMinutes().toString().padStart(2, "0");
    const day = date.getDate().toString().padStart(2, "0");
    const month = (date.getMonth() + 1).toString().padStart(2, "0");
    const year = date.getFullYear();

    return `${hours}:${minutes} ${day}/${month}/${year}`;
  };

  return (
    <Box
      style={{
        background: "#f4f4f4",
        borderRadius: GlobalStyles.borderRadius.sm,
        overflow: "hidden",
      }}
      onClick={() => navigate(`/giba/event-detail/${item?.id}`, { state: { eventData: item, fromGuestList: true } })}
    >
      <Box style={{ position: "relative" }}>
        <img
          src={getBannerUrl(item?.banner)}
          alt=""
          style={{
            width: "100%",
            aspectRatio: "16 / 9",
            border: "none",
            objectFit: "cover",
          }}
          onError={(e) => {
            e.target.src = DefaultImage;
          }}
        />
        <Box style={{ position: "absolute", top: 10, left: 10 }}>
          <div
            style={getStatusStyle(item?.status)}
            className={item?.status === 2 ? "gradient-animate" : ""}
          >
            {getStatusText(item?.status, item?.startTime)}
          </div>
        </Box>
        <Box
          style={{
            position: "absolute",
            top: 10,
            right: 10,
            display: "flex",
            flexDirection: "column",
            gap: "6px",
            alignItems: "flex-end",
          }}
        >
          {/* Badge loại nội dung */}
          <div
            style={{
              backgroundColor: "#E3F2FD",
              color: "#1976D2",
              border: "1px solid #90CAF9",
              padding: "4px 12px",
              borderRadius: "12px",
              fontSize: "11px",
              fontWeight: "600",
              display: "inline-block",
            }}
          >
            Sự kiện
          </div>
          {/* Badge type (Công khai/Nội bộ) */}
          {item?.type && (
            <div
              style={{
                backgroundColor: getEventTypeBadge(item.type).bgColor,
                color: getEventTypeBadge(item.type).textColor,
                border: `1px solid ${getEventTypeBadge(item.type).borderColor}`,
                padding: "4px 12px",
                borderRadius: "12px",
                fontSize: "11px",
                fontWeight: "600",
                display: "inline-block",
              }}
            >
              {getEventTypeBadge(item.type).label}
            </div>
          )}
        </Box>
      </Box>
      <Box
        flex
        flexDirection="column"
        style={{
          padding: GlobalStyles.spacing.sm,
          gap: GlobalStyles.spacing.sm,
        }}
      >
        <Box
          style={{
            fontWeight: "bold",
            display: "-webkit-box",
            WebkitBoxOrient: "vertical",
            WebkitLineClamp: titleMaxLines,
            overflow: "hidden",
            textOverflow: "ellipsis",
            wordBreak: "break-word",
            whiteSpace: "normal",
          }}
        >
          {item?.title}
        </Box>
        <Box className="item-content-event">
          <IoCalendarSharp size={20} />
          <Box>
            {`${formatDateTimeDisplay(item?.startTime)} - ${formatDateTimeDisplay(
              item?.endTime
            )}`}
          </Box>
        </Box>
        <Box className="item-content-event">
          <FaLocationDot size={20} />
          {item?.address}
        </Box>
        <Box>{renderStatusButton(item)}</Box>
      </Box>
    </Box>
  );
};

export default memo(EventGuestItem);
