import React, { useEffect } from "react";
import { Avatar, Box, Icon } from "zmp-ui";
import { useRecoilValue } from "recoil";
import {
  headerState,
  storeCurrent,
  isRegister,
  infoUser,
} from "../../recoil/RecoilState";
import { useLocation, useNavigate } from "react-router";
import { ChevronLeft } from "lucide-react";
import { isEmpty } from "../../common/Common";

import SampleAvatar from "../../assets/user_simple.png";

const typeColor = {
  primary: {
    headerColor: "bg-primary",
    textColor: "text-white",
    iconColor: "text-white",
  },
  secondary: {
    headerColor: "bg-white",
    textColor: "text-black",
    iconColor: "text-gray-400",
  },
};

const HeaderCustom = () => {
  const {
    route,
    hasLeftIcon,
    rightIcon,
    title,
    customTitle,
    type,
    isShowHeader,
    onBack,
    // Giba specific props
    userInfo,
    showUserInfo,
    showMenuButton,
    showCloseButton,
    onMenuClick,
    onCloseClick,
    isGibaHeader,
    groupLogo,
  } = useRecoilValue(headerState);

  const storeCurrentValue = useRecoilValue(storeCurrent);
  const isLoggedIn = useRecoilValue(isRegister);
  console.log("isLoggedIn123: ", isLoggedIn);

  const infoZalo = useRecoilValue(infoUser);
  const { headerColor, textColor, iconColor } = typeColor[type || "primary"];
  const navigate = useNavigate();
  const location = useLocation();

  const handleBackButton = () => {
    // if (!isEmpty(storeCurrentValue)) {
    //   if (route) {
    //     navigate(route);
    //   } else {
    //     navigate(-1);
    //   }
    // } else {
    //   navigate("/welcome");
    // }
    if (onBack) {
      onBack();
    } else if (route) {
      navigate(route);
    } else {
      if (location.key !== "default") {
        navigate(-1);
      } else {
        navigate("/home"); // fallback
      }
    }
  };

  const handleHeaderClick = () => {
    console.log("isLoggedIn: ", isLoggedIn);
    if (!isLoggedIn) {
      navigate("/giba/app-brief");
    }
  };

  return (
    <>
      {isShowHeader && (
        <div
          className={"header"}
          style={{
            display: "flex",
            flexDirection: "row",
            background: isGibaHeader
              ? "#003d82"
              : "var(--background-header-color)",
            height: isGibaHeader ? 60 : 50,
            padding: isGibaHeader ? "16px 8px" : "0 16px",
            alignItems: "center",
            justifyContent: "space-between",
          }}
          onClick={isGibaHeader && !isLoggedIn ? handleHeaderClick : undefined}
        >
          <div style={{ display: "flex", alignItems: "center", gap: "12px" }}>
            {isGibaHeader && showUserInfo ? (
              <div
                style={{ display: "flex", alignItems: "center", gap: "12px" }}
              >
                <div
                  style={{
                    width: "48px",
                    height: "48px",
                    borderRadius: "50%",
                    display: "flex",
                    alignItems: "center",
                    justifyContent: "end",
                  }}
                >
                  {userInfo?.avatar ? (
                    <img
                      src={userInfo.avatar}
                      alt={userInfo.name}
                      style={{
                        width: "42px",
                        height: "42px",
                        borderRadius: "50%",
                        objectFit: "cover",
                        border: "2px solid #fff",
                      }}
                    />
                  ) : (
                    <Avatar
                      src={SampleAvatar}
                      alt="avatar"
                      style={{
                        width: "42px",
                        height: "42px",
                        border: "2px solid #fff",
                      }}
                    />
                  )}
                </div>
                <div>
                  <div style={{ color: "#FFF", fontSize: "14px" }}>
                    Xin chào,
                  </div>
                  <div
                    style={{
                      color: "#FFF",
                      fontSize: "17px",
                      fontWeight: "600",
                    }}
                  >
                    {userInfo?.name || "Guest"}
                  </div>
                </div>
              </div>
            ) : hasLeftIcon ? (
              <>
                <span onClick={() => handleBackButton()}>
                  <ChevronLeft size={30} style={{ color: "#FFF" }} />
                </span>
              </>
            ) : null}
          </div>

          {/* Center - Title */}
          {!isGibaHeader && (
            <div
              style={{
                position: "relative",
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
                flex: 1,
              }}
            >
              {groupLogo &&
                (() => {
                  const titleText = title || "";
                  const titleLength = titleText.length;
                  // Nếu tên dài hơn 15 ký tự, logo di chuyển sang trái hơn
                  const logoLeft = titleLength > 15 ? "20%" : "15%";
                  return (
                    <div
                      style={{
                        position: "absolute",
                        left: logoLeft,
                        width: "36px",
                        height: "36px",
                        borderRadius: "50%",
                        overflow: "hidden",
                        border: "2px solid #fff",
                        flexShrink: 0,
                      }}
                    >
                      <img
                        src={groupLogo}
                        alt="Group logo"
                        style={{
                          width: "100%",
                          height: "100%",
                          objectFit: "cover",
                        }}
                      />
                    </div>
                  );
                })()}
              {customTitle ? (
                <div>{customTitle}</div>
              ) : (
                <b
                  style={{
                    fontWeight: "bold",
                    fontSize: 16,
                    color: "#FFF",
                    textAlign: "center",
                    fontWeight: 800,
                    marginRight: hasLeftIcon ? "30px" : "12px",
                    paddingLeft: groupLogo ? "65px" : 0,
                    paddingRight: "12px",
                    whiteSpace: "nowrap",
                    overflow: "hidden",
                    textOverflow: "ellipsis",
                  }}
                >
                  {title}
                </b>
              )}
            </div>
          )}

          {/* Right side - Action buttons */}
          <div style={{ display: "flex", alignItems: "center", gap: "8px" }}>
            {!isGibaHeader && (rightIcon || " ")}
          </div>
        </div>
      )}
    </>
  );
};

export default HeaderCustom;
