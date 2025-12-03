import React, { useState, useCallback, useMemo } from "react";
import { Page, Box } from "zmp-ui";
import { useNavigate } from "zmp-ui";
import { useLocation } from "react-router-dom";
import useSetHeader from "../../components/hooks/useSetHeader";
import LoadingGiba from "../../componentsGiba/LoadingGiba";
import { Plus, Calendar, MapPin, Check, X } from "lucide-react";
import FloatingActionButtonGiba from "../../componentsGiba/FloatingActionButtonGiba";
import { toast } from "react-toastify";
import RejectAppointmentModal from "./RejectAppointmentModal";
import TwoTierTab from "../../components/TwoTierTab";
import axios from "axios";
import dfData from "../../common/DefaultConfig.json";
import { useRecoilValue } from "recoil";
import { token, userMembershipInfo, infoUser } from "../../recoil/RecoilState";
import { useHasRole } from "../../hooks/useHasRole";
import {
  AppointmentStatus,
  AppointmentStatusLabel,
  AppointmentStatusColor,
  AppointmentStatusGradient,
  AppointmentTabsData,
} from "../../utils/enum/appointment.enum";
import { formatDateTimeLocale } from "../../utils/dateFormatter";

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

const AppointmentList: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const setHeader = useSetHeader();
  const userToken = useRecoilValue(token);
  const membershipInfo = useRecoilValue(userMembershipInfo);
  const userInfo = useRecoilValue(infoUser);
  const [currentUserZaloId, setCurrentUserZaloId] = useState<string | null>(
    null
  );
  const [loading, setLoading] = useState(false);
  const [activeTab, setActiveTab] = useState<string>("all");
  const [activeStatus, setActiveStatus] = useState<string>("");
  const [appointments, setAppointments] = useState<AppointmentItem[]>([]);
  const [showRejectModal, setShowRejectModal] = useState(false);
  const [rejectingAppointmentId, setRejectingAppointmentId] = useState<
    string | null
  >(null);
  const hasRole = useHasRole();

  React.useEffect(() => {
    setHeader({
      title: "ĐẶT HẸN",
      showUserInfo: false,
      showMenuButton: false,
      showCloseButton: false,
      hasLeftIcon: true,
    });
  }, [setHeader]);

  React.useEffect(() => {
    const fetchCurrentUserProfile = async () => {
      if (!userToken) return;

      try {
        const fromRecoil = membershipInfo?.userZaloId || (userInfo as any)?.id;
        if (fromRecoil) {
          setCurrentUserZaloId(String(fromRecoil));
          return;
        }

        const response = await axios.get(
          `${dfData.domain}/api/memberships/profile`,
          {
            headers: {
              Authorization: `Bearer ${userToken}`,
            },
          }
        );

        if (response.data.success && response.data.data) {
          const profile = response.data.data;
          const userZaloId = profile.userZaloId || profile.id;
          if (userZaloId) {
            setCurrentUserZaloId(String(userZaloId));
          }
        }
      } catch (error) {
        console.error("Error fetching current user profile:", error);
        const fallback = membershipInfo?.userZaloId || (userInfo as any)?.id;
        if (fallback) {
          setCurrentUserZaloId(String(fallback));
        }
      }
    };

    fetchCurrentUserProfile();
  }, [userToken, membershipInfo, userInfo]);

  React.useEffect(() => {
    loadAppointments();
  }, [location]);

  const loadAppointments = async () => {
    try {
      setLoading(true);
      const url = `${dfData.domain}/api/Appointment`;

      const response = await axios.get(url, {
        headers: {
          Authorization: `Bearer ${userToken}`,
        },
      });

      if (response.data.success && response.data.data) {
        setAppointments(response.data.data.items || []);
      }
    } catch (error: any) {
      console.error("Error loading appointments:", error);
      toast.error("Không thể tải danh sách lịch hẹn");
    } finally {
      setLoading(false);
    }
  };

  // Filter appointments on FE
  const filteredAppointments = useMemo(() => {
    let result = [...appointments];

    // Filter by type (from/to)
    if (activeTab === "from" && currentUserZaloId) {
      result = result.filter(
        (apt) => String(apt.appointmentFromId) === String(currentUserZaloId)
      );
    } else if (activeTab === "to" && currentUserZaloId) {
      result = result.filter(
        (apt) => String(apt.appointmentToId) === String(currentUserZaloId)
      );
    }

    // Filter by status
    if (activeStatus) {
      const statusNum = parseInt(activeStatus);
      result = result.filter((apt) => apt.status === statusNum);
    }

    return result;
  }, [appointments, activeTab, activeStatus, currentUserZaloId]);

  const handleTabChange = useCallback((tabValue: string) => {
    setActiveTab(tabValue);
    setActiveStatus("");
  }, []);

  const handleStatusChange = useCallback((statusValue: string) => {
    setActiveStatus(statusValue);
  }, []);

  const getStatusColor = useCallback((status: number) => {
    return (
      AppointmentStatusColor[status as AppointmentStatus] ||
      "bg-gray-100 text-gray-700"
    );
  }, []);

  const getStatusGradient = useCallback((status: number) => {
    return (
      AppointmentStatusGradient[status as AppointmentStatus] ||
      "linear-gradient(90deg, #6b7280, #4b5563)"
    );
  }, []);

  const getStatusText = useCallback((status: number) => {
    return (
      AppointmentStatusLabel[status as AppointmentStatus] || "Không xác định"
    );
  }, []);

  const handleViewDetail = useCallback(
    (appointment: AppointmentItem) => {
      navigate("/giba/appointment-detail", {
        state: { appointment },
      });
    },
    [navigate]
  );

  const handleCreateAppointment = useCallback(() => {
    navigate("/giba/appointment-create");
  }, [navigate]);

  const handleConfirmAppointment = useCallback(
    async (appointmentId: string) => {
      try {
        const payload = {
          id: appointmentId,
          status: AppointmentStatus.CONFIRMED,
        };

        const response = await axios.patch(
          `${dfData.domain}/api/Appointment/${appointmentId}/status`,
          payload,
          {
            headers: {
              Authorization: `Bearer ${userToken}`,
              "Content-Type": "application/json",
            },
          }
        );

        if (response.data.success || response.data.code === 0) {
          toast.success("Đã xác nhận lịch hẹn!");
          loadAppointments();
        } else {
          toast.error(response.data.message || "Có lỗi xảy ra");
        }
      } catch (error: any) {
        console.error("Error confirming appointment:", error);
        toast.error(
          error.response?.data?.message || "Có lỗi xảy ra khi xác nhận"
        );
      }
    },
    [userToken]
  );

  const handleOpenRejectModal = useCallback((appointmentId: string) => {
    setRejectingAppointmentId(appointmentId);
    setShowRejectModal(true);
  }, []);

  const handleRejectAppointment = useCallback(
    async (reason: string) => {
      if (!rejectingAppointmentId) return;

      try {
        const payload = {
          id: rejectingAppointmentId,
          status: AppointmentStatus.CANCELLED,
          cancelReason: reason,
        };

        const response = await axios.patch(
          `${dfData.domain}/api/Appointment/${rejectingAppointmentId}/status`,
          payload,
          {
            headers: {
              Authorization: `Bearer ${userToken}`,
              "Content-Type": "application/json",
            },
          }
        );

        if (response.data.success || response.data.code === 0) {
          toast.success("Đã từ chối lịch hẹn");
          loadAppointments();
          setShowRejectModal(false);
          setRejectingAppointmentId(null);
        } else {
          toast.error(response.data.message || "Có lỗi xảy ra");
        }
      } catch (error: any) {
        console.error("Error rejecting appointment:", error);
        toast.error(
          error.response?.data?.message || "Có lỗi xảy ra khi từ chối"
        );
        setShowRejectModal(false);
        setRejectingAppointmentId(null);
      }
    },
    [rejectingAppointmentId, userToken]
  );

  const AppointmentCard = React.memo(
    ({
      appointment,
      filterType,
      currentUserZaloId,
      onViewDetail,
      onConfirm,
      onReject,
      getStatusColor,
      getStatusGradient,
      getStatusText,
      formatDateTime,
    }: {
      appointment: AppointmentItem;
      filterType: string;
      currentUserZaloId?: string | null;
      onViewDetail: (appointment: AppointmentItem) => void;
      onConfirm: (id: string) => void;
      onReject: (id: string) => void;
      getStatusColor: (status: number) => string;
      getStatusGradient: (status: number) => string;
      getStatusText: (status: number) => string;
      formatDateTime: (timeString: string) => string;
    }) => {
      const isSender =
        currentUserZaloId &&
        String(appointment.appointmentFromId) === String(currentUserZaloId);
      const isReceiver =
        currentUserZaloId &&
        String(appointment.appointmentToId) === String(currentUserZaloId);

      const appointmentType = filterType
        ? filterType
        : isSender
        ? "from"
        : isReceiver
        ? "to"
        : "";
      return (
        <div
          className="bg-white rounded-lg shadow-md border border-gray-200 hover:shadow-lg transition-all duration-200"
          style={{
            padding: "12px",
            position: "relative",
            overflow: "hidden",
          }}
        >
          <div
            className="absolute top-0 left-0 right-0 h-0.5"
            style={{
              background: getStatusGradient(appointment.status),
            }}
          ></div>

          <div className="flex items-center justify-between gap-2 mb-3">
            <div className="flex items-center gap-3 flex-1 min-w-0">
              {/* Avatar */}
              <div className="flex-shrink-0">
                <img
                  src={
                    appointmentType === "from"
                      ? appointment.appointmentToAvatar
                      : appointment.appointmentFromAvatar
                  }
                  alt="avatar"
                  className="w-10 h-10 rounded-full object-cover border border-gray-200"
                />
              </div>
              <div className="flex-1 min-w-0">
                {/* <div className="text-base font-bold text-gray-900 truncate">
                  {appointmentType === "from"
                    ? appointment.appointmentToName
                    : appointment.appointmentFromName}
                </div> */}
                <div className="text-sm text-gray-600 truncate">
                  {appointmentType === "from"
                    ? `Gửi đến: ${appointment.appointmentToName}`
                    : appointmentType === "to"
                    ? `Từ: ${appointment.appointmentFromName}`
                    : ""}
                </div>
                <div className="text-xs text-gray-500 truncate">
                  {appointment.groupName}
                </div>
              </div>
            </div>
            <div className="flex items-center gap-2">
              <span
                className={`px-2 py-1 rounded-full text-sm font-medium ${getStatusColor(
                  appointment.status
                )}`}
              >
                {getStatusText(appointment.status)}
              </span>
              {appointment.status === AppointmentStatus.PENDING && (
                <div className="w-2 h-2 bg-amber-400 rounded-full animate-pulse"></div>
              )}
            </div>
          </div>

          {/* Appointment Details */}
          <div className="space-y-2 mb-3">
            <div className="flex items-start gap-2 text-sm">
              <Calendar
                size={16}
                className="text-blue-600 mt-0.5 flex-shrink-0"
              />
              <span className="text-gray-700">
                {formatDateTime(appointment.time)}
              </span>
            </div>

            <div className="flex items-start gap-2 text-sm">
              <MapPin size={16} className="text-red-600 mt-0.5 flex-shrink-0" />
              <span className="text-gray-700 line-clamp-1">
                {appointment.location}
              </span>
            </div>

            <div className="bg-purple-50 border-l-2 border-purple-400 p-2 rounded-r">
              <div className="text-sm text-purple-600 font-medium mb-1">
                Nội dung
              </div>
              <div className="text-sm text-purple-800 line-clamp-2">
                {appointment.content}
              </div>
            </div>
          </div>

          {/* Action Buttons */}
          <div className="flex gap-2">
            <Box
              className="divCenter"
              style={{
                flex: 1,
                padding: "10px 0px",
                fontSize: 14,
                fontWeight: "600",
                border: "1px solid #000",
                borderRadius: "6px",
                textAlign: "center",
                cursor: "pointer",
                transition: "all 0.2s",
              }}
              onClick={() => onViewDetail(appointment)}
            >
              Xem chi tiết
            </Box>

            {appointmentType === "to" &&
              appointment.status === AppointmentStatus.PENDING && (
                <>
                  <Box
                    className="divCenter"
                    style={{
                      flex: 0.5,
                      padding: "10px 0px",
                      fontSize: 14,
                      fontWeight: "600",
                      background: "#10b981",
                      color: "#fff",
                      borderRadius: "6px",
                      textAlign: "center",
                      cursor: "pointer",
                      transition: "all 0.2s",
                    }}
                    onClick={(e) => {
                      e.stopPropagation();
                      onConfirm(appointment.id);
                    }}
                  >
                    <Check size={18} />
                  </Box>
                  <Box
                    className="divCenter"
                    style={{
                      flex: 0.5,
                      padding: "10px 0px",
                      fontSize: 14,
                      fontWeight: "600",
                      background: "#ef4444",
                      color: "#fff",
                      borderRadius: "6px",
                      textAlign: "center",
                      cursor: "pointer",
                      transition: "all 0.2s",
                    }}
                    onClick={(e) => {
                      e.stopPropagation();
                      onReject(appointment.id);
                    }}
                  >
                    <X size={18} />
                  </Box>
                </>
              )}
          </div>
        </div>
      );
    }
  );

  return (
    <Page style={{ marginTop: "50px", background: "#f5f5f5" }}>
      <TwoTierTab
        tabs={AppointmentTabsData}
        activeTab={activeTab}
        onTabChange={handleTabChange}
        activeChildTab={activeStatus}
        onChildTabChange={handleStatusChange}
      />

      <div className="p-4 space-y-3">
        {loading ? (
          <div className="flex justify-center items-center py-16">
            <LoadingGiba size="lg" text="Đang tải danh sách lịch hẹn..." />
          </div>
        ) : filteredAppointments.length > 0 ? (
          <div className="space-y-3">
            {filteredAppointments.map((appointment) => (
              <AppointmentCard
                key={appointment.id}
                appointment={appointment}
                filterType={activeTab}
                currentUserZaloId={currentUserZaloId || undefined}
                onViewDetail={handleViewDetail}
                onConfirm={handleConfirmAppointment}
                onReject={handleOpenRejectModal}
                getStatusColor={getStatusColor}
                getStatusGradient={getStatusGradient}
                getStatusText={getStatusText}
                formatDateTime={formatDateTimeLocale}
              />
            ))}
          </div>
        ) : (
          <div className="text-center py-16">
            <div className="w-20 h-20 bg-gradient-to-br from-gray-100 to-gray-200 rounded-full flex items-center justify-center mx-auto mb-4">
              <Calendar className="w-8 h-8 text-gray-400" />
            </div>
            <div className="text-gray-800 text-lg font-bold mb-2">
              Chưa có lịch hẹn nào
            </div>
            <div className="text-gray-500 text-base">
              {activeTab === "from"
                ? "Bạn chưa gửi lịch hẹn nào"
                : activeTab === "to"
                ? "Bạn chưa nhận lịch hẹn nào"
                : "Chưa có dữ liệu"}
            </div>
          </div>
        )}
      </div>

      <FloatingActionButtonGiba
        icon={<Plus />}
        onClick={handleCreateAppointment}
        position="bottom-right"
        color="yellow"
        size="md"
        tooltip="Tạo lịch hẹn mới"
      />

      <RejectAppointmentModal
        visible={showRejectModal}
        onClose={() => {
          setShowRejectModal(false);
          setRejectingAppointmentId(null);
        }}
        onConfirm={handleRejectAppointment}
        appointmentId={rejectingAppointmentId || undefined}
      />
    </Page>
  );
};

export default AppointmentList;
