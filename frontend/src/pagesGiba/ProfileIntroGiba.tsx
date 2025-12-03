import React, { useState, useEffect, useCallback, useRef } from "react";
import { Page, Button, Input, Switch, Select } from "zmp-ui";
import { useNavigate } from "zmp-ui";
import useSetHeader from "../components/hooks/useSetHeader";
import { useRecoilValue } from "recoil";
import { token, infoUser } from "../recoil/RecoilState";
import LoadingGiba from "../componentsGiba/LoadingGiba";
import { toast } from "react-toastify";
import axios from "axios";
import dfData from "../common/DefaultConfig.json";
import {
  User,
  Settings,
  Plus,
  Trash2,
  Eye,
  EyeOff,
  Palette,
  Image as ImageIcon,
  Link,
  Calendar,
  Hash,
  Type,
  AlignLeft,
  Save,
  RefreshCw,
  Share2,
  Edit,
  Check,
  X,
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
  // Thông tin pháp nhân
  { key: "companyFullName", label: "Tên công ty đầy đủ", type: "text" },
  { key: "companyBrandName", label: "Tên thương hiệu", type: "text" },
  { key: "taxCode", label: "Mã số thuế", type: "text" },
  { key: "businessField", label: "Lĩnh vực kinh doanh", type: "text" },
  { key: "businessType", label: "Loại hình doanh nghiệp", type: "text" },
  { key: "headquartersAddress", label: "Địa chỉ trụ sở chính", type: "text" },
  { key: "companyWebsite", label: "Website công ty", type: "url" },
  { key: "companyPhoneNumber", label: "Số điện thoại công ty", type: "tel" },
  { key: "companyEmail", label: "Email công ty", type: "email" },
  { key: "legalRepresentative", label: "Người đại diện pháp lý", type: "text" },
  {
    key: "legalRepresentativePosition",
    label: "Chức vụ người đại diện",
    type: "text",
  },
  { key: "companyLogo", label: "Logo công ty", type: "image" },
  {
    key: "businessRegistrationNumber",
    label: "Số giấy chứng nhận đăng ký kinh doanh",
    type: "text",
  },
  {
    key: "businessRegistrationDate",
    label: "Ngày cấp giấy chứng nhận",
    type: "date",
  },
  {
    key: "businessRegistrationPlace",
    label: "Nơi cấp giấy chứng nhận",
    type: "text",
  },
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

  { value: "link", label: "Liên kết", icon: Link },
  { value: "number", label: "Số", icon: Hash },
  {
    value: "date",
    label: "Ngày tháng",
    icon: Calendar,
  },
];

/**
 * Theme Color Options - Expanded palette with unique colors
 */
const THEME_COLORS = [
  // Blues
  "#0066cc", // Blue
  "#1e40af", // Dark Blue
  "#0ea5e9", // Sky Blue
  "#06b6d4", // Cyan

  // Greens
  "#00cc66", // Green
  "#10b981", // Emerald
  "#22c55e", // Light Green
  "#84cc16", // Lime

  // Purples & Pinks
  "#6600cc", // Purple
  "#8b5cf6", // Violet
  "#a855f7", // Purple Light
  "#ec4899", // Pink
  "#f43f5e", // Rose

  // Reds & Oranges
  "#dc2626", // Red
  "#ef4444", // Light Red
  "#ff6600", // Orange
  "#f97316", // Dark Orange
  "#fb923c", // Light Orange

  // Yellows
  "#eab308", // Yellow
  "#facc15", // Light Yellow
  "#fbbf24", // Amber

  // Teals & Grays
  "#14b8a6", // Teal
  "#2dd4bf", // Teal Light
  "#64748b", // Slate
  "#6b7280", // Gray
  "#374151", // Dark Gray
  "#1f2937", // Charcoal
];

