import React, { useState, useEffect } from "react";
import { Page } from "zmp-ui";
import { useNavigate, useLocation } from "react-router-dom";
import { useRecoilValue } from "recoil";
import { token } from "../../recoil/RecoilState";
import { Input, Select } from "antd";
import {
  Calendar,
  MapPin,
  FileText,
  Users,
  Link as LinkIcon,
} from "lucide-react";
import useSetHeader from "../../components/hooks/useSetHeader";
import { toast } from "react-toastify";
import dayjs from "dayjs";
import axios from "axios";
import dfData from "../../common/DefaultConfig.json";

interface MeetingFormData {
  groupId: string;
  groupName: string;
  title: string;
  description: string;
  startDate: string;
  endDate: string;
  meetingType: number;
  location: string;
  meetingLink: string;
  isPublic: boolean;
}

interface Group {
  id: string;
  name: string;
  description: string;
}

const MeetingCreate: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const setHeader = useSetHeader();
  const userToken = useRecoilValue(token);

  const meetingToEdit = location.state?.meeting;
  const isEditMode = !!meetingToEdit;

  const [loading, setLoading] = useState(false);
  const [loadingGroups, setLoadingGroups] = useState(false);
  const [groups, setGroups] = useState<Group[]>([]);

  const [formData, setFormData] = useState<MeetingFormData>({
    groupId: "",
    groupName: "",
    title: "",
    description: "",
    startDate: "",
    endDate: "",
    meetingType: 1,
    location: "",
    meetingLink: "",
    isPublic: true,
  });

  useEffect(() => {
    setHeader({
      title: isEditMode ? "CHỈNH SỬA LỊCH HỌP" : "TẠO LỊCH HỌP",
      showUserInfo: false,
      showMenuButton: false,
      showCloseButton: false,
      hasLeftIcon: true,
    });
    loadGroups();
  }, [setHeader, isEditMode]);

  useEffect(() => {
    if (isEditMode && meetingToEdit) {
      setFormData({
        groupId: meetingToEdit.groupId,
        groupName: meetingToEdit.groupName,
        title: meetingToEdit.title,
        description: meetingToEdit.description || "",
        startDate: meetingToEdit.startDate || "",
        endDate: meetingToEdit.endDate || "",
        meetingType: meetingToEdit.meetingType,
        location: meetingToEdit.location || "",
        meetingLink: meetingToEdit.meetingLink || "",
        isPublic:
          meetingToEdit.isPublic !== undefined ? meetingToEdit.isPublic : true,
      });
    }
  }, [isEditMode, meetingToEdit]);

  const loadGroups = async () => {
    try {
      setLoadingGroups(true);
      const response = await axios.get(`${dfData.domain}/api/Groups/all-for-user`, {
        headers: {
          Authorization: `Bearer ${userToken}`,
        },
      });

      if (response.data.success && response.data.data) {
        setGroups(response.data.data);
      }
    } catch (error) {
      console.error("Error loading groups:", error);
      toast.error("Không thể tải danh sách Club");
    } finally {
      setLoadingGroups(false);
    }
  };

  const validateForm = (): boolean => {
    if (!formData.groupId) {
      toast.error("Vui lòng chọn Club");
      return false;
    }
    if (!formData.title.trim()) {
      toast.error("Vui lòng nhập chủ đề");
      return false;
    }
    if (!formData.startDate) {
      toast.error("Vui lòng chọn thời gian bắt đầu");
      return false;
    }
    if (!formData.endDate) {
      toast.error("Vui lòng chọn thời gian kết thúc");
      return false;
    }

    const start = new Date(formData.startDate);
    const end = new Date(formData.endDate);
    if (end <= start) {
      toast.error("Thời gian kết thúc phải sau thời gian bắt đầu");
      return false;
    }

    if (formData.meetingType === 1 && !formData.meetingLink.trim()) {
      toast.error("Vui lòng nhập link meeting cho cuộc họp online");
      return false;
    }

    if (formData.meetingType === 2 && !formData.location.trim()) {
      toast.error("Vui lòng nhập địa điểm cho cuộc họp offline");
      return false;
    }

    return true;
  };

  const handleSubmit = async () => {
    if (!validateForm()) return;

    const selectedGroup = groups.find((g) => g.id === formData.groupId);
    if (!selectedGroup) {
      toast.error("Không tìm thấy thông tin nhóm");
      return;
    }

    try {
      setLoading(true);

      const payload = {
        groupId: formData.groupId,
        groupName: selectedGroup.name,
        title: formData.title,
        description: formData.description || null,
        startDate: formData.startDate,
        endDate: formData.endDate,
        meetingType: formData.meetingType,
        location: formData.meetingType === 2 ? formData.location : null,
        meetingLink: formData.meetingType === 1 ? formData.meetingLink : null,
        isPublic: formData.isPublic,
      };

      let response;
      if (isEditMode && meetingToEdit) {
        response = await axios.put(
          `${dfData.domain}/api/Meeting/${meetingToEdit.id}`,
          payload,
          {
            headers: {
              Authorization: `Bearer ${userToken}`,
              "Content-Type": "application/json",
            },
          }
        );
      } else {
        response = await axios.post(
          `${dfData.domain}/api/Meeting/Create`,
          payload,
          {
            headers: {
              Authorization: `Bearer ${userToken}`,
              "Content-Type": "application/json",
            },
          }
        );
      }

      if (response.data.success || response.data.code === 0) {
        toast.success(
          isEditMode
            ? "Cập nhật lịch họp thành công!"
            : "Tạo lịch họp thành công!"
        );
        navigate("/giba/meeting-list", { replace: true });
      } else {
        toast.error(response.data.message || "Có lỗi xảy ra");
      }
    } catch (error: any) {
      console.error(
        `Error ${isEditMode ? "updating" : "creating"} meeting:`,
        error
      );
      toast.error(
        error.response?.data?.message ||
          `Có lỗi xảy ra khi ${isEditMode ? "cập nhật" : "tạo"} lịch họp`
      );
    } finally {
      setLoading(false);
    }
  };

  return (
    <Page
      style={{ marginTop: "50px", background: "#f9fafb", minHeight: "100vh" }}
    >
      <div className="p-4 space-y-4 pb-24">
        <div className="bg-white rounded-xl p-5 border border-gray-200 shadow-sm">
          <div className="flex items-center gap-2 mb-4">
            <Users size={20} className="text-gray-700" />
            <h3 className="font-semibold text-base text-gray-900">
              Thông tin nhóm
            </h3>
          </div>

          <div>
            <label className="block text-sm font-medium mb-2 text-gray-700">
              Nhóm <span className="text-red-500">*</span>
            </label>
            <Select
              placeholder="Chọn nhóm"
              value={formData.groupId || undefined}
              onChange={(value) => {
                const group = groups.find((g) => g.id === value);
                setFormData({
                  ...formData,
                  groupId: value,
                  groupName: group?.name || "",
                });
              }}
              style={{ width: "100%" }}
              size="large"
              loading={loadingGroups}
              disabled={loadingGroups}
            >
              {groups.map((group) => (
                <Select.Option key={group.id} value={group.id}>
                  {group.name}
                </Select.Option>
              ))}
            </Select>
          </div>
        </div>

        <div className="bg-white rounded-xl p-5 border border-gray-200 shadow-sm">
          <div className="flex items-center gap-2 mb-4">
            <FileText size={20} className="text-gray-700" />
            <h3 className="font-semibold text-base text-gray-900">
              Nội dung cuộc họp
            </h3>
          </div>

          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium mb-2 text-gray-700">
                Chủ đề <span className="text-red-500">*</span>
              </label>
              <Input
                placeholder="VD: Họp kế hoạch quý 4"
                value={formData.title}
                onChange={(e) =>
                  setFormData({ ...formData, title: e.target.value })
                }
                size="large"
                maxLength={150}
                showCount
              />
            </div>

            <div>
              <label className="block text-sm font-medium mb-2 text-gray-700">
                Mô tả (không bắt buộc)
              </label>
              <Input.TextArea
                style={{ padding: "10px" }}
                placeholder="Mô tả chi tiết về nội dung cuộc họp..."
                value={formData.description}
                onChange={(e) =>
                  setFormData({ ...formData, description: e.target.value })
                }
                rows={4}
                maxLength={500}
                showCount
              />
            </div>

            <div>
              <label className="block text-sm font-medium mb-2 text-gray-700">
                Quyền truy cập <span className="text-red-500">*</span>
              </label>
              <Select
                placeholder="Chọn quyền truy cập"
                value={formData.isPublic}
                onChange={(value) =>
                  setFormData({ ...formData, isPublic: value })
                }
                style={{ width: "100%" }}
                size="large"
              >
                <Select.Option value={true}>Công khai</Select.Option>
                <Select.Option value={false}>Nội bộ</Select.Option>
              </Select>
            </div>
          </div>
        </div>

        <div className="bg-white rounded-xl p-5 border border-gray-200 shadow-sm">
          <div className="flex items-center gap-2 mb-4">
            <Calendar size={20} className="text-gray-700" />
            <h3 className="font-semibold text-base text-gray-900">Thời gian</h3>
          </div>

          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium mb-2 text-gray-700">
                Bắt đầu <span className="text-red-500">*</span>
              </label>
              <input
                type="datetime-local"
                value={
                  formData.startDate
                    ? dayjs(formData.startDate).format("YYYY-MM-DDTHH:mm")
                    : ""
                }
                onChange={(e) => {
                  const value = e.target.value;
                  setFormData({
                    ...formData,
                    startDate: value ? dayjs(value).toISOString() : "",
                  });
                }}
                min={dayjs().format("YYYY-MM-DDTHH:mm")}
                className="w-full px-3 py-2.5 border border-gray-300 rounded-lg text-base focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
            </div>

            <div>
              <label className="block text-sm font-medium mb-2 text-gray-700">
                Kết thúc <span className="text-red-500">*</span>
              </label>
              <input
                type="datetime-local"
                value={
                  formData.endDate
                    ? dayjs(formData.endDate).format("YYYY-MM-DDTHH:mm")
                    : ""
                }
                onChange={(e) => {
                  const value = e.target.value;
                  setFormData({
                    ...formData,
                    endDate: value ? dayjs(value).toISOString() : "",
                  });
                }}
                min={
                  formData.startDate
                    ? dayjs(formData.startDate).format("YYYY-MM-DDTHH:mm")
                    : dayjs().format("YYYY-MM-DDTHH:mm")
                }
                className="w-full px-3 py-2.5 border border-gray-300 rounded-lg text-base focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
            </div>
          </div>
        </div>

        <div className="bg-white rounded-xl p-5 border border-gray-200 shadow-sm">
          <div className="flex items-center gap-2 mb-4">
            <MapPin size={20} className="text-gray-700" />
            <h3 className="font-semibold text-base text-gray-900">
              Hình thức & Địa điểm
            </h3>
          </div>

          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium mb-2 text-gray-700">
                Hình thức <span className="text-red-500">*</span>
              </label>
              <Select
                placeholder="Chọn hình thức"
                value={formData.meetingType}
                onChange={(value) =>
                  setFormData({ ...formData, meetingType: value })
                }
                style={{ width: "100%" }}
                size="large"
              >
                <Select.Option value={1}>Online</Select.Option>
                <Select.Option value={2}>Offline</Select.Option>
              </Select>
            </div>

            {formData.meetingType === 1 ? (
              <div>
                <label className="block text-sm font-medium mb-2 text-gray-700">
                  Link meeting <span className="text-red-500">*</span>
                </label>
                <Input
                  prefix={<LinkIcon size={16} />}
                  placeholder="VD: https://zoom.us/j/123456789"
                  value={formData.meetingLink}
                  onChange={(e) =>
                    setFormData({ ...formData, meetingLink: e.target.value })
                  }
                  size="large"
                  maxLength={300}
                />
              </div>
            ) : (
              <div>
                <label className="block text-sm font-medium mb-2 text-gray-700">
                  Địa điểm <span className="text-red-500">*</span>
                </label>
                <Input.TextArea
                  placeholder="VD: Phòng họp A - Tầng 5, Tòa nhà ABC"
                  value={formData.location}
                  onChange={(e) =>
                    setFormData({ ...formData, location: e.target.value })
                  }
                  rows={2}
                  maxLength={200}
                  showCount
                />
              </div>
            )}
          </div>
        </div>
      </div>

      <div className="fixed bottom-0 left-0 right-0 bg-white border-t border-gray-200 p-4 flex gap-3 shadow-lg z-50">
        <button
          onClick={() => navigate(-1)}
          className="flex-1 px-4 py-2 bg-white border border-gray-300 text-gray-700 rounded-lg font-medium text-base transition-all hover:bg-gray-50"
          disabled={loading}
        >
          Hủy
        </button>
        <button
          onClick={handleSubmit}
          className="flex-1 px-4 py-2 bg-black text-white rounded-lg font-semibold text-base transition-all hover:bg-gray-800 disabled:opacity-50 disabled:cursor-not-allowed"
          disabled={loading}
        >
          {loading ? "Đang xử lý..." : isEditMode ? "Cập nhật" : "Tạo lịch"}
        </button>
      </div>
    </Page>
  );
};

export default MeetingCreate;
