import React, { useEffect, useState, useRef } from "react";
import { useRecoilState, useRecoilValue, useSetRecoilState } from "recoil";
import { Page, Box, Modal as ZmpModal, useNavigate } from "zmp-ui";
import {
  infoShare,
  infoUser,
  isRegister,
  phoneNumberUser,
  token,
  codeAffiliate,
  cartState,
  actionTab,
  featureConfig,
} from "../../recoil/RecoilState";
import { QRCode } from "zmp-qrcode";
import "./info.css";
import { saveImageToGallery } from "zmp-sdk/apis";
import dfData from "../../common/DefaultConfig.json";
import useSetHeader from "../../components/hooks/useSetHeader";
import axios from "axios";
import { toast } from "react-toastify";
import { isEmpty, checkFeatureShow } from "../../common/Common";
import { CgShoppingBag } from "react-icons/cg";
import { IoDiamondOutline } from "react-icons/io5";
import { LuTicketPercent } from "react-icons/lu";
import UserCard from "../../assets/user_card.png";
import { openShareSheet, showToast, openWebview } from "zmp-sdk";
import HelloCard from "../home/HelloCard";
import html2canvas from "html2canvas";
import Loading from "../../components/loading/Loading";
import SignUpRequied from "./SignUpRequied";
import CommonShareModal from "../../components/CommonShareModal";
import { APP_MODE } from "../../state";

