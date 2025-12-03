import React, { useState, useEffect } from "react";
import { useNavigate } from "zmp-ui";
import { useRecoilValue } from "recoil";
import {
  isRegister,
  token,
  infoShare,
  userMembershipInfo,
} from "../recoil/RecoilState";
import axios from "axios";
import RegisterDrawer from "./RegisterDrawer";
import dfData from "../common/DefaultConfig.json";
interface GroupCardProps {
  id: string;
  groupName: string;
  description: string;
  memberCount: number;
  isJoined: boolean;
  isActive: boolean;
  isRegistered?: boolean;
  membershipInfo?: any;
  logo?: string | null;
  onJoinClick?: (groupId: string) => void;
  onViewClick?: (
    groupId: string,
    groupName: string,
    groupLogo?: string | null
  ) => void;
}

// Skeleton component cho loading
const GroupCardSkeleton: React.FC = () => {
  return (
    <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden min-w-[240px] max-w-[280px] animate-pulse">
      {/* Header */}
      <div className="p-4 pb-3">
        <div className="flex items-start justify-between mb-2">
          <div className="h-4 bg-gray-200 rounded flex-1 mr-2"></div>
          <div className="h-6 w-16 bg-gray-200 rounded-md"></div>
        </div>
        <div className="space-y-2">
          <div className="h-3 bg-gray-200 rounded w-full"></div>
          <div className="h-3 bg-gray-200 rounded w-3/4"></div>
        </div>
      </div>

      {/* Stats and Actions */}
      <div className="px-4 pb-4">
        <div className="flex items-center justify-between mb-3">
          <div className="h-3 w-20 bg-gray-200 rounded"></div>
        </div>
        <div className="h-10 bg-gray-200 rounded-lg"></div>
      </div>
    </div>
  );
};

const GroupCard: React.FC<GroupCardProps> = ({
  id,
  groupName,
  description,
  memberCount,
  isJoined,
  isActive,
  onJoinClick,
  onViewClick,
  isRegistered,
  membershipInfo,
}) => {
  return (
    <div
      className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden min-w-[240px] max-w-[280px] hover:shadow-md transition-shadow duration-200 cursor-pointer"
      onClick={() => onViewClick?.(id, groupName, logo)}
    >
      {/* Header */}
      <div className="p-4 pb-3">
        <div className="flex items-start justify-between mb-2">
          <h3 className="text-base font-semibold text-gray-900 line-clamp-1 flex-1 mr-2">
            {groupName}
          </h3>
          <span
            className={`px-2 py-1 rounded-md text-xs font-medium flex-shrink-0 ${
              isActive
                ? "bg-green-100 text-green-700"
                : "bg-gray-100 text-gray-600"
            }`}
          >
            {isActive ? "Hoạt động" : "Tạm dừng"}
          </span>
        </div>

        <p className="text-sm text-gray-600 line-clamp-2 leading-relaxed">
          {description}
        </p>
      </div>

      {/* Stats and Actions */}
      <div className="px-4 pb-4">
        {/* Stats */}
        <div className="flex items-center justify-between mb-3">
          <span className="text-xs text-gray-500">
            {memberCount.toLocaleString()} thành viên
          </span>
        </div>

        {/* Actions */}
        <div className="flex gap-2">
          {!isJoined && (
            <button
              disabled={
                !isRegistered ||
                (membershipInfo && membershipInfo.approvalStatus !== 1)
              }
              onClick={(e) => {
                e.stopPropagation(); // Ngăn chặn event bubbling lên card
                onJoinClick?.(id);
              }}
              className={`flex-1 bg-yellow-500 text-black py-2.5 px-4 rounded-lg text-sm font-medium hover:bg-yellow-600 transition-colors ${
                !isRegistered ||
                (membershipInfo && membershipInfo.approvalStatus !== 1)
                  ? "opacity-50 cursor-not-allowed"
                  : ""
              }`}
            >
              Tham gia
            </button>
          )}
        </div>
      </div>
    </div>
  );
};

interface SwiperGibaProps {
  className?: string;
  refreshTrigger?: number; // Trigger để reload data
  onRegisterSuccess?: () => void; // Callback khi đăng ký thành công
}

