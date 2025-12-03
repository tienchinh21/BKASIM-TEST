import React, { useState, useEffect, useRef } from "react";
import { Page } from "zmp-ui";
import { useRecoilValue } from "recoil";
import { token } from "../../recoil/RecoilState";
import { Input, Modal, Table, Button, Avatar, Tag, Space, Select } from "antd";
import type { ColumnsType } from "antd/es/table";
import useSetHeader from "../../components/hooks/useSetHeader";
import LoadingGiba from "../../componentsGiba/LoadingGiba";
import {
  Search,
  Users,
  Eye,
  CheckCircle,
  XCircle,
  ArrowLeft,
  Building2,
} from "lucide-react";
import { toast } from "react-toastify";
import axios from "axios";
import dfData from "../../common/DefaultConfig.json";
import { MemberData } from "../managerMembership/components/MemberCard";
import MemberDetailDrawer from "../managerMembership/components/MemberDetailDrawer";
import RejectModal from "../managerMembership/components/RejectModal";
import TwoTierTab, { TabGroup } from "../../components/TwoTierTab/TwoTierTab";

// Group filter tabs structure
const groupFilterTabs: TabGroup[] = [
  {
    id: "all",
    name: "Tất cả",
    value: "",
    children: [
      { id: "all-all", name: "Tất cả", value: "" },
      { id: "all-active", name: "Đang hoạt động", value: "true" },
      { id: "all-inactive", name: "Ngừng hoạt động", value: "false" },
    ],
  },
  {
    id: "nbd",
    name: "NBD",
    value: "NBD",
    children: [
      { id: "nbd-all", name: "Tất cả", value: "" },
      { id: "nbd-active", name: "Đang hoạt động", value: "true" },
      { id: "nbd-inactive", name: "Ngừng hoạt động", value: "false" },
    ],
  },
  {
    id: "club",
    name: "Club",
    value: "Club",
    children: [
      { id: "club-all", name: "Tất cả", value: "" },
      { id: "club-active", name: "Đang hoạt động", value: "true" },
      { id: "club-inactive", name: "Ngừng hoạt động", value: "false" },
    ],
  },
];

// Member filter tabs structure
const memberFilterTabs: TabGroup[] = [
  {
    id: "member-status",
    name: "Trạng thái",
    value: "status",
    children: [
      { id: "status-all", name: "Tất cả", value: "" },
      { id: "status-pending", name: "Chờ duyệt", value: "0" },
      { id: "status-approved", name: "Đã duyệt", value: "1" },
      { id: "status-rejected", name: "Từ chối", value: "2" },
    ],
  },
];

interface GroupData {
  id: string;
  name: string;
  description: string;
  status: boolean;
  memberCount: number;
  groupType: string;
  logo: string;
  createdDate: string;
  updatedDate: string;
}

