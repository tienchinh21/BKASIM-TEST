import React, { useState, forwardRef, useImperativeHandle } from "react";
import { Modal, Box } from "zmp-ui";
import { FaRegCopy } from "react-icons/fa6";
import { openShareSheet, showToast } from "zmp-sdk";
import { RiCloseLargeLine } from "react-icons/ri";
import { GlobalStyles } from "../store/styles/GlobalStyles";

const CommonShareModal = forwardRef((props, ref) => {
  const [visible, setVisible] = useState(false);
  const [link, setLink] = useState("");
  const [bannerUrl, setBannerUrl] = useState("");
  const [title, setTitle] = useState("Chia sẻ nội dung");
  const [description, setDescription] = useState(
    "Hãy chia sẻ nội dung này với bạn bè!"
  );

  useImperativeHandle(ref, () => ({
    open: (link = "", titleText = "", desc = "", banner = "") => {
      setLink(link);
      setTitle(titleText || "Chia sẻ nội dung");
      setDescription(desc || "Hãy chia sẻ nội dung này với bạn bè!");
      setBannerUrl(banner);
      setVisible(true);
    },
    close: () => setVisible(false),
  }));

  const handleCopy = () => {
    if (!link) return;
    navigator.clipboard
      .writeText(link)
      .then(() => showToast({ message: "Đã sao chép đường dẫn thành công!" }))
      .catch(() => showToast({ message: "Lỗi khi sao chép đường dẫn." }));
  };

  const handleZaloShare = () => {
    if (!link) return;
    openShareSheet({
      type: "link",
      data: { link, chatOnly: false },
    });
  };

  return (
    <Modal width="95%" visible={visible} onClose={() => setVisible(false)}>
      <Box
        style={{
          wordBreak: "break-word",
          whiteSpace: "normal",
          overflowX: "hidden",
          maxWidth: "100vw",
        }}
      >
        <Box
          style={{
            position: "absolute",
            top: GlobalStyles.spacing.sm,
            right: GlobalStyles.spacing.sm,
            fontWeight: GlobalStyles.fontWeight.bold,
            fontSize: GlobalStyles.fontSize.md,
          }}
        >
          <RiCloseLargeLine
            size={20}
            style={{ color: "var(--third-color)" }}
            onClick={() => setVisible(false)}
          />
        </Box>

        {bannerUrl && (
          <Box
            style={{
              width: "100%",
              marginBottom: 15,
              display: "flex",
              justifyContent: "center",
            }}
          >
            <img
              style={{
                maxWidth: "100%",
                maxHeight: "250px",
                objectFit: "contain",
                borderRadius: GlobalStyles.borderRadius.sm,
                width: "auto",
              }}
              src={bannerUrl}
              alt="banner"
            />
          </Box>
        )}

        <Box
          style={{
            fontWeight: GlobalStyles.fontWeight.bold,
            fontSize: GlobalStyles.fontSize.xl,
            textAlign: "center",
            marginBottom: GlobalStyles.spacing.sm,
          }}
        >
          {title}
        </Box>

        <Box
          style={{
            fontSize: GlobalStyles.fontSize.base,
            textAlign: "center",
            whiteSpace: "pre-wrap",
            marginBottom: GlobalStyles.spacing.md,
          }}
        >
          {description}
        </Box>

        <Box
          flex
          justifyContent="space-between"
          style={{
            gap: GlobalStyles.spacing.sm,
          }}
        >
          <Box className="btn-share-modal btn-outline" onClick={handleCopy}>
            <FaRegCopy size={30} />
            <div>
              Sao chép
              <br />
              đường dẫn
            </div>
          </Box>

          <Box
            className="btn-share-modal btn-primary"
            onClick={handleZaloShare}
          >
            <img
              style={{ width: 30, height: 30 }}
              src="https://upload.wikimedia.org/wikipedia/commons/thumb/9/91/Icon_of_Zalo.svg/1200px-Icon_of_Zalo.svg.png"
              alt="Zalo"
            />
            <div>
              Chia sẻ
              <br />
              qua Zalo
            </div>
          </Box>
        </Box>
      </Box>
    </Modal>
  );
});

export default CommonShareModal;
