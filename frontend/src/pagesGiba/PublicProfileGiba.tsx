import React, { useState, useEffect, useRef } from "react";
import { Page, Box } from "zmp-ui";
import { useParams, useNavigate, useLocation } from "react-router-dom";
import useSetHeader from "../components/hooks/useSetHeader";
import { useRecoilValue } from "recoil";
import { token } from "../recoil/RecoilState";
import axios from "axios";
import LoadingGiba from "../componentsGiba/LoadingGiba";
import CommonShareModal from "../components/CommonShareModal";
import { IoShareSocialOutline } from "react-icons/io5";
import { Phone, Mail } from "lucide-react";
import { Image } from "antd";
import dfData from "../common/DefaultConfig.json";
import { APP_MODE } from "../state";

interface BasicInfo {
  id: string;
  userZaloId: string;
  slug: string;
  createdDate: string;
  updatedDate: string;
  fullname: string;
  phoneNumber: string;
  email: string;
  zaloAvatar: string;
  profile: string;
  dayOfBirth: string;
  address: string;
  company: string;
  position: string;
}

interface RatingInfo {
  averageRating: number;
  totalRatings: number;
}

interface FieldDetail {
  id: string;
  name: string;
  isActive: boolean;
  parentId: string | null;
}

interface FieldInfo {
  fieldIds: string;
  fieldDetails: FieldDetail[];
  fieldNames: string[];
}

interface CompanyInfo {
  companyFullName: string;
  companyBrandName: string;
  taxCode: string;
  businessField: string;
  businessType: string;
  headquartersAddress: string;
  companyWebsite: string;
  companyPhoneNumber: string;
  companyEmail: string;
  legalRepresentative: string;
  legalRepresentativePosition: string;
  companyLogo: string;
  businessRegistrationNumber: string;
  businessRegistrationDate: string;
  businessRegistrationPlace: string;
}

interface CustomField {
  id: string;
  fieldName: string;
  fieldValue: string;
  fieldType: string;
  displayOrder: number;
  isVisible: boolean;
}

interface ProfileTemplate {
  visibleFields: string[];
  hiddenFields: string[];
  customDescription: string;
  coverImage: string;
  themeColor: string;
  isPublic: boolean;
  customFields: CustomField[];
}

interface PublicProfile {
  basicInfo: BasicInfo;
  ratingInfo: RatingInfo;
  fieldInfo: FieldInfo;
  companyInfo: CompanyInfo;
  profileTemplate: ProfileTemplate;
}

const InfoRow: React.FC<{
  label: string;
  value: string;
}> = ({ label, value }) => {
  return (
    <Box
      style={{
        display: "flex",
        padding: "16px 0",
        borderBottom: "1px solid #f3f4f6",
      }}
    >
      <Box
        style={{
          width: "140px",
          fontSize: "14px",
          fontWeight: "500",
          color: "#6b7280",
          flexShrink: 0,
        }}
      >
        {label}
      </Box>
      <Box
        style={{
          flex: 1,
          fontSize: "14px",
          color: "#000",
          fontWeight: "400",
          lineHeight: "1.6",
          wordBreak: "break-word",
          overflowWrap: "break-word",
        }}
      >
        {value || (
          <span style={{ color: "#9ca3af", fontStyle: "italic" }}>
            Chưa cập nhật
          </span>
        )}
      </Box>
    </Box>
  );
};