const ManagerClub: React.FC = () => {
  const setHeader = useSetHeader();
  const userToken = useRecoilValue(token);

  // View state: 'groups' or 'members'
  const [currentView, setCurrentView] = useState<"groups" | "members">(
    "groups"
  );
  const [selectedGroup, setSelectedGroup] = useState<GroupData | null>(null);

  // Groups state
  const [loadingGroups, setLoadingGroups] = useState(false);
  const [groups, setGroups] = useState<GroupData[]>([]);
  const [currentGroupPage, setCurrentGroupPage] = useState(1);
  const [totalGroupPages, setTotalGroupPages] = useState(0);
  const [totalGroupItems, setTotalGroupItems] = useState(0);
  const [groupSearchKeyword, setGroupSearchKeyword] = useState("");
  const [inputGroupSearchValue, setInputGroupSearchValue] = useState("");
  const [groupStatusFilter, setGroupStatusFilter] = useState("");
  const [groupTypeFilter, setGroupTypeFilter] = useState("");
  const [activeGroupTab, setActiveGroupTab] = useState(""); // "" = Tất cả
  const [activeGroupChildTab, setActiveGroupChildTab] = useState(""); // "" = Tất cả

  // Members state
  const [loadingMembers, setLoadingMembers] = useState(false);
  const [members, setMembers] = useState<MemberData[]>([]);
  const [currentMemberPage, setCurrentMemberPage] = useState(1);
  const [totalMemberPages, setTotalMemberPages] = useState(0);
  const [totalMemberItems, setTotalMemberItems] = useState(0);
  const [memberSearchKeyword, setMemberSearchKeyword] = useState("");
  const [inputMemberSearchValue, setInputMemberSearchValue] = useState("");
  const [memberStatusFilter, setMemberStatusFilter] = useState("");
  const [activeMemberTab, setActiveMemberTab] = useState("status");
  const [activeMemberChildTab, setActiveMemberChildTab] = useState("");
  const [selectedMember, setSelectedMember] = useState<MemberData | null>(null);
  const [detailDrawerVisible, setDetailDrawerVisible] = useState(false);
  const [rejectModalVisible, setRejectModalVisible] = useState(false);
  const [memberToReject, setMemberToReject] = useState<MemberData | null>(null);
  const [actionLoading, setActionLoading] = useState(false);

  const groupSearchTimeoutRef = useRef<any>(null);
  const memberSearchTimeoutRef = useRef<any>(null);
  const pageSize = 10;

  useEffect(() => {
    if (currentView === "groups") {
      setHeader({
        title: "QUẢN LÝ HỘI NHÓM",
        showUserInfo: false,
        showMenuButton: false,
        showCloseButton: false,
        hasLeftIcon: true,
      });
    } else {
      setHeader({
        title: selectedGroup?.name || "QUẢN LÝ THÀNH VIÊN",
        showUserInfo: false,
        showMenuButton: false,
        showCloseButton: false,
        hasLeftIcon: true,
      });
    }
  }, [setHeader, currentView, selectedGroup]);

  // Fetch groups
  useEffect(() => {
    if (currentView === "groups") {
      fetchGroups();
    }
  }, [
    currentGroupPage,
    groupSearchKeyword,
    groupStatusFilter,
    groupTypeFilter,
    currentView,
  ]);

  // Fetch members when group is selected
  useEffect(() => {
    if (currentView === "members" && selectedGroup) {
      fetchMembers();
    }
  }, [
    currentMemberPage,
    memberSearchKeyword,
    memberStatusFilter,
    currentView,
    selectedGroup,
  ]);

  // Group search debounce
  useEffect(() => {
    if (groupSearchTimeoutRef.current) {
      clearTimeout(groupSearchTimeoutRef.current);
    }

    groupSearchTimeoutRef.current = setTimeout(() => {
      setGroupSearchKeyword(inputGroupSearchValue);
      setCurrentGroupPage(1);
    }, 500);

    return () => {
      if (groupSearchTimeoutRef.current) {
        clearTimeout(groupSearchTimeoutRef.current);
      }
    };
  }, [inputGroupSearchValue]);

  // Member search debounce
  useEffect(() => {
    if (memberSearchTimeoutRef.current) {
      clearTimeout(memberSearchTimeoutRef.current);
    }

    memberSearchTimeoutRef.current = setTimeout(() => {
      setMemberSearchKeyword(inputMemberSearchValue);
      setCurrentMemberPage(1);
    }, 500);

    return () => {
      if (memberSearchTimeoutRef.current) {
        clearTimeout(memberSearchTimeoutRef.current);
      }
    };
  }, [inputMemberSearchValue]);

  const fetchGroups = async () => {
    if (!userToken) {
      toast.error("Vui lòng đăng nhập để tiếp tục");
      return;
    }

    try {
      setLoadingGroups(true);

      const params: any = {
        page: currentGroupPage,
        pageSize: pageSize,
      };

      if (groupSearchKeyword) {
        params.keyword = groupSearchKeyword;
      }

      if (groupStatusFilter) {
        params.status = groupStatusFilter === "true";
      }

      if (groupTypeFilter) {
        params.groupType = groupTypeFilter;
      }

      const response = await axios.get(`${dfData.domain}/Groups/GetPage`, {
        params,
        headers: {
          Authorization: `Bearer ${userToken}`,
        },
      });

      // Response format: { data: [...], totalItems: number, page: number, pageSize: number, totalPages: number }
      if (response.data && response.data.data) {
        const newGroups = response.data.data || [];
        if (currentGroupPage === 1) {
          // First page: replace data
          setGroups(newGroups);
        } else {
          // Load more: append data
          setGroups((prevGroups) => [...prevGroups, ...newGroups]);
        }
        setTotalGroupItems(response.data.totalItems || 0);
        setTotalGroupPages(response.data.totalPages || 0);
      } else {
        toast.error("Không thể tải danh sách hội nhóm");
      }
    } catch (error: any) {
      console.error("Error fetching groups:", error);
      toast.error(
        error.response?.data?.message ||
          "Có lỗi xảy ra khi tải danh sách hội nhóm"
      );
    } finally {
      setLoadingGroups(false);
    }
  };

  const fetchMembers = async () => {
    if (!userToken || !selectedGroup) {
      return;
    }

    try {
      setLoadingMembers(true);

      const params: any = {
        groupId: selectedGroup.id,
      };

      const response = await axios.get(
        `${dfData.domain}/Groups/GetGroupMembers`,
        {
          params,
          headers: {
            Authorization: `Bearer ${userToken}`,
          },
        }
      );

      if (response.data.success) {
        let membersData = response.data.data || [];

        // Filter by search keyword
        if (memberSearchKeyword) {
          const keyword = memberSearchKeyword.toLowerCase();
          membersData = membersData.filter(
            (member: any) =>
              member.memberName?.toLowerCase().includes(keyword) ||
              member.phoneNumber?.toLowerCase().includes(keyword) ||
              member.email?.toLowerCase().includes(keyword)
          );
        }

        // Filter by status
        if (memberStatusFilter) {
          const statusNum = parseInt(memberStatusFilter);
          membersData = membersData.filter((member: any) => {
            if (statusNum === 0) {
              // Chờ duyệt: isApproved === null
              return (
                member.isApproved === null || member.isApproved === undefined
              );
            } else if (statusNum === 1) {
              // Đã duyệt: isApproved === true
              return member.isApproved === true;
            } else if (statusNum === 2) {
              // Từ chối: isApproved === false
              return member.isApproved === false;
            }
            return true;
          });
        }

        // Transform data to match MemberData interface
        const transformedMembers: MemberData[] = membersData.map(
          (member: any) => ({
            id: member.id || member.membershipId,
            fullname: member.memberName || "",
            email: member.email || "",
            phone: member.phoneNumber || "",
            company: member.company || "",
            position: member.position || member.groupPosition || "",
            approvalStatus:
              member.isApproved === true
                ? 1
                : member.isApproved === false
                ? 2
                : 0,
            createdDate: member.createdDate || "",
            zaloAvatar: member.zaloAvatar,
            userZaloId: member.userZaloId,
          })
        );

        // Pagination
        const startIndex = (currentMemberPage - 1) * pageSize;
        const endIndex = startIndex + pageSize;
        const paginatedMembers = transformedMembers.slice(startIndex, endIndex);

        setMembers(paginatedMembers);
        setTotalMemberItems(transformedMembers.length);
        setTotalMemberPages(Math.ceil(transformedMembers.length / pageSize));
      } else {
        toast.error(
          response.data.message || "Không thể tải danh sách thành viên"
        );
      }
    } catch (error: any) {
      console.error("Error fetching members:", error);
      toast.error(
        error.response?.data?.message ||
          "Có lỗi xảy ra khi tải danh sách thành viên"
      );
    } finally {
      setLoadingMembers(false);
    }
  };

  const handleGroupTabChange = (tabValue: string) => {
    setActiveGroupTab(tabValue);
    setGroupTypeFilter(tabValue);
    setActiveGroupChildTab("");
    setGroupStatusFilter("");
    setCurrentGroupPage(1);
  };

  const handleGroupChildTabChange = (childValue: string) => {
    setActiveGroupChildTab(childValue);
    setGroupStatusFilter(childValue);
    setCurrentGroupPage(1);
  };

  const handleSelectGroup = (group: GroupData) => {
    setSelectedGroup(group);
    setCurrentView("members");
    setCurrentMemberPage(1);
    setMemberSearchKeyword("");
    setInputMemberSearchValue("");
    setMemberStatusFilter("");
    setActiveMemberTab("status");
    setActiveMemberChildTab("");
  };

  const handleBackToGroups = () => {
    setCurrentView("groups");
    setSelectedGroup(null);
    setMembers([]);
  };

  const handleMemberTabChange = (tabValue: string) => {
    setActiveMemberTab(tabValue);
    setActiveMemberChildTab("");
    setMemberStatusFilter("");
    setCurrentMemberPage(1);
  };

  const handleMemberChildTabChange = (childValue: string) => {
    setActiveMemberChildTab(childValue);
    setMemberStatusFilter(childValue);
    setCurrentMemberPage(1);
  };

  const handleViewDetail = (member: MemberData) => {
    setSelectedMember(member);
    setDetailDrawerVisible(true);
  };

  const handleApprove = async (memberId: string) => {
    Modal.confirm({
      title: "Xác nhận phê duyệt",
      content: "Bạn có chắc chắn muốn phê duyệt thành viên này?",
      okText: "Phê duyệt",
      cancelText: "Hủy",
      onOk: async () => {
        try {
          setActionLoading(true);

          const response = await axios.post(
            `${dfData.domain}/api/MembershipApproval/Approve`,
            { membershipId: memberId },
            {
              headers: {
                Authorization: `Bearer ${userToken}`,
              },
            }
          );

          if (response.data.success) {
            toast.success("Phê duyệt thành viên thành công!");
            fetchMembers();
          } else {
            toast.error(
              response.data.message || "Không thể phê duyệt thành viên"
            );
          }
        } catch (error: any) {
          console.error("Error approving member:", error);
          toast.error(
            error.response?.data?.message ||
              "Có lỗi xảy ra khi phê duyệt thành viên"
          );
        } finally {
          setActionLoading(false);
        }
      },
    });
  };

  const handleReject = (memberId: string) => {
    const member = members.find((m) => m.id === memberId);
    if (member) {
      setMemberToReject(member);
      setRejectModalVisible(true);
    }
  };

  const handleRejectConfirm = async (reason: string) => {
    if (!memberToReject) return;

    try {
      setActionLoading(true);

      const response = await axios.post(
        `${dfData.domain}/api/MembershipApproval/Reject`,
        {
          membershipId: memberToReject.id,
          reason: reason,
        },
        {
          headers: {
            Authorization: `Bearer ${userToken}`,
          },
        }
      );

      if (response.data.success) {
        toast.success("Từ chối thành viên thành công!");
        setRejectModalVisible(false);
        setMemberToReject(null);
        fetchMembers();
      } else {
        toast.error(response.data.message || "Không thể từ chối thành viên");
      }
    } catch (error: any) {
      console.error("Error rejecting member:", error);
      toast.error(
        error.response?.data?.message || "Có lỗi xảy ra khi từ chối thành viên"
      );
    } finally {
      setActionLoading(false);
    }
  };

  const handleRemoveFromGroup = async (membershipGroupId: string) => {
    Modal.confirm({
      title: "Xác nhận xóa thành viên",
      content: "Bạn có chắc chắn muốn xóa thành viên này khỏi nhóm?",
      okText: "Xóa",
      cancelText: "Hủy",
      okButtonProps: { danger: true },
      onOk: async () => {
        try {
          setActionLoading(true);

          const formData = new FormData();
          formData.append("membershipGroupId", membershipGroupId);

          const response = await axios.post(
            `${dfData.domain}/Groups/DeleteMember`,
            formData,
            {
              headers: {
                Authorization: `Bearer ${userToken}`,
                "Content-Type": "multipart/form-data",
              },
            }
          );

          if (response.data.success) {
            toast.success("Xóa thành viên khỏi nhóm thành công!");
            fetchMembers();
          } else {
            toast.error(
              response.data.message || "Không thể xóa thành viên khỏi nhóm"
            );
          }
        } catch (error: any) {
          console.error("Error removing member from group:", error);
          toast.error(
            error.response?.data?.message ||
              "Có lỗi xảy ra khi xóa thành viên khỏi nhóm"
          );
        } finally {
          setActionLoading(false);
        }
      },
    });
  };

  const handleLoadMoreGroups = () => {
    if (currentGroupPage < totalGroupPages) {
      setCurrentGroupPage(currentGroupPage + 1);
    }
  };

  const handleLoadMoreMembers = () => {
    if (currentMemberPage < totalMemberPages) {
      setCurrentMemberPage(currentMemberPage + 1);
    }
  };

  // Groups columns
  const groupColumns: ColumnsType<GroupData> = [
    {
      title: "STT",
      key: "index",
      width: 60,
      align: "center",
      render: (_: any, __: GroupData, index: number) =>
        (currentGroupPage - 1) * pageSize + index + 1,
    },
    {
      title: "Tên nhóm",
      dataIndex: "name",
      key: "name",
      width: 200,
      render: (text: string, record: GroupData) => (
        <Space>
          {record.logo && <Avatar src={record.logo} size="small" />}
          <span className="font-medium">{text}</span>
        </Space>
      ),
    },
    {
      title: "Loại",
      dataIndex: "groupType",
      key: "groupType",
      width: 100,
      align: "center",
      render: (text: string) => (
        <Tag color={text === "NBD" ? "blue" : "purple"}>{text || "-"}</Tag>
      ),
    },
    {
      title: "Số thành viên",
      dataIndex: "memberCount",
      key: "memberCount",
      width: 120,
      align: "center",
      render: (count: number) => (
        <span className="font-medium">{count || 0}</span>
      ),
    },
    {
      title: "Trạng thái",
      dataIndex: "status",
      key: "status",
      width: 120,
      align: "center",
      render: (status: boolean) => (
        <Tag color={status ? "success" : "error"}>
          {status ? "Đang hoạt động" : "Ngừng hoạt động"}
        </Tag>
      ),
    },
    {
      title: "Ngày tạo",
      dataIndex: "createdDate",
      key: "createdDate",
      width: 120,
      render: (text: string) => text || "-",
    },
    {
      title: "Thao tác",
      key: "action",
      width: 100,
      align: "center",
      fixed: "right",
      render: (_: any, record: GroupData) => (
        <Button
          type="primary"
          size="small"
          onClick={() => handleSelectGroup(record)}
        >
          Xem
        </Button>
      ),
    },
  ];

  // Members columns (same as managerMembership)
  const memberColumns: ColumnsType<MemberData> = [
    {
      title: "STT",
      key: "index",
      width: 60,
      align: "center",
      render: (_: any, __: MemberData, index: number) =>
        (currentMemberPage - 1) * pageSize + index + 1,
    },
    {
      title: "Họ tên",
      dataIndex: "fullname",
      key: "fullname",
      width: 200,
      render: (text: string, record: MemberData) => (
        <Space>
          {record.zaloAvatar && <Avatar src={record.zaloAvatar} size="small" />}
          <span>{text}</span>
        </Space>
      ),
    },
    {
      title: "Email",
      dataIndex: "email",
      key: "email",
      width: 200,
      render: (text: string) => text || "-",
    },
    {
      title: "Số điện thoại",
      dataIndex: "phone",
      key: "phone",
      width: 130,
      render: (text: string) => text || "-",
    },
    {
      title: "Công ty",
      dataIndex: "company",
      key: "company",
      width: 180,
      render: (text: string) => text || "-",
    },
    {
      title: "Chức vụ",
      dataIndex: "position",
      key: "position",
      width: 150,
      render: (text: string) => text || "-",
    },
    {
      title: "Ngày tạo",
      dataIndex: "createdDate",
      key: "createdDate",
      width: 120,
      render: (text: string) => text || "-",
    },
    {
      title: "Trạng thái",
      dataIndex: "approvalStatus",
      key: "approvalStatus",
      width: 120,
      align: "center",
      render: (status: number | null) => {
        if (status === null || status === 0) {
          return <Tag color="warning">Chờ duyệt</Tag>;
        } else if (status === 1) {
          return <Tag color="success">Đã duyệt</Tag>;
        } else if (status === 2) {
          return <Tag color="error">Từ chối</Tag>;
        }
        return null;
      },
    },
    {
      title: "Thao tác",
      key: "action",
      width: 70,
      align: "center",
      fixed: "right",
      render: (_: any, record: MemberData) => (
        <Space size={4}>
          <Button
            type="link"
            size="small"
            icon={<Eye className="w-3.5 h-3.5" />}
            style={{ padding: "0 4px" }}
            onClick={() => handleViewDetail(record)}
          />
          {(record.approvalStatus === null || record.approvalStatus === 0) && (
            <>
              <Button
                type="link"
                size="small"
                icon={<CheckCircle className="w-3.5 h-3.5" />}
                style={{ color: "#52c41a", padding: "0 4px" }}
                onClick={() => handleApprove(record.id)}
              />
              <Button
                type="link"
                size="small"
                icon={<XCircle className="w-3.5 h-3.5" />}
                danger
                style={{ padding: "0 4px" }}
                onClick={() => handleReject(record.id)}
              />
            </>
          )}
        </Space>
      ),
    },
  ];

  // Groups view
  if (currentView === "groups") {
    return (
      <Page style={{ marginTop: "50px", background: "#f5f5f5" }}>
        {/* Group Filter Tabs */}
        <TwoTierTab
          tabs={groupFilterTabs}
          activeTab={activeGroupTab}
          onTabChange={handleGroupTabChange}
          activeChildTab={activeGroupChildTab}
          onChildTabChange={handleGroupChildTabChange}
          backgroundColor="#fff"
          containerStyle={{
            borderBottom: "1px solid #e5e7eb",
          }}
        />

        {/* Search */}
        <div className="bg-white p-4 border-b border-gray-200">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-gray-400" />
            <Input
              value={inputGroupSearchValue}
              onChange={(e) => setInputGroupSearchValue(e.target.value)}
              placeholder="Tìm kiếm theo tên nhóm..."
              className="pl-10 pr-4 py-2 w-full rounded-lg border-gray-300"
            />
          </div>
        </div>

        {/* Summary */}
        <div className="bg-white px-4 py-3 border-b border-gray-200">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2">
              <Building2 className="w-5 h-5 text-gray-600" />
              <span className="text-sm text-gray-600">
                Tổng số:{" "}
                <span className="font-bold text-black">{totalGroupItems}</span>{" "}
                hội nhóm
              </span>
            </div>
            <span className="text-xs text-gray-500">
              Trang {currentGroupPage}/{totalGroupPages}
            </span>
          </div>
        </div>

        {/* Groups Table */}
        <div className="bg-white p-4">
          {loadingGroups && currentGroupPage === 1 ? (
            <div className="flex justify-center items-center py-16">
              <LoadingGiba size="lg" text="Đang tải danh sách hội nhóm..." />
            </div>
          ) : (
            <>
              <Table
                columns={groupColumns}
                dataSource={groups}
                rowKey="id"
                loading={loadingGroups && currentGroupPage > 1}
                pagination={false}
                scroll={{ x: 1000 }}
                locale={{
                  emptyText: (
                    <div className="text-center py-16">
                      <div className="w-20 h-20 bg-gradient-to-br from-gray-100 to-gray-200 rounded-full flex items-center justify-center mx-auto mb-4">
                        <Building2 className="w-8 h-8 text-gray-400" />
                      </div>
                      <div className="text-gray-800 text-lg font-bold mb-2">
                        Không tìm thấy hội nhóm
                      </div>
                      <div className="text-gray-500 text-sm">
                        {groupSearchKeyword
                          ? "Thử tìm kiếm với từ khóa khác"
                          : "Chưa có hội nhóm nào trong hệ thống"}
                      </div>
                    </div>
                  ),
                }}
              />

              {/* Load More Button */}
              {currentGroupPage < totalGroupPages && groups.length > 0 && (
                <div className="mt-4 text-center">
                  <Button
                    onClick={handleLoadMoreGroups}
                    loading={loadingGroups}
                    size="large"
                    style={{ minWidth: 200 }}
                  >
                    Tải thêm
                  </Button>
                </div>
              )}
            </>
          )}
        </div>
      </Page>
    );
  }

  // Members view
  return (
    <Page style={{ marginTop: "50px", background: "#f5f5f5" }}>
      {/* Back Button */}
      <div className="bg-white p-4 border-b border-gray-200">
        <Button
          icon={<ArrowLeft className="w-4 h-4" />}
          onClick={handleBackToGroups}
          type="text"
        >
          Quay lại danh sách Club
        </Button>
      </div>

      {/* Member Filter Tabs */}
      <TwoTierTab
        tabs={memberFilterTabs}
        activeTab={activeMemberTab}
        onTabChange={handleMemberTabChange}
        activeChildTab={activeMemberChildTab}
        onChildTabChange={handleMemberChildTabChange}
        backgroundColor="#fff"
        containerStyle={{
          borderBottom: "1px solid #e5e7eb",
        }}
      />

      {/* Search */}
      <div className="bg-white p-4 border-b border-gray-200">
        <div className="relative">
          <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-gray-400" />
          <Input
            value={inputMemberSearchValue}
            onChange={(e) => setInputMemberSearchValue(e.target.value)}
            placeholder="Tìm kiếm theo tên, email, số điện thoại..."
            className="pl-10 pr-4 py-2 w-full rounded-lg border-gray-300"
          />
        </div>
      </div>

      {/* Summary */}
      <div className="bg-white px-4 py-3 border-b border-gray-200">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-2">
            <Users className="w-5 h-5 text-gray-600" />
            <span className="text-sm text-gray-600">
              Tổng số:{" "}
              <span className="font-bold text-black">{totalMemberItems}</span>{" "}
              thành viên
            </span>
          </div>
          <span className="text-xs text-gray-500">
            Trang {currentMemberPage}/{totalMemberPages}
          </span>
        </div>
      </div>

      {/* Members Table */}
      <div className="bg-white p-4">
        {loadingMembers && currentMemberPage === 1 ? (
          <div className="flex justify-center items-center py-16">
            <LoadingGiba size="lg" text="Đang tải danh sách thành viên..." />
          </div>
        ) : (
          <>
            <Table
              columns={memberColumns}
              dataSource={members}
              rowKey="id"
              loading={loadingMembers && currentMemberPage > 1}
              pagination={false}
              scroll={{ x: 1200 }}
              locale={{
                emptyText: (
                  <div className="text-center py-16">
                    <div className="w-20 h-20 bg-gradient-to-br from-gray-100 to-gray-200 rounded-full flex items-center justify-center mx-auto mb-4">
                      <Users className="w-8 h-8 text-gray-400" />
                    </div>
                    <div className="text-gray-800 text-lg font-bold mb-2">
                      Không tìm thấy thành viên
                    </div>
                    <div className="text-gray-500 text-sm">
                      {memberSearchKeyword
                        ? "Thử tìm kiếm với từ khóa khác"
                        : "Chưa có thành viên nào trong nhóm này"}
                    </div>
                  </div>
                ),
              }}
            />

            {/* Load More Button */}
            {currentMemberPage < totalMemberPages && members.length > 0 && (
              <div className="mt-4 text-center">
                <Button
                  onClick={handleLoadMoreMembers}
                  loading={loadingMembers}
                  size="large"
                  style={{ minWidth: 200 }}
                >
                  Tải thêm
                </Button>
              </div>
            )}
          </>
        )}
      </div>

      {/* Member Detail Drawer */}
      <MemberDetailDrawer
        visible={detailDrawerVisible}
        member={selectedMember}
        onClose={() => setDetailDrawerVisible(false)}
        onApprove={handleApprove}
        onReject={handleReject}
        onRemoveFromGroup={handleRemoveFromGroup}
        membershipGroupId={selectedMember?.id}
      />

      {/* Reject Modal */}
      <RejectModal
        visible={rejectModalVisible}
        memberName={memberToReject?.fullname || ""}
        onConfirm={handleRejectConfirm}
        onCancel={() => {
          setRejectModalVisible(false);
          setMemberToReject(null);
        }}
        loading={actionLoading}
      />
    </Page>
  );
};

export default ManagerClub;
