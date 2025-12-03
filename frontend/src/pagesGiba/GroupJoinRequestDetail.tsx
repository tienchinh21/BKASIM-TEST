import React, { useState, useEffect } from "react";
import { Page, Box } from "zmp-ui";
import { useNavigate, useParams } from "react-router-dom";
import useSetHeader from "../components/hooks/useSetHeader";
import { useRecoilValue } from "recoil";
import { token } from "../recoil/RecoilState";
import axios from "axios";
import LoadingGiba from "../componentsGiba/LoadingGiba";
import { Drawer, Input } from "antd";
import { toast } from "react-toastify";
import dfData from "../common/DefaultConfig.json";
const { TextArea } = Input;

interface CustomField {
  fieldId: string;
  fieldName: string;
  fieldType: string;
  fieldTypeText: string;
  isRequired: boolean;
  options: string[] | null;
  tabId: string;
  displayOrder: number;
  value: string;
  hasValue: boolean;
}

interface CustomFieldTab {
  tabId: string;
  tabName: string;
  fields: CustomField[];
}

interface GroupInfo {
  id: string;
  groupName: string;
  description: string;
  rule: string | null;
  logo: string | null;
  isActive: boolean;
  memberCount: number;
  createdDate: string;
}

interface MembershipDetail {
  membershipGroupId: string;
  reason: string;
  company: string | null;
  position: string | null;
  groupPosition: string | null;
  isApproved: boolean;
  statusText: string;
  rejectReason: string | null;
  approvedDate: string | null;
  joinRequestDate: string;
  hasCustomFieldsSubmitted: boolean;
  canEdit: boolean;
  group: GroupInfo;
  customFieldsByTab: CustomFieldTab[];
}