const PublicProfileGiba: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const setHeader = useSetHeader();
  const userToken = useRecoilValue(token);
  const { slug } = useParams<{ slug: string }>();
  const [profileData, setProfileData] = useState<PublicProfile | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const shareModalRef = useRef<any>(null);

  const searchParams = new URLSearchParams(location.search);
  const groupId = searchParams.get("groupId") || "";

  React.useEffect(() => {
    setHeader({
      title: "PROFILE CÁ NHÂN",
      showUserInfo: false,
      showMenuButton: false,
      showCloseButton: false,
      hasLeftIcon: true,
    });
  }, [setHeader]);

  useEffect(() => {
    const fetchPublicProfile = async () => {
      if (!slug) {
        setError("Không tìm thấy thông tin hồ sơ");
        setLoading(false);
        return;
      }

      try {
        setLoading(true);
        setError(null);

        const url = groupId
          ? `${dfData.domain}/api/memberships/giba/profile/slug/${slug}?groupId=${groupId}`
          : `${dfData.domain}/api/memberships/giba/profile/slug/${slug}`;

        const response = await axios.get(url, {
          headers: {
            Authorization: `Bearer ${userToken}`,
          },
        });

        console.log("PublicProfile API Response:", response.data);

        if (
          response.data.success &&
          response.data.data &&
          response.data.data.data
        ) {
          console.log("Setting profile data:", response.data.data.data);
          setProfileData(response.data.data.data);
        } else if (response.data.success && response.data.data) {
          console.log("Using fallback structure:", response.data.data);
          setProfileData(response.data.data);
        } else {
          console.error("Unexpected response structure:", response.data);
          setError("Không thể tải thông tin hồ sơ");
        }
      } catch (error) {
        console.error("Error fetching public profile:", error);
        setError("Đã xảy ra lỗi khi tải thông tin");
      } finally {
        setLoading(false);
      }
    };

    fetchPublicProfile();
  }, [slug, groupId, userToken]);

  const handleShareProfile = () => {
    if (!profileData) return;

    const env = APP_MODE;
    let shareUrl = `https://zalo.me/s/${dfData.appid}/`;

    if (env == "TESTING" || env == "TESTING_LOCAL" || env == "DEVELOPMENT") {
      const params = new URLSearchParams();
      params.set("env", env);
      params.set("version", (window as any).APP_VERSION || "1.0.0");
      params.set("profileSlug", profileData.basicInfo.slug);
      if (groupId) params.set("groupId", groupId);
      params.set("default", "true");
      shareUrl += `?${params.toString()}`;
    } else {
      const params = new URLSearchParams();
      params.set("profileSlug", profileData.basicInfo.slug);
      if (groupId) params.set("groupId", groupId);
      shareUrl += `?${params.toString()}`;
    }

    const shareTitle = `Hồ sơ: ${profileData.basicInfo.fullname}`;

    const positionCompany = [
      profileData.basicInfo.position,
      profileData.basicInfo.company,
    ]
      .filter(Boolean)
      .join("\n");

    const shareDescription =
      [profileData.basicInfo.profile, positionCompany]
        .filter(Boolean)
        .join("\n") || `Xem hồ sơ của ${profileData.basicInfo.fullname}`;

    shareModalRef.current?.open(
      shareUrl,
      shareTitle,
      shareDescription,
      profileData.basicInfo.zaloAvatar
    );
  };

  if (loading) {
    return (
      <Page className="bg-gray-50 min-h-screen" style={{ paddingTop: 50 }}>
        <div className="flex justify-center items-center h-64">
          <LoadingGiba size="lg" text="Đang tải hồ sơ..." />
        </div>
      </Page>
    );
  }

  if (error || !profileData) {
    return (
      <Page className="bg-gray-50 min-h-screen" style={{ paddingTop: 50 }}>
        <div className="text-center py-12 px-4">
          <div className="text-gray-500 text-lg mb-4">
            {error || "Không tìm thấy hồ sơ"}
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

  return (
    <Page
      style={{
        paddingTop: 50,
        minHeight: "100vh",
        position: "relative",
      }}
    >
      {/* Background Layer - Full Screen */}
      <div
        style={{
          position: "fixed",
          top: 0,
          left: 0,
          right: 0,
          bottom: 0,
          background: profileData?.profileTemplate?.themeColor
            ? `linear-gradient(135deg, ${profileData.profileTemplate.themeColor} 0%, ${profileData.profileTemplate.themeColor}dd 100%)`
            : "linear-gradient(135deg, #003d82 0%, #001f47 100%)",
          backgroundSize: "cover",
          backgroundPosition: "center",
          backgroundAttachment: "fixed",
          zIndex: 0,
        }}
      />
      {/* Profile Card */}
      <Box
        style={{
          position: "relative",
          zIndex: 2,
          padding: "20px 16px",
        }}
      >
        <Box
          style={{
            background: "transparent",
            borderRadius: "20px",
            overflow: "hidden",
            textAlign: "center",
          }}
        >
          {/* Avatar */}
          <Box
            style={{
              display: "flex",
              justifyContent: "center",
              marginBottom: "20px",
            }}
          >
            <Box
              style={{
                width: "120px",
                height: "120px",
                borderRadius: "20px",
                background: "#fff",
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
                padding: "4px",
                boxShadow: "0 4px 12px rgba(0, 0, 0, 0.15)",
                border: "2px solid rgba(255, 255, 255, 0.3)",
              }}
            >
              <Box
                style={{
                  width: "100%",
                  height: "100%",
                  borderRadius: "16px",
                  background: profileData.basicInfo.zaloAvatar
                    ? "#fff"
                    : "#003d82",
                  display: "flex",
                  alignItems: "center",
                  justifyContent: "center",
                  color: "#fff",
                  fontWeight: "600",
                  fontSize: "36px",
                  overflow: "hidden",
                }}
              >
                {profileData.basicInfo.zaloAvatar ? (
                  <img
                    src={profileData.basicInfo.zaloAvatar.replace(
                      "/s120/",
                      "/s480/"
                    )}
                    alt={profileData.basicInfo.fullname}
                    style={{
                      width: "100%",
                      height: "100%",
                      objectFit: "cover",
                      borderRadius: "16px",
                    }}
                  />
                ) : (
                  profileData.basicInfo.fullname
                )}
              </Box>
            </Box>
          </Box>

          {/* Name */}
          <h1
            style={{
              fontSize: "28px",
              fontWeight: "700",
              color: "#fff",
              margin: "0 0 8px 0",
              lineHeight: "1.2",
              textShadow: "0 2px 4px rgba(0, 0, 0, 0.3)",
            }}
          >
            {profileData.basicInfo.fullname}
          </h1>

          {/* Position & Company */}
          {(profileData.basicInfo.position ||
            profileData.basicInfo.company) && (
            <Box
              style={{
                fontSize: "16px",
                color: "#fff",
                marginBottom: "20px",
                fontWeight: "500",
                textShadow: "0 1px 2px rgba(0, 0, 0, 0.3)",
              }}
            >
              {profileData.basicInfo.position && profileData.basicInfo.company
                ? `${profileData.basicInfo.position} - ${profileData.basicInfo.company}`
                : profileData.basicInfo.position ||
                  profileData.basicInfo.company}
            </Box>
          )}

          {/* Contact Buttons */}
          <Box
            style={{
              display: "flex",
              gap: "12px",
              justifyContent: "center",
              marginBottom: "30px",
            }}
          >
            {/* Phone Button */}
            <Box
              style={{
                background: "#fff",
                borderRadius: "12px",
                padding: "12px 16px",
                display: "flex",
                alignItems: "center",
                gap: "8px",
                boxShadow: "0 2px 8px rgba(0, 0, 0, 0.15)",
                minWidth: "140px",
                justifyContent: "center",
              }}
            >
              <Box
                style={{
                  width: "20px",
                  height: "20px",
                  background: "#003d82",
                  borderRadius: "50%",
                  display: "flex",
                  alignItems: "center",
                  justifyContent: "center",
                }}
              >
                <Phone size={12} color="#fff" />
              </Box>
              <span
                style={{
                  color: "#003d82",
                  fontSize: "14px",
                  fontWeight: "600",
                }}
              >
                {profileData.basicInfo.phoneNumber || "Chưa cập nhật"}
              </span>
            </Box>

            {/* Email Button */}
            <Box
              style={{
                background: "#fff",
                borderRadius: "12px",
                padding: "12px 16px",
                display: "flex",
                alignItems: "center",
                gap: "8px",
                boxShadow: "0 2px 8px rgba(0, 0, 0, 0.15)",
                minWidth: "140px",
                justifyContent: "center",
              }}
            >
              <Box
                style={{
                  width: "20px",
                  height: "20px",
                  background: "#003d82",
                  borderRadius: "50%",
                  display: "flex",
                  alignItems: "center",
                  justifyContent: "center",
                }}
              >
                <Mail size={12} color="#fff" />
              </Box>
              <span
                style={{
                  color: "#003d82",
                  fontSize: "14px",
                  fontWeight: "600",
                }}
              >
                {profileData.basicInfo.email || "Chưa cập nhật"}
              </span>
            </Box>
          </Box>

          {/* Profile Description */}
          {profileData?.basicInfo?.profile && (
            <div
              style={{
                background: "rgba(255, 255, 255, 0.9)",
                borderRadius: "16px",
                padding: "20px",
                marginBottom: "20px",
                backdropFilter: "blur(10px)",
              }}
            >
              <p
                style={{
                  color: "#333",
                  fontSize: "15px",
                  lineHeight: "1.6",
                  margin: "0",
                  textAlign: "left",
                }}
              >
                {profileData.basicInfo.profile}
              </p>
            </div>
          )}

          {/* All Information Display */}
          <div style={{ marginBottom: "40px" }}>
            {/* Basic Info */}
            <div style={{ marginBottom: "24px" }}>
              <div
                style={{
                  background: "rgba(255,255,255,0.08)",
                  backdropFilter: "blur(10px)",
                  borderRadius: "12px",
                  padding: "16px",
                  boxShadow: "0 4px 20px rgba(0,0,0,0.15)",
                }}
              >
                <div
                  style={{
                    display: "flex",
                    alignItems: "center",
                    marginBottom: "16px",
                  }}
                >
                  <div
                    style={{
                      width: "32px",
                      height: "32px",
                      borderRadius: "8px",
                      background: "rgba(255,255,255,0.15)",
                      display: "flex",
                      alignItems: "center",
                      justifyContent: "center",
                      marginRight: "12px",
                    }}
                  >
                    <div
                      style={{
                        width: "16px",
                        height: "16px",
                        borderRadius: "4px",
                        background: "#fff",
                        opacity: 0.9,
                      }}
                    />
                  </div>
                  <h3
                    style={{
                      color: "#fff",
                      fontSize: "18px",
                      fontWeight: "600",
                      margin: 0,
                      letterSpacing: "0.3px",
                    }}
                  >
                    Thông tin cá nhân
                  </h3>
                </div>

                <div
                  style={{
                    display: "flex",
                    flexDirection: "column",
                    gap: "12px",
                  }}
                >
                  {profileData?.basicInfo?.dayOfBirth && (
                    <div
                      style={{
                        background: "rgba(255,255,255,0.05)",
                        borderRadius: "8px",
                        padding: "12px 16px",
                      }}
                    >
                      <div
                        style={{
                          color: "#fff",
                          fontSize: "13px",
                          fontWeight: "500",
                          marginBottom: "4px",
                          opacity: 0.8,
                        }}
                      >
                        Ngày sinh
                      </div>
                      <div
                        style={{
                          color: "#f0f8ff",
                          fontSize: "15px",
                          fontWeight: "400",
                          lineHeight: "1.3",
                        }}
                      >
                        {profileData.basicInfo.dayOfBirth}
                      </div>
                    </div>
                  )}
                  {profileData?.basicInfo?.address && (
                    <div
                      style={{
                        background: "rgba(255,255,255,0.05)",
                        borderRadius: "8px",
                        padding: "12px 16px",
                      }}
                    >
                      <div
                        style={{
                          color: "#fff",
                          fontSize: "13px",
                          fontWeight: "500",
                          marginBottom: "4px",
                          opacity: 0.8,
                        }}
                      >
                        Địa chỉ
                      </div>
                      <div
                        style={{
                          color: "#f0f8ff",
                          fontSize: "15px",
                          fontWeight: "400",
                          lineHeight: "1.3",
                        }}
                      >
                        {profileData.basicInfo.address}
                      </div>
                    </div>
                  )}
                  {profileData?.ratingInfo &&
                    profileData.ratingInfo.totalRatings > 0 && (
                      <div
                        style={{
                          background: "rgba(255,255,255,0.05)",
                          borderRadius: "8px",
                          padding: "12px 16px",
                        }}
                      >
                        <div
                          style={{
                            color: "#fff",
                            fontSize: "13px",
                            fontWeight: "500",
                            marginBottom: "4px",
                            opacity: 0.8,
                          }}
                        >
                          Đánh giá
                        </div>
                        <div
                          style={{
                            color: "#f0f8ff",
                            fontSize: "15px",
                            fontWeight: "400",
                            lineHeight: "1.3",
                          }}
                        >
                          {profileData.ratingInfo.averageRating}/5 (
                          {profileData.ratingInfo.totalRatings} đánh giá)
                        </div>
                      </div>
                    )}
                </div>
              </div>
            </div>

            {/* Company Info - Always show if any company data exists */}
            {(() => {
              return (
                profileData?.companyInfo?.companyFullName ||
                profileData?.companyInfo?.companyBrandName ||
                profileData?.companyInfo?.taxCode ||
                profileData?.companyInfo?.businessField ||
                profileData?.companyInfo?.businessType ||
                profileData?.companyInfo?.headquartersAddress ||
                profileData?.companyInfo?.companyWebsite ||
                profileData?.companyInfo?.companyPhoneNumber ||
                profileData?.companyInfo?.companyEmail ||
                profileData?.companyInfo?.legalRepresentative ||
                profileData?.companyInfo?.legalRepresentativePosition ||
                profileData?.companyInfo?.companyLogo ||
                profileData?.companyInfo?.businessRegistrationNumber ||
                profileData?.companyInfo?.businessRegistrationDate ||
                profileData?.companyInfo?.businessRegistrationPlace
              );
            })() && (
              <div style={{ marginBottom: "24px" }}>
                <div
                  style={{
                    background: "rgba(255,255,255,0.08)",
                    backdropFilter: "blur(10px)",
                    borderRadius: "12px",
                    padding: "16px",
                    boxShadow: "0 4px 20px rgba(0,0,0,0.15)",
                  }}
                >
                  <div
                    style={{
                      display: "flex",
                      alignItems: "center",
                      marginBottom: "16px",
                    }}
                  >
                    <div
                      style={{
                        width: "32px",
                        height: "32px",
                        borderRadius: "8px",
                        background: "rgba(255,255,255,0.15)",
                        display: "flex",
                        alignItems: "center",
                        justifyContent: "center",
                        marginRight: "12px",
                      }}
                    >
                      <div
                        style={{
                          width: "16px",
                          height: "16px",
                          borderRadius: "4px",
                          background: "#fff",
                          opacity: 0.9,
                        }}
                      />
                    </div>
                    <h3
                      style={{
                        color: "#fff",
                        fontSize: "18px",
                        fontWeight: "600",
                        margin: 0,
                        letterSpacing: "0.3px",
                      }}
                    >
                      Thông tin công ty
                    </h3>
                  </div>
                  <div
                    style={{
                      display: "flex",
                      flexDirection: "column",
                      gap: "12px",
                    }}
                  >
                    {/* Company Logo */}
                    {profileData?.companyInfo?.companyLogo && (
                      <div
                        style={{
                          background: "rgba(255,255,255,0.05)",
                          borderRadius: "8px",
                          padding: "12px 16px",
                          textAlign: "center",
                        }}
                      >
                        <div
                          style={{
                            color: "#fff",
                            fontSize: "13px",
                            fontWeight: "500",
                            marginBottom: "8px",
                            opacity: 0.8,
                          }}
                        >
                          Logo công ty
                        </div>
                        <div
                          style={{
                            display: "flex",
                            justifyContent: "center",
                          }}
                        >
                          <Image
                            src={profileData.companyInfo.companyLogo}
                            alt="Logo công ty"
                            width={80}
                            height={80}
                            style={{
                              borderRadius: "8px",
                              border: "2px solid rgba(255, 255, 255, 0.2)",
                              cursor: "pointer",
                            }}
                            preview={{
                              mask: (
                                <span style={{ color: "#fff" }}>
                                  Xem chi tiết
                                </span>
                              ),
                              maskStyle: {
                                background: "rgba(0, 0, 0, 0.5)",
                                borderRadius: "4px",
                              },
                            }}
                          />
                        </div>
                      </div>
                    )}

                    {profileData?.companyInfo?.companyFullName && (
                      <div
                        style={{
                          background: "rgba(255,255,255,0.05)",
                          borderRadius: "8px",
                          padding: "12px 16px",
                        }}
                      >
                        <div
                          style={{
                            color: "#fff",
                            fontSize: "13px",
                            fontWeight: "500",
                            marginBottom: "4px",
                            opacity: 0.8,
                          }}
                        >
                          Tên công ty
                        </div>
                        <div
                          style={{
                            color: "#f0f8ff",
                            fontSize: "15px",
                            fontWeight: "400",
                            lineHeight: "1.3",
                          }}
                        >
                          {profileData.companyInfo.companyFullName}
                        </div>
                      </div>
                    )}
                    {profileData?.companyInfo?.companyBrandName && (
                      <div
                        style={{
                          background: "rgba(255,255,255,0.05)",
                          borderRadius: "8px",
                          padding: "12px 16px",
                        }}
                      >
                        <div
                          style={{
                            color: "#fff",
                            fontSize: "13px",
                            fontWeight: "500",
                            marginBottom: "4px",
                            opacity: 0.8,
                          }}
                        >
                          Thương hiệu
                        </div>
                        <div
                          style={{
                            color: "#f0f8ff",
                            fontSize: "15px",
                            fontWeight: "400",
                            lineHeight: "1.3",
                          }}
                        >
                          {profileData.companyInfo.companyBrandName}
                        </div>
                      </div>
                    )}
                    {profileData?.companyInfo?.taxCode && (
                      <div
                        style={{
                          background: "rgba(255,255,255,0.05)",
                          borderRadius: "8px",
                          padding: "12px 16px",
                        }}
                      >
                        <div
                          style={{
                            color: "#fff",
                            fontSize: "13px",
                            fontWeight: "500",
                            marginBottom: "4px",
                            opacity: 0.8,
                          }}
                        >
                          Mã số thuế
                        </div>
                        <div
                          style={{
                            color: "#f0f8ff",
                            fontSize: "15px",
                            fontWeight: "400",
                            lineHeight: "1.3",
                          }}
                        >
                          {profileData.companyInfo.taxCode}
                        </div>
                      </div>
                    )}
                    {profileData?.companyInfo?.businessField && (
                      <div
                        style={{
                          background: "rgba(255,255,255,0.05)",
                          borderRadius: "8px",
                          padding: "12px 16px",
                        }}
                      >
                        <div
                          style={{
                            color: "#fff",
                            fontSize: "13px",
                            fontWeight: "500",
                            marginBottom: "4px",
                            opacity: 0.8,
                          }}
                        >
                          Lĩnh vực
                        </div>
                        <div
                          style={{
                            color: "#f0f8ff",
                            fontSize: "15px",
                            fontWeight: "400",
                            lineHeight: "1.3",
                          }}
                        >
                          {profileData.companyInfo.businessField}
                        </div>
                      </div>
                    )}
                    {profileData?.companyInfo?.businessType && (
                      <div
                        style={{
                          background: "rgba(255,255,255,0.05)",
                          borderRadius: "8px",
                          padding: "12px 16px",
                        }}
                      >
                        <div
                          style={{
                            color: "#fff",
                            fontSize: "13px",
                            fontWeight: "500",
                            marginBottom: "4px",
                            opacity: 0.8,
                          }}
                        >
                          Loại hình
                        </div>
                        <div
                          style={{
                            color: "#f0f8ff",
                            fontSize: "15px",
                            fontWeight: "400",
                            lineHeight: "1.3",
                          }}
                        >
                          {profileData.companyInfo.businessType}
                        </div>
                      </div>
                    )}
                    {profileData?.companyInfo?.headquartersAddress && (
                      <div
                        style={{
                          background: "rgba(255,255,255,0.05)",
                          borderRadius: "8px",
                          padding: "12px 16px",
                        }}
                      >
                        <div
                          style={{
                            color: "#fff",
                            fontSize: "13px",
                            fontWeight: "500",
                            marginBottom: "4px",
                            opacity: 0.8,
                          }}
                        >
                          Địa chỉ trụ sở
                        </div>
                        <div
                          style={{
                            color: "#f0f8ff",
                            fontSize: "15px",
                            fontWeight: "400",
                            lineHeight: "1.3",
                          }}
                        >
                          {profileData.companyInfo.headquartersAddress}
                        </div>
                      </div>
                    )}
                    {profileData?.companyInfo?.companyWebsite && (
                      <div
                        style={{
                          background: "rgba(255,255,255,0.05)",
                          borderRadius: "8px",
                          padding: "12px 16px",
                        }}
                      >
                        <div
                          style={{
                            color: "#fff",
                            fontSize: "13px",
                            fontWeight: "500",
                            marginBottom: "4px",
                            opacity: 0.8,
                          }}
                        >
                          Website
                        </div>
                        <div
                          style={{
                            color: "#f0f8ff",
                            fontSize: "15px",
                            fontWeight: "400",
                            lineHeight: "1.3",
                          }}
                        >
                          {profileData.companyInfo.companyWebsite}
                        </div>
                      </div>
                    )}
                    {profileData?.companyInfo?.companyPhoneNumber && (
                      <div
                        style={{
                          background: "rgba(255,255,255,0.05)",
                          borderRadius: "8px",
                          padding: "12px 16px",
                        }}
                      >
                        <div
                          style={{
                            color: "#fff",
                            fontSize: "13px",
                            fontWeight: "500",
                            marginBottom: "4px",
                            opacity: 0.8,
                          }}
                        >
                          SĐT công ty
                        </div>
                        <div
                          style={{
                            color: "#f0f8ff",
                            fontSize: "15px",
                            fontWeight: "400",
                            lineHeight: "1.3",
                          }}
                        >
                          {profileData.companyInfo.companyPhoneNumber}
                        </div>
                      </div>
                    )}
                    {profileData?.companyInfo?.companyEmail && (
                      <div
                        style={{
                          background: "rgba(255,255,255,0.05)",
                          borderRadius: "8px",
                          padding: "12px 16px",
                        }}
                      >
                        <div
                          style={{
                            color: "#fff",
                            fontSize: "13px",
                            fontWeight: "500",
                            marginBottom: "4px",
                            opacity: 0.8,
                          }}
                        >
                          Email công ty
                        </div>
                        <div
                          style={{
                            color: "#f0f8ff",
                            fontSize: "15px",
                            fontWeight: "400",
                            lineHeight: "1.3",
                          }}
                        >
                          {profileData.companyInfo.companyEmail}
                        </div>
                      </div>
                    )}
                    {profileData?.companyInfo?.legalRepresentative && (
                      <div
                        style={{
                          background: "rgba(255,255,255,0.05)",
                          borderRadius: "8px",
                          padding: "12px 16px",
                        }}
                      >
                        <div
                          style={{
                            color: "#fff",
                            fontSize: "13px",
                            fontWeight: "500",
                            marginBottom: "4px",
                            opacity: 0.8,
                          }}
                        >
                          Người đại diện
                        </div>
                        <div
                          style={{
                            color: "#f0f8ff",
                            fontSize: "15px",
                            fontWeight: "400",
                            lineHeight: "1.3",
                          }}
                        >
                          {profileData.companyInfo.legalRepresentative}
                        </div>
                      </div>
                    )}
                    {profileData?.companyInfo?.legalRepresentativePosition && (
                      <div
                        style={{
                          background: "rgba(255,255,255,0.05)",
                          borderRadius: "8px",
                          padding: "12px 16px",
                        }}
                      >
                        <div
                          style={{
                            color: "#fff",
                            fontSize: "13px",
                            fontWeight: "500",
                            marginBottom: "4px",
                            opacity: 0.8,
                          }}
                        >
                          Chức vụ
                        </div>
                        <div
                          style={{
                            color: "#f0f8ff",
                            fontSize: "15px",
                            fontWeight: "400",
                            lineHeight: "1.3",
                          }}
                        >
                          {profileData.companyInfo.legalRepresentativePosition}
                        </div>
                      </div>
                    )}
                    {profileData?.companyInfo?.businessRegistrationNumber && (
                      <div
                        style={{
                          background: "rgba(255,255,255,0.05)",
                          borderRadius: "8px",
                          padding: "12px 16px",
                        }}
                      >
                        <div
                          style={{
                            color: "#fff",
                            fontSize: "13px",
                            fontWeight: "500",
                            marginBottom: "4px",
                            opacity: 0.8,
                          }}
                        >
                          Số giấy chứng nhận
                        </div>
                        <div
                          style={{
                            color: "#f0f8ff",
                            fontSize: "15px",
                            fontWeight: "400",
                            lineHeight: "1.3",
                          }}
                        >
                          {profileData.companyInfo.businessRegistrationNumber}
                        </div>
                      </div>
                    )}
                    {profileData?.companyInfo?.businessRegistrationDate && (
                      <div
                        style={{
                          background: "rgba(255,255,255,0.05)",
                          borderRadius: "8px",
                          padding: "12px 16px",
                        }}
                      >
                        <div
                          style={{
                            color: "#fff",
                            fontSize: "13px",
                            fontWeight: "500",
                            marginBottom: "4px",
                            opacity: 0.8,
                          }}
                        >
                          Ngày cấp
                        </div>
                        <div
                          style={{
                            color: "#f0f8ff",
                            fontSize: "15px",
                            fontWeight: "400",
                            lineHeight: "1.3",
                          }}
                        >
                          {profileData.companyInfo.businessRegistrationDate}
                        </div>
                      </div>
                    )}
                    {profileData?.companyInfo?.businessRegistrationPlace && (
                      <div
                        style={{
                          background: "rgba(255,255,255,0.05)",
                          borderRadius: "8px",
                          padding: "12px 16px",
                        }}
                      >
                        <div
                          style={{
                            color: "#fff",
                            fontSize: "13px",
                            fontWeight: "500",
                            marginBottom: "4px",
                            opacity: 0.8,
                          }}
                        >
                          Nơi cấp
                        </div>
                        <div
                          style={{
                            color: "#f0f8ff",
                            fontSize: "15px",
                            fontWeight: "400",
                            lineHeight: "1.3",
                          }}
                        >
                          {profileData.companyInfo.businessRegistrationPlace}
                        </div>
                      </div>
                    )}
                  </div>
                </div>
              </div>
            )}

            {/* Business Fields */}
            {profileData?.fieldInfo &&
              profileData.fieldInfo.fieldNames?.length > 0 && (
                <div style={{ marginBottom: "20px" }}>
                  <h3
                    style={{
                      color: "#fff",
                      fontSize: "18px",
                      fontWeight: "600",
                      marginBottom: "12px",
                      textShadow: "0 1px 2px rgba(0, 0, 0, 0.5)",
                    }}
                  >
                    Lĩnh vực kinh doanh
                  </h3>
                  <div
                    style={{ display: "flex", flexWrap: "wrap", gap: "8px" }}
                  >
                    {profileData.fieldInfo.fieldNames.map(
                      (fieldName, index) => (
                        <span
                          key={index}
                          style={{
                            background: "rgba(255, 255, 255, 0.2)",
                            color: "#fff",
                            padding: "6px 12px",
                            borderRadius: "16px",
                            fontSize: "12px",
                            fontWeight: "500",
                            backdropFilter: "blur(10px)",
                            border: "1px solid rgba(255, 255, 255, 0.3)",
                          }}
                        >
                          {fieldName}
                        </span>
                      )
                    )}
                  </div>
                </div>
              )}

            {/* Custom Fields */}
            {profileData?.profileTemplate?.customFields &&
              profileData.profileTemplate.customFields.length > 0 && (
                <div style={{ marginBottom: "20px" }}>
                  <h3
                    style={{
                      color: "#fff",
                      fontSize: "18px",
                      fontWeight: "600",
                      marginBottom: "12px",
                      textShadow: "0 1px 2px rgba(0, 0, 0, 0.5)",
                    }}
                  >
                    Thông tin bổ sung
                  </h3>
                  <div
                    style={{
                      display: "flex",
                      flexDirection: "column",
                      gap: "8px",
                    }}
                  >
                    {profileData.profileTemplate.customFields
                      .filter((field) => field.isVisible)
                      .map((field) => (
                        <div
                          key={field.id}
                          style={{ color: "#fff", fontSize: "14px" }}
                        >
                          <strong>{field.fieldName}:</strong> {field.fieldValue}
                        </div>
                      ))}
                  </div>
                </div>
              )}
          </div>

          {/* Share Button */}
          <Box
            onClick={handleShareProfile}
            style={{
              position: "absolute",
              top: "20px",
              right: "20px",
              width: "40px",
              height: "40px",
              borderRadius: "50%",
              background: "rgba(255, 255, 255, 0.2)",
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
              cursor: "pointer",
              transition: "all 0.2s",
              backdropFilter: "blur(10px)",
              border: "1px solid rgba(255, 255, 255, 0.3)",
            }}
          >
            <IoShareSocialOutline size={20} color="#fff" />
          </Box>
        </Box>
      </Box>

      {/* Share Modal */}
      <CommonShareModal ref={shareModalRef} />
    </Page>
  );
};

export default PublicProfileGiba;
