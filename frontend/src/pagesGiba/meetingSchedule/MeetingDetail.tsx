import React, { useState, useEffect } from "react";
import { Page } from "zmp-ui";
import { useLocation, useNavigate } from "react-router-dom";
import { useRecoilValue } from "recoil";
import { token } from "../../recoil/RecoilState";
import { Modal } from "antd";
import useSetHeader from "../../components/hooks/useSetHeader";
import LoadingGiba from "../../componentsGiba/LoadingGiba";
import {
  Calendar,
  Clock,
  MapPin,
  FileText,
  Edit,
  Trash2,
  Video,
  Link as LinkIcon,
  Globe,
  Lock,
} from "lucide-react";
import axios from "axios";
import dfData from "../../common/DefaultConfig.json";
import { toast } from "react-toastify";
import { useHasRole } from "../../hooks/useHasRole";
import {
  formatDateTimeLocale,
  formatFullDateTime,
} from "../../utils/dateFormatter";

interface MeetingDetail {
  id: string;
  groupId: string;
  groupName: string;
  title: string;
  description: string;
  startDate: string;
  endDate: string | null;
  meetingType: number;
  location: string;
  meetingLink: string;
  isPublic: boolean;
  roleId: string;
  createdBy: string;
  createdAt: string;
  updatedAt: string;
}

