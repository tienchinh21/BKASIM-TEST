import React, { useEffect, useCallback, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import { Page, Box } from "zmp-ui";
import axios from "axios";
import dfData from "../../common/DefaultConfig.json";
import useSetHeader from "../../components/hooks/useSetHeader";
import debounce from "lodash.debounce";
import "./news.css";
import Loading from "../../components/loading/Loading";
import ItemNew from "./ItemNew";
import { useRecoilValue } from "recoil";
import { token } from "../../recoil/RecoilState";
import TwoTierTab from "../../components/TwoTierTab/TwoTierTab";
import { NewsGroupType, createNewsTabsData } from "../../utils/enum/news.enum";
import { Search } from "lucide-react";
import { toast } from "react-toastify";
import { useHasRole } from "../../hooks/useHasRole";

export default function News() {
  const setHeader = useSetHeader();
  const navigate = useNavigate();
  const userToken = useRecoilValue(token);

  const [loading, setLoading] = useState(false);
  const [activeGroupType, setActiveGroupType] = useState(NewsGroupType.ALL);
  const [activeCategoryId, setActiveCategoryId] = useState("");
  const [categories, setCategories] = useState([]);
  const [newsTabsData, setNewsTabsData] = useState([]);
  const [inputSearchValue, setInputSearchValue] = useState("");
  const [searchKeyword, setSearchKeyword] = useState("");
  const [listNews, setListNews] = useState([]);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);

  const searchTimeoutRef = useRef(null);
  const scrollContainerRef = useRef(null);

  const pageSize = 10;

  useEffect(() => {
    setHeader({
      hasLeftIcon: true,
      type: "secondary",
      title: "BẢN TIN",
      customTitle: false,
      showUserInfo: false,
      showMenuButton: false,
      showCloseButton: false,
    });
    loadCategories();
  }, []);

  useEffect(() => {
    setPage(1);
    setListNews([]);
  }, [activeGroupType, activeCategoryId, searchKeyword]);

  useEffect(() => {
    getListNews();
  }, [page, activeGroupType, activeCategoryId, searchKeyword]);

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
        const tabsData = createNewsTabsData(response.data.data);
        setNewsTabsData(tabsData);
      }
    } catch (error) {
      console.error("Error loading categories:", error);
      toast.error("Không thể tải danh mục");
    }
  };

  const getListNews = async () => {
    if (loading) return;

    setLoading(true);
    try {
      const params = {
        keyword: searchKeyword,
      };

      // Only append categoryId if not ALL
      if (activeCategoryId) {
        params.categoryId = activeCategoryId;
      }

      // Only append groupType if not ALL
      if (activeGroupType && activeGroupType !== NewsGroupType.ALL) {
        params.groupType = activeGroupType;
      }

      const response = await axios.get(`${dfData.domain}/api/Articles/public`, {
        params,
        headers: { Authorization: `Bearer ${userToken}` },
      });

      if (response.data.code === 0) {
        const newData = response.data.data || [];
        const totalPages = response.data.totalPages || 1;

        setListNews((prev) => (page === 1 ? newData : [...prev, ...newData]));
        setTotalPages(totalPages);
      }
    } catch (error) {
      console.error("Error fetching news:", error);
      toast.error("Không thể tải bản tin");
    } finally {
      setLoading(false);
    }
  };

  const handleGroupTypeChange = (groupValue) => {
    setActiveGroupType(groupValue);
    setActiveCategoryId("");
    setInputSearchValue("");
    setSearchKeyword("");
    if (searchTimeoutRef.current) {
      clearTimeout(searchTimeoutRef.current);
    }
    scrollToTop();
  };

  const handleCategoryChange = (categoryValue) => {
    setActiveCategoryId(categoryValue);
    scrollToTop();
  };

  const handleSearchChange = (e) => {
    const value = e.target.value;
    setInputSearchValue(value);

    if (searchTimeoutRef.current) {
      clearTimeout(searchTimeoutRef.current);
    }

    searchTimeoutRef.current = setTimeout(() => {
      setSearchKeyword(value);
    }, 300);
  };

  const handleScroll = useCallback(
    debounce((e) => {
      const { scrollTop, clientHeight, scrollHeight } = e.target;
      const hasMore = listNews.length < totalPages * pageSize;

      if (
        scrollTop + clientHeight >= scrollHeight - 10 &&
        !loading &&
        hasMore
      ) {
        setPage((prev) => prev + 1);
      }
    }, 200),
    [loading, listNews.length, totalPages]
  );

  const scrollToTop = () => {
    if (scrollContainerRef.current) {
      scrollContainerRef.current.scrollTop = 0;
    }
  };

  return (
    <Page
      hideScrollbar={true}
      style={{
        paddingTop: "50px",
        background: "#f8fafc",
        overflowY: "hidden",
        height: "100vh",
      }}
    >
      {loading && page === 1 && <Loading />}

      <TwoTierTab
        tabs={newsTabsData}
        activeTab={activeGroupType}
        onTabChange={handleGroupTypeChange}
        activeChildTab={activeCategoryId}
        onChildTabChange={handleCategoryChange}
      />

      {/* Search */}
      <Box style={{ padding: "12px 16px", background: "#fff" }}>
        <Box style={{ position: "relative" }}>
          <input
            type="text"
            placeholder="Tìm kiếm bản tin..."
            value={inputSearchValue}
            onChange={handleSearchChange}
            style={{
              width: "100%",
              padding: "10px 12px 10px 38px",
              border: "1px solid #e2e8f0",
              borderRadius: "8px",
              fontSize: "14px",
              outline: "none",
            }}
          />
          <Search
            size={18}
            style={{
              position: "absolute",
              left: "12px",
              top: "50%",
              transform: "translateY(-50%)",
              color: "#94a3b8",
            }}
          />
        </Box>
      </Box>

      <Box
        ref={scrollContainerRef}
        style={{
          paddingBottom: 50,
          height: "calc(100vh - 200px)",
          overflowY: "scroll",
          width: "100%",
        }}
        onScroll={handleScroll}
      >
        {listNews.length > 0 ? (
          <div
            style={{
              padding: "12px 16px",
              display: "flex",
              flexDirection: "column",
              gap: "12px",
            }}
          >
            {listNews.map((item) => (
              <ItemNew key={item.id} item={item} />
            ))}
            {loading && page > 1 && (
              <Box style={{ textAlign: "center", padding: "16px" }}>
                <Loading />
              </Box>
            )}
          </div>
        ) : !loading ? (
          <Box
            style={{
              marginTop: "46px",
              textAlign: "center",
              color: "#64748b",
            }}
          >
            Không có bản tin
          </Box>
        ) : null}
      </Box>
    </Page>
  );
}
