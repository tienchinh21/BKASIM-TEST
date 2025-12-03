import React, { useState, useEffect, useCallback, useMemo } from "react";
import { Page, Box } from "zmp-ui";
import { useNavigate, useLocation } from "react-router-dom";
import useSetHeader from "../components/hooks/useSetHeader";
import { useRecoilValue } from "recoil";
import { token } from "../recoil/RecoilState";
import axios from "axios";
import LoadingGiba from "../componentsGiba/LoadingGiba";
import { Drawer, Input } from "antd";
import { toast } from "react-toastify";
import dfData from "../common/DefaultConfig.json";
import {
  Star,
  StarIcon,
  DollarSign,
  MessageCircle,
  User,
  Plus,
} from "lucide-react";
import FloatingActionButtonGiba from "../componentsGiba/FloatingActionButtonGiba";
import RefOptionModal from "./ref/RefOptionModal";
import Category from "../components/Category";

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

const RefListGiba: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const setHeader = useSetHeader();
  const userToken = useRecoilValue(token);
  const [loading, setLoading] = useState(false);
  const [refs, setRefs] = useState<RefItem[]>([]);
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  
  // Get initial filter from location state or default to "sent"
  const initialFilter = (location.state as any)?.filter as "sent" | "received" | undefined;
  const [selectedFilter, setSelectedFilter] = useState<"sent" | "received">(
    initialFilter || "sent"
  );

  const filterCategories = [
    { id: "sent", name: "Đã gửi" },
    { id: "received", name: "Đã nhận" },
  ];

  const [showCompleteDrawer, setShowCompleteDrawer] = useState(false);
  const [completingRef, setCompletingRef] = useState<RefItem | null>(null);
  const [refValue, setRefValue] = useState("");
  const [rating, setRating] = useState(0);
  const [feedback, setFeedback] = useState("");
  const [completing, setCompleting] = useState(false);

  const [showRefOptionModal, setShowRefOptionModal] = useState(false);

  React.useEffect(() => {
    setHeader({
      title: "DANH SÁCH REFERRAL",
      showUserInfo: false,
      showMenuButton: false,
      showCloseButton: false,
      hasLeftIcon: true,
    });
  }, [setHeader]);

  useEffect(() => {
    const fetchRefs = async () => {
      if (!userToken) return;

      try {
        setLoading(true);
        let url = `${dfData.domain}/api/refs/list?type=${selectedFilter}&page=${currentPage}&pageSize=10`;

        const response = await axios.get(url, {
          headers: {
            Authorization: `Bearer ${userToken}`,
          },
        });

        if (response.data.code === 0 && response.data.data) {
          setRefs(response.data.data.items || []);
          setTotalPages(response.data.data.totalPages || 1);
        } else {
          setRefs([]);
          setTotalPages(1);
        }
      } catch (error) {
        console.error("Error fetching refs:", error);
        toast.error("Không thể tải danh sách ref");
        setRefs([]);
        setTotalPages(1);
      } finally {
        setLoading(false);
      }
    };

    fetchRefs();
  }, [currentPage, selectedFilter, userToken]);

  // const statusCategories = [
  //   { id: "", name: "Tất cả" },
  //   { id: "1", name: "Đã gửi" },
  //   { id: "2", name: "Đã xem" },
  //   { id: "3", name: "Đã chấp nhận" },
  //   { id: "4", name: "Đã từ chối" },
  // ];

  const getStatusColor = useCallback((status: number) => {
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
  }, []);

  const getStatusGradient = useCallback((status: number) => {
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
  }, []);

  const handleCompleteClick = useCallback((ref: RefItem) => {
    setCompletingRef(ref);
    setRefValue("");
    setRating(0);
    setFeedback("");
    setShowCompleteDrawer(true);
  }, []);

  const handleCompleteRef = useCallback(async () => {
    if (!refValue.trim()) {
      toast.error("Vui lòng nhập giá trị ref");
      return;
    }

    const numValue = parseCurrency(refValue);
    if (numValue <= 0) {
      toast.error("Giá trị ref phải lớn hơn 0");
      return;
    }

    if (numValue < 1000) {
      toast.error("Giá trị ref tối thiểu là 1.000 VNĐ");
      return;
    }

    if (rating === 0) {
      toast.error("Vui lòng đánh giá từ 1-5 sao");
      return;
    }

    if (!feedback.trim()) {
      toast.error("Vui lòng nhập lời cảm ơn");
      return;
    }

    try {
      setCompleting(true);
      const response = await axios.put(
        `${dfData.domain}/api/refs/update-value/${completingRef?.id}`,
        {
          value: numValue,
          rating: rating,
          feedback: feedback.trim(),
        },
        {
          headers: {
            Authorization: `Bearer ${userToken}`,
          },
        }
      );

      if (response.data.code === 0 || response.data.success) {
        toast.success("Hoàn thành ref thành công");
        setShowCompleteDrawer(false);
        const updatedRefs = refs.map((ref) =>
          ref.id === completingRef?.id ? { ...ref, value: numValue } : ref
        );
        setRefs(updatedRefs);
      } else {
        toast.error(response.data.message || "Không thể hoàn thành ref");
      }
    } catch (error) {
      console.error("Error completing ref:", error);
      toast.error("Có lỗi xảy ra khi hoàn thành ref");
    } finally {
      setCompleting(false);
    }
  }, [refValue, rating, feedback, completingRef, userToken, refs]);

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

  // Format number with thousand separators
  const formatCurrency = useCallback((value: string) => {
    const num = value.replace(/\D/g, "");
    return num.replace(/\B(?=(\d{3})+(?!\d))/g, ".");
  }, []);

  // Parse formatted currency back to number
  const parseCurrency = useCallback((value: string) => {
    return parseInt(value.replace(/\./g, "")) || 0;
  }, []);

  const handleViewDetail = useCallback(
    (ref: RefItem) => {
      navigate("/giba/ref-detail", {
        state: { ref },
      });
    },
    [navigate]
  );

  const handleCreateRef = useCallback(() => {
    setShowRefOptionModal(true);
  }, []);

  const handleSelectRefOption = useCallback(
    (option: "option1" | "option2") => {
      setShowRefOptionModal(false);
      navigate("/giba/ref-create", {
        state: { optionType: option },
      });
    },
    [navigate]
  );

  const handleFilterChange = useCallback((filterId: string) => {
    setSelectedFilter(filterId as "sent" | "received");
    setCurrentPage(1);
  }, []);

  // Memoized RefCard component
  const RefCard = React.memo(
    ({
      refItem,
      filterType,
      onViewDetail,
      onCompleteClick,
      getStatusColor,
      getStatusGradient,
      formatDate,
    }: {
      refItem: RefItem;
      filterType: "sent" | "received";
      onViewDetail: (ref: RefItem) => void;
      onCompleteClick: (ref: RefItem) => void;
      getStatusColor: (status: number) => string;
      getStatusGradient: (status: number) => string;
      formatDate: (dateString: string) => string;
    }) => {
      const getRecipientInfo = () => {
        if (refItem.type === 0) {
          return {
            name:
              filterType === "sent"
                ? refItem.toMemberName
                : refItem.fromMemberName,
            company:
              filterType === "sent"
                ? refItem.toMemberCompany
                : refItem.fromMemberCompany,
            position:
              filterType === "sent"
                ? refItem.toMemberPosition
                : refItem.fromMemberPosition,
          };
        } else {
          return {
            name:
              filterType === "sent"
                ? refItem.recipientName || refItem.recipientPhone
                : refItem.fromMemberName,
            company:
              filterType === "sent" ? "Bên ngoài" : refItem.fromMemberCompany,
            position: filterType === "sent" ? "" : refItem.fromMemberPosition,
          };
        }
      };

      const recipientInfo = getRecipientInfo();

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
              background: getStatusGradient(refItem.status),
            }}
          ></div>

          <div className="mb-2">
            <div className="flex items-center justify-between mb-2">
              <span
                className={`px-2 py-0.5 rounded text-xs font-medium ${
                  refItem.type === 0
                    ? "bg-yellow-100 text-yellow-700"
                    : "bg-blue-100 text-blue-700"
                }`}
              >
                {refItem.typeText}
              </span>
              <div className="flex items-center gap-2">
                <span
                  className={`px-2 py-1 rounded-full text-xs font-medium ${getStatusColor(
                    refItem.status
                  )}`}
                >
                  {refItem.statusText}
                </span>
                {refItem.status === 2 && (
                  <div className="w-2 h-2 bg-amber-400 rounded-full animate-pulse"></div>
                )}
              </div>
            </div>

            <div className="flex-1 min-w-0">
              <div className="text-sm font-bold text-gray-900 truncate mb-1">
                {filterType === "sent" ? "Người nhận" : "Người gửi"}:{" "}
                {recipientInfo.name}
              </div>
              <div className="text-xs text-gray-500">
                ID: {refItem.id.slice(-6).toUpperCase()}
              </div>
            </div>
          </div>

          {refItem.status === 3 && refItem.rating && (
            <div className="mb-2">
              <div className="inline-flex items-center gap-1 px-2 py-1 bg-yellow-100 text-yellow-800 rounded-full text-xs font-medium">
                <StarIcon size={12} className="text-yellow-500" />
                <span>{refItem.rating}/5 sao</span>
              </div>
            </div>
          )}
          {refItem.value > 0 && (
            <div className="mb-2">
              <div className="inline-flex items-center gap-1 px-2 py-1 bg-green-100 text-green-800 rounded-full text-xs font-medium">
                <DollarSign size={12} className="text-green-600" />
                <span>{refItem.value.toLocaleString()} VNĐ</span>
              </div>
            </div>
          )}
          {refItem.feedback && (
            <div className="mb-2">
              <div className="text-xs text-gray-600 italic bg-gray-50 p-2 rounded border-l-2 border-gray-300">
                <span className="font-medium text-gray-700">Lời cảm ơn: </span>
                {refItem.feedback}
              </div>
            </div>
          )}

          <div className="flex items-center justify-between text-xs text-gray-600 mb-2">
            <div className="flex items-center gap-1">
              <svg className="w-3 h-3" fill="currentColor" viewBox="0 0 20 20">
                <path
                  fillRule="evenodd"
                  d="M5.05 4.05a7 7 0 119.9 9.9L10 18.9l-4.95-4.95a7 7 0 010-9.9zM10 11a2 2 0 100-4 2 2 0 000 4z"
                  clipRule="evenodd"
                />
              </svg>
              <span>
                {recipientInfo.company}
                {recipientInfo.position && ` - ${recipientInfo.position}`}
              </span>
            </div>
            <div className="text-gray-500">
              {formatDate(refItem.createdDate)}
            </div>
          </div>

          <div className="bg-blue-50 border-l-2 border-blue-400 p-2 rounded-r mb-2">
            <div className="text-xs text-blue-600 font-medium mb-1">
              {refItem.type === 0
                ? "Thông tin người được giới thiệu"
                : "Nội dung"}
            </div>
            {refItem.type === 0 ? (
              <div className="text-xs text-blue-800 space-y-1">
                {/* Type 0: Gửi cho thành viên */}
                {refItem.shareType === "own" && (
                  <>
                    <div className="font-semibold text-blue-900 mb-1">
                      Profile bản thân
                    </div>
                    <div>Tên: {refItem.fromMemberName}</div>
                    {refItem.fromMemberPhone && (
                      <div>SĐT: {refItem.fromMemberPhone}</div>
                    )}
                    {refItem.fromMemberCompany && (
                      <div>Công ty: {refItem.fromMemberCompany}</div>
                    )}
                    {refItem.fromMemberPosition && (
                      <div>Chức vụ: {refItem.fromMemberPosition}</div>
                    )}
                  </>
                )}
                {refItem.shareType === "member" && (
                  <>
                    <div className="font-semibold text-blue-900 mb-1">
                      Profile thành viên
                    </div>
                    {refItem.referredMemberName && (
                      <div>Tên: {refItem.referredMemberName}</div>
                    )}
                    {refItem.referredMemberPhone && (
                      <div>SĐT: {refItem.referredMemberPhone}</div>
                    )}
                    {refItem.referredMemberCompany && (
                      <div>Công ty: {refItem.referredMemberCompany}</div>
                    )}
                    {refItem.referredMemberPosition && (
                      <div>Chức vụ: {refItem.referredMemberPosition}</div>
                    )}
                    {refItem.referredMemberEmail && (
                      <div>Email: {refItem.referredMemberEmail}</div>
                    )}
                  </>
                )}
                {refItem.shareType === "external" && (
                  <>
                    <div className="font-semibold text-blue-900 mb-1">
                      Thông tin người ngoài
                    </div>
                    {refItem.referralName && (
                      <div>Tên: {refItem.referralName}</div>
                    )}
                    {refItem.referralPhone && (
                      <div>SĐT: {refItem.referralPhone}</div>
                    )}
                    {refItem.referralEmail && (
                      <div>Email: {refItem.referralEmail}</div>
                    )}
                    {refItem.referralAddress && (
                      <div>Địa chỉ: {refItem.referralAddress}</div>
                    )}
                  </>
                )}
                {/* Fallback cho trường hợp không có shareType (backward compatibility) */}
                {!refItem.shareType && (
                  <>
                    {refItem.referralName && (
                      <div>Tên: {refItem.referralName}</div>
                    )}
                    {refItem.referralPhone && (
                      <div>SĐT: {refItem.referralPhone}</div>
                    )}
                  </>
                )}
                {refItem.content && (
                  <div className="line-clamp-1 mt-1 text-gray-600">
                    Ghi chú: {refItem.content}
                  </div>
                )}
              </div>
            ) : (
              <div className="text-xs text-blue-800 space-y-1">
                {/* Type 1: Gửi cho người ngoài */}
                {refItem.shareType === "own" && (
                  <>
                    <div className="font-semibold text-blue-900 mb-1">
                      Profile bản thân
                    </div>
                    <div>Tên: {refItem.fromMemberName}</div>
                    {refItem.fromMemberPhone && (
                      <div>SĐT: {refItem.fromMemberPhone}</div>
                    )}
                    {refItem.fromMemberCompany && (
                      <div>Công ty: {refItem.fromMemberCompany}</div>
                    )}
                    {refItem.fromMemberPosition && (
                      <div>Chức vụ: {refItem.fromMemberPosition}</div>
                    )}
                  </>
                )}
                {refItem.shareType === "member" && (
                  <>
                    <div className="font-semibold text-blue-900 mb-1">
                      Profile thành viên
                    </div>
                    {refItem.referredMemberName && (
                      <div>Tên: {refItem.referredMemberName}</div>
                    )}
                    {refItem.referredMemberPhone && (
                      <div>SĐT: {refItem.referredMemberPhone}</div>
                    )}
                    {refItem.referredMemberCompany && (
                      <div>Công ty: {refItem.referredMemberCompany}</div>
                    )}
                    {refItem.referredMemberPosition && (
                      <div>Chức vụ: {refItem.referredMemberPosition}</div>
                    )}
                    {refItem.referredMemberEmail && (
                      <div>Email: {refItem.referredMemberEmail}</div>
                    )}
                  </>
                )}
                {refItem.shareType === "external" && (
                  <>
                    <div className="font-semibold text-blue-900 mb-1">
                      Thông tin người ngoài
                    </div>
                    {refItem.referralName && (
                      <div>Tên: {refItem.referralName}</div>
                    )}
                    {refItem.referralPhone && (
                      <div>SĐT: {refItem.referralPhone}</div>
                    )}
                    {refItem.referralEmail && (
                      <div>Email: {refItem.referralEmail}</div>
                    )}
                    {refItem.referralAddress && (
                      <div>Địa chỉ: {refItem.referralAddress}</div>
                    )}
                  </>
                )}
                {/* Fallback cho trường hợp không có shareType */}
                {!refItem.shareType && (
                  <>
                    {refItem.referralName && (
                      <div>Thành viên: {refItem.referralName}</div>
                    )}
                    {refItem.referralPhone && (
                      <div>SĐT TV: {refItem.referralPhone}</div>
                    )}
                  </>
                )}
                {refItem.recipientName && (
                  <div className="mt-1 pt-1 border-t border-blue-300">
                    <div className="font-semibold text-blue-900 mb-1">
                      Người nhận bên ngoài
                    </div>
                    <div>Tên: {refItem.recipientName}</div>
                    {refItem.recipientPhone && (
                      <div>SĐT: {refItem.recipientPhone}</div>
                    )}
                  </div>
                )}
                {refItem.content && (
                  <div className="line-clamp-1 mt-1 text-gray-600">
                    Ghi chú: {refItem.content}
                  </div>
                )}
              </div>
            )}
          </div>

          <div className="flex gap-2">
            <Box
              className="divCenter"
              style={{
                flex: 1,
                padding: "8px 0px",
                fontSize: 13,
                fontWeight: "600",
                border: "1px solid #000",
                borderRadius: "6px",
                textAlign: "center",
                cursor: "pointer",
                transition: "all 0.2s",
              }}
              onClick={() => onViewDetail(refItem)}
            >
              Xem chi tiết
            </Box>

            {filterType === "received" &&
              refItem.status === 1 &&
              refItem.type === 0 && (
                <Box
                  className="divCenter"
                  style={{
                    flex: 1,
                    padding: "8px 0px",
                    fontSize: 13,
                    fontWeight: "600",
                    border: "1px solid #000",
                    borderRadius: "6px",
                    background: "white",
                    color: "#000",
                    textAlign: "center",
                    cursor: "pointer",
                    transition: "all 0.2s",
                  }}
                  onClick={(e) => {
                    e.stopPropagation();
                    onCompleteClick(refItem);
                  }}
                >
                  Hoàn thành
                </Box>
              )}
          </div>
        </div>
      );
    }
  );

  return (
    <Page style={{ marginTop: "50px", background: "#f5f5f5" }}>
      <Category
        list={filterCategories}
        value={selectedFilter}
        onChange={handleFilterChange}
        onChangeValueChild={() => {}}
        backgroundColor="#fff"
      />

      <div className="p-4 space-y-3">
        {loading ? (
          <div className="flex justify-center items-center py-16">
            <LoadingGiba size="lg" text="Đang tải danh sách ref..." />
          </div>
        ) : refs.length > 0 ? (
          <div className="space-y-3">
            {refs.map((ref) => (
              <RefCard
                key={ref.id}
                refItem={ref}
                filterType={selectedFilter}
                onViewDetail={handleViewDetail}
                onCompleteClick={handleCompleteClick}
                getStatusColor={getStatusColor}
                getStatusGradient={getStatusGradient}
                formatDate={formatDate}
              />
            ))}
          </div>
        ) : (
          <div className="text-center py-16">
            <div className="w-20 h-20 bg-gradient-to-br from-gray-100 to-gray-200 rounded-full flex items-center justify-center mx-auto mb-4">
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
                  d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
                />
              </svg>
            </div>
            <div className="text-gray-800 text-lg font-bold mb-2">
              Chưa có đơn ref nào
            </div>
            <div className="text-gray-500 text-sm">
              {selectedFilter === "sent"
                ? "Bạn chưa gửi đơn ref nào"
                : "Bạn chưa nhận đơn ref nào"}
            </div>
          </div>
        )}
      </div>

      {/* Floating Plus Button */}
      <FloatingActionButtonGiba
        icon={<Plus />}
        onClick={handleCreateRef}
        position="bottom-right"
        color="yellow"
        size="md"
        tooltip="Tạo referral mới"
      />

      {/* Complete Drawer */}
      <Drawer
        title="Hoàn thành đơn referral"
        placement="bottom"
        style={{ borderRadius: "15px 15px 0 0" }}
        onClose={() => setShowCompleteDrawer(false)}
        open={showCompleteDrawer}
        height="auto"
      >
        <div className="space-y-4">
          <div>
            <label className="block text-sm font-semibold mb-2 text-black flex items-center gap-2">
              <User size={16} />
              Người gửi
            </label>
            <Input
              value={completingRef?.fromMemberName}
              disabled
              style={{ height: 43, background: "#f5f5f5" }}
            />
          </div>

          <div>
            <label className="block text-sm font-semibold mb-2 text-black flex items-center gap-2">
              <MessageCircle size={16} />
              Nội dung referral
            </label>
            <Input.TextArea
              value={completingRef?.content}
              disabled
              rows={3}
              style={{ background: "#f5f5f5" }}
            />
          </div>

          <div>
            <label className="block text-sm font-semibold mb-2 text-black flex items-center gap-2">
              <DollarSign size={16} />
              Giá trị referral (VNĐ) <span className="text-red-500">*</span>
            </label>
            <Input
              placeholder="Nhập giá trị referral (VD: 10.000.000)"
              value={refValue}
              onChange={(e) => {
                const formatted = formatCurrency(e.target.value);
                setRefValue(formatted);
              }}
              style={{ height: 43 }}
              prefix={<DollarSign size={16} style={{ color: "#666" }} />}
              suffix="VNĐ"
            />
            <div className="text-xs text-gray-500 mt-1">
              Tối thiểu: 1.000 VNĐ
            </div>
          </div>

          {/* Rating Section */}
          <div>
            <label className="block text-sm font-semibold mb-2 text-black flex items-center gap-2">
              <Star size={16} />
              Đánh giá <span className="text-red-500">*</span>
            </label>
            <div className="flex items-center justify-center gap-3">
              {[1, 2, 3, 4, 5].map((star) => (
                <button
                  key={star}
                  type="button"
                  onClick={() => setRating(star)}
                  className="transition-all duration-200 hover:scale-110 transform flex items-center justify-center"
                  disabled={completing}
                  style={{
                    strokeWidth: 3,
                    filter:
                      star <= rating
                        ? "drop-shadow(0 0 4px rgba(251, 191, 36, 0.5))"
                        : "none",
                  }}
                >
                  {star <= rating ? (
                    <StarIcon
                      size={32}
                      className="text-yellow-400"
                      style={{ strokeWidth: 3 }}
                    />
                  ) : (
                    <Star
                      size={32}
                      className="text-gray-300 hover:text-yellow-300"
                      style={{ strokeWidth: 3 }}
                    />
                  )}
                </button>
              ))}
              <span className="text-sm text-gray-600 ml-3 font-medium">
                {rating > 0 && `${rating}/5 sao`}
              </span>
            </div>
          </div>

          {/* Feedback Section */}
          <div>
            <label className="block text-sm font-semibold mb-2 text-black flex items-center gap-2">
              <MessageCircle size={16} />
              Lời cảm ơn <span className="text-red-500">*</span>
            </label>
            <Input.TextArea
              placeholder="Nhập lời cảm ơn của bạn về việc giới thiệu..."
              value={feedback}
              onChange={(e) => setFeedback(e.target.value)}
              rows={3}
              style={{ resize: "vertical" }}
              maxLength={500}
            />
            <div className="text-xs text-gray-500 mt-1">
              {feedback.length}/500 ký tự
            </div>
          </div>

          <div className="flex gap-2">
            <button
              onClick={() => setShowCompleteDrawer(false)}
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
              disabled={completing}
            >
              Đóng
            </button>
            <button
              onClick={handleCompleteRef}
              style={{
                flex: 1,
                padding: "10px",
                background: "#000",
                border: "none",
                borderRadius: "6px",
                color: "#fff",
                fontSize: "14px",
                fontWeight: "600",
              }}
              disabled={completing}
            >
              {completing ? "Đang xử lý..." : "Xác nhận"}
            </button>
          </div>
        </div>
      </Drawer>

      {/* Ref Option Modal */}
      <RefOptionModal
        visible={showRefOptionModal}
        onClose={() => setShowRefOptionModal(false)}
        onSelectOption={handleSelectRefOption}
      />
    </Page>
  );
};

export default RefListGiba;
