import React, { useState, useEffect } from "react";
import { Page } from "zmp-ui";
import { useParams } from "react-router-dom";
import useSetHeader from "../components/hooks/useSetHeader";
import { useRecoilValue } from "recoil";
import { token } from "../recoil/RecoilState";
import LoadingGiba from "../componentsGiba/LoadingGiba";
import axios from "axios";
import dfData from "../common/DefaultConfig.json";
import {
  User,
  Image,
  Link,
  Calendar,
  Hash,
  Type,
  AlignLeft,
  Eye,
  EyeOff,
  Share2,
  Heart,
  MessageCircle,
  Star,
} from "lucide-react";

/**
 * Profile Template Configuration Interface
 */
interface ProfileTemplate {
  id: string;
  userZaloId: string;
  visibleFields: string[];
  hiddenFields: string[];
  customDescription: string;
  coverImage: string;
  themeColor: string;
  isPublic: boolean;
  createdDate: string;
  updatedDate: string;
  customFields: CustomField[];
}

/**
 * Custom Field Interface
 */
interface CustomField {
  id: string;
  fieldName: string;
  fieldValue: string;
  fieldType: "text" | "textarea" | "image" | "link" | "number" | "date";
  displayOrder: number;
  isVisible: boolean;
}

/**
 * User Profile Interface
 */
interface UserProfile {
  id: string;
  name: string;
  email: string;
  phoneNumber: string;
  company: string;
  position: string;
  avatar: string;
  profile: string;
  dayOfBirth: string;
  address: string;
  averageRating: number;
  totalRatings: number;
}

/**
 * Standard Profile Fields
 */
const STANDARD_FIELDS = [
  { key: "fullname", label: "Họ và tên", type: "text" },
  { key: "email", label: "Email", type: "email" },
  { key: "phoneNumber", label: "Số điện thoại", type: "tel" },
  { key: "company", label: "Công ty", type: "text" },
  { key: "position", label: "Chức vụ", type: "text" },
  { key: "zaloAvatar", label: "Ảnh đại diện", type: "image" },
  { key: "profile", label: "Mô tả", type: "textarea" },
  { key: "dayOfBirth", label: "Ngày sinh", type: "date" },
  { key: "address", label: "Địa chỉ", type: "text" },
  { key: "averageRating", label: "Đánh giá trung bình", type: "number" },
  { key: "totalRatings", label: "Tổng số đánh giá", type: "number" },
];

/**
 * Field Type Options
 */
const FIELD_TYPES = [
  { value: "text", label: "Văn bản", icon: Type },
  {
    value: "textarea",
    label: "Văn bản dài",
    icon: AlignLeft,
  },
  {
    value: "image",
    label: "Hình ảnh",
    icon: Image,
  },
  { value: "link", label: "Liên kết", icon: Link },
  { value: "number", label: "Số", icon: Hash },
  {
    value: "date",
    label: "Ngày tháng",
    icon: Calendar,
  },
];

