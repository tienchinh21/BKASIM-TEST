import React from "react";
import { User, Mail, Phone, Building, Briefcase, Calendar } from "lucide-react";
import { formatDate } from "../../../utils/dateFormatter";

export interface MemberData {
  id: string;
  fullname: string;
  email: string;
  phone: string;
  company: string;
  position: string;
  approvalStatus: number | null;
  createdDate: string;
  zaloAvatar?: string;
  userZaloId?: string;
  fieldNames?: string[];
  averageRating?: number;
  totalRatings?: number;
  formattedData?: any;
}

interface MemberCardProps {
  member: MemberData;
  onViewDetail: (member: MemberData) => void;
  onApprove?: (memberId: string) => void;
  onReject?: (memberId: string) => void;
}

const MemberCard: React.FC<MemberCardProps> = ({
  member,
  onViewDetail,
  onApprove,
  onReject,
}) => {
  const getStatusBadge = (status: number) => {
    switch (status) {
      case 0:
        return (
          <span className="px-3 py-1 text-xs font-semibold rounded-full bg-yellow-100 text-yellow-800">
            Chờ duyệt
          </span>
        );
      case 1:
        return (
          <span className="px-3 py-1 text-xs font-semibold rounded-full bg-green-100 text-green-800">
            Đã duyệt
          </span>
        );
      case 2:
        return (
          <span className="px-3 py-1 text-xs font-semibold rounded-full bg-red-100 text-red-800">
            Từ chối
          </span>
        );
      default:
        return (
          <span className="px-3 py-1 text-xs font-semibold rounded-full bg-gray-100 text-gray-800">
            Không xác định
          </span>
        );
    }
  };

  // Using utility function from dateFormatter

  return (
    <div className="bg-white rounded-xl border border-gray-200 overflow-hidden hover:shadow-lg transition-shadow duration-200">
      {/* Header with status */}
      <div className="bg-gradient-to-r from-gray-50 to-gray-100 px-4 py-3 flex justify-between items-center border-b border-gray-200">
        <div className="flex items-center gap-2">
          <div className="w-10 h-10 bg-gradient-to-br from-black to-gray-700 rounded-full flex items-center justify-center">
            <User className="w-5 h-5 text-white" />
          </div>
          <div>
            <h3 className="font-bold text-gray-900 text-base">
              {member.fullname}
            </h3>
            <div className="flex items-center gap-1 text-xs text-gray-500">
              <Calendar className="w-3 h-3" />
              <span>{formatDate(member.createdDate)}</span>
            </div>
          </div>
        </div>
        {getStatusBadge(member.approvalStatus || 0)}
      </div>

      {/* Content */}
      <div className="p-4 space-y-3">
        {/* Email */}
        {member.email && (
          <div className="flex items-start gap-3">
            <Mail className="w-4 h-4 text-gray-400 mt-0.5 flex-shrink-0" />
            <span className="text-sm text-gray-700 break-all">
              {member.email}
            </span>
          </div>
        )}

        {/* Phone */}
        {member.phone && (
          <div className="flex items-center gap-3">
            <Phone className="w-4 h-4 text-gray-400 flex-shrink-0" />
            <span className="text-sm text-gray-700">{member.phone}</span>
          </div>
        )}

        {/* Company */}
        {member.company && (
          <div className="flex items-start gap-3">
            <Building className="w-4 h-4 text-gray-400 mt-0.5 flex-shrink-0" />
            <span className="text-sm text-gray-700">{member.company}</span>
          </div>
        )}

        {/* Position */}
        {member.position && (
          <div className="flex items-start gap-3">
            <Briefcase className="w-4 h-4 text-gray-400 mt-0.5 flex-shrink-0" />
            <span className="text-sm text-gray-700">{member.position}</span>
          </div>
        )}
      </div>

      {/* Actions */}
      <div className="px-4 py-3 bg-gray-50 border-t border-gray-200 flex gap-2">
        <button
          onClick={() => onViewDetail(member)}
          className="flex-1 px-4 py-2 bg-black text-white text-sm font-medium rounded-lg hover:bg-gray-800 transition-colors"
        >
          Xem chi tiết
        </button>

        {member.approvalStatus === 0 && onApprove && onReject && (
          <>
            <button
              onClick={() => onApprove(member.id)}
              className="flex-1 px-4 py-2 bg-green-600 text-white text-sm font-medium rounded-lg hover:bg-green-700 transition-colors"
            >
              Phê duyệt
            </button>
            <button
              onClick={() => onReject(member.id)}
              className="flex-1 px-4 py-2 bg-red-600 text-white text-sm font-medium rounded-lg hover:bg-red-700 transition-colors"
            >
              Từ chối
            </button>
          </>
        )}
      </div>
    </div>
  );
};

export default MemberCard;
