import React, { useState, useEffect, useCallback } from "react";
import { Page, Box } from "zmp-ui";
import { useNavigate } from "react-router-dom";
import useSetHeader from "../components/hooks/useSetHeader";
import { useRecoilValue } from "recoil";
import { token } from "../recoil/RecoilState";
import axios from "axios";
import LoadingGiba from "../componentsGiba/LoadingGiba";
import TwoTierTab, { TabGroup } from "../components/TwoTierTab/TwoTierTab";
import FloatingActionButtonGiba from "../componentsGiba/FloatingActionButtonGiba";
import { Plus } from "lucide-react";
import dfData from "../common/DefaultConfig.json";

interface GuestList {
  id: string;
  guestName: string;
  guestPhone: string;
  guestEmail: string;
  status: number;
  statusText: string;
}

interface RegistrationItem {
  id: string;
  type: number;
  typeText: string;
  eventId: string;
  eventTitle: string;
  eventStartTime?: string;
  eventEndTime?: string;
  eventAddress?: string;
  groupId?: string;
  groupName?: string;
  name?: string;
  phoneNumber?: string;
  email?: string;
  checkInCode?: string;
  checkInStatus?: number;
  userZaloId?: string;
  note?: string;
  guestNumber?: number;
  guestLists?: GuestList[];
  status: number;
  statusText: string;
  rejectReason?: string | null;
  cancelReason?: string | null;
  createdDate: string;
  updatedDate?: string;
}

// Memoized RegistrationCard component
const RegistrationCard = React.memo<{
  registration: RegistrationItem;
  getStatusColor: (status: number) => string;
  getAccentColor: (status: number) => string;
  formatDate: (dateString: string) => string;
  onViewDetail: (registration: RegistrationItem) => void;
}>(
  ({
    registration,
    getStatusColor,
    getAccentColor,
    formatDate,
    onViewDetail,
  }) => {
    const handleClick = useCallback(() => {
      onViewDetail(registration);
    }, [registration, onViewDetail]);

    return (
      <div
        className="bg-white rounded-lg shadow-md border border-gray-200 hover:shadow-lg transition-all duration-200"
        style={{ padding: "12px", position: "relative", overflow: "hidden" }}
      >
        {/* Decorative accent line */}
        <div
          className="absolute top-0 left-0 right-0 h-0.5"
          style={{ background: getAccentColor(registration.status) }}
        ></div>

        {/* Header with Status */}
        <div className="flex items-center justify-between gap-2 mb-2">
          <div className="flex-1 min-w-0">
            <div className="text-sm font-bold text-gray-900 truncate">
              {registration.eventTitle}
            </div>
            {registration.groupName && (
              <div className="text-xs text-gray-500">
                Club: DN BKASIM - {registration.groupName}
              </div>
            )}
          </div>
          <div className="flex items-center gap-2">
            <span
              className={`px-2 py-1 rounded-full text-xs font-medium ${getStatusColor(
                registration.status
              )}`}
            >
              {registration.statusText}
            </span>
            {registration.status === 0 && (
              <div className="w-2 h-2 bg-amber-400 rounded-full animate-pulse"></div>
            )}
          </div>
        </div>

        {/* Info Row */}
        <div className="flex items-center justify-between text-xs text-gray-600 mb-2">
          <div className="flex items-center gap-1">
            <span className="bg-gray-100 px-2 py-0.5 rounded text-gray-700 font-medium">
              {registration.typeText}
            </span>
          </div>
          <div className="text-gray-500">
            {formatDate(registration.createdDate)}
          </div>
        </div>

        {/* Type 1: Personal registration info */}
        {registration.type === 1 && (
          <div className="bg-blue-50 border-l-2 border-blue-400 p-2 rounded-r mb-2">
            <div className="text-xs text-blue-800">
              <span className="font-medium">Người đăng ký:</span>{" "}
              {registration.name}
            </div>
            {registration.checkInCode && (
              <div className="text-xs text-blue-800 mt-1">
                <span className="font-medium">Mã check-in:</span>{" "}
                {registration.checkInCode}
              </div>
            )}
          </div>
        )}

        {/* Type 2: Guest list info */}
        {registration.type === 2 && (
          <div className="bg-green-50 border-l-2 border-green-400 p-2 rounded-r mb-2">
            <div className="flex items-center gap-1 text-xs text-green-800">
              <svg
                className="w-3 h-3 text-green-500"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"
                />
              </svg>
              <span className="font-semibold">
                {registration.guestNumber} khách
              </span>
            </div>
            {registration.note && (
              <div className="text-xs text-green-700 mt-1 line-clamp-2">
                {registration.note}
              </div>
            )}
          </div>
        )}

        {/* View Detail Button */}
        <Box
          className="divCenter"
          style={{
            width: "100%",
            padding: "8px 0px",
            fontSize: 13,
            fontWeight: "600",
            border: "1px solid #000",
            borderRadius: "6px",
            textAlign: "center",
            cursor: "pointer",
            transition: "all 0.2s ease",
          }}
          onClick={handleClick}
        >
          Xem chi tiết
        </Box>
      </div>
    );
  }
);

