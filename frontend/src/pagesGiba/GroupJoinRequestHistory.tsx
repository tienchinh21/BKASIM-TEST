import React, { useState, useEffect, useLayoutEffect } from "react";
import { Page, Box } from "zmp-ui";
import { useNavigate } from "zmp-ui";
import useSetHeader from "../components/hooks/useSetHeader";
import { useRecoilValue, useRecoilState } from "recoil";
import { token, groupJoinRequestCache } from "../recoil/RecoilState";
import axios from "axios";
import LoadingGiba from "../componentsGiba/LoadingGiba";
import Category from "../components/Category";
import { Drawer, Input } from "antd";
import { toast } from "react-toastify";
import dfData from "../common/DefaultConfig.json";
import FloatingActionButtonGiba from "../componentsGiba/FloatingActionButtonGiba";
import { Plus } from "lucide-react";
import { isCacheValid, CACHE_MAX_AGE } from "../utils/infiniteScrollUtils";
const { TextArea } = Input;

interface JoinRequest {
  id: string;
  groupId: string;
  groupName: string;
  userZaloId: string;
  reason: string;
  company: string;
  position: string;
  isApproved: boolean | null; // null = Chờ xét duyệt, true = Đã duyệt, false = Từ chối
  statusText: string;
  createdDate: string;
  canEdit: boolean;
  rejectReason: string | null;
  approvedDate: string | null;
  updatedDate: string;
}

interface GroupJoinRequestCache {
  listRequests: JoinRequest[];
  filterSearch: {
    page: number;
    pageSize: number;
    statusFilter: string;
  };
  scrollTop: number;
  timestamp: number | null;
  totalPages: number;
}

