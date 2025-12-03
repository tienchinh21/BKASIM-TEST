import React, {
  useState,
  useEffect,
  useRef,
  useCallback,
  useMemo,
} from "react";
import { Page, Box, useSnackbar } from "zmp-ui";
import { useNavigate } from "zmp-ui";
import { useParams, useLocation } from "react-router-dom";
import useSetHeader from "../components/hooks/useSetHeader";
import { useRecoilValue } from "recoil";
import { token, infoShare, userMembershipInfo } from "../recoil/RecoilState";
import axios from "axios";
import RegisterDrawer from "../componentsGiba/RegisterDrawer";
import LoadingGiba from "../componentsGiba/LoadingGiba";
import EventItem from "../pages/event/EventItem";
import CommonShareModal from "../components/CommonShareModal";
import RefOptionModal from "../pagesGiba/ref/RefOptionModal";
import dfData from "../common/DefaultConfig.json";
import { APP_MODE } from "../state";

interface Event {
  id: string;
  title: string;
  banner: string;
  address: string;
  startTime: string;
  endTime: string;
  status: number;
  statusText: string;
}

interface Member {
  membershipId: string;
  membershipName: string;
  phoneNumber: string;
  avatar: string;
  joinedDate: string;
  isApproved: boolean;
  userZaloId?: string;
  slug?: string;
  sortOrder?: number;
  groupPosition?: string;
}

interface GroupInfo {
  id: string;
  groupName: string;
  description: string;
  rule: string;
  isActive: boolean;
  memberCount: number;
  createdDate: string;
  updatedDate: string;
  isJoined?: boolean;
  joinStatus?: string | null;
  joinStatusText?: string | null;
  logo?: string | null;
  mainActivities?: string;
  members?: Member[];
  allEvents?: Event[];
  publicEvents?: Event[];
}

type TabType = "info" | "events" | "members";

// Constants
const SNACKBAR_DURATION = 4000;
const SNACKBAR_ACTION = { text: "Đóng", close: true };

// Helper Functions
const normalizePhone = (phone: string): string => {
  return phone.replace(/\D/g, "");
};

const isPendingOrJoined = (groupData: GroupInfo): boolean => {
  return (
    groupData.isJoined ||
    groupData.joinStatus === "pending" ||
    groupData.joinStatus === "2" ||
    String(groupData.joinStatus) === "2" ||
    groupData.joinStatusText === "Chờ duyệt" ||
    groupData.joinStatus === "approved"
  );
};

const shouldShowJoinButton = (groupInfo: GroupInfo | null): boolean => {
  if (!groupInfo) return false;
  return (
    groupInfo.joinStatus !== "pending" &&
    groupInfo.joinStatus !== "2" &&
    String(groupInfo.joinStatus) !== "2" &&
    groupInfo.joinStatusText !== "Chờ duyệt" &&
    groupInfo.joinStatus !== "approved"
  );
};

const MemberCard: React.FC<{ member: Member; onClick?: () => void }> = ({
  member,
  onClick,
}) => {
  const avatarStyle: React.CSSProperties = {
    width: "48px",
    height: "48px",
    borderRadius: "50%",
    background: member.avatar ? "transparent" : "#ef4444",
    display: "flex",
    alignItems: "center",
    justifyContent: "center",
    color: "white",
    fontWeight: "600",
    fontSize: "18px",
    marginRight: "12px",
    overflow: "hidden",
    border: "2px solid #f1f5f9",
    flexShrink: 0,
  };

  return (
    <Box
      onClick={onClick}
      style={{
        background: "#fff",
        border: "1px solid #e2e8f0",
        borderRadius: "12px",
        padding: "16px",
        boxShadow: "0 1px 3px rgba(0, 0, 0, 0.1)",
        transition: "all 0.2s",
        cursor: onClick ? "pointer" : "default",
      }}
    >
      <Box style={{ display: "flex", alignItems: "center" }}>
        <Box style={avatarStyle}>
          {member.avatar ? (
            <img
              src={member.avatar}
              alt={member.membershipName}
              style={{ width: "100%", height: "100%", objectFit: "cover" }}
            />
          ) : (
            <span style={{ fontSize: "20px" }}>
              {member.membershipName?.charAt(0)?.toUpperCase() || "?"}
            </span>
          )}
        </Box>
        <Box style={{ flex: 1, minWidth: 0 }}>
          <Box
            style={{
              fontWeight: "600",
              color: "#111827",
              fontSize: "16px",
              marginBottom: "4px",
              overflow: "hidden",
              textOverflow: "ellipsis",
              whiteSpace: "nowrap",
            }}
          >
            {member.membershipName}
          </Box>
          <Box
            style={{ fontSize: "14px", color: "#6b7280", marginBottom: "2px" }}
          >
            {member.phoneNumber}
          </Box>
          {member.groupPosition && (
            <Box
              style={{ fontSize: "13px", color: "#9ca3af", marginTop: "2px" }}
            >
              {member.groupPosition}
            </Box>
          )}
        </Box>
      </Box>
    </Box>
  );
};

