import React, { useEffect, useRef, useState } from "react";
import { useLocation, useNavigate, useParams } from "react-router-dom";
import useSetHeader from "../../../components/hooks/useSetHeader";
import { Box, Page, Swiper } from "zmp-ui";
import EventInfo from "./EventInfo";
import PopUpJoinEvent from "./PopUpJoinEvent";
import dfData from "../../../common/DefaultConfig.json";
import "../event.css";
import Sponsor from "./sponsor";
import Loading from "../../../components/loading/Loading";
import { toast } from "react-toastify";
import axios from "axios";
import { isRegister, token } from "../../../recoil/RecoilState";
import { useRecoilValue } from "recoil";
import CommonButton from "../../../components/CommonButton";
import CommonShareModal from "../../../components/CommonShareModal";
import { APP_MODE } from "../../../state";
import { GlobalStyles } from "../../../store/styles/GlobalStyles";
import PopUpSignUp from "../../../pages/info/PopUpSignUp";
import EventRegisterSelector from "../../../componentsGiba/EventRegisterSelector";
import EventInvitationConfirmDrawer from "../../../componentsGiba/EventInvitationConfirmDrawer";
const EventDetail = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const setHeader = useSetHeader();
  const joinEventRef = useRef();
  const shareModalRef = useRef();

  const tokenAuth = useRecoilValue(token);
  const location = useLocation();
  const isRegistered = useRecoilValue(isRegister);
  const query = new URLSearchParams(location.search);
  const defaultScreenParam = query.get("default");

  const [loading, setLoading] = useState(false);
  const [detail, setDetail] = useState(null);
  const [activeTab, setActiveTab] = useState("info");
  const [modalSignUpVisible, setModalSignUpVisible] = useState(false);
  const [showRegisterSelector, setShowRegisterSelector] = useState(false);
  const [showInvitationConfirm, setShowInvitationConfirm] = useState(false);

  const env = APP_MODE;

  const eventDataFromState = location.state?.eventData;
  const fromGuestList = location.state?.fromGuestList;

  useEffect(() => {
    setHeader({
      hasLeftIcon: true,
      type: "secondary",
      title: "CHI TIẾT SỰ KIỆN",
      customTitle: false,
      route: defaultScreenParam ? "/home" : null,
    });

    const timeout = setTimeout(() => {
      // Always use state data if available
      if (eventDataFromState) {
        console.log("Using event data from state:", eventDataFromState);
        setDetail(eventDataFromState);
      } else {
        // Only fetch from API if no state data
        getDetailEvent();
      }
    }, 200);

    return () => clearTimeout(timeout);
  }, [defaultScreenParam, tokenAuth, id, eventDataFromState]);

  const getDetailEvent = () => {
    if (!eventDataFromState) {
      toast.error("Không có dữ liệu sự kiện");
    }
  };

  const handleCancelRegister = (id) => {
    axios
      .delete(
        `${dfData.domain}/api/EventRegistrations/Cancel/${id}`,
        {
          headers: { Authorization: `Bearer ${tokenAuth}` },
        }
      )
      .then((res) => {
        if (res.data.code === 0) {
          toast.success("Huỷ đăng ký sự kiện thành công!");
          navigate("/event");
        } else {
          toast.error(res.data.message);
        }
      })
      .catch((err) => {
        console.error("Huỷ đăng ký thất bại:", err);
        toast.error("Huỷ đăng ký thất bại. Vui lòng thử lại!");
      });
  };

  const renderTabContent = () => {
    if (!detail) return null;

    switch (activeTab) {
      case "info":
        return <EventInfo detail={detail} />;
      case "sponsor":
        return (
          <>
            {detail?.sponsorshipTiers?.length > 0 ? (
              detail.sponsorshipTiers.map((tier, index) => (
                <Sponsor key={index} tiers={tier} />
              ))
            ) : (
              <Box
                style={{
                  textAlign: "center",
                  padding: "40px 20px",
                  color: "#666",
                  fontSize: 14,
                }}
              >
                Chưa có nhà tài trợ
              </Box>
            )}
          </>
        );
      default:
        return null;
    }
  };

  const getTabStyle = (tabKey) => ({
    padding: "2px 20px",
    backgroundColor: activeTab === tabKey ? "#003d82" : "transparent",
    color: activeTab === tabKey ? "#fff" : "#000",
    borderRadius: GlobalStyles.borderRadius.xs,
    transition: "all 0.3s",
  });

  const getStatusStyle = (status) => {
    const baseStyle = {
      color: "white",
      padding: "4px 20px",
      borderRadius: GlobalStyles.borderRadius.xs,
      fontSize: GlobalStyles.fontSize.sm,
      fontWeight: "bold",
      display: "inline-block",
    };

    let background;
    if (status === 2) {
      return {
        ...baseStyle,
      };
    } else if (status === 3) {
      background = "var(--five-background-color)";
    } else {
      background = "#CCCCCC";
    }

    return {
      ...baseStyle,
      background,
    };
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

  const renderActionButtons = () => {
    if (!detail) return null;

    const {
      status,
      isRegister,
      checkInCode,
      checkInStatus,
      id,
      guestListId,
      formStatus,
    } = detail;

    const handleJoin = () => {
      // If guest invitation (has guestListId and formStatus = 0), show invitation confirm drawer
      if (guestListId && formStatus === 0) {
        setShowInvitationConfirm(true);
      } else if (isRegistered) {
        setShowRegisterSelector(true);
      } else {
        setModalSignUpVisible(true);
      }
    };

    const ButtonRow = ({ children }) => (
      <Box className="action-btn-row">{children}</Box>
    );

    const createButton = (text, onClick, variant = "primary", style = {}) => (
      <CommonButton
        text={text}
        onClick={onClick}
        variant={variant}
        style={{ width: "48%", height: 35, ...style }}
      />
    );

    // If has guestListId (invited as guest), use guest logic
    if (guestListId) {
      // Status 1: Sắp diễn ra
      if (status === 1) {
        return (
          <ButtonRow>
            {createButton("Chia sẻ sự kiện", () => shareEvent(), "outline")}
            {formStatus === 0 ? (
              // formStatus = 0: Chưa xác nhận -> Show button "Xác nhận tham gia"
              createButton("Xác nhận tham gia", handleJoin, "primary")
            ) : formStatus === 5 ? (
              // formStatus = 5: Đã xác nhận, đang đợi duyệt
              createButton("Đang đợi duyệt", null, "primary", {
                background: "#94A3B8",
                border: "none",
                cursor: "default",
                width: "48%",
              })
            ) : checkInCode ? (
              // formStatus = 1: Đã được duyệt
              createButton("Đã xác nhận tham gia", null, "primary", {
                background: "#919191",
                border: "none",
                cursor: "default",
                width: "48%",
              })
            ) : (
              createButton("Đang đợi duyệt", null, "primary", {
                background: "#94A3B8",
                border: "none",
                cursor: "default",
                width: "48%",
              })
            )}
          </ButtonRow>
        );
      }

      // Status 2: Đang diễn ra
      if (status === 2) {
        return (
          <ButtonRow>
            {createButton("Chia sẻ sự kiện", () => shareEvent(), "outline")}
            {formStatus === 0 ? (
              // formStatus = 0: Chưa xác nhận -> Show button "Xác nhận tham gia"
              createButton("Xác nhận tham gia", handleJoin, "primary")
            ) : formStatus === 5 ? (
              // formStatus = 5: Đã xác nhận, đang đợi duyệt
              createButton("Đang đợi duyệt", null, "primary", {
                background: "#94A3B8",
                border: "none",
                cursor: "default",
                width: "48%",
              })
            ) : checkInCode ? (
              checkInStatus === 2 ? (
                createButton("Đã check-in", null, "disabled")
              ) : (
                createButton("Check-in Sự kiện", () =>
                  navigate("/check-in", {
                    state: { checkInCode },
                  })
                )
              )
            ) : (
              createButton("Đang đợi duyệt", null, "primary", {
                background: "#94A3B8",
                border: "none",
                cursor: "default",
                width: "48%",
              })
            )}
          </ButtonRow>
        );
      }

      // Status 3: Đã kết thúc
      if (status === 3) {
        return (
          <ButtonRow>
            {createButton("Sự kiện đã kết thúc", null, "primary", {
              border: "none",
              background: "var(--five-background-color)",
              width: "100%",
            })}
          </ButtonRow>
        );
      }

      return null;
    }

    // Original logic for regular members
    // Status 1: Sắp diễn ra
    if (status === 1) {
      return (
        <ButtonRow>
          {createButton("Chia sẻ sự kiện", () => shareEvent(), "outline")}
          {isRegister ? (
            // Đã đăng ký
            checkInCode ? (
              // Có checkInCode -> Đã được duyệt
              createButton("Huỷ đăng ký", () => handleCancelRegister(id), "outline")
            ) : (
              // Không có checkInCode -> Đang đợi duyệt
              createButton("Đang đợi duyệt", null, "primary", {
                background: "#94A3B8",
                border: "none",
                cursor: "default",
                width: "48%",
              })
            )
          ) : (
            // Chưa đăng ký
            createButton("Đăng ký tham gia", handleJoin, "primary")
          )}
        </ButtonRow>
      );
    }

    // Status 2: Đang diễn ra
    if (status === 2) {
      return (
        <ButtonRow>
          {createButton("Chia sẻ sự kiện", () => shareEvent(), "outline")}
          {isRegister ? (
            // Đã đăng ký
            checkInCode ? (
              // Có checkInCode
              checkInStatus === 2 ? (
                // checkInStatus = 2: Đã check-in
                createButton("Đã check-in", null, "disabled")
              ) : (
                // checkInStatus = 1: Chưa check-in -> Hiển thị nút Check-in
                createButton("Check-in Sự kiện", () =>
                  navigate("/check-in", {
                    state: { checkInCode },
                  })
                )
              )
            ) : (
              // Không có checkInCode -> Đang đợi duyệt
              createButton("Đang đợi duyệt", null, "primary", {
                background: "#94A3B8",
                border: "none",
                cursor: "default",
                width: "48%",
              })
            )
          ) : (
            // Chưa đăng ký
            createButton("Đăng ký tham gia", handleJoin, "primary")
          )}
        </ButtonRow>
      );
    }

    // Status 3: Đã kết thúc
    if (status === 3) {
      return (
        <ButtonRow>
          {createButton("Sự kiện đã kết thúc", null, "primary", {
            border: "none",
            background: "var(--five-background-color)",
            width: "100%",
          })}
        </ButtonRow>
      );
    }

    return null;
  };

  const getBannerUrl = (banner) => {
    if (!banner) return null;
    if (banner.startsWith("http://") || banner.startsWith("https://")) {
      return banner;
    }
    if (banner.startsWith("/")) {
      return `${dfData.domain}${banner}`;
    }
    return banner;
  };

  const shareEvent = () => {
    let url = `https://zalo.me/s/${dfData.appid}/giba/event-detail/${detail?.id}`;
    if (env === "TESTING" || env === "DEVELOPMENT") {
      url += `?env=${env}&version=${window.APP_VERSION}&default=true`;
    }

    shareModalRef.current?.open(
      url,
      "Chia sẻ sự kiện",
      "Hãy chia sẻ sự kiện này đến bạn bè nhé!",
      getBannerUrl(detail?.banner)
    );
  };

  if (loading) {
    return (
      <Box
        style={{
          minHeight: 300,
          display: "flex",
          justifyContent: "center",
          alignItems: "center",
        }}
      >
        <Loading />
      </Box>
    );
  }

  return (
    <Page style={{ paddingTop: 50, paddingBottom: 90, background: "#fff" }}>
      {/* Swiper ảnh và tab */}
      <Box style={{ background: "#fff" }}>
        {(() => {
          // Handle both array and string for images
          let imagesList = [];
          if (detail?.images) {
            if (Array.isArray(detail.images)) {
              imagesList = detail.images;
            } else if (typeof detail.images === "string") {
              imagesList = [detail.images];
            }
          }
          // Fallback to banner if no images
          if (imagesList.length === 0 && detail?.banner) {
            imagesList = [detail.banner];
          }

          return imagesList.length > 0 ? (
            <Box style={{ position: "relative" }}>
              <Swiper
                style={{ borderRadius: "0px" }}
                loop={imagesList.length > 1}
                dots={imagesList.length > 1}
              >
                {imagesList.map((img, index) => (
                  <Swiper.Slide key={index}>
                    <div
                      style={{
                        position: "relative",
                        width: "100%",
                        paddingTop: "56.25%",
                        overflow: "hidden",
                      }}
                    >
                      <img
                        style={{
                          position: "absolute",
                          top: 0,
                          left: 0,
                          width: "100%",
                          maxHeight: "27vh",
                          objectFit: "fill",
                        }}
                        src={getBannerUrl(img)}
                        alt={`Slide ${index + 1}`}
                      />
                    </div>
                  </Swiper.Slide>
                ))}
                </Swiper>
              <Box style={{ position: "absolute", top: 10, left: 20 }}>
                <div
                  style={getStatusStyle(detail?.status)}
                  className={detail?.status === 2 ? "gradient-animate" : ""}
                >
                  {getStatusText(detail?.status, detail?.startTime)}
                </div>
              </Box>
              {detail?.type && (
                <Box style={{ position: "absolute", top: 10, right: 20 }}>
                  <div
                    style={{
                      backgroundColor: getEventTypeBadge(detail.type).bgColor,
                      color: getEventTypeBadge(detail.type).textColor,
                      border: `1px solid ${
                        getEventTypeBadge(detail.type).borderColor
                      }`,
                      padding: "4px 12px",
                      borderRadius: "12px",
                      fontSize: "11px",
                      fontWeight: "600",
                      display: "inline-block",
                    }}
                  >
                    {getEventTypeBadge(detail.type).label}
                  </div>
                </Box>
              )}
            </Box>
          ) : null;
        })()}
        {detail && (
          <Box
            style={{
              display: "flex",
              alignItems: "center",
              borderRadius: 8,
              border: "1px solid #333",
              width: "fit-content",
              fontSize: 14,
              margin: "14px",
            }}
          >
            <Box
              style={getTabStyle("info")}
              onClick={() => setActiveTab("info")}
            >
              Thông tin sự kiện
            </Box>
            <Box
              style={getTabStyle("sponsor")}
              onClick={() => setActiveTab("sponsor")}
            >
              Nhà tài trợ
            </Box>
          </Box>
        )}
      </Box>

      <Box
        style={{
          padding: "0 14px",
          background: "#fff",
          overflowY: "auto",
          height: "100%",
        }}
      >
        {loading || !detail ? (
          <Box
            style={{
              minHeight: 300,
              display: "flex",
              justifyContent: "center",
              alignItems: "center",
            }}
          >
            <Loading />
          </Box>
        ) : (
          renderTabContent()
        )}
      </Box>

      <Box
        style={{
          position: "fixed",
          bottom: 0,
          left: 0,
          right: 0,
          padding: "20px 10px",
          background: "#fff",
        }}
      >
        {renderActionButtons()}
      </Box>

      <PopUpJoinEvent ref={joinEventRef} id={detail?.id} />
      <CommonShareModal ref={shareModalRef} />
      <PopUpSignUp
        visible={modalSignUpVisible}
        onClose={() => setModalSignUpVisible(false)}
      />
      <EventRegisterSelector
        visible={showRegisterSelector}
        onClose={() => setShowRegisterSelector(false)}
        eventId={id || ""}
        isPersonalRegistered={detail?.isRegister && detail?.checkInCode}
        onSuccess={() => {
          setShowRegisterSelector(false);
          // Comment: Không gọi getDetailEvent() vì đã dùng state data
          // getDetailEvent();
          toast.success("Đăng ký thành công!");
        }}
      />
      <EventInvitationConfirmDrawer
        visible={showInvitationConfirm}
        onClose={() => setShowInvitationConfirm(false)}
        eventId={detail?.id || ""}
        guestListId={detail?.guestListId || ""}
        onSuccess={() => {
          setShowInvitationConfirm(false);
          toast.success("Xác nhận tham gia thành công!");
          // Reload page or update detail
          if (eventDataFromState) {
            setDetail({ ...detail, formStatus: 5 });
          }
        }}
      />
    </Page>
  );
};

export default EventDetail;
