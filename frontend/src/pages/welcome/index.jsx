import React, { useEffect, useState } from "react";
import { useRecoilValue, useSetRecoilState } from "recoil";
import { Page, Box, useNavigate } from "zmp-ui";
import { useParams } from "react-router";
import { storeCurrent, codeAffiliate } from "../../recoil/RecoilState";
import { isEmpty } from "../../common/Common";
import { toast } from "react-toastify";
import useSetHeader from "../../components/hooks/useSetHeader";
import CustomSelect from "./CustomSelect";

export default function Welcome(props) {
  let { referralCode } = useParams();
  const navigate = useNavigate();
  const setHeader = useSetHeader();
  const barnch = useRecoilValue(storeCurrent);
  const setInputRefCode = useSetRecoilState(codeAffiliate);

  useEffect(() => {
    if (referralCode) {
      setInputRefCode(referralCode);
    } else {
      setInputRefCode("");
    }
  }, [referralCode]);

  const BGWC =
    "https://miniapp-manage.incom.vn/images/gasbinhminh/welcome/BG.png";

  useEffect(() => {
    setHeader({
      hasLeftIcon: false,
      type: "secondary",
      title: "",
      customTitle: false,
      isShowHeader: false,
    });
  }, []);

  const handleClickConfirm = () => {
    if (!isEmpty(barnch)) {
      navigate("/home");
    } else {
      toast.error("Bạn vui lòng chọn cửa hàng để tiếp tục!");
    }
  };

  return (
    <Page
      className="page"
      style={{
        backgroundSize: "100% 100%",
        backgroundImage: `url(${BGWC})`,
        backgroundRepeat: "no-repeat",
      }}
    >
      <Box
        className="divCenter"
        style={{
          marginTop: "28vh",
          fontSize: 30,
          fontWeight: "bold",
        }}
      >
        XIN CHÀO
      </Box>
      <Box
        className="divCenter"
        style={{
          fontSize: 18,
          fontWeight: 700,
          flexDirection: "column",
        }}
      >
        <Box width={"80vw"}>
          Chào mừng bạn đến với Mini App của Gas Bình Minh
        </Box>
        <Box width={"80vw"} mt={2}>
          Vui lòng chọn cửa hàng để tiếp tục
        </Box>
      </Box>

      <Box p={6}>
        <CustomSelect></CustomSelect>
      </Box>
      <Box style={{ marginTop: "20vh" }} className="divCenter">
        <Box
          className="divCenter"
          style={{
            width: "50vw",
            height: 50,
            color: "#fff",
            fontSize: 20,
            fontWeight: "bold",
            backgroundColor: "#D82229",
            borderRadius: 20,
          }}
          onClick={handleClickConfirm}
        >
          Xác nhận
        </Box>
      </Box>
    </Page>
  );
}
