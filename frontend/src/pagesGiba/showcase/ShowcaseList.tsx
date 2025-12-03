import React, { useState, useCallback, useEffect, useRef } from "react";
import { Page, Box } from "zmp-ui";
import { useNavigate } from "react-router-dom";
import { useLocation } from "react-router-dom";
import { useRecoilValue } from "recoil";
import { token } from "../../recoil/RecoilState";
import { Select, Modal } from "antd";
import useSetHeader from "../../components/hooks/useSetHeader";
import LoadingGiba from "../../componentsGiba/LoadingGiba";
import { Calendar, Plus, Search } from "lucide-react";
import FloatingActionButtonGiba from "../../componentsGiba/FloatingActionButtonGiba";
import TwoTierTab from "../../components/TwoTierTab";
import {
  ShowcaseTabsData,
  ShowcaseGroupType,
  ShowcaseStatus,
} from "../../utils/enum/showcase.enum";
import { useHasRole } from "../../hooks/useHasRole";
import axios from "axios";
import dfData from "../../common/DefaultConfig.json";
import { toast } from "react-toastify";
import ShowcaseItem from "./ShowcaseItem";

interface GroupItem {
  id: string;
  groupName: string;
  description: string;
  memberCount: number;
  joinStatus: string | null;
}

const ShowcaseList: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const setHeader = useSetHeader();
  const hasRole = useHasRole();
  const userToken = useRecoilValue(token);
  const [loading, setLoading] = useState(false);
  const [activeGroup, setActiveGroup] = useState<string>(ShowcaseGroupType.ALL);
  const [activeStatus, setActiveStatus] = useState<string>(ShowcaseStatus.ALL);
  const [searchKeyword, setSearchKeyword] = useState<string>("");
  const [inputSearchValue, setInputSearchValue] = useState<string>("");
  const [showcases, setShowcases] = useState<any[]>([]);
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(0);
  const [totalItems, setTotalItems] = useState(0);
  const [groups, setGroups] = useState<GroupItem[]>([]);
  const [selectedGroupId, setSelectedGroupId] = useState<string>("");
  const [loadingGroups, setLoadingGroups] = useState(false);
  const searchTimeoutRef = useRef<any>(null);

  const isAdminMode = location.state?.isAdminMode || false;

  useEffect(() => {
    setHeader({
      title: "LỊCH SHOWCASE",
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

        if (activeGroup !== ShowcaseGroupType.ALL) {
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
    const fetchShowcases = async () => {
      try {
        setLoading(true);

        const params = new URLSearchParams();
        params.append("page", currentPage.toString());
        params.append("pageSize", "10");

        if (searchKeyword.trim()) {
          params.append("keyword", searchKeyword.trim());
        }

        if (activeGroup !== ShowcaseGroupType.ALL) {
          params.append("type", activeGroup);
        }

        if (activeStatus !== ShowcaseStatus.ALL) {
          params.append("status", activeStatus);
        }

        if (selectedGroupId) {
          params.append("groupId", selectedGroupId);
        }

        const response = await axios.get(
          `${dfData.domain}/api/Showcase/GetPage?${params.toString()}`,
          {
            headers: {
              Authorization: `Bearer ${userToken}`,
            },
          }
        );

        if (response.data.code === 0 && response.data.items) {
          setShowcases(response.data.items);
          setTotalPages(response.data.totalPages || 0);
          setTotalItems(response.data.totalItems || 0);
        } else {
          setShowcases([]);
          setTotalPages(0);
          setTotalItems(0);
        }
      } catch (error) {
        console.error("Error fetching showcases:", error);
        toast.error("Không thể tải danh sách showcase");
        setShowcases([]);
        setTotalPages(0);
        setTotalItems(0);
      } finally {
        setLoading(false);
      }
    };

    if (userToken) {
      fetchShowcases();
    }
  }, [
    activeGroup,
    activeStatus,
    searchKeyword,
    currentPage,
    userToken,
    selectedGroupId,
  ]);

  const handleViewDetail = useCallback(
    (showcase: any) => {
      navigate("/giba/showcase-detail", {
        state: { showcase },
      });
    },
    [navigate]
  );

  const handleCreateShowcase = useCallback(() => {
    navigate("/giba/showcase-create");
  }, [navigate]);

  const handleGroupChange = useCallback((groupValue: string) => {
    setActiveGroup(groupValue);
    setActiveStatus(ShowcaseStatus.ALL);
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

  const handleDelete = (showcase: any) => {
    Modal.confirm({
      title: "Xác nhận xóa",
      content: `Bạn có chắc chắn muốn xóa lịch showcase "${showcase.title}"?`,
      okText: "Xóa",
      cancelText: "Hủy",
      okButtonProps: { danger: true },
      onOk: async () => {
        try {
          const response = await axios.delete(
            `${dfData.domain}/api/Showcase/${showcase.id}`,
            {
              headers: {
                Authorization: `Bearer ${userToken}`,
              },
            }
          );

          if (response.data.code === 0) {
            toast.success(
              response.data.message || "Xóa lịch showcase thành công!"
            );
            // Refresh list
            setCurrentPage(1);
            setShowcases([]);
          } else {
            toast.error(response.data.message || "Xóa lịch showcase thất bại");
          }
        } catch (error: any) {
          console.error("Error deleting showcase:", error);
          toast.error(
            error.response?.data?.message ||
              "Có lỗi xảy ra khi xóa lịch showcase"
          );
        }
      },
    });
  };

  const handleEdit = (showcase: any) => {
    navigate("/giba/showcase-create", {
      state: { showcase },
    });
  };

  return (
    <Page style={{ marginTop: "50px", background: "#f5f5f5" }}>
      <TwoTierTab
        tabs={ShowcaseTabsData}
        activeTab={activeGroup}
        onTabChange={handleGroupChange}
        activeChildTab={activeStatus}
        onChildTabChange={handleStatusChange}
      />

      <div className="bg-white p-3 border-b border-gray-200">
        <div className="flex gap-2">
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
      </div>

      <div className="p-4 space-y-3">
        {loading ? (
          <div className="flex justify-center items-center py-16">
            <LoadingGiba size="lg" text="Đang tải danh sách showcase..." />
          </div>
        ) : showcases.length > 0 ? (
          <div className="space-y-3">
            {showcases.map((showcase) => (
              <ShowcaseItem
                key={showcase.id}
                showcase={showcase}
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
              Chưa có lịch showcase nào
            </div>
          </div>
        )}
      </div>

      {hasRole && (
        <FloatingActionButtonGiba
          icon={<Plus />}
          onClick={handleCreateShowcase}
          position="bottom-right"
          color="yellow"
          size="md"
          tooltip="Tạo lịch showcase mới"
        />
      )}
    </Page>
  );
};

export default ShowcaseList;
