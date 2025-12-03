import React, { useEffect } from "react";
import { Page, Box } from "zmp-ui";
import { useLocation, useNavigate } from "react-router-dom";
import useSetHeader from "../components/hooks/useSetHeader";

interface GuestList {
  id: string;
  guestName: string;
  guestPhone: string;
  guestEmail: string;
  status: number;
  statusText: string;
}

interface RegistrationDetail {
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
  memberName?: string;
  memberPhone?: string;
  memberEmail?: string;
  memberCompany?: string;
  memberPosition?: string;
}

const EventRegistrationDetail: React.FC = () => {
  const setHeader = useSetHeader();
  const location = useLocation();
  const navigate = useNavigate();
  const registration = location.state as RegistrationDetail;

  useEffect(() => {
    setHeader({
      title: "CHI TIẾT KHÁCH MỜI",
      showUserInfo: false,
      showMenuButton: false,
      showCloseButton: false,
      hasLeftIcon: true,
    });
  }, [setHeader]);

  const getBackgroundColorByStatus = (status: number) => {
    switch (status) {
      case 0:
        return "#FFA500"; // Chờ duyệt (Orange)
      case 1:
        return "#3b82f6"; // Đã duyệt (Blue)
      case 2:
        return "#558B2F"; // Đã check-in (Green)
      case 3:
        return "#757575"; // Đã hủy (Gray)
      default:
        return "#DD5531";
    }
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

  if (!registration) {
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
        paddingBottom: "20px",
      }}
    >
      <div className="p-4 space-y-4">
        {/* Main Info Card */}
        <div className="bg-white rounded-xl shadow-lg border border-gray-200 overflow-hidden">
          <div
            className="h-1 w-full"
            style={{
              background: getBackgroundColorByStatus(registration.status),
            }}
          ></div>

          <div className="p-4">
            <div className="flex items-start justify-between mb-4">
              <div className="flex-1">
                <div className="flex gap-2 items-center mb-2">
                  <span>Tên sự kiện:</span>
                  <h1 className="text-lg font-bold text-gray-900">
                    {registration.eventTitle}
                  </h1>
                </div>
                {registration.groupName && (
                  <div className="text-sm text-gray-600 mb-1">
                    Club:{" "}
                    <span className="font-semibold text-gray-900">
                      Giba - {registration.groupName}
                    </span>
                  </div>
                )}
                <div className="text-sm text-gray-600">
                  Mã đơn:{" "}
                  <span className="font-semibold text-gray-900">
                    {registration.id.slice(0, 23).toUpperCase()}
                  </span>
                </div>
              </div>
              <div className="ml-4">
                <span
                  className="px-3 py-1.5 rounded-full text-xs font-semibold text-white"
                  style={{
                    background: getBackgroundColorByStatus(registration.status),
                  }}
                >
                  {registration.statusText}
                </span>
              </div>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div className="bg-gray-50 rounded-lg p-3">
                <div className="text-xs text-gray-500 mb-1">Loại đăng ký</div>
                <div className="text-sm font-bold text-gray-900">
                  {registration.typeText}
                </div>
              </div>
              <div className="bg-gray-50 rounded-lg p-3">
                <div className="text-xs text-gray-500 mb-1">Ngày tạo</div>
                <div className="text-sm font-semibold text-gray-900">
                  {formatDate(registration.createdDate)}
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Type 1: Personal Registration Info */}
        {registration.type === 1 && (
          <div className="bg-white rounded-xl shadow-lg border border-gray-200 overflow-hidden">
            <div className="bg-blue-50 px-4 py-3 border-b border-blue-200">
              <h2 className="text-base font-bold text-blue-900">
                Thông tin đăng ký
              </h2>
            </div>
            <div className="p-4 space-y-3">
              <div className="flex justify-between items-center py-2 border-b border-gray-100">
                <span className="text-sm text-gray-600">Họ tên</span>
                <span className="text-sm font-semibold text-gray-900">
                  {registration.name}
                </span>
              </div>
              <div className="flex justify-between items-center py-2 border-b border-gray-100">
                <span className="text-sm text-gray-600">Số điện thoại</span>
                <span className="text-sm font-semibold text-gray-900">
                  {registration.phoneNumber}
                </span>
              </div>
              <div className="flex justify-between items-center py-2 border-b border-gray-100">
                <span className="text-sm text-gray-600">Email</span>
                <span className="text-sm font-semibold text-gray-900">
                  {registration.email}
                </span>
              </div>
              {registration.checkInCode && (
                <div className="flex justify-between items-center py-2">
                  <span className="text-sm text-gray-600">Mã check-in</span>
                  <span className="text-sm font-semibold text-blue-600">
                    {registration.checkInCode}
                  </span>
                </div>
              )}
            </div>
          </div>
        )}

        {/* Type 2: Guest List Registration Info */}
        {registration.type === 2 && (
          <>
            {/* Member Info Card */}
            <div className="bg-white rounded-xl shadow-lg border border-gray-200 overflow-hidden">
              <div className="bg-gray-50 px-4 py-3 border-b border-gray-200">
                <h2 className="text-base font-bold text-gray-900">
                  Thông tin người đăng ký
                </h2>
              </div>
              <div className="p-4 space-y-3">
                <div className="flex justify-between items-center py-2 border-b border-gray-100">
                  <span className="text-sm text-gray-600">Họ tên</span>
                  <span className="text-sm font-semibold text-gray-900">
                    {registration.memberName}
                  </span>
                </div>
                <div className="flex justify-between items-center py-2 border-b border-gray-100">
                  <span className="text-sm text-gray-600">Số điện thoại</span>
                  <span className="text-sm font-semibold text-gray-900">
                    {registration.memberPhone}
                  </span>
                </div>
                <div className="flex justify-between items-center py-2 border-b border-gray-100">
                  <span className="text-sm text-gray-600">Email</span>
                  <span className="text-sm font-semibold text-gray-900">
                    {registration.memberEmail}
                  </span>
                </div>
                {registration.memberCompany && (
                  <div className="flex justify-between items-center py-2 border-b border-gray-100">
                    <span className="text-sm text-gray-600">Công ty</span>
                    <span className="text-sm font-semibold text-gray-900">
                      {registration.memberCompany}
                    </span>
                  </div>
                )}
                {registration.memberPosition && (
                  <div className="flex justify-between items-center py-2">
                    <span className="text-sm text-gray-600">Chức vụ</span>
                    <span className="text-sm font-semibold text-gray-900">
                      {registration.memberPosition}
                    </span>
                  </div>
                )}
              </div>
            </div>

            {/* Guest Number */}
            <div className="bg-white rounded-xl shadow-lg border border-gray-200 overflow-hidden">
              <div className="p-4">
                <div className="flex items-center gap-2">
                  <svg
                    className="w-5 h-5 text-blue-500"
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
                  <span className="text-lg font-bold text-gray-900">
                    {registration.guestNumber} khách mời
                  </span>
                </div>
              </div>
            </div>
          </>
        )}

        {/* Note */}
        {registration.note && (
          <div className="bg-white rounded-xl shadow-lg border border-gray-200 overflow-hidden">
            <div className="bg-blue-50 px-4 py-3 border-b border-blue-200">
              <h2 className="text-base font-bold text-blue-900">Ghi chú</h2>
            </div>
            <div className="p-4">
              <p className="text-sm text-gray-700 leading-relaxed whitespace-pre-line">
                {registration.note}
              </p>
            </div>
          </div>
        )}

        {/* Guest List */}
        {registration.guestLists && registration.guestLists.length > 0 && (
          <div className="bg-white rounded-xl shadow-lg border border-gray-200 overflow-hidden">
            <div className="bg-gray-50 px-4 py-3 border-b border-gray-200">
              <h2 className="text-base font-bold text-gray-900">
                Danh sách khách mời ({registration.guestLists.length})
              </h2>
            </div>
            <div className="p-4">
              <div className="space-y-3">
                {registration.guestLists.map((guest, index) => (
                  <div
                    key={guest.id}
                    className="bg-gray-50 rounded-lg p-4 border border-gray-200"
                  >
                    <div className="flex items-center justify-between mb-3">
                      <h3 className="text-sm font-bold text-gray-900">
                        Khách #{index + 1}
                      </h3>
                      <span
                        className="px-2 py-1 rounded-full text-xs font-medium text-white"
                        style={{
                          background: getBackgroundColorByStatus(guest.status),
                        }}
                      >
                        {guest.statusText}
                      </span>
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

        {/* Reject/Cancel Reason */}
        {(registration.rejectReason || registration.cancelReason) && (
          <div className="bg-red-50 rounded-xl shadow-lg border border-red-200 overflow-hidden">
            <div className="bg-red-100 px-4 py-3 border-b border-red-200">
              <h2 className="text-base font-bold text-red-900">
                {registration.rejectReason ? "Lý do từ chối" : "Lý do hủy"}
              </h2>
            </div>
            <div className="p-4">
              <p className="text-sm text-red-800 leading-relaxed">
                {registration.rejectReason || registration.cancelReason}
              </p>
            </div>
          </div>
        )}
      </div>
    </Page>
  );
};

export default EventRegistrationDetail;
