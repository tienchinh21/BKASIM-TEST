import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { useRecoilValue } from "recoil";
import { userMembershipInfo, token } from "../recoil/RecoilState";
import axios from "axios";
import dfData from "../common/DefaultConfig.json";
import EventItem from "../pagesGiba/event/EventItem";
import EventInvitationSkeleton from "./skeletons/EventInvitationSkeleton";

// Form status enum for guest invitation
// 0: Chưa xác nhận (được mời nhưng chưa xác nhận) -> Button "Xác nhận tham gia"
// 5: Đã xác nhận, đợi admin duyệt -> Button "Đang đợi duyệt"  
// 1: Admin đã duyệt, có checkInCode -> Button "QR Check-in"
// 2: Bị từ chối -> Button "Bị từ chối"
export enum FormStatus {
  Invited = 0,              // Được mời nhưng chưa xác nhận
  Approved = 1,             // Admin đã duyệt -> có checkInCode
  Rejected = 2,             // Bị từ chối
  Cancelled = 3,            // Hủy
  WaitingApproval = 5,      // Đã xác nhận, đợi admin duyệt
}

interface EventInvitation {
  id: string;
  groupId: string;
  title: string;
  content: string;
  banner: string;
  images: string;
  startTime: string;
  endTime: string;
  joinCount: number;
  type: number;
  meetingLink: string;
  googleMapURL: string | null;
  address: string;
  status: number;
  isActive: boolean;
  needApproval: boolean;
  createdDate: string;
  updatedDate: string;
  // Guest registration info
  avatar?: string;
  userZaloId?: string;
  guestName?: string;
  guestListId?: string;
  formStatus?: number; // Trạng thái đăng ký của khách mời
  checkInCode?: string;
  checkInStatus?: number | null;
}

interface EventInvitationsSectionProps {
  onInvitationConfirmed?: () => void;
  refreshTrigger?: number;
}

// Helper function to get button text based on formStatus
const getButtonText = (formStatus?: number) => {
  switch (formStatus) {
    case FormStatus.Invited:
      return "Xác nhận tham gia";
    case FormStatus.WaitingApproval:
      return "Đang đợi duyệt";
    case FormStatus.Approved:
      return "QR Check-in";
    case FormStatus.Rejected:
      return "Bị từ chối";
    case FormStatus.Cancelled:
      return "Đã hủy";
    default:
      return "Xác nhận tham gia";
  }
};

const EventInvitationCard: React.FC<{
  item: EventInvitation;
  onConfirmSuccess?: () => void;
}> = ({ item, onConfirmSuccess }) => {
  const buttonText = getButtonText(item.formStatus);

  return (
    <div
      style={{
        minWidth: "280px",
        width: "280px",
        flexShrink: 0,
      }}
    >
      {/* Invitation Header - chỉ hiển thị tên người mời */}
      {item.guestName && (
        <div className="flex items-center gap-2 mb-2 px-1">
          {item.avatar ? (
            <img
              src={item.avatar}
              alt={item.guestName}
              className="w-6 h-6 rounded-full object-cover border border-gray-100"
            />
          ) : (
            <div className="w-6 h-6 rounded-full bg-orange-100 flex items-center justify-center text-[10px] text-orange-600 font-bold border border-orange-200">
              {item.guestName.charAt(0).toUpperCase()}
            </div>
          )}
          <p className="text-xs text-gray-500 truncate">
            <span className="font-bold text-gray-800">{item.guestName}</span>{" "}
            mời bạn tham gia
          </p>
        </div>
      )}

      <EventItem
        item={item}
        titleMaxLines={1}
        customRegisterText={buttonText}
        onInvitationConfirmSuccess={onConfirmSuccess}
        hideGuestListButton={true}
      />
    </div>
  );
};

const EventInvitationsSection: React.FC<EventInvitationsSectionProps> = ({
  onInvitationConfirmed,
  refreshTrigger,
}) => {
  const navigate = useNavigate();
  const membershipInfo = useRecoilValue(userMembershipInfo);
  const userToken = useRecoilValue(token);
  const [invitations, setInvitations] = useState<EventInvitation[]>([]);
  const [loading, setLoading] = useState(false);

  const fetchInvitations = async () => {
    if (!membershipInfo?.phoneNumber) {
      console.log("EventInvitationsSection: No phone number available");
      return;
    }

    
    try {
      setLoading(true);
      
      const headers: any = {};
      if (userToken) {
        headers.Authorization = `Bearer ${userToken}`;
      }
      
      const response = await axios.get(
        `${dfData.domain}/api/EventGuests/GetEventByPhone/${membershipInfo.phoneNumber}`,
        { headers }
      );


      if (Array.isArray(response.data)) {
        setInvitations(response.data);
      } else if (response.data.success || response.data.code === 0) {
        setInvitations(response.data.data || []);
      }
    } catch (error) {
      console.error("Error fetching event invitations:", error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    console.log("EventInvitationsSection: useEffect triggered", {
      phoneNumber: membershipInfo?.phoneNumber,
      hasToken: !!userToken,
      refreshTrigger,
    });
    
    if (membershipInfo?.phoneNumber) {
      fetchInvitations();
    }
  }, [membershipInfo?.phoneNumber, userToken, refreshTrigger]);

  if (loading) {
    return (
      <div className="w-full">
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-lg font-bold text-black">Lời mời tham dự</h2>
        </div>
        <div className="flex gap-4 overflow-x-auto mt-4">
          <EventInvitationSkeleton />
          <EventInvitationSkeleton />
          <EventInvitationSkeleton />
        </div>
      </div>
    );
  }

  if (!membershipInfo?.phoneNumber || invitations.length === 0) {
    return null;
  }

  return (
    <div className="w-full">
      <div className="flex items-center justify-between mb-4">
        <h2 className="text-lg font-bold text-black">Lời mời tham dự</h2>
        <span className="text-sm text-gray-500">
          {invitations.length} sự kiện
        </span>
      </div>

      <div className="flex gap-4 overflow-x-auto mt-4">
        {invitations.map((event) => (
          <EventInvitationCard
            key={event.id}
            item={event}
            onConfirmSuccess={() => {
              fetchInvitations();
              onInvitationConfirmed?.();
            }}
          />
        ))}
      </div>
    </div>
  );
};

export default EventInvitationsSection;