// Tab Navigation Component
const TabNavigation: React.FC<{
  activeTab: TabType;
  onTabClick: (tab: TabType) => void;
  isJoined: boolean;
}> = ({ activeTab, onTabClick, isJoined }) => {
  const tabs: { key: TabType; label: string }[] = [
    { key: "info", label: "Thông tin" },
    { key: "events", label: "Sự kiện" },
    { key: "members", label: "Thành viên" },
  ];

  return (
    <Box
      style={{
        position: "sticky",
        top: 0,
        zIndex: 100,
        backgroundColor: "#fff",
        borderBottom: "1px solid #e5e7eb",
        boxShadow: "0 2px 4px rgba(0, 0, 0, 0.05)",
      }}
    >
      <Box
        style={{
          display: "flex",
          alignItems: "center",
          width: "100%",
        }}
      >
        {tabs.map((tab) => {
          const isDisabled = !isJoined && tab.key === "members";
          return (
            <Box
              key={tab.key}
              onClick={() => !isDisabled && onTabClick(tab.key)}
              style={{
                flex: 1,
                textAlign: "center",
                padding: "12px 8px",
                position: "relative",
                transition: "all 0.3s",
                color: isDisabled
                  ? "#d1d5db"
                  : activeTab === tab.key
                  ? "#000"
                  : "#6b7280",
                fontWeight: activeTab === tab.key ? "600" : "400",
                fontSize: "14px",
                cursor: isDisabled ? "not-allowed" : "pointer",
                opacity: isDisabled ? 0.5 : 1,
              }}
            >
              {tab.label}
              {activeTab === tab.key && !isDisabled && (
                <Box
                  style={{
                    position: "absolute",
                    bottom: 0,
                    left: "50%",
                    transform: "translateX(-50%)",
                    width: "60%",
                    height: "3px",
                    backgroundColor: "#000",
                    borderRadius: "3px 3px 0 0",
                  }}
                />
              )}
            </Box>
          );
        })}
      </Box>
    </Box>
  );
};

