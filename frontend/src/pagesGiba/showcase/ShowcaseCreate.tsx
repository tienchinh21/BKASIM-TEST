import React, { useState, useEffect } from "react";
import { Page } from "zmp-ui";
import { useNavigate, useLocation } from "react-router-dom";
import { useRecoilValue } from "recoil";
import { token } from "../../recoil/RecoilState";
import { Input, Select } from "antd";
import { Calendar, MapPin, FileText, Users } from "lucide-react";
import useSetHeader from "../../components/hooks/useSetHeader";
import { toast } from "react-toastify";
import dayjs from "dayjs";
import axios from "axios";
import dfData from "../../common/DefaultConfig.json";
import { useHasRole } from "../../hooks/useHasRole";
import {
  ShowcaseStatus,
  ShowcaseStatusLabel,
} from "../../utils/enum/showcase.enum";

interface ShowcaseFormData {
  groupId: string;
  membershipId: string;
  title: string;
  description: string;
  startDate: string;
  endDate: string;
  location: string;
  status: number;
  isPublic: boolean;
}

interface Group {
  id: string;
  name: string;
  description: string;
}

interface Member {
  id: string;
  userZaloId: string;
  memberName: string;
  zaloAvatar: string;
  company: string;
  position: string;
}

const ShowcaseCreate: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const setHeader = useSetHeader();
  const userToken = useRecoilValue(token);

  const showcaseToEdit = location.state?.showcase;
  const isEditMode = !!showcaseToEdit;

  const [loading, setLoading] = useState(false);
  const [loadingGroups, setLoadingGroups] = useState(false);
  const [loadingMembers, setLoadingMembers] = useState(false);
  const [groups, setGroups] = useState<Group[]>([]);
  const [members, setMembers] = useState<Member[]>([]);
  const [selectedMember, setSelectedMember] = useState<Member | null>(null);

  const [formData, setFormData] = useState<ShowcaseFormData>({
    groupId: "",
    membershipId: "",
    title: "",
    description: "",
    startDate: "",
    endDate: "",
    location: "",
    status: 1,
    isPublic: true,
  });

  useEffect(() => {
    setHeader({
      title: isEditMode ? "CHỈNH SỬA SHOWCASE" : "TẠO LỊCH SHOWCASE",
      showUserInfo: false,
      showMenuButton: false,
      showCloseButton: false,
      hasLeftIcon: true,
    });
    loadGroups();
  }, [setHeader, isEditMode]);

  useEffect(() => {
    if (isEditMode && showcaseToEdit) {
      setFormData({
        groupId: showcaseToEdit.groupId,
        membershipId: showcaseToEdit.membershipId,
        title: showcaseToEdit.title,
        description: showcaseToEdit.description || "",
        startDate: showcaseToEdit.startDate,
        endDate: showcaseToEdit.endDate,
        location: showcaseToEdit.location,
        status: showcaseToEdit.status || 1,
        isPublic:
          showcaseToEdit.isPublic !== undefined
            ? showcaseToEdit.isPublic
            : true,
      });
    }
  }, [isEditMode, showcaseToEdit]);

  useEffect(() => {
    if (formData.groupId) {
      loadMembers(formData.groupId);
    } else {
      setMembers([]);
      setSelectedMember(null);
    }
  }, [formData.groupId]);

  useEffect(() => {
    if (formData.membershipId) {
      const member = members.find(
        (m) => m.userZaloId === formData.membershipId
      );
      setSelectedMember(member || null);
    } else {
      setSelectedMember(null);
    }
  }, [formData.membershipId, members]);

  const loadGroups = async () => {
    try {
      setLoadingGroups(true);
      const response = await axios.get(`${dfData.domain}/Groups/GetAll`, {
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

  const loadMembers = async (groupId: string) => {
    try {
      setLoadingMembers(true);
      setMembers([]);
      const response = await axios.get(
        `${dfData.domain}/api/memberships/get-membership-by-group?groupId=${groupId}`,
        {
          headers: {
            Authorization: `Bearer ${userToken}`,
          },
        }
      );

      if (response.data.success && response.data.data?.membership) {
        setMembers(response.data.data.membership);
      }
    } catch (error) {
      console.error("Error loading members:", error);
      toast.error("Không thể tải danh sách thành viên");
    } finally {
      setLoadingMembers(false);
    }
  };

  const validateForm = (): boolean => {
    if (!formData.groupId) {
      toast.error("Vui lòng chọn nhóm");
      return false;
    }
    if (!formData.membershipId) {
      toast.error("Vui lòng chọn diễn giả");
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
    if (!formData.location.trim()) {
      toast.error("Vui lòng nhập địa điểm");
      return false;
    }

    const start = new Date(formData.startDate);
    const end = new Date(formData.endDate);
    if (end <= start) {
      toast.error("Thời gian kết thúc phải sau thời gian bắt đầu");
      return false;
    }

    return true;
  };

  const handleSubmit = async () => {
    if (!validateForm()) return;

    if (!selectedMember) {
      toast.error("Vui lòng chọn diễn giả");
      return;
    }

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
        membershipId: formData.membershipId,
        membershipName: selectedMember.memberName,
        startDate: formData.startDate,
        endDate: formData.endDate,
        location: formData.location,
        status: formData.status,
        memberShipAvatar: selectedMember.zaloAvatar,
        IsPublic: formData.isPublic,
      };

      let response;
      if (isEditMode && showcaseToEdit) {
        response = await axios.put(
          `${dfData.domain}/api/Showcase/${showcaseToEdit.id}`,
          payload,
          {
            headers: {
              Authorization: `Bearer ${userToken}`,
              "Content-Type": "application/json",
            },
          }
        );
      } else {
        // Create new showcase
        response = await axios.post(
          `${dfData.domain}/api/Showcase/Create`,
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
            ? "Cập nhật showcase thành công!"
            : "Tạo lịch showcase thành công!"
        );
        navigate("/giba/showcase-list");
      } else {
        toast.error(response.data.message || "Có lỗi xảy ra");
      }
    } catch (error: any) {
      console.error(
        `Error ${isEditMode ? "updating" : "creating"} showcase:`,
        error
      );
      toast.error(
        error.response?.data?.message ||
          `Có lỗi xảy ra khi ${isEditMode ? "cập nhật" : "tạo"} showcase`
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

          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium mb-2 text-gray-700">
                Nhóm <span className="text-red-500">*</span>
              </label>
              <Select
                placeholder="Chọn nhóm"
                value={formData.groupId || undefined}
                onChange={(value) =>
                  setFormData({ ...formData, groupId: value, membershipId: "" })
                }
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

            <div>
              <label className="block text-sm font-medium mb-2 text-gray-700">
                Diễn giả <span className="text-red-500">*</span>
              </label>
              <Select
                placeholder="Chọn diễn giả"
                value={formData.membershipId || undefined}
                onChange={(value) =>
                  setFormData({ ...formData, membershipId: value })
                }
                style={{ width: "100%" }}
                size="large"
                loading={loadingMembers}
                disabled={!formData.groupId || loadingMembers}
              >
                {members.map((member) => (
                  <Select.Option key={member.id} value={member.userZaloId}>
                    <div className="flex items-center gap-2">
                      <img
                        src={member.zaloAvatar}
                        alt={member.memberName}
                        className="w-6 h-6 rounded-full"
                      />
                      <div>
                        <div className="text-sm font-medium">
                          {member.memberName}
                        </div>
                        <div className="text-xs text-gray-500">
                          {member.position} - {member.company}
                        </div>
                      </div>
                    </div>
                  </Select.Option>
                ))}
              </Select>
            </div>

            {selectedMember && (
              <div className="bg-gray-50 p-3 rounded-lg border border-gray-200">
                <div className="flex items-center gap-3">
                  <img
                    src={selectedMember.zaloAvatar}
                    alt={selectedMember.memberName}
                    className="w-12 h-12 rounded-full border-2 border-gray-300"
                  />
                  <div className="flex-1">
                    <div className="font-semibold text-sm text-gray-900">
                      {selectedMember.memberName}
                    </div>
                    <div className="text-xs text-gray-600">
                      {selectedMember.position}
                    </div>
                    <div className="text-xs text-gray-500">
                      {selectedMember.company}
                    </div>
                  </div>
                </div>
              </div>
            )}

            {isEditMode && (
              <div>
                <label className="block text-sm font-medium mb-2 text-gray-700">
                  Trạng thái <span className="text-red-500">*</span>
                </label>
                <Select
                  placeholder="Chọn trạng thái"
                  value={formData.status}
                  onChange={(value) =>
                    setFormData({ ...formData, status: value })
                  }
                  style={{ width: "100%" }}
                  size="large"
                >
                  <Select.Option value={1}>
                    {ShowcaseStatusLabel[ShowcaseStatus.SCHEDULED]}
                  </Select.Option>
                  <Select.Option value={2}>
                    {ShowcaseStatusLabel[ShowcaseStatus.ONGOING]}
                  </Select.Option>
                  <Select.Option value={3}>
                    {ShowcaseStatusLabel[ShowcaseStatus.COMPLETED]}
                  </Select.Option>
                  <Select.Option value={4}>
                    {ShowcaseStatusLabel[ShowcaseStatus.CANCELLED]}
                  </Select.Option>
                </Select>
              </div>
            )}
          </div>
        </div>

        <div className="bg-white rounded-xl p-5 border border-gray-200 shadow-sm">
          <div className="flex items-center gap-2 mb-4">
            <FileText size={20} className="text-gray-700" />
            <h3 className="font-semibold text-base text-gray-900">
              Nội dung showcase
            </h3>
          </div>

          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium mb-2 text-gray-700">
                Chủ đề <span className="text-red-500">*</span>
              </label>
              <Input
                placeholder="VD: Chiến lược Marketing trong kỷ nguyên số"
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
                placeholder="Mô tả chi tiết về nội dung showcase..."
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
                min={dayjs().format("YYYY-MM-DDTHH:mm")}
                className="w-full px-3 py-2.5 border border-gray-300 rounded-lg text-base focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
            </div>
          </div>
        </div>

        <div className="bg-white rounded-xl p-5 border border-gray-200 shadow-sm">
          <div className="flex items-center gap-2 mb-4">
            <MapPin size={20} className="text-gray-700" />
            <h3 className="font-semibold text-base text-gray-900">Địa điểm</h3>
          </div>

          <div>
            <label className="block text-sm font-medium mb-2 text-gray-700">
              Địa chỉ/Địa điểm tổ chức <span className="text-red-500">*</span>
            </label>
            <Input.TextArea
              placeholder="VD: Phòng hội thảo A - Tầng 15, Tòa nhà Landmark 81 hoặc Online - Zoom Meeting"
              value={formData.location}
              onChange={(e) =>
                setFormData({ ...formData, location: e.target.value })
              }
              rows={2}
              maxLength={200}
              showCount
            />
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
          {loading ? "Đang xử lý..." : isEditMode ? "Cập nhật " : "Tạo lịch "}
        </button>
      </div>
    </Page>
  );
};

export default ShowcaseCreate;
