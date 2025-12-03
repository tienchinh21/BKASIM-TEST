import React, { useState, useEffect, useCallback, useRef, memo } from "react";
import { Page, Box } from "zmp-ui";
import { useNavigate, useLocation } from "react-router-dom";
import { useRecoilValue, useSetRecoilState } from "recoil";
import { infoUser, token, userMembershipInfo } from "../recoil/RecoilState";
import useSetHeader from "../components/hooks/useSetHeader";
import axios from "axios";
import { toast } from "react-toastify";
import dfData from "../common/DefaultConfig.json";
import LoadingGiba from "../componentsGiba/LoadingGiba";
import GroupRulesDrawer from "../componentsGiba/GroupRulesDrawer";
import { CustomFieldType } from "../utils/enum/event.enum";
import { Input, InputNumber, DatePicker, Radio, Checkbox, Select } from "antd";
import dayjs from "dayjs";

// Tab Navigation Component
interface TabItem {
  id: string;
  name: string;
}

interface TabNavigationProps {
  tabs: TabItem[];
  activeTabId: string;
  onTabChange: (tabId: string) => void;
}

const TabNavigation = memo<TabNavigationProps>(
  ({ tabs, activeTabId, onTabChange }) => {
    const itemRefs = useRef<Record<string, HTMLButtonElement | null>>({});

    useEffect(() => {
      if (activeTabId && itemRefs.current[activeTabId]) {
        itemRefs.current[activeTabId]?.scrollIntoView({
          behavior: "smooth",
          inline: "center",
          block: "nearest",
        });
      }
    }, [activeTabId]);

    return (
      <Box
        style={{
          display: "flex",
          gap: "8px",
          marginBottom: "16px",
          borderBottom: "1px solid #e5e7eb",
          overflowX: "auto",
          scrollBehavior: "smooth",
          WebkitOverflowScrolling: "touch",
          msOverflowStyle: "none",
          scrollbarWidth: "none",
        }}
        className="hidden-scrollbar-y"
      >
        {tabs.map((tab) => (
          <button
            key={tab.id}
            ref={(el) => (itemRefs.current[tab.id] = el)}
            onClick={() => onTabChange(tab.id)}
            style={{
              padding: "10px 16px",
              border: "none",
              background: "transparent",
              cursor: "pointer",
              fontSize: "14px",
              fontWeight: activeTabId === tab.id ? "600" : "400",
              color: activeTabId === tab.id ? "#000" : "#6b7280",
              borderBottom:
                activeTabId === tab.id ? "3px solid #003d82" : "none",
              transition: "all 0.3s ease",
              whiteSpace: "nowrap",
              flexShrink: 0,
            }}
          >
            {tab.name}
          </button>
        ))}
      </Box>
    );
  }
);

// Type definitions
interface CustomField {
  id: string;
  customFieldTabId: string;
  entityId: string;
  fieldName: string;
  fieldType: number;
  fieldTypeText: string;
  fieldOptions: string[] | null;
  isRequired: boolean;
  displayOrder: number;
  createdDate: string;
  updatedDate: string;
}

interface CustomFieldTab {
  id: string;
  tabName: string;
  displayOrder: number;
  fields: CustomField[];
}

interface CustomFieldsResponse {
  success: boolean;
  message: string;
  data: {
    groupId: string;
    groupName: string;
    tabs: CustomFieldTab[];
  };
}

interface RegistrationFormData {
  reason: string;
}

interface CustomFieldValue {
  eventCustomFieldId: string;
  fieldValue: string;
}

interface RegistrationRequest {
  reason: string;
  values: Record<string, string>;
}