// Info Tab Component
const InfoTab: React.FC<{
  groupInfo: GroupInfo;
  events: Event[];
  groupRulesTotalPages: number;
  onNavigateToRules: () => void;
}> = ({ groupInfo, events, groupRulesTotalPages, onNavigateToRules }) => {
  const sectionTitleStyle: React.CSSProperties = {
    fontSize: "16px",
    fontWeight: "600",
    color: "#111827",
    marginBottom: "12px",
    margin: 0,
  };

  const sectionTextStyle: React.CSSProperties = {
    color: "#6b7280",
    lineHeight: "1.6",
    margin: 0,
    fontSize: "14px",
  };

  return (
    <Box>
      {/* Description */}
      <Box style={{ marginBottom: "24px" }}>
        <h3 style={sectionTitleStyle}>Giới thiệu</h3>
        <p style={sectionTextStyle}>
          {groupInfo.description || "Chưa cập nhật"}
        </p>
      </Box>

      {/* Main Activities */}
      {groupInfo.mainActivities && (
        <Box style={{ marginBottom: "24px" }}>
          <h3 style={sectionTitleStyle}>Các hoạt động chính</h3>
          <div
            style={sectionTextStyle}
            dangerouslySetInnerHTML={{ __html: groupInfo.mainActivities }}
          />
        </Box>
      )}

      {groupRulesTotalPages > 0 && (
        <Box style={{ marginBottom: "24px" }}>
          <h3 style={sectionTitleStyle}>Quy định nhóm</h3>
          <Box
            onClick={onNavigateToRules}
            style={{
              background: "#fff",
              border: "1px solid #e5e7eb",
              borderRadius: "12px",
              padding: "16px",
              display: "flex",
              alignItems: "center",
              justifyContent: "space-between",
              transition: "all 0.2s",
              marginTop: "12px",
              cursor: "pointer",
            }}
          >
            <Box style={{ display: "flex", alignItems: "center", gap: "12px" }}>
              <Box
                style={{
                  width: "40px",
                  height: "40px",
                  background: "#000",
                  borderRadius: "8px",
                  display: "flex",
                  alignItems: "center",
                  justifyContent: "center",
                }}
              >
                <svg
                  width="20"
                  height="20"
                  viewBox="0 0 24 24"
                  fill="none"
                  stroke="#fff"
                  strokeWidth="2"
                >
                  <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z" />
                  <polyline points="14 2 14 8 20 8" />
                  <line x1="16" y1="13" x2="8" y2="13" />
                  <line x1="16" y1="17" x2="8" y2="17" />
                  <polyline points="10 9 9 9 8 9" />
                </svg>
              </Box>
              <Box>
                <div
                  style={{
                    fontSize: "14px",
                    fontWeight: "600",
                    color: "#111827",
                    marginBottom: "2px",
                  }}
                >
                  Quy tắc ứng xử
                </div>
                <div style={{ fontSize: "12px", color: "#6b7280" }}>
                  {groupRulesTotalPages} nguyên tắc cơ bản
                </div>
              </Box>
            </Box>
            <svg
              width="20"
              height="20"
              viewBox="0 0 24 24"
              fill="none"
              stroke="#9ca3af"
              strokeWidth="2"
            >
              <polyline points="9 18 15 12 9 6" />
            </svg>
          </Box>
        </Box>
      )}

      {/* Info Grid */}
      <Box>
        <h3 style={sectionTitleStyle}>Thông tin</h3>
        <Box
          style={{
            display: "grid",
            gridTemplateColumns: "1fr 1fr",
            gap: "12px",
          }}
        >
          <Box>
            <div
              style={{
                fontSize: "12px",
                color: "#9ca3af",
                marginBottom: "4px",
              }}
            >
              Thành viên
            </div>
            <div
              style={{ fontSize: "16px", fontWeight: "600", color: "#111827" }}
            >
              {groupInfo.memberCount.toLocaleString()}
            </div>
          </Box>
          <Box>
            <div
              style={{
                fontSize: "12px",
                color: "#9ca3af",
                marginBottom: "4px",
              }}
            >
              Sự kiện
            </div>
            <div
              style={{ fontSize: "16px", fontWeight: "600", color: "#111827" }}
            >
              {events.length}
            </div>
          </Box>
          <Box>
            <div
              style={{
                fontSize: "12px",
                color: "#9ca3af",
                marginBottom: "4px",
              }}
            >
              Ngày tạo
            </div>
            <div style={{ fontSize: "14px", color: "#6b7280" }}>
              {new Date(groupInfo.createdDate).toLocaleDateString("vi-VN")}
            </div>
          </Box>
          <Box>
            <div
              style={{
                fontSize: "12px",
                color: "#9ca3af",
                marginBottom: "4px",
              }}
            >
              Trạng thái
            </div>
            <div
              style={{
                fontSize: "14px",
                color: groupInfo.isActive ? "#10b981" : "#6b7280",
                fontWeight: "500",
              }}
            >
              {groupInfo.isActive ? "Hoạt động" : "Tạm dừng"}
            </div>
          </Box>
        </Box>
      </Box>
    </Box>
  );
};

