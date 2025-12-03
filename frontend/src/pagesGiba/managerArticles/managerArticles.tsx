import React, { useState, useEffect } from "react";
import { Page, Box } from "zmp-ui";
import { useRecoilValue } from "recoil";
import { useNavigate } from "react-router-dom";
import { token } from "../../recoil/RecoilState";
import { Input, Table, Button, Modal, Tag, Space } from "antd";
import type { ColumnsType } from "antd/es/table";
import useSetHeader from "../../components/hooks/useSetHeader";
import LoadingGiba from "../../componentsGiba/LoadingGiba";
import Category from "../../components/Category";
import { Search, Eye } from "lucide-react";
import { toast } from "react-toastify";
import axios from "axios";
import dfData from "../../common/DefaultConfig.json";

interface CategoryData {
  id: string;
  name: string;
  displayOrder?: number;
}

interface ArticleData {
  id: string;
  title: string;
  categoryId?: string;
  categoryName?: string;
  status: number;
  createdDate: string;
  bannerImage?: string;
  groupCategory?: string;
  groupNames?: string[];
}

const ManagerArticles: React.FC = () => {
  const navigate = useNavigate();
  const setHeader = useSetHeader();
  React.useEffect(() => {
    setHeader({
      title: "QUẢN LÝ BẢN TIN",
      showUserInfo: false,
      showMenuButton: false,
      showCloseButton: false,
      hasLeftIcon: true,
    });
  }, [setHeader]);
  const userToken = useRecoilValue(token);
  const [articles, setArticles] = useState<ArticleData[]>([]);
  const [categories, setCategories] = useState<CategoryData[]>([]);
  const [activeCategoryId, setActiveCategoryId] = useState("");
  const [loading, setLoading] = useState(false);
  const [searchKeyword, setSearchKeyword] = useState("");
  const [inputSearchValue, setInputSearchValue] = useState("");
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalItems, setTotalItems] = useState(0);
  const pageSize = 10;

  // Load categories on mount
  useEffect(() => {
    if (userToken) {
      loadCategories();
    }
  }, [userToken]);

  const loadCategories = async () => {
    try {
      const response = await axios.get(
        `${dfData.domain}/api/ArticleCategories`,
        {
          params: { page: 1, pagesize: 100, keyword: "" },
          headers: { Authorization: `Bearer ${userToken}` },
        }
      );

      if (response.data.code === 200 && response.data.data) {
        setCategories(response.data.data);
      }
    } catch (error) {
      console.error("Error loading categories:", error);
    }
  };

  // Debounce search
  useEffect(() => {
    const timer = setTimeout(() => {
      if (searchKeyword !== inputSearchValue) {
        setSearchKeyword(inputSearchValue);
        setCurrentPage(1);
        setArticles([]);
      }
    }, 500);

    return () => clearTimeout(timer);
  }, [inputSearchValue, searchKeyword]);

  // Reset when category changes
  useEffect(() => {
    setCurrentPage(1);
    setArticles([]);
  }, [activeCategoryId]);

  // Fetch articles
  useEffect(() => {
    if (userToken) {
      fetchArticles();
    }
  }, [userToken, searchKeyword, currentPage, activeCategoryId]);

  const fetchArticles = async () => {
    try {
      setLoading(true);
      const params: any = {
        keyword: searchKeyword,
        page: currentPage,
        pageSize: pageSize,
      };

      // Add categoryId filter if selected
      if (activeCategoryId) {
        params.categoryId = activeCategoryId;
      }

      const response = await axios.get(
        `${dfData.domain}/api/Articles/GetPage`,
        {
          params,
          headers: { Authorization: `Bearer ${userToken}` },
        }
      );

      if (response.data.code === 0) {
        const newData = response.data.data || [];

        // Map categoryId to categoryName
        const articlesWithCategoryName = newData.map((article: ArticleData) => {
          const category = categories.find(
            (cat) => cat.id === article.categoryId
          );
          return {
            ...article,
            categoryName: category ? category.name : "-",
          };
        });

        setArticles((prev) =>
          currentPage === 1
            ? articlesWithCategoryName
            : [...prev, ...articlesWithCategoryName]
        );
        setTotalPages(response.data.totalPages || 1);
        setTotalItems(response.data.totalCount || 0);
      }
    } catch (error) {
      console.error("Error fetching articles:", error);
      toast.error("Không thể tải danh sách bản tin");
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = (articleId: string, articleTitle: string) => {
    Modal.confirm({
      title: "Xác nhận xóa",
      content: `Bạn có chắc chắn muốn xóa bản tin "${articleTitle}"?`,
      okText: "Xóa",
      cancelText: "Hủy",
      okButtonProps: { danger: true },
      onOk: async () => {
        try {
          const response = await axios.delete(
            `${dfData.domain}/api/Articles/${articleId}`,
            {
              headers: { Authorization: `Bearer ${userToken}` },
            }
          );

          if (response.data.code === 0) {
            toast.success(response.data.message || "Xóa bản tin thành công!");
            setArticles((prev) => prev.filter((item) => item.id !== articleId));
            setTotalItems((prev) => prev - 1);
          } else {
            toast.error(response.data.message || "Xóa bản tin thất bại");
          }
        } catch (error) {
          console.error("Error deleting article:", error);
          toast.error("Không thể xóa bản tin");
        }
      },
    });
  };

  const handleViewDetail = (article: ArticleData) => {
    // Navigate to detail page with article data and admin mode
    navigate(`/giba/news-detail/${article.id}`, {
      state: { article, isAdminMode: true },
    });
  };

  const handleLoadMore = () => {
    if (currentPage < totalPages) {
      setCurrentPage((prev) => prev + 1);
    }
  };

  const columns: ColumnsType<ArticleData> = [
    {
      title: "STT",
      key: "index",
      width: 60,
      align: "center",
      render: (_: any, __: ArticleData, index: number) =>
        (currentPage - 1) * pageSize + index + 1,
    },
    {
      title: "Tiêu đề",
      dataIndex: "title",
      key: "title",
      width: 300,
      render: (text: string, record: ArticleData) => (
        <Space>
          {record.bannerImage && (
            <img
              src={record.bannerImage}
              alt={text}
              className="w-12 h-12 object-cover rounded"
            />
          )}
          <span className="font-medium">{text}</span>
        </Space>
      ),
    },
    {
      title: "Danh mục",
      dataIndex: "categoryName",
      key: "categoryName",
      width: 150,
      render: (text: string) => text || "-",
    },
    {
      title: "Nhóm",
      dataIndex: "groupCategory",
      key: "group",
      width: 150,
      render: (groupCategory: string, record: ArticleData) => {
        if (groupCategory) {
          return groupCategory;
        } else if (record.groupNames && record.groupNames.length > 0) {
          return record.groupNames.join(", ");
        }
        return "-";
      },
    },
    {
      title: "Ngày tạo",
      dataIndex: "createdDate",
      key: "createdDate",
      width: 150,
      render: (text: string) => {
        if (!text) return "-";
        const date = new Date(text);
        return date.toLocaleDateString("vi-VN");
      },
    },
    {
      title: "Trạng thái",
      dataIndex: "status",
      key: "status",
      width: 120,
      align: "center",
      render: (status: number) => {
        if (status === 1) {
          return <Tag color="success">Công khai</Tag>;
        } else {
          return <Tag color="default">Riêng tư</Tag>;
        }
      },
    },
    {
      title: "Thao tác",
      key: "action",
      width: 70,
      align: "center",
      fixed: "right",
      render: (_: any, record: ArticleData) => (
        <Button
          type="link"
          size="small"
          icon={<Eye className="w-4 h-4" />}
          onClick={() => handleViewDetail(record)}
          title="Xem chi tiết"
        />
      ),
    },
  ];

  if (!userToken) {
    return (
      <Page style={{ marginTop: "50px", background: "#f5f5f5" }}>
        <LoadingGiba />
      </Page>
    );
  }

  // Prepare category list for Category component
  const categoryList = [
    { id: "", name: "Tất cả" },
    ...categories.map((cat) => ({ id: cat.id, name: cat.name })),
  ];

  return (
    <Page style={{ marginTop: "50px", background: "#f5f5f5" }}>
      {/* Category Filter */}
      <Box className="bg-white">
        <Category
          list={categoryList}
          value={activeCategoryId}
          onChange={setActiveCategoryId}
          valueChild=""
          onChangeValueChild={() => {}}
        />
      </Box>

      {/* Search Bar */}
      <div className="bg-white p-4 border-b border-gray-200">
        <div className="relative">
          <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-gray-400" />
          <Input
            value={inputSearchValue}
            onChange={(e) => setInputSearchValue(e.target.value)}
            placeholder="Tìm kiếm theo tiêu đề..."
            className="pl-10 pr-4 py-2 w-full rounded-lg border-gray-300"
          />
        </div>
      </div>

      {/* Stats */}
      <div className="bg-white px-4 py-3 border-b border-gray-200">
        <div className="flex items-center justify-between">
          <span className="text-sm text-gray-600">
            Tổng số: <span className="font-bold text-black">{totalItems}</span>{" "}
            bản tin
          </span>
          <span className="text-xs text-gray-500">
            Trang {currentPage}/{totalPages}
          </span>
        </div>
      </div>

      {/* Table */}
      <div className="bg-white p-4">
        <Table
          columns={columns}
          dataSource={articles}
          rowKey="id"
          loading={loading}
          pagination={false}
          scroll={{ x: 1000 }}
          locale={{
            emptyText: (
              <div className="text-center py-16">
                <div className="text-gray-800 text-lg font-bold mb-2">
                  Không tìm thấy bản tin
                </div>
                <div className="text-gray-500 text-sm">
                  {searchKeyword
                    ? "Thử tìm kiếm với từ khóa khác"
                    : "Chưa có bản tin nào trong hệ thống"}
                </div>
              </div>
            ),
          }}
        />

        {/* Load More Button */}
        {currentPage < totalPages && articles.length > 0 && (
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
    </Page>
  );
};

export default ManagerArticles;
