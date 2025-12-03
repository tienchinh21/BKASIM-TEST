import React, { useState } from "react";
import { Drawer, Input, Button, Row, Col } from "antd";
import { useNavigate } from "zmp-ui";
import { toast } from "react-toastify";
import { PlusOutlined, DeleteOutlined } from "@ant-design/icons";
import { useRecoilValue } from "recoil";
import { token } from "../recoil/RecoilState";
import axios from "axios";
import dfData from "../common/DefaultConfig.json";

interface Guest {
  guestName: string;
  guestPhone: string;
  guestEmail: string;
}

interface EventGuestRegisterDrawerProps {
  visible: boolean;
  onClose: () => void;
  eventId: string;
  onSuccess?: () => void;
}

const EventGuestRegisterDrawer: React.FC<EventGuestRegisterDrawerProps> = ({
  visible,
  onClose,
  eventId,
  onSuccess,
}) => {
  const navigate = useNavigate();
  const userToken = useRecoilValue(token);
  const [loading, setLoading] = useState(false);
  const [formData, setFormData] = useState({
    note: "",
    guestNumber: 1,
  });
  const [guestList, setGuestList] = useState<Guest[]>([
    { guestName: "", guestPhone: "", guestEmail: "" },
  ]);

  // Validate phone number (max 10 digits)
  const isValidPhone = (phone: string) => {
    const cleanPhone = phone.replace(/\s/g, "");
    return /^\d{10}$/.test(cleanPhone);
  };

  // Format phone number: replace leading 0 with 84
  const formatPhoneNumber = (phone: string) => {
    const cleanPhone = phone.replace(/\s/g, "");
    if (cleanPhone.startsWith("0")) {
      return "84" + cleanPhone.substring(1);
    }
    return cleanPhone;
  };

  const addGuest = () => {
    setGuestList([
      ...guestList,
      { guestName: "", guestPhone: "", guestEmail: "" },
    ]);
    setFormData({ ...formData, guestNumber: guestList.length + 1 });
  };

  const removeGuest = (index: number) => {
    if (guestList.length > 1) {
      const newGuestList = guestList.filter((_, i) => i !== index);
      setGuestList(newGuestList);
      setFormData({ ...formData, guestNumber: newGuestList.length });
    }
  };

  const updateGuest = (index: number, field: keyof Guest, value: string) => {
    const newGuestList = [...guestList];
    newGuestList[index] = { ...newGuestList[index], [field]: value };
    setGuestList(newGuestList);
  };

  const handleSubmit = async () => {
    if (!formData.note.trim()) {
      toast.error("⚠️ Vui lòng nhập lý do tham gia");
      return;
    }

    // Validate guest list
    for (let i = 0; i < guestList.length; i++) {
      const guest = guestList[i];
      if (!guest.guestName.trim()) {
        toast.error(`⚠️ Vui lòng nhập tên khách mời thứ ${i + 1}`);
        return;
      }
      if (!guest.guestPhone.trim()) {
        toast.error(`⚠️ Vui lòng nhập số điện thoại khách mời thứ ${i + 1}`);
        return;
      }
      if (!isValidPhone(guest.guestPhone.trim())) {
        toast.error(
          ` Số điện thoại khách mời thứ ${
            i + 1
          } không hợp lệ (phải có đúng 10 chữ số)`
        );
        return;
      }
      // Email is optional, no validation needed
    }

    let loadingToastId: any = null;
    try {
      setLoading(true);
      loadingToastId = toast.loading("Đang đăng ký danh sách khách mời...");

      const payload = {
        note: formData.note.trim(),
        guestNumber: guestList.length,
        guestList: guestList.map((guest) => {
          const guestData: any = {
            guestName: guest.guestName.trim(),
            guestPhone: formatPhoneNumber(guest.guestPhone.trim()),
          };

          // Only include email if it has a value
          if (guest.guestEmail.trim()) {
            guestData.guestEmail = guest.guestEmail.trim();
          }

          return guestData;
        }),
      };

      const response = await axios.post(
        `${dfData.domain}/api/EventGuests/Register/${eventId}`,
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

      if (response.data.success || response.data.code === 0) {
        const successMessage =
          response.data.message || "Đăng ký danh sách khách mời thành công!";
        toast.success(successMessage);
        onSuccess?.();
        setFormData({ note: "", guestNumber: 1 });
        setGuestList([{ guestName: "", guestPhone: "", guestEmail: "" }]);
        onClose();
        console.log("Attempting ZMP navigation to success page...");
        setTimeout(() => {
          navigate("/event/success-join");
        }, 200);
      } else {
        toast.error(
          response.data.message ||
            "Có lỗi xảy ra khi đăng ký danh sách khách mời"
        );
      }
    } catch (error) {
      console.error("Error registering event guests:", error);
      if (loadingToastId) {
        toast.dismiss(loadingToastId);
      }
      let errorMessage =
        "Có lỗi xảy ra khi đăng ký danh sách khách mời. Vui lòng thử lại!";
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

  return (
    <div onClick={(e) => e.stopPropagation()}>
      <Drawer
        title="Đăng ký danh sách khách mời"
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
          {/* Scrollable Content */}
          <div className="flex-1 overflow-y-auto p-5 pb-64">
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Lý do tham gia sự kiện *
                </label>
                <Input.TextArea
                  value={formData.note}
                  onChange={(e) =>
                    setFormData({ ...formData, note: e.target.value })
                  }
                  placeholder="Nhập lý do tham gia sự kiện"
                  rows={3}
                />
              </div>

              <div>
                <div className="flex items-center justify-between mb-3">
                  <label className="text-sm font-medium text-gray-700">
                    Danh sách khách mời ({guestList.length} người)
                  </label>
                  <Button
                    type="dashed"
                    onClick={addGuest}
                    icon={<PlusOutlined />}
                    size="small"
                    className="text-xs"
                  >
                    Thêm khách mời
                  </Button>
                </div>

                <div className="space-y-3">
                  {guestList.map((guest, index) => (
                    <div
                      key={index}
                      className="border border-gray-200 rounded-lg p-3 bg-gray-50"
                    >
                      <div className="flex items-center justify-between mb-2">
                        <span className="text-sm font-medium text-gray-600">
                          Khách mời {index + 1}
                        </span>
                        {guestList.length > 1 && (
                          <Button
                            type="text"
                            danger
                            size="small"
                            icon={<DeleteOutlined />}
                            onClick={() => removeGuest(index)}
                            className="text-xs"
                          >
                            Xóa
                          </Button>
                        )}
                      </div>

                      <div className="space-y-2">
                        <Input
                          className="h-10"
                          value={guest.guestName}
                          onChange={(e) =>
                            updateGuest(index, "guestName", e.target.value)
                          }
                          placeholder="Họ và tên"
                          size="small"
                        />
                        <Input
                          className="h-10"
                          value={guest.guestPhone}
                          onChange={(e) =>
                            updateGuest(index, "guestPhone", e.target.value)
                          }
                          placeholder="Số điện thoại"
                          size="small"
                        />
                        <Input
                          className="h-10"
                          value={guest.guestEmail}
                          onChange={(e) =>
                            updateGuest(index, "guestEmail", e.target.value)
                          }
                          placeholder="Email (Tùy chọn)"
                          size="small"
                        />
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          </div>

          {/* Fixed Bottom Buttons */}
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
                  {loading ? "Đang đăng ký..." : "Đăng ký"}
                </Button>
              </Col>
            </Row>
          </div>
        </div>
      </Drawer>
    </div>
  );
};

export default EventGuestRegisterDrawer;
