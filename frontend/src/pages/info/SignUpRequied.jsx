import React, { useEffect, useState, useRef } from "react";
import { Box, Text, useNavigate } from "zmp-ui";
import UserCard from "../../assets/require-signin.png";
import CommonButton from "../../components/CommonButton";
import { GlobalStyles } from "../../store/styles/GlobalStyles";
import { Col, Row } from "antd";
const SignUpRequied = () => {
  const navigate = useNavigate();
  return (
    <Box
      //   className="divCenter"
      style={{
        display: "flex",
        width: "100%",
        height: "90vh",
        padding: GlobalStyles.spacing.md,
      }}
    >
      <Row
        style={{
          height: "100%",
        }}
        justify="space-between"
      >
        <Col
          span={24}
          style={{
            flexDirection: "column",
            display: "flex",
            alignItems: "center",
            justifyContent: "space-evenly",
          }}
        >
          <img
            src={UserCard}
            style={{ height: 240, width: 240, marginTop: 20 }}
            alt=""
          />
          <Box>
            <Text style={{ fontWeight: "bold", fontSize: 17, textAlign: "center" }}>
              Trở thành thành viên để sử dụng tính năng
            </Text>
            <Box
              style={{
                wordBreak: "break-word",
                whiteSpace: "normal",
                overflowWrap: "break-word",
                textAlign: "center",
                fontSize: 15,
                marginTop: 10,
              }}
            >
              Tính năng này cần số điện thoại của bạn để định danh tài khoản
            </Box>
          </Box>
        </Col>
        <Col
          span={24}
          style={{
            display: "flex",
            flexDirection: "column",
            gap: 10,
          }}
        >
          <CommonButton
            text="Đăng ký thành viên"
            onClick={() => navigate("/register/default")}
          />
          <CommonButton
            text="Để sau"
            onClick={() => {
              navigate("/home", { animate: false });
            }}
            variant="outline"
          />
        </Col>
      </Row>
    </Box>
  );
};

export default SignUpRequied;
