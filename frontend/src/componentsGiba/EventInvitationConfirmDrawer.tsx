import React, { useState, useEffect } from "react";
import {
  Drawer,
  Input,
  Button,
  Row,
  Col,
  InputNumber,
  DatePicker,
  Radio,
  Checkbox,
  Select,
} from "antd";
import dayjs from "dayjs";
import { toast } from "react-toastify";
import { useRecoilValue } from "recoil";
import { token } from "../recoil/RecoilState";
import axios from "axios";
import dfData from "../common/DefaultConfig.json";
import { CustomFieldType } from "../utils/enum/event.enum";

interface EventInvitationConfirmDrawerProps {
  visible: boolean;
  onClose: () => void;
  eventId: string;
  guestListId: string;
  onSuccess?: () => void;
}

interface CustomField {
  id: string;
  eventId: string;
  fieldName: string;
  fieldValue: string;
  fieldType: number;
  fieldTypeText: string;
  isRequired: boolean;
}

const EventInvitationConfirmDrawer: React.FC<
  EventInvitationConfirmDrawerProps
> = ({ visible, onClose, eventId, guestListId, onSuccess }) => {
  const userToken = useRecoilValue(token);
  const [loading, setLoading] = useState(false);
  const [customFields, setCustomFields] = useState<CustomField[]>([]);
  const [customFieldValues, setCustomFieldValues] = useState<
    Record<string, any>
  >({});
  const [isFetchingFields, setIsFetchingFields] = useState(false);

  useEffect(() => {
    if (visible && eventId) {
      fetchCustomFields();
    } else if (!visible) {
      // Reset state khi đóng drawer
      setCustomFields([]);
      setCustomFieldValues({});
      setIsFetchingFields(false);
    }
  }, [visible, eventId]);

  const fetchCustomFields = async () => {
    try {
      setIsFetchingFields(true);

      const headers: any = {};
      if (userToken) {
        headers.Authorization = `Bearer ${userToken}`;
      }

      const response = await axios.get(
        `${dfData.domain}/Event/GetCustomFields/${eventId}`,
        { headers }
      );

      if (response.data.success && response.data.data) {
        const fields = response.data.data;

        // Nếu có custom fields, hiển thị form
        if (fields.length > 0) {
          setCustomFields(fields);
        } else {
          // Không có custom fields, tự động confirm luôn
          await handleAutoConfirm();
        }
      } else {
        // Không có custom fields, confirm luôn
        await handleAutoConfirm();
      }
    } catch (error) {
      console.error("Error fetching custom fields:", error);
      // Nếu lỗi khi fetch, vẫn cho phép confirm
      await handleAutoConfirm();
    } finally {
      setIsFetchingFields(false);
    }
  };

  const handleAutoConfirm = async () => {
    try {
      setLoading(true);

      const headers: any = {};
      if (userToken) {
        headers.Authorization = `Bearer ${userToken}`;
      }

      const response = await axios.post(
        `${dfData.domain}/api/EventCustomField/Value/${guestListId}`,
        [],
        { headers }
      );

      if (response.data.success || response.data.code === 0) {
        toast.success("Xác nhận tham gia sự kiện thành công!");
        setLoading(false);
        onSuccess?.();
        onClose();
      } else {
        setLoading(false);
        toast.error(
          response.data.message || "Có lỗi xảy ra khi xác nhận tham gia"
        );
      }
    } catch (error: any) {
      console.error("Error confirming event:", error);
      setLoading(false);
      let errorMessage =
        "Có lỗi xảy ra khi xác nhận tham gia. Vui lòng thử lại!";
      if (error.response?.data?.message) {
        errorMessage = error.response.data.message;
      }
      toast.error(errorMessage);
    }
  };

  const handleSubmit = async () => {
    // Validate required fields
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
      loadingToastId = toast.loading("Đang xác nhận tham gia...");

      const payload = Object.entries(customFieldValues).map(([key, value]) => ({
        eventCustomFieldId: key,
        fieldValue: Array.isArray(value) ? value.join(", ") : String(value),
      }));

      const headers: any = {};
      if (userToken) {
        headers.Authorization = `Bearer ${userToken}`;
      }

      const response = await axios.post(
        `${dfData.domain}/api/EventCustomField/Value/${guestListId}`,
        payload,
        { headers }
      );

      if (loadingToastId) {
        toast.dismiss(loadingToastId);
      }

      if (response.data.success || response.data.code === 0) {
        toast.success("Xác nhận tham gia sự kiện thành công!");
        onSuccess?.();
        setCustomFieldValues({});
        onClose();
      } else {
        toast.error(
          response.data.message || "Có lỗi xảy ra khi xác nhận tham gia"
        );
      }
    } catch (error: any) {
      console.error("Error confirming event:", error);
      if (loadingToastId) {
        toast.dismiss(loadingToastId);
      }
      let errorMessage =
        "Có lỗi xảy ra khi xác nhận tham gia. Vui lòng thử lại!";
      if (error.response?.data?.message) {
        errorMessage = error.response.data.message;
      }
      toast.error(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div onClick={(e) => e.stopPropagation()}>
      <Drawer
        title="Xác nhận tham gia sự kiện"
        placement="bottom"
        height={500}
        open={visible}
        onClose={() => onClose()}
        style={{ borderRadius: "15px 15px 0 0" }}
        styles={{
          body: { padding: "0" },
        }}
        maskClosable={false}
        closable={true}
        closeIcon={
          <span
            onClick={(e) => {
              e.stopPropagation();
              onClose();
            }}
          >
            ×
          </span>
        }
      >
        <div className="flex flex-col h-full">
          <div className="flex-1 overflow-y-auto p-5 pb-20">
            {isFetchingFields ? (
              <div className="flex flex-col items-center justify-center py-12">
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-gray-900 mb-4"></div>
                <p className="text-gray-600">Đang tải thông tin...</p>
              </div>
            ) : customFields.length === 0 ? (
              <div className="flex flex-col items-center justify-center py-12">
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-gray-900 mb-4"></div>
                <p className="text-gray-600">Đang xác nhận tham gia...</p>
              </div>
            ) : (
              <div className="space-y-4">
                <div className="mb-4 p-3 bg-blue-50 border-l-4 border-blue-400 rounded">
                  <p className="text-sm text-blue-800">
                    Vui lòng điền thông tin bổ sung để xác nhận tham gia sự kiện
                  </p>
                </div>

                {customFields.map((field) => (
                  <div key={field.id}>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      {field.fieldName}{" "}
                      {field.isRequired && (
                        <span style={{ color: "#ef4444" }}>*</span>
                      )}
                    </label>
                    {(() => {
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
                        case CustomFieldType.Url:
                          return (
                            <Input
                              placeholder={`Nhập ${field.fieldName}`}
                              value={customFieldValues[field.id]}
                              onChange={(e) => handleChange(e.target.value)}
                              className="h-10"
                            />
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
                              onChange={(date) =>
                                handleChange(date ? date.toISOString() : null)
                              }
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
                              onChange={(date) =>
                                handleChange(date ? date.toISOString() : null)
                              }
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
                            <Checkbox.Group
                              value={customFieldValues[field.id] || []}
                              onChange={handleChange}
                              options={options.map((opt) => ({
                                label: opt,
                                value: opt,
                              }))}
                            />
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
                    })()}
                  </div>
                ))}
              </div>
            )}
          </div>

          {!isFetchingFields && customFields.length > 0 && (
            <div className="fixed bottom-0 left-0 right-0 bg-white border-t border-gray-200 p-4">
              <Row gutter={12}>
                <Col span={12}>
                  <Button
                    onClick={onClose}
                    className="w-full h-10 text-sm font-medium"
                    size="middle"
                  >
                    Hủy
                  </Button>
                </Col>
                <Col span={12}>
                  <Button
                    type="primary"
                    onClick={handleSubmit}
                    loading={loading}
                    className="w-full bg-black hover:bg-gray-800 h-10 text-sm font-medium"
                    size="middle"
                  >
                    {loading ? "Đang xác nhận..." : "Xác nhận"}
                  </Button>
                </Col>
              </Row>
            </div>
          )}
        </div>
      </Drawer>
    </div>
  );
};

export default EventInvitationConfirmDrawer;
