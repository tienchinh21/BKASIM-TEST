import React, { useState, useEffect } from "react";
import { Page, Box, Button } from "zmp-ui";
import { useNavigate } from "zmp-ui";
import { useLocation } from "react-router-dom";
import useSetHeader from "../components/hooks/useSetHeader";
import { useRecoilValue } from "recoil";
import { token } from "../recoil/RecoilState";
import axios from "axios";
import LoadingGiba from "../componentsGiba/LoadingGiba";
import { toast } from "react-toastify";
import { Drawer, Input } from "antd";
import dfData from "../common/DefaultConfig.json";

const { TextArea } = Input;

interface Guest {
  id: string;
  eventGuestId: string;
  guestName: string;
  guestPhone: string;
  guestEmail: string;
  status: number;
  statusText: string;
}

interface GuestListDetail {
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
  guestLists: Guest[];
  memberName: string;
  memberPhone: string;
  memberEmail: string;
  memberCompany: string;
  memberPosition: string;
}

const GuestListDetail: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const setHeader = useSetHeader();
  const userToken = useRecoilValue(token);
  const [loading, setLoading] = useState(true);
  const [detail, setDetail] = useState<GuestListDetail | null>(null);
  const [showCancelDrawer, setShowCancelDrawer] = useState(false);
  const [cancelReason, setCancelReason] = useState("");
  const [cancelling, setCancelling] = useState(false);

  const guestListId = (location.state as any)?.guestListId;

  // Set header
  React.useEffect(() => {
    setHeader({
      title: "CHI TIẾT KHÁCH MỜI",
      showUserInfo: false,
      showMenuButton: false,
      showCloseButton: false,
      hasLeftIcon: true,
    });
  }, [setHeader]);

  // Fetch detail
  useEffect(() => {
    const fetchDetail = async () => {
      if (!guestListId) {
        navigate(-1);
        return;
      }

      try {
        setLoading(true);
        const response = await axios.get(
          `${dfData.domain}/api/EventGuests/Detail/${guestListId}`,
          {
            headers: {
              Authorization: `Bearer ${userToken}`,
            },
          }
        );

        if (response.data.code === 0 && response.data.data) {
          setDetail(response.data.data);
        }
      } catch (error) {
        console.error("Error fetching detail:", error);
        toast.error("Không thể tải chi tiết đơn");
        navigate(-1);
      } finally {
        setLoading(false);
      }
    };

    fetchDetail();
  }, [guestListId, userToken]);

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

  const getBackgroundColorByStatus = (status: number) => {
    switch (status) {
      case 1:
        return "#FFA500"; // Chờ xử lý (Orange)
      case 2:
        return "#558B2F"; // Đã duyệt (Green)
      case 3:
        return "#003d82"; // Từ chối (Navy Blue - BKASIM)
      case 4:
        return "#757575"; // Đã hủy (Gray)
      default:
        return "#0066cc"; // Default (Sky Blue - BKASIM)
    }
  };

  const handleCancelClick = () => {
    setShowCancelDrawer(true);
  };

  const handleCancelConfirm = async () => {
    if (!cancelReason.trim()) {
      toast.error("Vui lòng nhập lý do hủy");
      return;
    }

    try {
      setCancelling(true);
      const response = await axios.post(
        `${dfData.domain}/api/EventGuests/Cancel/${guestListId}`,
        {
          cancelReason: cancelReason.trim(),
        },
        {
          headers: {
            Authorization: `Bearer ${userToken}`,
          },
        }
      );

      if (response.data.code === 0 || response.data.success) {
        toast.success("Hủy đơn thành công");
        setShowCancelDrawer(false);
        navigate(-1);
      } else {
        toast.error(response.data.message || "Không thể hủy đơn");
      }
    } catch (error) {
      console.error("Error cancelling:", error);
      toast.error("Có lỗi xảy ra khi hủy đơn");
    } finally {
      setCancelling(false);
    }
  };

  if (loading) {
    return (
      <Page className="bg-white min-h-screen mt-[50px]">
        <div className="flex justify-center items-center h-64">
          <LoadingGiba size="lg" text="Đang tải chi tiết..." />
        </div>
      </Page>
    );
  }

  if (!detail) {
    return (
      <Page className="bg-white min-h-screen mt-[50px]">
        <div className="text-center py-20">
          <div className="text-black text-xl font-bold">
            Không tìm thấy thông tin
          </div>
        </div>
      </Page>
    );
  }

  return (
    <Page
      style={{
        marginTop: "50px",
        background: "#f8fafc",
        paddingBottom: detail.status === 1 ? "80px" : "20px",
      }}
    >
      <div className="p-4 space-y-4">
        {/* Main Info Card - Enhanced */}
        <div className="bg-white rounded-xl shadow-lg border border-gray-200 overflow-hidden">
          {/* Status accent line */}
          <div
            className="h-1 w-full"
            style={{
              background: getBackgroundColorByStatus(detail.status),
            }}
          ></div>

          <div className="p-4">
            <div className="flex items-start justify-between mb-4">
              <div className="flex-1">
                <div className="flex gap-2 items-center mb-2">
                  <span>Tên sự kiện:</span>
                  <h1 className="text-lg font-bold text-gray-900 ">
                    {detail.eventTitle}
                  </h1>
                </div>
                <div className="text-sm text-gray-600">
                  Mã đơn:{" "}
                  <span className="font-semibold text-gray-900">
                    {detail.id.slice(0, 23).toUpperCase()}
                  </span>
                </div>
              </div>
              <div className="ml-4">
                <span
                  className="px-3 py-1.5 rounded-full text-xs font-semibold text-white"
                  style={{
                    background: getBackgroundColorByStatus(detail.status),
                  }}
                >
                  {detail.statusText}
                </span>
              </div>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div className="bg-gray-50 rounded-lg p-3">
                <div className="text-xs text-gray-500 mb-1">Số khách mời</div>
                <div className="text-lg font-bold text-gray-900 flex items-center gap-1">
                  <svg
                    className="w-4 h-4 text-blue-500"
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
                  {detail.guestNumber}
                </div>
              </div>
              <div className="bg-gray-50 rounded-lg p-3">
                <div className="text-xs text-gray-500 mb-1">Ngày tạo</div>
                <div className="text-sm font-semibold text-gray-900">
                  {formatDate(detail.createdDate)}
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Member Info Card - Enhanced */}
        <div className="bg-white rounded-xl shadow-lg border border-gray-200 overflow-hidden">
          <div className="bg-gray-50 px-4 py-3 border-b border-gray-200">
            <h2 className="text-base font-bold text-gray-900">
              Thông tin người đăng ký
            </h2>
          </div>
          <div className="p-4 space-y-3">
            <div className="grid grid-cols-1 gap-3">
              <div className="flex justify-between items-center py-2 border-b border-gray-100">
                <span className="text-sm text-gray-600">Họ tên</span>
                <span className="text-sm font-semibold text-gray-900">
                  {detail.memberName}
                </span>
              </div>
              <div className="flex justify-between items-center py-2 border-b border-gray-100">
                <span className="text-sm text-gray-600">Số điện thoại</span>
                <span className="text-sm font-semibold text-gray-900">
                  {detail.memberPhone}
                </span>
              </div>
              <div className="flex justify-between items-center py-2 border-b border-gray-100">
                <span className="text-sm text-gray-600">Email</span>
                <span className="text-sm font-semibold text-gray-900">
                  {detail.memberEmail}
                </span>
              </div>
              <div className="flex justify-between items-center py-2 border-b border-gray-100">
                <span className="text-sm text-gray-600">Công ty</span>
                <span className="text-sm font-semibold text-gray-900">
                  {detail.memberCompany}
                </span>
              </div>
              <div className="flex justify-between items-center py-2">
                <span className="text-sm text-gray-600">Chức vụ</span>
                <span className="text-sm font-semibold text-gray-900">
                  {detail.memberPosition}
                </span>
              </div>
            </div>
          </div>
        </div>

        {/* Note - Enhanced */}
        {detail.note && (
          <div className="bg-white rounded-xl shadow-lg border border-gray-200 overflow-hidden">
            <div className="bg-blue-50 px-4 py-3 border-b border-blue-200">
              <h2 className="text-base font-bold text-blue-900">
                Nội dung đăng ký{" "}
              </h2>
            </div>
            <div className="p-4">
              <p className="text-sm text-gray-700 leading-relaxed whitespace-pre-line">
                {detail.note}
              </p>
            </div>
          </div>
        )}

        {/* Guest List - Enhanced without status */}
        {detail.guestLists && detail.guestLists.length > 0 && (
          <div className="bg-white rounded-xl shadow-lg border border-gray-200 overflow-hidden">
            <div className="bg-gray-50 px-4 py-3 border-b border-gray-200">
              <h2 className="text-base font-bold text-gray-900">
                Danh sách khách mời ({detail.guestLists.length})
              </h2>
            </div>
            <div className="p-4">
              <div className="space-y-3">
                {detail.guestLists.map((guest, index) => (
                  <div
                    key={guest.id}
                    className="bg-gray-50 rounded-lg p-4 border border-gray-200"
                  >
                    <div className="flex items-center justify-between mb-3">
                      <h3 className="text-sm font-bold text-gray-900">
                        Khách #{index + 1}
                      </h3>
                      <div className="w-2 h-2 bg-green-400 rounded-full"></div>
                    </div>
                    <div className="space-y-2">
                      <div className="flex justify-between items-center py-1">
                        <span className="text-xs text-gray-600">Họ tên</span>
                        <span className="text-sm font-semibold text-gray-900">
                          {guest.guestName}
                        </span>
                      </div>
                      <div className="flex justify-between items-center py-1">
                        <span className="text-xs text-gray-600">
                          Số điện thoại
                        </span>
                        <span className="text-sm font-semibold text-gray-900">
                          {guest.guestPhone}
                        </span>
                      </div>
                      <div className="flex justify-between items-center py-1">
                        <span className="text-xs text-gray-600">Email</span>
                        <span className="text-sm font-semibold text-gray-900">
                          {guest.guestEmail}
                        </span>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          </div>
        )}

        {/* Reject/Cancel Reason - Enhanced */}
        {(detail.rejectReason || detail.cancelReason) && (
          <div className="bg-red-50 rounded-xl shadow-lg border border-red-200 overflow-hidden">
            <div className="bg-red-100 px-4 py-3 border-b border-red-200">
              <h2 className="text-base font-bold text-red-900">
                {detail.rejectReason ? "Lý do từ chối" : "Lý do hủy"}
              </h2>
            </div>
            <div className="p-4">
              <p className="text-sm text-red-800 leading-relaxed">
                {detail.rejectReason || detail.cancelReason}
              </p>
            </div>
          </div>
        )}
      </div>

      {/* Cancel Button - Enhanced */}
      {detail.status === 1 && (
        <div className="fixed bottom-0 left-0 right-0 flex justify-center p-4 bg-white border-t border-gray-200 shadow-lg z-50">
          <button
            onClick={handleCancelClick}
            className="w-[60%] py-2 px-2 bg-white border-2 border-red-500 rounded-xl text-red-600 font-bold text-base hover:bg-red-50 transition-colors duration-200"
          >
            Hủy đơn
          </button>
        </div>
      )}

      {/* Cancel Drawer */}
      <Drawer
        title="Hủy đơn khách mời"
        placement="bottom"
        onClose={() => setShowCancelDrawer(false)}
        open={showCancelDrawer}
        height="auto"
      >
        <div className="space-y-4">
          <div>
            <label className="block text-sm font-semibold mb-2 text-black">
              Lý do hủy <span className="text-red-500">*</span>
            </label>
            <TextArea
              rows={4}
              placeholder="Nhập lý do hủy đơn..."
              value={cancelReason}
              onChange={(e) => setCancelReason(e.target.value)}
              maxLength={500}
              showCount
            />
          </div>
          <div className="flex gap-2">
            <button
              onClick={() => setShowCancelDrawer(false)}
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
              disabled={cancelling}
            >
              Đóng
            </button>
            <button
              onClick={handleCancelConfirm}
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
              disabled={cancelling}
            >
              {cancelling ? "Đang xử lý..." : "Xác nhận hủy"}
            </button>
          </div>
        </div>
      </Drawer>
    </Page>
  );
};

export default GuestListDetail;
