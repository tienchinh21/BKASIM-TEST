import React, { useState } from "react";
import { Page } from "zmp-ui";
import { useLocation, useNavigate } from "react-router-dom";
import useSetHeader from "../../components/hooks/useSetHeader";
import {
  Calendar,
  Clock,
  MapPin,
  FileText,
  Users,
  MessageCircle,
  X,
  Check,
  Edit,
} from "lucide-react";
import { toast } from "react-toastify";
import RejectAppointmentModal from "./RejectAppointmentModal";
import { useRecoilValue } from "recoil";
import { infoUser, userMembershipInfo, token } from "../../recoil/RecoilState";
import {
  AppointmentStatus,
  AppointmentStatusLabel,
  AppointmentStatusColor,
  AppointmentStatusGradient,
} from "../../utils/enum/appointment.enum";
import axios from "axios";
import dfData from "../../common/DefaultConfig.json";
import { formatDateTimeLocale, formatDate } from "../../utils/dateFormatter";
import { getAppointmentStatus } from "../../utils/statusHelpers";

interface AppointmentItem {
  id: string;
  name: string;
  appointmentFromId: string;
  appointmentToId: string;
  appointmentFromName: string;
  appointmentToName: string;
  appointmentFromAvatar: string;
  appointmentToAvatar: string;
  groupId: string;
  groupName: string;
  content: string;
  location: string;
  time: string;
  status: number;
  statusText: string;
  cancelReason: string | null;
  createdDate: string;
  updatedDate: string;
}

