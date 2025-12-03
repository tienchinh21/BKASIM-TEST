import React, { useState, useEffect, useCallback, useMemo } from "react";
import { Page, useNavigate } from "zmp-ui";
import { useLocation } from "react-router-dom";
import { useRecoilValue } from "recoil";
import axios from "axios";
import { toast } from "react-toastify";
import useSetHeader from "../components/hooks/useSetHeader";
import { token, isRegister, userMembershipInfo } from "../recoil/RecoilState";
import RegisterDrawer from "../componentsGiba/RegisterDrawer";
import LoadingGiba from "../componentsGiba/LoadingGiba";
import TwoTierTab from "../components/TwoTierTab/TwoTierTab";
import { useLoading } from "../hooks/useApiCall";
import {
  GroupType,
  GroupJoinStatus,
  GroupTabsData,
} from "../utils/enum/group.enum";
import dfData from "../common/DefaultConfig.json";
import noImage from "../assets/no_image.png";

interface Group {
  id: string;
  groupName: string;
  description: string;
  rule: string;
  isActive: boolean;
  memberCount: number;
  createdDate: string;
  updatedDate: string;
  isJoined: boolean;
  joinStatus: string | null;
  joinStatusText: string | null;
  logo?: string | null;
}

const GroupsListGiba: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const setHeader = useSetHeader();
  const userToken = useRecoilValue(token);
  const isLoggedIn = useRecoilValue(isRegister);
  const membershipInfo = useRecoilValue(userMembershipInfo);
  const [searchTerm, setSearchTerm] = useState("");
  const [debouncedSearchTerm, setDebouncedSearchTerm] = useState("");
  const [groups, setGroups] = useState<Group[]>([]);
  const [registerDrawerVisible, setRegisterDrawerVisible] = useState(false);
  const [selectedGroup, setSelectedGroup] = useState<{
    id: string;
    name: string;
  } | null>(null);
  const [refreshTrigger, setRefreshTrigger] = useState(0);

  const [activeTab, setActiveTab] = useState<string>(GroupType.ALL);
  const [activeChildTab, setActiveChildTab] = useState<string>(
    GroupJoinStatus.ALL
  );

  const { loading, withLoading } = useLoading();

  useEffect(() => {
    setHeader({
      title: "DANH SÁCH CLUB",
      showUserInfo: false,
      showMenuButton: false,
      showCloseButton: false,
      hasLeftIcon: true,
    });
  }, [setHeader]);

  useEffect(() => {
    if (groups.length === 0 || registerDrawerVisible) return;

    const keys = Object.keys(sessionStorage);
    const drawerStateKey = keys.find((key) =>
      key.startsWith("registerDrawer_state_")
    );

    if (!drawerStateKey) return;

    try {
      const savedState = JSON.parse(
        sessionStorage.getItem(drawerStateKey) || "{}"
      );
      if (savedState.visible && savedState.groupId) {
        const group = groups.find((g) => g.id === savedState.groupId);
        if (group) {
          setSelectedGroup({ id: group.id, name: group.groupName });
          setRegisterDrawerVisible(true);
        }
      }
    } catch (error) {
      console.error("Error restoring drawer state:", error);
    }
  }, [groups, location.pathname, location.key, registerDrawerVisible]);

  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedSearchTerm(searchTerm);
    }, 350);

    return () => clearTimeout(timer);
  }, [searchTerm]);

  const buildApiUrl = useCallback(() => {
    const params = new URLSearchParams({
      Page: "1",
      PageSize: "15",
      Keyword: debouncedSearchTerm || "",
    });

    if (
      activeTab &&
      activeTab !== GroupType.ALL &&
      activeTab !== GroupType.MY_GROUPS
    ) {
      params.append("GroupType", activeTab);
    }

    if (activeChildTab === GroupJoinStatus.PENDING) {
      params.append("JoinStatus", "pending");
    } else if (activeChildTab === GroupJoinStatus.APPROVED) {
      params.append("JoinStatus", "approved");
    }

    return `${dfData.domain}/api/Groups/all?${params.toString()}`;
  }, [debouncedSearchTerm, activeTab, activeChildTab]);

  const filterGroups = useCallback(
    (groups: Group[]): Group[] => {
      if (activeChildTab === GroupJoinStatus.NOT_JOINED) {
        return groups.filter(
          (group) =>
            group.isActive &&
            (group.joinStatus === null || group.joinStatus === "rejected")
        );
      }

      if (activeChildTab === GroupJoinStatus.ALL) {
        if (activeTab === GroupType.MY_GROUPS) {
          return groups.filter(
            (group) =>
              group.isActive &&
              (group.joinStatus === "pending" ||
                group.joinStatus === "approved")
          );
        }
        return groups.filter((group) => group.isActive);
      }

      return groups.filter(
        (group) => group.isActive && group.joinStatus !== "rejected"
      );
    },
    [activeTab, activeChildTab]
  );

  useEffect(() => {
    const fetchGroups = async () => {
      await withLoading(async () => {
        try {
          const url = buildApiUrl();
          const headers: any = {};

          if (userToken) {
            headers.Authorization = `Bearer ${userToken}`;
          }

          const response = await axios.get(url, { headers });

          if (response.data.code === 0 && response.data.data?.items) {
            const filteredGroups = filterGroups(response.data.data.items);
            setGroups(filteredGroups);
          } else {
            setGroups([]);
          }
        } catch (error) {
          console.error("Error fetching groups:", error);
          setGroups([]);
        }
      });
    };

    fetchGroups();
  }, [buildApiUrl, filterGroups, userToken, refreshTrigger, withLoading]);

  // Memoized handlers
  const handleJoinClick = useCallback(
    (groupId: string, groupName: string, joinStatus?: string | null) => {
      if (!isLoggedIn) {
        toast.error("Vui lòng đăng nhập để tham gia nhóm!");
        return;
      }

      if (
        membershipInfo.approvalStatus === 0 ||
        membershipInfo.approvalStatus === null
      ) {
        toast.error(
          "Tài khoản đang trong thời gian xét duyệt. Vui lòng chờ admin phê duyệt."
        );
        return;
      }

      if (membershipInfo.approvalStatus === 2) {
        toast.error("Tài khoản đã bị từ chối. Vui lòng liên hệ admin.");
        return;
      }

      if (joinStatus === "pending") {
        toast.info(
          "Bạn đã gửi yêu cầu tham gia nhóm này. Vui lòng chờ admin phê duyệt."
        );
        return;
      }

      setSelectedGroup({ id: groupId, name: groupName });
      setRegisterDrawerVisible(true);
    },
    [isLoggedIn, membershipInfo.approvalStatus]
  );

  const handleViewClick = useCallback(
    (groupId: string, groupName: string, groupLogo?: string | null) => {
      navigate(`/giba/group-detail/${groupId}`, {
        state: { groupName, groupLogo },
      });
    },
    [navigate]
  );

  const handleRegisterSuccess = useCallback(() => {
    setRefreshTrigger((prev) => prev + 1);
  }, []);

  const handleTabChange = useCallback((tabValue: string) => {
    setActiveTab(tabValue);
    setActiveChildTab(GroupJoinStatus.ALL);
  }, []);

  const handleChildTabChange = useCallback((childValue: string) => {
    setActiveChildTab(childValue);
  }, []);

  const handleCloseDrawer = useCallback(() => {
    setRegisterDrawerVisible(false);
    setSelectedGroup(null);
  }, []);

  const filteredTabs = useMemo(() => {
    if (isLoggedIn) {
      return GroupTabsData;
    }

    return GroupTabsData.filter((tab) => tab.value !== GroupType.MY_GROUPS).map(
      (tab) => {
        if (tab.children) {
          return {
            ...tab,
            children: tab.children.filter(
              (child) =>
                child.value !== GroupJoinStatus.PENDING &&
                child.value !== GroupJoinStatus.APPROVED
            ),
          };
        }
        return tab;
      }
    );
  }, [isLoggedIn]);

  useEffect(() => {
    const isValidTab = filteredTabs.some((tab) => tab.value === activeTab);

    if (!isValidTab && filteredTabs.length > 0) {
      setActiveTab(filteredTabs[0].value);
      setActiveChildTab(GroupJoinStatus.ALL);
    }
  }, [filteredTabs, activeTab]);

  return (
    <Page className="bg-white min-h-screen mt-[50px]">
      <div className="">
        <div className="mb-4">
          <TwoTierTab
            tabs={filteredTabs}
            activeTab={activeTab}
            onTabChange={handleTabChange}
            activeChildTab={activeChildTab}
            onChildTabChange={handleChildTabChange}
            backgroundColor="#fff"
          />
        </div>

        <div className="px-4 mb-6">
          <input
            type="text"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            placeholder="Tìm kiếm nhóm..."
            className="w-full px-4 py-3 border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-black focus:border-transparent"
          />
        </div>

        <div className="px-4">
          {loading ? (
            <div className="flex justify-center py-8">
              <LoadingGiba size="lg" text="Đang tải danh sách Club..." />
            </div>
          ) : (
            <div className="space-y-4">
              {groups.length > 0 ? (
                groups.map((group) => (
                  <div
                    key={group.id}
                    className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden hover:shadow-md transition-shadow duration-200"
                  >
                    <div className="w-full h-40 bg-gray-100 overflow-hidden">
                      <img
                        src={group.logo || noImage}
                        alt={group.groupName}
                        className="w-full h-full object-cover"
                        onError={(e) => {
                          (e.target as HTMLImageElement).src = noImage;
                        }}
                      />
                    </div>

                    <div className="p-4 pb-3">
                      <div className="flex items-start justify-between mb-2">
                        <h3 className="text-base font-semibold text-gray-900 line-clamp-1 flex-1 mr-2">
                          {group.groupName}
                        </h3>
                        <span
                          className={`px-2 py-1 rounded-md text-xs font-medium flex-shrink-0 ${
                            group.isJoined
                              ? "bg-green-100 text-green-700"
                              : group.joinStatus === "pending"
                              ? "bg-yellow-100 text-yellow-700"
                              : "bg-blue-100 text-blue-700"
                          }`}
                        >
                          {group.isJoined
                            ? "Đã tham gia"
                            : group.joinStatus === "rejected" ||
                              !group.joinStatusText
                            ? "Có thể tham gia"
                            : group.joinStatusText}
                        </span>
                      </div>

                      <p className="text-sm text-gray-600 line-clamp-2 leading-relaxed">
                        {group.description}
                      </p>
                    </div>

                    <div className="px-4 pb-4">
                      <div className="flex items-center justify-between mb-3">
                        <span className="text-xs text-gray-500">
                          {group.memberCount.toLocaleString()} thành viên
                        </span>
                      </div>

                      <div className="flex gap-2">
                        {group.isJoined ? (
                          <button
                            onClick={() =>
                              handleViewClick(
                                group.id,
                                group.groupName,
                                group.logo
                              )
                            }
                            className="flex-1 bg-white text-gray-900 py-2.5 px-4 rounded-lg text-sm font-medium border border-gray-300 hover:bg-gray-50 transition-colors"
                          >
                            Xem chi tiết
                          </button>
                        ) : (
                          <>
                            <button
                              onClick={() =>
                                handleViewClick(
                                  group.id,
                                  group.groupName,
                                  group.logo
                                )
                              }
                              className="flex-1 bg-white text-gray-900 py-2.5 px-4 rounded-lg text-sm font-medium border border-gray-300 hover:bg-gray-50 transition-colors"
                            >
                              Xem chi tiết
                            </button>
                            <button
                              disabled={
                                !isLoggedIn ||
                                membershipInfo.approvalStatus !== 1 ||
                                group.joinStatus === "pending"
                              }
                              onClick={() =>
                                handleJoinClick(
                                  group.id,
                                  group.groupName,
                                  group.joinStatus
                                )
                              }
                              className={`flex-1 py-2.5 px-4 rounded-lg text-sm font-medium transition-colors ${
                                !isLoggedIn ||
                                membershipInfo.approvalStatus !== 1 ||
                                group.joinStatus === "pending"
                                  ? "bg-gray-300 text-gray-500 cursor-not-allowed opacity-50"
                                  : "bg-yellow-500 text-black hover:bg-yellow-600"
                              }`}
                            >
                              {group.joinStatus === "pending"
                                ? "Chờ phê duyệt"
                                : "Tham gia"}
                            </button>
                          </>
                        )}
                      </div>
                    </div>
                  </div>
                ))
              ) : (
                <div className="text-center py-12">
                  <div className="text-gray-500 text-lg mb-2">
                    Không tìm thấy nhóm nào
                  </div>
                  <div className="text-gray-400 text-sm">
                    Thử tìm kiếm với từ khóa khác
                  </div>
                </div>
              )}
            </div>
          )}
        </div>

        <div className="pb-20"></div>
      </div>
      {selectedGroup && (
        <RegisterDrawer
          visible={registerDrawerVisible}
          onClose={handleCloseDrawer}
          groupId={selectedGroup.id}
          groupName={selectedGroup.name}
          onSuccess={handleRegisterSuccess}
        />
      )}
    </Page>
  );
};

export default GroupsListGiba;
