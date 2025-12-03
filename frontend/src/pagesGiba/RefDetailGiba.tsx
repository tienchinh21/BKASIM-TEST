import React from "react";
import { Page, Box } from "zmp-ui";
import { useLocation, useNavigate } from "react-router-dom";
import useSetHeader from "../components/hooks/useSetHeader";
import {
  Star,
  StarIcon,
  MessageCircle,
  FileText,
  Users,
  DollarSign,
  Calendar,
  Clock,
  User,
  Phone,
  Mail,
  MapPin,
  ExternalLink,
} from "lucide-react";

interface RefItem {
  id: string;
  refFrom: string;
  refTo: string | null;
  content: string;
  status: number;
  statusText: string;
  value: number;
  groupId?: string;
  groupName?: string;
  refToGroupId?: string | null;
  refToGroupName?: string | null;
  type: number;
  typeText: string;
  shareType?: "own" | "member" | "external";
  referredMemberId: string | null;
  referredMemberGroupId?: string | null;
  referredMemberGroupName?: string | null;
  referralName: string | null;
  referralPhone: string | null;
  referralEmail: string | null;
  referralAddress: string | null;
  recipientName: string | null;
  recipientPhone: string | null;
  referredMemberSnapshot: string | null;
  createdDate: string;
  updatedDate: string;
  fromMemberName: string;
  fromMemberCompany: string;
  fromMemberPosition: string;
  fromMemberPhone?: string | null;
  fromMemberEmail?: string | null;
  fromMemberAvatar?: string | null;
  fromMemberSlug?: string | null;
  toMemberName: string | null;
  toMemberCompany: string | null;
  toMemberPosition: string | null;
  toMemberPhone?: string | null;
  toMemberEmail?: string | null;
  toMemberAvatar?: string | null;
  toMemberSlug?: string | null;
  referredMemberName?: string | null;
  referredMemberCompany?: string | null;
  referredMemberPosition?: string | null;
  referredMemberPhone?: string | null;
  referredMemberEmail?: string | null;
  referredMemberAvatar?: string | null;
  referredMemberSlug?: string | null;
  rating?: number;
  feedback?: string;
  ratingDate?: string;
}

