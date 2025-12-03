import React, { useState, useEffect, useCallback } from "react";
import { useNavigate } from "zmp-ui";
import { useRecoilValue } from "recoil";
import { token } from "../recoil/RecoilState";
import axios from "axios";
import dfData from "../common/DefaultConfig.json";
import noImage from "../assets/no_image.png";

interface MyGroupCardProps {
  id: string;
  groupName: string;
  description: string;
  memberCount: number;
  isActive: boolean;
  isJoined: boolean;
  joinStatus?: string;
  joinStatusText?: string;
  logo?: string | null;
  onViewClick?: (
    groupId: string,
    groupName: string,
    groupLogo?: string | null
  ) => void;
}

const MyGroupCard: React.FC<MyGroupCardProps> = ({
  id,
  groupName,
  description,
  memberCount,
  isActive,
  isJoined,
  joinStatus,
  joinStatusText,
  logo,
  onViewClick,
}) => {
  return (
    <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden min-w-[240px] max-w-[280px] hover:shadow-md transition-shadow duration-200">
      {/* Logo */}
      <div className="w-full h-32 bg-gray-100 overflow-hidden">
        <img
          src={logo || noImage}
          alt={groupName}
          className="w-full h-full object-cover"
          onError={(e) => {
            (e.target as HTMLImageElement).src = noImage;
          }}
        />
      </div>

      {/* Header */}
      <div className="p-4 pb-3">
        <div className="mb-2">
          <span
            className={`inline-block px-2 py-1 rounded-md text-xs font-medium ${
              isJoined
                ? "bg-green-100 text-green-700"
                : joinStatus === "pending"
                ? "bg-yellow-100 text-yellow-700"
                : "bg-blue-100 text-blue-700"
            }`}
          >
            {isJoined
              ? "Đã tham gia"
              : joinStatus === "pending"
              ? joinStatusText || "Chờ phê duyệt"
              : "Có thể tham gia"}
          </span>
        </div>
        {/* Tên nhóm */}
        <h3 className="text-base font-semibold text-gray-900 line-clamp-1">
          {groupName}
        </h3>
      </div>

      {/* Stats and Actions */}
      <div className="px-4 pb-4">
        {/* Stats */}
        {/* <div className="flex items-center justify-between mb-3">
          <span className="text-xs text-gray-500">
            {memberCount.toLocaleString()} thành viên
          </span>
        </div> */}

        {/* Actions */}
        <div className="flex gap-2">
          <button
            onClick={() => onViewClick?.(id, groupName, logo)}
            className="flex-1 bg-white text-gray-900 py-2.5 px-4 rounded-lg text-sm font-medium border border-gray-300 hover:bg-gray-50 transition-colors"
          >
            Xem chi tiết
          </button>
        </div>
      </div>
    </div>
  );
};

interface MyGroupsSwiperProps {
  className?: string;
  refreshTrigger?: number; // Trigger để reload data
}

const MyGroupsSwiper: React.FC<MyGroupsSwiperProps> = ({
  className = "",
  refreshTrigger,
}) => {
  const navigate = useNavigate();
  const userToken = useRecoilValue(token);
  const [myGroups, setMyGroups] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);

  // Fetch my groups - chỉ lấy nhóm đã được approve
  const fetchMyGroups = useCallback(async () => {
    if (!userToken) {
      console.log("No token available, skipping API call");
      setLoading(false);
      setMyGroups([]);
      return;
    }

    try {
      setLoading(true);
      const response = await axios.get(
        `${dfData.domain}/api/groups/my-groups`,
        {
          headers: {
            Authorization: `Bearer ${userToken}`,
          },
        }
      );

      if (response.data.data) {
        const myGroups = response.data.data.filter(
          (group: any) => group.isJoined || group.joinStatus === "pending"
        );
        setMyGroups(myGroups);
      } else {
        setMyGroups([]);
      }
    } catch (error) {
      console.error("Error fetching my groups:", error);
      setMyGroups([]);
    } finally {
      setLoading(false);
    }
  }, [userToken]);

  useEffect(() => {
    fetchMyGroups();
  }, [fetchMyGroups]);

  // useEffect theo dõi refreshTrigger để reload data
  useEffect(() => {
    if (refreshTrigger && refreshTrigger > 0) {
      fetchMyGroups();
    }
  }, [refreshTrigger, fetchMyGroups]);

  const handleViewClick = (
    groupId: string,
    groupName: string,
    groupLogo?: string | null
  ) => {
    navigate(`/giba/group-detail/${groupId}`, {
      state: { groupName, groupLogo },
    });
  };

  if (loading) {
    return (
      <div className={`w-full ${className}`}>
        <div className="flex justify-center py-8">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-black"></div>
        </div>
      </div>
    );
  }

  if (myGroups.length === 0) {
    return (
      <div className={`w-full ${className}`}>
        <div className="text-center text-gray-500 py-8">
          {!userToken ? (
            <p className="text-sm">Vui lòng đăng nhập để xem nhóm của bạn</p>
          ) : (
            <p className="text-sm">Bạn chưa có nhóm nào</p>
          )}
        </div>
      </div>
    );
  }

  return (
    <div className={`w-full ${className}`}>
      <div className="flex gap-4 overflow-x-auto pb-4">
        {myGroups.map((group) => (
          <MyGroupCard
            key={group.id}
            id={group.id}
            groupName={group.groupName}
            description={group.description || "Nhóm đã tham gia"}
            memberCount={group.memberCount || 0}
            isActive={group.isActive}
            isJoined={group.isJoined}
            joinStatus={group.joinStatus}
            joinStatusText={group.joinStatusText}
            logo={group.logo}
            onViewClick={(id, name) => handleViewClick(id, name)}
          />
        ))}
      </div>
    </div>
  );
};

export default MyGroupsSwiper;