const SwiperGiba: React.FC<SwiperGibaProps> = ({
  className = "",
  refreshTrigger,
  onRegisterSuccess,
}) => {
  const navigate = useNavigate();
  const userToken = useRecoilValue(token);
  const [groups, setGroups] = useState<any[]>([]);
  const [myGroups, setMyGroups] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const isLoggedIn = useRecoilValue(isRegister);
  const membershipInfo = useRecoilValue(userMembershipInfo);

  const [registerDrawerVisible, setRegisterDrawerVisible] = useState(false);
  const [selectedGroup, setSelectedGroup] = useState<{
    id: string;
    name: string;
  } | null>(null);

  // Fetch groups from API
  const fetchGroups = async () => {
    try {
      setLoading(true);
      const response = await axios.get(`${dfData.domain}/api/groups`, {
        headers: {
          Authorization: `Bearer ${userToken}`,
        },
      });

      if (response.data.code === 0 && response.data.data?.items) {
        // Filter only active groups
        const activeGroups = response.data.data.items.filter(
          (group: any) => group.isActive
        );
        console.log("Filtered groups:", activeGroups);
        setGroups(activeGroups);
      } else {
        setGroups([]);
      }
    } catch (err) {
      console.error("Error fetching groups:", err);
      setError("Không thể tải danh sách Club");
      setGroups([]);
    } finally {
      setLoading(false);
    }
  };

  // Fetch my groups để biết nhóm nào đã tham gia
  const fetchMyGroups = async () => {
    if (!userToken) {
      setMyGroups([]);
      return;
    }

    try {
      const response = await axios.get(
        `${dfData.domain}/api/groups/my-groups`,
        {
          headers: {
            Authorization: `Bearer ${userToken}`,
          },
        }
      );

      if (response.data.data) {
        const joinedGroups = response.data.data.filter(
          (group: any) => group.isJoined
        );
        setMyGroups(joinedGroups);
      } else {
        setMyGroups([]);
      }
    } catch (error) {
      console.error("Error fetching my groups:", error);
      setMyGroups([]);
    }
  };

  // useEffect theo dõi token và fetch data
  useEffect(() => {
    const loadData = async () => {
      await Promise.all([fetchGroups(), fetchMyGroups()]);
    };

    loadData();
  }, [userToken]); // Theo dõi token thay đổi

  // useEffect theo dõi refreshTrigger để reload data
  useEffect(() => {
    if (refreshTrigger && refreshTrigger > 0) {
      const loadData = async () => {
        await Promise.all([fetchGroups(), fetchMyGroups()]);
      };
      loadData();
    }
  }, [refreshTrigger]);

  const handleJoinClick = (groupId: string, groupName: string) => {
    setSelectedGroup({ id: groupId, name: groupName });
    setRegisterDrawerVisible(true);
  };

  const handleRegisterSuccess = () => {
    // Refresh cả groups và myGroups sau khi đăng ký thành công
    const refreshData = async () => {
      await Promise.all([fetchGroups(), fetchMyGroups()]);
    };
    refreshData();

    // Gọi callback từ parent component
    onRegisterSuccess?.();
  };

  const handleViewClick = (
    groupId: string,
    groupName: string,
    groupLogo?: string | null
  ) => {
    navigate(`/giba/group-detail/${groupId}`, {
      state: { groupName, groupLogo },
    });
  };

  const isGroupJoined = (groupId: string) => {
    return myGroups.some((group) => group.id === groupId);
  };

  if (loading) {
    return (
      <div className={`w-full ${className}`}>
        <div className="flex gap-4 overflow-x-auto pb-4">
          {[...Array(3)].map((_, index) => (
            <GroupCardSkeleton key={index} />
          ))}
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className={`w-full ${className}`}>
        <div className="text-center text-red-500 py-4">{error}</div>
      </div>
    );
  }

  if (groups.length === 0) {
    return (
      <div className={`w-full ${className}`}>
        <div className="text-center text-gray-500 py-8">
          <p>Chưa có nhóm nào hoạt động</p>
        </div>
      </div>
    );
  }

  return (
    <div className={`w-full ${className}`}>
      <div className="flex gap-4 overflow-x-auto pb-4">
        {groups.map((group) => (
          <GroupCard
            isRegistered={isLoggedIn && membershipInfo.approvalStatus === 1}
            membershipInfo={membershipInfo}
            key={group.id}
            id={group.id}
            groupName={group.groupName}
            description={group.description}
            memberCount={group.memberCount}
            isJoined={isGroupJoined(group.id)} // Sử dụng helper function
            isActive={group.isActive}
            logo={group.logo}
            onJoinClick={(id) => handleJoinClick(id, group.groupName)}
            onViewClick={(id, name, logo) => handleViewClick(id, name, logo)}
          />
        ))}
      </div>

      {/* Register Drawer */}
      {selectedGroup && (
        <RegisterDrawer
          visible={registerDrawerVisible}
          onClose={() => {
            setRegisterDrawerVisible(false);
            setSelectedGroup(null);
          }}
          groupId={selectedGroup.id}
          groupName={selectedGroup.name}
          onSuccess={handleRegisterSuccess}
        />
      )}
    </div>
  );
};

export default SwiperGiba;
