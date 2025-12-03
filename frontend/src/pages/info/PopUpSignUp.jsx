import React from "react";
import { useRecoilState } from "recoil";
import { Box, Modal, useNavigate } from "zmp-ui";
import { actionTab } from "../../recoil/RecoilState";

const PopUpSignUp = ({ visible, onClose }) => {
  const [activeTab, setActiveTab] = useRecoilState(actionTab);
  const navigate = useNavigate();

  return (
    <Modal width="90%" visible={visible} onClose={onClose}>
      <Box
        mt={3}
        textAlign="center"
        style={{ fontWeight: "bold", fontSize: 22 }}
      >
        Thông báo
      </Box>
      <Box mt={3} textAlign="center">
        Đăng ký thành viên để tiếp tục?
      </Box>

      <Box
        mt={5}
        style={{
          display: "flex",
          justifyContent: "center",
          alignItems: "center",
          gap: 10,
        }}
      >
        <Box
          className="btn"
          style={{
            background: "var(--primary-background-color)",
            fontWeight: "bold",
            width: "100px",
          }}
          onClick={onClose}
        >
          Hủy
        </Box>
        <Box
          className="btn"
          style={{
            background: "green",
            color: "#fff",
            fontWeight: "bold",
            width: "100px",
          }}
          onClick={() => {
            //setActiveTab("me");
            navigate("/register/default");
          }}
        >
          Tiếp tục
        </Box>
      </Box>
    </Modal>
  );
};

export default PopUpSignUp;
