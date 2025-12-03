import React from "react";
import { Users, Calendar, MapPin } from "lucide-react";

interface GroupListCardGibaProps {
  id: string;
  name: string;
  description: string;
  image: string;
  memberCount: number;
  status: "pending" | "approved" | "active";
  category?: string;
  eventCount?: number;
  location?: string;
  onClick?: () => void;
  className?: string;
}

const GroupListCardGiba: React.FC<GroupListCardGibaProps> = ({
  id,
  name,
  description,
  image,
  memberCount,
  status,
  category,
  eventCount = 0,
  location,
  onClick,
  className = "",
}) => {
  console.log("image", image);
  const getStatusColor = (status: string) => {
    switch (status) {
      case "active":
        return "bg-emerald-100 text-emerald-700 border-emerald-200";
      case "approved":
        return "bg-blue-100 text-blue-700 border-blue-200";
      case "pending":
        return "bg-amber-100 text-amber-700 border-amber-200";
      default:
        return "bg-gray-100 text-gray-700 border-gray-200";
    }
  };

  const getStatusText = (status: string) => {
    switch (status) {
      case "active":
        return "Hoạt động";
      case "approved":
        return "Đã duyệt";
      case "pending":
        return "Chờ duyệt";
      default:
        return "Không xác định";
    }
  };

  return (
    <div
      className={`
        bg-white rounded-xl border border-gray-200 p-4
        transition-all duration-200
        ${onClick ? "cursor-pointer hover:shadow-lg hover:border-gray-300" : ""}
        ${className}
      `}
      onClick={onClick}
    >
      {/* Header with image and basic info */}
      <div className="flex gap-4 mb-3 items-center">
        {/* Group Image */}
        <div className="relative w-20 h-20 flex-shrink-0">
          <img
            src={image}
            alt={name}
            className="w-full h-full object-cover rounded-lg border border-gray-200"
          />
        </div>

        {/* Group Info */}
        <div className="flex-1 min-w-0">
          <h3 className="text-lg font-semibold text-black mb-1 line-clamp-1">
            {name}
          </h3>
          <p className="text-sm text-gray-600 line-clamp-2 mb-2">
            {description}
          </p>

          {/* Category */}
          {category && (
            <div className="inline-block bg-gray-100 text-gray-700 px-2 py-1 rounded-md text-xs font-medium mb-2">
              {category}
            </div>
          )}
        </div>
      </div>

      {/* Stats and Actions */}
      <div className="flex items-center justify-between pt-3 border-t border-gray-100">
        {/* Stats */}
        <div className="flex items-center gap-4">
          <div className="flex items-center gap-1">
            <Users className="w-4 h-4 text-gray-500" />
            <span className="text-sm text-gray-600">
              {memberCount.toLocaleString()}
            </span>
          </div>

          {eventCount > 0 && (
            <div className="flex items-center gap-1">
              <Calendar className="w-4 h-4 text-gray-500" />
              <span className="text-sm text-gray-600">{eventCount}</span>
            </div>
          )}

          {location && (
            <div className="flex items-center gap-1">
              <MapPin className="w-4 h-4 text-gray-500" />
              <span className="text-sm text-gray-600 truncate max-w-20">
                {location}
              </span>
            </div>
          )}
        </div>

        {/* Join Button */}
        <button
          className="bg-black text-white px-4 py-2 rounded-lg text-sm font-medium hover:bg-gray-800 transition-colors"
          onClick={(e) => {
            e.stopPropagation();
            onClick?.();
          }}
        >
          Tham gia
        </button>
      </div>
    </div>
  );
};

export default GroupListCardGiba;
