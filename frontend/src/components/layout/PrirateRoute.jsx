import React, { useState, useEffect, useRef } from "react";
import { useNavigate, useParams, useLocation } from "react-router";
import { useRecoilState, useSetRecoilState } from "recoil";
import {
  getUserInfo,
  getPhoneNumber,
  getAccessToken,
  getSetting,
  followOA,
} from "zmp-sdk/apis";
import {
  infoUser,
  isRegister,
  phoneNumberUser,
  source,
  codeAffiliate,
token,
infoShare,
  isShareLocation,
  phoneNumberLoading,
  userMembershipInfo,
  isFollowedOA,
} from "../../recoil/RecoilState";
import dfData from "../../common/DefaultConfig.json";
import { fixFormatPhoneNumber, isEmpty } from "../../common/Common";
import { Spin } from "antd";
import { toast } from "react-toastify";

import axios from "axios";

export default function PrivateRoute({ children }) {
  const location = useLocation();
  const query = new URLSearchParams(location.search);
  const sourceParam = query.get("source");
  const refParam = query.get("ref");
  const [isRegistered, setIsRegistered] = useRecoilState(isRegister);

  const [infoZalo, setInfoZalo] = useRecoilState(infoUser);
  const [phoneUser, setPhoneUser] = useRecoilState(phoneNumberUser);
  const [referenceCode, setReferenceCode] = useRecoilState(codeAffiliate);
  const [tokenAuth, setTokenAuth] = useRecoilState(token);
  const [dataSource, setDataSource] = useRecoilState(source);
  const [shareInfo, setShareInfo] = useRecoilState(infoShare);
  const [isLocationAccessGranted, setIsLocationAccessGranted] =
    useRecoilState(isShareLocation);
  const [loading, setLoading] = useState(true);
  const setPhoneLoading = useSetRecoilState(phoneNumberLoading);
  const [membershipInfo, setMembershipInfo] = useRecoilState(userMembershipInfo);
    const [isFollow, setIsFollow] = useRecoilState(isFollowedOA)

  useEffect(() => {
    checkAuthSettingLocation();
  }, []);

  useEffect(() => {
    getAuthSetting();
  }, [isRegistered]);

  useEffect(() => {
    if (!referenceCode && referenceCode != refParam) {
      setReferenceCode(refParam);
    }
    if (!dataSource && dataSource != sourceParam) {
      setDataSource(sourceParam);
    }
  }, [refParam, sourceParam]);

  const checkAuthSettingLocation = async () => {
    setLoading(true);
    try {
      const data = await getSetting({});
      if (
        !isEmpty(data?.authSetting) &&
        data.authSetting["scope.userLocation"] !== false
      ) {
        setIsLocationAccessGranted(true);
      } else {
        setIsLocationAccessGranted(false);
      }
      await new Promise((resolve) => setTimeout(resolve, 50));
      setLoading(false);
    } catch (error) {
      setLoading(false);
    }
  };

  const getAuthSetting = async () => {
    var checkSetting = true; // Cho phép vào app
    var isLoggedIn = false; // Chỉ true khi thực sự đã login thành công

    try {
      const data = await getSetting({});
      const hasUserInfo = data?.authSetting?.["scope.userInfo"] !== false;
      const hasPhoneNumber =
        data?.authSetting?.["scope.userPhonenumber"] !== false;

      // COMMENTED: Không tự động request permission khi vào app
      // User sẽ được yêu cầu permission khi click vào input fields trong LoginGiba
      if (hasUserInfo && hasPhoneNumber) {
        // User has granted permissions, get info and login
        if (isEmpty(infoZalo) || phoneUser == "") {
          var zaloInfoData = await getInfoUserZalo();

          // Không set từ zaloInfoData.followedOA vì Zalo SDK không có field này
          // isFollowedOA sẽ được set sau khi authenticate thành công

          var phonenumber = await getInfoPhonenumber();
          setPhoneUser(phonenumber);

          if (tokenAuth == "") {
            var tokenNew = await authenticate(zaloInfoData, phonenumber);
            if (tokenNew == "") {
              checkSetting = false;
              isLoggedIn = false;
            } else if (tokenNew == "PENDING_APPROVAL") {
              checkSetting = true; // Tài khoản chờ duyệt - vẫn cho phép hiển thị
              isLoggedIn = false; // Chưa login thành công
              setTokenAuth(""); // Không lưu token thật
            } else if (tokenNew == "REJECTED") {
              checkSetting = true; // Tài khoản bị từ chối - vẫn cho phép hiển thị
              isLoggedIn = false; // Chưa login thành công
              setTokenAuth(""); // Không lưu token thật
            } else {
              checkSetting = true; // Login thành công
              isLoggedIn = true; // Đã login thành công
            }
            setTokenAuth(tokenNew);
          } else {
            checkSetting = true; // Đã có token
            isLoggedIn = true; // Đã có token hợp lệ
            setIsFollow(true); // Set isFollowedOA = true khi đã có token (vì đăng ký yêu cầu follow OA)
          }

          if (isEmpty(zaloInfoData)) {
            checkSetting = false;
            isLoggedIn = false;
          }
        } else {
          // Already have user info, just authenticate
          if (tokenAuth == "") {
            var tokenNew = await authenticate(infoZalo, phoneUser);
            if (tokenNew == "") {
              checkSetting = false;
              isLoggedIn = false;
            } else if (tokenNew == "PENDING_APPROVAL") {
              checkSetting = true; // Tài khoản chờ duyệt - vẫn cho phép hiển thị
              isLoggedIn = false; // Chưa login thành công
              setTokenAuth(""); // Không lưu token thật
            } else if (tokenNew == "REJECTED") {
              checkSetting = true; // Tài khoản bị từ chối - vẫn cho phép hiển thị
              isLoggedIn = false; // Chưa login thành công
              setTokenAuth(""); // Không lưu token thật
            } else {
              checkSetting = true; // Login thành công
              isLoggedIn = true; // Đã login thành công
            }
            setTokenAuth(tokenNew);
          } else {
            checkSetting = true; // Đã có token
            isLoggedIn = true; // Đã có token hợp lệ
            setIsFollow(true); // Set isFollowedOA = true khi đã có token (vì đăng ký yêu cầu follow OA)
          }
        }
      } else {
        // COMMENTED: Không tự động request permissions nữa
        // User sẽ được yêu cầu khi click vào input trong LoginGiba
        // User hasn't granted permissions, request them (this will show popup)
        // try {
        //   var zaloInfoData = await getInfoUserZalo();

        //   var phonenumber = await getInfoPhonenumber();
        //   setPhoneUser(phonenumber);

        //   if (tokenAuth == "") {
        //     var tokenNew = await authenticate(zaloInfoData, phonenumber);
        //     if (tokenNew == "") {
        //       checkSetting = false;
        //     } else if (tokenNew == "PENDING_APPROVAL") {
        //       checkSetting = true; // Tài khoản chờ duyệt - vẫn cho phép hiển thị
        //       setTokenAuth(""); // Không lưu token thật
        //     } else if (tokenNew == "REJECTED") {
        //       checkSetting = true; // Tài khoản bị từ chối - vẫn cho phép hiển thị
        //       setTokenAuth(""); // Không lưu token thật
        //     } else {
        //       checkSetting = true; // Login thành công
        //     }
        //     setTokenAuth(tokenNew);
        //   } else {
        //     checkSetting = true; // Đã có token
        //   }

        //   if (isEmpty(zaloInfoData)) {
        //     checkSetting = false;
        //   }
        // } catch (error) {
        //   checkSetting = false;
        // }

        // Cho phép vào app mà không cần authentication ngay
        checkSetting = true; // Cho phép vào app
        isLoggedIn = false; // Nhưng chưa login
      }
    } catch (error) {
      checkSetting = false;
      isLoggedIn = false;
    }

    // Chỉ set isRegister = true khi thực sự đã login thành công
    setIsRegistered(isLoggedIn);
  };

  const getInfoUserZalo = async () => {
    const infoZaloPromise = await new Promise((resolve, reject) => {
      getUserInfo({
        autoRequestPermission: false, // CHANGED: Không tự động yêu cầu quyền
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



  let cachedAccessToken = null;

  const getAccessTokenSafe = async (
    maxRetries = 5,
    initialDelay = 200,
    useCache = true
  ) => {
    if (useCache && cachedAccessToken) return cachedAccessToken;

    let delay = initialDelay;

    for (let i = 0; i < maxRetries; i++) {
      try {
        const token = await getAccessToken({});
        if (token) {
          cachedAccessToken = token;
          return token;
        }
      } catch (err) {
        console.warn("getAccessToken fail:", err);
      }
      // Chờ exponential backoff
      await new Promise((r) => setTimeout(r, delay));
      delay *= 2; // tăng dần 300ms -> 600ms -> 1200ms ...
    }

    // Cuối cùng: fallback về token cache (nếu có) hoặc null
    return cachedAccessToken;
  };

  const getInfoPhonenumber = async () => {
    try {
      setPhoneLoading(true);
      const accessToken = await getAccessTokenSafe();

      if (!accessToken) {
        setPhoneLoading(false);
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
    } finally {
      setPhoneLoading(false);
    }
  };

  const getProfile = (tokenUser) => {
    axios
      .get(`${dfData.domain}/api/memberships/profile`, {
        headers: {
          Authorization: `Bearer ${tokenUser}`,
        },
      })
      .then((res) => {
        setShareInfo(res.data.data);
      })
      .catch((error) => console.log(error));
  };

  const authenticate = async (zaloInfo, phonenumber) => {
    const formData = new FormData();
    formData.append("UserZaloId", zaloInfo?.id);
    formData.append("phoneNumber", fixFormatPhoneNumber(phonenumber));
    try {
      var res = await axios.post(
        `${dfData.domain}/api/auth/miniapp-login`,
        formData,
        {
          headers: {
            "Content-Type": "multipart/form-data",
          },
        }
      );


      if (res.data.success === false) {
        return "";
      } else {
        if (res.data.data.isLoginSuccess === false) {

          const userInfo = res.data.data.userInfo || {};

          // User đã đăng ký (dù đang chờ duyệt) = đã follow OA
          setIsFollow(true);

          const userInfoForDisplay = {
            id: userInfo.userZaloId || zaloInfo?.id,
            name: userInfo.fullname || zaloInfo?.name || "User",
            avatar: userInfo.zaloAvatar || zaloInfo?.avatar || null,
            roleId: userInfo.roleId || "",
            roleName: userInfo.roleName || "",
            idByOA: userInfo.idByOA || zaloInfo?.idByOA || "",
          };
          setInfoZalo(userInfoForDisplay);

          let approvalStatus = 0;
          let returnValue = "PENDING_APPROVAL";

          const messageToCheck = res.data.data?.message || res.data.message || "";

          if (messageToCheck.includes("đã bị từ chối")) {
            approvalStatus = 2;
            returnValue = "REJECTED";
          } else if (messageToCheck.includes("đang chờ duyệt")) {
            approvalStatus = 0;
            returnValue = "PENDING_APPROVAL";
          }

          setMembershipInfo({
            id: userInfo.id || "",
            userZaloId: userInfo.userZaloId || zaloInfo?.id,
            phoneNumber: userInfo.phoneNumber || phonenumber,
            fullname: userInfo.fullname || zaloInfo?.name || "",
            approvalStatus: approvalStatus,
            roleId: userInfo.roleId || "",
            roleName: userInfo.roleName || "",
            idByOA: userInfo.idByOA || zaloInfo?.idByOA || "",
          });

          return returnValue;
        }

        if (res.data.data.token) {
          getProfile(res.data.data.token);

          const membership = res.data.data.membership || res.data.data.userInfo || {};
          setMembershipInfo({
            id: membership.id || "",
            userZaloId: membership.userZaloId || zaloInfo?.id,
            phoneNumber: membership.phoneNumber || phonenumber,
            fullname: membership.fullname || zaloInfo?.name || "",
            approvalStatus: membership.approvalStatus !== undefined ? membership.approvalStatus : 1,
            roleId: membership.roleId || "",
            roleName: membership.roleName || "",
            idByOA: membership.idByOA || zaloInfo?.idByOA || "",
          });

          const userInfoForDisplay = {
            id: membership.userZaloId || zaloInfo?.id,
            name: membership.fullname || zaloInfo?.name || "User",
            avatar: membership.zaloAvatar || zaloInfo?.avatar || null,
            roleId: membership.roleId || "",
            roleName: membership.roleName || "",
            idByOA: membership.idByOA || zaloInfo?.idByOA || "",
          };

          setInfoZalo(userInfoForDisplay);
          setIsRegistered(true);
          setIsFollow(true); // Set isFollowedOA = true khi login thành công (vì đăng ký yêu cầu follow OA)
        }

        return res.data.data.token;
      }
    } catch (error) {

      if (error.response?.data) {
        const errorData = error.response.data.data || {};

        if (errorData.isLoginSuccess === false) {
          const userInfo = errorData.userInfo || {};

          const userInfoForDisplay = {
            id: userInfo.userZaloId || zaloInfo?.id,
            name: userInfo.fullname || zaloInfo?.name || "User",
            avatar: userInfo.zaloAvatar || zaloInfo?.avatar || null,
            roleId: userInfo.roleId || "",
            roleName: userInfo.roleName || "",
            idByOA: userInfo.idByOA || zaloInfo?.idByOA || "",
          };
          setInfoZalo(userInfoForDisplay);

          setMembershipInfo({
            id: userInfo.id || "",
            userZaloId: userInfo.userZaloId || zaloInfo?.id,
            phoneNumber: userInfo.phoneNumber || phonenumber,
            fullname: userInfo.fullname || zaloInfo?.name || "",
            approvalStatus: 0,
            roleId: userInfo.roleId || "",
            roleName: userInfo.roleName || "",
            idByOA: userInfo.idByOA || zaloInfo?.idByOA || "",
          });

          setIsRegistered(false);

          toast.warning(errorData.message || "Tài khoản đang chờ duyệt", {
            position: "top-center",
            autoClose: 5000,
          });
        }
      }
    }
    return "";
  };

  return (
    <>{loading ? <Spin spinning={loading} fullscreen /> : <>{children}</>}</>
  );
}
