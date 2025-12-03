import React from "react";
import { useRecoilState } from "recoil";
import { Avatar, Box } from "zmp-ui";
import { followOA } from "zmp-sdk/apis";
import { infoUser, isFollowedOA } from "../../recoil/RecoilState";
import dfData from "../../common/DefaultConfig.json";
import { toast } from "react-toastify";
import logo from "../../assets/logo_incom.png";
import { GlobalStyles } from "../../store/styles/GlobalStyles";

const FollowOACard = () => {
  const [infoZalo, setInfoZalo] = useRecoilState(infoUser);
  const [isFollowedOAState, setIsFollowedOAState] = useRecoilState(isFollowedOA);
  //   const logo = `${dfData.domain}/images/frontend/common/logo.png`;

  console.log("isFollowedOAState in FollowOACard:", isFollowedOAState);

  const getFollowOA = async () => {
    const isFollow = await new Promise((resolve, reject) => {
      followOA({
        id: dfData.oaId,
        success: async (res) => {
          setInfoZalo({ ...infoZalo, followedOA: true });
          setIsFollowedOAState(true); // Cập nhật Recoil state
          resolve(true);
        },
        fail: (err) => {
          if (err.code == -203) {
            toast.error(
              "Bạn từ chối follow OA quá nhiều lần. Vui lòng tải lại trang!"
            );
          }
          resolve(false);
        },
      });
    });
  };

  return (
    <>
      {!isFollowedOAState && (
        <Box className="box-common-giba">
          <Box
            style={{
              display: "flex",
              flexDirection: "row",
              width: "100%",
              justifyContent: "space-between",
            }}
          >
            <Box
              style={{
                padding: 4,
                display: "inline-flex",
                alignItems: "center",
                justifyContent: "center",
                gap: GlobalStyles.spacing.sm,
              }}
            >
              <Avatar
                style={{
                  background: "#fff",
                }}
                src={logo}
                size={42}
              />
              <Box
                style={{
                  display: "flex",
                  flexDirection: "column",
                  lineHeight: 1.5,
                  whiteSpace: "nowrap",
                }}
              >
                <div style={{ fontSize: 14, fontWeight: "bold" }}>
                  Quan tâm OA để nhận ưu đãi!
                </div>
                <Box className="text-sub">{dfData.oaName}</Box>
              </Box>
            </Box>

            <Box
              width={"30%"}
              flex
              justifyContent="flex-end"
              alignItems="center"
            >
              <Box
                className="button-common"
                style={{
                  background: "#000000",
                  color: "#ffffff",
                  border: "1px solid #000000",
                }}
                onClick={() => {
                  getFollowOA();
                }}
              >
                Quan tâm
              </Box>
            </Box>
          </Box>
        </Box>
      )}
    </>
  );
};

export default FollowOACard;
