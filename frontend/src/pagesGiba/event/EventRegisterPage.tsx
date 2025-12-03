import React, { useState, useEffect } from "react";
import { Page } from "zmp-ui";
import {
  Input,
  Button,
  InputNumber,
  DatePicker,
  Radio,
  Checkbox,
  Select,
} from "antd";
import dayjs from "dayjs";
import { useNavigate, useLocation } from "react-router-dom";
import { toast } from "react-toastify";
import { useRecoilValue } from "recoil";
import { token, userMembershipInfo } from "../../recoil/RecoilState";
import axios from "axios";
import dfData from "../../common/DefaultConfig.json";
import { CustomFieldType } from "../../utils/enum/event.enum";
import { openWebview } from "zmp-sdk/apis";
import useSetHeader from "../../components/hooks/useSetHeader";

interface CustomField {
  id: string;
  eventId: string;
  fieldName: string;
  fieldValue: string;
  fieldType: number;
  fieldTypeText: string;
  isRequired: boolean;
}

const EventRegisterPage: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const setHeader = useSetHeader();
  const eventId = location.state?.eventId || "";
  const userToken = useRecoilValue(token);
  const membershipInfo = useRecoilValue(userMembershipInfo);
  const [loading, setLoading] = useState(false);
  const [formData, setFormData] = useState({
    name: "",
    phoneNumber: "",
    email: "",
  });
  const [customFields, setCustomFields] = useState<CustomField[]>([]);
  const [customFieldValues, setCustomFieldValues] = useState<
    Record<string, any>
  >({});

  useEffect(() => {
    setHeader({
      title: "ĐĂNG KÝ SỰ KIỆN",
      hasLeftIcon: true,
    });
  }, [setHeader]);

  useEffect(() => {
    if (membershipInfo) {
      setFormData({
        name: membershipInfo.fullname || "",
        phoneNumber: membershipInfo.phoneNumber || "",
        email: "",
      });
    }
  }, [membershipInfo]);

  useEffect(() => {
    if (eventId && userToken) {
      fetchCustomFields();
    } else if (!eventId) {
      toast.error("Không tìm thấy thông tin sự kiện");
      navigate(-1);
    }
  }, [eventId, userToken]);

  const fetchCustomFields = async () => {
    try {
      const response = await axios.get(
        `${dfData.domain}/Event/GetCustomFields/${eventId}`,
        {
          headers: {
            Authorization: `Bearer ${userToken}`,
          },
        }
      );
      if (response.data.success) {
        setCustomFields(response.data.data || []);
      }
    } catch (error) {
      console.error("Error fetching custom fields:", error);
      // Không hiển thị lỗi vì có thể sự kiện không có custom fields
    }
  };

  const isValidEmail = (email: string) => {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  };

  const handleSubmit = async () => {
    if (!formData.name.trim()) {
      toast.error("Vui lòng nhập họ tên");
      return;
    }
    if (!formData.phoneNumber.trim()) {
      toast.error("Vui lòng nhập số điện thoại");
      return;
    }
    if (formData.email.trim() && !isValidEmail(formData.email.trim())) {
      toast.error("Vui lòng nhập email hợp lệ");
      return;
    }

    for (const field of customFields) {
      if (field.isRequired) {
        const val = customFieldValues[field.id];
        if (
          val === undefined ||
          val === null ||
          val === "" ||
          (Array.isArray(val) && val.length === 0)
        ) {
          toast.error(`Vui lòng nhập ${field.fieldName}`);
          return;
        }
      }
    }

    let loadingToastId: any = null;
    try {
      setLoading(true);
      loadingToastId = toast.loading("Đang đăng ký sự kiện...");

      const payload = {
        name: formData.name.trim(),
        phoneNumber: formData.phoneNumber.trim(),
        email: formData.email.trim() || undefined,
        customFields: Object.entries(customFieldValues).map(([key, value]) => ({
          eventCustomFieldId: key,
          fieldValue: Array.isArray(value) ? value.join(", ") : String(value),
        })),
      };

      const response = await axios.post(
        `${dfData.domain}/api/EventRegistrations/Register/${eventId}`,
        payload,
        {
          headers: {
            Authorization: `Bearer ${userToken}`,
          },
        }
      );

      if (loadingToastId) {
        toast.dismiss(loadingToastId);
      }

      if (response) {
        const responseData = response.data;
        const registrationStatus = responseData?.data?.status;

        let successMessage =
          responseData?.message || "Đăng ký sự kiện thành công!";

        if (registrationStatus === 0) {
          successMessage = "Đăng ký thành công! Vui lòng đợi admin duyệt đơn.";
        }

        toast.success(successMessage);
        setTimeout(() => {
          navigate("/event/success-join", {
            state: {
              status: registrationStatus,
            },
            replace: true,
          });
        }, 200);
      } else {
        toast.error(
          (response as any).message || "Có lỗi xảy ra khi đăng ký sự kiện"
        );
      }
    } catch (error) {
      console.error("Error registering event:", error);
      if (loadingToastId) {
        toast.dismiss(loadingToastId);
      }
      let errorMessage = "Có lỗi xảy ra khi đăng ký sự kiện. Vui lòng thử lại!";
      if ((error as any).response?.data?.message) {
        errorMessage = (error as any).response.data.message;
      } else if ((error as any).message) {
        errorMessage = (error as any).message;
      }
      toast.error(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  const renderCustomField = (field: CustomField) => {
    const handleChange = (val: any) => {
      setCustomFieldValues((prev) => ({
        ...prev,
        [field.id]: val,
      }));
    };

    const options = field.fieldValue
      ? field.fieldValue
          .split(",")
          .map((opt) => opt.trim())
          .filter((opt) => opt)
      : [];

    switch (field.fieldType) {
      case CustomFieldType.Text:
      case CustomFieldType.Email:
        return (
          <Input
            placeholder={`Nhập ${field.fieldName}`}
            value={customFieldValues[field.id]}
            onChange={(e) => handleChange(e.target.value)}
            className="h-10"
          />
        );
      case CustomFieldType.Url:
        return (
          <div>
            {field.fieldValue && (
              <a
                className="text-blue-600 hover:text-blue-800 underline cursor-pointer text-sm mb-2 inline-block"
                onClick={(e) => {
                  e.preventDefault();
                  openWebview({
                    url: field.fieldValue,
                    success: () => {
                      console.log("Opened webview successfully");
                    },
                    fail: (error) => {
                      console.error("Failed to open webview:", error);
                      toast.error("Không thể mở link");
                    },
                  });
                }}
              >
                {field.fieldValue}
              </a>
            )}
            <Input
              placeholder={`Nhập ${field.fieldName}`}
              value={customFieldValues[field.id]}
              onChange={(e) => handleChange(e.target.value)}
              className="h-10"
            />
          </div>
        );
      case CustomFieldType.Integer:
        return (
          <InputNumber
            style={{ width: "100%" }}
            placeholder={`Nhập ${field.fieldName}`}
            value={customFieldValues[field.id]}
            onChange={handleChange}
            precision={0}
            className="h-10 flex items-center"
          />
        );
      case CustomFieldType.Decimal:
        return (
          <InputNumber
            style={{ width: "100%" }}
            placeholder={`Nhập ${field.fieldName}`}
            value={customFieldValues[field.id]}
            onChange={handleChange}
            className="h-10 flex items-center"
          />
        );
      case CustomFieldType.YearOfBirth:
        return (
          <InputNumber
            style={{ width: "100%" }}
            placeholder={`Nhập ${field.fieldName}`}
            value={customFieldValues[field.id]}
            onChange={handleChange}
            precision={0}
            className="h-10 flex items-center"
          />
        );
      case CustomFieldType.Boolean:
        return (
          <Radio.Group
            value={customFieldValues[field.id]}
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
            style={{ width: "100%" }}
            placeholder={`Chọn ${field.fieldName}`}
            value={
              customFieldValues[field.id]
                ? dayjs(customFieldValues[field.id])
                : null
            }
            onChange={(date) => handleChange(date ? date.toISOString() : null)}
            className="h-10"
          />
        );
      case CustomFieldType.Date:
        return (
          <DatePicker
            style={{ width: "100%" }}
            placeholder={`Chọn ${field.fieldName}`}
            value={
              customFieldValues[field.id]
                ? dayjs(customFieldValues[field.id])
                : null
            }
            onChange={(date) => handleChange(date ? date.toISOString() : null)}
            className="h-10"
          />
        );
      case CustomFieldType.PhoneNumber:
        return (
          <Input
            type="tel"
            placeholder={`Nhập ${field.fieldName}`}
            value={customFieldValues[field.id]}
            onChange={(e) => handleChange(e.target.value)}
            className="h-10"
          />
        );
      case CustomFieldType.LongText:
        return (
          <Input.TextArea
            rows={3}
            placeholder={`Nhập ${field.fieldName}`}
            value={customFieldValues[field.id]}
            onChange={(e) => handleChange(e.target.value)}
          />
        );
      case CustomFieldType.Dropdown:
        return (
          <Select
            style={{ width: "100%" }}
            placeholder={`Chọn ${field.fieldName}`}
            value={customFieldValues[field.id]}
            onChange={handleChange}
            className="h-10"
            options={options.map((opt) => ({
              label: opt,
              value: opt,
            }))}
          />
        );
      case CustomFieldType.MultipleChoice:
        return (
          <div className="space-y-2">
            {options.map((opt) => (
              <div
                key={opt}
                className="flex items-center p-3 border border-gray-200 rounded-lg hover:border-blue-400 hover:bg-blue-50 transition-colors cursor-pointer"
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
                  <span className="text-sm text-gray-700">{opt}</span>
                </Checkbox>
              </div>
            ))}
          </div>
        );
      default:
        return (
          <Input
            placeholder={`Nhập ${field.fieldName}`}
            value={customFieldValues[field.id]}
            onChange={(e) => handleChange(e.target.value)}
            className="h-10"
          />
        );
    }
  };

  return (
    <Page
      className="bg-white min-h-screen"
      style={{ paddingTop: "50px", paddingBottom: "80px" }}
    >
      <div className="p-5">
        <div className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Họ và tên <span style={{ color: "#ef4444" }}>*</span>
            </label>
            <Input
              value={formData.name}
              onChange={(e) =>
                setFormData({ ...formData, name: e.target.value })
              }
              placeholder="Nhập họ và tên"
              className="h-10"
              readOnly={!!membershipInfo?.fullname}
              style={{
                backgroundColor: membershipInfo?.fullname ? "#f5f5f5" : "white",
                cursor: membershipInfo?.fullname ? "not-allowed" : "text",
              }}
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Số điện thoại <span style={{ color: "#ef4444" }}>*</span>
            </label>
            <Input
              value={formData.phoneNumber}
              onChange={(e) =>
                setFormData({ ...formData, phoneNumber: e.target.value })
              }
              placeholder="Nhập số điện thoại"
              className="h-10"
              readOnly={!!membershipInfo?.phoneNumber}
              style={{
                backgroundColor: membershipInfo?.phoneNumber
                  ? "#f5f5f5"
                  : "white",
                cursor: membershipInfo?.phoneNumber ? "not-allowed" : "text",
              }}
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Email
            </label>
            <Input
              type="email"
              value={formData.email}
              onChange={(e) =>
                setFormData({ ...formData, email: e.target.value })
              }
              placeholder="Nhập email hợp lệ (ví dụ: example@gmail.com)"
              className="h-10"
            />
          </div>

          {customFields.map((field) => (
            <div key={field.id}>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                {field.fieldName}{" "}
                {field.isRequired && (
                  <span style={{ color: "#ef4444" }}>*</span>
                )}
              </label>
              {renderCustomField(field)}
            </div>
          ))}
        </div>
      </div>

      <div className="fixed bottom-0 left-0 right-0 bg-white border-t border-gray-200 p-4">
        <Button
          type="primary"
          onClick={handleSubmit}
          loading={loading}
          className="w-full bg-black hover:bg-gray-800 h-12 text-base font-medium"
          size="large"
        >
          {loading ? "Đang đăng ký..." : "Đăng ký"}
        </Button>
      </div>
    </Page>
  );
};

export default EventRegisterPage;
