import React, { useState, useCallback, useEffect, useRef } from "react";
import { Page } from "zmp-ui";
import { useNavigate, useLocation } from "react-router-dom";
import { useRecoilValue } from "recoil";
import { token } from "../../recoil/RecoilState";
import { Select, Modal } from "antd";
import useSetHeader from "../../components/hooks/useSetHeader";
import LoadingGiba from "../../componentsGiba/LoadingGiba";
import { Plus, Calendar, Search } from "lucide-react";
import FloatingActionButtonGiba from "../../componentsGiba/FloatingActionButtonGiba";
import TwoTierTab from "../../components/TwoTierTab";
import {
  MeetingTabsData,
  MeetingGroupType,
  MeetingStatus,
} from "../../utils/enum/meeting.enum";
import { useHasRole } from "../../hooks/useHasRole";
import axios from "axios";
import dfData from "../../common/DefaultConfig.json";
import { toast } from "react-toastify";
import MeetingItem from "./MeetingItem";

interface GroupItem {
  id: string;
  groupName: string;
  description: string;
  memberCount: number;
  joinStatus: string | null;
}

const MeetingList: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const setHeader = useSetHeader();
  const hasRole = useHasRole();
  const userToken = useRecoilValue(token);
  const [loading, setLoading] = useState(false);
  const [activeGroup, setActiveGroup] = useState<string>(MeetingGroupType.ALL);
  const [activeStatus, setActiveStatus] = useState<string>(MeetingStatus.ALL);
  const [searchKeyword, setSearchKeyword] = useState<string>("");
  const [inputSearchValue, setInputSearchValue] = useState<string>("");
  const [meetings, setMeetings] = useState<any[]>([]);
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(0);
  const [totalItems, setTotalItems] = useState(0);
  const [groups, setGroups] = useState<GroupItem[]>([]);
  const [selectedGroupId, setSelectedGroupId] = useState<string>("");
  const [loadingGroups, setLoadingGroups] = useState(false);
  const [meetingType, setMeetingType] = useState<number | undefined>(undefined);
  const [isPublic, setIsPublic] = useState<boolean | undefined>(undefined);
  const searchTimeoutRef = useRef<any>(null);

  const isAdminMode = location.state?.isAdminMode || false;

  useEffect(() => {
    setHeader({
      title: "LỊCH HỌP",
      showUserInfo: false,
      showMenuButton: false,
      showCloseButton: false,
      hasLeftIcon: true,
    });
  }, [setHeader]);

  useEffect(() => {
    const fetchGroups = async () => {
      if (!userToken) return;

      try {
        setLoadingGroups(true);
        const params = new URLSearchParams();
        params.append("joinstatus", "approved");

        if (activeGroup !== MeetingGroupType.ALL) {
          params.append("grouptype", activeGroup);
        }

        const response = await axios.get(
          `${dfData.domain}/api/Groups/all?${params.toString()}`,
          {
            headers: {
              Authorization: `Bearer ${userToken}`,
            },
          }
        );

        if (response.data.code === 0 && response.data.data?.items) {
          setGroups(response.data.data.items);
        }
      } catch (error) {
        console.error("Error fetching groups:", error);
        toast.error("Không thể tải danh sách Club");
      } finally {
        setLoadingGroups(false);
      }
    };

    fetchGroups();
  }, [userToken, activeGroup]);

  useEffect(() => {
    const fetchMeetings = async () => {
      try {
        setLoading(true);

        const params = new URLSearchParams();
        params.append("Page", currentPage.toString());
        params.append("PageSize", "10");

        if (searchKeyword.trim()) {
          params.append("Keyword", searchKeyword.trim());
        }

        if (activeGroup !== MeetingGroupType.ALL) {
          params.append("type", activeGroup);
        }

        if (activeStatus !== MeetingStatus.ALL) {
          params.append("status", activeStatus);
        }

        if (selectedGroupId) {
          params.append("GroupId", selectedGroupId);
        }

        if (meetingType !== undefined) {
          params.append("MeetingType", meetingType.toString());
        }

        if (isPublic !== undefined) {
          params.append("IsPublic", isPublic.toString());
        }

        const response = await axios.get(
          `${dfData.domain}/api/Meeting/GetPage?${params.toString()}`,
          {
            headers: {
              Authorization: `Bearer ${userToken}`,
            },
          }
        );

        if (response.data.code === 0 && response.data.items) {
          setMeetings(response.data.items);
          setTotalPages(response.data.totalPages || 0);
          setTotalItems(response.data.totalItems || 0);
        } else {
          setMeetings([]);
          setTotalPages(0);
          setTotalItems(0);
        }
      } catch (error) {
        console.error("Error fetching meetings:", error);
        toast.error("Không thể tải danh sách lịch họp");
        setMeetings([]);
        setTotalPages(0);
        setTotalItems(0);
      } finally {
        setLoading(false);
      }
    };

    if (userToken) {
      fetchMeetings();
    }
  }, [
    activeGroup,
    activeStatus,
    searchKeyword,
    currentPage,
    userToken,
    selectedGroupId,
    meetingType,
    isPublic,
  ]);

  const handleViewDetail = useCallback(
    (meeting: any) => {
      navigate("/giba/meeting-detail", {
        state: { meeting, isAdminMode },
      });
    },
    [navigate, isAdminMode]
  );

  const handleCreateMeeting = useCallback(() => {
    navigate("/giba/meeting-create");
  }, [navigate]);

  const handleGroupChange = useCallback((groupValue: string) => {
    setActiveGroup(groupValue);
    setActiveStatus(MeetingStatus.ALL);
    setSelectedGroupId("");
    setInputSearchValue("");
    setSearchKeyword("");
    if (searchTimeoutRef.current) {
      clearTimeout(searchTimeoutRef.current);
    }
  }, []);

  const handleStatusChange = useCallback((statusValue: string) => {
    setActiveStatus(statusValue);
  }, []);

  const handleSearchChange = useCallback(
    (e: React.ChangeEvent<HTMLInputElement>) => {
      const value = e.target.value;
      setInputSearchValue(value);

      if (searchTimeoutRef.current) {
        clearTimeout(searchTimeoutRef.current);
      }

      searchTimeoutRef.current = setTimeout(() => {
        setSearchKeyword(value);
      }, 200);
    },
    []
  );

  const handleGroupSelect = useCallback((value: string | undefined) => {
    setSelectedGroupId(value || "");
  }, []);

  useEffect(() => {
    return () => {
      if (searchTimeoutRef.current) {
        clearTimeout(searchTimeoutRef.current);
      }
    };
  }, []);

  const handleDelete = (meeting: any) => {
    Modal.confirm({
      title: "Xác nhận xóa",
      content: `Bạn có chắc chắn muốn xóa lịch họp "${meeting.title}"?`,
      okText: "Xóa",
      cancelText: "Hủy",
      okButtonProps: { danger: true },
      onOk: async () => {
        try {
          const response = await axios.delete(
            `${dfData.domain}/api/Meeting/${meeting.id}`,
            {
              headers: {
                Authorization: `Bearer ${userToken}`,
              },
            }
          );

          if (response.data.code === 0) {
            toast.success(response.data.message || "Xóa lịch họp thành công!");
            setCurrentPage(1);
            setMeetings([]);
          } else {
            toast.error(response.data.message || "Xóa lịch họp thất bại");
          }
        } catch (error: any) {
          console.error("Error deleting meeting:", error);
          toast.error(
            error.response?.data?.message || "Có lỗi xảy ra khi xóa lịch họp"
          );
        }
      },
    });
  };

  const handleEdit = (meeting: any) => {
    navigate("/giba/meeting-create", {
      state: { meeting },
      replace: true,
    });
  };

  return (
    <Page style={{ marginTop: "50px", background: "#f5f5f5" }}>
      <TwoTierTab
        tabs={MeetingTabsData}
        activeTab={activeGroup}
        onTabChange={handleGroupChange}
        activeChildTab={activeStatus}
        onChildTabChange={handleStatusChange}
      />

      <div className="bg-white p-3 border-b border-gray-200">
        <div className="flex gap-2 mb-2">
          <div className="relative flex-1">
            <Search
              size={18}
              className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400"
            />
            <input
              type="text"
              placeholder="Tìm kiếm chủ đề..."
              value={inputSearchValue}
              onChange={handleSearchChange}
              className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent text-sm"
              style={{ height: "38px" }}
            />
          </div>

          <div className="w-48">
            <Select
              placeholder="Nhóm..."
              value={selectedGroupId || undefined}
              onChange={handleGroupSelect}
              style={{ width: "100%", height: "38px" }}
              loading={loadingGroups}
              allowClear
            >
              {groups.map((group) => (
                <Select.Option key={group.id} value={group.id}>
                  <div className="flex items-center justify-between">
                    <span className="font-medium">{group.groupName}</span>
                    <span className="text-xs text-gray-500">
                      {group.memberCount} thành viên
                    </span>
                  </div>
                </Select.Option>
              ))}
            </Select>
          </div>
        </div>

        <div className="flex gap-2">
          <Select
            placeholder="Hình thức"
            value={meetingType}
            onChange={(value) => setMeetingType(value)}
            style={{ flex: 1, height: "38px" }}
            allowClear
          >
            <Select.Option value={1}>Online</Select.Option>
            <Select.Option value={2}>Offline</Select.Option>
          </Select>

          <Select
            placeholder="Phạm vi"
            value={isPublic}
            onChange={(value) => setIsPublic(value)}
            style={{ flex: 1, height: "38px" }}
            allowClear
          >
            <Select.Option value={true}>Công khai</Select.Option>
            <Select.Option value={false}>Nội bộ</Select.Option>
          </Select>
        </div>
      </div>

      <div className="p-4 space-y-3">
        {loading ? (
          <div className="flex justify-center items-center py-16">
            <LoadingGiba size="lg" text="Đang tải danh sách lịch họp..." />
          </div>
        ) : meetings.length > 0 ? (
          <div className="space-y-3">
            {meetings.map((meeting) => (
              <MeetingItem
                key={meeting.id}
                meeting={meeting}
                onViewDetail={handleViewDetail}
                onEdit={handleEdit}
                onDelete={handleDelete}
                isAdminMode={isAdminMode}
              />
            ))}
          </div>
        ) : (
          <div className="text-center py-16">
            <div className="w-20 h-20 bg-gradient-to-br from-gray-100 to-gray-200 rounded-full flex items-center justify-center mx-auto mb-4">
              <Calendar className="w-8 h-8 text-gray-400" />
            </div>
            <div className="text-gray-800 text-lg font-bold mb-2">
              Chưa có lịch họp nào
            </div>
          </div>
        )}
      </div>

      {hasRole && (
        <FloatingActionButtonGiba
          icon={<Plus />}
          onClick={handleCreateMeeting}
          position="bottom-right"
          color="yellow"
          size="md"
          tooltip="Tạo lịch họp mới"
        />
      )}
    </Page>
  );
};

export default MeetingList;
