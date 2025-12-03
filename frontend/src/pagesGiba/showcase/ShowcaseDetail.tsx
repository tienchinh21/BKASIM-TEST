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
  User,
  Edit,
  Trash2,
} from "lucide-react";
import axios from "axios";
import dfData from "../../common/DefaultConfig.json";
import { toast } from "react-toastify";
import { useHasRole } from "../../hooks/useHasRole";
import {
  formatDateTimeLocale,
  formatFullDateTime,
} from "../../utils/dateFormatter";

interface ShowcaseDetail {
  id: string;
  groupId: string;
  groupName: string;
  title: string;
  description: string;
  membershipId: string;
  membershipName: string;
  memberShipAvatar: string;
  startDate: string;
  endDate: string;
  location: string;
  status: number;
  roleId: string;
  createdBy: string;
  createdDate: string;
  updatedDate: string;
}

const ShowcaseDetail: React.FC = () => {
  const location = useLocation();
  const navigate = useNavigate();
  const setHeader = useSetHeader();
  const userToken = useRecoilValue(token);
  const hasRole = useHasRole();

  const showcaseFromState = location.state?.showcase;
  const showcaseId = showcaseFromState?.id;

  const [showcase, setShowcase] = useState<ShowcaseDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [deleting, setDeleting] = useState(false);
  const [showDeleteModal, setShowDeleteModal] = useState(false);

  useEffect(() => {
    setHeader({
      title: "CHI TIẾT SHOWCASE",
      showUserInfo: false,
      showMenuButton: false,
      showCloseButton: false,
      hasLeftIcon: true,
    });
  }, [setHeader]);

  useEffect(() => {
    const fetchShowcaseDetail = async () => {
      if (!showcaseId || !userToken) {
        setLoading(false);
        return;
      }

      try {
        setLoading(true);
        const response = await axios.get(
          `${dfData.domain}/api/Showcase/Detail/${showcaseId}`,
          {
            headers: {
              Authorization: `Bearer ${userToken}`,
            },
          }
        );

        if (response.data.code === 0 && response.data.item) {
          setShowcase(response.data.item);
        } else {
          toast.error("Không thể tải chi tiết showcase");
          navigate(-1);
        }
      } catch (error) {
        console.error("Error fetching showcase detail:", error);
        toast.error("Có lỗi xảy ra khi tải chi tiết showcase");
        navigate(-1);
      } finally {
        setLoading(false);
      }
    };

    fetchShowcaseDetail();
  }, [showcaseId, userToken, navigate]);

  const getStatusBadge = (status: number) => {
    switch (status) {
      case 1:
        return {
          label: "Sắp diễn ra",
          bgColor: "bg-amber-100",
          textColor: "text-amber-700",
          borderColor: "border-amber-300",
        };
      case 2:
        return {
          label: "Đang diễn ra",
          bgColor: "bg-green-100",
          textColor: "text-green-700",
          borderColor: "border-green-300",
        };
      case 3:
        return {
          label: "Đã hoàn thành",
          bgColor: "bg-gray-100",
          textColor: "text-gray-700",
          borderColor: "border-gray-300",
        };
      case 4:
        return {
          label: "Đã hủy",
          bgColor: "bg-red-100",
          textColor: "text-red-700",
          borderColor: "border-red-300",
        };
      default:
        return {
          label: "",
          bgColor: "bg-gray-100",
          textColor: "text-gray-700",
          borderColor: "border-gray-300",
        };
    }
  };

  const statusBadge = showcase ? getStatusBadge(showcase.status) : null;

  const handleDelete = async () => {
    if (!showcaseId || !userToken) return;

    try {
      setDeleting(true);
      const response = await axios.delete(
        `${dfData.domain}/api/Showcase/${showcaseId}`,
        {
          headers: {
            Authorization: `Bearer ${userToken}`,
          },
        }
      );

      if (response.data.code === 0) {
        toast.success("Đã hủy lịch showcase thành công!");
        navigate("/giba/showcase-list");
      } else {
        toast.error(response.data.message || "Không thể hủy lịch showcase");
      }
    } catch (error: any) {
      console.error("Error deleting showcase:", error);
      toast.error(
        error.response?.data?.message || "Có lỗi xảy ra khi hủy lịch"
      );
    } finally {
      setDeleting(false);
      setShowDeleteModal(false);
    }
  };

  const handleEdit = () => {
    navigate("/giba/showcase-create", {
      state: { showcase },
    });
  };

  // Using utility functions from dateFormatter

  if (loading) {
    return (
      <Page className="bg-white min-h-screen mt-[50px]">
        <div className="flex justify-center items-center h-64">
          <LoadingGiba size="lg" text="Đang tải chi tiết showcase..." />
        </div>
      </Page>
    );
  }

  if (!showcase) {
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
              Không tìm thấy thông tin showcase
            </div>
          </div>
        </div>
      </Page>
    );
  }

  const startDateTime = formatFullDateTime(showcase.startDate);
  const endDateTime = formatFullDateTime(showcase.endDate);
  const shouldShowButtons = hasRole && showcase.status === 1;

  return (
    <Page style={{ marginTop: "50px", background: "#f8fafc" }}>
      <div className={shouldShowButtons ? "pb-24" : ""}>
        {/* Header */}
        <div className="bg-white rounded-lg shadow-sm p-4">
          <div className="flex items-center justify-between mb-4">
            <div>
              <p className="text-purple-600 text-sm font-semibold">
                {showcase.groupName}
              </p>
              <p className="text-gray-500 text-xs mt-1">
                ID: {showcase.id.slice(-8).toUpperCase()}
              </p>
            </div>
            {statusBadge && (
              <div className="text-right">
                <div
                  className={`inline-flex items-center px-3 py-1.5 rounded-full text-sm font-semibold border ${statusBadge.bgColor} ${statusBadge.textColor} ${statusBadge.borderColor}`}
                >
                  {statusBadge.label}
                  {showcase.status === 2 && (
                    <div className="w-2 h-2 bg-green-400 rounded-full animate-pulse ml-2"></div>
                  )}
                </div>
              </div>
            )}
          </div>

          <div>
            <div className="text-xs text-gray-500 mb-1 flex items-center gap-1">
              <Calendar size={14} />
              Ngày tạo
            </div>
            <div className="font-medium text-gray-900 text-sm">
              {formatDateTimeLocale(showcase.createdDate)}
            </div>
          </div>
        </div>

        {/* Topic */}
        <div className="bg-white rounded-lg shadow-sm p-4">
          <h2 className="text-lg font-bold text-gray-900 mb-4 flex items-center gap-2">
            <FileText size={20} />
            Chủ đề
          </h2>

          <div className="bg-yellow-50 rounded-lg p-4 border-l-4 border-yellow-500">
            <div className="text-lg font-bold text-gray-900 mb-2">
              {showcase.title}
            </div>
            {showcase.description && (
              <div className="text-sm text-gray-700 leading-relaxed whitespace-pre-line">
                {showcase.description}
              </div>
            )}
          </div>
        </div>

        {/* Speaker Info */}
        <div className="bg-white rounded-lg shadow-sm p-4">
          <h2 className="text-lg font-bold text-gray-900 mb-4 flex items-center gap-2">
            <User size={20} />
            Diễn giả
          </h2>

          <div className="bg-purple-50 rounded-lg p-4 border-l-4 border-purple-500">
            <div className="flex items-start gap-3">
              <img
                src={showcase.memberShipAvatar}
                alt={showcase.membershipName}
                className="w-16 h-16 rounded-full border-2 border-purple-400"
              />
              <div className="flex-1">
                <div className="font-bold text-gray-900 text-base">
                  {showcase.membershipName}
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Time */}
        <div className="bg-white rounded-lg shadow-sm p-4">
          <h2 className="text-lg font-bold text-gray-900 mb-4 flex items-center gap-2">
            <Calendar size={20} />
            Thời gian
          </h2>

          <div className="space-y-3">
            <div className="bg-blue-50 rounded-lg p-4 border-l-4 border-blue-500">
              <div className="text-xs text-blue-600 mb-1 flex items-center gap-1">
                <Calendar size={14} />
                Bắt đầu
              </div>
              <div className="text-sm font-bold text-blue-900">
                {startDateTime.fullDate}
              </div>
              <div className="text-base font-bold text-blue-900 mt-1 flex items-center gap-1">
                <Clock size={16} />
                {startDateTime.time}
              </div>
            </div>

            <div className="bg-green-50 rounded-lg p-4 border-l-4 border-green-500">
              <div className="text-xs text-green-600 mb-1 flex items-center gap-1">
                <Calendar size={14} />
                Kết thúc
              </div>
              <div className="text-sm font-bold text-green-900">
                {endDateTime.fullDate}
              </div>
              <div className="text-base font-bold text-green-900 mt-1 flex items-center gap-1">
                <Clock size={16} />
                {endDateTime.time}
              </div>
            </div>

            {/* Duration Display */}
            <div className="bg-gray-50 rounded-lg p-3 text-center">
              <div className="text-xs text-gray-600 mb-1">Thời lượng</div>
              <div className="text-sm font-bold text-gray-900">
                {(() => {
                  const start = new Date(showcase.startDate);
                  const end = new Date(showcase.endDate);
                  const diffMs = end.getTime() - start.getTime();
                  const diffHours = Math.floor(diffMs / (1000 * 60 * 60));
                  const diffMinutes = Math.floor(
                    (diffMs % (1000 * 60 * 60)) / (1000 * 60)
                  );
                  return `${diffHours} giờ ${diffMinutes} phút`;
                })()}
              </div>
            </div>
          </div>
        </div>

        {/* Location */}
        <div className="bg-white rounded-lg shadow-sm p-4 mb-4">
          <h2 className="text-lg font-bold text-gray-900 mb-4 flex items-center gap-2">
            <MapPin size={20} className="text-red-500" />
            Địa điểm
          </h2>

          <div className="bg-red-50 rounded-lg p-4 border-l-4 border-red-500">
            <div className="text-gray-800 leading-relaxed text-sm font-medium">
              📍 {showcase.location}
            </div>
          </div>
        </div>
      </div>

      {shouldShowButtons && (
        <div className="fixed bottom-0 left-0 right-0 bg-white border-t border-gray-200 p-4 flex gap-3 shadow-lg z-50">
          <button
            onClick={() => setShowDeleteModal(true)}
            className="flex-1 px-4 py-2 bg-white border border-red-500 text-red-600 rounded-lg font-semibold text-base transition-all hover:bg-red-50 disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2"
            disabled={deleting}
          >
            <Trash2 size={20} />
            Hủy lịch
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
        title="Xác nhận hủy lịch showcase"
        open={showDeleteModal}
        onOk={handleDelete}
        onCancel={() => setShowDeleteModal(false)}
        okText="Xác nhận hủy"
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
          Bạn có chắc chắn muốn hủy lịch showcase này không?
        </p>
        <p className="text-red-600 text-sm mt-2 font-semibold">
          Hành động này không thể hoàn tác!
        </p>
      </Modal>
    </Page>
  );
};

export default ShowcaseDetail;
