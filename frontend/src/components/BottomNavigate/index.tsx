import React, { useEffect, useState } from "react";
import { useNavigate, Page, useSnackbar } from "zmp-ui";
import { useRecoilState, useRecoilValue } from "recoil";
import {
  actionTab,
  cartState,
  featureConfig,
  isRegister,
  userMembershipInfo,
  token,
} from "../../recoil/RecoilState";
import { checkFeatureShow, shortNum } from "../../common/Common";
import usePlatform from "../../hooks/usePlatform";
import { Home, Users, Settings, User } from "lucide-react";
import { useLocation } from "react-router-dom";
import axios from "axios";
import dfData from "../../common/DefaultConfig.json";
import "./index.css";

interface BottomNavigationPageProps {
  forceShow?: boolean;
}

const BottomNavigationPage: React.FC<BottomNavigationPageProps> = ({
  forceShow = false,
}) => {
  const navigate = useNavigate();
  const location = useLocation();
  const snackbar = useSnackbar();
  const listFeature = useRecoilValue(featureConfig);
  const [activeTab, setActiveTab] = useRecoilState(actionTab);
  const [totalCartQuantity, setTotalCartQuantity] = useRecoilState(cartState);
  const isLoggedIn = useRecoilValue(isRegister);
  const membershipInfo = useRecoilValue(userMembershipInfo);
  const userToken = useRecoilValue(token);
  const platform = usePlatform();
  const path = location.pathname;
  const [userSlug, setUserSlug] = useState<string | null>(null);

  useEffect(() => {
    const fetchUserProfile = async () => {
      if (!isLoggedIn || !userToken) {
        setUserSlug(null);
        return;
      }

      try {
        const response = await axios.get(
          `${dfData.domain}/api/memberships/profile`,
          {
            headers: {
              Authorization: `Bearer ${userToken}`,
              "Content-Type": "application/json",
            },
          }
        );

        if (response.data.success && response.data.data?.slug) {
          setUserSlug(response.data.data.slug);
        } else {
          setUserSlug(null);
        }
      } catch (error) {
        console.error("Error fetching user profile:", error);
        setUserSlug(null);
      }
    };

    fetchUserProfile();
  }, [isLoggedIn, userToken]);

  useEffect(() => {
    // Skip auto-navigation when forceShow is true (e.g., on member detail page)
    if (!forceShow) {
      handleScreenByTab(activeTab);
    }
  }, [activeTab, forceShow]);

  useEffect(() => {
    const currentPath = location.pathname;

    if (
      currentPath === "/giba" ||
      currentPath === "/" ||
      currentPath.includes("/s/") ||
      currentPath.includes("/zapps/")
    ) {
      setActiveTab("home");
    } else if (currentPath === "/giba/groups") {
      setActiveTab("groups");
    } else if (
      currentPath === "/giba/profile" ||
      currentPath.includes("/giba/member-detail")
    ) {
      setActiveTab("profile");
    } else if (currentPath === "/giba/member") {
      setActiveTab("me");
    }
  }, [location.pathname]);

  const handleScreenByTab = async (text) => {
    switch (text) {
      case "home":
        navigate("/giba", { animate: false });
        break;
      case "groups":
        navigate("/giba/groups", { animate: false });
        break;
      case "me":
        navigate("/giba/member", { animate: false });
        break;
      case "event":
        navigate("/giba/event", { animate: false });
        break;
      default:
        break;
    }
  };

  const handleTabChange = (key: string): void => {
    // Handle profile tab click
    if (key === "profile") {
      if (!isLoggedIn) {
        snackbar.openSnackbar({
          text: "Vui lòng đăng nhập để sử dụng tính năng này!",
          type: "warning",
          duration: 4000,
        });
        return;
      }
      if (
        membershipInfo.approvalStatus === 0 ||
        membershipInfo.approvalStatus === null
      ) {
        snackbar.openSnackbar({
          text: "Tài khoản đang trong thời gian xét duyệt. Vui lòng chờ admin phê duyệt.",
          type: "warning",
          duration: 4000,
        });
        return;
      }
      if (membershipInfo.approvalStatus === 2) {
        snackbar.openSnackbar({
          text: "Tài khoản đã bị từ chối. Vui lòng liên hệ admin.",
          type: "error",
          duration: 4000,
        });
        return;
      }

      if (userSlug) {
        navigate(`/giba/member-detail/${userSlug}`);
      } else {
        navigate("/giba/profile-membership");
      }
      return;
    }

    if (key === "event") {
      if (!isLoggedIn) {
        snackbar.openSnackbar({
          text: "Vui lòng đăng nhập để sử dụng tính năng này!",
          type: "warning",
          duration: 4000,
        });
        return;
      }
      if (
        membershipInfo.approvalStatus === 0 ||
        membershipInfo.approvalStatus === null
      ) {
        snackbar.openSnackbar({
          text: "Tài khoản đang trong thời gian xét duyệt. Vui lòng chờ admin phê duyệt.",
          type: "warning",
          duration: 4000,
        });
        return;
      }
      if (membershipInfo.approvalStatus === 2) {
        snackbar.openSnackbar({
          text: "Tài khoản đã bị từ chối. Vui lòng liên hệ admin.",
          type: "error",
          duration: 4000,
        });
        return;
      }
    }

    setActiveTab(key);
  };

  const navItems = [
    {
      key: "home",
      label: "Home",
      icon: <Home className="w-6 h-6" />,
    },
    {
      key: "groups",
      label: "Club",
      icon: <Users className="w-6 h-6" />,
    },
    {
      key: "profile",
      label: "Profile",
      icon: <User className="w-6 h-6" />,
    },
    {
      key: "me",
      label: "Setting",
      icon: <Settings className="w-6 h-6" />,
    },
  ];

  var checkPathAccept = (pathCheck) => {
    var listPathNotAccept = [
      "/giba/group-join-request-detail",
      "/event/success-join",
      "/giba/manager-club",
      "/giba/event",
      "/giba/app-brief",
      "/giba/achievements",
      "/giba/manager-articles",
      "/giba/manager-membership",
      "/giba/showcase-detail",
      "/giba/showcase-list",
      "/giba/showcase-create",
      "/giba/appointment-detail",
      "/giba/appointment-create",
      "/giba/ref-create",
      "/giba/appointment-list",
      "/giba/meeting-schedule",
      "/giba/meeting-list",
      "/giba/meeting-create",
      "/giba/meeting-detail",
      "/giba/train",
      "/giba/contact",
      "/giba/profile",
      "/giba/profile-intro",
      "/giba/edit-info",
      "/giba/member-detail",
      "/giba/ref-list",
      "/giba/ref-detail",
      "/giba/guest-list-history",
      "/giba/guest-list-detail",
      "/giba/group-join-request-history",
      "/giba/group-detail",
      "/giba/login",
      "/giba/dashboard",
      "/giba/groups/",
      "/giba/coming-soon",
      "cart",
      "welcome",
      "editInfo",
      "promotion",
      "affilate",
      "booking",
      "services-booking",
      "history-detail-booking",
      "success-booking",
      "detailBill",
      "detailNew",
      "payment",
      "statusBill",
      "gift",
      "paymentGift",
      "detail-product",
      "addressBook",
      "credit",
      "historyWithdraw",
      "game",
      "luckyWheel",
      "rules",
      "listGift",
      "historyWheel",
      "new",
      "policy",
      "businessInfo",
      "commission",
      "affiliate",
      "default",
      "detailPromotion",
      "rating",
      "register",
      "survey",
      "detail-survey",
      "rank",
      "rank-history",
      "vat",
      // "event",
      "/giba/event-detail",
      "check-in",
      "detail-sponsor",
    ];
    var check = listPathNotAccept.find((x) => pathCheck.includes(x));
    if (check) {
      return false;
    }
    return true;
  };

  return (
    <>
      {(forceShow || checkPathAccept(path)) && (
        <div
          className="
            fixed bottom-0 left-0 right-0 z-[1000]
            bg-white
            border-t border-gray-200
          "
          style={{
            paddingBottom:
              platform === "iOS" ? "env(safe-area-inset-bottom, 20px)" : "0px",
          }}
        >
          <div className="max-w-md mx-auto px-4">
            <nav className="flex items-center justify-around py-2">
              {navItems.map((item) => (
                <button
                  key={item.key}
                  onClick={() => handleTabChange(item.key)}
                  className="
                    flex flex-col items-center justify-center
                    min-w-[60px]
                    transition-colors duration-200
                  "
                >
                  {/* Icon */}
                  <div
                    className={`
                            mb-1
                            ${
                              activeTab === item.key
                                ? "text-blue-600"
                                : "text-gray-400"
                            }
                          `}
                  >
                    {item.icon}
                  </div>

                  {/* Label */}
                  <span
                    className={`
                            text-xs font-medium
                            ${
                              activeTab === item.key
                                ? "text-blue-600 font-bold"
                                : "text-gray-400"
                            }
                          `}
                  >
                    {item.label}
                  </span>
                </button>
              ))}
            </nav>
          </div>
        </div>
      )}
    </>
  );
};

export default BottomNavigationPage;