const MeetingDetail: React.FC = () => {
  const location = useLocation();
  const navigate = useNavigate();
  const setHeader = useSetHeader();
  const userToken = useRecoilValue(token);
  const hasRole = useHasRole();

  const meetingFromState = location.state?.meeting;
  const meetingId = meetingFromState?.id;
  const isAdminMode = location.state?.isAdminMode || false;

  const [meeting, setMeeting] = useState<MeetingDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [deleting, setDeleting] = useState(false);
  const [showDeleteModal, setShowDeleteModal] = useState(false);

  useEffect(() => {
    setHeader({
      title: "CHI TIẾT LỊCH HỌP",
      showUserInfo: false,
      showMenuButton: false,
      showCloseButton: false,
      hasLeftIcon: true,
    });
  }, [setHeader]);

  useEffect(() => {
    const fetchMeetingDetail = async () => {
      if (!meetingId) {
        setLoading(false);
        return;
      }

      try {
        setLoading(true);
        const response = await axios.get(
          `${dfData.domain}/api/Meeting/Detail/${meetingId}`,
          {
            headers: {
              Authorization: `Bearer ${userToken}`,
            },
          }
        );

        if (response.data.code === 0 && response.data.item) {
          setMeeting(response.data.item);
        } else {
          toast.error("Không thể tải chi tiết lịch họp");
          navigate(-1);
        }
      } catch (error) {
        console.error("Error fetching meeting detail:", error);
        toast.error("Có lỗi xảy ra khi tải chi tiết lịch họp");
        navigate(-1);
      } finally {
        setLoading(false);
      }
    };

    fetchMeetingDetail();
  }, [meetingId, userToken, navigate]);

  const handleDelete = async () => {
    if (!meetingId || !userToken) return;

    try {
      setDeleting(true);
      const response = await axios.delete(
        `${dfData.domain}/api/Meeting/${meetingId}`,
        {
          headers: {
            Authorization: `Bearer ${userToken}`,
          },
        }
      );

      if (response.data.code === 0) {
        toast.success("Đã xóa lịch họp thành công!");
        navigate("/giba/meeting-list", { replace: true });
      } else {
        toast.error(response.data.message || "Không thể xóa lịch họp");
      }
    } catch (error: any) {
      console.error("Error deleting meeting:", error);
      toast.error(
        error.response?.data?.message || "Có lỗi xảy ra khi xóa lịch họp"
      );
    } finally {
      setDeleting(false);
      setShowDeleteModal(false);
    }
  };

  const handleEdit = () => {
    navigate("/giba/meeting-create", {
      state: { meeting },
      replace: true,
    });
  };

  // Using utility functions from dateFormatter

  if (loading) {
    return (
      <Page className="bg-white min-h-screen mt-[50px]">
        <div className="flex justify-center items-center h-64">
          <LoadingGiba size="lg" text="Đang tải chi tiết lịch họp..." />
        </div>
      </Page>
    );
  }

  if (!meeting) {
    return (
      <Page style={{ marginTop: "50px" }}>
        <div className="flex justify-center items-center min-h-64 p-8">
          <div className="text-center">
            <div className="w-16 h-16 bg-gray-100 rounded-full flex items-center justify-center mx-auto mb-4">
              <svg
                className="w-8 h-8 text-gray-400"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16.5c-.77.833.192 2.5 1.732 2.5z"
                />
              </svg>
            </div>
            <div className="text-gray-600 text-lg font-medium">
              Không tìm thấy thông tin lịch họp
            </div>
          </div>
        </div>
      </Page>
    );
  }

  const startDateTime = formatFullDateTime(meeting.startDate);
  const endDateTime = meeting.endDate
    ? formatFullDateTime(meeting.endDate)
    : null;
  const isSameDay =
    endDateTime && startDateTime.shortDate === endDateTime.shortDate;

  return (
    <Page style={{ marginTop: "50px", background: "#f8fafc" }}>
      <div className={isAdminMode ? "pb-24" : ""}>
        <div className="bg-white rounded-lg shadow-sm p-4">
          <div className="flex items-center justify-between mb-4">
            <div>
              <p className="text-purple-600 text-sm font-semibold">
                {meeting.groupName}
              </p>
              <p className="text-gray-500 text-xs mt-1">
                ID: {meeting.id.slice(-8).toUpperCase()}
              </p>
            </div>
            <div className="text-right">
              <div
                className={`inline-flex items-center px-3 py-1.5 rounded-full text-sm font-semibold border ${
                  meeting.isPublic
                    ? "bg-green-100 text-green-700 border-green-300"
                    : "bg-gray-100 text-gray-700 border-gray-300"
                }`}
              >
                {meeting.isPublic ? (
                  <>
                    <Globe size={14} className="mr-1" />
                    Công khai
                  </>
                ) : (
                  <>
                    <Lock size={14} className="mr-1" />
                    Nội bộ
                  </>
                )}
              </div>
            </div>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow-sm p-4">
          <h2 className="text-lg font-bold text-gray-900 mb-4 flex items-center gap-2">
            <FileText size={20} />
            Chủ đề
          </h2>

          <div className="bg-yellow-50 rounded-lg p-4 border-l-4 border-yellow-500">
            <div className="text-lg font-bold text-gray-900 mb-2">
              {meeting.title}
            </div>
            {meeting.description && (
              <div className="text-sm text-gray-700 leading-relaxed whitespace-pre-line">
                {meeting.description}
              </div>
            )}
          </div>
        </div>

        <div className="bg-white rounded-lg shadow-sm p-4">
          <h2 className="text-lg font-bold text-gray-900 mb-4 flex items-center gap-2">
            <Calendar size={20} />
            Thời gian
          </h2>

          <div className="bg-blue-50 rounded-lg p-4 border-l-4 border-blue-500">
            <div className="text-sm font-bold text-blue-900">
              {startDateTime.fullDate}
            </div>
            <div className="text-base font-bold text-blue-900 mt-1 flex items-center gap-1">
              <Clock size={16} />
              {startDateTime.time}
              {endDateTime && isSameDay && (
                <span className="text-blue-700 font-normal ml-2">
                  - {endDateTime.time}
                </span>
              )}
            </div>
            {endDateTime && !isSameDay && (
              <div className="mt-3 pt-3 border-t border-blue-200">
                <div className="text-sm font-bold text-blue-900">
                  {endDateTime.fullDate}
                </div>
                <div className="text-base font-bold text-blue-900 mt-1 flex items-center gap-1">
                  <Clock size={16} />
                  {endDateTime.time}
                </div>
              </div>
            )}
          </div>
        </div>

        <div className="bg-white rounded-lg shadow-sm p-4 mb-4">
          <h2 className="text-lg font-bold text-gray-900 mb-4 flex items-center gap-2">
            {meeting.meetingType === 1 ? (
              <>
                <Video size={20} className="text-purple-500" />
                Hình thức: Online
              </>
            ) : (
              <>
                <MapPin size={20} className="text-red-500" />
                Hình thức: Offline
              </>
            )}
          </h2>

          {meeting.meetingType === 1 ? (
            <div className="bg-purple-50 rounded-lg p-4 border-l-4 border-purple-500">
              <div className="text-xs text-purple-600 mb-2 flex items-center gap-1">
                <LinkIcon size={14} />
                Link meeting
              </div>
              <a
                href={meeting.meetingLink}
                target="_blank"
                rel="noopener noreferrer"
                className="text-sm text-purple-700 font-medium hover:underline break-all"
              >
                {meeting.meetingLink}
              </a>
            </div>
          ) : (
            <div className="bg-red-50 rounded-lg p-4 border-l-4 border-red-500">
              <div className="text-gray-800 leading-relaxed text-sm font-medium">
                📍 {meeting.location}
              </div>
            </div>
          )}
        </div>
      </div>

      {isAdminMode && (
        <div className="fixed bottom-0 left-0 right-0 bg-white border-t border-gray-200 p-4 flex gap-3 shadow-lg z-50">
          <button
            onClick={() => setShowDeleteModal(true)}
            className="flex-1 px-4 py-2 bg-white border border-red-500 text-red-600 rounded-lg font-semibold text-base transition-all hover:bg-red-50 disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2"
            disabled={deleting}
          >
            <Trash2 size={20} />
            Xóa lịch
          </button>
          <button
            onClick={handleEdit}
            className="flex-1 px-4 py-2 bg-black text-white rounded-lg font-semibold text-base transition-all hover:bg-gray-800 disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2"
            disabled={deleting}
          >
            <Edit size={20} />
            Chỉnh sửa
          </button>
        </div>
      )}

      <Modal
        title="Xác nhận xóa lịch họp"
        open={showDeleteModal}
        onOk={handleDelete}
        onCancel={() => setShowDeleteModal(false)}
        okText="Xác nhận xóa"
        cancelText="Quay lại"
        okButtonProps={{
          danger: true,
          loading: deleting,
        }}
        cancelButtonProps={{
          disabled: deleting,
        }}
      >
        <p className="text-gray-700">
          Bạn có chắc chắn muốn xóa lịch họp này không?
        </p>
        <p className="text-red-600 text-sm mt-2 font-semibold">
          Hành động này không thể hoàn tác!
        </p>
      </Modal>
    </Page>
  );
};

export default MeetingDetail;
