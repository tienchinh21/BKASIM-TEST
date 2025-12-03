import React, { useEffect } from "react";
import useSetHeader from "../../components/hooks/useSetHeader";
import { Box, Page } from "zmp-ui";
import { useLocation, useNavigate } from "react-router-dom";
import { QRCode } from "zmp-qrcode";
import { Col, Row } from "antd";
import CommonButton from "../../components/CommonButton";

const EventCheckIn = () => {
  const setHeader = useSetHeader();
  const navigate = useNavigate();
  const location = useLocation();
  const query = new URLSearchParams(location.search);
  const defaultScreenParam = query.get("default");
  const checkInCode = location.state?.checkInCode;

  useEffect(() => {
    setHeader({
      hasLeftIcon: true,
      type: "secondary",
      title: "THÔNG TIN CHECK-IN",
      customTitle: false,
      route: defaultScreenParam ? "/home" : null,
    });
  }, [defaultScreenParam]);

  return (
    <Page
      className="divCenter"
      flexDirection="column"
      style={{
        paddingTop: 50,
        padding: 14,
      }}
    >
      <Row gutter={[16, 16]} style={{ marginBottom: "70px" }}>
        <Col span={24}>
          <Box
            style={{
              fontWeight: "bold",
              textAlign: "center",
              fontSize: 20,
            }}
          >
            QR Check-in của bạn
          </Box>
        </Col>
        <Col span={24}>
          <Box
            flex
            justifyContent="center"
            alignItems="center"
            style={{
              background: "#fff",
              border: "2px dashed #BDBDBD",
              borderRadius: 12,
              padding: 20,
              minWidth: 200,
              minHeight: 300,
              margin: "0 auto 0 auto",
              boxSizing: "border-box",
            }}
          >
            <QRCode
              value={checkInCode || "Không có mã"}
              size={250}
              color="#000000"
              backgroundColor="#ffffff"
            />
          </Box>
        </Col>
        {checkInCode && (
          <Col span={24}>
            <Box
              style={{
                textAlign: "center",
                marginTop: 16,
                padding: "12px 20px",
                background: "#f5f5f5",
                borderRadius: 8,
                border: "1px solid #ddd",
              }}
            >
              <Box style={{ fontSize: 14, color: "#666", marginBottom: 4 }}>
                Mã check-in của bạn:
              </Box>
              <Box style={{ fontSize: 16, fontWeight: "bold", color: "#333" }}>
                {checkInCode}
              </Box>
            </Box>
          </Col>
        )}
      </Row>
      <CommonButton
        isFixedBottom
        text="Quay lại"
        onClick={() => navigate(-1)}
      />
    </Page>
  );
};

export default EventCheckIn;
