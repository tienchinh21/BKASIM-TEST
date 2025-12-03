import React, { useState, useEffect, useRef } from "react";
import { useNavigate, Page, useSnackbar } from "zmp-ui";
import { useRecoilState } from "recoil";
import { useLocation } from "react-router-dom";
import {
  isRegister,
  infoUser,
  phoneNumberUser,
  token,
  userMembershipInfo,
  isFollowedOA,
} from "../../recoil/RecoilState";
import MyGroupsSwiper from "../../componentsGiba/MyGroupsSwiper";
import ComingSoonSwiper from "../../componentsGiba/ComingSoonSwiper";
import useSetHeader from "../../components/hooks/useSetHeader";
import FollowOACard from "../../pages/home/FollowOACard";
import ApprovalStatusBadge from "../../componentsGiba/ApprovalStatusBadge";
import AchievementsSection from "./components/AchievementsSection";
import axios from "axios";
import dfData from "../../common/DefaultConfig.json";
import {
  getUserInfo,
  getPhoneNumber,
  getAccessToken,
  getSetting,
} from "zmp-sdk/apis";
import { fixFormatPhoneNumber } from "../../common/Common";
import { toast } from "react-toastify";
import HelloCard from "./components/HelloCard";
import EventInvitationsSection from "../../componentsGiba/EventInvitationsSection";

interface News {
  id: string;
  title: string;
  image: string;
  date: string;
}