// Events Tab Component
const EventsTab: React.FC<{ events: Event[]; isJoined: boolean }> = ({
  events,
  isJoined,
}) => {
  if (events.length === 0) {
    return (
      <Box style={{ textAlign: "center", padding: "32px 0", color: "#6b7280" }}>
        Chưa có sự kiện nào
      </Box>
    );
  }

  return (
    <Box
      style={{
        display: "grid",
        gridTemplateColumns: "repeat(auto-fit, minmax(300px, 1fr))",
        gap: "16px",
      }}
    >
      {events.map((event) => (
        <EventItem key={event.id} item={event} disableActions={!isJoined} />
      ))}
    </Box>
  );
};

// Members Tab Component
const MembersTab: React.FC<{
  members: Member[];
  isJoined: boolean;
  onCreateRefClick: () => void;
  onNavigateToRefList: () => void;
  onMemberClick: (member: Member) => void;
}> = ({
  members,
  isJoined,
  onCreateRefClick,
  onNavigateToRefList,
  onMemberClick,
}) => {
  // Sort members by sortOrder (ascending), then by name if sortOrder is the same
  const sortedMembers = [...members].sort((a, b) => {
    const orderA = a.sortOrder ?? 999;
    const orderB = b.sortOrder ?? 999;
    if (orderA !== orderB) {
      return orderA - orderB;
    }
    return (a.membershipName || "").localeCompare(b.membershipName || "");
  });

  if (sortedMembers.length === 0) {
    return (
      <Box style={{ textAlign: "center", padding: "32px 0", color: "#6b7280" }}>
        Chưa có thành viên nào
      </Box>
    );
  }

  return (
    <Box>
      <Box
        style={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
          marginBottom: "16px",
        }}
      >
        <Box style={{ fontSize: "18px", fontWeight: "600", color: "#111827" }}>
          Thành viên ({sortedMembers.length})
        </Box>
        {isJoined && sortedMembers.length > 0 && (
          <div style={{ display: "flex", gap: "12px" }}>
            <Box
              onClick={onCreateRefClick}
              style={{
                background: "#eab308",
                color: "black",
                padding: "8px 16px",
                borderRadius: "8px",
                fontSize: "14px",
                fontWeight: "500",
                cursor: "pointer",
                transition: "all 0.3s",
              }}
            >
              Tạo Referral
            </Box>
            <Box
              onClick={onNavigateToRefList}
              style={{
                padding: "8px 16px",
                borderRadius: "8px",
                fontSize: "14px",
                fontWeight: "500",
                cursor: "pointer",
                transition: "all 0.3s",
                border: "1px solid #000",
              }}
            >
              Lịch sử Ref
            </Box>
          </div>
        )}
      </Box>

      <Box
        style={{
          display: "grid",
          gridTemplateColumns: "repeat(auto-fit, minmax(280px, 1fr))",
          gap: "16px",
        }}
      >
        {sortedMembers.map((member) => (
          <MemberCard
            key={member.membershipId}
            member={member}
            onClick={() => onMemberClick(member)}
          />
        ))}
      </Box>
    </Box>
  );
};

// Bottom Action Buttons Component
const BottomActionButtons: React.FC<{
  onShareClick: () => void;
  onJoinClick: () => void;
  showJoinButton: boolean;
}> = ({ onShareClick, onJoinClick, showJoinButton }) => {
  return (
    <Box
      style={{
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        gap: "12px",
        marginTop: "32px",
        position: "fixed",
        bottom: "0",
        left: "0",
        right: "0",
        padding: "16px",
        background: "#fff",
        borderTop: "1px solid #e2e8f0",
      }}
    >
      <Box
        onClick={onShareClick}
        style={{
          width: "50%",
          background: "white",
          border: "1px solid #000",
          color: "#000",
          padding: "10px 14px",
          borderRadius: "8px",
          textAlign: "center",
          fontWeight: "500",
          cursor: "pointer",
          transition: "all 0.3s",
        }}
      >
        Chia sẻ nhóm
      </Box>
      {showJoinButton && (
        <Box
          onClick={onJoinClick}
          style={{
            flex: 1,
            background: "#eab308",
            color: "black",
            padding: "10px 14px",
            borderRadius: "8px",
            textAlign: "center",
            fontWeight: "500",
            cursor: "pointer",
            transition: "all 0.3s",
            border: "1px solid #eab308",
          }}
        >
          Tham gia nhóm
        </Box>
      )}
    </Box>
  );
};

