import React from "react";

import { Calendar, MapPin, User, Edit, Trash2 } from "lucide-react";
import { formatDateTime } from "../../utils/dateFormatter";

interface ShowcaseItemProps {
  showcase: any;
  onViewDetail: (showcase: any) => void;
  onEdit?: (showcase: any) => void;
  onDelete?: (showcase: any) => void;
  isAdminMode?: boolean;
}

const ShowcaseItem: React.FC<ShowcaseItemProps> = ({
  showcase,
  onViewDetail,
  onEdit,
  onDelete,
  isAdminMode = false,
}) => {
  const getStatusBadge = (status: number) => {
    switch (status) {
      case 1:
        return {
          label: "Đã lên lịch",
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

  // Badge Công khai/Nội bộ dựa trên isPublic
  const getVisibilityBadge = () => {
    if (showcase.isPublic) {
      return {
        label: "Công khai",
        bgColor: "#E8F5E9",
        textColor: "#2E7D32",
        borderColor: "#81C784",
      };
    } else {
      return {
        label: "Nội bộ",
        bgColor: "#FFF3E0",
        textColor: "#E65100",
        borderColor: "#FFB74D",
      };
    }
  };

  const startDateTime = formatDateTime(showcase.startDate);
  const endDateTime = formatDateTime(showcase.endDate);
  const statusBadge = getStatusBadge(showcase.status);
  const visibilityBadge = getVisibilityBadge();

  return (
    <div
      className="bg-white rounded-lg shadow-sm border border-gray-200 hover:shadow-md transition-all duration-200 cursor-pointer"
      style={{ padding: "16px" }}
      onClick={() => onViewDetail(showcase)}
    >
      {/* Badge row */}
      <div className="flex items-center justify-between flex-wrap gap-2 mb-3">
        {statusBadge.label && (
          <div
            className={`px-3 py-1 rounded-full text-xs font-semibold border ${statusBadge.bgColor} ${statusBadge.textColor} ${statusBadge.borderColor}`}
          >
            {statusBadge.label}
          </div>
        )}
        <div className="flex items-center gap-2 ml-auto">
          <div
            style={{
              backgroundColor: "#F3E5F5",
              color: "#7B1FA2",
              border: "1px solid #CE93D8",
              padding: "4px 12px",
              borderRadius: "12px",
              fontSize: "11px",
              fontWeight: "600",
            }}
          >
            Showcase
          </div>
          <div
            style={{
              backgroundColor: visibilityBadge.bgColor,
              color: visibilityBadge.textColor,
              border: `1px solid ${visibilityBadge.borderColor}`,
              padding: "4px 12px",
              borderRadius: "12px",
              fontSize: "11px",
              fontWeight: "600",
            }}
          >
            {visibilityBadge.label}
          </div>
        </div>
      </div>

      <div className="flex items-center justify-center mb-4">
        <div className="px-4 py-2 bg-green-600 text-white rounded-lg text-sm font-semibold">
          {showcase.groupName}
        </div>
      </div>

      <h3 className="text-base font-bold text-gray-900 mb-4 text-center">
        {showcase.title}
      </h3>

      <div className="space-y-2 mb-3">
        <div className="flex items-start gap-2">
          <User size={16} className="text-purple-600 mt-0.5 flex-shrink-0" />
          <div className="flex-1 min-w-0">
            <div className="text-xs text-gray-500">Diễn giả</div>
            <div className="text-sm font-semibold text-gray-900">
              {showcase.membershipName}
            </div>
          </div>
        </div>

        <div className="flex items-start gap-2">
          <Calendar size={16} className="text-blue-600 mt-0.5 flex-shrink-0" />
          <div className="flex-1 min-w-0">
            <div className="text-xs text-gray-500">Thời gian</div>
            <div className="text-sm text-gray-700">
              {startDateTime.date} • {startDateTime.time} - {endDateTime.time}
            </div>
          </div>
        </div>

        <div className="flex items-start gap-2">
          <MapPin size={16} className="text-red-600 mt-0.5 flex-shrink-0" />
          <div className="flex-1 min-w-0">
            <div className="text-xs text-gray-500">Địa điểm</div>
            <div className="text-sm text-gray-700 line-clamp-2">
              {showcase.location}
            </div>
          </div>
        </div>
      </div>

      {isAdminMode && onEdit && onDelete && (
        <div className="flex gap-2 mt-3 justify-end">
          <button
            className="px-3 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors"
            onClick={(e) => {
              e.stopPropagation();
              onEdit(showcase);
            }}
            title="Chỉnh sửa"
          >
            <Edit size={18} />
          </button>
          <button
            className="px-3 py-2 bg-red-600 text-white rounded-md hover:bg-red-700 transition-colors"
            onClick={(e) => {
              e.stopPropagation();
              onDelete(showcase);
            }}
            title="Xóa"
          >
            <Trash2 size={18} />
          </button>
        </div>
      )}
    </div>
  );
};

export default ShowcaseItem;
