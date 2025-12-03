import React, { useEffect, useRef, useState } from "react";
import { useLocation, useNavigate, useParams } from "react-router-dom";
import useSetHeader from "../../../components/hooks/useSetHeader";
import { Box, Page, Swiper } from "zmp-ui";
import EventInfo from "./EventInfo";
import PopUpJoinEvent from "./PopUpJoinEvent";
import dfData from "../../../common/DefaultConfig.json";
import "../event.css";
import Sponsor from "./sponsor";
import PopUpShareEvent from "./PopUpShareEvent";
import Loading from "../../../components/loading/Loading";
import { toast } from "react-toastify";
import axios from "axios";
import { isRegister, token } from "../../../recoil/RecoilState";
import { useRecoilValue } from "recoil";
import PopUpSignUp from "../../info/PopUpSignUp";
import CommonButton from "../../../components/CommonButton";
import CommonShareModal from "../../../components/CommonShareModal";
import { APP_MODE } from "../../../state";
import { Row } from "antd";
import { GlobalStyles } from "../../../store/styles/GlobalStyles";
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

  const env = APP_MODE;

  useEffect(() => {
    setHeader({
      hasLeftIcon: true,
      type: "secondary",
      title: "CHI TIẾT SỰ KIỆN",
      customTitle: false,
      route: defaultScreenParam ? "/home" : null,
    });

    const timeout = setTimeout(() => {
      getDetailEvent();
    }, 200);

    return () => clearTimeout(timeout);
  }, [defaultScreenParam, tokenAuth, id]);

  const getDetailEvent = () => {
    setLoading(true);
    axios
      .get(`${dfData.domain}/api/groups/events/${id}`, {
        headers: { Authorization: `Bearer ${tokenAuth}` },
      })
      .then((response) => {
        setDetail(response.data.data);
        setLoading(false);
      })
      .catch((error) => {
        toast.error("Lấy chi tiết sự kiện không thành công:", error);
        setLoading(false);
      })
      .finally(() => {
        setLoading(false);
      });
  };

  const handleCancelRegister = (id) => {
    axios
      .post(
        `${dfData.domain}/api/EventRegistrations/Cancel/${id}`,
        {},
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
    backgroundColor: activeTab === tabKey ? "#333" : "transparent",
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
      background = "linear-gradient(to right, #21C4A8, #558B2F)";
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

  const renderActionButtons = () => {
    if (!detail) return null;

    const { status, isRegister, isCheckIn, id, checkInCode } = detail;

    const handleJoin = () => {
      if (isRegistered) {
        joinEventRef.current?.open();
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

    if (status === 1) {
      return (
        <ButtonRow>
          {isRegister
            ? [
                createButton(
                  "Huỷ đăng ký",
                  () => handleCancelRegister(id),
                  "outline"
                ),
                createButton("Chia sẻ sự kiện", () => shareEvent(), "outline"),
              ]
            : [
                createButton("Chia sẻ sự kiện", () => shareEvent(), "outline"),
                createButton("Đăng ký tham gia", handleJoin, "primary"),
              ]}
        </ButtonRow>
      );
    }

    if (status === 2) {
      return (
        <ButtonRow>
          {createButton("Chia sẻ sự kiện", () => shareEvent(), "outline")}
          {isRegister
            ? isCheckIn
              ? createButton("Đã check-in", null, "disabled")
              : createButton("Check-in Sự kiện", () =>
                  navigate("/check-in", {
                    state: { checkInCode },
                  })
                )
            : createButton("Đăng ký tham gia", handleJoin, "primary")}
        </ButtonRow>
      );
    }

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

  const shareEvent = () => {
    let url = `https://zalo.me/s/${dfData.appid}/detail-event/${detail?.id}`;
    if (env === "TESTING" || env === "DEVELOPMENT") {
      url += `?env=${env}&version=${window.APP_VERSION}&default=true`;
    }

    shareModalRef.current?.open(
      url,
      "Chia sẻ sự kiện",
      "Hãy chia sẻ sự kiện này đến bạn bè nhé!",
      detail?.banner
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
        {detail?.images?.length > 0 && (
          <Box style={{ position: "relative" }}>
            <Swiper
              style={{ borderRadius: "0px" }}
              loop={detail.images.length > 1}
              dots={detail.images.length > 1}
            >
              {detail.images.map((img, index) => (
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
                      src={img}
                      alt={`Slide ${index + 1}`}
                    />
                  </div>
                </Swiper.Slide>
              ))}
            </Swiper>
            <Box style={{ position: "absolute", top: 10, left: 20 }}>
              <div style={getStatusStyle(detail?.status)}>
                {getStatusText(detail?.status, detail?.startTime)}
              </div>
            </Box>
          </Box>
        )}
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
    </Page>
  );
};

export default EventDetail;
