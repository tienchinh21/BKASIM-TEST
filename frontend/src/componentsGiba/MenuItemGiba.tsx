import React from "react";
import {
  ChevronRight,
  Settings,
  FileText,
  Clock,
  MapPin,
  Users,
  Group,
  BarChart3,
  User,
} from "lucide-react";

interface MenuItemGibaProps {
  id: string;
  title: string;
  subtitle?: string;
  icon: React.ReactNode;
  onClick?: () => void;
  showSubAction?: boolean;
  subActionText?: string;
  onSubActionClick?: () => void;
  className?: string;
}

const MenuItemGiba: React.FC<MenuItemGibaProps> = ({
  id,
  title,
  subtitle,
  icon,
  onClick,
  showSubAction = false,
  subActionText,
  onSubActionClick,
  className = "",
}) => {
  return (
    <div
      className={`
        group flex items-center justify-between py-3 px-4
        border-b border-gray-100 last:border-b-0
        transition-all duration-200
        ${onClick ? "cursor-pointer hover:bg-gray-50 hover:shadow-sm" : ""}
        ${className}
      `}
      onClick={onClick}
    >
      {/* Left side - Icon and content */}
      <div className="flex items-center gap-3 flex-1 min-w-0">
        {/* Left accent border */}
        <div className="w-1 h-6 bg-gray-300 rounded-full flex-shrink-0 group-hover:bg-black transition-colors duration-200"></div>

        {/* Icon */}
        <div className="w-5 h-5 flex items-center justify-center flex-shrink-0">
          {icon}
        </div>

        {/* Content */}
        <div className="flex-1 min-w-0">
          <h3 className="text-sm font-medium text-black">{title}</h3>
          {subtitle && (
            <p className="text-xs text-gray-500 mt-0.5">{subtitle}</p>
          )}
        </div>
      </div>

      {/* Right side - Sub action and arrow */}
      <div className="flex items-center gap-2 flex-shrink-0">
        {showSubAction && subActionText && (
          <button
            onClick={(e) => {
              e.stopPropagation();
              onSubActionClick?.();
            }}
            className="text-xs text-gray-500 hover:text-gray-700 transition-colors"
          >
            {subActionText}
          </button>
        )}
        <ChevronRight className="w-4 h-4 text-gray-400" />
      </div>
    </div>
  );
};

// Predefined menu items
export const menuItems = [
  {
    id: "dashboard",
    title: "Thống kê",
    subtitle: "Xem tổng quan hoạt động",
    icon: <BarChart3 className="w-5 h-5 text-gray-600" />,
  },
  {
    id: "edit-info",
    title: "Chỉnh sửa thông tin",
    icon: <Settings className="w-5 h-5 text-gray-600" />,
  },
  {
    id: "profile-intro",
    title: "Profile",
    subtitle: "Tùy chỉnh profile cá nhân",
    icon: <User className="w-5 h-5 text-gray-600" />,
  },
  {
    id: "1",
    title: "REFERRAL",
    icon: <FileText className="w-5 h-5 text-gray-600" />,
    showSubAction: true,
    subActionText: "Xem tất cả",
  },
  // {
  //   id: "2",
  //   title: "Khách mời",
  //   icon: <Users className="w-5 h-5 text-gray-600" />,
  //   showSubAction: true,
  //   subActionText: "Xem tất cả",
  // },
  {
    id: "3",
    title: "Đơn tham gia nhóm",
    icon: <Group className="w-5 h-5 text-gray-600" />,
    showSubAction: true,
    subActionText: "Xem tất cả",
  },
  {
    id: "event-registration-history",
    title: "Khách mời",
    icon: <Clock className="w-5 h-5 text-gray-600" />,
  },
];

export default MenuItemGiba;
