import React, { useEffect } from "react";
import { Page, Box, useNavigate } from "zmp-ui";
import useSetHeader from "../../components/hooks/useSetHeader";
import { useLocation } from "react-router-dom";

export default function SuccessJoinEvent(props) {
  const navigate = useNavigate();
  const setHeader = useSetHeader();
  const location = useLocation();
  const query = new URLSearchParams(location.search);
  const defaultScreenParam = query.get("default");
  
  // Get status from location state
  const registrationStatus = location.state?.status;

  useEffect(() => {
    setHeader({
      hasLeftIcon: true,
      type: "secondary",
      title: "ĐĂNG KÝ THÀNH CÔNG",
      customTitle: false,
      route: defaultScreenParam ? "/home" : null,
    });
  }, [defaultScreenParam]);

  const doneImg = "https://congdongbau.incom.vn/images/miniapp/event/done.jpg";

  // Determine message based on status
  const getMessage = () => {
    if (registrationStatus === 0) {
      // Chờ xử lý - cần duyệt
      return "Cảm ơn bạn đã đăng ký! Đơn đăng ký của bạn đang chờ admin phê duyệt. Chúng tôi sẽ thông báo cho bạn sau khi đơn được xử lý.";
    } else if (registrationStatus === 1) {
      // Đã duyệt - không cần duyệt
      return "Cảm ơn bạn đã đăng ký! Đăng ký của bạn đã được xác nhận thành công. Chúng tôi sẽ liên hệ với bạn nếu có thông tin bổ sung.";
    } else {
      // Default message
      return "Cảm ơn bạn đã đăng ký sự kiện! Chúng tôi sẽ liên lạc lại với bạn để xác nhận.";
    }
  };

  return (
    <Page
      className="page"
      style={{
        paddingTop: 50,
      }}
    >
      <Box
        className="divCenter"
        style={{
          marginTop: "10vh",
        }}
      >
        <img
          src={doneImg}
          style={{
            objectFit: "contain",
            width: "70vw",
          }}
        ></img>
      </Box>

      <Box
        className="divCenter"
        style={{
          fontSize: 18,
          fontWeight: 700,
          flexDirection: "column",
          padding: 8,
          textAlign: "center",
        }}
      >
        {getMessage()}
      </Box>
      <Box
        style={{
          display: "flex",
          flexDirection: "column",
          alignItems: "center",
          justifyContent: "center",
          width: "100%",
          textAlign: "center",
          gap: 10,
        }}
      >
        {/* <Box
          className="button-common"
          style={{
            width: "90%",
            display: "flex",
            justifyContent: "center",
            cursor: "pointer",
            fontSize: 18,
          }}
          onClick={() => {
            navigate("/event", { animate: false });
          }}
        >
          Quay lại danh sách sự kiện
        </Box> */}
        <Box
          style={{
            border: "1px solid var(--primary-color)",
            width: "90%",
            display: "flex",
            justifyContent: "center",
            padding: "4px 0",
            borderRadius: "5px",
            color: "var(--primary-color)",
            fontWeight: "bold",
            cursor: "pointer",
          }}
          onClick={() => {
            navigate("/giba/event-registration-history", { animate: false });
          }}
        >
          Xem lịch sử đăng ký
        </Box>
        <Box
          style={{
            border: "1px solid var(--primary-color)",
            width: "90%",
            display: "flex",
            justifyContent: "center",
            padding: "4px 0",
            borderRadius: "5px",
            color: "var(--primary-color)",
            fontWeight: "bold",
            cursor: "pointer",
          }}
          onClick={() => {
            navigate("/home", { animate: false });
          }}
        >
         Về Home
        </Box>
      </Box>
    </Page>
  );
}
