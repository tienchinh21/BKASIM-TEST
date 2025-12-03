import React, {
  useState,
  useEffect,
  useCallback,
  useMemo,
  useRef,
  useLayoutEffect,
} from "react";
import { Page, useNavigate } from "zmp-ui";
import { useLocation } from "react-router-dom";
import { useRecoilState, useRecoilValue } from "recoil";
import axios from "axios";
import { toast } from "react-toastify";
import debounce from "lodash.debounce";
import useSetHeader from "../components/hooks/useSetHeader";
import {
  token,
  isRegister,
  userMembershipInfo,
  groupsDataCache,
} from "../recoil/RecoilState";
import { isCacheValid, CACHE_MAX_AGE } from "../utils/infiniteScrollUtils";
import RegisterDrawer from "../componentsGiba/RegisterDrawer";
import LoadingGiba from "../componentsGiba/LoadingGiba";
import Category from "../components/Category";
import { useLoading } from "../hooks/useApiCall";
import {
  GroupJoinStatus,
  GroupJoinStatusLabel,
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

interface GroupsFilterSearch {
  page: number;
  pageSize: number;
  keyword: string;
  joinStatus: string;
}

interface GroupsDataCache {
  listGroups: Group[];
  filterSearch: GroupsFilterSearch;
  scrollTop: number;
  timestamp: number | null;
  totalPages: number;
}

const GroupsListGiba: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const setHeader = useSetHeader();
  const userToken = useRecoilValue(token);
  const isLoggedIn = useRecoilValue(isRegister);
  const membershipInfo = useRecoilValue(userMembershipInfo);

  const [cache, setCache] = useRecoilState(groupsDataCache) as [
    GroupsDataCache,
    (
      valOrUpdater:
        | GroupsDataCache
        | ((currVal: GroupsDataCache) => GroupsDataCache)
    ) => void
  ];

  const [searchTerm, setSearchTerm] = useState(
    cache.filterSearch.keyword || ""
  );
  const [registerDrawerVisible, setRegisterDrawerVisible] = useState(false);
  const [selectedGroup, setSelectedGroup] = useState<{
    id: string;
    name: string;
  } | null>(null);
  const [refreshTrigger, setRefreshTrigger] = useState(0);
  const [loading, setLoading] = useState(false);

  const activeTab = cache.filterSearch.joinStatus || "";
  const groups = cache.listGroups as Group[];
  const isLoadingMore = loading && cache.filterSearch.page > 1;

  const scrollContainerRef = useRef<HTMLDivElement>(null);
  const isRestoringRef = useRef<boolean>(false);

  useEffect(() => {
    setHeader({
      title: "DANH SÁCH CLUB",
      showUserInfo: false,
      showMenuButton: false,
      showCloseButton: false,
      hasLeftIcon: true,
    });
  }, [setHeader]);

  useLayoutEffect(() => {
    setCache((prev) => {
      const cacheIsValid = isCacheValid(prev.timestamp, CACHE_MAX_AGE);
      if (cacheIsValid && prev.listGroups.length > 0) {
        isRestoringRef.current = true;
        return prev;
      }
      return {
        ...prev,
        listGroups: [],
        filterSearch: { ...prev.filterSearch, page: 1 },
        scrollTop: 0,
        timestamp: null,
      };
    });
  }, []);

  useLayoutEffect(() => {
    if (!isRestoringRef.current || cache.scrollTop === 0) return;
    const container = scrollContainerRef.current;
    if (!container) {
      isRestoringRef.current = false;
      return;
    }
    requestAnimationFrame(() => {
      if (container && cache.scrollTop > 0) {
        container.scrollTop = cache.scrollTop;
      }
      isRestoringRef.current = false;
    });
  }, [cache.listGroups.length]);

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
      if (searchTerm !== cache.filterSearch.keyword) {
        setCache((prev) => ({
          ...prev,
          listGroups: [],
          filterSearch: { ...prev.filterSearch, page: 1, keyword: searchTerm },
          scrollTop: 0,
          timestamp: null,
        }));
      }
    }, 350);
    return () => clearTimeout(timer);
  }, [searchTerm, cache.filterSearch.keyword, setCache]);

  const buildApiUrl = useCallback(
    (page: number = cache.filterSearch.page) => {
      const params = new URLSearchParams({
        Page: String(page),
        PageSize: String(cache.filterSearch.pageSize),
        Keyword: cache.filterSearch.keyword || "",
      });

      const joinStatus = cache.filterSearch.joinStatus;
      if (joinStatus === GroupJoinStatus.PENDING) {
        params.append("JoinStatus", "pending");
      } else if (joinStatus === GroupJoinStatus.APPROVED) {
        params.append("JoinStatus", "approved");
      }

      return `${dfData.domain}/api/Groups/all?${params.toString()}`;
    },
    [cache.filterSearch]
  );

  const filterGroups = useCallback(
    (groupsList: Group[]): Group[] => {
      const currentJoinStatus = cache.filterSearch.joinStatus;
      if (currentJoinStatus === GroupJoinStatus.NOT_JOINED) {
        return groupsList.filter(
          (group) =>
            group.isActive &&
            (group.joinStatus === null || group.joinStatus === "rejected")
        );
      }

      if (currentJoinStatus === GroupJoinStatus.PENDING) {
        return groupsList.filter(
          (group) => group.isActive && group.joinStatus === "pending"
        );
      }

      if (currentJoinStatus === GroupJoinStatus.APPROVED) {
        return groupsList.filter(
          (group) => group.isActive && group.joinStatus === "approved"
        );
      }

      return groupsList.filter((group) => group.isActive);
    },
    [cache.filterSearch.joinStatus]
  );

  useEffect(() => {
    if (isRestoringRef.current) {
      isRestoringRef.current = false;
      return;
    }
    const fetchGroups = async () => {
      setLoading(true);
      try {
        const currentPage = cache.filterSearch.page;
        const url = buildApiUrl(currentPage);
        const headers: Record<string, string> = {};
        if (userToken) headers.Authorization = `Bearer ${userToken}`;
        const response = await axios.get(url, { headers });
        if (response.data.code === 0 && response.data.data?.items) {
          const filteredGroups = filterGroups(response.data.data.items);
          const totalPages = response.data.data.totalPages || 1;
          const isFirstPage = currentPage === 1;
          setCache((prev) => ({
            ...prev,
            listGroups: isFirstPage
              ? filteredGroups
              : [...prev.listGroups, ...filteredGroups],
            totalPages,
            timestamp: isFirstPage ? Date.now() : prev.timestamp,
          }));
        } else if (cache.filterSearch.page === 1) {
          setCache((prev) => ({
            ...prev,
            listGroups: [],
            timestamp: Date.now(),
          }));
        }
      } catch (error) {
        console.error("Error fetching groups:", error);
        if (cache.filterSearch.page === 1) {
          setCache((prev) => ({
            ...prev,
            listGroups: [],
            timestamp: Date.now(),
          }));
        }
      } finally {
        setLoading(false);
      }
    };
    fetchGroups();
  }, [
    cache.filterSearch,
    buildApiUrl,
    filterGroups,
    userToken,
    refreshTrigger,
    setCache,
  ]);

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

  const handleTabChange = useCallback(
    (tabValue: string) => {
      setCache((prev) => ({
        ...prev,
        listGroups: [],
        filterSearch: { ...prev.filterSearch, page: 1, joinStatus: tabValue },
        scrollTop: 0,
        timestamp: null,
      }));
    },
    [setCache]
  );

  const handleCloseDrawer = useCallback(() => {
    setRegisterDrawerVisible(false);
    setSelectedGroup(null);
  }, []);

  // eslint-disable-next-line react-hooks/exhaustive-deps
  const handleScroll = useCallback(
    debounce((e: React.UIEvent<HTMLDivElement>) => {
      if (isRestoringRef.current) {
        isRestoringRef.current = false;
        return;
      }
      const { scrollTop, clientHeight, scrollHeight } =
        e.target as HTMLDivElement;
      if (scrollTop === 0 && cache.filterSearch.page === 1) return;
      setCache((prev) => {
        const page = prev.filterSearch?.page || 1;
        const totalPages = prev.totalPages || 1;
        if (
          scrollTop + clientHeight >= scrollHeight - 100 &&
          !loading &&
          page < totalPages
        ) {
          return {
            ...prev,
            scrollTop,
            timestamp: Date.now(),
            filterSearch: { ...prev.filterSearch, page: page + 1 },
          };
        }
        return { ...prev, scrollTop, timestamp: Date.now() };
      });
    }, 200),
    [loading, cache.filterSearch.page, cache.totalPages]
  );

  const statusTabs = useMemo(() => {
    const tabs = [
      {
        id: "all",
        name: GroupJoinStatusLabel[GroupJoinStatus.ALL],
        value: GroupJoinStatus.ALL,
      },
      {
        id: "not-joined",
        name: GroupJoinStatusLabel[GroupJoinStatus.NOT_JOINED],
        value: GroupJoinStatus.NOT_JOINED,
      },
      {
        id: "pending",
        name: GroupJoinStatusLabel[GroupJoinStatus.PENDING],
        value: GroupJoinStatus.PENDING,
      },
      {
        id: "approved",
        name: GroupJoinStatusLabel[GroupJoinStatus.APPROVED],
        value: GroupJoinStatus.APPROVED,
      },
    ];

    if (!isLoggedIn) {
      return tabs.filter(
        (tab) =>
          tab.value !== GroupJoinStatus.PENDING &&
          tab.value !== GroupJoinStatus.APPROVED
      );
    }

    return tabs;
  }, [isLoggedIn]);

  return (
    <Page className="bg-white min-h-screen mt-[50px]">
      <div
        className="h-full overflow-y-auto"
        ref={scrollContainerRef}
        style={{ height: "calc(100vh - 50px)" }}
        onScroll={handleScroll}
      >
        <div className="mb-4">
          <Category
            list={statusTabs}
            value={activeTab}
            onChange={handleTabChange}
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
          {loading && cache.filterSearch.page === 1 && groups.length === 0 ? (
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
              {isLoadingMore && (
                <div className="flex justify-center py-4">
                  <LoadingGiba size="md" text="Đang tải thêm..." />
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