const GroupJoinRequestDetail: React.FC = () => {
  const navigate = useNavigate();
  const { groupId } = useParams<{ groupId: string }>();
  const setHeader = useSetHeader();
  const userToken = useRecoilValue(token);

  const [loading, setLoading] = useState(true);
  const [detail, setDetail] = useState<MembershipDetail | null>(null);
  const [showEditDrawer, setShowEditDrawer] = useState(false);
  const [editForm, setEditForm] = useState({
    reason: "",
    company: "",
    position: "",
  });
  const [updating, setUpdating] = useState(false);

  useEffect(() => {
    setHeader({
      title: "CHI TIẾT ĐƠN THAM GIA",
      showUserInfo: false,
      showMenuButton: false,
      showCloseButton: false,
      hasLeftIcon: true,
    });
  }, [setHeader]);

  useEffect(() => {
    const fetchDetail = async () => {
      if (!userToken || !groupId) return;

      try {
        setLoading(true);
        const response = await axios.get(
          `${dfData.domain}/api/Groups/my-membership/${groupId}`,
          {
            headers: {
              Authorization: `Bearer ${userToken}`,
            },
          }
        );

        if (response.data.success && response.data.data) {
          setDetail(response.data.data);
          setEditForm({
            reason: response.data.data.reason || "",
            company: response.data.data.company || "",
            position: response.data.data.position || "",
          });
        } else {
          toast.error("Không thể tải chi tiết đơn tham gia");
          navigate("/giba/group-join-request-history");
        }
      } catch (error) {
        console.error("Error fetching detail:", error);
        toast.error("Có lỗi xảy ra khi tải chi tiết");
        navigate("/giba/group-join-request-history");
      } finally {
        setLoading(false);
      }
    };

    fetchDetail();
  }, [userToken, groupId, navigate]);

  const getStatusColor = (statusText: string) => {
    if (statusText === "Chờ xét duyệt") {
      return "bg-yellow-100 text-yellow-700";
    } else if (statusText === "Đã duyệt") {
      return "bg-green-100 text-green-700";
    } else if (statusText === "Từ chối") {
      return "bg-red-100 text-red-700";
    }
    return "bg-gray-100 text-gray-700";
  };

  const formatDate = (dateString: string | null) => {
    if (!dateString) return "N/A";
    const date = new Date(dateString);
    return date.toLocaleDateString("vi-VN", {
      year: "numeric",
      month: "2-digit",
      day: "2-digit",
      hour: "2-digit",
      minute: "2-digit",
    });
  };

  const formatFieldValue = (value: string, fieldType: string) => {
    if (!value) return "N/A";

    if (fieldType === "Boolean") {
      return value === "true" || value === "1" ? "Có" : "Không";
    }

    return value;
  };

  const handleEditClick = () => {
    setShowEditDrawer(true);
  };

  const handleUpdateRequest = async () => {
    if (!editForm.reason.trim()) {
      toast.error("Vui lòng nhập lý do tham gia");
      return;
    }
    if (!editForm.company.trim()) {
      toast.error("Vui lòng nhập tên công ty");
      return;
    }
    if (!editForm.position.trim()) {
      toast.error("Vui lòng nhập chức vụ");
      return;
    }

    try {
      setUpdating(true);
      const response = await axios.put(
        `${dfData.domain}/api/groups/my-join-requests/${detail?.membershipGroupId}`,
        {
          reason: editForm.reason.trim(),
          company: editForm.company.trim(),
          position: editForm.position.trim(),
        },
        {
          headers: {
            Authorization: `Bearer ${userToken}`,
          },
        }
      );

      if (response.data.code === 0 || response.data.success) {
        toast.success("Cập nhật thông tin thành công");
        setShowEditDrawer(false);
        // Refresh detail
        if (detail) {
          setDetail({
            ...detail,
            reason: editForm.reason,
            company: editForm.company,
            position: editForm.position,
          });
        }
      } else {
        toast.error(response.data.message || "Không thể cập nhật thông tin");
      }
    } catch (error) {
      console.error("Error updating request:", error);
      toast.error("Có lỗi xảy ra khi cập nhật thông tin");
    } finally {
      setUpdating(false);
    }
  };

  const handleCancelRequest = async () => {
    if (!window.confirm("Bạn chắc chắn muốn hủy đơn xin tham gia này?")) {
      return;
    }

    try {
      setUpdating(true);
      const response = await axios.delete(
        `${dfData.domain}/api/Groups/my-join-requests/${detail?.membershipGroupId}`,
        {
          headers: {
            Authorization: `Bearer ${userToken}`,
          },
        }
      );

      if (response.data.success) {
        toast.success("Hủy đơn xin tham gia thành công");
        navigate("/giba/group-join-request-history");
      } else {
        toast.error(response.data.message || "Không thể hủy đơn xin tham gia");
      }
    } catch (error) {
      console.error("Error canceling request:", error);
      toast.error("Có lỗi xảy ra khi hủy đơn xin tham gia");
    } finally {
      setUpdating(false);
    }
  };

  if (loading) {
    return (
      <Page style={{ marginTop: "50px", background: "#f8fafc" }}>
        <div className="flex justify-center items-center py-16">
          <LoadingGiba size="lg" text="Đang tải chi tiết..." />
        </div>
      </Page>
    );
  }

  if (!detail) {
    return (
      <Page style={{ marginTop: "50px", background: "#f8fafc" }}>
        <div className="text-center py-20">
          <div className="text-gray-500 text-lg">Không tìm thấy thông tin</div>
        </div>
      </Page>
    );
  }

  return (
    <Page style={{ marginTop: "50px", background: "#f8fafc" }}>
      <div className="p-4 space-y-4">
        {/* Group Info Card */}
        <div className="bg-white rounded-lg shadow-md border border-gray-200 p-4">
          <div className="flex items-start gap-4 mb-4">
            {detail.group.logo && (
              <img
                src={detail.group.logo}
                alt={detail.group.groupName}
                className="w-16 h-16 rounded-lg object-cover"
              />
            )}
            <div className="flex-1">
              <h2 className="text-lg font-bold text-gray-900 mb-1">
                {detail.group.groupName}
              </h2>
              <span
                className={`inline-block px-3 py-1 rounded-full text-sm font-medium ${getStatusColor(
                  detail.statusText
                )}`}
              >
                {detail.statusText}
              </span>
            </div>
          </div>
          {/* <div className="text-sm text-gray-600">
            {detail.group.memberCount} thành viên
          </div> */}
        </div>

        {/* Request Info */}
        <div className="bg-white rounded-lg shadow-md border border-gray-200 p-4 space-y-4">
          {/* <div>
            <label className="block text-sm font-semibold mb-2 text-gray-600">
              Công ty
            </label>
            <div className="text-base text-gray-900">
              {detail.company || "N/A"}
            </div>
          </div>

          <div>
            <label className="block text-sm font-semibold mb-2 text-gray-600">
              Chức vụ
            </label>
            <div className="text-base text-gray-900">
              {detail.position || "N/A"}
            </div>
          </div> */}

          <div>
            <label className="block text-sm font-semibold mb-2 text-gray-600">
              Lý do tham gia
            </label>
            <div className="bg-blue-50 border-l-2 border-blue-400 p-3 rounded-r text-sm text-gray-900 whitespace-pre-wrap">
              {detail.reason}
            </div>
          </div>

          {detail.rejectReason && (
            <div>
              <label className="block text-sm font-semibold mb-2 text-gray-600">
                Lý do từ chối
              </label>
              <div className="bg-red-50 border-l-2 border-red-400 p-3 rounded-r text-sm text-gray-900 whitespace-pre-wrap">
                {detail.rejectReason}
              </div>
            </div>
          )}

          <div className="grid grid-cols-2 gap-4 pt-2">
            <div>
              <label className="block text-xs font-semibold mb-1 text-gray-600">
                Ngày gửi
              </label>
              <div className="text-sm text-gray-900">
                {formatDate(detail.joinRequestDate)}
              </div>
            </div>
            {detail.approvedDate && (
              <div>
                <label className="block text-xs font-semibold mb-1 text-gray-600">
                  Ngày duyệt
                </label>
                <div className="text-sm text-gray-900">
                  {formatDate(detail.approvedDate)}
                </div>
              </div>
            )}
          </div>
        </div>

        {/* Custom Fields */}
        {detail.customFieldsByTab && detail.customFieldsByTab.length > 0 && (
          <div className="bg-white rounded-lg shadow-md border border-gray-200 p-4 space-y-4">
            <h3 className="font-semibold text-gray-900">Thông tin bổ sung</h3>
            {detail.customFieldsByTab.map((tab) => (
              <div key={tab.tabId} className="space-y-3">
                {tab.fields.map((field) => (
                  <div key={field.fieldId}>
                    <label className="block text-sm font-semibold mb-1 text-gray-600">
                      {field.fieldName}
                      {field.isRequired && (
                        <span className="text-red-500">*</span>
                      )}
                    </label>
                    <div className="text-sm text-gray-900 bg-gray-50 p-2 rounded">
                      {field.hasValue
                        ? formatFieldValue(field.value, field.fieldType)
                        : "N/A"}
                    </div>
                  </div>
                ))}
              </div>
            ))}
          </div>
        )}

        {/* Action Buttons */}
        <div className="bg-white rounded-lg shadow-md border border-gray-200 p-4 space-y-2">
          {detail.canEdit && (
            <button
              onClick={handleEditClick}
              className="w-full bg-blue-600 text-white py-3 px-4 rounded-lg text-sm font-medium hover:bg-blue-700 transition-colors"
            >
              Chỉnh sửa thông tin
            </button>
          )}
          {detail.statusText === "Chờ xét duyệt" && (
            <button
              onClick={handleCancelRequest}
              disabled={updating}
              className="w-full bg-red-600 text-white py-3 px-4 rounded-lg text-sm font-medium hover:bg-red-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {updating ? "Đang hủy..." : "Hủy đơn"}
            </button>
          )}
        </div>

        <div className="pb-20"></div>
      </div>

      {/* Edit Drawer */}
      <Drawer
        title="Chỉnh sửa thông tin đơn xin tham gia"
        placement="bottom"
        style={{ borderRadius: "15px 15px 0 0" }}
        onClose={() => setShowEditDrawer(false)}
        open={showEditDrawer}
        height="auto"
      >
        <div className="space-y-4">
          <div>
            <label className="block text-sm font-semibold mb-2 text-black">
              Tên nhóm
            </label>
            <Input
              value={detail.group.groupName}
              disabled
              style={{ height: 43, background: "#f5f5f5" }}
            />
          </div>

          <div>
            <label className="block text-sm font-semibold mb-2 text-black">
              Công ty <span className="text-red-500">*</span>
            </label>
            <Input
              placeholder="Nhập tên công ty"
              value={editForm.company}
              onChange={(e) =>
                setEditForm({ ...editForm, company: e.target.value })
              }
              style={{ height: 43 }}
              maxLength={200}
            />
          </div>

          <div>
            <label className="block text-sm font-semibold mb-2 text-black">
              Chức vụ <span className="text-red-500">*</span>
            </label>
            <Input
              placeholder="Nhập chức vụ"
              value={editForm.position}
              onChange={(e) =>
                setEditForm({ ...editForm, position: e.target.value })
              }
              style={{ height: 43 }}
              maxLength={100}
            />
          </div>

          <div>
            <label className="block text-sm font-semibold mb-2 text-black">
              Lý do tham gia <span className="text-red-500">*</span>
            </label>
            <TextArea
              rows={4}
              placeholder="Nhập lý do bạn muốn tham gia nhóm này..."
              value={editForm.reason}
              onChange={(e) =>
                setEditForm({ ...editForm, reason: e.target.value })
              }
              maxLength={500}
              showCount
            />
          </div>

          <div className="flex gap-2">
            <button
              onClick={() => setShowEditDrawer(false)}
              style={{
                flex: 1,
                padding: "10px",
                background: "#f5f5f5",
                border: "1px solid #d1d5db",
                borderRadius: "6px",
                color: "#000",
                fontSize: "14px",
                fontWeight: "600",
              }}
              disabled={updating}
            >
              Đóng
            </button>
            <button
              onClick={handleUpdateRequest}
              style={{
                flex: 1,
                padding: "10px",
                background: "#003d82",
                border: "none",
                borderRadius: "6px",
                color: "#fff",
                fontSize: "14px",
                fontWeight: "600",
              }}
              disabled={updating}
            >
              {updating ? "Đang cập nhật..." : "Lưu thay đổi"}
            </button>
          </div>
        </div>
      </Drawer>
    </Page>
  );
};

export default GroupJoinRequestDetail;
