import React, { useState, useEffect, useRef, useMemo } from "react";
import { Page, Box } from "zmp-ui";
import { useParams, useNavigate, useSearchParams } from "react-router-dom";
import { DragDropContext, Droppable, Draggable } from "react-beautiful-dnd";
import useSetHeader from "../components/hooks/useSetHeader";
import { useRecoilValue } from "recoil";
import { token, infoShare } from "../recoil/RecoilState";
import axios from "axios";
import LoadingGiba from "../componentsGiba/LoadingGiba";
import CommonShareModal from "../components/CommonShareModal";
import { IoShareSocialOutline } from "react-icons/io5";
import { Phone, Mail, GripVertical, UserPlus, Calendar } from "lucide-react";
import dfData from "../common/DefaultConfig.json";
import { APP_MODE } from "../state";
import BasicInfoCard from "../componentsGiba/MemberDetail/BasicInfoCard";
import CompanyInfoCard from "../componentsGiba/MemberDetail/CompanyInfoCard";
import { useMemberDetailOrder } from "../hooks/useMemberDetailOrder";
import { useDragDropHandler } from "../hooks/useDragDropHandler";
import { MemberDetail, CompanyInfo } from "../types/memberDetail";
import QRCanvas from "../componentsGiba/QRCanvas";
import { openPhone } from "zmp-sdk/apis";
import { toast } from "react-toastify";
import BottomNavigationPage from "../components/BottomNavigate";
import "./MemberDetailGiba.css";

const generateRandomCode = (): string => {
  const chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
  let code = "";
  for (let i = 0; i < 5; i++) {
    code += chars.charAt(Math.floor(Math.random() * chars.length));
  }
  return code;
};