const GroupDetailGiba: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const setHeader = useSetHeader();
  const userToken = useRecoilValue(token);
  const userProfile = useRecoilValue(infoShare);
  const membershipInfo = useRecoilValue(userMembershipInfo);
  const snackbar = useSnackbar();
  const { id: groupId } = useParams<{ id: string }>();

  const [groupInfo, setGroupInfo] = useState<GroupInfo | null>(null);
  const [events, setEvents] = useState<Event[]>([]);
  const [members, setMembers] = useState<Member[]>([]);
  const [loading, setLoading] = useState(true);
  const [isJoined, setIsJoined] = useState(false);
  const [registerDrawerVisible, setRegisterDrawerVisible] = useState(false);
  const [refOptionModalVisible, setRefOptionModalVisible] = useState(false);
  const [activeTab, setActiveTab] = useState<TabType>("info");
  const shareModalRef = useRef<any>(null);
  const [groupRulesTotalPages, setGroupRulesTotalPages] = useState(0);
  const [headerTitle, setHeaderTitle] = useState("CHI TIẾT NHÓM");
  const [headerLogo, setHeaderLogo] = useState<string | null>(null);

  // Helper function to show snackbar
  const showSnackbar = useCallback(
    (text: string, type: "info" | "warning" | "error" = "info") => {
      snackbar.openSnackbar({
        text,
        type,
        duration: SNACKBAR_DURATION,
        action: SNACKBAR_ACTION,
      });
    },
    [snackbar]
  );

  // Check membership status
  const checkMembershipStatus = useCallback((): boolean => {
    if (
      membershipInfo.approvalStatus === null ||
      membershipInfo.approvalStatus === 0
    ) {
      showSnackbar(
        "Tài khoản đang trong thời gian xét duyệt. Vui lòng chờ admin phê duyệt.",
        "warning"
      );
      return false;
    }
    if (membershipInfo.approvalStatus === 2) {
      showSnackbar(
        "Tài khoản của bạn đã bị từ chối. Vui lòng liên hệ admin.",
        "error"
      );
      return false;
    }
    return true;
  }, [membershipInfo, showSnackbar]);

  // Fetch group info
  const fetchGroupInfo = useCallback(async () => {
    if (!groupId) {
      console.error("No groupId found in URL");
      return;
    }

    setLoading(true);
    
    let shouldTryPublicAPI = false;

    // Try member API first
    try {
      const memberResponse = await axios.get(
        `${dfData.domain}/api/groups/${groupId}/member`,
        {
          headers: { Authorization: `Bearer ${userToken}` },
        }
      );

      // Nếu member API thành công, dùng data từ đó
      if (memberResponse.data.code === 0 && memberResponse.data.data) {
        const data = memberResponse.data.data;
        setGroupInfo(data);
        setEvents(data.allEvents || []);
        setMembers(data.members || []);
        setIsJoined(true);
        setLoading(false);
        return;
      } else {
        // Response code !== 0, cần thử public API
        shouldTryPublicAPI = true;
      }
    } catch (error) {
      // Member API lỗi (404, network error, etc.), cần thử public API
      console.log("Member API failed, will try public API...", error);
      shouldTryPublicAPI = true;
    }

    // Nếu member API không thành công, fallback về public API
    if (shouldTryPublicAPI) {
      try {
        console.log("Calling public API...");
        const publicResponse = await axios.get(
          `${dfData.domain}/api/groups/${groupId}/public`,
          {
            headers: { Authorization: `Bearer ${userToken}` },
          }
        );
        console.log("Public API response:", publicResponse.data);

        if (publicResponse.data.code === 0 && publicResponse.data.data) {
          const groupData = publicResponse.data.data;
          setGroupInfo(groupData);
          setEvents(groupData.publicEvents || []);
          setMembers(groupData.members || []);
          setIsJoined(isPendingOrJoined(groupData));
        } else {
          console.error("Failed to fetch group info from public API");
        }
      } catch (error) {
        console.error("Error fetching public group info:", error);
      }
    }

    setLoading(false);
  }, [groupId, userToken]);

  const fetchGroupRulesCount = useCallback(async () => {
    if (!groupId) return;

    try {
      const response = await axios.get(
        `${dfData.domain}/api/BehaviorRuleV2/pageInfo`,
        {
          params: { type: "GROUP", groupid: groupId, page: 1 },
          headers: { Authorization: `Bearer ${userToken}` },
        }
      );

      if (response.data.success && response.data.data) {
        setGroupRulesTotalPages(response.data.data.totalPages);
      }
    } catch (error) {
      console.error("Error fetching group rules count:", error);
    }
  }, [groupId, userToken]);

  useEffect(() => {
    const stateGroupName = (location.state as any)?.groupName;
    const stateGroupLogo = (location.state as any)?.groupLogo;

    if (stateGroupName) {
      setHeaderTitle(stateGroupName);
    } else if (groupInfo?.groupName) {
      setHeaderTitle(groupInfo.groupName);
    }

    if (stateGroupLogo) {
      setHeaderLogo(stateGroupLogo);
    } else if (groupInfo?.logo) {
      setHeaderLogo(groupInfo.logo);
    }
  }, [location.state, groupInfo]);

  useEffect(() => {
    setHeader({
      title: headerTitle,
      showUserInfo: false,
      showMenuButton: false,
      showCloseButton: false,
      hasLeftIcon: true,
      groupLogo: headerLogo || undefined,
    } as any);
  }, [setHeader, headerTitle, headerLogo]);

  useEffect(() => {
    if (groupId && location.state) {
      const stateKey = `registerDrawer_state_${groupId}`;
      const savedState = sessionStorage.getItem(stateKey);

      if (savedState) {
        try {
          const parsed = JSON.parse(savedState);
          if (
            parsed.visible &&
            parsed.groupId === groupId &&
            !registerDrawerVisible
          ) {
            setRegisterDrawerVisible(true);
            sessionStorage.removeItem(stateKey);
          }
        } catch (error) {
          console.error("Error parsing saved drawer state:", error);
        }
      }
    }
  }, [
    groupId,
    location.pathname,
    location.key,
    location.state,
    registerDrawerVisible,
  ]);

  useEffect(() => {
    fetchGroupInfo();
  }, [fetchGroupInfo]);

  useEffect(() => {
    fetchGroupRulesCount();
  }, [fetchGroupRulesCount]);

  const handleJoinClick = useCallback(() => {
    if (!checkMembershipStatus()) return;

    if (groupInfo?.joinStatus === "pending") {
      showSnackbar(
        "Bạn đã gửi yêu cầu tham gia nhóm này. Vui lòng chờ admin phê duyệt.",
        "info"
      );
      return;
    }

    if (groupInfo && groupId) {
      setRegisterDrawerVisible(true);
    }
  }, [checkMembershipStatus, groupInfo, groupId, showSnackbar]);

  const handleTabClick = useCallback(
    (tab: TabType) => {
      if (tab === "members") {
        if (!isJoined) {
          if (groupInfo?.joinStatus === "pending") {
            showSnackbar(
              "Bạn đang chờ phê duyệt tham gia nhóm này. Vui lòng chờ admin phê duyệt để xem nội dung này.",
              "warning"
            );
          } else {
            showSnackbar(
              "Bạn chưa tham gia nhóm này. Vui lòng tham gia nhóm để xem nội dung này.",
              "info"
            );
          }
          return;
        }
      }

      setActiveTab(tab);
    },
    [isJoined, groupInfo, showSnackbar]
  );

  const handleShareClick = useCallback(() => {
    if (!groupInfo) return;

    const env = APP_MODE;
    let shareUrl = `https://zalo.me/s/${dfData.appid}/giba/group-detail/${groupInfo.id}`;

    if (env === "TESTING" || env === "TESTING_LOCAL" || env === "DEVELOPMENT") {
      shareUrl += `?env=${env}&version=${
        (window as any).APP_VERSION || "1.0.0"
      }&default=true`;
    }

    const shareTitle = `Tham gia nhóm: ${groupInfo.groupName}`;
    const shareDescription = `Hãy tham gia nhóm "${groupInfo.groupName}" để kết nối và học hỏi cùng nhau!`;

    shareModalRef.current?.open(shareUrl, shareTitle, shareDescription);
  }, [groupInfo]);

  const handleRegisterSuccess = useCallback(async () => {
    if (!groupId) return;

    try {
      setLoading(true);
      const memberResponse = await axios.get(
        `${dfData.domain}/api/groups/${groupId}/member`,
        {
          headers: { Authorization: `Bearer ${userToken}` },
        }
      );

      if (memberResponse.data.code === 0 && memberResponse.data.data) {
        const data = memberResponse.data.data;
        setGroupInfo(data);
        setEvents(data.allEvents || []);
        setMembers(data.members || []);
        setIsJoined(true);
      }
    } catch (error) {
      console.error("Error refreshing group info:", error);
    } finally {
      setLoading(false);
    }
  }, [groupId, userToken]);

  const handleNavigateToRules = useCallback(() => {
    navigate("/giba/behavior-rules", { state: { groupId } });
  }, [navigate, groupId]);

  const handleMemberClick = useCallback(
    (member: Member) => {
      if (member.userZaloId) {
        const identifier = member.slug || member.userZaloId;
        navigate(`/giba/member-detail/${identifier}?groupId=${groupId}`);
      }
    },
    [navigate, groupId]
  );

  const showJoinButton = useMemo(
    () => !isJoined && shouldShowJoinButton(groupInfo),
    [isJoined, groupInfo]
  );

  if (loading) {
    return (
      <Page className="bg-white min-h-screen mt-[50px]">
        <div className="flex justify-center items-center h-64">
          <LoadingGiba size="lg" text="Đang tải thông tin nhóm..." />
        </div>
      </Page>
    );
  }

  if (!groupInfo) {
    return (
      <Page className="bg-white min-h-screen mt-[50px]">
        <div className="text-center py-12">
          <div className="text-gray-500 text-lg mb-2">
            Không tìm thấy thông tin nhóm
          </div>
        </div>
      </Page>
    );
  }

  return (
    <Page style={{ paddingTop: 50, overflowY: "auto", height: "100vh" }}>
      <Box style={{ position: "relative" }}>
        <TabNavigation
          activeTab={activeTab}
          onTabClick={handleTabClick}
          isJoined={isJoined}
        />

        <Box
          style={{
            padding: "16px",
            background: "#fff",
            minHeight: "80vh",
            marginBottom: "75px",
          }}
        >
          {activeTab === "info" && (
            <InfoTab
              groupInfo={groupInfo}
              events={events}
              groupRulesTotalPages={groupRulesTotalPages}
              onNavigateToRules={handleNavigateToRules}
            />
          )}

          {activeTab === "events" && (
            <EventsTab events={events} isJoined={isJoined} />
          )}

          {activeTab === "members" && (
            <MembersTab
              members={members}
              isJoined={isJoined}
              onCreateRefClick={() => setRefOptionModalVisible(true)}
              onNavigateToRefList={() => navigate("/giba/ref-list")}
              onMemberClick={handleMemberClick}
            />
          )}

          <BottomActionButtons
            onShareClick={handleShareClick}
            onJoinClick={handleJoinClick}
            showJoinButton={showJoinButton}
          />
        </Box>
      </Box>

      {/* Register Drawer */}
      {groupInfo && groupId && (
        <RegisterDrawer
          visible={registerDrawerVisible}
          onClose={() => setRegisterDrawerVisible(false)}
          groupId={groupId}
          groupName={groupInfo.groupName}
          onSuccess={handleRegisterSuccess}
        />
      )}

      {/* Share Modal */}
      <CommonShareModal ref={shareModalRef} />

      {/* Ref Option Modal */}
      <RefOptionModal
        visible={refOptionModalVisible}
        onClose={() => setRefOptionModalVisible(false)}
        onSelectOption={(option) => {
          setRefOptionModalVisible(false);
          navigate("/giba/ref-create", {
            state: {
              optionType: option,
              groupId: groupInfo.id,
            },
          });
        }}
      />
    </Page>
  );
};

export default GroupDetailGiba;
