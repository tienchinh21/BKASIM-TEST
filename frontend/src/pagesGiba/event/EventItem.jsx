import React, { memo, useState } from "react";
import { Box, useSnackbar } from "zmp-ui";
import DefaultImage from "../../assets/no_image.png";
import {
  IoCalendarSharp,
  IoTimeOutline,
  IoCheckmarkCircle,
  IoAddCircle,
  IoCloseCircle,
} from "react-icons/io5";
import { FaLocationDot, FaCheck } from "react-icons/fa6";
import { FaUsers } from "react-icons/fa";
import { useNavigate } from "react-router-dom";

import CommonButton from "../../components/CommonButton";
import "./event.css";
import { GlobalStyles } from "../../store/styles/GlobalStyles";
import { QRCodeIcon } from "../../components/Icons/SVGIcon";
import EventRegisterSelector from "../../componentsGiba/EventRegisterSelector";
import EventInvitationConfirmDrawer from "../../componentsGiba/EventInvitationConfirmDrawer";
import dfData from "../../common/DefaultConfig.json";
import { formatDateTime } from "../../utils/dateFormatter";
import { useRecoilValue } from "recoil";
import { isRegister, userMembershipInfo } from "../../recoil/RecoilState";

// Form status enum for guest invitation (sync with EventInvitationsSection)
const FormStatus = {
  Invited: 0,           // Được mời nhưng chưa xác nhận
  Approved: 1,          // Admin đã duyệt -> có checkInCode
  Rejected: 2,          // Bị từ chối
  Cancelled: 3,         // Hủy
  WaitingApproval: 5,   // Đã xác nhận, đợi admin duyệt
};

