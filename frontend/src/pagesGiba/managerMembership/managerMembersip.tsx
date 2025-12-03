import React, { useState, useEffect, useRef } from "react";
import { Page } from "zmp-ui";
import { useRecoilValue } from "recoil";
import { token } from "../../recoil/RecoilState";
import { Input, Modal, Table, Button, Avatar, Tag, Space } from "antd";
import type { ColumnsType } from "antd/es/table";
import useSetHeader from "../../components/hooks/useSetHeader";
import LoadingGiba from "../../componentsGiba/LoadingGiba";
import { Search, Users, Eye, CheckCircle, XCircle } from "lucide-react";
import { toast } from "react-toastify";
import axios from "axios";
import dfData from "../../common/DefaultConfig.json";
import { MemberData } from "./components/MemberCard";
import MemberDetailDrawer from "./components/MemberDetailDrawer";
import RejectModal from "./components/RejectModal";
import Category from "../../components/Category";
import MemberCard from "./components/MemberCard";

// Status filter categories
const statusCategories = [
  { id: "", name: "Tất cả" },
  { id: "0", name: "Chờ duyệt" },
  { id: "1", name: "Đã duyệt" },
  { id: "2", name: "Từ chối" },
];

const ManagerMembership: React.FC = () => {
  const setHeader = useSetHeader();
  const userToken = useRecoilValue(token);

  // State
  const [loading, setLoading] = useState(false);
  const [members, setMembers] = useState<MemberData[]>([]);
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(0);
  const [totalItems, setTotalItems] = useState(0);
  const [searchKeyword, setSearchKeyword] = useState("");
  const [inputSearchValue, setInputSearchValue] = useState("");
  const [statusFilter, setStatusFilter] = useState("");
  const [selectedMember, setSelectedMember] = useState<MemberData | null>(null);
  const [detailDrawerVisible, setDetailDrawerVisible] = useState(false);
  const [rejectModalVisible, setRejectModalVisible] = useState(false);
  const [memberToReject, setMemberToReject] = useState<MemberData | null>(null);
  const [actionLoading, setActionLoading] = useState(false);

  const searchTimeoutRef = useRef<any>(null);
  const pageSize = 10;

  useEffect(() => {
    setHeader({
      title: "QUẢN LÝ THÀNH VIÊN",
      showUserInfo: false,
      showMenuButton: false,
      showCloseButton: false,
      hasLeftIcon: true,
    });
  }, [setHeader]);

  useEffect(() => {
    fetchMembers();
  }, [currentPage, searchKeyword, statusFilter]);

  useEffect(() => {
    if (searchTimeoutRef.current) {
      clearTimeout(searchTimeoutRef.current);
    }

    searchTimeoutRef.current = setTimeout(() => {
      setSearchKeyword(inputSearchValue);
      setCurrentPage(1);
    }, 500);

    return () => {
      if (searchTimeoutRef.current) {
        clearTimeout(searchTimeoutRef.current);
      }
    };
  }, [inputSearchValue]);

  const fetchMembers = async () => {
    if (!userToken) {
      toast.error("Vui lòng đăng nhập để tiếp tục");
      return;
    }

    try {
      setLoading(true);

      const params: any = {
        page: currentPage,
        pageSize: pageSize,
      };

      if (searchKeyword) {
        params.keyword = searchKeyword;
      }

      if (statusFilter) {
        params.status = statusFilter;
      }

      const response = await axios.get(`${dfData.domain}/Membership/GetPage`, {
        params,
        headers: {
          Authorization: `Bearer ${userToken}`,
        },
      });

      if (response.data.success) {
        setMembers(response.data.data || []);
        setTotalItems(response.data.totalItems || 0);
        setTotalPages(response.data.totalPages || 0);
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
      setLoading(false);
    }
  };

  const handleStatusChange = (statusId: string) => {
    setStatusFilter(statusId);
    setCurrentPage(1);
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

  const handleLoadMore = () => {
    if (currentPage < totalPages) {
      setCurrentPage(currentPage + 1);
    }
  };

  if (loading && currentPage === 1) {
    return (
      <Page className="bg-white min-h-screen mt-[50px]">
        <div className="flex justify-center items-center h-64">
          <LoadingGiba size="lg" text="Đang tải danh sách thành viên..." />
        </div>
      </Page>
    );
  }

  const columns: ColumnsType<MemberData> = [
    {
      title: "STT",
      key: "index",
      width: 60,
      align: "center",
      render: (_: any, __: MemberData, index: number) =>
        (currentPage - 1) * pageSize + index + 1,
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

  return (
    <Page style={{ marginTop: "50px", background: "#f5f5f5" }}>
      <Category
        list={statusCategories}
        value={statusFilter}
        onChange={handleStatusChange}
        valueChild=""
        onChangeValueChild={() => {}}
        backgroundColor="#fff"
        containerStyle={{
          borderBottom: "1px solid #e5e7eb",
        }}
      />

      <div className="bg-white p-4 border-b border-gray-200">
        <div className="relative">
          <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-gray-400" />
          <Input
            value={inputSearchValue}
            onChange={(e) => setInputSearchValue(e.target.value)}
            placeholder="Tìm kiếm theo tên, email, số điện thoại..."
            className="pl-10 pr-4 py-2 w-full rounded-lg border-gray-300"
          />
        </div>
      </div>

      <div className="bg-white px-4 py-3 border-b border-gray-200">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-2">
            <Users className="w-5 h-5 text-gray-600" />
            <span className="text-sm text-gray-600">
              Tổng số:{" "}
              <span className="font-bold text-black">{totalItems}</span> thành
              viên
            </span>
          </div>
          <span className="text-xs text-gray-500">
            Trang {currentPage}/{totalPages}
          </span>
        </div>
      </div>

      <div className="bg-white p-4">
        <Table
          columns={columns}
          dataSource={members}
          rowKey="id"
          loading={loading}
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
                  {searchKeyword
                    ? "Thử tìm kiếm với từ khóa khác"
                    : "Chưa có thành viên nào trong hệ thống"}
                </div>
              </div>
            ),
          }}
        />

        {/* Load More Button */}
        {currentPage < totalPages && members.length > 0 && (
          <div className="mt-4 text-center">
            <Button
              onClick={handleLoadMore}
              loading={loading}
              size="large"
              style={{ minWidth: 200 }}
            >
              Tải thêm
            </Button>
          </div>
        )}
      </div>

      {/* Member Detail Drawer */}
      <MemberDetailDrawer
        visible={detailDrawerVisible}
        member={selectedMember}
        onClose={() => setDetailDrawerVisible(false)}
        onApprove={handleApprove}
        onReject={handleReject}
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

export default ManagerMembership;
