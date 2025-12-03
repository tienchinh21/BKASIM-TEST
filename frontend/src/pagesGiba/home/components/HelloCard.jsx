import React, { useEffect } from "react";
import { useRecoilValue, useSetRecoilState } from "recoil";
import { Avatar, Box } from "zmp-ui";
import { useNavigate } from "react-router-dom";
import {
  infoShare,
  infoUser,
  isRegister,
  phoneNumberLoading,
  userMembershipInfo,
} from "../../../recoil/RecoilState";
import SampleAvatar from "../../../assets/user_simple.png";
import { EditIcon } from "lucide-react";
import { GlobalStyles } from "../../../store/styles/GlobalStyles";
import { Spin } from "antd";

const HelloCard = ({ isInfo, term, appPosition, isLoadingProfile = false }) => {
  const navigate = useNavigate();
  const infoZalo = useRecoilValue(infoUser);
  const isRegistered = useRecoilValue(isRegister);
  const shareInfo = useRecoilValue(infoShare);
  const loadingPhone = useRecoilValue(phoneNumberLoading);
  const setPhoneLoading = useSetRecoilState(phoneNumberLoading);
  const membershipInfo = useRecoilValue(userMembershipInfo);

  const getPositionText = () => {
    if (!appPosition && !term) return null;
    if (appPosition && term) {
      return `${appPosition} (Nhiệm kỳ ${term})`;
    }
    return appPosition || term;
  };

  const positionText = getPositionText();

  const shouldShowPositionText =
    isRegistered && !isLoadingProfile && positionText;

  // Kiểm tra nếu có thông tin membership (kể cả khi chưa đăng nhập thành công)
  const hasMembershipInfo = membershipInfo && membershipInfo.fullname;
  const isPendingApproval = membershipInfo?.approvalStatus === 0;

  // Lấy tên và avatar từ membershipInfo hoặc infoZalo
  const displayName = hasMembershipInfo
    ? membershipInfo.fullname
    : isRegistered && infoZalo?.name
    ? infoZalo?.name
    : "Thành viên";

  // Ưu tiên avatar từ infoZalo nếu có, nếu không thì dùng SampleAvatar
  const displayAvatar = infoZalo?.avatar || SampleAvatar;

  useEffect(() => {
    if (!isRegistered) {
      setPhoneLoading(false);
    }
  }, [isRegistered, setPhoneLoading]);

  return (
    <Box
      style={{
        border: "1px solid #0066cc",
        borderRadius: "8px",
        padding: "8px",
        display: "flex",
        alignItems: "center",
        justifyContent: "space-between",
        cursor: "default",
      }}
    >
      <Box
        flex
        alignItems="center"
        style={{
          gap: GlobalStyles.spacing.sm,
        }}
      >
        <Box
          style={{
            padding: 4,
            display: "inline-flex",
            alignItems: "center",
            justifyContent: "center",
          }}
        >
          <Avatar
            src={displayAvatar}
            size={40}
            style={{
              borderRadius: "50%",
            }}
          />
        </Box>

        <Box
          style={{
            display: "flex",
            flexDirection: "column",
            lineHeight: 1.5,
            whiteSpace: "nowrap",
          }}
        >
          {shouldShowPositionText ? (
            <div style={{ fontWeight: "bold" }}>{positionText}</div>
          ) : (
            <div style={{ fontWeight: "bold" }}>Chào mừng</div>
          )}
          <span className="text-sub">{displayName}</span>
        </Box>
      </Box>
      {isRegistered ? (
        <Box
          flex
          alignItems="center"
          justifyContent="space-between"
          style={{
            gap: GlobalStyles.spacing.md,
          }}
        >
          {isInfo && (
            <Box flex alignItems="flex-end" flexDirection="column">
              {shareInfo?.rank?.name && (
                <Box
                  style={{
                    color: "#0092F3",
                    fontSize: 14,
                  }}
                >
                  {shareInfo.rank.name}
                </Box>
              )}
            </Box>
          )}
          {isInfo && <EditIcon size={20} color="var(--primary-color)" />}
        </Box>
      ) : (
        <Box
          style={{
            background: isPendingApproval ? "#E5E7EB" : "#0066cc",
            color: isPendingApproval ? "#9CA3AF" : "#fff",
            borderRadius: "8px",
            padding: "8px 12px",
            cursor: loadingPhone || isPendingApproval ? "default" : "pointer",
            fontWeight: "600",
            opacity: isPendingApproval ? 0.6 : 1,
          }}
          onClick={() => {
            if (!loadingPhone && !isPendingApproval) {
              navigate("/giba/app-brief");
            }
          }}
        >
          {loadingPhone ? (
            <Box mr={8}>
              <Spin size="small" />
            </Box>
          ) : isPendingApproval ? (
            "Đang chờ duyệt"
          ) : (
            "Đăng ký thành viên"
          )}
        </Box>
      )}
    </Box>
  );
};

export default HelloCard;
