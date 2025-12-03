import React, { useEffect, useState } from "react";
import useSetHeader from "../../../../components/hooks/useSetHeader";
import { Box, useNavigate } from "zmp-ui";
import { openWebview } from "zmp-sdk";
import "../../event.css";
import axios from "axios";
import Loading from "../../../../components/loading/Loading";
import dfData from "../../../../common/DefaultConfig.json";
import { useParams, useLocation } from "react-router-dom";
import { toast } from "react-toastify";
import DefaultImage from "../../../../assets/no_image.png";
import ViewTextRender from "../../../../components/renderview/ViewTextRender";
import CommonButton from "../../../../components/CommonButton";
import { GlobalStyles } from "../../../../store/styles/GlobalStyles";

const SponsorDetail = () => {
  const setHeader = useSetHeader();
  const navigate = useNavigate();
  const { id } = useParams();
  const location = useLocation();
  const [loading, setLoading] = useState(false);
  const [detail, setDetail] = useState(null);

  useEffect(() => {
    setHeader({
      hasLeftIcon: true,
      type: "secondary",
      title: "CHI TIẾT NHÀ TÀI TRỢ",
      customTitle: false,
    });
    getDetailSponsor();
  }, []);

  const fromTab = location.state?.fromTab;
  const eventId = location.state?.eventId;
  const openURL = async (link) => {
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

  const getDetailSponsor = () => {
    setLoading(true);
    axios
      .get(`${dfData.domain}/api/sponsors/${id}`)
      .then((response) => {
        setDetail(response.data.data);
        setLoading(false);
      })
      .catch((error) => {
        toast.error("Lấy chi tiết nhà tài trợ không thành công:", error);
        setLoading(false);
      })
      .finally(() => {
        setLoading(false);
      });
  };

  return (
    <Box style={{ padding: "50px 14px 90px" }}>
      {loading && <Loading />}
      <Box
        style={{
          gap: GlobalStyles.spacing.xs,
          display: "flex",
          flexDirection: "column",
        }}
      >
        <div
          style={{
            position: "relative",
            width: "100%",
            paddingTop: "100%",
            overflow: "hidden",
            marginTop: "16px",
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
              borderRadius: GlobalStyles.borderRadius.xs,
              border: "1px solid #3333",
            }}
            src={detail?.image || DefaultImage}
            alt="Banner"
          />
        </div>
        <Box style={{ fontWeight: "bold", marginTop: "16px", fontSize: 18 }}>
          {detail?.sponsorName}
        </Box>
        <ViewTextRender
          dataText={detail?.introduction}
          style={{ textAlign: "justify", fontSize: 16 }}
        />
        <Box
          style={{
            position: "fixed",
            bottom: 0,
            left: 0,
            right: 0,
            padding: "10px 10px 30px",
            background: "#fff",
          }}
        >
          <Box className="action-btn-row">
            <CommonButton
              text="Quay lại"
              onClick={() => navigate(-1)}
              style={{ width: "48%" }}
              variant="outline"
            />
            <CommonButton
              text="Xem website"
              onClick={() => openURL(detail?.websiteURL)}
              style={{ width: "48%" }}
            />
          </Box>
        </Box>
      </Box>
    </Box>
  );
};

export default SponsorDetail;
