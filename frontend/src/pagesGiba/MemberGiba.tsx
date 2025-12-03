import React, { useState, useEffect, useRef } from "react";
import { Page, useSnackbar } from "zmp-ui";
import { useNavigate } from "zmp-ui";
import { useRecoilValue, useRecoilState } from "recoil";
import {
  infoUser,
  isRegister,
  userMembershipInfo,
  token,
} from "../recoil/RecoilState";
import useSetHeader from "../components/hooks/useSetHeader";
import MemberCardGiba from "../componentsGiba/MemberCardGiba";
import MenuItemGiba, { menuItems } from "../componentsGiba/MenuItemGiba";
import SectionTitleGiba from "../componentsGiba/SectionTitleGiba";
import ApprovalStatusBadge from "../componentsGiba/ApprovalStatusBadge";
import { getManagementMenuItems } from "../componentsGiba/ManagementMenuItems";
import { useGetUserRole } from "../hooks/useHasRole";
import MemberHelloCard from "../componentsGiba/MemberHelloCard";
import axios from "axios";
import dfData from "../common/DefaultConfig.json";
import HelloCard from "./home/components/HelloCard";

const MemberGiba: React.FC = () => {
  const navigate = useNavigate();
  const setHeader = useSetHeader();
  const snackbar = useSnackbar();
  const [userInfo, setUserInfo] = useRecoilState(infoUser);
  const [isLoggedIn, setIsLoggedIn] = useRecoilState(isRegister);
  const membershipInfo = useRecoilValue(userMembershipInfo);
  const userToken = useRecoilValue(token);
  const userRole = useGetUserRole();

  const [userProfile, setUserProfile] = useState<any>(null);
  const [loading, setLoading] = useState(false);
  const [userProfileData, setUserProfileData] = useState<{
    slug: string | null;
    term: string | null;
    appPosition: string | null;
  }>({
    slug: null,
    term: null,
    appPosition: null,
  });
  const [isLoadingProfile, setIsLoadingProfile] = useState(false);
  const profileFetchedRef = useRef(false);

  const managementMenuItems = getManagementMenuItems(userRole);

  const fetchUserProfile = async () => {
    if (!userToken || !isLoggedIn) {
      return;
    }

    try {
      setLoading(true);
      const response = await axios.get(
        `${dfData.domain}/api/memberships/profile`,
        {
          headers: {
            Authorization: `Bearer ${userToken}`,
            "Content-Type": "application/json",
          },
        }
      );

      if (response.data.success && response.data.data) {
        console.log("User profile data:", response.data.data);
        setUserProfile(response.data.data);
      }
    } catch (error) {
      console.error("Error fetching user profile:", error);
    } finally {
      setLoading(false);
    }
  };

  // Fetch user profile data (slug, term, appPosition) - similar to HomeGiba
  useEffect(() => {
    const fetchUserProfileData = async () => {
      // Reset profile data if user logged out
      if (!isLoggedIn || !userToken) {
        setUserProfileData({
          slug: null,
          term: null,
          appPosition: null,
        });
        setIsLoadingProfile(false);
        profileFetchedRef.current = false;
        return;
      }

      // Always fetch when component mounts or token changes
      // Reset ref to allow fresh fetch each time
      profileFetchedRef.current = false;

      try {
        setIsLoadingProfile(true);
        const response = await axios.get(
          `${dfData.domain}/api/memberships/profile`,
          {
            headers: {
              Authorization: `Bearer ${userToken}`,
              "Content-Type": "application/json",
            },
          }
        );

        if (response.data.success && response.data.data) {
          const profileData = response.data.data;

          setUserProfileData({
            slug: profileData.slug || null,
            term: profileData.term || null,
            appPosition: profileData.appPosition || null,
          });
          profileFetchedRef.current = true;
        } else {
          console.log("MemberGiba - No profile data in response");
        }
      } catch (error) {
        console.error("Error fetching user profile:", error);
        // Reset on error to allow retry
        profileFetchedRef.current = false;
      } finally {
        setIsLoadingProfile(false);
      }
    };

    fetchUserProfileData();
  }, [isLoggedIn, userToken]);

  React.useEffect(() => {
    setHeader({
      title: "CÀI ĐẶT",
      hasLeftIcon: true,
      showUserInfo: false,
      showMenuButton: false,
      showCloseButton: false,
    } as any);
  }, [setHeader]);

  const handleMemberCardClick = () => {
    navigate("/giba/login");
  };

  const handleMenuItemClick = (itemId: string) => {
    if (!isLoggedIn) {
      snackbar.openSnackbar({
        text: "Vui lòng đăng nhập để sử dụng tính năng này!",
        type: "warning",
        duration: 4000,
        action: {
          text: "Đóng",
          close: true,
        },
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
        action: {
          text: "Đóng",
          close: true,
        },
      });
      return;
    }

    if (membershipInfo.approvalStatus === 2) {
      snackbar.openSnackbar({
        text: "Tài khoản của bạn đã bị từ chối. Vui lòng liên hệ admin.",
        type: "error",
        duration: 4000,
        action: {
          text: "Đóng",
          close: true,
        },
      });
      return;
    }

    switch (itemId) {
      case "dashboard":
        navigate("/giba/dashboard");
        break;
      case "edit-info":
        navigate("/giba/edit-info");
        break;
      case "1":
        navigate("/giba/ref-list");
        break;
      case "2":
        navigate("/giba/guest-list-history");
        break;
      case "3":
        navigate("/giba/group-join-request-history");
        break;
      case "event-registration-history":
        navigate("/giba/event-registration-history");
        break;
      case "profile-intro":
        const userSlug = userProfileData?.slug;
        if (userSlug) {
          navigate(`/giba/profile-intro?slug=${userSlug}`);
        } else {
          navigate("/giba/profile-intro");
        }
        break;
      case "point-history":
        break;
      case "address-book":
        break;
      default:
        break;
    }
  };

  const handleSubActionClick = (itemId: string) => {
    if (!isLoggedIn) {
      snackbar.openSnackbar({
        text: "Vui lòng đăng nhập để sử dụng tính năng này!",
        type: "warning",
        duration: 4000,
        action: {
          text: "Đóng",
          close: true,
        },
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
        action: {
          text: "Đóng",
          close: true,
        },
      });
      return;
    }

    if (membershipInfo.approvalStatus === 2) {
      snackbar.openSnackbar({
        text: "Tài khoản của bạn đã bị từ chối. Vui lòng liên hệ admin.",
        type: "error",
        duration: 4000,
        action: {
          text: "Đóng",
          close: true,
        },
      });
      return;
    }

    if (itemId === "1") {
      navigate("/giba/ref-list");
    } else if (itemId === "2") {
      navigate("/giba/guest-list-history");
    } else if (itemId === "3") {
      navigate("/giba/group-join-request-history");
    }
  };

  const handleManagementMenuClick = (
    itemId: string,
    isDevelopment?: boolean
  ) => {
    if (!isLoggedIn) {
      snackbar.openSnackbar({
        text: "Vui lòng đăng nhập để sử dụng tính năng này!",
        type: "warning",
        duration: 4000,
        action: {
          text: "Đóng",
          close: true,
        },
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
        action: {
          text: "Đóng",
          close: true,
        },
      });
      return;
    }

    if (membershipInfo.approvalStatus === 2) {
      snackbar.openSnackbar({
        text: "Tài khoản của bạn đã bị từ chối. Vui lòng liên hệ admin.",
        type: "error",
        duration: 4000,
        action: {
          text: "Đóng",
          close: true,
        },
      });
      return;
    }

    if (isDevelopment) {
      snackbar.openSnackbar({
        text: "Tính năng đang phát triển. Vui lòng quay lại sau!",
        type: "info",
        duration: 4000,
        action: {
          text: "Đóng",
          close: true,
        },
      });
      return;
    }

    switch (itemId) {
      case "manage-members":
        navigate("/giba/manager-membership");
        break;
      case "manage-groups":
        navigate("/giba/manager-club");
        break;
      case "manage-articles":
        navigate("/giba/manager-articles");
        break;
      case "manage-showcase":
        navigate("/giba/showcase-list", { state: { isAdminMode: true } });
        break;
      case "manage-meetings":
        navigate("/giba/meeting-list", { state: { isAdminMode: true } });
        break;
      case "manage-training":
        navigate("/giba/manager-training");
        break;
      case "manage-reports":
        snackbar.openSnackbar({
          text: "Trang thống kê & báo cáo đang được phát triển",
          type: "info",
          duration: 3000,
        });
        break;
      default:
        break;
    }
  };

  return (
    <Page className="bg-gray-50 min-h-screen mt-[50px] pb-20">
      <div className="relative">
        <div className="relative z-30 mx-3 pt-2 mb-4">
          <HelloCard
            isInfo={false}
            term={userProfileData.term}
            appPosition={userProfileData.appPosition}
            isLoadingProfile={isLoadingProfile}
          />
        </div>
        <div className="relative z-30 mx-3">
          <MemberHelloCard
            term={userProfileData.term}
            appPosition={userProfileData.appPosition}
            isLoadingProfile={isLoadingProfile}
          />
        </div>

        {isLoggedIn &&
          membershipInfo &&
          membershipInfo.approvalStatus !== null &&
          membershipInfo.approvalStatus !== 1 && (
            <div className="relative z-40 mx-4 pt-3">
              <div
                className={`border-l-4 p-4 rounded-r-lg shadow-md ${
                  membershipInfo.approvalStatus === 0
                    ? "bg-blue-50 border-blue-400"
                    : "bg-red-50 border-red-400"
                }`}
              >
                <div className="flex items-start gap-3">
                  <div className="flex-shrink-0 pt-0.5">
                    <svg
                      className={`w-5 h-5 ${
                        membershipInfo.approvalStatus === 0
                          ? "text-blue-600"
                          : "text-red-600"
                      }`}
                      fill="currentColor"
                      viewBox="0 0 20 20"
                    >
                      <path
                        fillRule="evenodd"
                        d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z"
                        clipRule="evenodd"
                      />
                    </svg>
                  </div>
                  <div className="flex-1 min-w-0">
                    <p
                      className={`text-sm font-semibold ${
                        membershipInfo.approvalStatus === 0
                          ? "text-blue-900"
                          : "text-red-900"
                      }`}
                    >
                      {membershipInfo.approvalStatus === 0 &&
                        "⏳ Tài khoản đang chờ duyệt"}
                      {membershipInfo.approvalStatus === 2 &&
                        "❌ Tài khoản đã bị từ chối"}
                    </p>
                    <p
                      className={`text-xs mt-1.5 ${
                        membershipInfo.approvalStatus === 0
                          ? "text-blue-700"
                          : "text-red-700"
                      }`}
                    >
                      {membershipInfo.approvalStatus === 0 &&
                        "Một số tính năng sẽ bị hạn chế cho đến khi admin phê duyệt tài khoản của bạn."}
                      {membershipInfo.approvalStatus === 2 &&
                        "Vui lòng liên hệ admin để biết thêm chi tiết."}
                    </p>
                  </div>
                  <ApprovalStatusBadge
                    status={membershipInfo.approvalStatus}
                    size="small"
                    showIcon={false}
                  />
                </div>
              </div>
            </div>
          )}

        <div className="relative bg-white rounded-t-2xl relative z-20 px-3 pb-14">
          <div className="bg-white rounded-lg border border-gray-200 overflow-hidden mt-4 shadow-sm">
            <SectionTitleGiba title="Tính năng cá nhân" />
            {menuItems.map((item) => (
              <MenuItemGiba
                key={item.id}
                id={item.id}
                title={item.title}
                subtitle={item.subtitle}
                icon={item.icon}
                onClick={() => handleMenuItemClick(item.id)}
                showSubAction={item.showSubAction}
                subActionText={item.subActionText}
                onSubActionClick={() => handleSubActionClick(item.id)}
              />
            ))}
          </div>

          {/* {isLoggedIn && managementMenuItems.length > 0 && (
            <div className="bg-white rounded-xl border border-gray-100 overflow-hidden mt-4">
              <SectionTitleGiba title="Quản lý hội nhóm" />
              {managementMenuItems.map((item) => (
                <MenuItemGiba
                  key={item.id}
                  id={item.id}
                  title={item.title}
                  subtitle={item.subtitle}
                  icon={item.icon}
                  onClick={() =>
                    handleManagementMenuClick(item.id, item.isDevelopment)
                  }
                />
              ))}
            </div>
          )} */}
        </div>
      </div>
    </Page>
  );
};

export default MemberGiba;