const GroupRegisterPage: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const setHeader = useSetHeader();
  const userInfo = useRecoilValue(infoUser);
  const userToken = useRecoilValue(token);
  const setMembershipInfo = useSetRecoilState(userMembershipInfo);

  // Extract groupId and groupName from navigation state
  const groupId = (location.state as any)?.groupId;
  const groupName = (location.state as any)?.groupName;

  // State management
  const [customFieldTabs, setCustomFieldTabs] = useState<CustomFieldTab[]>([]);
  const [formData, setFormData] = useState<RegistrationFormData>({
    reason: "",
  });
  const [customFieldValues, setCustomFieldValues] = useState<
    Record<string, any>
  >({});
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [activeTabId, setActiveTabId] = useState<string>("");
  const [isAgreedRules, setIsAgreedRules] = useState(false);
  const [rulesDrawerVisible, setRulesDrawerVisible] = useState(false);

  // Set header
  useEffect(() => {
    setHeader({
      title: groupName || "Đăng ký thành viên",
      showUserInfo: false,
      showMenuButton: false,
      showCloseButton: false,
      hasLeftIcon: true,
    } as any);
  }, [setHeader, groupName]);

  // Fetch custom fields
  const fetchCustomFields = useCallback(async () => {
    if (!groupId) {
      toast.error("Không tìm thấy ID nhóm");
      navigate(-1);
      return;
    }

    setLoading(true);
    try {
      const response = await axios.get<CustomFieldsResponse>(
        `${dfData.domain}/api/groups/${groupId}/custom-fields`,
        {
          headers: { Authorization: `Bearer ${userToken}` },
        }
      );

      if (response.data.success && response.data.data) {
        const tabs = response.data.data.tabs || [];
        setCustomFieldTabs(tabs);

        // Set first tab as active
        if (tabs.length > 0) {
          setActiveTabId(tabs[0].id);
        }
      } else {
        toast.error(
          response.data.message || "Không thể tải biểu mẫu. Vui lòng thử lại!"
        );
      }
    } catch (error) {
      console.error("Error fetching custom fields:", error);
      toast.error("Không thể tải biểu mẫu. Vui lòng thử lại!");
    } finally {
      setLoading(false);
    }
  }, [groupId, userToken, navigate]);

  useEffect(() => {
    fetchCustomFields();
  }, [fetchCustomFields]);

  // Handle basic field changes
  const handleInputChange = useCallback(
    (field: keyof RegistrationFormData, value: string) => {
      setFormData((prev) => ({
        ...prev,
        [field]: value,
      }));
    },
    []
  );

  // Handle custom field changes
  const handleCustomFieldChange = useCallback((fieldId: string, value: any) => {
    setCustomFieldValues((prev) => ({
      ...prev,
      [fieldId]: value,
    }));
  }, []);

  // Validate form
  const validateForm = useCallback((): boolean => {
    if (!formData.reason.trim()) {
      toast.error("Vui lòng nhập lý do tham gia");
      return false;
    }

    if (!isAgreedRules) {
      toast.error("Vui lòng đồng ý với quy tắc ứng xử của nhóm");
      return false;
    }

    // Check custom required fields
    for (const tab of customFieldTabs) {
      for (const field of tab.fields) {
        if (field.isRequired) {
          const value = customFieldValues[field.id];
          if (value === undefined || value === null || value === "") {
            toast.error(`Vui lòng nhập ${field.fieldName}`);
            return false;
          }
        }
      }
    }

    return true;
  }, [formData, customFieldValues, customFieldTabs, isAgreedRules]);

  // Construct registration payload
  const constructPayload = useCallback((): RegistrationRequest => {
    const values: Record<string, string> = {};

    // Add custom field values
    Object.entries(customFieldValues).forEach(([fieldId, value]) => {
      values[fieldId] = String(value || "");
    });

    return {
      reason: formData.reason,
      values,
    };
  }, [formData, customFieldValues]);

  // Handle form submission
  const handleSubmit = useCallback(async () => {
    if (!validateForm()) {
      return;
    }

    setSubmitting(true);
    try {
      const payload = constructPayload();

      const response = await axios.post(
        `${dfData.domain}/api/groups/${groupId}/register-with-custom-fields`,
        payload,
        {
          headers: { Authorization: `Bearer ${userToken}` },
        }
      );

      if (response.data.success) {
        toast.success("Đăng ký thành công!");

        // Call login API to refresh user state with approved status
        try {
          const loginResponse = await axios.get(
            `${dfData.domain}/api/auth/me`,
            {
              headers: { Authorization: `Bearer ${userToken}` },
            }
          );

          if (loginResponse.data.code === 0 && loginResponse.data.data) {
            const userData = loginResponse.data.data;
            // Update membership info with approved status
            setMembershipInfo({
              id: userData.id || "",
              userZaloId: userData.userZaloId || "",
              phoneNumber: userData.phoneNumber || "",
              fullname: userData.fullname || "",
              approvalStatus: 1 as any, // Set to approved
              idByOA: userData.idByOA || "",
            });
          }
        } catch (loginError) {
          console.error("Error refreshing user state:", loginError);
          // Continue anyway, user can refresh manually
        }

        // Navigate back after a short delay
        setTimeout(() => {
          navigate(-1);
        }, 1000);
      } else {
        toast.error(
          response.data.message || "Đăng ký thất bại. Vui lòng thử lại!"
        );
      }
    } catch (error) {
      console.error("Error submitting registration:", error);
      toast.error("Đăng ký thất bại. Vui lòng thử lại!");
    } finally {
      setSubmitting(false);
    }
  }, [
    validateForm,
    constructPayload,
    groupId,
    userToken,
    navigate,
    setMembershipInfo,
  ]);

  // Render custom field based on type
  const renderCustomField = (field: CustomField): React.ReactNode => {
    const handleChange = (val: any) => {
      handleCustomFieldChange(field.id, val);
    };

    const options = Array.isArray(field.fieldOptions) ? field.fieldOptions : [];

    switch (field.fieldType) {
      case CustomFieldType.Text:
      case CustomFieldType.Email:
        return (
          <Input
            placeholder={`Nhập ${field.fieldName}`}
            value={customFieldValues[field.id] || ""}
            onChange={(e) => handleChange(e.target.value)}
            style={{ height: "40px" }}
          />
        );
      case CustomFieldType.PhoneNumber:
        return (
          <Input
            type="tel"
            placeholder={`Nhập ${field.fieldName}`}
            value={customFieldValues[field.id] || ""}
            onChange={(e) => handleChange(e.target.value)}
            style={{ height: "40px" }}
          />
        );
      case CustomFieldType.LongText:
        return (
          <Input.TextArea
            rows={4}
            placeholder={`Nhập ${field.fieldName}`}
            value={customFieldValues[field.id] || ""}
            onChange={(e) => handleChange(e.target.value)}
          />
        );
      case CustomFieldType.Integer:
        return (
          <InputNumber
            style={{ width: "100%", height: "40px" }}
            placeholder={`Nhập ${field.fieldName}`}
            value={customFieldValues[field.id] || null}
            onChange={handleChange}
            precision={0}
          />
        );
      case CustomFieldType.Decimal:
        return (
          <InputNumber
            style={{ width: "100%", height: "40px" }}
            placeholder={`Nhập ${field.fieldName}`}
            value={customFieldValues[field.id] || null}
            onChange={handleChange}
          />
        );
      case CustomFieldType.YearOfBirth:
        return (
          <InputNumber
            style={{ width: "100%", height: "40px" }}
            placeholder={`Nhập ${field.fieldName}`}
            value={customFieldValues[field.id] || null}
            onChange={handleChange}
            precision={0}
          />
        );
      case CustomFieldType.Boolean:
        return (
          <Radio.Group
            value={customFieldValues[field.id] || ""}
            onChange={(e) => handleChange(e.target.value)}
          >
            <Radio value="true">Có</Radio>
            <Radio value="false">Không</Radio>
          </Radio.Group>
        );
      case CustomFieldType.DateTime:
        return (
          <DatePicker
            showTime
            style={{ width: "100%", height: "40px" }}
            placeholder={`Chọn ${field.fieldName}`}
            value={
              customFieldValues[field.id]
                ? dayjs(customFieldValues[field.id])
                : null
            }
            onChange={(date) => handleChange(date ? date.toISOString() : null)}
          />
        );
      case CustomFieldType.Date:
        return (
          <DatePicker
            style={{ width: "100%", height: "40px" }}
            placeholder={`Chọn ${field.fieldName}`}
            value={
              customFieldValues[field.id]
                ? dayjs(customFieldValues[field.id])
                : null
            }
            onChange={(date) => handleChange(date ? date.toISOString() : null)}
          />
        );
      case CustomFieldType.Dropdown:
        return (
          <Select
            style={{ width: "100%", height: "40px" }}
            placeholder={`Chọn ${field.fieldName}`}
            value={customFieldValues[field.id] || undefined}
            onChange={handleChange}
            options={options.map((opt) => ({
              label: opt,
              value: opt,
            }))}
          />
        );
      case CustomFieldType.MultipleChoice:
        return (
          <div style={{ display: "flex", flexDirection: "column", gap: "8px" }}>
            {options.map((opt) => (
              <div
                key={opt}
                style={{
                  display: "flex",
                  alignItems: "center",
                  padding: "12px",
                  border: "1px solid #e5e7eb",
                  borderRadius: "8px",
                  cursor: "pointer",
                  transition: "all 0.2s",
                }}
                onClick={() => {
                  const currentValues = customFieldValues[field.id] || [];
                  const newValues = currentValues.includes(opt)
                    ? currentValues.filter((v: string) => v !== opt)
                    : [...currentValues, opt];
                  handleChange(newValues);
                }}
              >
                <Checkbox
                  checked={(customFieldValues[field.id] || []).includes(opt)}
                  onChange={(e) => {
                    const currentValues = customFieldValues[field.id] || [];
                    const newValues = e.target.checked
                      ? [...currentValues, opt]
                      : currentValues.filter((v: string) => v !== opt);
                    handleChange(newValues);
                  }}
                  onClick={(e) => e.stopPropagation()}
                >
                  <span style={{ fontSize: "14px", color: "#374151" }}>
                    {opt}
                  </span>
                </Checkbox>
              </div>
            ))}
          </div>
        );
      case CustomFieldType.Url:
        return (
          <Input
            placeholder={`Nhập ${field.fieldName}`}
            value={customFieldValues[field.id] || ""}
            onChange={(e) => handleChange(e.target.value)}
            style={{ height: "40px" }}
          />
        );
      default:
        return (
          <Input
            placeholder={`Nhập ${field.fieldName}`}
            value={customFieldValues[field.id] || ""}
            onChange={(e) => handleChange(e.target.value)}
            style={{ height: "40px" }}
          />
        );
    }
  };

  if (loading) {
    return (
      <Page className="bg-white min-h-screen mt-[50px]">
        <div className="flex justify-center items-center h-64">
          <LoadingGiba size="lg" text="Đang tải biểu mẫu..." />
        </div>
      </Page>
    );
  }

  return (
    <Page style={{ paddingTop: 50, overflowY: "auto", height: "100vh" }}>
      <Box
        style={{ padding: "16px", paddingBottom: "100px", background: "#fff" }}
      >
        {/* Reason Field Section */}
        <Box style={{ marginBottom: "24px" }}>
          <Box style={{ marginBottom: "12px" }}>
            <label
              style={{
                display: "block",
                fontSize: "14px",
                fontWeight: "500",
                marginBottom: "6px",
                color: "#000",
              }}
            >
              Lý do tham gia <span style={{ color: "#ef4444" }}>*</span>
            </label>
            <textarea
              value={formData.reason}
              onChange={(e) => handleInputChange("reason", e.target.value)}
              placeholder="Nhập lý do tham gia nhóm"
              rows={4}
              style={{
                width: "100%",
                padding: "10px 12px",
                border: "1px solid #e5e7eb",
                borderRadius: "8px",
                fontSize: "14px",
                fontFamily: "inherit",
                boxSizing: "border-box",
              }}
            />
          </Box>
        </Box>

        {/* Group Rules Section */}

        {/* Custom Fields Section */}
        {customFieldTabs.length > 0 && (
          <Box style={{ marginBottom: "24px" }}>
            {/* Tab Navigation */}
            {customFieldTabs.length > 1 && (
              <TabNavigation
                tabs={customFieldTabs.map((tab) => ({
                  id: tab.id,
                  name: tab.tabName,
                }))}
                activeTabId={activeTabId}
                onTabChange={setActiveTabId}
              />
            )}

            {/* Tab Content */}
            {customFieldTabs.map((tab) => {
              if (tab.id !== activeTabId) return null;

              return (
                <Box key={tab.id}>
                  {tab.fields.map((field) => (
                    <Box key={field.id} style={{ marginBottom: "12px" }}>
                      <label
                        style={{
                          display: "block",
                          fontSize: "14px",
                          fontWeight: "500",
                          marginBottom: "6px",
                          color: "#000",
                        }}
                      >
                        {field.fieldName}
                        {field.isRequired && (
                          <span style={{ color: "#ef4444" }}> *</span>
                        )}
                      </label>
                      {renderCustomField(field)}
                    </Box>
                  ))}
                </Box>
              );
            })}
          </Box>
        )}
        <Box style={{ marginBottom: "24px" }}>
          <div
            style={{
              background: "#f3f4f6",
              border: "1px solid #e5e7eb",
              padding: "12px",
              borderRadius: "8px",
              marginBottom: "16px",
            }}
          >
            <div
              style={{
                display: "flex",
                alignItems: "center",
                justifyContent: "space-between",
              }}
            >
              <div
                style={{ display: "flex", alignItems: "center", gap: "12px" }}
              >
                <div
                  style={{
                    width: "40px",
                    height: "40px",
                    background: "#dbeafe",
                    borderRadius: "8px",
                    display: "flex",
                    alignItems: "center",
                    justifyContent: "center",
                  }}
                >
                  <svg
                    style={{ width: "20px", height: "20px", color: "#2563eb" }}
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      strokeWidth={2}
                      d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
                    />
                  </svg>
                </div>
                <div>
                  <p
                    style={{
                      fontSize: "14px",
                      fontWeight: "600",
                      color: "#111827",
                      marginBottom: "2px",
                    }}
                  >
                    Quy tắc ứng xử của nhóm
                  </p>
                  <p style={{ fontSize: "12px", color: "#6b7280" }}>
                    Xem tài liệu quy định
                  </p>
                </div>
                <button
                  onClick={() => setRulesDrawerVisible(true)}
                  style={{
                    padding: "8px 16px",
                    background: "#2563eb",
                    color: "#fff",
                    fontSize: "12px",
                    fontWeight: "600",
                    border: "none",
                    borderRadius: "6px",
                    cursor: "pointer",
                    transition: "all 0.3s",
                  }}
                  onMouseEnter={(e) => {
                    (e.target as HTMLButtonElement).style.background =
                      "#1d4ed8";
                  }}
                  onMouseLeave={(e) => {
                    (e.target as HTMLButtonElement).style.background =
                      "#2563eb";
                  }}
                >
                  Xem
                </button>
              </div>
            </div>
          </div>

          {/* Checkbox Agreement */}
          <div
            style={{
              background: "#fef3c7",
              border: "1px solid #fcd34d",
              padding: "12px",
              borderRadius: "8px",
            }}
          >
            <Checkbox
              checked={isAgreedRules}
              onChange={(e) => setIsAgreedRules(e.target.checked)}
              style={{ width: "100%" }}
            >
              <div style={{ flex: 1 }}>
                <p
                  style={{
                    fontSize: "14px",
                    color: "#374151",
                    lineHeight: "1.5",
                  }}
                >
                  Tôi đã đọc, hiểu và đồng ý tuân thủ{" "}
                  <span style={{ color: "#2563eb", fontWeight: "600" }}>
                    Quy tắc ứng xử của nhóm
                  </span>{" "}
                  <span style={{ color: "#ef4444", fontWeight: "bold" }}>
                    *
                  </span>
                </p>
              </div>
            </Checkbox>
          </div>

          {/* Approval Process Info */}
          <div
            style={{
              background: "#dbeafe",
              border: "1px solid #93c5fd",
              padding: "12px",
              borderRadius: "8px",
              marginTop: "12px",
            }}
          >
            <p
              style={{
                fontSize: "14px",
                color: "#1e40af",
                fontWeight: "600",
                marginBottom: "8px",
              }}
            >
              Quy trình xét duyệt:
            </p>
            <ul
              style={{
                fontSize: "12px",
                color: "#1e40af",
                margin: 0,
                paddingLeft: "20px",
                lineHeight: "1.6",
              }}
            >
              <li>Đơn xin tham gia sẽ được gửi đến quản trị viên nhóm</li>
              <li>Quản trị viên sẽ xem xét và phê duyệt trong 24-48h</li>
              <li>Bạn sẽ nhận được thông báo khi có kết quả</li>
              <li>Có thể kiểm tra trạng thái trong mục "Nhóm của tôi"</li>
            </ul>
          </div>
        </Box>
      </Box>

      {/* Fixed Submit Button */}
      <Box
        style={{
          position: "fixed",
          bottom: 0,
          left: 0,
          right: 0,
          display: "flex",
          gap: "12px",
          padding: "16px",
          background: "#fff",
          borderTop: "1px solid #e5e7eb",
          zIndex: 50,
        }}
      >
        <button
          onClick={() => navigate(-1)}
          disabled={submitting}
          style={{
            flex: 1,
            padding: "12px 16px",
            border: "1px solid #0066cc",
            background: "#fff",
            color: "#0066cc",
            borderRadius: "8px",
            fontSize: "14px",
            fontWeight: "600",
            cursor: submitting ? "not-allowed" : "pointer",
            opacity: submitting ? 0.6 : 1,
            transition: "all 0.3s",
          }}
        >
          Hủy
        </button>
        <button
          onClick={handleSubmit}
          disabled={submitting}
          style={{
            flex: 1,
            padding: "12px 16px",
            border: "none",
            background: submitting ? "#d1d5db" : "#0066cc",
            color: "#fff",
            borderRadius: "8px",
            fontSize: "14px",
            fontWeight: "600",
            cursor: submitting ? "not-allowed" : "pointer",
            opacity: submitting ? 0.6 : 1,
            transition: "all 0.3s",
          }}
        >
          {submitting ? "Đang gửi..." : "Đăng ký"}
        </button>
      </Box>

      {/* Group Rules Drawer */}
      <GroupRulesDrawer
        visible={rulesDrawerVisible}
        onClose={() => setRulesDrawerVisible(false)}
        groupId={groupId}
      />
    </Page>
  );
};

export default GroupRegisterPage;