const MemberDetailGiba: React.FC = () => {
  const navigate = useNavigate();
  const setHeader = useSetHeader();
  const userToken = useRecoilValue(token);
  const userProfile = useRecoilValue(infoShare);
  const { userZaloId } = useParams<{ userZaloId: string }>();
  const [searchParams] = useSearchParams();
  const [memberDetail, setMemberDetail] = useState<MemberDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [isDragMode, setIsDragMode] = useState(false);
  const shareModalRef = useRef<any>(null);
  const [currentUserSlug, setCurrentUserSlug] = useState<string | null>(null);
  const [currentUserZaloId, setCurrentUserZaloId] = useState<string | null>(
    null
  );
  const [isInSameGroup, setIsInSameGroup] = useState(false);
  const [checkingGroup, setCheckingGroup] = useState(false);

  // Get groupId from URL search params
  const groupIdFromUrl = searchParams.get("groupId");

  // Get QR code from API response
  const qrCode = useMemo(() => {
    return memberDetail?.basicInfo?.code || generateRandomCode();
  }, [memberDetail?.basicInfo?.code]);

  // Generate QR code URL with code parameter
  const qrCodeUrl = useMemo(() => {
    if (!memberDetail?.basicInfo) return "";

    // Get current environment from URL or APP_MODE
    const currentUrl = window.location.href;
    const urlParams = new URLSearchParams(window.location.search);
    const envFromUrl = urlParams.get("env");
    const env = envFromUrl || APP_MODE;

    let shareUrl = `https://zalo.me/s/${dfData.appid}/giba/member-detail/${memberDetail.basicInfo.slug}`;

    // Log environment detection for debugging
    console.log("🔍 Environment Detection:", {
      currentUrl: currentUrl,
      envFromUrl: envFromUrl,
      APP_MODE: APP_MODE,
      finalEnv: env,
      windowLocationSearch: window.location.search,
    });

    if (env == "TESTING" || env == "TESTING_LOCAL" || env == "DEVELOPMENT") {
      const params = new URLSearchParams();
      params.set("env", env);
      params.set("version", (window as any).APP_VERSION || "1.0.0");
      if (groupIdFromUrl) params.set("groupId", groupIdFromUrl);
      params.set("default", "true");
      if (qrCode) params.set("code", qrCode);
      shareUrl += `?${params.toString()}`;
    } else {
      const params = new URLSearchParams();
      if (groupIdFromUrl) params.set("groupId", groupIdFromUrl);
      if (qrCode) params.set("code", qrCode);
      shareUrl += `?${params.toString()}`;
    }

    // Log QR code URL for debugging
    console.log("🔗 QR Code URL:", shareUrl);
    console.log("📋 QR Code URL Details:", {
      baseUrl: `https://zalo.me/s/${dfData.appid}/giba/member-detail/${memberDetail.basicInfo.slug}`,
      slug: memberDetail.basicInfo.slug,
      groupId: groupIdFromUrl,
      qrCode: qrCode,
      env: env,
      fullUrl: shareUrl,
    });

    return shareUrl;
  }, [memberDetail?.basicInfo?.slug, qrCode, groupIdFromUrl]);

  useEffect(() => {
    const fetchCurrentUserProfile = async () => {
      if (!userToken) return;

      try {
        const response = await axios.get(
          `${dfData.domain}/api/memberships/profile`,
          {
            headers: {
              Authorization: `Bearer ${userToken}`,
            },
          }
        );

        if (response.data.success && response.data.data) {
          const profile = response.data.data;
          setCurrentUserSlug(profile.slug || null);
          setCurrentUserZaloId(profile.userZaloId || null);
        }
      } catch (error) {
        console.error("Error fetching current user profile:", error);
      }
    };

    fetchCurrentUserProfile();
  }, [userToken]);

  // Check if this is own profile by comparing current user info with profile being viewed
  const isOwnProfile = React.useMemo(() => {
    if (!memberDetail?.basicInfo) return false;

    const viewedSlug = memberDetail.basicInfo.slug;
    const viewedUserZaloId = memberDetail.basicInfo.userZaloId;
    const currentSlug = currentUserSlug || (userProfile as any)?.slug;
    const currentZaloId = currentUserZaloId || (userProfile as any)?.userZaloId;

    // Compare: current user's slug/userZaloId with viewed profile's slug/userZaloId
    const result =
      (currentSlug && currentSlug === viewedSlug) ||
      (currentZaloId && currentZaloId === viewedUserZaloId) ||
      (currentSlug && currentSlug === userZaloId) ||
      (currentZaloId && currentZaloId === userZaloId);

    return result;
  }, [
    currentUserSlug,
    currentUserZaloId,
    userZaloId,
    memberDetail?.basicInfo?.slug,
    memberDetail?.basicInfo?.userZaloId,
    userProfile,
  ]);

  // Check if viewed member is in the same group
  useEffect(() => {
    const checkGroupMembership = async () => {
      if (
        !groupIdFromUrl ||
        !memberDetail?.basicInfo?.userZaloId ||
        isOwnProfile ||
        !userToken
      ) {
        setIsInSameGroup(false);
        return;
      }

      try {
        setCheckingGroup(true);
        const response = await axios.get(
          `${dfData.domain}/api/groups/membership-in-group/${groupIdFromUrl}`,
          {
            headers: {
              Authorization: `Bearer ${userToken}`,
            },
          }
        );

        if (response.data.code === 0 && response.data.data) {
          const members = response.data.data;
          const isMember = members.some(
            (m: any) => m.userZaloId === memberDetail.basicInfo.userZaloId
          );
          setIsInSameGroup(isMember);
        } else {
          setIsInSameGroup(false);
        }
      } catch (error) {
        console.error("Error checking group membership:", error);
        setIsInSameGroup(false);
      } finally {
        setCheckingGroup(false);
      }
    };

    checkGroupMembership();
  }, [groupIdFromUrl, memberDetail, isOwnProfile, userToken]);

  console.log("memberDetail?.basicInfo?.", memberDetail?.basicInfo);

  const {
    cardOrder,
    basicInfoItemOrder,
    companyInfoItemOrder,
    isLoading: isOrderLoading,
    saveCardOrder,
    saveBasicInfoOrder,
    saveCompanyInfoOrder,
  } = useMemberDetailOrder(isOwnProfile);

  // Calculate visible items for drag and drop
  const getBasicInfoVisibleItems = () => {
    const hasItem = (itemKey: string) => {
      if (itemKey === "dayOfBirth")
        return !!memberDetail?.basicInfo?.dayOfBirth;
      if (itemKey === "address") return !!memberDetail?.basicInfo?.address;
      if (itemKey === "rating")
        return (
          memberDetail?.ratingInfo && memberDetail.ratingInfo.totalRatings > 0
        );
      return false;
    };
    return basicInfoItemOrder.filter(hasItem);
  };

  const getCompanyInfoVisibleItems = () => {
    const fieldMap: Record<string, keyof CompanyInfo> = {
      logo: "companyLogo",
      fullName: "companyFullName",
      brandName: "companyBrandName",
      taxCode: "taxCode",
      businessField: "businessField",
      businessType: "businessType",
      address: "headquartersAddress",
      website: "companyWebsite",
      phone: "companyPhoneNumber",
      email: "companyEmail",
      representative: "legalRepresentative",
      position: "legalRepresentativePosition",
      regNumber: "businessRegistrationNumber",
      regDate: "businessRegistrationDate",
      regPlace: "businessRegistrationPlace",
    };

    const hasItem = (itemKey: string) => {
      const field = fieldMap[itemKey];
      return field && !!memberDetail?.companyInfo?.[field];
    };
    return companyInfoItemOrder.filter(hasItem);
  };

  const basicInfoVisibleItems = memberDetail ? getBasicInfoVisibleItems() : [];
  const companyInfoVisibleItems = memberDetail
    ? getCompanyInfoVisibleItems()
    : [];

  const { handleDragEnd } = useDragDropHandler({
    onBasicInfoOrderChange: saveBasicInfoOrder,
    onCompanyInfoOrderChange: saveCompanyInfoOrder,
    basicInfoItemOrder,
    companyInfoItemOrder,
    basicInfoVisibleItems,
    companyInfoVisibleItems,
  });

  // iOS fix: Handle drag start
  const handleDragStart = () => {
    // Prevent scroll on iOS during drag
    if (typeof window !== "undefined") {
      document.body.style.overflow = "hidden";
      document.body.style.position = "fixed";
      document.body.style.width = "100%";
    }
  };

  // iOS fix: Handle drag update
  const handleDragUpdate = () => {
    // Force repaint on iOS
    if (typeof window !== "undefined") {
      window.requestAnimationFrame(() => {
        document.body.offsetHeight;
      });
    }
  };

  // iOS fix: Cleanup on drag end
  const handleDragEndWrapper = (result: any) => {
    // Restore scroll on iOS
    if (typeof window !== "undefined") {
      document.body.style.overflow = "";
      document.body.style.position = "";
      document.body.style.width = "";
    }

    // Call original handler
    handleDragEnd(result);
  };

  const handleShareProfile = () => {
    if (!memberDetail) return;

    const env = APP_MODE;
    let shareUrl = `https://zalo.me/s/${dfData.appid}/giba/member-detail/${memberDetail.basicInfo.slug}`;

    if (env == "TESTING" || env == "TESTING_LOCAL" || env == "DEVELOPMENT") {
      const params = new URLSearchParams();
      params.set("env", env);
      params.set("version", (window as any).APP_VERSION || "1.0.0");
      if (groupIdFromUrl) params.set("groupId", groupIdFromUrl);
      params.set("default", "true");
      shareUrl += `?${params.toString()}`;
    } else {
      if (groupIdFromUrl) {
        shareUrl += `?groupId=${groupIdFromUrl}`;
      }
    }

    const shareTitle = `DN BKASIM - ${memberDetail.basicInfo.fullName}`;
    const shareDescription = memberDetail.basicInfo.profile
      ? `${memberDetail.basicInfo.profile}\n${
          memberDetail.basicInfo.position ? memberDetail.basicInfo.position : ""
        }${
          memberDetail.basicInfo.company
            ? "\ntại " + memberDetail.basicInfo.company
            : ""
        }`
      : `Xem hồ sơ của ${memberDetail.basicInfo.fullName}${
          memberDetail.basicInfo.position
            ? " - " + memberDetail.basicInfo.position
            : ""
        }`;

    shareModalRef.current?.open(
      shareUrl,
      shareTitle,
      shareDescription,
      memberDetail.basicInfo.zaloAvatar
    );
  };

  const handleCreateRef = () => {
    if (!userToken) {
      toast.error("Vui lòng đăng nhập để sử dụng tính năng này!");
      return;
    }

    if (!memberDetail || !groupIdFromUrl) return;

    navigate("/giba/ref-create", {
      state: {
        optionType: "option1",
        groupId: groupIdFromUrl,
        receiverId: memberDetail.basicInfo.userZaloId,
        receiverName: memberDetail.basicInfo.fullName,
      },
    });
  };

  const handleCreateAppointment = () => {
    if (!userToken) {
      toast.error("Vui lòng đăng nhập để sử dụng tính năng này!");
      return;
    }

    if (!memberDetail || !groupIdFromUrl) return;

    navigate("/giba/appointment-create", {
      state: {
        groupId: groupIdFromUrl,
        receiverId: memberDetail.basicInfo.userZaloId,
        receiverName: memberDetail.basicInfo.fullName,
      },
    });
  };

  React.useEffect(() => {
    setHeader({
      title: "PROFILE THÀNH VIÊN",
      showUserInfo: false,
      showMenuButton: false,
      showCloseButton: false,
      hasLeftIcon: true,
    });
  }, [setHeader]);

  useEffect(() => {
    const fetchMemberDetail = async () => {
      if (!userZaloId) {
        setError("Không tìm thấy thông tin thành viên");
        setLoading(false);
        return;
      }

      try {
        setLoading(true);
        setError(null);

        const response = await axios.get(
          `${dfData.domain}/api/memberships/giba/profile/slug/${userZaloId}`,
          {
            headers: {
              Authorization: `Bearer ${userToken}`,
            },
          }
        );

        if (
          response.data.success &&
          response.data.data &&
          response.data.data.data
        ) {
          setMemberDetail(response.data.data.data);
        } else {
          setError("Không thể tải thông tin thành viên");
        }
      } catch (error) {
        console.error("Error fetching member detail:", error);
        setError("Đã xảy ra lỗi khi tải thông tin");
      } finally {
        setLoading(false);
      }
    };

    fetchMemberDetail();
  }, [userZaloId, userToken]);

  if (loading) {
    return (
      <Page className="bg-gray-50 min-h-screen" style={{ paddingTop: 50 }}>
        <div className="flex justify-center items-center h-64">
          <LoadingGiba size="lg" text="Đang tải thông tin thành viên..." />
        </div>
      </Page>
    );
  }

  if (error || !memberDetail) {
    return (
      <Page className="bg-gray-50 min-h-screen">
        <div className="text-center py-12 px-2">
          <div className="text-gray-500 text-lg mb-4">
            {error || "Không tìm thấy thông tin thành viên"}
          </div>
          <button
            onClick={() => navigate(-1)}
            style={{
              background: "#003d82",
              color: "#fff",
              padding: "10px 24px",
              borderRadius: "8px",
              fontSize: "14px",
              fontWeight: "500",
              border: "none",
              cursor: "pointer",
            }}
          >
            Quay lại
          </button>
        </div>
      </Page>
    );
  }

  const themeColor = memberDetail?.profileTemplate?.themeColor || "#1877F2";
  const coverImage = memberDetail?.profileTemplate?.coverImage;

  const addOpacity = (color: string, opacity: number) => {
    if (color.startsWith("#")) {
      const hex = color.slice(1);
      const r = parseInt(hex.substring(0, 2), 16);
      const g = parseInt(hex.substring(2, 4), 16);
      const b = parseInt(hex.substring(4, 6), 16);
      return `rgba(${r}, ${g}, ${b}, ${opacity})`;
    }
    return color;
  };

  const renderFieldInfoCard = () => {
    if (!memberDetail?.fieldInfo?.fieldNames?.length) return null;

    return (
      <div
        style={{
          background: "#fff",
          borderRadius: "8px",
          padding: "16px",
          marginBottom: "40px",
          boxShadow: "0 1px 2px rgba(0, 0, 0, 0.1)",
          borderLeft: `4px solid ${themeColor}`,
        }}
      >
        <h2
          style={{
            fontSize: "20px",
            fontWeight: "700",
            color: themeColor,
            marginBottom: "12px",
            display: "flex",
            alignItems: "center",
            gap: "8px",
          }}
        >
          <div
            style={{
              width: "4px",
              height: "20px",
              background: themeColor,
              borderRadius: "2px",
            }}
          />
          Lĩnh vực kinh doanh
        </h2>
        <div style={{ display: "flex", flexWrap: "wrap", gap: "8px" }}>
          {memberDetail.fieldInfo.fieldNames.map((field, index) => (
            <span
              key={index}
              style={{
                background: addOpacity(themeColor, 0.1),
                color: themeColor,
                padding: "6px 12px",
                borderRadius: "6px",
                fontSize: "15px",
                fontWeight: "500",
                border: `1px solid ${addOpacity(themeColor, 0.3)}`,
              }}
            >
              {field}
            </span>
          ))}
        </div>
      </div>
    );
  };

  const renderCustomFieldsCard = () => {
    const visibleFields =
      memberDetail?.profileTemplate?.customFields?.filter((f) => f.isVisible) ||
      [];
    if (!visibleFields.length) return null;

    return (
      <div
        style={{
          background: "#fff",
          borderRadius: "8px",
          padding: "16px",
          marginBottom: "16px",
          boxShadow: "0 1px 2px rgba(0, 0, 0, 0.1)",
          borderLeft: `4px solid ${themeColor}`,
        }}
      >
        <h2
          style={{
            fontSize: "20px",
            fontWeight: "700",
            color: themeColor,
            marginBottom: "12px",
            display: "flex",
            alignItems: "center",
            gap: "8px",
          }}
        >
          <div
            style={{
              width: "4px",
              height: "20px",
              background: themeColor,
              borderRadius: "2px",
            }}
          />
          Thông tin bổ sung
        </h2>
        <div style={{ display: "flex", flexDirection: "column", gap: "12px" }}>
          {visibleFields.map((field) => (
            <div
              key={field.id}
              style={{
                padding: "12px",
                background: addOpacity(themeColor, 0.05),
                borderRadius: "8px",
                borderLeft: `3px solid ${themeColor}`,
              }}
            >
              <div
                style={{
                  fontSize: "13px",
                  fontWeight: "600",
                  color: themeColor,
                  marginBottom: "4px",
                }}
              >
                {field.fieldName}
              </div>
              <div
                style={{
                  fontSize: "15px",
                  color: "#050505",
                  wordBreak: "break-word",
                }}
              >
                {field.fieldValue}
              </div>
            </div>
          ))}
        </div>
      </div>
    );
  };

  const renderGroupInfoCard = () => {
    const groupInfo = memberDetail?.groupInfo;
    const groupsArray = Array.isArray(groupInfo)
      ? groupInfo
      : groupInfo
      ? [groupInfo]
      : [];

    if (groupsArray.length === 0) {
      return (
        <div
          style={{
            background: "#fff",
            borderRadius: "8px",
            padding: "16px",
            marginBottom: "16px",
            boxShadow: "0 1px 2px rgba(0, 0, 0, 0.1)",
            borderLeft: `4px solid ${themeColor}`,
          }}
        >
          <h2
            style={{
              fontSize: "20px",
              fontWeight: "700",
              color: themeColor,
              marginBottom: "12px",
              display: "flex",
              alignItems: "center",
              gap: "8px",
            }}
          >
            <div
              style={{
                width: "4px",
                height: "20px",
                background: themeColor,
                borderRadius: "2px",
              }}
            />
            Thành viên CLB DN BKASIM
          </h2>
          <div
            style={{
              fontSize: "15px",
              color: "#050505",
            }}
          >
            GIBA
          </div>
        </div>
      );
    }

    return (
      <div
        style={{
          background: "#fff",
          borderRadius: "8px",
          padding: "16px",
          marginBottom: "16px",
          boxShadow: "0 1px 2px rgba(0, 0, 0, 0.1)",
          borderLeft: `4px solid ${themeColor}`,
        }}
      >
        <h2
          style={{
            fontSize: "20px",
            fontWeight: "700",
            color: themeColor,
            marginBottom: "12px",
            display: "flex",
            alignItems: "center",
            gap: "8px",
          }}
        >
          <div
            style={{
              width: "4px",
              height: "20px",
              background: themeColor,
              borderRadius: "2px",
            }}
          />
          Thành viên CLB DN BKASIM
        </h2>
        <div style={{ display: "flex", flexDirection: "column", gap: "12px" }}>
          {groupsArray.map((group, index) => (
            <div
              key={group?.groupId || index}
              style={{
                padding: "12px",
                background: addOpacity(themeColor, 0.05),
                borderRadius: "8px",
                borderLeft: `3px solid ${themeColor}`,
              }}
            >
              <div
                style={{
                  fontSize: "15px",
                  color: "#050505",
                  fontWeight: "500",
                  marginBottom: "4px",
                }}
              >
                {group?.groupName
                  ? `DN BKASIM - ${group.groupName}`
                  : "DN BKASIM"}
              </div>
              {group?.groupPosition && (
                <div
                  style={{
                    fontSize: "14px",
                    color: "#65676b",
                    marginTop: "4px",
                  }}
                >
                  Vị trí: {group.groupPosition}
                </div>
              )}
            </div>
          ))}
        </div>
      </div>
    );
  };

  return (
    <Page
      style={{
        paddingTop: 0,
        marginTop: 50,
        minHeight: "100vh",
        background: "#f0f2f5",
      }}
    >
      {/* Cover Photo Section with Theme Color */}
      {coverImage && (
        <div
          style={{
            position: "absolute",
            top: 0,
            left: 0,
            right: 0,
            height: "200px",
            background: `url(${coverImage}) center/cover no-repeat`,
            zIndex: 0,
          }}
        />
      )}
      {!coverImage && (
        <div
          style={{
            position: "absolute",
            top: 0,
            left: 0,
            right: 0,
            height: "200px",
            background: `linear-gradient(135deg, ${themeColor} 0%, ${addOpacity(
              themeColor,
              0.8
            )} 100%)`,
            zIndex: 0,
          }}
        />
      )}

      {/* Share Button - Floating */}
      <div
        onClick={handleShareProfile}
        style={{
          position: "fixed",
          top: "70px",
          right: "16px",
          width: "48px",
          height: "48px",
          borderRadius: "50%",
          background: themeColor,
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
          cursor: "pointer",
          boxShadow: `0 4px 12px ${addOpacity(themeColor, 0.4)}`,
          zIndex: 100,
          transition: "all 0.2s",
        }}
        onMouseEnter={(e) => {
          e.currentTarget.style.transform = "scale(1.1)";
          e.currentTarget.style.boxShadow = `0 6px 16px ${addOpacity(
            themeColor,
            0.5
          )}`;
        }}
        onMouseLeave={(e) => {
          e.currentTarget.style.transform = "scale(1)";
          e.currentTarget.style.boxShadow = `0 4px 12px ${addOpacity(
            themeColor,
            0.4
          )}`;
        }}
      >
        <IoShareSocialOutline size={22} color="#fff" />
      </div>
      <Box
        style={{
          position: "relative",
          padding: "0 8px 40px",
          maxWidth: "900px",
          margin: "0 auto",
          marginTop: 100,
        }}
      >
        {/* Profile Picture - Overlapping Cover */}
        <div
          style={{
            position: "absolute",
            top: "-84px",
            left: "16px",
            width: "148px",
            height: "148px",
            borderRadius: "50%",
            border: `4px solid #fff`,
            overflow: "hidden",
            background: "#e4e6eb",
            boxShadow: `0 4px 16px ${addOpacity(
              themeColor,
              0.3
            )}, 0 2px 8px rgba(0, 0, 0, 0.1)`,
            zIndex: 10,
          }}
        >
          {memberDetail?.basicInfo?.zaloAvatar ? (
            <img
              src={memberDetail.basicInfo.zaloAvatar}
              alt={memberDetail?.basicInfo?.fullName || ""}
              style={{
                width: "100%",
                height: "100%",
                objectFit: "cover",
              }}
            />
          ) : (
            <div
              style={{
                width: "100%",
                height: "100%",
                background: themeColor,
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
                color: "#fff",
                fontSize: "64px",
                fontWeight: "700",
              }}
            >
              {memberDetail?.basicInfo?.fullName?.charAt(0) || "?"}
            </div>
          )}
        </div>

        {/* Drag Mode Toggle Button - Top Right of Avatar */}
        {isOwnProfile && (
          <button
            onClick={() => setIsDragMode(!isDragMode)}
            style={{
              position: "absolute",
              top: "8px",
              right: "0px",
              background: isDragMode ? themeColor : "#fff",
              color: isDragMode ? "#fff" : themeColor,
              padding: "6px 14px",
              borderRadius: "20px",
              fontSize: "13px",
              fontWeight: "600",
              border: isDragMode ? "none" : `2px solid ${themeColor}`,
              cursor: "pointer",
              transition: "all 0.2s",
              display: "flex",
              alignItems: "center",
              gap: "5px",
              boxShadow: isDragMode
                ? `0 2px 8px ${addOpacity(themeColor, 0.4)}`
                : "0 2px 8px rgba(0, 0, 0, 0.15)",
              zIndex: 11,
              whiteSpace: "nowrap",
            }}
            onMouseEnter={(e) => {
              if (!isDragMode) {
                e.currentTarget.style.background = addOpacity(themeColor, 0.1);
                e.currentTarget.style.boxShadow = `0 4px 12px ${addOpacity(
                  themeColor,
                  0.3
                )}`;
                e.currentTarget.style.transform = "scale(1.05)";
              }
            }}
            onMouseLeave={(e) => {
              if (!isDragMode) {
                e.currentTarget.style.background = "#fff";
                e.currentTarget.style.boxShadow =
                  "0 2px 8px rgba(0, 0, 0, 0.15)";
                e.currentTarget.style.transform = "scale(1)";
              }
            }}
          >
            <GripVertical size={14} />
            {isDragMode ? "Hoàn tất" : "Sắp xếp"}
          </button>
        )}

        {/* Profile Info Card - Facebook Style */}
        <div
          style={{
            background: "#fff",
            borderRadius: "8px",
            padding: "20px",
            paddingTop: "84px",
            marginTop: "80px",
            marginBottom: "16px",
            boxShadow: `0 2px 8px ${addOpacity(
              themeColor,
              0.15
            )}, 0 1px 2px rgba(0, 0, 0, 0.1)`,
            borderTop: `3px solid ${themeColor}`,
          }}
        >
          {/* Name and Basic Info */}
          <div style={{ marginBottom: "16px" }}>
            <h1
              style={{
                fontSize: "32px",
                fontWeight: "700",
                color: "#050505",
                marginBottom: "4px",
                lineHeight: "1.2",
              }}
            >
              {memberDetail?.basicInfo?.fullName}
            </h1>

            {memberDetail?.basicInfo?.position && (
              <p
                style={{
                  fontSize: "15px",
                  color: "#65676b",
                  marginBottom: "4px",
                  display: "flex",
                  alignItems: "center",
                  gap: "6px",
                }}
              >
                <div
                  style={{
                    width: "3px",
                    height: "3px",
                    borderRadius: "50%",
                    background: themeColor,
                  }}
                />
                {memberDetail.basicInfo.position}
                {memberDetail?.basicInfo?.company &&
                  ` tại ${memberDetail.basicInfo.company}`}
              </p>
            )}

            {/* Contact Buttons */}
            <div
              style={{
                display: "flex",
                flexWrap: "wrap",
                gap: "8px",
                marginTop: "12px",
                alignItems: "center",
              }}
            >
              {memberDetail?.basicInfo?.phoneNumber && (
                <div
                  onClick={(e) => {
                    e.stopPropagation();
                    if (memberDetail.basicInfo.phoneNumber) {
                      openPhone({
                        phoneNumber: memberDetail.basicInfo.phoneNumber,
                        success: () => console.log("Call successful"),
                        fail: (error) => console.error("Call failed:", error),
                      });
                    }
                  }}
                  style={{
                    display: "inline-flex",
                    alignItems: "center",
                    gap: "6px",
                    background: addOpacity(themeColor, 0.1),
                    padding: "6px 12px",
                    borderRadius: "6px",
                    color: themeColor,
                    fontSize: "15px",
                    fontWeight: "500",
                    transition: "all 0.2s",
                    border: `1px solid ${addOpacity(themeColor, 0.2)}`,
                    cursor: "pointer",
                  }}
                  onMouseEnter={(e) => {
                    e.currentTarget.style.background = themeColor;
                    e.currentTarget.style.color = "#fff";
                    e.currentTarget.style.borderColor = themeColor;
                  }}
                  onMouseLeave={(e) => {
                    e.currentTarget.style.background = addOpacity(
                      themeColor,
                      0.1
                    );
                    e.currentTarget.style.color = themeColor;
                    e.currentTarget.style.borderColor = addOpacity(
                      themeColor,
                      0.2
                    );
                  }}
                >
                  <Phone size={16} />
                  <span>{memberDetail.basicInfo.phoneNumber}</span>
                </div>
              )}

              {memberDetail?.basicInfo?.email && (
                <div
                  style={{
                    display: "inline-flex",
                    alignItems: "center",
                    gap: "6px",
                    background: addOpacity(themeColor, 0.1),
                    padding: "6px 12px",
                    borderRadius: "6px",
                    color: themeColor,
                    fontSize: "15px",
                    fontWeight: "500",
                    transition: "all 0.2s",
                    border: `1px solid ${addOpacity(themeColor, 0.2)}`,
                    cursor: "pointer",
                  }}
                  onMouseEnter={(e) => {
                    e.currentTarget.style.background = themeColor;
                    e.currentTarget.style.color = "#fff";
                    e.currentTarget.style.borderColor = themeColor;
                  }}
                  onMouseLeave={(e) => {
                    e.currentTarget.style.background = addOpacity(
                      themeColor,
                      0.1
                    );
                    e.currentTarget.style.color = themeColor;
                    e.currentTarget.style.borderColor = addOpacity(
                      themeColor,
                      0.2
                    );
                  }}
                >
                  <Mail size={16} />
                  <span>{memberDetail.basicInfo.email}</span>
                </div>
              )}
            </div>
          </div>

          {/* QR Code Section */}
          <div
            style={{
              paddingTop: "16px",
              borderTop: "1px solid #e4e6eb",
              display: "flex",
              justifyContent: "center",
              alignItems: "center",
            }}
          >
            <QRCanvas value={qrCodeUrl} size={120} showBorder={false} />
          </div>

          {/* Action Buttons - Trao Ref và Đặt Lịch */}
          {!isOwnProfile && isInSameGroup && (
            <div
              style={{
                display: "flex",
                gap: "8px",
                paddingTop: "16px",
                borderTop: "1px solid #e4e6eb",
              }}
            >
              <button
                onClick={handleCreateRef}
                style={{
                  flex: 1,
                  background: "#fff",
                  color: themeColor,
                  padding: "8px 16px",
                  borderRadius: "6px",
                  fontSize: "15px",
                  fontWeight: "600",
                  border: `2px solid ${themeColor}`,
                  cursor: "pointer",
                  display: "flex",
                  alignItems: "center",
                  justifyContent: "center",
                  gap: "6px",
                }}
              >
                <UserPlus size={18} />
                Trao Ref
              </button>
              <button
                onClick={handleCreateAppointment}
                style={{
                  flex: 1,
                  background: themeColor,
                  color: "#fff",
                  padding: "8px 16px",
                  borderRadius: "6px",
                  fontSize: "15px",
                  fontWeight: "600",
                  border: "none",
                  display: "flex",
                  alignItems: "center",
                  justifyContent: "center",
                  gap: "6px",
                }}
              >
                <Calendar size={18} />
                Đặt Lịch
              </button>
            </div>
          )}
          {isOwnProfile && (
            <div
              style={{
                display: "flex",
                gap: "8px",
                paddingTop: "16px",
                borderTop: "1px solid #e4e6eb",
              }}
            >
              <button
                onClick={() => navigate("/giba/edit-info")}
                style={{
                  flex: 1,
                  background: themeColor,
                  color: "#fff",
                  padding: "8px 12px",
                  borderRadius: "6px",
                  fontSize: "15px",
                  fontWeight: "600",
                  border: "none",
                }}
              >
                Chỉnh sửa thông tin
              </button>
              <button
                onClick={() => navigate("/giba/profile-intro")}
                style={{
                  flex: 1,
                  background: themeColor,
                  color: "#fff",
                  padding: "8px 16px",
                  borderRadius: "6px",
                  fontSize: "15px",
                  fontWeight: "600",
                  border: "none",
                }}
              >
                Chỉnh sửa profile
              </button>
            </div>
          )}
        </div>

        {/* About Section - Facebook Style */}
        {memberDetail?.basicInfo?.profile && (
          <div
            style={{
              background: "#fff",
              borderRadius: "8px",
              padding: "16px",
              marginBottom: "16px",
              boxShadow: "0 1px 2px rgba(0, 0, 0, 0.1)",
              borderLeft: `4px solid ${themeColor}`,
              minWidth: 0,
              overflow: "hidden",
            }}
          >
            <h2
              style={{
                fontSize: "20px",
                fontWeight: "700",
                color: themeColor,
                marginBottom: "12px",
                display: "flex",
                alignItems: "center",
                gap: "8px",
              }}
            >
              <div
                style={{
                  width: "4px",
                  height: "20px",
                  background: themeColor,
                  borderRadius: "2px",
                }}
              />
              Giới thiệu
            </h2>
            <p
              style={{
                color: "#050505",
                fontSize: "15px",
                lineHeight: "1.5",
                margin: 0,
                whiteSpace: "pre-wrap",
                wordBreak: "break-word",
                overflowWrap: "break-word",
                minWidth: 0,
              }}
            >
              {memberDetail.basicInfo.profile}
            </p>
          </div>
        )}

        {/* Group Info Card */}
        {renderGroupInfoCard()}

        {/* Content Cards */}
        <DragDropContext
          onDragStart={handleDragStart}
          onDragUpdate={handleDragUpdate}
          onDragEnd={handleDragEndWrapper}
        >
          <div style={{ marginBottom: "16px" }}>
            {cardOrder.map((cardKey) => (
              <div key={cardKey} style={{ marginBottom: "16px" }}>
                {cardKey === "basicInfo" && (
                  <BasicInfoCard
                    basicInfo={memberDetail?.basicInfo}
                    ratingInfo={memberDetail?.ratingInfo}
                    itemOrder={basicInfoItemOrder}
                    isDragMode={isDragMode}
                    themeColor={themeColor}
                  />
                )}

                {cardKey === "companyInfo" && (
                  <CompanyInfoCard
                    companyInfo={memberDetail?.companyInfo}
                    itemOrder={companyInfoItemOrder}
                    isDragMode={isDragMode}
                    themeColor={themeColor}
                  />
                )}

                {cardKey === "fieldInfo" && renderFieldInfoCard()}

                {cardKey === "customFields" && renderCustomFieldsCard()}
              </div>
            ))}
          </div>
        </DragDropContext>
      </Box>

      <CommonShareModal ref={shareModalRef} />

      {/* Show bottom navigation for profile owner or when viewing from same group */}
      {(isOwnProfile || (groupIdFromUrl && isInSameGroup)) && (
        <BottomNavigationPage forceShow />
      )}
    </Page>
  );
};

export default MemberDetailGiba;
