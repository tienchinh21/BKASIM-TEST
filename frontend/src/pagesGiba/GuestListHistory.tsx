import React, { useState, useEffect, useMemo, useCallback } from "react";
import { Page, Box } from "zmp-ui";
import { useNavigate } from "zmp-ui";
import useSetHeader from "../components/hooks/useSetHeader";
import { useRecoilValue } from "recoil";
import { token } from "../recoil/RecoilState";
import axios from "axios";
import LoadingGiba from "../componentsGiba/LoadingGiba";
import Category from "../components/Category";
import dfData from "../common/DefaultConfig.json";
import FloatingActionButtonGiba from "../componentsGiba/FloatingActionButtonGiba";
import { Plus } from "lucide-react";

interface GuestListItem {
  id: string;
  eventId: string;
  eventTitle: string;
  userZaloId: string;
  note: string;
  guestNumber: number;
  status: number;
  statusText: string;
  rejectReason: string | null;
  cancelReason: string | null;
  createdDate: string;
  memberName: string;
  memberPhone: string;
  memberEmail: string;
  memberCompany: string;
  memberPosition: string;
}

// Memoized GuestListCard component
const GuestListCard = React.memo<{
  guestList: GuestListItem;
  getStatusColor: (status: number) => string;
  formatDate: (dateString: string) => string;
  onViewDetail: (guestList: GuestListItem) => void;
}>(({ guestList, getStatusColor, formatDate, onViewDetail }) => {
  const handleMouseEnter = useCallback(
    (e: React.MouseEvent<HTMLDivElement>) => {
      e.currentTarget.style.background = "#374151";
    },
    []
  );

  const handleMouseLeave = useCallback(
    (e: React.MouseEvent<HTMLDivElement>) => {
      e.currentTarget.style.background = "#000";
    },
    []
  );

  const handleClick = useCallback(() => {
    onViewDetail(guestList);
  }, [guestList, onViewDetail]);

  return (
    <div
      className="bg-white rounded-lg shadow-md border border-gray-200 hover:shadow-lg transition-all duration-200"
      style={{
        padding: "12px",
        position: "relative",
        overflow: "hidden",
      }}
    >
      {/* Decorative accent line */}
      <div
        className="absolute top-0 left-0 right-0 h-0.5"
        style={{
          background:
            guestList.status === 1
              ? "linear-gradient(90deg, #fbbf24, #f59e0b)"
              : guestList.status === 2
              ? "linear-gradient(90deg, #10b981, #059669)"
              : guestList.status === 3
              ? "linear-gradient(90deg, #ef4444, #dc2626)"
              : "linear-gradient(90deg, #6b7280, #4b5563)",
        }}
      ></div>

      {/* Header with Status - Compact */}
      <div className="flex items-center justify-between gap-2 mb-2">
        <div className="flex-1 min-w-0">
          <div className="text-sm font-bold text-gray-900 truncate">
            {guestList.eventTitle}
          </div>
          <div className="text-xs text-gray-500">
            ID: {guestList.id.slice(-6).toUpperCase()}
          </div>
        </div>
        <div className="flex items-center gap-2">
          <span
            className={`px-2 py-1 rounded-full text-xs font-medium ${getStatusColor(
              guestList.status
            )}`}
          >
            {guestList.statusText}
          </span>
          {guestList.status === 1 && (
            <div className="w-2 h-2 bg-amber-400 rounded-full animate-pulse"></div>
          )}
        </div>
      </div>

      {/* Info Row - Compact */}
      <div className="flex items-center justify-between text-xs text-gray-600 mb-2">
        <div className="flex items-center gap-1">
          <svg
            className="w-3 h-3 text-blue-500"
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
          <span className="font-semibold text-gray-900">
            {guestList.guestNumber} khách
          </span>
        </div>
        <div className="text-gray-500">{formatDate(guestList.createdDate)}</div>
      </div>

      {/* Note if exists - Compact */}
      {guestList.note && (
        <div className="bg-blue-50 border-l-2 border-blue-400 p-2 rounded-r mb-2">
          <div className="text-xs text-blue-600 font-medium mb-1">Ghi chú</div>
          <div className="text-xs text-blue-800 line-clamp-2">
            {guestList.note}
          </div>
        </div>
      )}

      {/* View Detail Button - Compact */}
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
        onMouseEnter={handleMouseEnter}
        onMouseLeave={handleMouseLeave}
      >
        Xem chi tiết
      </Box>
    </div>
  );
});

GuestListCard.displayName = "GuestListCard";

