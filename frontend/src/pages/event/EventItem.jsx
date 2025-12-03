import React, { memo, useState } from "react";
import { Box } from "zmp-ui";
import DefaultImage from "../../assets/no_image.png";
import { IoCalendarSharp } from "react-icons/io5";
import { FaLocationDot } from "react-icons/fa6";
import { useNavigate } from "react-router-dom";
import { MdQrCode } from "react-icons/md";
import CommonButton from "../../components/CommonButton";
import "./event.css";
import { GlobalStyles } from "../../store/styles/GlobalStyles";
import { QRCodeIcon } from "../../components/Icons/SVGIcon";
import EventRegisterSelector from "../../componentsGiba/EventRegisterSelector";
const EventItem = ({ item, disableActions = false }) => {
  const navigate = useNavigate();
  const [showRegisterSelector, setShowRegisterSelector] = useState(false);

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

  const renderStatusButton = (item, navigate) => {
    // Nếu disable actions, chỉ hiển thị nút xem chi tiết
    if (disableActions) {
      return (
        <CommonButton
          text="Xem chi tiết"
          onClick={(e) => {
            e.stopPropagation();
            navigate(`/giba/event-detail/${item?.id}`);
          }}
          style={{
            background: "#6b7280",
            border: "none",
            fontSize: 15,
            height: 40,
          }}
        />
      );
    }

    if (item?.status === 1) {
      return (
        <div style={{ display: "flex", gap: "8px", flexDirection: "column" }}>
          {item?.isRegister && (
            <CommonButton
              text="Đăng ký danh sách khách mời"
              onClick={(e) => {
                e.stopPropagation();
                navigate("/giba/event-guest-register", {
                  state: { eventId: item?.id },
                });
              }}
              style={{
                background: "#9C010B",
                border: "none",
                fontSize: 13,
                height: 32,
              }}
            />
          )}
          {/* Nếu đã đăng ký và có checkInCode thì hiển thị QR Check-in */}
          {item?.isRegister && item?.checkInCode ? (
            <CommonButton
              text="Đã đăng ký tham gia"
              onClick={(e) => {
                e.stopPropagation();
                navigate(`/giba/event-detail/${item?.id}`);
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
              text="Đăng ký tham gia"
              onClick={(e) => {
                e.stopPropagation();
                console.log("Button clicked, isRegister:", item?.isRegister);
                // Nếu đã đăng ký cá nhân, mở thẳng form danh sách khách mời
                if (item?.isRegister === true) {
                  console.log("Opening guest register page directly");
                  navigate("/giba/event-guest-register", {
                    state: { eventId: item?.id },
                  });
                } else {
                  // Chưa đăng ký thì mở selector để chọn
                  console.log("Opening register selector");
                  setShowRegisterSelector(true);
                }
              }}
              style={{
                background: "#558B2F",
                border: "none",
                fontSize: 15,
                height: 40,
              }}
            />
          )}
        </div>
      );
    }

    if (item?.status === 2) {
      if (item?.isCheckIn) {
        return (
          <CommonButton
            text="Đã Check-in"
            onClick={(e) => e.stopPropagation()}
            style={{
              background: "#919191",
              border: "none",
              height: 40,
              fontSize: 15,
            }}
          />
        );
      } else {
        return (
          <div style={{ display: "flex", gap: "8px", flexDirection: "column" }}>
            {item?.isRegister && (
              <CommonButton
                text="Đăng ký danh sách khách mời"
                onClick={(e) => {
                  e.stopPropagation();
                  navigate("/giba/event-guest-register", {
                    state: { eventId: item?.id },
                  });
                }}
                style={{
                  background: "#9C010B",
                  border: "none",
                  fontSize: 13,
                  height: 32,
                }}
              />
            )}
            {item?.isRegister ? (
              <CommonButton
                leftIcon={<QRCodeIcon />}
                text="QR Check-in"
                onClick={(e) => {
                  e.stopPropagation();
                  navigate(`/giba/event-detail/${item?.id}`);
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
                text="Đăng ký tham gia"
                onClick={(e) => {
                  e.stopPropagation();
                  setShowRegisterSelector(true);
                }}
                style={{
                  background: "#558B2F",
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

    if (item?.status === 3) {
      return (
        <CommonButton
          text="Sự kiện đã kết thúc"
          onClick={(e) => e.stopPropagation()}
          style={{
            background: "var(--five-background-color)",
            border: "none",
            fontSize: 15,
            height: 40,
          }}
        />
      );
    }

    return null;
  };

  const formatDateTime = (isoString) => {
    const date = new Date(isoString);
    const hours = date.getHours().toString().padStart(2, "0");
    const minutes = date.getMinutes().toString().padStart(2, "0");
    const day = date.getDate().toString().padStart(2, "0");
    const month = (date.getMonth() + 1).toString().padStart(2, "0"); // Tháng bắt đầu từ 0
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
      onClick={() => navigate(`/giba/event-detail/${item?.id}`)}
    >
      <Box style={{ position: "relative" }}>
        <img
          src={item?.banner || DefaultImage}
          alt=""
          style={{
            width: "100%",
            aspectRatio: "16 / 9",
            border: "none",
            objectFit: "cover",
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
            WebkitLineClamp: 2,
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
            {`${formatDateTime(item?.startTime)} - ${formatDateTime(
              item?.endTime
            )}`}
          </Box>
        </Box>
        <Box className="item-content-event">
          <FaLocationDot size={20} />
          {item?.address}
        </Box>
        <Box>{renderStatusButton(item, navigate)}</Box>
      </Box>

      {/* Event Register Selector */}
      <EventRegisterSelector
        visible={showRegisterSelector}
        onClose={() => {
          setShowRegisterSelector(false);
        }}
        eventId={item?.id}
        isPersonalRegistered={item?.isRegister}
        onSuccess={() => {
          // Refresh data or show success message
          console.log("Event registration successful");
          setShowRegisterSelector(false);
        }}
      />
    </Box>
  );
};
export default memo(EventItem);
