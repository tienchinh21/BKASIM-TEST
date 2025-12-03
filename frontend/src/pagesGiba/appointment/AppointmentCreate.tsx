import React, { useState, useEffect } from "react";
import { Page } from "zmp-ui";
import { useNavigate, useLocation } from "react-router-dom";
import { Input, Select } from "antd";
import { Calendar, MapPin, FileText, Users } from "lucide-react";
import useSetHeader from "../../components/hooks/useSetHeader";
import { toast } from "react-toastify";
import { useRecoilValue } from "recoil";
import { token, infoUser, userMembershipInfo } from "../../recoil/RecoilState";
import axios from "axios";
import dfData from "../../common/DefaultConfig.json";
import dayjs from "dayjs";
import "./AppointmentCreate.css";

interface AppointmentItem {
  id: string;
  name: string;
  appointmentFrom?: string;
  appointmentTo?: string;
  appointmentFromId?: string;
  appointmentToId?: string;
  appointmentFromName?: string;
  appointmentToName?: string;
  groupId: string;
  groupName?: string;
  content: string;
  location: string;
  time: string;
  status: number;
  cancelReason: string | null;
  createdDate: string;
  updatedDate: string;
}
interface AppointmentFormData {
  groupId: string;
  receiverId: string;
  appointmentDate: string;
  appointmentTime: string;
  location: string;
  purpose: string;
  notes: string;
}

interface Group {
  id: string;
  groupName: string;
  description: string;
  rule: string;
  isActive: boolean;
  memberCount: number;
  createdDate: string;
  updatedDate: string;
  type: string;
  isJoined: boolean;
  joinStatus: string | null;
  joinStatusText: string | null;
  logo?: string | null;
}

interface Member {
  id: string;
  userZaloId: string;
  memberName: string;
  zaloAvatar: string;
  company: string;
  position: string;
}