const GuestListHistory: React.FC = () => {
  const navigate = useNavigate();
  const setHeader = useSetHeader();
  const userToken = useRecoilValue(token);
  const [initialLoading, setInitialLoading] = useState(true);
  const [listLoading, setListLoading] = useState(false);
  const [guestLists, setGuestLists] = useState<GuestListItem[]>([]);
  const [statusFilter, setStatusFilter] = useState<string>("");
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);

  // Set header
  React.useEffect(() => {
    setHeader({
      title: "KHÁCH MỜI",
      showUserInfo: false,
      showMenuButton: false,
      showCloseButton: false,
      hasLeftIcon: true,
    });
  }, [setHeader]);

  const handleCreateAppointment = useCallback(() => {
    navigate("/giba/event");
  }, [navigate]);

  // Memoize status categories - không thay đổi
  const statusCategories = useMemo(
    () => [
      { id: "", name: "Tất cả" },
      { id: "1", name: "Chờ xử lý" },
      { id: "2", name: "Đã duyệt" },
      { id: "3", name: "Từ chối" },
      { id: "4", name: "Đã hủy" },
    ],
    []
  );

  // Memoize getStatusColor function
  const getStatusColor = useCallback((status: number) => {
    switch (status) {
      case 1:
        return "bg-yellow-100 text-yellow-700";
      case 2:
        return "bg-green-100 text-green-700";
      case 3:
        return "bg-red-100 text-red-700";
      case 4:
        return "bg-gray-100 text-gray-700";
      default:
        return "bg-gray-100 text-gray-700";
    }
  }, []);

  // Memoize formatDate function
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

  // Memoize handleViewDetail function
  const handleViewDetail = useCallback(
    (guestList: GuestListItem) => {
      navigate("/giba/guest-list-detail", {
        state: { guestListId: guestList.id },
      });
    },
    [navigate]
  );

  // Memoize handleStatusChange function
  const handleStatusChange = useCallback((value: string) => {
    setStatusFilter(value);
    setCurrentPage(1);
  }, []);

  // Memoize empty handlers
  const emptyHandler = useCallback(() => {}, []);

  // Fetch guest lists
  useEffect(() => {
    const fetchGuestLists = async () => {
      try {
        // Use initialLoading for first load, listLoading for subsequent loads
        if (initialLoading) {
          setInitialLoading(true);
        } else {
          setListLoading(true);
        }

        let url = `${dfData.domain}/api/EventGuests/MyGuestLists?page=${currentPage}&pageSize=10&keyword=`;
        if (statusFilter) url += `&status=${statusFilter}`;

        const response = await axios.get(url, {
          headers: {
            Authorization: `Bearer ${userToken}`,
          },
        });

        if (response.data.code === 0 && response.data.data) {
          setGuestLists(response.data.data);
          setTotalPages(response.data.totalPages || 1);
        }
      } catch (error) {
        console.error("Error fetching guest lists:", error);
      } finally {
        setInitialLoading(false);
        setListLoading(false);
      }
    };

    fetchGuestLists();
  }, [currentPage, statusFilter, userToken]);

  // Memoize loading component
  const loadingComponent = useMemo(
    () => (
      <Page className="bg-white min-h-screen mt-[50px]">
        <div className="flex justify-center items-center h-64">
          <LoadingGiba size="lg" text="Đang tải danh sách..." />
        </div>
      </Page>
    ),
    []
  );

  // Memoize empty state component
  const emptyStateComponent = useMemo(
    () => (
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
                d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"
              />
            </svg>
          </div>
          <div className="absolute -top-2 -right-2 w-6 h-6 bg-yellow-400 rounded-full flex items-center justify-center">
            <span className="text-xs font-bold text-yellow-800">!</span>
          </div>
        </div>
        <div className="text-gray-900 text-xl font-bold mb-3">
          Chưa có đơn khách mời nào
        </div>
        <div className="text-gray-500 text-base max-w-sm mx-auto leading-relaxed">
          Bạn chưa đăng ký khách mời cho sự kiện nào. Hãy tham gia các sự kiện
          để tạo danh sách khách mời!
        </div>
      </div>
    ),
    []
  );

  if (initialLoading) {
    return loadingComponent;
  }

  return (
    <Page style={{ marginTop: "50px", background: "#f8fafc" }}>
      <Box
        style={{
          background: "#fff",
          boxShadow: "0 2px 4px rgba(0,0,0,0.1)",
        }}
      >
        <Category
          list={statusCategories}
          value={statusFilter}
          onChange={handleStatusChange}
          valueChild=""
          onChangeValueChild={emptyHandler}
          backgroundColor="#fff"
        />
      </Box>

      {/* Guest List with enhanced cards */}
      <div className="p-4 relative min-h-[400px]">
        {listLoading && (
          <div className="absolute inset-0 bg-white bg-opacity-75 flex items-center justify-center z-10">
            <LoadingGiba size="md" text="Đang tải..." />
          </div>
        )}
        {guestLists.length > 0 ? (
          <div className="space-y-4">
            {guestLists.map((guestList) => (
              <GuestListCard
                key={guestList.id}
                guestList={guestList}
                getStatusColor={getStatusColor}
                formatDate={formatDate}
                onViewDetail={handleViewDetail}
              />
            ))}
          </div>
        ) : (
          emptyStateComponent
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
    </Page>
  );
};

export default GuestListHistory;