const ProfileIntroGiba: React.FC = () => {
  const navigate = useNavigate();
  const userToken = useRecoilValue(token);
  const userInfo = useRecoilValue(infoUser);

  // Get slug from URL params using window.location
  const getSlugFromUrl = () => {
    const urlParams = new URLSearchParams(window.location.search);
    return urlParams.get("slug");
  };
  const userSlug = getSlugFromUrl();

  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [template, setTemplate] = useState<ProfileTemplate | null>(null);
  const [activeTab, setActiveTab] = useState<
    "view" | "fields" | "custom" | "design" | "preview"
  >("view");

  // Form states
  const [visibleFields, setVisibleFields] = useState<string[]>([]);
  const [hiddenFields, setHiddenFields] = useState<string[]>([]);
  const [customDescription, setCustomDescription] = useState("");
  const [themeColor, setThemeColor] = useState("#0066cc");
  const [isPublic, setIsPublic] = useState(true);
  const [coverImage, setCoverImage] = useState("");
  const setHeader = useSetHeader();

  // Custom fields
  const [customFields, setCustomFields] = useState<CustomField[]>([]);
  const [newField, setNewField] = useState({
    fieldName: "",
    fieldValue: "",
    fieldType: "text" as const,
  });
  const [editingField, setEditingField] = useState<string | null>(null);
  const [editFieldData, setEditFieldData] = useState<Partial<CustomField>>({});

  // Use ref to prevent multiple API calls
  const isLoadingRef = useRef(false);

  /**
   * Load profile template configuration
   */
  const loadProfileTemplate = useCallback(async () => {
    // Prevent duplicate calls
    if (isLoadingRef.current) {
      console.log("Already loading profile template, skipping...");
      return;
    }

    try {
      isLoadingRef.current = true;
      setLoading(true);

      if (!userToken) {
        setVisibleFields(["fullname", "email", "phoneNumber"]);
        setHiddenFields([]);
        setCustomDescription("");
        setThemeColor("#0066cc");
        setIsPublic(true);
        setCoverImage("");
        setCustomFields([]);
        setLoading(false);
        return;
      }

      const response = await axios.get(
        `${dfData.domain}/api/memberships/profile-template`,
        {
          headers: {
            Authorization: `Bearer ${userToken}`,
            "Content-Type": "application/json",
          },
          timeout: 10000,
        }
      );

      if (response.data.success) {
        const data = response.data.data;
        setTemplate(data);
        setVisibleFields(
          data.visibleFields || ["fullname", "email", "phoneNumber"]
        );
        setHiddenFields(data.hiddenFields || []);
        setCustomDescription(data.customDescription || "");
        setThemeColor(data.themeColor || "#0066cc");
        setIsPublic(data.isPublic !== undefined ? data.isPublic : true);
        setCoverImage(data.coverImage || "");
        setCustomFields(data.customFields || []);
      } else {
        setTemplate(null);
        setVisibleFields(["fullname", "email", "phoneNumber"]);
        setHiddenFields([]);
        setCustomDescription("");
        setThemeColor("#0066cc");
        setIsPublic(true);
        setCoverImage("");
        setCustomFields([]);
      }
    } catch (error: any) {
      setTemplate(null);
      setVisibleFields(["fullname", "email", "phoneNumber"]);
      setHiddenFields([]);
      setCustomDescription("");
      setThemeColor("#0066cc");
      setIsPublic(true);
      setCoverImage("");
      setCustomFields([]);
    } finally {
      setLoading(false);
      isLoadingRef.current = false;
    }
  }, [userToken]);

  const saveProfileTemplate = async () => {
    try {
      setSaving(true);

      const payload = {
        visibleFields,
        hiddenFields,
        customDescription,
        themeColor,
        isPublic,
        coverImage,
      };

      const response = template
        ? await axios.put(
            `${dfData.domain}/api/memberships/profile-template`,
            payload,
            {
              headers: {
                Authorization: `Bearer ${userToken}`,
                "Content-Type": "application/json",
              },
            }
          )
        : await axios.post(
            `${dfData.domain}/api/memberships/profile-template`,
            payload,
            {
              headers: {
                Authorization: `Bearer ${userToken}`,
                "Content-Type": "application/json",
              },
            }
          );

      if (response.data.success) {
        const mockTemplate = {
          id: "mock-template-id",
          userZaloId: "mock-user-id",
          visibleFields,
          hiddenFields,
          customDescription,
          coverImage,
          themeColor,
          isPublic,
          createdDate: new Date().toISOString(),
          updatedDate: new Date().toISOString(),
          customFields: customFields,
        };
        setTemplate(mockTemplate);
        toast.success("Lưu cấu hình profile thành công!");
      }
    } catch (error: any) {
      console.error("Error saving profile template:", error);
      toast.error("Có lỗi xảy ra khi lưu cấu hình. Vui lòng thử lại!");
      const mockTemplate = {
        id: "local-template-id",
        userZaloId: "local-user-id",
        visibleFields,
        hiddenFields,
        customDescription,
        coverImage,
        themeColor,
        isPublic,
        createdDate: new Date().toISOString(),
        updatedDate: new Date().toISOString(),
        customFields: customFields,
      };
      setTemplate(mockTemplate);
    } finally {
      setSaving(false);
    }
  };

  /**
   * Add custom field
   */
  const addCustomField = async () => {
    if (!newField.fieldName.trim()) {
      toast.error("Vui lòng nhập tên trường");
      return;
    }

    try {
      const payload = {
        fieldName: newField.fieldName,
        fieldValue: newField.fieldValue,
        fieldType: newField.fieldType,
        displayOrder: customFields.length,
        isVisible: true,
      };

      const response = await axios.post(
        `${dfData.domain}/api/memberships/profile-template/custom-fields`,
        payload,
        {
          headers: {
            Authorization: `Bearer ${userToken}`,
            "Content-Type": "application/json",
          },
        }
      );

      if (response.data.success) {
        const newCustomField: CustomField = {
          id: response.data.data.id || Date.now().toString(),
          fieldName: newField.fieldName,
          fieldValue: newField.fieldValue,
          fieldType: newField.fieldType,
          displayOrder: customFields.length,
          isVisible: true,
        };

        setCustomFields([...customFields, newCustomField]);
        setNewField({ fieldName: "", fieldValue: "", fieldType: "text" });
        toast.success("Thêm trường tùy chỉnh thành công!");
      } else {
        toast.error("Không thể thêm trường tùy chỉnh. Vui lòng thử lại!");
      }
    } catch (error: any) {
      console.error("Error adding custom field:", error);
      toast.error("Có lỗi xảy ra khi thêm trường tùy chỉnh!");

      // Fallback: Add locally if API fails
      const newCustomField: CustomField = {
        id: Date.now().toString(),
        fieldName: newField.fieldName,
        fieldValue: newField.fieldValue,
        fieldType: newField.fieldType,
        displayOrder: customFields.length,
        isVisible: true,
      };

      setCustomFields([...customFields, newCustomField]);
      setNewField({ fieldName: "", fieldValue: "", fieldType: "text" });
      toast.success("Thêm trường tùy chỉnh thành công (lưu cục bộ)!");
    }
  };

  /**
   * Delete custom field
   */
  const deleteCustomField = async (fieldId: string) => {
    try {
      const response = await axios.delete(
        `${dfData.domain}/api/memberships/profile-template/custom-fields/${fieldId}`,
        {
          headers: {
            Authorization: `Bearer ${userToken}`,
            "Content-Type": "application/json",
          },
        }
      );

      if (response.data.success) {
        setCustomFields(customFields.filter((field) => field.id !== fieldId));
        toast.success("Xóa trường tùy chỉnh thành công!");
      } else {
        toast.error("Không thể xóa trường tùy chỉnh. Vui lòng thử lại!");
      }
    } catch (error: any) {
      console.error("Error deleting custom field:", error);
      toast.error("Có lỗi xảy ra khi xóa trường tùy chỉnh!");

      setCustomFields(customFields.filter((field) => field.id !== fieldId));
      toast.success("Xóa trường tùy chỉnh thành công (cục bộ)!");
    }
  };

  const updateCustomField = async (
    fieldId: string,
    updatedField: Partial<CustomField>
  ) => {
    try {
      const payload = {
        fieldName: updatedField.fieldName,
        fieldValue: updatedField.fieldValue,
        fieldType: updatedField.fieldType,
        displayOrder: updatedField.displayOrder,
        isVisible: updatedField.isVisible,
      };

      const response = await axios.put(
        `${dfData.domain}/api/memberships/profile-template/custom-fields/${fieldId}`,
        payload,
        {
          headers: {
            Authorization: `Bearer ${userToken}`,
            "Content-Type": "application/json",
          },
        }
      );

      if (response.data.success) {
        setCustomFields(
          customFields.map((field) =>
            field.id === fieldId ? { ...field, ...updatedField } : field
          )
        );
        toast.success("Cập nhật trường tùy chỉnh thành công!");
      } else {
        toast.error("Không thể cập nhật trường tùy chỉnh. Vui lòng thử lại!");
      }
    } catch (error: any) {
      console.error("Error updating custom field:", error);
      toast.error("Có lỗi xảy ra khi cập nhật trường tùy chỉnh!");

      setCustomFields(
        customFields.map((field) =>
          field.id === fieldId ? { ...field, ...updatedField } : field
        )
      );
      toast.success("Cập nhật trường tùy chỉnh thành công (cục bộ)!");
    }
  };

  const startEditingField = (field: CustomField) => {
    setEditingField(field.id);
    setEditFieldData({
      fieldName: field.fieldName,
      fieldValue: field.fieldValue,
      fieldType: field.fieldType,
      isVisible: field.isVisible,
    });
  };

  const cancelEditingField = () => {
    setEditingField(null);
    setEditFieldData({});
  };

  const saveEditedField = async () => {
    if (!editingField || !editFieldData.fieldName?.trim()) {
      toast.error("Vui lòng nhập tên trường");
      return;
    }

    await updateCustomField(editingField, editFieldData);
    setEditingField(null);
    setEditFieldData({});
  };

  const toggleFieldVisibility = (fieldKey: string, isVisible: boolean) => {
    if (isVisible) {
      setVisibleFields((prev) => {
        if (!prev.includes(fieldKey)) {
          return [...prev, fieldKey];
        }
        return prev;
      });
      setHiddenFields((prev) => prev.filter((f) => f !== fieldKey));
    } else {
      setHiddenFields((prev) => {
        if (!prev.includes(fieldKey)) {
          return [...prev, fieldKey];
        }
        return prev;
      });
      setVisibleFields((prev) => prev.filter((f) => f !== fieldKey));
    }
  };

  useEffect(() => {
    setHeader({
      title: "TRANG GIỚI THIỆU",
      showUserInfo: false,
      showMenuButton: false,
      showCloseButton: false,
      hasLeftIcon: true,
    });
  }, []);

  useEffect(() => {
    if (userToken) {
      loadProfileTemplate();
    } else {
      setVisibleFields(["fullname", "email", "phoneNumber"]);
      setHiddenFields([]);
      setCustomDescription("");
      setThemeColor("#0066cc");
      setIsPublic(true);
      setCoverImage("");
      setCustomFields([]);
      setTemplate(null);
      setLoading(false);
    }
  }, [userToken, loadProfileTemplate]);

  if (loading) {
    return <LoadingGiba />;
  }

  return (
    <Page className="bg-gray-50 min-h-screen">
      {/* Header */}
      <div className="bg-white px-4 py-3 border-b border-gray-100"></div>

      {/* Mobile Tabs */}
      <div className="bg-white border-b border-gray-100 mt-[30px]">
        <div className="flex overflow-x-auto scrollbar-hide">
          {[
            {
              id: "view",
              label: "Hiện tại",
              icon: <User className="w-4 h-4" />,
            },
            {
              id: "fields",
              label: "Trường",
              icon: <Settings className="w-4 h-4" />,
            },
            {
              id: "custom",
              label: "Tùy chỉnh",
              icon: <Plus className="w-4 h-4" />,
            },
            {
              id: "design",
              label: "Thiết kế",
              icon: <Palette className="w-4 h-4" />,
            },
            {
              id: "preview",
              label: "Xem trước",
              icon: <Eye className="w-4 h-4" />,
            },
          ].map((tab) => (
            <button
              key={tab.id}
              onClick={() => {
                if (tab.id === "preview") {
                  // Navigate to MemberDetailGiba with slug from URL params or userInfo
                  const finalSlug =
                    userSlug ||
                    (userInfo as any)?.slug ||
                    (userInfo as any)?.zaloId;
                  if (finalSlug) {
                    navigate(`/giba/member-detail/${finalSlug}`);
                  } else {
                    console.error("Không tìm thấy slug của user");
                    toast.error("Không tìm thấy thông tin profile");
                  }
                } else {
                  setActiveTab(tab.id as any);
                }
              }}
              className={`flex-shrink-0 flex flex-col items-center gap-1 py-3 px-3 text-xs font-medium transition-colors ${
                activeTab === tab.id
                  ? "text-blue-600 border-b-2 border-blue-600"
                  : "text-gray-500"
              }`}
            >
              {tab.icon}
              <span className="text-xs">{tab.label}</span>
            </button>
          ))}
        </div>
      </div>

      {/* Content */}
      <div className="flex-1 overflow-y-auto pb-20">
        <div className="p-4">
          {/* View Tab */}
          {activeTab === "view" && (
            <div className="space-y-4">
              <div className="flex sm:flex-row sm:items-center sm:justify-between gap-3 mb-4">
                <div className="flex-1 min-w-0">
                  <h3 className="text-lg font-medium text-gray-900">
                    Template hiện tại
                  </h3>
                  <p className="text-sm text-gray-500">
                    Xem cấu hình profile template của bạn
                  </p>
                </div>
                <div className="flex-shrink-0">
                  <Button
                    onClick={() => setActiveTab("fields")}
                    type="highlight"
                    size="small"
                    className="flex items-center gap-1 px-3 py-1.5"
                  >
                    <Settings className="w-4 h-4" />
                  </Button>
                </div>
              </div>

              {template ? (
                <div className="space-y-4">
                  {/* Template Info */}
                  <div className="bg-white rounded-xl p-4 border border-gray-100">
                    <h4 className="font-medium text-gray-900 mb-3">
                      Thông tin template
                    </h4>
                    <div className="space-y-3">
                      <div className="flex items-center justify-between">
                        <span className="text-sm text-gray-500">
                          Trạng thái:
                        </span>
                        <span
                          className={`px-3 py-1 rounded-full text-xs font-medium ${
                            isPublic
                              ? "bg-green-100 text-green-700"
                              : "bg-gray-100 text-gray-600"
                          }`}
                        >
                          {isPublic ? "Công khai" : "Riêng tư"}
                        </span>
                      </div>
                      <div className="flex items-center justify-between">
                        <span className="text-sm text-gray-500">
                          Màu chủ đề:
                        </span>
                        <div className="flex items-center gap-2">
                          <span
                            className="w-5 h-5 rounded-full border border-gray-300"
                            style={{ backgroundColor: themeColor }}
                          ></span>
                          <span className="text-xs text-gray-600">
                            {themeColor}
                          </span>
                        </div>
                      </div>
                    </div>
                  </div>

                  {/* Visible Fields */}
                  <div className="bg-white rounded-xl p-4 border border-gray-100">
                    <h4 className="font-medium text-gray-900 mb-3">
                      Trường hiển thị
                    </h4>
                    {visibleFields.length > 0 ? (
                      <div className="space-y-2">
                        {visibleFields.map((fieldKey) => {
                          const field = STANDARD_FIELDS.find(
                            (f) => f.key === fieldKey
                          );
                          return field ? (
                            <div
                              key={fieldKey}
                              className="flex items-center gap-2 text-sm p-2 bg-gray-50 rounded-lg"
                            >
                              <div className="w-6 h-6 bg-blue-100 rounded-full flex items-center justify-center flex-shrink-0">
                                <Type className="w-3 h-3 text-blue-600" />
                              </div>
                              <span className="truncate">{field.label}</span>
                            </div>
                          ) : null;
                        })}
                      </div>
                    ) : (
                      <p className="text-sm text-gray-500 text-center py-4">
                        Chưa có trường nào được hiển thị
                      </p>
                    )}
                  </div>

                  {/* Custom Fields */}
                  <div className="bg-white rounded-xl p-4 border border-gray-100">
                    <h4 className="font-medium text-gray-900 mb-3">
                      Trường tùy chỉnh
                    </h4>
                    {customFields.length > 0 ? (
                      <div className="space-y-2">
                        {customFields.map((field) => (
                          <div
                            key={field.id}
                            className="flex items-center gap-2 text-sm p-2 bg-gray-50 rounded-lg"
                          >
                            <div className="w-6 h-6 bg-green-100 rounded-full flex items-center justify-center flex-shrink-0">
                              <Type className="w-3 h-3 text-green-600" />
                            </div>
                            <div className="flex-1 min-w-0">
                              <div className="flex items-center gap-2">
                                <span className="font-medium truncate">
                                  {field.fieldName}
                                </span>
                                <span className="text-gray-400 text-xs flex-shrink-0">
                                  ({field.fieldType})
                                </span>
                              </div>
                            </div>
                          </div>
                        ))}
                      </div>
                    ) : (
                      <p className="text-sm text-gray-500 text-center py-4">
                        Chưa có trường tùy chỉnh nào
                      </p>
                    )}
                  </div>
                </div>
              ) : (
                <div className="text-center py-12">
                  <div className="w-16 h-16 bg-gray-100 rounded-full flex items-center justify-center mx-auto mb-4">
                    <User className="w-8 h-8 text-gray-400" />
                  </div>
                  <h3 className="text-lg font-medium text-gray-900 mb-2">
                    Chưa có template
                  </h3>
                  <p className="text-sm text-gray-500 mb-6 px-4">
                    Bạn chưa có profile template. Hãy tạo template đầu tiên của
                    bạn.
                  </p>
                  <div className="space-y-3">
                    <Button
                      onClick={() => setActiveTab("fields")}
                      type="highlight"
                      className="w-full"
                    >
                      <Plus className="w-4 h-4 mr-2" />
                      Tạo template
                    </Button>
                    <Button
                      onClick={() => loadProfileTemplate()}
                      type="neutral"
                      className="w-full"
                    >
                      <RefreshCw className="w-4 h-4 mr-2" />
                      Thử lại
                    </Button>
                  </div>
                </div>
              )}
            </div>
          )}

          {/* Fields Tab */}
          {activeTab === "fields" && (
            <div className="space-y-4">
              <h3 className="text-lg font-medium text-gray-900">
                Quản lý trường thông tin
              </h3>
              <p className="text-sm text-gray-500">
                Chọn các trường thông tin hiển thị trên profile
              </p>

              <div className="space-y-3">
                {STANDARD_FIELDS.map((field) => {
                  const isVisible = visibleFields.includes(field.key);
                  return (
                    <div
                      key={field.key}
                      className="flex items-center justify-between p-3 bg-white rounded-xl border border-gray-100"
                    >
                      <div className="flex items-center gap-3 flex-1 min-w-0">
                        <div className="w-8 h-8 bg-blue-100 rounded-full flex items-center justify-center flex-shrink-0">
                          {field.type === "image" ? (
                            <ImageIcon className="w-4 h-4 text-blue-600" />
                          ) : field.type === "email" ? (
                            <Type className="w-4 h-4 text-blue-600" />
                          ) : field.type === "tel" ? (
                            <Type className="w-4 h-4 text-blue-600" />
                          ) : field.type === "date" ? (
                            <Calendar className="w-4 h-4 text-blue-600" />
                          ) : field.type === "number" ? (
                            <Hash className="w-4 h-4 text-blue-600" />
                          ) : field.type === "textarea" ? (
                            <AlignLeft className="w-4 h-4 text-blue-600" />
                          ) : (
                            <Type className="w-4 h-4 text-blue-600" />
                          )}
                        </div>
                        <div className="flex-1 min-w-0">
                          <p className="font-medium text-gray-900 truncate">
                            {field.label}
                          </p>
                          <p className="text-xs text-gray-500 truncate">
                            {field.key}
                          </p>
                        </div>
                      </div>
                      <div className="flex-shrink-0 ml-2">
                        <button
                          onClick={() =>
                            toggleFieldVisibility(field.key, !isVisible)
                          }
                          className={`flex items-center gap-2 px-3 py-1.5 rounded-full text-xs font-medium transition-colors whitespace-nowrap ${
                            isVisible
                              ? "bg-green-100 text-green-700"
                              : "bg-gray-100 text-gray-500"
                          }`}
                        >
                          {isVisible ? (
                            <Eye className="w-3 h-3" />
                          ) : (
                            <EyeOff className="w-3 h-3" />
                          )}
                          {isVisible ? "Hiển thị" : "Ẩn"}
                        </button>
                      </div>
                    </div>
                  );
                })}
              </div>
            </div>
          )}

          {/* Custom Fields Tab */}
          {activeTab === "custom" && (
            <div className="space-y-4">
              <h3 className="text-lg font-medium text-gray-900">
                Trường tùy chỉnh
              </h3>
              <p className="text-sm text-gray-500">
                Thêm các trường thông tin riêng của bạn
              </p>

              {/* Add new field form */}
              <div className="bg-white rounded-xl p-4 border border-gray-100 space-y-3">
                <h4 className="font-medium text-gray-900">Thêm trường mới</h4>
                <div className="space-y-3">
                  <Input
                    placeholder="Tên trường (ví dụ: Kỹ năng, Sở thích...)"
                    value={newField.fieldName}
                    onChange={(e) =>
                      setNewField((prev) => ({
                        ...prev,
                        fieldName: e.target.value,
                      }))
                    }
                  />
                  <Input
                    placeholder="Giá trị trường"
                    value={newField.fieldValue}
                    onChange={(e) =>
                      setNewField((prev) => ({
                        ...prev,
                        fieldValue: e.target.value,
                      }))
                    }
                  />
                  <Select
                    value={newField.fieldType}
                    onChange={(value) =>
                      setNewField((prev) => ({
                        ...prev,
                        fieldType: value as any,
                      }))
                    }
                    placeholder="Chọn loại trường"
                  >
                    {FIELD_TYPES.map((type) => (
                      <Select.Option
                        key={type.value}
                        value={type.value}
                        title={type.label}
                      >
                        {type.label}
                      </Select.Option>
                    ))}
                  </Select>
                  <button
                    onClick={addCustomField}
                    className="w-full py-3 px-6 rounded-lg font-semibold text-base transition-all duration-200 flex items-center justify-center gap-2 bg-white text-black hover:bg-gray-200 shadow-lg border border-gray-300"
                  >
                    <Plus className="w-4 h-4" />
                    Thêm trường
                  </button>
                </div>
              </div>

              {/* Custom fields list */}
              <div className="space-y-3">
                {customFields.map((field) => (
                  <div key={field.id}>
                    {editingField === field.id ? (
                      // Edit mode
                      <div className="bg-white border border-blue-200 rounded-xl p-4 space-y-3">
                        <div className="flex items-center gap-2 mb-3">
                          <Edit className="w-4 h-4 text-blue-600" />
                          <h5 className="font-medium text-gray-900">
                            Chỉnh sửa trường
                          </h5>
                        </div>
                        <Input
                          placeholder="Tên trường"
                          value={editFieldData.fieldName || ""}
                          onChange={(e) =>
                            setEditFieldData((prev) => ({
                              ...prev,
                              fieldName: e.target.value,
                            }))
                          }
                        />
                        <Input
                          placeholder="Giá trị trường"
                          value={editFieldData.fieldValue || ""}
                          onChange={(e) =>
                            setEditFieldData((prev) => ({
                              ...prev,
                              fieldValue: e.target.value,
                            }))
                          }
                        />
                        <Select
                          value={editFieldData.fieldType || "text"}
                          onChange={(value) =>
                            setEditFieldData((prev) => ({
                              ...prev,
                              fieldType: value as any,
                            }))
                          }
                          placeholder="Chọn loại trường"
                        >
                          {FIELD_TYPES.map((type) => (
                            <Select.Option
                              key={type.value}
                              value={type.value}
                              title={type.label}
                            >
                              {type.label}
                            </Select.Option>
                          ))}
                        </Select>
                        <div className="flex gap-2">
                          <button
                            onClick={saveEditedField}
                            className="flex-1 py-2 px-4 bg-blue-600 text-white rounded-lg font-medium hover:bg-blue-700 transition-colors flex items-center justify-center gap-2"
                          >
                            <Check className="w-4 h-4" />
                            Lưu
                          </button>
                          <button
                            onClick={cancelEditingField}
                            className="flex-1 py-2 px-4 bg-gray-200 text-gray-700 rounded-lg font-medium hover:bg-gray-300 transition-colors flex items-center justify-center gap-2"
                          >
                            <X className="w-4 h-4" />
                            Hủy
                          </button>
                        </div>
                      </div>
                    ) : (
                      // View mode
                      <div className="flex items-center justify-between p-3 bg-white border border-gray-100 rounded-xl">
                        <div className="flex items-center gap-3 flex-1 min-w-0">
                          <div className="w-8 h-8 bg-green-100 rounded-full flex items-center justify-center flex-shrink-0">
                            {(() => {
                              const fieldType = FIELD_TYPES.find(
                                (t) => t.value === field.fieldType
                              );
                              const IconComponent = fieldType?.icon || Type;
                              return (
                                <IconComponent className="w-4 h-4 text-green-600" />
                              );
                            })()}
                          </div>
                          <div className="flex-1 min-w-0">
                            <p className="font-medium text-gray-900 truncate">
                              {field.fieldName}
                            </p>
                            <p className="text-xs text-gray-500 truncate">
                              {field.fieldValue}
                            </p>
                          </div>
                        </div>
                        <div className="flex-shrink-0 ml-2 flex gap-1">
                          <button
                            onClick={() => startEditingField(field)}
                            className="p-2 text-blue-500 hover:bg-blue-50 rounded-full transition-colors"
                          >
                            <Edit className="w-4 h-4" />
                          </button>
                          <button
                            onClick={() => deleteCustomField(field.id)}
                            className="p-2 text-red-500 hover:bg-red-50 rounded-full transition-colors"
                          >
                            <Trash2 className="w-4 h-4" />
                          </button>
                        </div>
                      </div>
                    )}
                  </div>
                ))}

                {customFields.length === 0 && (
                  <div className="text-center py-8 text-gray-500">
                    <Plus className="w-8 h-8 mx-auto mb-2 opacity-50" />
                    <p>Chưa có trường tùy chỉnh nào</p>
                  </div>
                )}
              </div>
            </div>
          )}

          {/* Design Tab */}
          {activeTab === "design" && (
            <div className="space-y-6">
              <h3 className="text-lg font-medium text-gray-900">
                Thiết kế profile
              </h3>

              {/* Custom Description */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Mô tả tùy chỉnh
                </label>
                <Input
                  placeholder="Viết mô tả về bản thân..."
                  value={customDescription}
                  onChange={(e) => setCustomDescription(e.target.value)}
                />
              </div>

              {/* Theme Color */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-3">
                  Màu chủ đề
                </label>
                <div className="bg-white rounded-xl p-4 border border-gray-100">
                  <div className="grid grid-cols-7 gap-3">
                    {THEME_COLORS.map((color) => (
                      <button
                        key={color}
                        onClick={() => setThemeColor(color)}
                        className={`relative w-10 h-10 rounded-full border-2 transition-all hover:scale-110 ${
                          themeColor === color
                            ? "border-gray-900 scale-110 shadow-lg"
                            : "border-gray-200 hover:border-gray-400"
                        }`}
                        style={{ backgroundColor: color }}
                      >
                        {themeColor === color && (
                          <div className="absolute inset-0 flex items-center justify-center">
                            <Check className="w-5 h-5 text-white drop-shadow-md" />
                          </div>
                        )}
                      </button>
                    ))}
                  </div>

                  {/* Current Color Info */}
                  <div className="mt-4 pt-4 border-t border-gray-100">
                    <div className="flex items-center gap-3">
                      <div
                        className="w-12 h-12 rounded-lg border-2 border-gray-200 shadow-sm"
                        style={{ backgroundColor: themeColor }}
                      />
                      <div>
                        <p className="text-xs text-gray-500">Màu đang chọn</p>
                        <p className="text-sm font-mono font-medium text-gray-900">
                          {themeColor}
                        </p>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          )}
        </div>
      </div>

      {/* Save Button */}
      <div className="fixed bottom-0 left-0 right-0 px-4 py-2 bg-black border-t border-gray-800 z-50">
        <button
          onClick={saveProfileTemplate}
          disabled={saving}
          className={`w-full mb-2 py-3 px-6 rounded-lg font-semibold text-base transition-all duration-200 flex items-center justify-center gap-2 ${
            saving
              ? "bg-gray-700 text-gray-400 cursor-not-allowed"
              : "bg-white text-black hover:bg-gray-200 shadow-lg"
          }`}
        >
          {saving ? (
            <>
              <RefreshCw className="w-4 h-4 animate-spin" />
              Đang lưu...
            </>
          ) : (
            <>
              <Save className="w-4 h-4" />
              Lưu cấu hình
            </>
          )}
        </button>
      </div>
    </Page>
  );
};

export default ProfileIntroGiba;