const AppointmentCreate: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const setHeader = useSetHeader();
  const userToken = useRecoilValue(token);
  const [loading, setLoading] = useState(false);
  const [loadingGroups, setLoadingGroups] = useState(false);
  const [loadingMembers, setLoadingMembers] = useState(false);

  const userInfo = useRecoilValue(infoUser);
  const membershipInfo = useRecoilValue(userMembershipInfo);
  //@ts-ignore
  const userZaloId = membershipInfo?.userZaloId || userInfo?.userZaloId;

  // Check if editing existing appointment
  const editingAppointment = location.state?.appointment as
    | AppointmentItem
    | undefined;
  const isEditMode = !!editingAppointment;

  const [formData, setFormData] = useState<AppointmentFormData>({
    groupId: "",
    receiverId: "",
    appointmentDate: "",
    appointmentTime: "",
    location: "",
    purpose: "",
    notes: "",
  });

  const [groups, setGroups] = useState<Group[]>([]);
  const [members, setMembers] = useState<Member[]>([]);
  const [selectedMember, setSelectedMember] = useState<Member | null>(null);
  const [errors, setErrors] = useState<
    Partial<Record<keyof AppointmentFormData, string>>
  >({});
  const [isPreFilled, setIsPreFilled] = useState(false);

  useEffect(() => {
    setHeader({
      title: isEditMode ? "CHỈNH SỬA ĐẶT HẸN" : "TẠO ĐẶT HẸN",
      showUserInfo: false,
      showMenuButton: false,
      showCloseButton: false,
      hasLeftIcon: true,
    });
    loadGroups();
  }, [setHeader, isEditMode]);

  useEffect(() => {
    if (formData.groupId) {
      loadMembers(formData.groupId);
    } else {
      setMembers([]);
      setSelectedMember(null);
    }
  }, [formData.groupId]);

  useEffect(() => {
    if (formData.receiverId) {
      const member = members.find((m) => m.userZaloId === formData.receiverId);
      setSelectedMember(member || null);
    } else {
      setSelectedMember(null);
    }
  }, [formData.receiverId, members]);

  useEffect(() => {
    // Pre-fill from MemberDetailGiba
    const state = location.state as {
      groupId?: string;
      receiverId?: string;
      receiverName?: string;
      appointment?: AppointmentItem;
    };

    if (state?.groupId && state?.receiverId && !editingAppointment) {
      setFormData((prev) => ({
        ...prev,
        groupId: state.groupId!,
        receiverId: state.receiverId!,
      }));
      setIsPreFilled(true);
    }
  }, [location.state, editingAppointment]);

  // Set groupId first when editing
  useEffect(() => {
    if (editingAppointment && groups.length > 0 && !formData.groupId) {
      setFormData((prev) => ({
        ...prev,
        groupId: editingAppointment.groupId,
      }));
    }
  }, [editingAppointment, groups]);

  // Set other fields after members are loaded
  useEffect(() => {
    if (
      editingAppointment &&
      members.length > 0 &&
      formData.groupId === editingAppointment.groupId &&
      !formData.receiverId
    ) {
      const appointmentTime = dayjs(editingAppointment.time);

      const contentParts = editingAppointment.content.split(" - ");
      const purpose = contentParts[0] || "";
      const notes =
        contentParts.length > 1 ? contentParts.slice(1).join(" - ") : "";

      // Support both old (appointmentTo) and new (appointmentToId) API response
      const receiverId =
        editingAppointment.appointmentToId || editingAppointment.appointmentTo;

      setFormData((prev) => ({
        ...prev,
        receiverId: receiverId || "",
        appointmentDate: appointmentTime.format("YYYY-MM-DD"),
        appointmentTime: appointmentTime.format("HH:mm"),
        location: editingAppointment.location,
        purpose: purpose,
        notes: notes,
      }));
    }
  }, [editingAppointment, members, formData.groupId]);

  const loadGroups = async () => {
    try {
      setLoadingGroups(true);
      const response = await axios.get(
        `${dfData.domain}/api/Groups/all?Page=1&PageSize=15&Keyword=&JoinStatus=approved`,
        {
          headers: {
            Authorization: `Bearer ${userToken}`,
          },
        }
      );

      if (response.data.code === 0 && response.data.data?.items) {
        setGroups(response.data.data.items);
      } else {
        setGroups([]);
      }
    } catch (error) {
      console.error("Error loading groups:", error);
      toast.error("Không thể tải danh sách Club");
      setGroups([]);
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
    const newErrors: Partial<Record<keyof AppointmentFormData, string>> = {};

    if (!formData.groupId) {
      newErrors.groupId = "Vui lòng chọn nhóm";
    }
    if (!formData.receiverId) {
      newErrors.receiverId = "Vui lòng chọn người nhận";
    }
    if (!formData.appointmentDate) {
      newErrors.appointmentDate = "Vui lòng chọn ngày hẹn";
    }
    if (!formData.appointmentTime) {
      newErrors.appointmentTime = "Vui lòng chọn giờ hẹn";
    }
    if (!formData.location.trim()) {
      newErrors.location = "Vui lòng nhập địa điểm";
    }
    if (!formData.purpose.trim()) {
      newErrors.purpose = "Vui lòng nhập mục đích";
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleDateChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData({ ...formData, appointmentDate: e.target.value });
  };

  const handleTimeChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData({ ...formData, appointmentTime: e.target.value });
  };

  // Get min date (today)
  const getMinDate = () => {
    return dayjs().format("YYYY-MM-DD");
  };

  const handleSubmit = async () => {
    if (!validateForm()) {
      toast.error("Vui lòng kiểm tra lại thông tin");
      return;
    }

    if (!selectedMember) {
      toast.error("Vui lòng chọn người nhận");
      return;
    }

    const selectedGroup = groups.find((g) => g.id === formData.groupId);
    if (!selectedGroup) {
      toast.error("Không tìm thấy thông tin nhóm");
      return;
    }

    try {
      setLoading(true);

      const appointmentDateTime = `${formData.appointmentDate}T${formData.appointmentTime}:00`;

      const payload = {
        name: selectedMember.memberName,
        groupId: formData.groupId,
        appointmentTo: formData.receiverId,
        content: formData.notes?.trim()
          ? `${formData.purpose} - ${formData.notes}`
          : formData.purpose,
        location: formData.location,
        time: appointmentDateTime,
        appointmentFrom: userZaloId || "",
      };

      let response;
      if (isEditMode && editingAppointment) {
        response = await axios.put(
          `${dfData.domain}/api/Appointment/${editingAppointment.id}`,
          { ...payload, id: editingAppointment.id },
          {
            headers: {
              Authorization: `Bearer ${userToken}`,
              "Content-Type": "application/json",
            },
          }
        );
      } else {
        response = await axios.post(
          `${dfData.domain}/api/Appointment`,
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
            ? "Cập nhật lịch hẹn thành công!"
            : "Tạo lịch hẹn thành công!"
        );
        navigate("/giba/appointment-list");
      } else {
        toast.error(response.data.message || "Có lỗi xảy ra");
      }
    } catch (error: any) {
      console.error("Error saving appointment:", error);
      toast.error(
        error.response?.data?.message ||
          `Có lỗi xảy ra khi ${isEditMode ? "cập nhật" : "tạo"} lịch hẹn`
      );
    } finally {
      setLoading(false);
    }
  };

  return (
    <Page
      style={{ marginTop: "50px", background: "#f5f5f5", minHeight: "100vh" }}
    >
      <div className="p-4 space-y-4 max-h-[calc(100vh-50px)] overflow-y-auto pb-24">
        <div className="bg-blue-50 p-4 rounded-lg border border-blue-200 animate-fadeIn">
          <div className="flex items-center gap-2 mb-3">
            <Users size={18} className="text-blue-600" />
            <h3 className="font-semibold text-blue-900">
              Thông tin người nhận
            </h3>
          </div>

          <div className="space-y-3">
            <div>
              <label className="block text-sm font-medium mb-1 text-gray-700">
                Nhóm <span className="text-red-500">*</span>
              </label>
              <Select
                placeholder="Chọn nhóm"
                value={formData.groupId || undefined}
                onChange={(value) =>
                  setFormData({ ...formData, groupId: value, receiverId: "" })
                }
                style={{ width: "100%" }}
                size="large"
                loading={loadingGroups}
                disabled={loadingGroups || isEditMode || isPreFilled}
                status={errors.groupId ? "error" : ""}
              >
                {groups.map((group) => (
                  <Select.Option key={group.id} value={group.id}>
                    {group.groupName}
                  </Select.Option>
                ))}
              </Select>
              {errors.groupId && (
                <div className="text-red-500 text-xs mt-1">
                  {errors.groupId}
                </div>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium mb-1 text-gray-700">
                Người nhận <span className="text-red-500">*</span>
              </label>
              <Select
                placeholder="Chọn người nhận"
                value={formData.receiverId || undefined}
                onChange={(value) =>
                  setFormData({ ...formData, receiverId: value })
                }
                style={{ width: "100%" }}
                size="large"
                loading={loadingMembers}
                disabled={
                  !formData.groupId ||
                  loadingMembers ||
                  isEditMode ||
                  isPreFilled
                }
                status={errors.receiverId ? "error" : ""}
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
                        <div className="font-medium">{member.memberName}</div>
                      </div>
                    </div>
                  </Select.Option>
                ))}
              </Select>
              {errors.receiverId && (
                <div className="text-red-500 text-xs mt-1">
                  {errors.receiverId}
                </div>
              )}
              {formData.groupId && !loadingMembers && members.length === 0 && (
                <div className="text-gray-500 text-xs mt-1">
                  Không có thành viên trong nhóm này
                </div>
              )}
            </div>

            {selectedMember && (
              <div className="bg-white p-3 rounded-lg border border-gray-200 shadow-sm animate-fadeIn">
                <div className="flex items-center gap-3">
                  <img
                    src={selectedMember.zaloAvatar}
                    alt={selectedMember.memberName}
                    className="w-12 h-12 rounded-full border-2 border-blue-400"
                  />
                  <div className="flex-1">
                    <div className="font-semibold text-gray-900">
                      {selectedMember.memberName}
                    </div>
                    <div className="text-xs text-gray-500">
                      {selectedMember.company
                        ? `${selectedMember.company} - ${selectedMember.position}`
                        : selectedMember.position}
                    </div>
                  </div>
                </div>
              </div>
            )}
          </div>
        </div>

        <div className="bg-purple-50 p-4 rounded-lg border border-purple-200 animate-fadeIn">
          <div className="flex items-center gap-2 mb-3">
            <Calendar size={18} className="text-purple-600" />
            <h3 className="font-semibold text-purple-900">Thời gian hẹn</h3>
          </div>

          <div className="space-y-3">
            <div>
              <label className="block text-sm font-medium mb-1 text-gray-700">
                Ngày hẹn <span className="text-red-500">*</span>
              </label>
              <input
                type="date"
                className={`native-date-input ${
                  errors.appointmentDate ? "error" : ""
                }`}
                value={formData.appointmentDate}
                onChange={handleDateChange}
                min={getMinDate()}
              />
              {errors.appointmentDate && (
                <div className="text-red-500 text-xs mt-1">
                  {errors.appointmentDate}
                </div>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium mb-1 text-gray-700">
                Giờ hẹn <span className="text-red-500">*</span>
              </label>
              <input
                type="time"
                className={`native-time-input ${
                  errors.appointmentTime ? "error" : ""
                }`}
                value={formData.appointmentTime}
                onChange={handleTimeChange}
              />
              {errors.appointmentTime && (
                <div className="text-red-500 text-xs mt-1">
                  {errors.appointmentTime}
                </div>
              )}
            </div>
          </div>
        </div>

        <div className="bg-green-50 p-4 rounded-lg border border-green-200 animate-fadeIn">
          <div className="flex items-center gap-2 mb-3">
            <MapPin size={18} className="text-green-600" />
            <h3 className="font-semibold text-green-900">Địa điểm</h3>
          </div>

          <div>
            <label className="block text-sm font-medium mb-1 text-gray-700">
              Địa chỉ/Địa điểm gặp <span className="text-red-500">*</span>
            </label>
            <Input.TextArea
              placeholder="VD: Quán cafe The Coffee House - Chi nhánh Trần Hưng Đạo"
              value={formData.location}
              onChange={(e) =>
                setFormData({ ...formData, location: e.target.value })
              }
              rows={2}
              status={errors.location ? "error" : ""}
              maxLength={200}
              showCount
            />
            {errors.location && (
              <div className="text-red-500 text-xs mt-1">{errors.location}</div>
            )}
          </div>
        </div>

        <div className="bg-yellow-50 p-4 rounded-lg border border-yellow-200 animate-fadeIn">
          <div className="flex items-center gap-2 mb-3">
            <FileText size={18} className="text-yellow-600" />
            <h3 className="font-semibold text-yellow-900">Nội dung cuộc hẹn</h3>
          </div>

          <div className="space-y-3">
            <div>
              <label className="block text-sm font-medium mb-1 text-gray-700">
                Mục đích <span className="text-red-500">*</span>
              </label>
              <Input.TextArea
                placeholder="VD: Thảo luận về hợp tác marketing, bàn về kế hoạch tài chính Q4..."
                value={formData.purpose}
                onChange={(e) =>
                  setFormData({ ...formData, purpose: e.target.value })
                }
                rows={2}
                status={errors.purpose ? "error" : ""}
                maxLength={200}
                showCount
              />
              {errors.purpose && (
                <div className="text-red-500 text-xs mt-1">
                  {errors.purpose}
                </div>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium mb-1 text-gray-700">
                Ghi chú (không bắt buộc)
              </label>
              <Input.TextArea
                placeholder="Thêm ghi chú hoặc lưu ý cho cuộc hẹn..."
                value={formData.notes}
                onChange={(e) =>
                  setFormData({ ...formData, notes: e.target.value })
                }
                rows={3}
                maxLength={500}
                showCount
              />
            </div>
          </div>
        </div>

        {selectedMember &&
          formData.appointmentDate &&
          formData.appointmentTime && (
            <div className="bg-gradient-to-r from-blue-50 to-purple-50 p-4 rounded-lg border-2 border-blue-200 animate-fadeIn">
              <div className="text-sm font-semibold text-gray-700 mb-2">
                Tóm tắt lịch hẹn
              </div>
              <div className="space-y-1 text-xs text-gray-600">
                <div>
                  Người nhận:{" "}
                  <span className="font-medium">
                    {selectedMember.memberName}
                  </span>
                </div>
                <div>
                  Thời gian:{" "}
                  <span className="font-medium">
                    {dayjs(formData.appointmentDate).format("DD/MM/YYYY")} -{" "}
                    {formData.appointmentTime}
                  </span>
                </div>
                {formData.location && (
                  <div>
                    Địa điểm:{" "}
                    <span className="font-medium">{formData.location}</span>
                  </div>
                )}
              </div>
            </div>
          )}
      </div>

      <div className="fixed bottom-0 left-0 right-0 bg-white border-t border-gray-200 p-4 flex gap-2 shadow-lg z-50">
        <button
          onClick={() => navigate(-1)}
          className="flex-1 px-4 py-2.5 bg-gray-100 hover:bg-gray-200 text-gray-700 rounded-lg font-medium transition-all"
          disabled={loading}
        >
          Hủy
        </button>
        <button
          onClick={handleSubmit}
          className="flex-1 px-4 py-2.5 bg-yellow-400 hover:bg-yellow-500 text-black rounded-lg font-semibold transition-all disabled:opacity-50 disabled:cursor-not-allowed"
          disabled={loading}
        >
          {loading
            ? "Đang xử lý..."
            : isEditMode
            ? "Cập nhật lịch hẹn"
            : "Tạo lịch hẹn"}
        </button>
      </div>
    </Page>
  );
};

export default AppointmentCreate;