const Info = () => {
  const navigate = useNavigate();
  const setHeader = useSetHeader();
  const popupRef = useRef(null);

  const [shareInfo, setShareInfo] = useRecoilState(infoShare);
  const [infoZalo, setInfoZalo] = useRecoilState(infoUser);
  const [phoneUser, setPhoneUser] = useRecoilState(phoneNumberUser);
  const [isRegistered, setIsRegistered] = useRecoilState(isRegister);
  const [tokenAuth, setTokenAuth] = useRecoilState(token);
  const inputRefCode = useRecoilValue(codeAffiliate);
  const [isFollow, setIsFollow] = useState(false);
  const [referralModalVisible, setReferralModalVisible] = useState(false);
  const [totalCartQuantity, setTotalCartQuantity] = useRecoilState(cartState);
  const [loading, setLoading] = useState(false);
  const [linkTutorial, setLinkTutorial] = useState("");
  const setActiveTab = useSetRecoilState(actionTab);
  const listFeature = useRecoilValue(featureConfig);
  const qrRef = useRef(null);

  const env = APP_MODE;

  console.log(shareInfo);

  useEffect(() => {
    setHeader({
      hasLeftIcon: false,
      type: "secondary",
      title: "THÀNH VIÊN",
      customTitle: false,
      //route: "/welcome",
    });
    setActiveTab("me");
    getTutorialVNPay();
  }, []);

  useEffect(() => {
    if (tokenAuth) {
      getProfile();
    }
  }, [tokenAuth]);

  const getProfile = () => {
    axios
      .get(`${dfData.domain}/api/Memberships/profile`, {
        headers: {
          Authorization: `Bearer ${tokenAuth}`,
        },
      })
      .then((res) => {
        setShareInfo(res.data.data);
      })
      .catch((error) => {});
  };

  const openToast = async (text) => {
    try {
      const data = await showToast({
        message: text,
      });
    } catch (error) {
      console.log(error);
    }
  };

  const generateQRCodeURL = () => {
    let url = `https://zalo.me/s/${dfData.appid}/home`;
    if (shareInfo?.referralCode) {
      url += `?ref=${shareInfo?.referralCode}&source=invite`;
      if (env == "TESTING" || env == "TESTING_LOCAL" || env == "DEVELOPMENT") {
        url += `&env=${env}&version=${window.APP_VERSION}`;
      }
    }

    return url;
  };

  const url = generateQRCodeURL();

  const copyToClipboard = () => {
    navigator.clipboard.writeText(url).then(() => {
      openToast("Đã sao chép link giới thiệu!");
    });
  };

  const handleShareQR = async () => {
    try {
      const data = await openShareSheet({
        type: "link",
        data: {
          link: url,
          chatOnly: false,
        },
      });
    } catch (err) {}
  };

  const handleSaveScreenshot = async () => {
    try {
      setLoading(true);
      if (!qrRef.current) {
        toast.error("Không thể chụp ảnh. Vui lòng thử lại!");
        return;
      }
      // Đảm bảo tất cả font chữ đã được tải xong
      await document.fonts.ready;

      // Đảm bảo tất cả hình ảnh đã được tải xong
      const images = qrRef.current.querySelectorAll("img");
      await Promise.all(
        Array.from(images).map((img) => {
          if (!img.src) return Promise.resolve(); // Bỏ qua nếu không có src
          if (img.complete) return Promise.resolve();
          return new Promise((resolve, reject) => {
            img.onload = resolve;
            img.onerror = reject;
          });
        })
      );

      // Đợi một chút để đảm bảo DOM render đầy đủ
      await new Promise((resolve) => setTimeout(resolve, 1000));

      // Chụp ảnh
      const canvas = await html2canvas(qrRef.current, {
        useCORS: true,
        backgroundColor: null,
        scrollX: 0,
        scrollY: -window.scrollY,
        windowWidth: document.documentElement.offsetWidth,
        windowHeight: document.documentElement.offsetHeight,
        scale: 2, // Tăng độ phân giải
      });

      const imageBase64 = canvas.toDataURL("image/png");

      // Lưu Base64 vào state thay vì tải xuống
      await saveImage(imageBase64);
      setLoading(false);

      toast.success("Lưu ảnh thành công");
    } catch (error) {
      console.error("Lỗi khi chụp màn hình:", error);
      toast.error("Lưu ảnh thất bại. Vui lòng thử lại!");
      setLoading(false);
    }
  };

  const saveImage = async (imageBase64) => {
    try {
      await saveImageToGallery({
        imageBase64Data: imageBase64,
        onProgress: (progress) => {
          console.log(progress);
        },
      });
    } catch (error) {
      // xử lý khi gọi api thất bại
      console.log(error);
    }
  };

  const getTutorialVNPay = () => {
    axios
      .get(`${dfData.domain}/api/SystemSettings/About`)
      .then((res) => {
        if (res.data.code === 0) {
          setLinkTutorial(res.data.data);
        }
      })
      .catch((error) => {
        console.log("Lấy thông tin doanh nghiệp không thành công:", error);
      });
  };

  const webView = async (link) => {
    try {
      await openWebview({
        url: link,
        config: {
          style: "bottomSheet",
          leftButton: "back",
        },
      });
    } catch (error) {
      console.log(error);
    }
  };

  const shareAffiliate = () => {
    popupRef.current.open(url, "Chia sẻ mã giới thiệu", "", "", true);
  };

  return (
    <Page
      className="page"
      hideScrollbar={true}
      style={{
        paddingBottom: 120,
        paddingTop: 50,
      }}
    >
      {loading && <Loading />}
      {isRegistered ? (
        <Box>
          <HelloCard isInfo={true} />
          <Box
            style={{
              display: "flex",
              alignItems: "center",
              borderRadius: "8px",
              flexDirection: "column",
              border: "1px solid var(--primary-color)",
            }}
          >
            <Box
              style={{
                width: "100%",
                background: "var(--background-button-color)",
                padding: "4px 14px",
                borderRadius: "5px 5px 0 0",
                color: "#fff",
                fontWeight: "bold",
                fontSize: 18,
              }}
            >
              Tính năng cá nhân
            </Box>
            <Box
              style={{
                display: "flex",
                alignItems: "center",
                justifyContent: "space-between",
                width: "100%",
                borderBottom: "1px solid var(--primary-color)",
                padding: "8px 14px",
              }}
            >
              <Box style={{ display: "flex", alignItems: "center", gap: 10 }}>
                <CgShoppingBag size={22} style={{ color: "#558B2F" }} />
                Xem đơn hàng đã mua
              </Box>
              <Box
                className="affilate_btn"
                onClick={() => {
                  navigate("/statusBill");
                }}
              >
                Xem chi tiết
              </Box>
            </Box>
            <Box
              style={{
                display: "flex",
                alignItems: "center",
                justifyContent: "space-between",
                width: "100%",
                borderBottom: checkFeatureShow(
                  listFeature,
                  "MembershipRankButton"
                )
                  ? "1px solid var(--primary-color)"
                  : null,
                padding: "8px 14px",
              }}
            >
              <Box style={{ display: "flex", alignItems: "center", gap: 10 }}>
                <LuTicketPercent
                  size={22}
                  style={{ color: "var(--primary-color)" }}
                />
                Xem mã voucher của tôi
              </Box>
              <Box
                className="affilate_btn"
                onClick={() => {
                  navigate("/promotion");
                }}
              >
                Xem chi tiết
              </Box>
            </Box>
            {checkFeatureShow(listFeature, "MembershipRankButton") && (
              <Box
                style={{
                  display: "flex",
                  alignItems: "center",
                  justifyContent: "space-between",
                  width: "100%",
                  padding: "8px 14px",
                }}
              >
                <Box style={{ display: "flex", alignItems: "center", gap: 10 }}>
                  <IoDiamondOutline size={22} style={{ color: "#0069F3" }} />
                  Xem hạng thành viên
                </Box>
                <Box
                  className="affilate_btn"
                  onClick={() => {
                    //navigate("/default");
                    navigate("/rank");
                  }}
                >
                  Xem chi tiết
                </Box>
              </Box>
            )}
          </Box>

          <Box
            style={{
              display: "flex",
              alignItems: "center",
              margin: "14px 0px",
              borderRadius: "8px",
              flexDirection: "column",
              border: "1px solid var(--primary-color)",
            }}
          >
            <Box
              style={{
                width: "100%",
                background: "var(--background-button-color)",
                padding: "4px 14px",
                borderRadius: "5px 5px 0 0",
                color: "#fff",
                fontWeight: "bold",
                fontSize: 18,
              }}
            >
              Tiếp thị liên kết
            </Box>
            <Box
              style={{
                display: "flex",
                alignItems: "center",
                justifyContent: "space-between",
                width: "100%",
                borderBottom: "1px solid var(--primary-color)",
                padding: "8px 14px",
              }}
            >
              <Box
                style={{
                  display: "flex",
                  alignItems: "center",
                  gap: 4,
                  fontSize: 14,
                }}
              >
                <Box>Mã giới thiệu của bạn:</Box>
                <Box
                  style={{
                    fontWeight: "bold",
                    color: "var(--color-discount-price)",
                  }}
                >
                  {shareInfo?.referralCode}
                </Box>
              </Box>
              <Box
                className="affilate_btn"
                onClick={() => {
                  shareAffiliate();
                }}
              >
                Chia sẻ
              </Box>
            </Box>
            <Box
              style={{
                display: "flex",
                alignItems: "center",
                justifyContent: "space-between",
                width: "100%",
                borderBottom: "1px solid var(--primary-color)",
                padding: "8px 14px",
              }}
            >
              <Box
                style={{
                  display: "flex",
                  alignItems: "center",
                  gap: 4,
                  fontSize: 14,
                }}
              >
                <Box>Hoa hồng nhận được:</Box>
                <Box style={{ fontWeight: "bold", color: "#4F80E9" }}>
                  {shareInfo?.totalCommission?.toLocaleString("vi-VN")}
                </Box>
              </Box>
              <Box
                className="affilate_btn"
                onClick={() => {
                  navigate("/commission");
                }}
              >
                Xem chi tiết
              </Box>
            </Box>
            <Box
              style={{
                display: "flex",
                alignItems: "center",
                justifyContent: "space-between",
                width: "100%",
                borderBottom: "1px solid var(--primary-color)",
                padding: "8px 14px",
              }}
            >
              <Box style={{ fontSize: 14 }}>Danh sách giới thiệu</Box>
              <Box
                className="affilate_btn"
                onClick={() => navigate("/affiliate")}
              >
                Xem chi tiết
              </Box>
            </Box>

            <Box
              style={{
                display: "flex",
                alignItems: "center",
                justifyContent: "space-between",
                width: "100%",
                borderBottom: "1px solid var(--primary-color)",
                padding: "8px 14px",
              }}
            >
              <Box style={{ fontSize: 14 }}>Chính sách hoa hồng</Box>
              <Box className="affilate_btn" onClick={() => navigate("/policy")}>
                Xem chi tiết
              </Box>
            </Box>
            <Box
              style={{
                display: "flex",
                alignItems: "center",
                justifyContent: "space-between",
                width: "100%",
                padding: "8px 14px",
              }}
            >
              <Box className="qr" ref={qrRef} style={{ margin: "10px 0" }}>
                <QRCode
                  style={{ border: "none" }}
                  value={generateQRCodeURL()}
                  size={150}
                />
              </Box>

              <Box
                style={{
                  display: "flex",
                  flexDirection: "column",
                  width: "28%",
                  gap: 10,
                }}
              >
                <Box
                  className="affilate_btn"
                  style={{ width: "100%" }}
                  onClick={handleSaveScreenshot}
                >
                  Tải về
                </Box>
                <Box
                  className="affilate_btn"
                  style={{ width: "100%" }}
                  onClick={handleShareQR}
                >
                  Chia sẻ
                </Box>
              </Box>
            </Box>
          </Box>

          <Box
            style={{
              display: "flex",
              alignItems: "center",
              margin: "14px 0px",
              // padding: "8px 14px",
              borderRadius: "8px",
              flexDirection: "column",
              border: "1px solid var(--primary-color)",
            }}
          >
            <Box
              style={{
                width: "100%",
                background: "var(--background-button-color)",
                padding: "4px 14px",
                borderRadius: "5px 5px 0 0",
                color: "#fff",
                fontWeight: "bold",
                fontSize: 18,
              }}
            >
              Tính năng khác
            </Box>
            <Box
              style={{
                display: "flex",
                alignItems: "center",
                justifyContent: "space-between",
                width: "100%",
                borderBottom: "1px solid var(--primary-color)",
                padding: "8px 14px",
              }}
            >
              <Box style={{ fontSize: 14 }}>Thông tin doanh nghiệp</Box>
              <Box
                className="affilate_btn"
                onClick={() => navigate("/businessInfo")}
              >
                Xem chi tiết
              </Box>
            </Box>
            <Box
              style={{
                display: "flex",
                alignItems: "center",
                justifyContent: "space-between",
                width: "100%",
                padding: "8px 14px",
              }}
            >
              <Box style={{ fontSize: 14 }}>Hướng dẫn thanh toán</Box>
              <Box
                className="affilate_btn"
                onClick={() => {
                  webView(linkTutorial?.paymentInstruction);
                }}
              >
                Xem chi tiết
              </Box>
            </Box>
          </Box>

          <ZmpModal
            visible={referralModalVisible}
            title="Link giới thiệu:"
            onClose={() => setReferralModalVisible(false)}
          >
            <Box>
              <Box
                style={{
                  padding: "10px",
                  background: "#f0f0f0",
                  borderRadius: "5px",
                  marginBottom: "10px",
                  wordBreak: "break-all",
                }}
              >
                {generateQRCodeURL()}
              </Box>

              <Box style={{ display: "flex", justifyContent: "space-evenly" }}>
                <Box
                  className="btn btn-danger"
                  onClick={() => setReferralModalVisible(false)}
                >
                  Đóng
                </Box>
                <Box className="btn btn-success" onClick={copyToClipboard}>
                  Sao chép link
                </Box>
              </Box>
            </Box>
          </ZmpModal>
          <CommonShareModal
            ref={popupRef}
            title="Link giới thiệu:"
            link={url}
          />
        </Box>
      ) : (
        <SignUpRequied />
      )}
    </Page>
  );
};

export default Info;
