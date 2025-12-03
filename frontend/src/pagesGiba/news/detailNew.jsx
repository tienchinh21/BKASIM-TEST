import React, { useState, useEffect, useRef } from "react";
import { Box, Page, Swiper } from "zmp-ui";
import { useParams, useLocation, useNavigate } from "react-router";
import useSetHeader from "../../components/hooks/useSetHeader";
import dayjs from "dayjs";
import dfData from "../../common/DefaultConfig.json";
import axios from "axios";
import { useRecoilValue } from "recoil";
import { infoShare, token } from "../../recoil/RecoilState";
import { APP_MODE } from "../../state";
import ViewTextRender from "../../components/renderview/ViewTextRender";
import CommonButton from "../../components/CommonButton";
import CommonShareModal from "../../components/CommonShareModal";
import { Modal } from "antd";
import { toast } from "react-toastify";
import DefaultImage from "../../assets/no_image.png";

const DetailNew = () => {
  const { id } = useParams();
  const location = useLocation();
  const navigate = useNavigate();
  const query = new URLSearchParams(location.search);
  const defaultScreenParam = query.get("default");
  const setHeader = useSetHeader();
  const [loading, setLoading] = useState(false);
  const [detail, setDetail] = useState({});
  const shareRef = useRef(null);
  const shareInfo = useRecoilValue(infoShare);
  const userToken = useRecoilValue(token);
  const env = APP_MODE;

  // Check if in admin mode from navigation state
  const isAdminMode = location.state?.isAdminMode || false;

  useEffect(() => {
    setHeader({
      hasLeftIcon: true,
      type: "secondary",
      title: "CHI TIẾT BẢN TIN",
      customTitle: false,
      route: defaultScreenParam ? "/home" : null,
    });
    getDetailNew();
  }, [defaultScreenParam]);

  // Prepare images for ImageViewer
  const images = detail?.images?.map((item, index) => ({
    src: item,
    alt: index + 1,
    key: index + 1,
  }));

  const getDetailNew = () => {
    setLoading(true);
    axios
      .get(`${dfData.domain}/api/articles/${id}`, {
        params: {
          type: "tintuc",
        },
      })
      .then((response) => {
        setDetail({ ...response.data.data });
        setLoading(false);
      })
      .catch((error) => {
        setLoading(false);
      })
      .finally(() => {
        setLoading(false);
      });
  };
  const linkShare = (id) => {
    let url = `https://zalo.me/s/${dfData.appid}/detailNew/${id}`;

    if (env == "TESTING" || env == "TESTING_LOCAL" || env == "DEVELOPMENT") {
      url += `?env=${env}&version=${window.APP_VERSION}&ref=${shareInfo?.referralCode}&source=invite&default=true`;
    } else {
      url += `?ref=${shareInfo?.referralCode}&source=invite&default=true`;
    }

    //url += `?ref=${shareInfo?.referralCode}&source=invite&default=true`;
    return url;
  };

  const handleDeleteArticle = () => {
    Modal.confirm({
      title: "Xác nhận xóa",
      content: `Bạn có chắc chắn muốn xóa bản tin "${detail?.title}"?`,
      okText: "Xóa",
      cancelText: "Hủy",
      okButtonProps: { danger: true },
      onOk: async () => {
        try {
          const response = await axios.delete(
            `${dfData.domain}/api/Articles/${id}`,
            {
              headers: { Authorization: `Bearer ${userToken}` },
            }
          );

          if (response.data.code === 0) {
            toast.success(response.data.message || "Xóa bản tin thành công!");
            // Navigate back to manager articles page
            navigate("/giba/manager-articles");
          } else {
            toast.error(response.data.message || "Xóa bản tin thất bại");
          }
        } catch (error) {
          console.error("Error deleting article:", error);
          toast.error("Không thể xóa bản tin");
        }
      },
    });
  };

  return (
    <Page style={{ padding: "60px 0px 60px", position: "relative" }}>
      <div
        style={{
          display: "flex",
          flexDirection: "column",
          padding: "0 8px 20px 8px",
          overflowY: "scroll",
          overflowX: "hidden",
        }}
      >
        {detail?.images?.length > 0 ? (
          <Swiper
            style={{ borderRadius: "0px" }}
            loop={detail.images.length > 1}
            dots={detail.images.length > 1}
          >
            {detail.images.map((img, index) => {
              return (
                <Swiper.Slide key={index}>
                  <div
                    style={{
                      display: "flex",
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
                        maxHeight: "25vh",
                        objectFit: "fill",
                        borderRadius: 10,
                      }}
                      src={img || DefaultImage}
                      alt={`Slide ${index + 1}`}
                      onError={(e) => {
                        e.target.src = DefaultImage;
                      }}
                    />
                  </div>
                </Swiper.Slide>
              );
            })}
          </Swiper>
        ) : (
          <Box mt={3}>
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
                  height: "100%",
                  objectFit: "cover",
                  borderRadius: 10,
                }}
                src={detail?.bannerImage || DefaultImage}
                alt="Banner"
                onError={(e) => {
                  e.target.src = DefaultImage;
                }}
              />
            </div>
          </Box>
        )}
        <Box style={{ fontSize: 13 }}>
          <span style={{ marginLeft: 4 }}>
            {dayjs(detail?.createdDate).format("DD/MM/YYYY")}
          </span>
          <span style={{ marginLeft: 6 }}>
            {" "}
            Bởi{" "}
            <span style={{ color: "var(--primary-color)" }}>
              {detail?.author}
            </span>
          </span>
        </Box>
        <Box
          mt={4}
          style={{
            textAlign: "center",
            fontWeight: "bold",
            fontSize: 18,
            wordBreak: "break-word",
            whiteSpace: "normal",
          }}
        >
          {detail?.title}
        </Box>
        <ViewTextRender
          style={{
            paddingTop: 15,
            fontWeight: 600,
            fontSize: 14,
            wordBreak: "break-word",
            whiteSpace: "normal",
            overflowWrap: "break-word",
            maxWidth: "100%",
          }}
          dataText={detail?.content}
        />
      </div>
      {isAdminMode ? (
        <div
          style={{
            position: "fixed",
            bottom: 0,
            left: 0,
            right: 0,
            display: "flex",
            gap: "8px",
            padding: "12px 16px",
            backgroundColor: "#fff",
            borderTop: "1px solid #e5e7eb",
            zIndex: 999,
          }}
        >
          <CommonButton
            text="Xóa"
            onClick={handleDeleteArticle}
            style={{
              flex: 1,
              backgroundColor: "#ef4444",
              color: "#fff",
            }}
          />
          <CommonButton
            text="Chia sẻ"
            onClick={() => {
              shareRef.current.open(
                linkShare(detail?.id),
                "Chia sẻ bản tin",
                "Đừng quên chia sẻ bản tin này với bạn bè để cùng nhau khám phá thêm nhiều thông tin thú vị!",
                detail?.bannerImage || detail?.images?.[0]
              );
            }}
            style={{ flex: 1 }}
          />
        </div>
      ) : (
        <CommonButton
          text="Chia sẻ"
          onClick={() => {
            shareRef.current.open(
              linkShare(detail?.id),
              "Chia sẻ bản tin",
              "Đừng quên chia sẻ bản tin này với bạn bè để cùng nhau khám phá thêm nhiều thông tin thú vị!",
              detail?.bannerImage || detail?.images?.[0]
            );
          }}
          isFixedBottom
        />
      )}

      <CommonShareModal ref={shareRef} />
    </Page>
  );
};

export default DetailNew;