const ProfileViewGiba: React.FC = () => {
  const { userZaloId } = useParams<{ userZaloId: string }>();
  const userToken = useRecoilValue(token);
  const setHeader = useSetHeader();

  const [loading, setLoading] = useState(true);
  const [template, setTemplate] = useState<ProfileTemplate | null>(null);
  const [userProfile, setUserProfile] = useState<UserProfile | null>(null);
  const [error, setError] = useState<string>("");

  useEffect(() => {
    setHeader({
      title: "TRANG GIỚI THIỆU",
      showUserInfo: false,
      showMenuButton: false,
      showCloseButton: false,
      hasLeftIcon: true,
    });
  }, [setHeader]);

  /**
   * Load profile template and user data
   */
  const loadProfileData = async () => {
    try {
      setLoading(true);
      setError("");

      if (!userZaloId) {
        setError("Không tìm thấy thông tin người dùng");
        return;
      }

      console.log("Loading profile for user:", userZaloId);

      // Load profile template
      const templateResponse = await axios.get(
        `${dfData.domain}/api/memberships/profile-template/${userZaloId}`,
        {
          headers: userToken
            ? {
                Authorization: `Bearer ${userToken}`,
                "Content-Type": "application/json",
              }
            : {
                "Content-Type": "application/json",
              },
          timeout: 10000,
        }
      );

      if (
        templateResponse.data.success &&
        templateResponse.data.data.isPublic
      ) {
        setTemplate(templateResponse.data.data);
      } else {
        setError("Profile này không công khai hoặc không tồn tại");
        return;
      }

      // Load user profile data
      const profileResponse = await axios.get(
        `${dfData.domain}/api/memberships/profile/${userZaloId}`,
        {
          headers: userToken
            ? {
                Authorization: `Bearer ${userToken}`,
                "Content-Type": "application/json",
              }
            : {
                "Content-Type": "application/json",
              },
          timeout: 10000,
        }
      );

      console.log("Profile response:", profileResponse.data);

      if (profileResponse.data.success) {
        setUserProfile(profileResponse.data.data);
      }
    } catch (error: any) {
      console.error("Error loading profile data:", error);
      setError("Không thể tải thông tin profile");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadProfileData();
  }, [userZaloId]);

  if (loading) {
    return <LoadingGiba />;
  }

  if (error) {
    return (
      <Page className="bg-gray-50 min-h-screen">
        <div className="p-4">
          <div className="bg-white rounded-lg p-8 text-center shadow-sm">
            <EyeOff className="w-16 h-16 text-gray-400 mx-auto mb-4" />
            <h2 className="text-xl font-semibold text-gray-900 mb-2">
              Không thể xem profile
            </h2>
            <p className="text-gray-600 mb-4">{error}</p>
            <button
              onClick={() => window.history.back()}
              className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
            >
              Quay lại
            </button>
          </div>
        </div>
      </Page>
    );
  }

  if (!template || !userProfile) {
    return (
      <Page className="bg-gray-50 min-h-screen">
        <div className="p-4">
          <div className="bg-white rounded-lg p-8 text-center shadow-sm">
            <User className="w-16 h-16 text-gray-400 mx-auto mb-4" />
            <h2 className="text-xl font-semibold text-gray-900 mb-2">
              Profile không tồn tại
            </h2>
            <p className="text-gray-600 mb-4">
              Không tìm thấy thông tin profile của người dùng này.
            </p>
          </div>
        </div>
      </Page>
    );
  }

  return (
    <Page className="bg-gray-50">
      {/* Cover Image */}
      {template.coverImage && (
        <div
          className="h-40 bg-cover bg-center relative"
          style={{
            backgroundImage: `url(${template.coverImage})`,
            backgroundColor: template.themeColor + "20",
          }}
        />
      )}

      {/* Profile Header */}
      <div className="px-4 pb-4">
        <div
          className="bg-white rounded-t-2xl -mt-8 relative z-10 px-4 pt-8 pb-4"
          style={{ backgroundColor: template.themeColor + "05" }}
        >
          {/* Avatar */}
          <div className="flex justify-center -mt-16 mb-4">
            <div className="w-24 h-24 bg-white rounded-full p-1 shadow-lg">
              <div className="w-full h-full bg-gray-200 rounded-full flex items-center justify-center overflow-hidden">
                {userProfile.avatar ? (
                  <img
                    src={userProfile.avatar}
                    alt="Avatar"
                    className="w-full h-full object-cover"
                  />
                ) : (
                  <User className="w-12 h-12 text-gray-400" />
                )}
              </div>
            </div>
          </div>

          {/* User Info */}
          <div className="text-center mb-4">
            <h1 className="text-xl font-bold text-gray-900 mb-2">
              {userProfile.name}
            </h1>
            {template.customDescription && (
              <p className="text-sm text-gray-600 mb-3 px-4">
                {template.customDescription}
              </p>
            )}

            {/* Rating */}
            {userProfile.averageRating > 0 && (
              <div className="flex items-center justify-center gap-2 mb-4">
                <div className="flex items-center">
                  {[...Array(5)].map((_, i) => (
                    <Star
                      key={i}
                      className={`w-4 h-4 ${
                        i < Math.floor(userProfile.averageRating)
                          ? "text-yellow-400 fill-current"
                          : "text-gray-300"
                      }`}
                    />
                  ))}
                </div>
                <span className="text-xs text-gray-600">
                  {userProfile.averageRating.toFixed(1)} (
                  {userProfile.totalRatings})
                </span>
              </div>
            )}

            {/* Action Buttons */}
            <div className="flex justify-center gap-3 mb-4">
              <button className="flex items-center gap-1 px-4 py-2 bg-blue-600 text-white rounded-full text-sm font-medium">
                <Heart className="w-4 h-4" />
                Theo dõi
              </button>
              <button className="flex items-center gap-1 px-4 py-2 bg-gray-100 text-gray-700 rounded-full text-sm font-medium">
                <MessageCircle className="w-4 h-4" />
                Nhắn tin
              </button>
              <button className="flex items-center gap-1 px-4 py-2 bg-gray-100 text-gray-700 rounded-full text-sm font-medium">
                <Share2 className="w-4 h-4" />
                Chia sẻ
              </button>
            </div>
          </div>
        </div>

        {/* Profile Fields */}
        <div className="bg-white rounded-2xl px-4 py-4 mb-4">
          {/* Standard Fields */}
          {STANDARD_FIELDS.map((field) => {
            if (!template.visibleFields.includes(field.key)) return null;

            let fieldValue = "";
            switch (field.key) {
              case "fullname":
                fieldValue = userProfile.name || "Chưa cập nhật";
                break;
              case "email":
                fieldValue = userProfile.email || "Chưa cập nhật";
                break;
              case "phoneNumber":
                fieldValue = userProfile.phoneNumber || "Chưa cập nhật";
                break;
              case "company":
                fieldValue = userProfile.company || "Chưa cập nhật";
                break;
              case "position":
                fieldValue = userProfile.position || "Chưa cập nhật";
                break;
              case "address":
                fieldValue = userProfile.address || "Chưa cập nhật";
                break;
              case "dayOfBirth":
                fieldValue = userProfile.dayOfBirth || "Chưa cập nhật";
                break;
              case "averageRating":
                fieldValue = userProfile.averageRating
                  ? `${userProfile.averageRating}/5`
                  : "Chưa có đánh giá";
                break;
              case "totalRatings":
                fieldValue = userProfile.totalRatings
                  ? `${userProfile.totalRatings} đánh giá`
                  : "Chưa có đánh giá";
                break;
              default:
                fieldValue = "Chưa cập nhật";
            }

            return (
              <div
                key={field.key}
                className="flex items-center gap-3 py-3 border-b border-gray-100 last:border-b-0"
              >
                <div className="w-8 h-8 bg-gray-100 rounded-full flex items-center justify-center flex-shrink-0">
                  {field.type === "image" ? (
                    <Image className="w-4 h-4 text-gray-600" />
                  ) : field.type === "email" ? (
                    <Type className="w-4 h-4 text-gray-600" />
                  ) : field.type === "tel" ? (
                    <Type className="w-4 h-4 text-gray-600" />
                  ) : field.type === "date" ? (
                    <Calendar className="w-4 h-4 text-gray-600" />
                  ) : field.type === "number" ? (
                    <Hash className="w-4 h-4 text-gray-600" />
                  ) : field.type === "textarea" ? (
                    <AlignLeft className="w-4 h-4 text-gray-600" />
                  ) : (
                    <Type className="w-4 h-4 text-gray-600" />
                  )}
                </div>
                <div className="flex-1 min-w-0">
                  <p className="text-xs font-medium text-gray-500 uppercase tracking-wide">
                    {field.label}
                  </p>
                  <p className="text-sm text-gray-900 break-words mt-1">
                    {fieldValue}
                  </p>
                </div>
              </div>
            );
          })}

          {/* Custom Fields */}
          {template.customFields
            .filter((field) => field.isVisible)
            .map((field) => (
              <div
                key={field.id}
                className="flex items-center gap-3 py-3 border-b border-gray-100 last:border-b-0"
              >
                <div className="w-8 h-8 bg-gray-100 rounded-full flex items-center justify-center flex-shrink-0">
                  {(() => {
                    const fieldType = FIELD_TYPES.find(
                      (t) => t.value === field.fieldType
                    );
                    const IconComponent = fieldType?.icon || Type;
                    return <IconComponent className="w-4 h-4 text-gray-600" />;
                  })()}
                </div>
                <div className="flex-1 min-w-0">
                  <p className="text-xs font-medium text-gray-500 uppercase tracking-wide">
                    {field.fieldName}
                  </p>
                  <p className="text-sm text-gray-900 break-words mt-1">
                    {field.fieldType === "link" ? (
                      <a
                        href={field.fieldValue}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="text-blue-600 underline"
                      >
                        {field.fieldValue}
                      </a>
                    ) : (
                      field.fieldValue
                    )}
                  </p>
                </div>
              </div>
            ))}

          {/* Empty state */}
          {template.visibleFields.length === 0 &&
            template.customFields.filter((f) => f.isVisible).length === 0 && (
              <div className="text-center py-8 text-gray-500">
                <User className="w-8 h-8 mx-auto mb-2 opacity-50" />
                <p className="text-sm">Chưa có thông tin nào được hiển thị</p>
              </div>
            )}
        </div>

        {/* Footer */}
        <div className="text-center pb-6">
          <p className="text-xs text-gray-400">
            Profile được tạo bởi Giba Platform
          </p>
        </div>
      </div>
    </Page>
  );
};

export default ProfileViewGiba;
