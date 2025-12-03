import React from "react";
import { Modal, Box } from "zmp-ui";
import { FaRegCopy } from "react-icons/fa6";
import { openShareSheet, showToast } from "zmp-sdk";
import dfData from "../../../common/DefaultConfig.json";
import { APP_MODE } from "../../../state";

const PopUpShareEvent = ({ visible, onClose, detail }) => {
  const env = APP_MODE;

  const openToast = async (text) => {
    try {
      await showToast({
        message: text,
      });
    } catch (error) {
      console.log(error);
    }
  };

  const getBannerUrl = (banner) => {
    if (!banner) return null;
    if (banner.startsWith("http://") || banner.startsWith("https://")) {
      return banner;
    }
    if (banner.startsWith("/")) {
      return `${dfData.domain}${banner}`;
    }
    return banner;
  };

  const linkShare = (id) => {
    let url = `https://zalo.me/s/${dfData.appid}/detail-event/${id}`;
    if (env == "TESTING" || env == "TESTING_LOCAL" || env == "DEVELOPMENT") {
      url += `?env=${env}&version=${window.APP_VERSION}&default=true`;
    }
    return url;
  };

  return (
    <Modal width="100%" visible={visible} onClose={onClose}>
      <Box style={{ width: "100%" }}>
        <img style={{ width: "100%" }} src={getBannerUrl(detail?.banner)} />
      </Box>
      <Box
        mt={3}
        textAlign="center"
        style={{ fontWeight: "bold", fontSize: 22 }}
      >
        Chia sẻ sự kiện này
      </Box>
      <Box
        mt={3}
        style={{ fontSize: 14 }}
        flex
        justifyContent="center"
        textAlign="center"
      >
        Hãy chia sẻ sự kiện này đến bạn bè nhé!
      </Box>

      {/* Các nút chia sẻ */}
      <Box mt={5} flex justifyContent="space-around">
        {/* Nút sao chép đường dẫn */}
        <Box
          className="btn btn-light"
          style={{
            border: "1px solid #003d82",
            color: "#003d82",
            width: "44%",
            gap: 8,
            display: "flex",
            justifyContent: "center",
            alignItems: "center",
            fontSize: 14,
          }}
          onClick={() => {
            navigator.clipboard
              .writeText(linkShare(detail?.id))
              .then(() => openToast("Đã sao chép đường dẫn thành công!"))
              .catch((err) => openToast("Lỗi khi sao chép đường dẫn."));
          }}
        >
          <FaRegCopy size={30} />
          <div>Sao chép đường dẫn</div>
        </Box>

        {/* Nút chia sẻ trên Zalo */}
        <Box
          style={{
            background: "#003d82",
            color: "#fff",
            width: "44%",
            gap: 8,
            display: "flex",
            justifyContent: "center",
            alignItems: "center",
            fontSize: 14,
          }}
          className="btn btn-info"
          onClick={() =>
            openShareSheet({
              type: "link",
              data: {
                link: linkShare(detail?.id),
                chatOnly: false,
              },
            })
          }
        >
          <img
            style={{ width: 44, height: 30 }}
            src="https://upload.wikimedia.org/wikipedia/commons/thumb/9/91/Icon_of_Zalo.svg/1200px-Icon_of_Zalo.svg.png"
          />
          Chia sẻ qua Zalo
        </Box>
      </Box>
    </Modal>
  );
};

export default PopUpShareEvent;