const GroupJoinRequestHistory: React.FC = () => {
  const navigate = useNavigate();
  const setHeader = useSetHeader();
  const userToken = useRecoilValue(token);
  const [cache, setCache] = useRecoilState(groupJoinRequestCache) as [
    GroupJoinRequestCache,
    (
      valOrUpdater:
        | GroupJoinRequestCache
        | ((currVal: GroupJoinRequestCache) => GroupJoinRequestCache)
    ) => void
  ];
  const [loading, setLoading] = useState(false);

  const [editingRequest, setEditingRequest] = useState<JoinRequest | null>(
    null
  );
  const [editForm, setEditForm] = useState({
    reason: "",
    company: "",
    position: "",
  });
  const [updating, setUpdating] = useState(false);
  const [showEditDrawer, setShowEditDrawer] = useState(false);

  const requests = cache.listRequests;
  const statusFilter = cache.filterSearch.statusFilter;
  const currentPage = cache.filterSearch.page;
  const totalPages = cache.totalPages;

  // Set header
  React.useEffect(() => {
    setHeader({
      title: "ĐƠN THAM GIA NHÓM",
      showUserInfo: false,
      showMenuButton: false,
      showCloseButton: false,
      hasLeftIcon: true,
    });
  }, [setHeader]);

  // Initialize cache on mount
  useLayoutEffect(() => {
    setCache((prev) => {
      const cacheIsValid = isCacheValid(prev.timestamp, CACHE_MAX_AGE);
      if (cacheIsValid && prev.listRequests.length > 0) {
        return prev;
      }
      // Reset to "Tất cả" tab when entering page
      return {
        ...prev,
        listRequests: [],
        filterSearch: { ...prev.filterSearch, page: 1, statusFilter: "" },
        scrollTop: 0,
        timestamp: null,
      };
    });
  }, [setCache]);

  // Fetch join requests
  useEffect(() => {
    const fetchRequests = async () => {
      if (!userToken) return;

      try {
        setLoading(true);
        let url = `${dfData.domain}/api/Groups/my-join-requests?page=${currentPage}&pageSize=10`;
        if (statusFilter) {
          url += `&status=${statusFilter}`;
        }

        const response = await axios.get(url, {
          headers: {
            Authorization: `Bearer ${userToken}`,
          },
        });

        if (response.data.data) {
          const newRequests = response.data.data.items || [];
          const newTotalPages = response.data.data.totalPages || 1;
          const isFirstPage = currentPage === 1;

          setCache((prev) => ({
            ...prev,
            listRequests: isFirstPage
              ? newRequests
              : [...prev.listRequests, ...newRequests],
            totalPages: newTotalPages,
            timestamp: isFirstPage ? Date.now() : prev.timestamp,
          }));
        } else {
          if (currentPage === 1) {
            setCache((prev) => ({
              ...prev,
              listRequests: [],
              totalPages: 1,
              timestamp: Date.now(),
            }));
          }
        }
      } catch (error) {
        console.error("Error fetching join requests:", error);
        toast.error("Không thể tải danh sách đơn tham gia nhóm");
        if (currentPage === 1) {
          setCache((prev) => ({
            ...prev,
            listRequests: [],
            totalPages: 1,
            timestamp: Date.now(),
          }));
        }
      } finally {
        setLoading(false);
      }
    };

    fetchRequests();
  }, [currentPage, statusFilter, userToken, setCache]);

  const statusCategories = [
    { id: "", name: "Tất cả", value: "" },
    { id: "null", name: "Chờ xét duyệt", value: "null" },
    { id: "true", name: "Đã duyệt", value: "true" },
    { id: "false", name: "Từ chối", value: "false" },
  ];

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

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString("vi-VN", {
      year: "numeric",
      month: "2-digit",
      day: "2-digit",
      hour: "2-digit",
      minute: "2-digit",
    });
  };

  const handleDetailClick = (request: JoinRequest) => {
    navigate(`/giba/group-join-request-detail/${request.groupId}`);
  };

  const handleEditClick = (request: JoinRequest) => {
    setEditingRequest(request);
    setEditForm({
      reason: request.reason,
      company: request.company,
      position: request.position,
    });
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
        `${dfData.domain}/api/groups/my-join-requests/${editingRequest?.id}`,
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
        // Refresh list
        const updatedRequests = requests.map((req) =>
          req.id === editingRequest?.id ? { ...req, ...editForm } : req
        );
        setCache((prev) => ({
          ...prev,
          listRequests: updatedRequests,
        }));
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

  return (
    <Page style={{ marginTop: "50px", background: "#f8fafc" }}>
      {/* Category Filter - Enhanced */}
      <Box
        style={{
          background: "#fff",
          boxShadow: "0 2px 4px rgba(0,0,0,0.1)",
        }}
      >
        <Category
          list={statusCategories}
          value={statusFilter}
          onChange={(value) => {
            setCache((prev) => ({
              ...prev,
              listRequests: [],
              filterSearch: {
                ...prev.filterSearch,
                page: 1,
                statusFilter: value,
              },
              scrollTop: 0,
              timestamp: null,
            }));
          }}
          backgroundColor="#fff"
        />
      </Box>

      {/* Request List - Enhanced */}
      <div className="p-4">
        {loading ? (
          <div className="flex justify-center items-center py-16">
            <LoadingGiba size="lg" text="Đang tải danh sách..." />
          </div>
        ) : requests.length > 0 ? (
          <div className="space-y-4">
            {requests.map((request) => (
              <div
                key={request.id}
                className="bg-white rounded-lg shadow-md border border-gray-200 hover:shadow-lg transition-all duration-200 cursor-pointer"
                style={{
                  padding: "12px",
                  position: "relative",
                  overflow: "hidden",
                }}
                onClick={() => handleDetailClick(request)}
              >
                {/* Status accent line */}
                <div
                  className="absolute top-0 left-0 right-0 h-0.5"
                  style={{
                    background:
                      request.statusText === "Chờ xét duyệt"
                        ? "linear-gradient(90deg, #0066cc, #003d82)" // Chờ xét duyệt (xanh BKASIM)
                        : request.statusText === "Đã duyệt"
                        ? "linear-gradient(90deg, #10b981, #059669)" // Đã duyệt (xanh)
                        : "linear-gradient(90deg, #ef4444, #dc2626)", // Từ chối (đỏ)
                  }}
                ></div>

                {/* Header with Status - Compact */}
                <div className="flex items-center justify-between gap-2 mb-2">
                  <div className="flex-1 min-w-0">
                    <div className="text-sm font-bold text-gray-900 truncate">
                      {request.groupName}
                    </div>
                    <div className="text-xs text-gray-500">
                      ID: {request.id.slice(-6).toUpperCase()}
                    </div>
                  </div>
                  <div className="flex items-center gap-2">
                    <span
                      className={`px-2 py-1 rounded-full text-xs font-medium ${getStatusColor(
                        request.statusText
                      )}`}
                    >
                      {request.statusText}
                    </span>
                    {request.statusText === "Chờ xét duyệt" && (
                      <div className="w-2 h-2 bg-amber-400 rounded-full animate-pulse"></div>
                    )}
                    {/* Debug info - remove in production */}
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
                        d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4"
                      />
                    </svg>
                    <span className="font-semibold text-gray-900">
                      {request.company} - {request.position}
                    </span>
                  </div>
                  <div className="text-gray-500">
                    {formatDate(request.createdDate)}
                  </div>
                </div>

                {/* Reason Preview - Compact */}
                {request.reason && (
                  <div className="bg-blue-50 border-l-2 border-blue-400 p-2 rounded-r mb-2">
                    <div className="text-xs text-blue-600 font-medium mb-1">
                      Lý do tham gia
                    </div>
                    <div className="text-xs text-blue-800 line-clamp-2">
                      {request.reason.length > 80
                        ? `${request.reason.substring(0, 80)}...`
                        : request.reason}
                    </div>
                  </div>
                )}

                {request.rejectReason && (
                  <div className="bg-red-50 border-l-2 border-red-400 p-2 rounded-r mb-2">
                    <div className="text-xs text-red-600 font-medium mb-1">
                      Lý do từ chối
                    </div>
                    <div className="text-xs text-red-800 line-clamp-2">
                      {request.rejectReason}
                    </div>
                  </div>
                )}

                {/* Edit Button - Compact */}
                {request.canEdit && (
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
                    onClick={() => handleEditClick(request)}
                    // onMouseEnter={(e) => {
                    //   e.currentTarget.style.background = "#374151";
                    // }}
                    // onMouseLeave={(e) => {
                    //   e.currentTarget.style.background = "#000";
                    // }}
                  >
                    Chỉnh sửa thông tin
                  </Box>
                )}
              </div>
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
                    d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"
                  />
                </svg>
              </div>
              <div className="absolute -top-2 -right-2 w-6 h-6 bg-yellow-400 rounded-full flex items-center justify-center">
                <span className="text-xs font-bold text-yellow-800">!</span>
              </div>
            </div>
            <div className="text-gray-900 text-xl font-bold mb-3">
              Chưa có đơn xin tham gia nhóm
            </div>
            <div className="text-gray-500 text-base max-w-sm mx-auto leading-relaxed">
              Bạn chưa gửi đơn xin tham gia nhóm nào. Hãy tìm kiếm và tham gia
              các nhóm phù hợp!
            </div>
          </div>
        )}
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
              value={editingRequest?.groupName}
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

      {/* Floating Action Button */}
      <FloatingActionButtonGiba
        icon={<Plus />}
        onClick={() => navigate("/giba/groups")}
        position="bottom-right"
        color="yellow"
        size="md"
        tooltip="Xem danh sách Club"
      />
    </Page>
  );
};

export default GroupJoinRequestHistory;
