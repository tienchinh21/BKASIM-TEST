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
  const { status } = location.state || {};

  useEffect(() => {
    setHeader({
      hasLeftIcon: true,
      type: "secondary",
      title: "ĐĂNG KÝ THÀNH CÔNG",
      customTitle: false,
      route: "/giba",
    });
  }, [defaultScreenParam]);

  const doneImg = "https://congdongbau.incom.vn/images/miniapp/event/done.jpg";

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
          marginTop: "20px",
        }}
      >
        {status === 0
          ? "Đăng ký thành công! Vui lòng đợi admin duyệt đơn."
          : "Cảm ơn Quý khách đã đăng ký, nhân viên chúng tôi sẽ liên lạc lại Quý khách để xác nhận."}
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
          marginTop: "20px",
        }}
      >
        <Box
          style={{
            border: "1px solid var(--primary-color)",
            width: "90%",
            display: "flex",
            justifyContent: "center",
            padding: "12px 0",
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
            padding: "12px 0",
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