const RefDetailGiba: React.FC = () => {
  const location = useLocation();
  const navigate = useNavigate();
  const setHeader = useSetHeader();
  const ref = location.state?.ref as RefItem;

  const handleViewProfile = (
    slug: string | null | undefined,
    groupId?: string | null
  ) => {
    if (!slug) {
      console.log("❌ handleViewProfile: No slug provided", { slug, groupId });
      return;
    }

    // Use slug as identifier (same as GroupDetailGiba.tsx)
    const identifier = slug;
    const url = groupId
      ? `/giba/member-detail/${identifier}?groupId=${groupId}`
      : `/giba/member-detail/${identifier}`;

    console.log("🔗 handleViewProfile - Navigation URL:", {
      slug,
      identifier,
      groupId,
      url,
      fullUrl: url,
    });

    navigate(url);
  };

  React.useEffect(() => {
    setHeader({
      title: "CHI TIẾT REFERRAL",
      showUserInfo: false,
      showMenuButton: false,
      showCloseButton: false,
      hasLeftIcon: true,
    });
  }, [setHeader]);

  const getStatusColor = (status: number) => {
    switch (status) {
      case 1:
        return "bg-blue-100 text-blue-700"; // Đã gửi
      case 2:
        return "bg-yellow-100 text-yellow-700"; // Đã xem
      case 3:
        return "bg-green-100 text-green-700"; // Đã chấp nhận
      case 4:
        return "bg-red-100 text-red-700"; // Đã từ chối
      default:
        return "bg-gray-100 text-gray-700";
    }
  };

  const getStatusGradient = (status: number) => {
    switch (status) {
      case 1:
        return "linear-gradient(90deg, #3b82f6, #2563eb)"; // Đã gửi (xanh dương)
      case 2:
        return "linear-gradient(90deg, #fbbf24, #f59e0b)"; // Đã xem (vàng)
      case 3:
        return "linear-gradient(90deg, #10b981, #059669)"; // Đã chấp nhận (xanh lá)
      case 4:
        return "linear-gradient(90deg, #ef4444, #dc2626)"; // Đã từ chối (đỏ)
      default:
        return "linear-gradient(90deg, #6b7280, #4b5563)"; // Mặc định (xám)
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

  if (!ref) {
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
              Không tìm thấy thông tin đơn ref
            </div>
          </div>
        </div>
      </Page>
    );
  }

  return (
    <Page style={{ marginTop: "50px", background: "#f8fafc" }}>
      <div className="bg-white rounded-lg shadow-sm p-4">
        <div className="flex items-center justify-between mb-4">
          <div>
            <p className="text-gray-600 text-sm">
              Mã số: {ref.id.slice(-8).toUpperCase()}
            </p>
            <p className="text-gray-500 text-xs mt-1">
              Nhóm: {ref.groupName || ref.refToGroupName || "N/A"}
            </p>
          </div>
          <div className="text-right flex flex-col items-end gap-2">
            <div
              className={`inline-flex items-center px-3 py-1.5 rounded-full text-sm font-semibold ${getStatusColor(
                ref.status
              )}`}
            >
              {ref.statusText}
              {ref.status === 2 && (
                <div className="w-2 h-2 bg-amber-400 rounded-full animate-pulse ml-2"></div>
              )}
            </div>
            <span
              className={`px-2 py-1 rounded text-xs font-medium ${
                ref.type === 0
                  ? "bg-yellow-100 text-yellow-700"
                  : "bg-blue-100 text-blue-700"
              }`}
            >
              {ref.typeText}
            </span>
          </div>
        </div>

        <div className="grid grid-cols-2 gap-4">
          <div>
            <div className="text-xs text-gray-500 mb-1 flex items-center gap-1">
              <Calendar size={14} />
              Ngày tạo
            </div>
            <div className="font-medium text-gray-900 text-sm">
              {formatDate(ref.createdDate)}
            </div>
          </div>
          {ref.updatedDate && ref.updatedDate !== ref.createdDate && (
            <div>
              <div className="text-xs text-gray-500 mb-1 flex items-center gap-1">
                <Clock size={14} />
                Cập nhật
              </div>
              <div className="font-medium text-gray-900 text-sm">
                {formatDate(ref.updatedDate)}
              </div>
            </div>
          )}
          {ref.value > 0 && (
            <div className="col-span-2">
              <div className="text-xs text-gray-500 mb-1 flex items-center gap-1">
                <DollarSign size={14} />
                Giá trị
              </div>
              <div className="font-bold text-green-600 text-base">
                {ref.value.toLocaleString()} VNĐ
              </div>
            </div>
          )}
        </div>
      </div>

      <div className="bg-white rounded-lg shadow-sm p-4">
        <h2 className="text-lg font-bold text-gray-900 mb-4 flex items-center gap-2">
          <Users size={20} />
          Thông tin các bên
        </h2>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div className="border-2 border-dashed border-blue-200 rounded-lg p-4 bg-blue-50/30">
            <div className="flex items-center mb-3">
              <div className="w-8 h-8 bg-blue-500 rounded-full flex items-center justify-center text-white font-bold mr-3 text-sm">
                A
              </div>
              <div>
                <h3 className="font-bold text-gray-900 text-sm">Người gửi</h3>
                <p className="text-xs text-gray-600">From</p>
              </div>
            </div>
            <div className="space-y-2">
              <div className="flex items-center">
                <span className="w-1.5 h-1.5 bg-blue-500 rounded-full mr-2"></span>
                <span className="text-gray-600 text-xs w-12">Họ tên:</span>
                <span className="font-semibold text-gray-900 text-sm">
                  {ref.fromMemberName}
                </span>
              </div>
              <div className="flex items-center">
                <span className="w-1.5 h-1.5 bg-blue-500 rounded-full mr-2"></span>
                <span className="text-gray-600 text-xs w-12">Công ty:</span>
                <span className="font-semibold text-gray-900 text-sm">
                  {ref.fromMemberCompany || "N/A"}
                </span>
              </div>
              <div className="flex items-center">
                <span className="w-1.5 h-1.5 bg-blue-500 rounded-full mr-2"></span>
                <span className="text-gray-600 text-xs w-12">Chức vụ:</span>
                <span className="font-semibold text-gray-900 text-sm">
                  {ref.fromMemberPosition}
                </span>
              </div>
            </div>
          </div>

          {ref.type === 0 ? (
            <div className="border-2 border-dashed border-green-200 rounded-lg p-4 bg-green-50/30">
              <div className="flex items-center mb-3">
                <div className="w-8 h-8 bg-green-500 rounded-full flex items-center justify-center text-white font-bold mr-3 text-sm">
                  B
                </div>
                <div>
                  <h3 className="font-bold text-gray-900 text-sm">
                    Người nhận
                  </h3>
                  <p className="text-xs text-gray-600">To (Member)</p>
                </div>
              </div>
              <div className="space-y-2">
                <div className="flex items-center">
                  <span className="w-1.5 h-1.5 bg-green-500 rounded-full mr-2"></span>
                  <span className="text-gray-600 text-xs w-12">Họ tên:</span>
                  <span className="font-semibold text-gray-900 text-sm">
                    {ref.toMemberName}
                  </span>
                </div>
                <div className="flex items-center">
                  <span className="w-1.5 h-1.5 bg-green-500 rounded-full mr-2"></span>
                  <span className="text-gray-600 text-xs w-12">Công ty:</span>
                  <span className="font-semibold text-gray-900 text-sm">
                    {ref.toMemberCompany || "N/A"}
                  </span>
                </div>
                <div className="flex items-center">
                  <span className="w-1.5 h-1.5 bg-green-500 rounded-full mr-2"></span>
                  <span className="text-gray-600 text-xs w-12">Chức vụ:</span>
                  <span className="font-semibold text-gray-900 text-sm">
                    {ref.toMemberPosition || "N/A"}
                  </span>
                </div>
              </div>
            </div>
          ) : (
            <div className="border-2 border-dashed border-purple-200 rounded-lg p-4 bg-purple-50/30">
              <div className="flex items-center mb-3">
                <div className="w-8 h-8 bg-purple-500 rounded-full flex items-center justify-center text-white font-bold mr-3 text-sm">
                  B
                </div>
                <div>
                  <h3 className="font-bold text-gray-900 text-sm">
                    Người nhận
                  </h3>
                  <p className="text-xs text-gray-600">To (External)</p>
                </div>
              </div>
              <div className="space-y-2">
                {ref.recipientName && (
                  <div className="flex items-center">
                    <span className="w-1.5 h-1.5 bg-purple-500 rounded-full mr-2"></span>
                    <span className="text-gray-600 text-xs w-12">Họ tên:</span>
                    <span className="font-semibold text-gray-900 text-sm">
                      {ref.recipientName}
                    </span>
                  </div>
                )}
                <div className="flex items-center">
                  <span className="w-1.5 h-1.5 bg-purple-500 rounded-full mr-2"></span>
                  <span className="text-gray-600 text-xs w-12">SĐT:</span>
                  <span className="font-semibold text-gray-900 text-sm">
                    {ref.recipientPhone}
                  </span>
                </div>
                <div className="flex items-center">
                  <span className="w-1.5 h-1.5 bg-purple-500 rounded-full mr-2"></span>
                  <span className="text-gray-600 text-xs w-12">Loại:</span>
                  <span className="font-semibold text-gray-900 text-sm">
                    Bên ngoài
                  </span>
                </div>
              </div>
            </div>
          )}
        </div>
      </div>

      {ref.type === 0 && (
        <div className="bg-white rounded-lg shadow-sm p-4">
          <h2 className="text-lg font-bold text-gray-900 mb-4 flex items-center gap-2">
            <User size={20} />
            Thông tin người được giới thiệu
          </h2>

          <div className="bg-gradient-to-br from-yellow-50 to-orange-50 rounded-lg p-4 border border-yellow-200">
            {ref.shareType === "own" && (
              <div className="space-y-3">
                <div className="mb-3 pb-2 border-b border-yellow-300">
                  <span className="text-xs font-semibold text-yellow-700 bg-yellow-100 px-2 py-1 rounded">
                    Profile bản thân
                  </span>
                </div>
                <div className="flex items-start gap-3">
                  <User size={16} className="text-yellow-600 mt-0.5" />
                  <div className="flex-1">
                    <div className="text-xs text-gray-600 mb-1">Họ tên</div>
                    <div className="font-semibold text-gray-900">
                      {ref.fromMemberName}
                    </div>
                  </div>
                </div>
                {ref.fromMemberPhone && (
                  <div className="flex items-start gap-3">
                    <Phone size={16} className="text-yellow-600 mt-0.5" />
                    <div className="flex-1">
                      <div className="text-xs text-gray-600 mb-1">
                        Số điện thoại
                      </div>
                      <div className="font-semibold text-gray-900">
                        {ref.fromMemberPhone}
                      </div>
                    </div>
                  </div>
                )}
                {ref.fromMemberCompany && (
                  <div className="flex items-start gap-3">
                    <Users size={16} className="text-yellow-600 mt-0.5" />
                    <div className="flex-1">
                      <div className="text-xs text-gray-600 mb-1">Công ty</div>
                      <div className="font-semibold text-gray-900">
                        {ref.fromMemberCompany}
                      </div>
                    </div>
                  </div>
                )}
                {ref.fromMemberPosition && (
                  <div className="flex items-start gap-3">
                    <User size={16} className="text-yellow-600 mt-0.5" />
                    <div className="flex-1">
                      <div className="text-xs text-gray-600 mb-1">Chức vụ</div>
                      <div className="font-semibold text-gray-900">
                        {ref.fromMemberPosition}
                      </div>
                    </div>
                  </div>
                )}
                {ref.fromMemberSlug && (
                  <button
                    onClick={() =>
                      handleViewProfile(ref.fromMemberSlug, ref.refToGroupId)
                    }
                    className="mt-3 w-full flex items-center justify-center gap-2 px-4 py-2 bg-yellow-500 hover:bg-yellow-600 text-white rounded-lg text-sm font-semibold transition-colors"
                  >
                    <ExternalLink size={16} />
                    Xem profile
                  </button>
                )}
              </div>
            )}

            {ref.shareType === "member" && (
              <div className="space-y-3">
                <div className="mb-3 pb-2 border-b border-yellow-300">
                  <span className="text-xs font-semibold text-yellow-700 bg-yellow-100 px-2 py-1 rounded">
                    Profile thành viên
                  </span>
                  {ref.referredMemberGroupName && (
                    <span className="text-xs text-gray-600 ml-2">
                      ({ref.referredMemberGroupName})
                    </span>
                  )}
                </div>
                {ref.referredMemberName && (
                  <div className="flex items-start gap-3">
                    <User size={16} className="text-yellow-600 mt-0.5" />
                    <div className="flex-1">
                      <div className="text-xs text-gray-600 mb-1">Họ tên</div>
                      <div className="font-semibold text-gray-900">
                        {ref.referredMemberName}
                      </div>
                    </div>
                  </div>
                )}
                {ref.referredMemberPhone && (
                  <div className="flex items-start gap-3">
                    <Phone size={16} className="text-yellow-600 mt-0.5" />
                    <div className="flex-1">
                      <div className="text-xs text-gray-600 mb-1">
                        Số điện thoại
                      </div>
                      <div className="font-semibold text-gray-900">
                        {ref.referredMemberPhone}
                      </div>
                    </div>
                  </div>
                )}
                {ref.referredMemberCompany && (
                  <div className="flex items-start gap-3">
                    <Users size={16} className="text-yellow-600 mt-0.5" />
                    <div className="flex-1">
                      <div className="text-xs text-gray-600 mb-1">Công ty</div>
                      <div className="font-semibold text-gray-900">
                        {ref.referredMemberCompany}
                      </div>
                    </div>
                  </div>
                )}
                {ref.referredMemberPosition && (
                  <div className="flex items-start gap-3">
                    <User size={16} className="text-yellow-600 mt-0.5" />
                    <div className="flex-1">
                      <div className="text-xs text-gray-600 mb-1">Chức vụ</div>
                      <div className="font-semibold text-gray-900">
                        {ref.referredMemberPosition}
                      </div>
                    </div>
                  </div>
                )}
                {ref.referredMemberSlug && (
                  <button
                    onClick={() =>
                      handleViewProfile(
                        ref.referredMemberSlug,
                        ref.referredMemberGroupId
                      )
                    }
                    className="mt-3 w-full flex items-center justify-center gap-2 px-4 py-2 bg-yellow-500 hover:bg-yellow-600 text-white rounded-lg text-sm font-semibold transition-colors"
                  >
                    <ExternalLink size={16} />
                    Xem profile
                  </button>
                )}
              </div>
            )}

            {ref.shareType === "external" && (
              <div className="space-y-3">
                <div className="mb-3 pb-2 border-b border-yellow-300">
                  <span className="text-xs font-semibold text-yellow-700 bg-yellow-100 px-2 py-1 rounded">
                    Thông tin người ngoài
                  </span>
                </div>
                {ref.referralName && (
                  <div className="flex items-start gap-3">
                    <User size={16} className="text-yellow-600 mt-0.5" />
                    <div className="flex-1">
                      <div className="text-xs text-gray-600 mb-1">Họ tên</div>
                      <div className="font-semibold text-gray-900">
                        {ref.referralName}
                      </div>
                    </div>
                  </div>
                )}
                {ref.referralPhone && (
                  <div className="flex items-start gap-3">
                    <Phone size={16} className="text-yellow-600 mt-0.5" />
                    <div className="flex-1">
                      <div className="text-xs text-gray-600 mb-1">
                        Số điện thoại
                      </div>
                      <div className="font-semibold text-gray-900">
                        {ref.referralPhone}
                      </div>
                    </div>
                  </div>
                )}
                {ref.referralEmail && (
                  <div className="flex items-start gap-3">
                    <Mail size={16} className="text-yellow-600 mt-0.5" />
                    <div className="flex-1">
                      <div className="text-xs text-gray-600 mb-1">Email</div>
                      <div className="font-semibold text-gray-900">
                        {ref.referralEmail}
                      </div>
                    </div>
                  </div>
                )}
                {ref.referralAddress && (
                  <div className="flex items-start gap-3">
                    <MapPin size={16} className="text-yellow-600 mt-0.5" />
                    <div className="flex-1">
                      <div className="text-xs text-gray-600 mb-1">Địa chỉ</div>
                      <div className="font-semibold text-gray-900">
                        {ref.referralAddress}
                      </div>
                    </div>
                  </div>
                )}
              </div>
            )}

            {/* Fallback cho trường hợp không có shareType (backward compatibility) */}
            {!ref.shareType && (
              <div className="space-y-3">
                {ref.referralName && (
                  <div className="flex items-start gap-3">
                    <User size={16} className="text-yellow-600 mt-0.5" />
                    <div className="flex-1">
                      <div className="text-xs text-gray-600 mb-1">Họ tên</div>
                      <div className="font-semibold text-gray-900">
                        {ref.referralName}
                      </div>
                    </div>
                  </div>
                )}
                {ref.referralPhone && (
                  <div className="flex items-start gap-3">
                    <Phone size={16} className="text-yellow-600 mt-0.5" />
                    <div className="flex-1">
                      <div className="text-xs text-gray-600 mb-1">
                        Số điện thoại
                      </div>
                      <div className="font-semibold text-gray-900">
                        {ref.referralPhone}
                      </div>
                    </div>
                  </div>
                )}
                {ref.referralAddress && (
                  <div className="flex items-start gap-3">
                    <MapPin size={16} className="text-yellow-600 mt-0.5" />
                    <div className="flex-1">
                      <div className="text-xs text-gray-600 mb-1">Địa chỉ</div>
                      <div className="font-semibold text-gray-900">
                        {ref.referralAddress}
                      </div>
                    </div>
                  </div>
                )}
              </div>
            )}
          </div>
        </div>
      )}

      {ref.type === 1 && (
        <div className="bg-white rounded-lg shadow-sm p-4">
          <h2 className="text-lg font-bold text-gray-900 mb-4 flex items-center gap-2">
            <User size={20} />
            Thông tin được chia sẻ
          </h2>

          <div className="bg-gradient-to-br from-blue-50 to-indigo-50 rounded-lg p-4 border border-blue-200">
            {ref.shareType === "own" && (
              <div className="space-y-3">
                <div className="mb-3 pb-2 border-b border-blue-300">
                  <span className="text-xs font-semibold text-blue-700 bg-blue-100 px-2 py-1 rounded">
                    Profile bản thân
                  </span>
                </div>
                <div className="flex items-start gap-3">
                  <User size={16} className="text-blue-600 mt-0.5" />
                  <div className="flex-1">
                    <div className="text-xs text-gray-600 mb-1">Họ tên</div>
                    <div className="font-semibold text-gray-900">
                      {ref.fromMemberName}
                    </div>
                  </div>
                </div>
                {ref.fromMemberPhone && (
                  <div className="flex items-start gap-3">
                    <Phone size={16} className="text-blue-600 mt-0.5" />
                    <div className="flex-1">
                      <div className="text-xs text-gray-600 mb-1">
                        Số điện thoại
                      </div>
                      <div className="font-semibold text-gray-900">
                        {ref.fromMemberPhone}
                      </div>
                    </div>
                  </div>
                )}
                {ref.fromMemberCompany && (
                  <div className="flex items-start gap-3">
                    <Users size={16} className="text-blue-600 mt-0.5" />
                    <div className="flex-1">
                      <div className="text-xs text-gray-600 mb-1">Công ty</div>
                      <div className="font-semibold text-gray-900">
                        {ref.fromMemberCompany}
                      </div>
                    </div>
                  </div>
                )}
                {ref.fromMemberPosition && (
                  <div className="flex items-start gap-3">
                    <User size={16} className="text-blue-600 mt-0.5" />
                    <div className="flex-1">
                      <div className="text-xs text-gray-600 mb-1">Chức vụ</div>
                      <div className="font-semibold text-gray-900">
                        {ref.fromMemberPosition}
                      </div>
                    </div>
                  </div>
                )}
                {ref.fromMemberSlug && (
                  <button
                    onClick={() => handleViewProfile(ref.fromMemberSlug)}
                    className="mt-3 w-full flex items-center justify-center gap-2 px-4 py-2 bg-blue-500 hover:bg-blue-600 text-white rounded-lg text-sm font-semibold transition-colors"
                  >
                    <ExternalLink size={16} />
                    Xem profile
                  </button>
                )}
              </div>
            )}

            {ref.shareType === "member" && (
              <div className="space-y-3">
                <div className="mb-3 pb-2 border-b border-blue-300">
                  <span className="text-xs font-semibold text-blue-700 bg-blue-100 px-2 py-1 rounded">
                    Profile thành viên
                  </span>
                  {ref.referredMemberGroupName && (
                    <span className="text-xs text-gray-600 ml-2">
                      ({ref.referredMemberGroupName})
                    </span>
                  )}
                </div>
                {ref.referredMemberName && (
                  <div className="flex items-start gap-3">
                    <User size={16} className="text-blue-600 mt-0.5" />
                    <div className="flex-1">
                      <div className="text-xs text-gray-600 mb-1">Họ tên</div>
                      <div className="font-semibold text-gray-900">
                        {ref.referredMemberName}
                      </div>
                    </div>
                  </div>
                )}
                {ref.referredMemberPhone && (
                  <div className="flex items-start gap-3">
                    <Phone size={16} className="text-blue-600 mt-0.5" />
                    <div className="flex-1">
                      <div className="text-xs text-gray-600 mb-1">
                        Số điện thoại
                      </div>
                      <div className="font-semibold text-gray-900">
                        {ref.referredMemberPhone}
                      </div>
                    </div>
                  </div>
                )}
                {ref.referredMemberCompany && (
                  <div className="flex items-start gap-3">
                    <Users size={16} className="text-blue-600 mt-0.5" />
                    <div className="flex-1">
                      <div className="text-xs text-gray-600 mb-1">Công ty</div>
                      <div className="font-semibold text-gray-900">
                        {ref.referredMemberCompany}
                      </div>
                    </div>
                  </div>
                )}
                {ref.referredMemberPosition && (
                  <div className="flex items-start gap-3">
                    <User size={16} className="text-blue-600 mt-0.5" />
                    <div className="flex-1">
                      <div className="text-xs text-gray-600 mb-1">Chức vụ</div>
                      <div className="font-semibold text-gray-900">
                        {ref.referredMemberPosition}
                      </div>
                    </div>
                  </div>
                )}
                {ref.referredMemberSlug && (
                  <button
                    onClick={() =>
                      handleViewProfile(
                        ref.referredMemberSlug,
                        ref.referredMemberGroupId
                      )
                    }
                    className="mt-3 w-full flex items-center justify-center gap-2 px-4 py-2 bg-blue-500 hover:bg-blue-600 text-white rounded-lg text-sm font-semibold transition-colors"
                  >
                    <ExternalLink size={16} />
                    Xem profile
                  </button>
                )}
              </div>
            )}

            {ref.shareType === "external" && (
              <div className="space-y-3">
                <div className="mb-3 pb-2 border-b border-blue-300">
                  <span className="text-xs font-semibold text-blue-700 bg-blue-100 px-2 py-1 rounded">
                    Thông tin người ngoài
                  </span>
                </div>
                {ref.referralName && (
                  <div className="flex items-start gap-3">
                    <User size={16} className="text-blue-600 mt-0.5" />
                    <div className="flex-1">
                      <div className="text-xs text-gray-600 mb-1">Họ tên</div>
                      <div className="font-semibold text-gray-900">
                        {ref.referralName}
                      </div>
                    </div>
                  </div>
                )}
                {ref.referralPhone && (
                  <div className="flex items-start gap-3">
                    <Phone size={16} className="text-blue-600 mt-0.5" />
                    <div className="flex-1">
                      <div className="text-xs text-gray-600 mb-1">
                        Số điện thoại
                      </div>
                      <div className="font-semibold text-gray-900">
                        {ref.referralPhone}
                      </div>
                    </div>
                  </div>
                )}
                {ref.referralAddress && (
                  <div className="flex items-start gap-3">
                    <MapPin size={16} className="text-blue-600 mt-0.5" />
                    <div className="flex-1">
                      <div className="text-xs text-gray-600 mb-1">Địa chỉ</div>
                      <div className="font-semibold text-gray-900">
                        {ref.referralAddress}
                      </div>
                    </div>
                  </div>
                )}
              </div>
            )}

            {!ref.shareType && (
              <div className="space-y-3">
                {ref.referralName && (
                  <div className="flex items-start gap-3">
                    <User size={16} className="text-blue-600 mt-0.5" />
                    <div className="flex-1">
                      <div className="text-xs text-gray-600 mb-1">Họ tên</div>
                      <div className="font-semibold text-gray-900">
                        {ref.referralName}
                      </div>
                    </div>
                  </div>
                )}
                {ref.referralPhone && (
                  <div className="flex items-start gap-3">
                    <Phone size={16} className="text-blue-600 mt-0.5" />
                    <div className="flex-1">
                      <div className="text-xs text-gray-600 mb-1">
                        Số điện thoại
                      </div>
                      <div className="font-semibold text-gray-900">
                        {ref.referralPhone}
                      </div>
                    </div>
                  </div>
                )}
              </div>
            )}
          </div>
        </div>
      )}

      {ref.content && (
        <div className="bg-white rounded-lg shadow-sm p-4">
          <h2 className="text-lg font-bold text-gray-900 mb-4 flex items-center gap-2">
            <FileText size={20} />
            Nội dung / Ghi chú
          </h2>

          <div className="bg-gray-50 rounded-lg p-4 border-l-4 border-purple-500">
            <div className="text-gray-800 leading-relaxed whitespace-pre-line text-sm">
              {ref.content}
            </div>
          </div>
        </div>
      )}

      {ref.status === 3 && (ref.rating || ref.feedback) && (
        <div className="bg-white rounded-lg shadow-sm p-4">
          <h2 className="text-lg font-bold text-gray-900 mb-4 flex items-center gap-2">
            <Star size={20} />
            Đánh giá & Lời cảm ơn
          </h2>

          {ref.rating && (
            <div className="mb-4">
              <div className="flex items-center gap-2 mb-2">
                <span className="text-sm font-semibold text-gray-700">
                  Đánh giá:
                </span>
                <div className="flex items-center gap-1">
                  {[1, 2, 3, 4, 5].map((star) => (
                    <div
                      key={star}
                      className="flex items-center justify-center"
                    >
                      {star <= ref.rating! ? (
                        <StarIcon
                          size={24}
                          className="text-yellow-400"
                          style={{ strokeWidth: 2 }}
                        />
                      ) : (
                        <Star
                          size={24}
                          className="text-gray-300"
                          style={{ strokeWidth: 2 }}
                        />
                      )}
                    </div>
                  ))}
                  <span className="text-sm text-gray-600 ml-2 font-medium">
                    {ref.rating}/5 sao
                  </span>
                </div>
              </div>
            </div>
          )}

          {ref.feedback && (
            <div className="bg-yellow-50 rounded-lg p-4 border-l-4 border-yellow-400">
              <div className="flex items-center gap-2 mb-2">
                <MessageCircle size={16} className="text-yellow-600" />
                <span className="text-sm font-semibold text-yellow-800">
                  Lời cảm ơn:
                </span>
              </div>
              <div className="text-yellow-800 leading-relaxed text-sm">
                "{ref.feedback}"
              </div>
            </div>
          )}

          {ref.ratingDate && (
            <div className="mt-3 text-xs text-gray-500">
              Đánh giá lúc: {formatDate(ref.ratingDate)}
            </div>
          )}
        </div>
      )}
    </Page>
  );
};

export default RefDetailGiba;
