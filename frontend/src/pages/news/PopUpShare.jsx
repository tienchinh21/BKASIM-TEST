import React from "react";
import { Modal, Box } from "zmp-ui";
import { FaRegCopy } from "react-icons/fa6";
import { openShareSheet, showToast } from "zmp-sdk";
import dfData from "../../common/DefaultConfig.json";
import { APP_MODE } from "../../state";
import { useRecoilValue } from "recoil";
import { infoShare} from "../../recoil/RecoilState";

const PopUpShare = ({ visible, onClose, detail }) => {
  const env = APP_MODE;
  const shareInfo = useRecoilValue(infoShare);

  const openToast = async (text) => {
    try {
      await showToast({
        message: text,
      });
    } catch (error) {
      console.log(error);
    }
  };

  const linkShare = (id) => {
    let url = `https://zalo.me/s/${dfData.appid}/detailNew/${id}`;
    url += `?ref=${shareInfo?.referralCode}&source=invite`;

    return url;
  };

  return (
    <Modal width="100%" visible={visible} onClose={onClose}>
      <Box style={{ width: "100%" }}>
        <img style={{ width: "100%" }} src={detail?.bannerImage} />
      </Box>

      <Box
        mt={3}
        textAlign="center"
        style={{ fontWeight: "bold", fontSize: 22 }}
      >
        Chia sẻ bài viết
      </Box>
      <Box
        mt={3}
        style={{ fontSize: 14 }}
        flex
        justifyContent="center"
        textAlign="center"
      >
        Đừng quên chia sẻ bài viết này với bạn bè để cùng nhau khám phá thêm
        nhiều thông tin thú vị!
      </Box>

      {/* Các nút chia sẻ */}
      <Box mt={5} flex justifyContent="space-around">
        {/* Nút sao chép đường dẫn */}
        <Box
          className="btn btn-light"
          style={{
            border: "1px solid #4665D6",
            color: "#4665D6",
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
            background: "#4665D6",
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

export default PopUpShare;