const AppointmentDetail: React.FC = () => {
  const location = useLocation();
  const navigate = useNavigate();
  const setHeader = useSetHeader();
  const appointment = location.state?.appointment as AppointmentItem;
  const userToken = useRecoilValue(token);
  const [loading, setLoading] = useState(false);
  const [showRejectModal, setShowRejectModal] = useState(false);

  const userInfo = useRecoilValue(infoUser);
  const membershipInfo = useRecoilValue(userMembershipInfo);
  //@ts-ignore
  const currentUserZaloId = membershipInfo?.userZaloId || userInfo?.userZaloId;

  React.useEffect(() => {
    setHeader({
      title: "CHI TIẾT LỊCH HẸN",
      showUserInfo: false,
      showMenuButton: false,
      showCloseButton: false,
      hasLeftIcon: true,
    });
  }, [setHeader]);

  // Using utility functions from statusHelpers and dateFormatter
  const {
    color: statusColor,
    gradient: statusGradient,
    text: statusText,
  } = getAppointmentStatus(appointment?.status || 0);

  const getStatusColor = (status: number) => getAppointmentStatus(status).color;
  const getStatusGradient = (status: number) =>
    getAppointmentStatus(status).gradient;
  const getStatusText = (status: number) => getAppointmentStatus(status).text;

  const handleConfirmAppointment = async () => {
    try {
      setLoading(true);

      const payload = {
        id: appointment.id,
        status: AppointmentStatus.CONFIRMED,
      };

      const response = await axios.patch(
        `${dfData.domain}/api/Appointment/${appointment.id}/status`,
        payload,
        {
          headers: {
            Authorization: `Bearer ${userToken}`,
            "Content-Type": "application/json",
          },
        }
      );

      if (response.data.success || response.data.code === 0) {
        toast.success("Đã xác nhận lịch hẹn thành công!");
        navigate(-1);
      } else {
        toast.error(response.data.message || "Có lỗi xảy ra");
      }
    } catch (error: any) {
      console.error("Error confirming appointment:", error);
      toast.error(
        error.response?.data?.message || "Có lỗi xảy ra khi xác nhận"
      );
    } finally {
      setLoading(false);
    }
  };

  const handleRejectAppointment = async (reason: string) => {
    try {
      setLoading(true);

      const payload = {
        id: appointment.id,
        status: AppointmentStatus.CANCELLED,
        cancelReason: reason,
      };

      const response = await axios.patch(
        `${dfData.domain}/api/Appointment/${appointment.id}/status`,
        payload,
        {
          headers: {
            Authorization: `Bearer ${userToken}`,
            "Content-Type": "application/json",
          },
        }
      );

      if (response.data.success || response.data.code === 0) {
        toast.success(
          isSender ? "Đã huỷ đơn thành công" : "Đã từ chối lịch hẹn"
        );
        setShowRejectModal(false);
        navigate(-1);
      } else {
        toast.error(response.data.message || "Có lỗi xảy ra");
      }
    } catch (error: any) {
      console.error("Error rejecting appointment:", error);
      toast.error(error.response?.data?.message || "Có lỗi xảy ra khi huỷ đơn");
    } finally {
      setLoading(false);
      setShowRejectModal(false);
    }
  };

  const handleEditAppointment = () => {
    navigate("/giba/appointment-create", {
      state: { appointment },
    });
  };

  if (!appointment) {
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
            <div className="text-gray-600 text-base font-medium">
              Không tìm thấy thông tin lịch hẹn
            </div>
          </div>
        </div>
      </Page>
    );
  }

  const isSender = appointment.appointmentFromId === currentUserZaloId;
  const isReceiver = appointment.appointmentToId === currentUserZaloId;
  const canEdit = isSender && appointment.status === AppointmentStatus.PENDING;
  const showConfirmRejectButtons =
    isReceiver && appointment.status === AppointmentStatus.PENDING;

  return (
    <Page
      style={{
        marginTop: "50px",
        background: "#f8fafc",
        paddingBottom: canEdit || showConfirmRejectButtons ? "80px" : "0",
      }}
    >
      <div className="bg-white rounded-lg shadow-sm p-4 mb-3">
        <div className="flex items-center justify-between mb-4">
          <div>
            <p className="text-gray-600 text-base font-medium">
              ID: {appointment.id.slice(-8).toUpperCase()}
            </p>
          </div>
          <div className="text-right">
            <div
              className={`inline-flex items-center px-3 py-1.5 rounded-full text-base font-semibold ${getStatusColor(
                appointment.status
              )}`}
            >
              {getStatusText(appointment.status)}
              {appointment.status === 1 && (
                <div className="w-2 h-2 bg-amber-400 rounded-full animate-pulse ml-2"></div>
              )}
            </div>
          </div>
        </div>

        {/* Document Info */}
        <div className="grid grid-cols-2 gap-4">
          <div>
            <div className="text-sm text-gray-500 mb-1 flex items-center gap-1">
              <Calendar size={22} />
              Ngày tạo
            </div>
            <div className="font-medium text-gray-900 text-base">
              {formatDate(appointment.createdDate)}
            </div>
          </div>
          {appointment.updatedDate &&
            appointment.updatedDate !== appointment.createdDate && (
              <div>
                <div className="text-sm text-gray-500 mb-1 flex items-center gap-1">
                  <Clock size={16} />
                  Cập nhật
                </div>
                <div className="font-medium text-gray-900 text-base">
                  {formatDate(appointment.updatedDate)}
                </div>
              </div>
            )}
        </div>
      </div>

      {/* Appointment Time */}
      <div className="bg-white rounded-lg shadow-sm p-4 mb-3">
        <h2 className="text-lg font-bold text-gray-900 mb-4 flex items-center gap-2">
          <Calendar size={22} />
          Thời gian lịch hẹn
        </h2>

        <div className="bg-blue-50 rounded-lg p-4 border-l-4 border-blue-500">
          <div className="text-sm text-blue-600 mb-2 flex items-center gap-1 font-medium">
            <Clock size={16} />
            Thời gian
          </div>
          <div className="text-base font-bold text-blue-900">
            {formatDateTimeLocale(appointment.time)}
          </div>
        </div>
      </div>

      {/* Location Section */}
      <div className="bg-white rounded-lg shadow-sm p-4 mb-3">
        <h2 className="text-lg font-bold text-gray-900 mb-4 flex items-center gap-2">
          <MapPin size={22} className="text-red-500" />
          Địa điểm
        </h2>

        <div className="bg-red-50 rounded-lg p-4 border-l-4 border-red-500">
          <div className="text-gray-800 leading-relaxed text-base font-medium">
            📍 {appointment.location}
          </div>
        </div>
      </div>

      {/* Appointment Info */}
      <div className="bg-white rounded-lg shadow-sm p-4 mb-3">
        <h2 className="text-lg font-bold text-gray-900 mb-4 flex items-center gap-2">
          <Users size={22} />
          Thông tin cuộc hẹn
        </h2>

        {/* Group Info */}
        <div className="bg-indigo-50 rounded-lg p-4 border-l-4 border-indigo-500 mb-3">
          <div className="text-sm text-indigo-600 mb-2 flex items-center gap-1 font-medium">
            <Users size={16} />
            Nhóm
          </div>
          <div className="text-base font-bold text-indigo-900">
            {appointment.groupName}
          </div>
        </div>

        {/* Participants Info */}
        <div className="flex flex-col gap-3">
          {/* From */}
          <div className="bg-green-50 rounded-lg p-3 border-l-4 border-green-500">
            <div className="text-xs text-green-600 mb-2 font-medium">Từ</div>
            <div className="flex items-center gap-2">
              <img
                src={appointment.appointmentFromAvatar}
                alt={appointment.appointmentFromName}
                className="w-8 h-8 rounded-full object-cover"
              />
              <div className="text-sm font-semibold text-green-900 truncate">
                {appointment.appointmentFromName}
              </div>
            </div>
          </div>

          {/* To */}
          <div className="bg-orange-50 rounded-lg p-3 border-l-4 border-orange-500">
            <div className="text-xs text-orange-600 mb-2 font-medium">Đến</div>
            <div className="flex items-center gap-2">
              <img
                src={appointment.appointmentToAvatar}
                alt={appointment.appointmentToName}
                className="w-8 h-8 rounded-full object-cover"
              />
              <div className="text-sm font-semibold text-orange-900 truncate">
                {appointment.appointmentToName}
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Content Section */}
      <div className="bg-white rounded-lg shadow-sm p-4 mb-3">
        <h2 className="text-lg font-bold text-gray-900 mb-4 flex items-center gap-2">
          <FileText size={22} />
          Nội dung cuộc hẹn
        </h2>

        <div className="bg-purple-50 rounded-lg p-4 border-l-4 border-purple-500">
          <div className="text-gray-800 leading-relaxed whitespace-pre-line text-base font-medium">
            {appointment.content}
          </div>
        </div>
      </div>

      {/* Cancel Reason Section */}
      {appointment.status === AppointmentStatus.CANCELLED &&
        appointment.cancelReason && (
          <div className="bg-white rounded-lg shadow-sm p-4 mb-3">
            <h2 className="text-lg font-bold text-gray-900 mb-4 flex items-center gap-2">
              <X size={22} className="text-red-500" />
              Lý do hủy
            </h2>

            <div className="bg-red-50 rounded-lg p-4 border-l-4 border-red-500">
              <div className="flex items-center gap-2 mb-2">
                <MessageCircle size={18} className="text-red-600" />
                <span className="text-base font-semibold text-red-800">
                  Thông báo hủy:
                </span>
              </div>
              <div className="text-red-800 leading-relaxed text-base">
                "{appointment.cancelReason}"
              </div>
            </div>
          </div>
        )}

      {/* Action Buttons - Edit and Cancel buttons for sender with pending status */}
      {canEdit && (
        <div className="fixed bottom-0 left-0 right-0 bg-white border-t-2 border-gray-200 p-4 flex gap-3 shadow-2xl z-50">
          <button
            onClick={() => setShowRejectModal(true)}
            className="flex-1 px-4 py-3 bg-red-500 hover:bg-red-600 text-white rounded-lg font-semibold transition-all disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2"
            disabled={loading}
          >
            <X size={20} />
            Huỷ lịch hẹn
          </button>
          <button
            onClick={handleEditAppointment}
            className="flex-1 px-4 py-3 bg-yellow-400 hover:bg-yellow-500 text-black rounded-lg font-semibold transition-all flex items-center justify-center gap-2"
          >
            <Edit size={20} />
            Chỉnh sửa
          </button>
        </div>
      )}

      {/* Action Buttons - Confirm/Reject buttons for receiver with pending status */}
      {showConfirmRejectButtons && (
        <div className="fixed bottom-0 left-0 right-0 bg-white border-t-2 border-gray-200 p-4 flex gap-3 shadow-2xl z-50">
          <button
            onClick={() => setShowRejectModal(true)}
            className="flex-1 px-4 py-3 bg-red-500 hover:bg-red-600 text-white rounded-lg font-semibold transition-all disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2"
            disabled={loading}
          >
            <X size={20} />
            Từ chối
          </button>
          <button
            onClick={handleConfirmAppointment}
            className="flex-1 px-4 py-3 bg-green-500 hover:bg-green-600 text-white rounded-lg font-semibold transition-all disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2"
            disabled={loading}
          >
            <Check size={20} />
            {loading ? "Đang xử lý..." : "Xác nhận"}
          </button>
        </div>
      )}

      {/* Reject Reason Modal */}
      <RejectAppointmentModal
        visible={showRejectModal}
        onClose={() => setShowRejectModal(false)}
        onConfirm={handleRejectAppointment}
        appointmentId={appointment.id}
        isSender={isSender}
      />
    </Page>
  );
};

export default AppointmentDetail;
