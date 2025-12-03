import React, { useState, useEffect } from "react";
import { Page } from "zmp-ui";
import { Input, Button } from "antd";
import { useNavigate, useLocation } from "react-router-dom";
import { toast } from "react-toastify";
import { PlusOutlined, DeleteOutlined } from "@ant-design/icons";
import { useRecoilValue } from "recoil";
import { token } from "../../recoil/RecoilState";
import axios from "axios";
import dfData from "../../common/DefaultConfig.json";
import useSetHeader from "../../components/hooks/useSetHeader";

interface Guest {
  guestName: string;
  guestPhone: string;
  guestEmail: string;
}

const EventGuestRegisterPage: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const setHeader = useSetHeader();
  const eventId = location.state?.eventId || "";
  const userToken = useRecoilValue(token);
  const [loading, setLoading] = useState(false);
  const [formData, setFormData] = useState({
    note: "",
    guestNumber: 1,
  });
  const [guestList, setGuestList] = useState<Guest[]>([
    { guestName: "", guestPhone: "", guestEmail: "" },
  ]);

  useEffect(() => {
    setHeader({
      title: "ĐĂNG KÝ KHÁCH MỜI",
      hasLeftIcon: true,
    });
  }, [setHeader]);

  useEffect(() => {
    if (!eventId) {
      toast.error("Không tìm thấy thông tin sự kiện");
      navigate(-1);
    }
  }, [eventId, navigate]);

  const isValidPhone = (phone: string) => {
    const cleanPhone = phone.replace(/\s/g, "");
    return /^\d{10}$/.test(cleanPhone);
  };

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
          `❌ Số điện thoại khách mời thứ ${
            i + 1
          } không hợp lệ (phải có đúng 10 chữ số)`
        );
        return;
      }
    }

    let loadingToastId: any = null;
    try {
      setLoading(true);
      loadingToastId = toast.loading("Đang đăng ký danh sách khách mời...");

      const payload = {
        note: formData.note.trim(),
        guestNumber: guestList.length,
        guestList: guestList.map((guest) => ({
          guestName: guest.guestName.trim(),
          guestPhone: formatPhoneNumber(guest.guestPhone.trim()),
          guestEmail: guest.guestEmail.trim() || undefined,
        })),
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
        toast.success("✅ Đăng ký danh sách khách mời thành công!");
        setTimeout(() => {
          navigate("/giba", { replace: true });
        }, 500);
      } else {
        toast.error(response.data.message || "Có lỗi xảy ra khi đăng ký");
      }
    } catch (error) {
      console.error("Error registering guest list:", error);
      if (loadingToastId) {
        toast.dismiss(loadingToastId);
      }
      let errorMessage = "Có lỗi xảy ra khi đăng ký. Vui lòng thử lại!";
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
    <Page
      className="bg-white min-h-screen"
      style={{ paddingTop: "50px", paddingBottom: "80px" }}
    >
      <div className="p-5">
        <div className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Lý do tham gia <span style={{ color: "#ef4444" }}>*</span>
            </label>
            <Input.TextArea
              rows={3}
              value={formData.note}
              onChange={(e) =>
                setFormData({ ...formData, note: e.target.value })
              }
              placeholder="Nhập lý do tham gia sự kiện"
            />
          </div>

          <div className="flex items-center justify-between mb-4">
            <h3 className="text-base font-semibold text-gray-900">
              Danh sách khách mời ({guestList.length})
            </h3>
            <Button
              type="primary"
              icon={<PlusOutlined />}
              onClick={addGuest}
              className="bg-green-600 hover:bg-green-700"
            >
              Thêm khách
            </Button>
          </div>

          {guestList.map((guest, index) => (
            <div
              key={index}
              className="border border-gray-200 rounded-lg p-4 space-y-3"
            >
              <div className="flex items-center justify-between mb-2">
                <span className="text-sm font-semibold text-gray-700">
                  Khách mời #{index + 1}
                </span>
                {guestList.length > 1 && (
                  <Button
                    type="text"
                    danger
                    icon={<DeleteOutlined />}
                    onClick={() => removeGuest(index)}
                    size="small"
                  >
                    Xóa
                  </Button>
                )}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Họ và tên <span style={{ color: "#ef4444" }}>*</span>
                </label>
                <Input
                  value={guest.guestName}
                  onChange={(e) =>
                    updateGuest(index, "guestName", e.target.value)
                  }
                  placeholder="Nhập họ và tên"
                  className="h-10"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Số điện thoại <span style={{ color: "#ef4444" }}>*</span>
                </label>
                <Input
                  value={guest.guestPhone}
                  onChange={(e) =>
                    updateGuest(index, "guestPhone", e.target.value)
                  }
                  placeholder="Nhập số điện thoại (10 chữ số)"
                  maxLength={10}
                  className="h-10"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Email
                </label>
                <Input
                  type="email"
                  value={guest.guestEmail}
                  onChange={(e) =>
                    updateGuest(index, "guestEmail", e.target.value)
                  }
                  placeholder="Nhập email (không bắt buộc)"
                  className="h-10"
                />
              </div>
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
          {loading ? "Đang đăng ký..." : "Đăng ký danh sách"}
        </Button>
      </div>
    </Page>
  );
};

export default EventGuestRegisterPage;
