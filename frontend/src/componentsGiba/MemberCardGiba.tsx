import React from "react";
import { ChevronRight, User } from "lucide-react";

interface MemberCardGibaProps {
  onClick?: () => void;
  className?: string;
}

const MemberCardGiba: React.FC<MemberCardGibaProps> = ({
  onClick,
  className = "",
}) => {
  return (
    <div
      className={`
        flex items-center gap-4
        transition-all duration-200
        ${onClick ? "cursor-pointer hover:bg-gray-200" : ""}
        ${className}
      `}
      onClick={onClick}
    >
      {/* Avatar */}
      <div className="w-12 h-12 bg-gray-300 rounded-full flex items-center justify-center flex-shrink-0">
        <User className="w-6 h-6 text-gray-400" />
      </div>

      {/* Content */}
      <div className="flex-1 min-w-0">
        <h3 className="text-base font-semibold text-black mb-1">
          Đăng ký thành viên
        </h3>
        <p className="text-xs text-gray-500 leading-relaxed">
          Tích điểm đổi thưởng, mở rộng tiện ích
        </p>
      </div>

      {/* Arrow */}
      <div className="flex-shrink-0">
        <ChevronRight className="w-5 h-5 text-gray-400" />
      </div>
    </div>
  );
};

export default MemberCardGiba;
