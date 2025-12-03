import React from "react";

import { Calendar, MapPin, Video, Edit, Trash2 } from "lucide-react";
import { formatDateTime } from "../../utils/dateFormatter";

interface MeetingItemProps {
  meeting: any;
  onViewDetail: (meeting: any) => void;
  onEdit?: (meeting: any) => void;
  onDelete?: (meeting: any) => void;
  isAdminMode?: boolean;
}

const MeetingItem: React.FC<MeetingItemProps> = ({
  meeting,
  onViewDetail,
  onEdit,
  onDelete,
  isAdminMode = false,
}) => {
  const startDateTime = formatDateTime(meeting.startDate);
  const endDateTime = meeting.endDate ? formatDateTime(meeting.endDate) : null;
  const isSameDay = endDateTime && startDateTime.date === endDateTime.date;

  const getMeetingTypeBadge = () => {
    if (meeting.meetingType === 1) {
      return {
        label: "Online",
        bgColor: "bg-purple-100",
        textColor: "text-purple-700",
        borderColor: "border-purple-300",
      };
    } else {
      return {
        label: "Offline",
        bgColor: "bg-blue-100",
        textColor: "text-blue-700",
        borderColor: "border-blue-300",
      };
    }
  };

  // Badge Công khai/Nội bộ dựa trên isPublic
  const getVisibilityBadge = () => {
    if (meeting.isPublic) {
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

  const typeBadge = getMeetingTypeBadge();
  const visibilityBadge = getVisibilityBadge();

  return (
    <div
      className="bg-white rounded-lg shadow-sm border border-gray-200 hover:shadow-md transition-all duration-200 cursor-pointer"
      style={{ padding: "16px" }}
      onClick={() => onViewDetail(meeting)}
    >
      {/* Badge row */}
      <div className="flex items-center justify-between flex-wrap gap-2 mb-3">
        <div
          className={`px-3 py-1 rounded-full text-xs font-semibold border ${typeBadge.bgColor} ${typeBadge.textColor} ${typeBadge.borderColor}`}
        >
          {typeBadge.label}
        </div>
        <div className="flex items-center gap-2">
          <div
            style={{
              backgroundColor: "#E8F5E9",
              color: "#2E7D32",
              border: "1px solid #81C784",
              padding: "4px 12px",
              borderRadius: "12px",
              fontSize: "11px",
              fontWeight: "600",
            }}
          >
            Lịch họp
          </div>
          {meeting.type && (
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
          )}
        </div>
      </div>

      <div className="flex items-center justify-center mb-4">
        <div className="px-4 py-2 bg-green-600 text-white rounded-lg text-sm font-semibold">
          {meeting.groupName}
        </div>
      </div>

      <h3 className="text-base font-bold text-gray-900 mb-4 text-center">
        {meeting.title}
      </h3>

      <div className="space-y-2 mb-3">
        <div className="flex items-start gap-2">
          <Calendar size={16} className="text-blue-600 mt-0.5 flex-shrink-0" />
          <div className="flex-1 min-w-0">
            <div className="text-xs text-gray-500">Thời gian</div>
            <div className="text-sm text-gray-700">
              {startDateTime.date} • {startDateTime.time}
              {endDateTime && isSameDay && (
                <span className="text-gray-500"> - {endDateTime.time}</span>
              )}
              {endDateTime && !isSameDay && (
                <span className="text-gray-500">
                  <br />
                  {endDateTime.date} • {endDateTime.time}
                </span>
              )}
            </div>
          </div>
        </div>

        {meeting.meetingType === 1 ? (
          <div className="flex items-start gap-2">
            <Video size={16} className="text-purple-600 mt-0.5 flex-shrink-0" />
            <div className="flex-1 min-w-0">
              <div className="text-xs text-gray-500">Hình thức</div>
              <div className="text-sm text-gray-700">Online</div>
            </div>
          </div>
        ) : (
          <div className="flex items-start gap-2">
            <MapPin size={16} className="text-red-600 mt-0.5 flex-shrink-0" />
            <div className="flex-1 min-w-0">
              <div className="text-xs text-gray-500">Địa điểm</div>
              <div className="text-sm text-gray-700 line-clamp-2">
                {meeting.location}
              </div>
            </div>
          </div>
        )}
      </div>

      {isAdminMode && onEdit && onDelete && (
        <div className="flex gap-2 mt-3 justify-end">
          <button
            className="px-3 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors"
            onClick={(e) => {
              e.stopPropagation();
              onEdit(meeting);
            }}
            title="Chỉnh sửa"
          >
            <Edit size={18} />
          </button>
          <button
            className="px-3 py-2 bg-red-600 text-white rounded-md hover:bg-red-700 transition-colors"
            onClick={(e) => {
              e.stopPropagation();
              onDelete(meeting);
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

export default MeetingItem;