RegistrationCard.displayName = "RegistrationCard";

const EventRegistrationHistory: React.FC = () => {
  const setHeader = useSetHeader();
  const navigate = useNavigate();
  const userToken = useRecoilValue(token);
  const [registrations, setRegistrations] = useState<RegistrationItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [typeFilter, setTypeFilter] = useState("");
  const [statusFilter, setStatusFilter] = useState("");

  const tabGroups: TabGroup[] = [
    {
      id: "all",
      name: "Tất cả",
      value: "",
      children: [
        { id: "all-status", name: "Tất cả", value: "" },
        { id: "pending", name: "Chờ duyệt", value: "0" },
        { id: "registered", name: "Đã duyệt", value: "1" },
        { id: "checkedin", name: "Đã check-in", value: "2" },
        { id: "cancelled", name: "Đã hủy", value: "3" },
      ],
    },
    {
      id: "personal",
      name: "Đơn lẻ",
      value: "1",
      children: [
        { id: "personal-all", name: "Tất cả", value: "" },
        { id: "personal-pending", name: "Chờ duyệt", value: "0" },
        { id: "personal-registered", name: "Đã duyệt", value: "1" },
        { id: "personal-checkedin", name: "Đã check-in", value: "2" },
        { id: "personal-cancelled", name: "Đã hủy", value: "3" },
      ],
    },
    {
      id: "guest",
      name: "Khách mời",
      value: "2",
      children: [
        { id: "guest-all", name: "Tất cả", value: "" },
        { id: "guest-pending", name: "Chờ duyệt", value: "0" },
        { id: "guest-registered", name: "Đã duyệt", value: "1" },
        { id: "guest-checkedin", name: "Đã check-in", value: "2" },
        { id: "guest-cancelled", name: "Đã hủy", value: "3" },
      ],
    },
  ];

  React.useEffect(() => {
    setHeader({
      title: "KHÁCH MỜI",
      showUserInfo: false,
      showMenuButton: false,
      showCloseButton: false,
      hasLeftIcon: true,
    });
  }, [setHeader]);

  const getStatusColor = useCallback((status: number) => {
    switch (status) {
      case 0:
        return "bg-yellow-100 text-yellow-700";
      case 1:
        return "bg-blue-100 text-blue-700";
      case 2:
        return "bg-green-100 text-green-700";
      case 3:
        return "bg-gray-100 text-gray-700";
      default:
        return "bg-gray-100 text-gray-700";
    }
  }, []);

  const getAccentColor = useCallback((status: number) => {
    switch (status) {
      case 0:
        return "linear-gradient(90deg, #0066cc, #003d82)";
      case 1:
        return "linear-gradient(90deg, #0066cc, #003d82)";
      case 2:
        return "linear-gradient(90deg, #10b981, #059669)";
      case 3:
        return "linear-gradient(90deg, #6b7280, #4b5563)";
      default:
        return "linear-gradient(90deg, #6b7280, #4b5563)";
    }
  }, []);

  const formatDate = useCallback((dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString("vi-VN", {
      year: "numeric",
      month: "2-digit",
      day: "2-digit",
      hour: "2-digit",
      minute: "2-digit",
    });
  }, []);

  const handleViewDetail = useCallback(
    (registration: RegistrationItem) => {
      navigate("/giba/event-registration-detail", { state: registration });
    },
    [navigate]
  );

  useEffect(() => {
    const fetchRegistrations = async () => {
      try {
        setLoading(true);

        const params = new URLSearchParams();
        params.append("page", currentPage.toString());
        params.append("pageSize", "10");
        params.append("keyword", "");

        if (typeFilter) {
          params.append("type", typeFilter);
        }
        if (statusFilter) {
          params.append("status", statusFilter);
        }

        const url = `${
          dfData.domain
        }/api/EventRegistrations/MyAllRegistrations?${params.toString()}`;
        console.log("Fetching registrations with URL:", url);

        const response = await axios.get(url, {
          headers: {
            Authorization: `Bearer ${userToken}`,
          },
        });
        console.log("My All Registrations API Response:", response);

        if (response.data.code === 0 && response.data.data) {
          setRegistrations(response.data.data);
          setTotalPages(response.data.totalPages || 1);
        } else {
          setRegistrations([]);
        }
      } catch (error) {
        console.error("Error fetching registration history:", error);
        setRegistrations([]);
      } finally {
        setLoading(false);
      }
    };

    fetchRegistrations();
  }, [currentPage, typeFilter, statusFilter, userToken]);

  if (loading) {
    return (
      <Page className="bg-white min-h-screen mt-[50px]">
        <div className="flex justify-center items-center h-64">
          <LoadingGiba size="lg" text="Đang tải lịch sử đăng ký..." />
        </div>
      </Page>
    );
  }

  return (
    <Page style={{ marginTop: "50px", background: "#f8fafc" }}>
      <Box
        style={{
          background: "#fff",
          boxShadow: "0 2px 4px rgba(0,0,0,0.1)",
        }}
      >
        <TwoTierTab
          tabs={tabGroups}
          activeTab={typeFilter}
          onTabChange={(value) => {
            setTypeFilter(value);
            setStatusFilter("");
            setCurrentPage(1);
            setRegistrations([]);
          }}
          activeChildTab={statusFilter}
          onChildTabChange={(value) => {
            setStatusFilter(value);
            setCurrentPage(1);
            setRegistrations([]);
          }}
        />
      </Box>

      <div className="p-4 relative min-h-[400px]">
        {registrations.length > 0 ? (
          <div className="space-y-4">
            {registrations.map((registration) => (
              <RegistrationCard
                key={registration.id}
                registration={registration}
                getStatusColor={getStatusColor}
                getAccentColor={getAccentColor}
                formatDate={formatDate}
                onViewDetail={handleViewDetail}
              />
            ))}
          </div>
        ) : (
          <div className="text-center py-20">
            <div className="relative">
              <div className="w-32 h-32 bg-gradient-to-br from-gray-100 to-gray-200 rounded-full flex items-center justify-center mx-auto mb-6 shadow-lg">
                <svg
                  className="w-16 h-16 text-gray-400"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={1.5}
                    d="M9 5H7a2 2 0 00-2 2v10a2 2 0 002 2h8a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"
                  />
                </svg>
              </div>
            </div>
            <div className="text-gray-900 text-xl font-bold mb-3">
              Chưa có đăng ký nào
            </div>
            <div className="text-gray-500 text-base max-w-sm mx-auto leading-relaxed">
              Bạn chưa đăng ký sự kiện nào. Hãy tham gia các sự kiện để đăng ký!
            </div>
          </div>
        )}
      </div>

      <FloatingActionButtonGiba
        icon={<Plus />}
        onClick={() => navigate("/giba/event")}
        position="bottom-right"
        color="yellow"
        size="md"
        tooltip="Xem danh sách sự kiện"
      />
    </Page>
  );
};

export default EventRegistrationHistory;
