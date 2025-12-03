import React, { useState, useEffect, useCallback } from "react";
import { useNavigate } from "zmp-ui";
import { useLocation } from "react-router-dom";
import { useRecoilValue } from "recoil";
import {
  isRegister,
  token,
  userMembershipInfo,
} from "../../../recoil/RecoilState";
import axios from "axios";
import RegisterDrawer from "../../../componentsGiba/RegisterDrawer";
import dfData from "../../../common/DefaultConfig.json";
import noImage from "../../../assets/no_image.png";
import { ChevronRight } from "lucide-react";

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
      <div className="w-full h-32 bg-gray-200"></div>
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
  logo,
}) => {
  return (
    <div
      className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden min-w-[240px] max-w-[280px] hover:shadow-md transition-shadow duration-200 cursor-pointer"
      onClick={() => onViewClick?.(id, groupName, logo)}
    >
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

      <div className="p-4 pb-3">
        <div className="flex items-start justify-between mb-2">
          <h3 className="text-base font-semibold text-gray-900 line-clamp-1 flex-1 mr-2">
            {groupName}
          </h3>
          {/* <span
            className={`px-2 py-1 rounded-md text-xs font-medium flex-shrink-0 ${
              isActive
                ? "bg-green-100 text-green-700"
                : "bg-gray-100 text-gray-600"
            }`}
          >
            {isActive ? "Hoạt động" : "Tạm dừng"}
          </span> */}
        </div>

        {/* <p className="text-sm text-gray-600 line-clamp-2 leading-relaxed">
          {description}
        </p> */}
      </div>

      <div className="px-4 pb-4">
        {/* <div className="flex items-center justify-between mb-3">
          <span className="text-xs text-gray-500">
            {memberCount.toLocaleString()} thành viên
          </span>
        </div> */}

        <div className="flex gap-2">
          {!isJoined && (
            <button
              disabled={
                !isRegistered ||
                (membershipInfo && membershipInfo.approvalStatus !== 1)
              }
              onClick={(e) => {
                e.stopPropagation();
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

interface GroupsSectionHomeProps {
  refreshTrigger?: number;
  onRegisterSuccess?: () => void;
}

const GroupsSectionHome: React.FC<GroupsSectionHomeProps> = ({
  refreshTrigger,
  onRegisterSuccess,
}) => {
  const navigate = useNavigate();
  const userToken = useRecoilValue(token);
  const [nbdGroups, setNbdGroups] = useState<any[]>([]);
  const [clubGroups, setClubGroups] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const isLoggedIn = useRecoilValue(isRegister);
  const membershipInfo = useRecoilValue(userMembershipInfo);

  const [registerDrawerVisible, setRegisterDrawerVisible] = useState(false);
  const [selectedGroup, setSelectedGroup] = useState<{
    id: string;
    name: string;
  } | null>(null);

  // Check if we need to reopen drawer when returning from BehaviorRulesGiba
  const location = useLocation();
  useEffect(() => {
    // Check all possible drawer state keys in sessionStorage
    const keys = Object.keys(sessionStorage);
    const drawerStateKey = keys.find((key) =>
      key.startsWith("registerDrawer_state_")
    );

    if (
      drawerStateKey &&
      (nbdGroups.length > 0 || clubGroups.length > 0) &&
      !registerDrawerVisible
    ) {
      try {
        const savedState = JSON.parse(
          sessionStorage.getItem(drawerStateKey) || "{}"
        );
        if (savedState.visible && savedState.groupId) {
          // Find the group and open drawer
          const allGroups = [...(nbdGroups || []), ...(clubGroups || [])];
          const group = allGroups.find((g) => g.id === savedState.groupId);
          if (group) {
            setSelectedGroup({ id: group.id, name: group.name });
            setRegisterDrawerVisible(true);
          }
        }
      } catch (error) {
        console.error("Error restoring drawer state:", error);
      }
    }
  }, [
    nbdGroups,
    clubGroups,
    registerDrawerVisible,
    location.pathname,
    location.key,
  ]);

  // Fetch groups from new API
  const fetchGroups = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);

      const response = await axios.get(
        `${dfData.domain}/api/Groups/all?PageSize=20`,
        {
          headers: userToken
            ? {
                Authorization: `Bearer ${userToken}`,
              }
            : {},
        }
      );

      if (response.data.code === 0 && response.data.data?.items) {
        const allGroups = response.data.data.items;

        // Filter only groups that are NOT joined (joinStatus is null or rejected)
        const availableGroups = allGroups.filter(
          (group: any) =>
            group.joinStatus === null || group.joinStatus === "rejected"
        );

        // Separate by type and limit to 10 each
        const nbdFiltered = availableGroups
          .filter((group: any) => group.type === "NBD")
          .slice(0, 10);

        const clubFiltered = availableGroups
          .filter((group: any) => group.type === "Club")
          .slice(0, 10);

        setNbdGroups(nbdFiltered);
        setClubGroups(clubFiltered);
      } else {
        setNbdGroups([]);
        setClubGroups([]);
      }
    } catch (err) {
      console.error("Error fetching groups:", err);
      setError("Không thể tải danh sách Club");
      setNbdGroups([]);
      setClubGroups([]);
    } finally {
      setLoading(false);
    }
  }, [userToken]);

  useEffect(() => {
    fetchGroups();
  }, [fetchGroups]);

  useEffect(() => {
    if (refreshTrigger && refreshTrigger > 0) {
      fetchGroups();
    }
  }, [refreshTrigger, fetchGroups]);

  const handleJoinClick = (groupId: string, groupName: string) => {
    setSelectedGroup({ id: groupId, name: groupName });
    setRegisterDrawerVisible(true);
  };

  const handleRegisterSuccess = () => {
    fetchGroups();
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

  if (loading) {
    return (
      <div className="w-full">
        <div className="mb-6">
          <h3 className="text-lg font-bold text-black mb-4">NBD</h3>
          <div className="flex gap-4 overflow-x-auto pb-4">
            {[...Array(3)].map((_, index) => (
              <GroupCardSkeleton key={index} />
            ))}
          </div>
        </div>
        <div className="mb-6">
          <h3 className="text-lg font-bold text-black mb-4">CLUB</h3>
          <div className="flex gap-4 overflow-x-auto pb-4">
            {[...Array(3)].map((_, index) => (
              <GroupCardSkeleton key={index} />
            ))}
          </div>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="w-full">
        <div className="text-center text-red-500 py-4">{error}</div>
      </div>
    );
  }

  return (
    <div className="w-full">
      <div className="mb-8">
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-lg font-bold text-black">NBD</h3>
          <div className="flex items-center gap-1">
            <button
              className="text-black text-sm font-medium hover:text-gray-600 transition-colors"
              onClick={() => navigate("/giba/groups?type=NBD")}
            >
              Xem thêm
            </button>
            <ChevronRight size={14} />
          </div>
        </div>

        {nbdGroups.length === 0 ? (
          <div className="text-center text-gray-500 py-8">
            <p className="text-sm">Chưa có nhóm NBD nào</p>
          </div>
        ) : (
          <div className="flex gap-4 overflow-x-auto pb-4">
            {nbdGroups.map((group) => (
              <GroupCard
                isRegistered={isLoggedIn && membershipInfo.approvalStatus === 1}
                membershipInfo={membershipInfo}
                key={group.id}
                id={group.id}
                groupName={group.groupName}
                description={group.description}
                memberCount={group.memberCount}
                isJoined={group.isJoined}
                isActive={group.isActive}
                logo={group.logo}
                onJoinClick={(id) => handleJoinClick(id, group.groupName)}
                onViewClick={(id, name) => handleViewClick(id, name)}
              />
            ))}
          </div>
        )}
      </div>

      {/* CLUB Section */}
      <div className="mb-6">
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-lg font-bold text-black">CLUB</h3>
          <div className="flex items-center gap-1">
            <button
              className="text-black text-sm font-medium hover:text-gray-600 transition-colors"
              onClick={() => navigate("/giba/groups?type=Club")}
            >
              Xem thêm
            </button>
            <ChevronRight size={14} />
          </div>
        </div>

        {clubGroups.length === 0 ? (
          <div className="text-center text-gray-500 py-8">
            <p className="text-sm">Chưa có nhóm Club nào</p>
          </div>
        ) : (
          <div className="flex gap-4 overflow-x-auto pb-4">
            {clubGroups.map((group) => (
              <GroupCard
                isRegistered={isLoggedIn && membershipInfo.approvalStatus === 1}
                membershipInfo={membershipInfo}
                key={group.id}
                id={group.id}
                groupName={group.groupName}
                description={group.description}
                memberCount={group.memberCount}
                isJoined={group.isJoined}
                isActive={group.isActive}
                logo={group.logo}
                onJoinClick={(id) => handleJoinClick(id, group.groupName)}
                onViewClick={(id, name) => handleViewClick(id, name)}
              />
            ))}
          </div>
        )}
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

export default GroupsSectionHome;
