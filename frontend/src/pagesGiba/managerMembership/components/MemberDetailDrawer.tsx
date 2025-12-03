import React, { useState, useEffect } from "react";
import { Drawer } from "antd";
import {
  User,
  Mail,
  Phone,
  Building,
  Briefcase,
  Calendar,
  CheckCircle,
  XCircle,
  Clock,
  UserMinus,
} from "lucide-react";
import { useRecoilValue } from "recoil";
import { token } from "../../../recoil/RecoilState";
import axios from "axios";
import dfData from "../../../common/DefaultConfig.json";
import LoadingGiba from "../../../componentsGiba/LoadingGiba";
import { toast } from "react-toastify";
import { MemberData } from "./MemberCard";

interface MemberDetailDrawerProps {
  visible: boolean;
  member: MemberData | null;
  onClose: () => void;
  onApprove?: (memberId: string) => void;
  onReject?: (memberId: string) => void;
  onRemoveFromGroup?: (membershipGroupId: string) => void;
  membershipGroupId?: string; // ID của membership trong nhóm (từ GetGroupMembers)
}

const MemberDetailDrawer: React.FC<MemberDetailDrawerProps> = ({
  visible,
  member,
  onClose,
  onApprove,
  onReject,
  onRemoveFromGroup,
  membershipGroupId,
}) => {
  const userToken = useRecoilValue(token);
  const [loading, setLoading] = useState(false);
  const [memberDetail, setMemberDetail] = useState<any>(null);

  useEffect(() => {
    const fetchMemberDetail = async () => {
      if (!visible || !member?.userZaloId || !userToken) {
        setMemberDetail(null);
        return;
      }

      try {
        setLoading(true);
        const response = await axios.get(
          `${dfData.domain}/api/memberships/${member.userZaloId}`,
          {
            headers: {
              Authorization: `Bearer ${userToken}`,
            },
          }
        );

        if (response.data.success && response.data.data) {
          setMemberDetail(response.data.data);
        } else {
          toast.error("Không thể tải chi tiết thành viên");
        }
      } catch (error: any) {
        console.error("Error fetching member detail:", error);
        toast.error(
          error.response?.data?.message ||
            "Có lỗi xảy ra khi tải chi tiết thành viên"
        );
      } finally {
        setLoading(false);
      }
    };

    fetchMemberDetail();
  }, [visible, member?.userZaloId, userToken]);

  if (!member) return null;

  // Use API data if available, otherwise use member prop
  const displayMember = memberDetail || member;

  const getStatusInfo = (status: number) => {
    switch (status) {
      case 0:
        return {
          icon: <Clock className="w-5 h-5" />,
          text: "Chờ duyệt",
          bgColor: "bg-yellow-100",
          textColor: "text-yellow-800",
          borderColor: "border-yellow-200",
        };
      case 1:
        return {
          icon: <CheckCircle className="w-5 h-5" />,
          text: "Đã duyệt",
          bgColor: "bg-green-100",
          textColor: "text-green-800",
          borderColor: "border-green-200",
        };
      case 2:
        return {
          icon: <XCircle className="w-5 h-5" />,
          text: "Từ chối",
          bgColor: "bg-red-100",
          textColor: "text-red-800",
          borderColor: "border-red-200",
        };
      default:
        return {
          icon: <Clock className="w-5 h-5" />,
          text: "Không xác định",
          bgColor: "bg-gray-100",
          textColor: "text-gray-800",
          borderColor: "border-gray-200",
        };
    }
  };

  const statusInfo = getStatusInfo(member.approvalStatus || 0);

  const formatDate = (dateString: string) => {
    try {
      const date = new Date(dateString);
      return date.toLocaleString("vi-VN", {
        day: "2-digit",
        month: "2-digit",
        year: "numeric",
        hour: "2-digit",
        minute: "2-digit",
      });
    } catch {
      return dateString;
    }
  };

  return (
    <Drawer
      title={
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 bg-gradient-to-br from-black to-gray-700 rounded-full flex items-center justify-center">
            <User className="w-5 h-5 text-white" />
          </div>
          <div>
            <h3 className="font-bold text-lg">Chi tiết thành viên</h3>
            <p className="text-xs text-gray-500">Thông tin chi tiết</p>
          </div>
        </div>
      }
      placement="right"
      onClose={onClose}
      open={visible}
      width={400}
      style={{ marginTop: "50px" }}
      bodyStyle={{
        padding: "24px",
        paddingBottom: onRemoveFromGroup && membershipGroupId ? "50px" : "24px",
        overflowY: "auto",
        maxHeight: "calc(100vh - 200px)",
      }}
      footer={
        (member.approvalStatus === 0 && onApprove && onReject) ||
        (onRemoveFromGroup && membershipGroupId) ? (
          <div className="flex flex-col gap-2">
            {/* Approve/Reject buttons for pending members */}
            {member.approvalStatus === 0 && onApprove && onReject && (
              <div className="flex gap-2">
                <button
                  onClick={() => {
                    onApprove(member.id);
                    onClose();
                  }}
                  className="flex-1 px-4 py-3 bg-green-600 text-white font-medium rounded-lg hover:bg-green-700 transition-colors flex items-center justify-center gap-2"
                >
                  <CheckCircle className="w-4 h-4" />
                  Phê duyệt
                </button>
                <button
                  onClick={() => {
                    onReject(member.id);
                    onClose();
                  }}
                  className="flex-1 px-4 py-3 bg-red-600 text-white font-medium rounded-lg hover:bg-red-700 transition-colors flex items-center justify-center gap-2"
                >
                  <XCircle className="w-4 h-4" />
                  Từ chối
                </button>
              </div>
            )}
            {/* Remove from group button */}
            {onRemoveFromGroup && membershipGroupId && (
              <button
                onClick={() => {
                  onRemoveFromGroup(membershipGroupId);
                  onClose();
                }}
                className="w-full px-4 py-3 bg-orange-600 text-white font-medium rounded-lg hover:bg-orange-700 transition-colors flex items-center justify-center gap-2"
              >
                <UserMinus className="w-4 h-4" />
                Xóa khỏi nhóm
              </button>
            )}
          </div>
        ) : null
      }
      footerStyle={{
        position: "fixed",
        bottom: "0px",
        left: 0,
        right: 0,
        width: "400px",
        background: "#fff",
        borderTop: "1px solid #e5e7eb",
        padding: "16px",
        zIndex: 1001,
        boxShadow: "0 -2px 8px rgba(0, 0, 0, 0.1)",
      }}
    >
      {loading ? (
        <div className="flex justify-center items-center py-16">
          <LoadingGiba size="lg" text="Đang tải chi tiết thành viên..." />
        </div>
      ) : (
        <div className="space-y-6">
          {/* Status Badge */}
          <div
            className={`${statusInfo.bgColor} ${statusInfo.textColor} ${statusInfo.borderColor} border-2 rounded-xl p-4 flex items-center gap-3`}
          >
            {statusInfo.icon}
            <div>
              <p className="text-xs font-medium opacity-75">Trạng thái</p>
              <p className="text-base font-bold">{statusInfo.text}</p>
            </div>
          </div>

          {/* Member Info */}
          <div className="space-y-4">
            <h4 className="font-bold text-gray-900 text-sm uppercase tracking-wide">
              Thông tin cá nhân
            </h4>

            {/* Full Name */}
            <div className="bg-gray-50 rounded-lg p-3 border border-gray-200">
              <div className="flex items-start gap-3">
                <User className="w-5 h-5 text-gray-400 mt-0.5 flex-shrink-0" />
                <div className="flex-1">
                  <p className="text-xs text-gray-500 mb-1">Họ và tên</p>
                  <p className="text-sm font-medium text-gray-900">
                    {displayMember.fullname ||
                      displayMember.memberName ||
                      displayMember.userZaloName ||
                      "-"}
                  </p>
                </div>
              </div>
            </div>

            {/* Email - Only show if it's a valid email format (not field IDs) */}
            {displayMember.email &&
              !displayMember.email.includes(",") &&
              displayMember.email.includes("@") && (
                <div className="bg-gray-50 rounded-lg p-3 border border-gray-200">
                  <div className="flex items-start gap-3">
                    <Mail className="w-5 h-5 text-gray-400 mt-0.5 flex-shrink-0" />
                    <div className="flex-1">
                      <p className="text-xs text-gray-500 mb-1">Email</p>
                      <p className="text-sm font-medium text-gray-900 break-all">
                        {displayMember.email}
                      </p>
                    </div>
                  </div>
                </div>
              )}

            {/* Phone */}
            {displayMember.phoneNumber && (
              <div className="bg-gray-50 rounded-lg p-3 border border-gray-200">
                <div className="flex items-start gap-3">
                  <Phone className="w-5 h-5 text-gray-400 mt-0.5 flex-shrink-0" />
                  <div className="flex-1">
                    <p className="text-xs text-gray-500 mb-1">Số điện thoại</p>
                    <p className="text-sm font-medium text-gray-900">
                      {displayMember.phoneNumber || displayMember.phone || "-"}
                    </p>
                  </div>
                </div>
              </div>
            )}

            {/* Company - Check multiple fields from API response */}
            {(displayMember.company ||
              displayMember.companyFullName ||
              displayMember.companyBrandName) && (
              <div className="bg-gray-50 rounded-lg p-3 border border-gray-200">
                <div className="flex items-start gap-3">
                  <Building className="w-5 h-5 text-gray-400 mt-0.5 flex-shrink-0" />
                  <div className="flex-1">
                    <p className="text-xs text-gray-500 mb-1">Công ty</p>
                    <p className="text-sm font-medium text-gray-900">
                      {displayMember.companyFullName ||
                        displayMember.companyBrandName ||
                        displayMember.company ||
                        "-"}
                    </p>
                  </div>
                </div>
              </div>
            )}

            {/* Position */}
            {(displayMember.position ||
              displayMember.appPosition ||
              displayMember.groupPosition) && (
              <div className="bg-gray-50 rounded-lg p-3 border border-gray-200">
                <div className="flex items-start gap-3">
                  <Briefcase className="w-5 h-5 text-gray-400 mt-0.5 flex-shrink-0" />
                  <div className="flex-1">
                    <p className="text-xs text-gray-500 mb-1">Chức vụ</p>
                    <p className="text-sm font-medium text-gray-900">
                      {displayMember.position ||
                        displayMember.appPosition ||
                        displayMember.groupPosition ||
                        "-"}
                    </p>
                  </div>
                </div>
              </div>
            )}

            {/* Code */}
            {displayMember.code && (
              <div className="bg-gray-50 rounded-lg p-3 border border-gray-200">
                <div className="flex items-start gap-3">
                  <User className="w-5 h-5 text-gray-400 mt-0.5 flex-shrink-0" />
                  <div className="flex-1">
                    <p className="text-xs text-gray-500 mb-1">Mã thành viên</p>
                    <p className="text-sm font-medium text-gray-900">
                      {displayMember.code}
                    </p>
                  </div>
                </div>
              </div>
            )}

            {/* Created Date */}
            {displayMember.createdDate && (
              <div className="bg-gray-50 rounded-lg p-3 border border-gray-200">
                <div className="flex items-start gap-3">
                  <Calendar className="w-5 h-5 text-gray-400 mt-0.5 flex-shrink-0" />
                  <div className="flex-1">
                    <p className="text-xs text-gray-500 mb-1">Ngày đăng ký</p>
                    <p className="text-sm font-medium text-gray-900">
                      {displayMember.createdDate}
                    </p>
                  </div>
                </div>
              </div>
            )}
          </div>
        </div>
      )}
    </Drawer>
  );
};

export default MemberDetailDrawer;