const EventItem = ({
  item,
  titleMaxLines = 2,
  customRegisterText = null,
  customRegisterDisabled = false,
  onInvitationConfirmSuccess = null,
  hideGuestListButton = false,
}) => {
  const navigate = useNavigate();
  const snackbar = useSnackbar();
  const isLoggedIn = useRecoilValue(isRegister);
  const membershipInfo = useRecoilValue(userMembershipInfo);
  const [showRegisterSelector, setShowRegisterSelector] = useState(false);

  // Helper function để lấy message phù hợp với trạng thái user
  const getLoginMessage = () => {
    if (membershipInfo?.approvalStatus === 0) {
      return "Tài khoản đang chờ duyệt, vui lòng đợi admin phê duyệt";
    }
    if (membershipInfo?.approvalStatus === 2) {
      return "Tài khoản đã bị từ chối, vui lòng liên hệ admin";
    }
    return "Vui lòng đăng nhập để đăng ký sự kiện";
  };

  // Check xem user có thể đăng ký không (đã login và được duyệt)
  const canRegister = isLoggedIn && membershipInfo?.approvalStatus === 1;
  const [showInvitationConfirm, setShowInvitationConfirm] = useState(false);

  const getBannerUrl = (banner) => {
    if (!banner) return DefaultImage;
    if (banner.startsWith("http://") || banner.startsWith("https://")) {
      return banner;
    }
    if (banner.startsWith("/")) {
      const fullUrl = `${dfData.domain}${banner}`;
      return fullUrl;
    }
    // Trường hợp khác, trả về banner như cũ
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

  const renderStatusButton = (item, navigate) => {
    if (item?.status === 1) {
      if (item?.isRegister && item?.checkInCode) {
        return (
          <div style={{ display: "flex", gap: "8px", flexDirection: "column", minHeight: hideGuestListButton ? "auto" : "96px", justifyContent: "flex-end" }}>
            {!item?.guestListId && !hideGuestListButton && (
              <CommonButton
                leftIcon={<FaUsers size={16} color="white" />}
                text="Đăng ký danh sách khách mời"
                onClick={(e) => {
                  e.stopPropagation();
                  if (!canRegister) {
                    snackbar.openSnackbar({
                      type: "warning",
                      text: getLoginMessage(),
                      duration: 3000,
                    });
                    return;
                  }
                  navigate("/giba/event-guest-register", {
                    state: { eventId: item?.id },
                  });
                }}
                style={{
                  background: "#9C010B",
                  border: "none",
                  fontSize: 15,
                  height: 40,
                }}
              />
            )}
            <CommonButton
              leftIcon={<IoCheckmarkCircle size={18} color="white" />}
              text="Đã đăng ký tham gia"
              onClick={(e) => {
                e.stopPropagation();
                navigate(`/giba/event-detail/${item?.id}`, { state: { eventData: item } });
              }}
              style={{
                background: "#919191",
                border: "none",
                fontSize: 15,
                height: 40,
              }}
            />
          </div>
        );
      }
      
      // Nếu đã đăng ký nhưng đang đợi duyệt
      if (item?.isRegister && item?.needApproval) {
        return (
          <div style={{ display: "flex", gap: "8px", flexDirection: "column", minHeight: hideGuestListButton ? "48px" : "96px", justifyContent: "flex-end" }}>
            {/* Chỉ hiển thị nút đăng ký danh sách khách mời nếu KHÔNG phải từ lời mời và không bị ẩn */}
            {!item?.guestListId && !hideGuestListButton && (
              <CommonButton
                leftIcon={<FaUsers size={16} color="white" />}
                text="Đăng ký danh sách khách mời"
                onClick={(e) => {
                  e.stopPropagation();
                  if (!canRegister) {
                    snackbar.openSnackbar({
                      type: "warning",
                      text: getLoginMessage(),
                      duration: 3000,
                    });
                    return;
                  }
                  navigate("/giba/event-guest-register", {
                    state: { eventId: item?.id },
                  });
                }}
                style={{
                  background: "#9C010B",
                  border: "none",
                  fontSize: 15,
                  height: 40,
                }}
              />
            )}
            <CommonButton
              leftIcon={<IoTimeOutline size={18} color="white" />}
              text="Đang đợi duyệt"
              onClick={(e) => {
                e.stopPropagation();
                navigate(`/giba/event-detail/${item?.id}`, { state: { eventData: item } });
              }}
              style={{
                background: "#94A3B8",
                border: "none",
                fontSize: 15,
                height: 40,
              }}
            />
          </div>
        );
      }
      
      // Xử lý cho lời mời (có guestListId và formStatus) - STATUS 1
      if (item?.guestListId && item?.formStatus !== undefined) {
        const formStatus = item.formStatus;
        
        // formStatus = 0: Được mời nhưng chưa xác nhận
        if (formStatus === FormStatus.Invited) {
          return (
            <div style={{ display: "flex", gap: "8px", flexDirection: "column", minHeight: hideGuestListButton ? "auto" : "96px", justifyContent: "flex-end" }}>
              <CommonButton
                leftIcon={<IoAddCircle size={18} color="white" />}
                text={customRegisterText || "Xác nhận tham gia"}
                onClick={(e) => {
                  e.stopPropagation();
                  setShowInvitationConfirm(true);
                }}
                style={{
                  background: "#558B2F",
                  border: "none",
                  fontSize: 15,
                  height: 40,
                }}
              />
            </div>
          );
        }
        
        // formStatus = 5: Đã xác nhận, đợi admin duyệt
        if (formStatus === FormStatus.WaitingApproval) {
          return (
            <div style={{ display: "flex", gap: "8px", flexDirection: "column", minHeight: hideGuestListButton ? "auto" : "96px", justifyContent: "flex-end" }}>
              <CommonButton
                leftIcon={<IoTimeOutline size={18} color="white" />}
                text={customRegisterText || "Đang đợi duyệt"}
                onClick={(e) => {
                  e.stopPropagation();
                  navigate(`/giba/event-detail/${item?.id}`, { state: { eventData: item } });
                }}
                style={{
                  background: "#94A3B8",
                  border: "none",
                  fontSize: 15,
                  height: 40,
                }}
              />
            </div>
          );
        }
        
        // formStatus = 1: Admin đã duyệt, có checkInCode
        if (formStatus === FormStatus.Approved && item?.checkInCode) {
          return (
            <div style={{ display: "flex", gap: "8px", flexDirection: "column", minHeight: hideGuestListButton ? "auto" : "96px", justifyContent: "flex-end" }}>
              <CommonButton
                leftIcon={<IoCheckmarkCircle size={18} color="white" />}
                text="Đã đăng ký tham gia"
                onClick={(e) => {
                  e.stopPropagation();
                  navigate(`/giba/event-detail/${item?.id}`, { state: { eventData: item } });
                }}
                style={{
                  background: "#919191",
                  border: "none",
                  fontSize: 15,
                  height: 40,
                }}
              />
            </div>
          );
        }
        
        // formStatus = 2: Bị từ chối
        if (formStatus === FormStatus.Rejected) {
          return (
            <div style={{ display: "flex", gap: "8px", flexDirection: "column", minHeight: hideGuestListButton ? "auto" : "96px", justifyContent: "flex-end" }}>
              <CommonButton
                leftIcon={<IoCloseCircle size={18} color="white" />}
                text={customRegisterText || "Bị từ chối"}
                onClick={(e) => e.stopPropagation()}
                style={{
                  background: "#DC2626",
                  border: "none",
                  fontSize: 15,
                  height: 40,
                  opacity: 0.7,
                }}
              />
            </div>
          );
        }
        
        // formStatus = 3: Đã hủy
        if (formStatus === FormStatus.Cancelled) {
          return (
            <div style={{ display: "flex", gap: "8px", flexDirection: "column", minHeight: hideGuestListButton ? "auto" : "96px", justifyContent: "flex-end" }}>
              <CommonButton
                leftIcon={<IoCloseCircle size={18} color="white" />}
                text={customRegisterText || "Đã hủy"}
                onClick={(e) => e.stopPropagation()}
                style={{
                  background: "#6B7280",
                  border: "none",
                  fontSize: 15,
                  height: 40,
                  opacity: 0.7,
                }}
              />
            </div>
          );
        }
      }
      
      // Fallback cho guestListId không có formStatus (legacy)
      if (item?.guestListId) {
        return (
          <div style={{ display: "flex", gap: "8px", flexDirection: "column", minHeight: hideGuestListButton ? "auto" : "96px", justifyContent: "flex-end" }}>
            <CommonButton
              leftIcon={<IoAddCircle size={18} color="white" />}
              text="Xác nhận tham gia"
              onClick={(e) => {
                e.stopPropagation();
                setShowInvitationConfirm(true);
              }}
              style={{
                background: "#558B2F",
                border: "none",
                fontSize: 15,
                height: 40,
              }}
            />
          </div>
        );
      }
      
      return (
        <div style={{ display: "flex", gap: "8px", flexDirection: "column", minHeight: hideGuestListButton ? "auto" : "96px", justifyContent: "flex-end" }}>
          {!hideGuestListButton && (
            <CommonButton
              leftIcon={<FaUsers size={16} color="white" />}
              text="Đăng ký danh sách khách mời"
              onClick={(e) => {
                e.stopPropagation();
                if (!canRegister) {
                  snackbar.openSnackbar({
                    type: "warning",
                    text: getLoginMessage(),
                    duration: 3000,
                  });
                  return;
                }
                navigate("/giba/event-guest-register", {
                  state: { eventId: item?.id },
                });
              }}
              style={{
                background: "#9C010B",
                border: "none",
                fontSize: 15,
                height: 40,
              }}
            />
          )}
          <CommonButton
            leftIcon={<IoAddCircle size={18} color="white" />}
            text={customRegisterText || "Đăng ký tham gia"}
            onClick={(e) => {
              e.stopPropagation();
              if (!canRegister) {
                snackbar.openSnackbar({
                  type: "warning",
                  text: getLoginMessage(),
                  duration: 3000,
                });
                return;
              }
              navigate("/giba/event-register", {
                state: { eventId: item?.id },
              });
            }}
            style={{
              background: "#558B2F",
              border: "none",
              fontSize: 15,
              height: 40,
            }}
          />
        </div>
      );
    }

    if (item?.status === 2) {
      if (item?.isCheckIn) {
        return (
          <div style={{ minHeight: hideGuestListButton ? "auto" : "96px", display: "flex", alignItems: "flex-end" }}>
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
      }
      
      // Xử lý cho lời mời (có guestListId và formStatus) - STATUS 2
      if (item?.guestListId && item?.formStatus !== undefined) {
        const formStatus = item.formStatus;
        
        // formStatus = 0: Được mời nhưng chưa xác nhận
        if (formStatus === FormStatus.Invited) {
          return (
            <div style={{ display: "flex", gap: "8px", flexDirection: "column", minHeight: hideGuestListButton ? "auto" : "96px", justifyContent: "flex-end" }}>
              <CommonButton
                leftIcon={<IoAddCircle size={18} color="white" />}
                text={customRegisterText || "Xác nhận tham gia"}
                onClick={(e) => {
                  e.stopPropagation();
                  setShowInvitationConfirm(true);
                }}
                style={{
                  background: "#558B2F",
                  border: "none",
                  fontSize: 15,
                  height: 40,
                }}
              />
            </div>
          );
        }
        
        // formStatus = 5: Đã xác nhận, đợi admin duyệt
        if (formStatus === FormStatus.WaitingApproval) {
          return (
            <div style={{ display: "flex", gap: "8px", flexDirection: "column", minHeight: hideGuestListButton ? "auto" : "96px", justifyContent: "flex-end" }}>
              <CommonButton
                leftIcon={<IoTimeOutline size={18} color="white" />}
                text={customRegisterText || "Đang đợi duyệt"}
                onClick={(e) => {
                  e.stopPropagation();
                  navigate(`/giba/event-detail/${item?.id}`, { state: { eventData: item } });
                }}
                style={{
                  background: "#94A3B8",
                  border: "none",
                  fontSize: 15,
                  height: 40,
                }}
              />
            </div>
          );
        }
        
        // formStatus = 1: Admin đã duyệt, có checkInCode -> QR Check-in
        if (formStatus === FormStatus.Approved && item?.checkInCode) {
          return (
            <div style={{ display: "flex", gap: "8px", flexDirection: "column", minHeight: hideGuestListButton ? "auto" : "96px", justifyContent: "flex-end" }}>
              <CommonButton
                leftIcon={<QRCodeIcon />}
                text={customRegisterText || "QR Check-in"}
                onClick={(e) => {
                  e.stopPropagation();
                  navigate("/check-in", { state: { checkInCode: item?.checkInCode } });
                }}
                style={{
                  background: "#0069F3",
                  border: "none",
                  fontSize: 15,
                  height: 40,
                }}
              />
            </div>
          );
        }
        
        // formStatus = 2: Bị từ chối
        if (formStatus === FormStatus.Rejected) {
          return (
            <div style={{ display: "flex", gap: "8px", flexDirection: "column", minHeight: hideGuestListButton ? "auto" : "96px", justifyContent: "flex-end" }}>
              <CommonButton
                leftIcon={<IoCloseCircle size={18} color="white" />}
                text={customRegisterText || "Bị từ chối"}
                onClick={(e) => e.stopPropagation()}
                style={{
                  background: "#DC2626",
                  border: "none",
                  fontSize: 15,
                  height: 40,
                  opacity: 0.7,
                }}
              />
            </div>
          );
        }
        
        // formStatus = 3: Đã hủy
        if (formStatus === FormStatus.Cancelled) {
          return (
            <div style={{ display: "flex", gap: "8px", flexDirection: "column", minHeight: hideGuestListButton ? "auto" : "96px", justifyContent: "flex-end" }}>
              <CommonButton
                leftIcon={<IoCloseCircle size={18} color="white" />}
                text={customRegisterText || "Đã hủy"}
                onClick={(e) => e.stopPropagation()}
                style={{
                  background: "#6B7280",
                  border: "none",
                  fontSize: 15,
                  height: 40,
                  opacity: 0.7,
                }}
              />
            </div>
          );
        }
      }
      
      // Fallback cho guestListId không có formStatus (legacy)
      if (item?.guestListId) {
        return (
          <div style={{ display: "flex", gap: "8px", flexDirection: "column", minHeight: hideGuestListButton ? "auto" : "96px", justifyContent: "flex-end" }}>
            <CommonButton
              leftIcon={<IoAddCircle size={18} color="white" />}
              text="Xác nhận tham gia"
              onClick={(e) => {
                e.stopPropagation();
                setShowInvitationConfirm(true);
              }}
              style={{
                background: "#558B2F",
                border: "none",
                fontSize: 15,
                height: 40,
              }}
            />
          </div>
        );
      }
      
      return (
        <div style={{ display: "flex", gap: "8px", flexDirection: "column", minHeight: hideGuestListButton ? "auto" : "96px", justifyContent: "flex-end" }}>
          {item?.isRegister && !hideGuestListButton && (
            <CommonButton
              leftIcon={<FaUsers size={16} color="white" />}
              text="Đăng ký danh sách khách mời"
              onClick={(e) => {
                e.stopPropagation();
                if (!canRegister) {
                  snackbar.openSnackbar({
                    type: "warning",
                    text: getLoginMessage(),
                    duration: 3000,
                  });
                  return;
                }
                navigate("/giba/event-guest-register", {
                  state: { eventId: item?.id },
                });
              }}
              style={{
                background: "#9C010B",
                border: "none",
                fontSize: 15,
                height: 40,
              }}
            />
          )}
          {item?.isRegister && item?.checkInCode ? (
            <CommonButton
              leftIcon={<QRCodeIcon />}
              text="QR Check-in"
              onClick={(e) => {
                e.stopPropagation();
                navigate("/check-in", { state: { checkInCode: item?.checkInCode } });
              }}
              style={{
                background: "#0069F3",
                border: "none",
                fontSize: 15,
                height: 40,
              }}
            />
          ) : item?.isRegister && item?.needApproval ? (
            <CommonButton
              leftIcon={<IoTimeOutline size={18} />}
              text="Đang đợi duyệt"
              onClick={(e) => {
                e.stopPropagation();
                navigate(`/giba/event-detail/${item?.id}`, { state: { eventData: item } });
              }}
              style={{
                background: "#94A3B8",
                border: "none",
                fontSize: 15,
                height: 40,
              }}
            />
          ) : !item?.isRegister ? (
            <>
              <CommonButton
                leftIcon={<FaUsers size={16} />}
                text="Đăng ký danh sách khách mời"
                onClick={(e) => {
                  e.stopPropagation();
                  if (!canRegister) {
                    snackbar.openSnackbar({
                      type: "warning",
                      text: getLoginMessage(),
                      duration: 3000,
                    });
                    return;
                  }
                  navigate("/giba/event-guest-register", {
                    state: { eventId: item?.id },
                  });
                }}
                style={{
                  background: "#9C010B",
                  border: "none",
                  fontSize: 15,
                  height: 40,
                }}
              />
              <CommonButton
                leftIcon={<IoAddCircle size={18} />}
                text={customRegisterText || "Đăng ký tham gia"}
                onClick={(e) => {
                  e.stopPropagation();
                  if (!canRegister) {
                    snackbar.openSnackbar({
                      type: "warning",
                      text: getLoginMessage(),
                      duration: 3000,
                    });
                    return;
                  }
                  navigate("/giba/event-register", {
                    state: { eventId: item?.id },
                  });
                }}
                style={{
                  background: "#558B2F",
                  border: "none",
                  fontSize: 15,
                  height: 40,
                }}
              />
            </>
          ) : null}
        </div>
      );
    }

    if (item?.status === 3) {
      return (
        <div style={{ minHeight: hideGuestListButton ? "auto" : "96px", display: "flex", alignItems: "flex-end" }}>
          <CommonButton
            leftIcon={<IoCloseCircle size={18} color="white" />}
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
      onClick={() => navigate(`/giba/event-detail/${item?.id}`, { state: { eventData: item } })}
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
            {`${formatDateTimeDisplay(
              item?.startTime
            )} - ${formatDateTimeDisplay(item?.endTime)}`}
          </Box>
        </Box>
        <Box className="item-content-event">
          <FaLocationDot size={20} />
          {item?.address}
        </Box>
        <Box>{renderStatusButton(item, navigate)}</Box>
      </Box>

      <EventRegisterSelector
        visible={showRegisterSelector}
        onClose={() => {
          setShowRegisterSelector(false);
        }}
        eventId={item?.id}
        isPersonalRegistered={item?.isRegister}
        onSuccess={() => {
          setShowRegisterSelector(false);
        }}
      />

      {item?.guestListId && (
        <EventInvitationConfirmDrawer
          visible={showInvitationConfirm}
          onClose={() => {
            setShowInvitationConfirm(false);
          }}
          eventId={item?.id}
          guestListId={item.guestListId}
          onSuccess={() => {
            setShowInvitationConfirm(false);
            onInvitationConfirmSuccess?.();
          }}
        />
      )}
    </Box>
  );
};
export default memo(EventItem);