const HomeGiba: React.FC = () => {
  const location = useLocation();
  const setHeader = useSetHeader();
  const [isLoggedIn, setIsLoggedIn] = useRecoilState(isRegister);
  const [userInfo, setUserInfo] = useRecoilState(infoUser);
  const [userToken, setUserToken] = useRecoilState(token);
  const [phoneUser, setPhoneUser] = useRecoilState(phoneNumberUser);
  const [membershipInfo, setMembershipInfo] =
    useRecoilState(userMembershipInfo);
  const [isFollowedOAState, setIsFollowedOA] = useRecoilState(isFollowedOA);
  const snackbar = useSnackbar();
  const [news, setNews] = useState<News[]>([]);
  const [refreshTrigger, setRefreshTrigger] = useState(0);
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
  const hasCalledLogin = useRef(false);
  const profileFetchedRef = useRef(false);

  const triggerRefresh = () => {
    setRefreshTrigger((prev) => prev + 1);
  };

  useEffect(() => {
    if (location.state?.refresh) {
      triggerRefresh();
      window.history.replaceState({}, document.title);
    }
  }, [location.state]);

  const getInfoUserZalo = async () => {
    const infoZaloPromise = await new Promise((resolve) => {
      getUserInfo({
        autoRequestPermission: false,
        success: (data) => {
          const { userInfo } = data;
          resolve(userInfo);
        },
        fail: (error) => {
          resolve({});
        },
      });
    });
    return infoZaloPromise;
  };

  const getInfoPhonenumber = async () => {
    try {
      const accessToken = await getAccessToken({});
      if (!accessToken) {
        return "";
      }

      const phoneNumberPromise = await new Promise((resolve) => {
        getPhoneNumber({
          success: async (data) => {
            try {
              const { token } = data;
              const response = await axios.post(
                `${dfData.domain}/api/ZaloHelperApi/GetPhoneNumber`,
                {
                  accessToken: accessToken,
                  tokenNumber: token,
                  secretKey: dfData.secretKey,
                }
              );

              let phoneNumber = response.data.data.number;

              if (phoneNumber.startsWith("84")) {
                phoneNumber = "0" + phoneNumber.slice(2);
              }
              resolve(phoneNumber);
            } catch (error) {
              console.log("Error fetching phone number:", error);
              resolve("");
            }
          },
          fail: (error) => {
            console.log("Error getting phone number:", error);
            resolve("");
          },
        });
      });

      return phoneNumberPromise;
    } catch (error) {
      console.log("Error in getInfoPhonenumber:", error);
      return "";
    }
  };

  const reAuthenticate = async () => {
    try {
      let zaloInfoData: any = userInfo;
      let phonenumber = phoneUser;

      if (!zaloInfoData || !zaloInfoData.id) {
        zaloInfoData = await getInfoUserZalo();
        if (zaloInfoData) {
          setUserInfo(zaloInfoData);
        }
      }

      if (!phonenumber) {
        phonenumber = (await getInfoPhonenumber()) as string;
        if (phonenumber) {
          setPhoneUser(phonenumber);
        }
      }
      if (!zaloInfoData?.id || !phonenumber) {
        console.log("Không đủ thông tin để login");
        return;
      }

      const formData = new FormData();
      formData.append("UserZaloId", zaloInfoData.id);
      formData.append("phoneNumber", fixFormatPhoneNumber(phonenumber));

      const res = await axios.post(
        `${dfData.domain}/api/auth/miniapp-login`,
        formData,
        {
          headers: {
            "Content-Type": "multipart/form-data",
          },
        }
      );

      if (res.data.success === false) {
        return;
      }

      if (res.data.data.isLoginSuccess === false) {
        const userInfo = res.data.data.userInfo || {};

        setIsFollowedOA(true);

        const userInfoForDisplay = {
          id: userInfo.userZaloId || zaloInfoData?.id,
          name: userInfo.fullname || zaloInfoData?.name || "User",
          avatar: userInfo.zaloAvatar || zaloInfoData?.avatar || null,
          roleId: userInfo.roleId || "",
          roleName: userInfo.roleName || "",
          idByOA: userInfo.idByOA || zaloInfoData?.idByOA || "",
        };
        setUserInfo(userInfoForDisplay);

        let approvalStatus = 1; // Default to approved since system auto-approves
        const messageToCheck = res.data.data?.message || res.data.message || "";

        if (messageToCheck.includes("đã bị từ chối")) {
          approvalStatus = 2;
        } else if (messageToCheck.includes("đang chờ duyệt")) {
          approvalStatus = 0;
        }

        setMembershipInfo({
          id: userInfo.id || "",
          userZaloId: userInfo.userZaloId || zaloInfoData?.id,
          phoneNumber: userInfo.phoneNumber || phonenumber,
          fullname: userInfo.fullname || zaloInfoData?.name || "",
          approvalStatus: approvalStatus as any,
          roleId: userInfo.roleId || "",
          roleName: userInfo.roleName || "",
          idByOA: userInfo.idByOA || zaloInfoData?.idByOA || "",
        } as any);

        setIsLoggedIn(false);
        setUserToken("");
        return;
      }

      if (res.data.data.token) {
        const membership =
          res.data.data.membership || res.data.data.userInfo || {};
        setMembershipInfo({
          id: membership.id || "",
          userZaloId: membership.userZaloId || zaloInfoData?.id,
          phoneNumber: membership.phoneNumber || phonenumber,
          fullname: membership.fullname || zaloInfoData?.name || "",
          approvalStatus: (membership.approvalStatus !== undefined
            ? membership.approvalStatus
            : 1) as any,
          roleId: membership.roleId || "",
          roleName: membership.roleName || "",
          idByOA: membership.idByOA || zaloInfoData?.idByOA || "",
        } as any);

        const userInfoForDisplay = {
          id: membership.userZaloId || zaloInfoData?.id,
          name: membership.fullname || zaloInfoData?.name || "User",
          avatar: membership.zaloAvatar || zaloInfoData?.avatar || null,
          roleId: membership.roleId || "",
          roleName: membership.roleName || "",
          idByOA: membership.idByOA || zaloInfoData?.idByOA || "",
        };

        setUserInfo(userInfoForDisplay);
        setUserToken(res.data.data.token);
        setIsLoggedIn(true);
        setIsFollowedOA(true);
      }
    } catch (error: any) {
      console.error("Error in reAuthenticate:", error);

      if (error.response?.data) {
        const errorData = error.response.data.data || {};

        if (errorData.isLoginSuccess === false) {
          const userInfo = errorData.userInfo || {};

          const userInfoForDisplay = {
            id: userInfo.userZaloId || (userInfo as any)?.id,
            name: userInfo.fullname || (userInfo as any)?.name || "User",
            avatar: userInfo.zaloAvatar || (userInfo as any)?.avatar || null,
            roleId: userInfo.roleId || "",
            roleName: userInfo.roleName || "",
            idByOA: userInfo.idByOA || (userInfo as any)?.idByOA || "",
          };
          setUserInfo(userInfoForDisplay);

          setMembershipInfo({
            id: userInfo.id || "",
            userZaloId: userInfo.userZaloId || "",
            phoneNumber: userInfo.phoneNumber || "",
            fullname: userInfo.fullname || "",
            approvalStatus: 0 as any,
            roleId: userInfo.roleId || "",
            roleName: userInfo.roleName || "",
            idByOA: userInfo.idByOA || "",
          } as any);

          setIsLoggedIn(false);
          setUserToken("");

          toast.warning(errorData.message || "Tài khoản đang chờ duyệt", {
            position: "top-center",
            autoClose: 5000,
          });
        }
      }
    }
  };

  useEffect(() => {
    const checkPermissionAndAuthenticate = async () => {
      if (hasCalledLogin.current) {
        return;
      }

      try {
        const settingData = await getSetting({});
        const hasUserInfo =
          settingData?.authSetting?.["scope.userInfo"] !== false;
        const hasPhoneNumber =
          settingData?.authSetting?.["scope.userPhonenumber"] !== false;
        if (hasUserInfo && hasPhoneNumber) {
          hasCalledLogin.current = true;
          await reAuthenticate();
        } else {
          console.log(
            "Người dùng chưa cấp quyền số điện thoại, không gọi API login"
          );
        }
      } catch (error) {
        console.error("Error checking permissions:", error);
      }
    };

    checkPermissionAndAuthenticate();
  }, []);

  useEffect(() => {
    const fetchUserProfile = async () => {
      if (!isLoggedIn || !userToken) {
        if (profileFetchedRef.current) {
          setUserProfileData({
            slug: null,
            term: null,
            appPosition: null,
          });
          setIsLoadingProfile(false);
          profileFetchedRef.current = false;
        }
        return;
      }

      // Prevent duplicate fetches
      if (profileFetchedRef.current) {
        return;
      }

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
        }
      } catch (error) {
        console.error("Error fetching user profile:", error);
        profileFetchedRef.current = false;
      } finally {
        setIsLoadingProfile(false);
      }
    };

    fetchUserProfile();
  }, [isLoggedIn, userToken]);

  useEffect(() => {
    // @ts-ignore
    const userName = isLoggedIn
      ? // @ts-ignore
        userInfo?.name || "Đăng nhập tại đây"
      : "Đăng nhập tại đây";
    // @ts-ignore
    const userAvatar = isLoggedIn ? userInfo?.avatar || null : null;

    setHeader({
      title: "TRANG CHỦ",
      showUserInfo: false,
      showMenuButton: false,
      showCloseButton: false,
      hasLeftIcon: false,
    } as any);
  }, [setHeader]);

  return (
    <Page className="bg-white min-h-screen mt-[50px]">
      <div className="relative">
        {membershipInfo &&
          membershipInfo.approvalStatus !== null &&
          membershipInfo.approvalStatus !== 1 && (
            <div className="relative z-40 mx-4 pt-3">
              <div
                className={`p-3 rounded-r-lg shadow-md border-l-4 ${
                  membershipInfo.approvalStatus === 0
                    ? "bg-yellow-50 border-yellow-400"
                    : "bg-red-50 border-red-400"
                }`}
              >
                <div className="flex items-start gap-3">
                  <div className="flex-shrink-0 pt-0.5">
                    <svg
                      className={`w-5 h-5 ${
                        membershipInfo.approvalStatus === 0
                          ? "text-yellow-600"
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
                      className={`text-sm font-medium ${
                        membershipInfo.approvalStatus === 0
                          ? "text-yellow-800"
                          : "text-red-800"
                      }`}
                    >
                      {membershipInfo.approvalStatus === 0 &&
                        "Tài khoản đang chờ duyệt"}
                      {membershipInfo.approvalStatus === 2 &&
                        "Tài khoản đã bị từ chối"}
                    </p>
                    <p
                      className={`text-xs mt-1 ${
                        membershipInfo.approvalStatus === 0
                          ? "text-yellow-700"
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
        <div className="relative z-30 mx-4 py-2 mb-4">
          <HelloCard
            isInfo={false}
            term={userProfileData.term}
            appPosition={userProfileData.appPosition}
            isLoadingProfile={isLoadingProfile}
          />
        </div>

        <div className="bg-white -mt-20 rounded-t-2xl relative z-20 px-4 pt-6 pb-14">
          <div className="mb-6 mt-10">
            {!isFollowedOAState && <FollowOACard />}

            <div className="mt-8">
              <ComingSoonSwiper refreshTrigger={refreshTrigger} />
            </div>

            {(isLoggedIn || (membershipInfo && membershipInfo.userZaloId)) && (
              <div className="mt-8">
                <EventInvitationsSection
                  onInvitationConfirmed={triggerRefresh}
                  refreshTrigger={refreshTrigger}
                />
              </div>
            )}

            {isLoggedIn && userToken && (
              <>
                <div className="mt-8">
                  <AchievementsSection />
                </div>
                <div className="mt-8">
                  <div className="flex items-center justify-between mb-4">
                    <h3 className="text-lg font-bold text-black">
                      Nhóm của tôi
                    </h3>
                  </div>
                  <MyGroupsSwiper refreshTrigger={refreshTrigger} />
                </div>
              </>
            )}
          </div>
        </div>
      </div>
    </Page>
  );
};

export default HomeGiba;
