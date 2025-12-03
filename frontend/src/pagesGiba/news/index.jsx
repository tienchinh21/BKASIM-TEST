import React, { useEffect, useCallback, useRef, useState, useLayoutEffect } from "react";
import { useNavigate } from "react-router-dom";
import { Page, Box } from "zmp-ui";
import axios from "axios";
import dfData from "../../common/DefaultConfig.json";
import useSetHeader from "../../components/hooks/useSetHeader";
import debounce from "lodash.debounce";
import "./news.css";
import Loading from "../../components/loading/Loading";
import ItemNew from "./ItemNew";
import { useRecoilValue, useRecoilState } from "recoil";
import { token, newsDataCacheGiba } from "../../recoil/RecoilState";
import TwoTierTab from "../../components/TwoTierTab";
import { Search } from "lucide-react";
import { toast } from "react-toastify";
import { isCacheValid, CACHE_MAX_AGE } from "../../utils/infiniteScrollUtils";

export default function News() {
  const setHeader = useSetHeader();
  const navigate = useNavigate();
  const userToken = useRecoilValue(token);
  const [cache, setCache] = useRecoilState(newsDataCacheGiba);

  const [loading, setLoading] = useState(false);
  const [categories, setCategories] = useState([]);
  const [inputSearchValue, setInputSearchValue] = useState(
    cache.filterSearch.keyword || ""
  );

  const searchTimeoutRef = useRef(null);
  const scrollContainerRef = useRef(null);
  const isRestoringRef = useRef(false);

  const pageSize = 10;
  const activeCategoryId = cache.filterSearch.categoryId || "";
  const activeGroupType = cache.filterSearch.groupType || "";
  const searchKeyword = cache.filterSearch.keyword || "";
  const listNews = cache.listNews;
  const currentPage = cache.filterSearch.page;
  const totalPages = cache.totalPages;
  const isLoadingMore = loading && currentPage > 1;

  const newsTabsData = [
    { id: "all", name: "Tất cả", value: "" },
    ...categories.map((cat) => ({
      id: cat.id,
      name: cat.name,
      value: cat.id,
    })),
  ];

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

  useLayoutEffect(() => {
    setCache((prev) => {
      const cacheIsValid = isCacheValid(prev.timestamp, CACHE_MAX_AGE);
      if (cacheIsValid && prev.listNews.length > 0) {
        isRestoringRef.current = true;
        return prev;
      }
      return {
        ...prev,
        listNews: [],
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
  }, [cache.listNews.length]);

  useEffect(() => {
    const timer = setTimeout(() => {
      if (inputSearchValue !== cache.filterSearch.keyword) {
        setCache((prev) => ({
          ...prev,
          listNews: [],
          filterSearch: { ...prev.filterSearch, page: 1, keyword: inputSearchValue },
          scrollTop: 0,
          timestamp: null,
        }));
      }
    }, 350);
    return () => clearTimeout(timer);
  }, [inputSearchValue, cache.filterSearch.keyword, setCache]);

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
      toast.error("Không thể tải danh mục");
    }
  };

  useEffect(() => {
    if (isRestoringRef.current) {
      isRestoringRef.current = false;
      return;
    }

    const getListNews = async () => {
      setLoading(true);
      try {
        const params = {
          page: currentPage,
          pageSize: pageSize,
          keyword: searchKeyword,
        };

        if (activeCategoryId) {
          params.categoryId = activeCategoryId;
        }

        const response = await axios.get(`${dfData.domain}/api/Articles/public`, {
          params,
          headers: { Authorization: `Bearer ${userToken}` },
        });

        if (response.data.code === 0) {
          const newData = response.data.data || [];
          const newTotalPages = response.data.totalPages || 1;
          const isFirstPage = currentPage === 1;

          setCache((prev) => ({
            ...prev,
            listNews: isFirstPage ? newData : [...prev.listNews, ...newData],
            totalPages: newTotalPages,
            timestamp: isFirstPage ? Date.now() : prev.timestamp,
          }));
        } else if (currentPage === 1) {
          setCache((prev) => ({
            ...prev,
            listNews: [],
            timestamp: Date.now(),
          }));
        }
      } catch (error) {
        console.error("Error fetching news:", error);
        toast.error("Không thể tải bản tin");
        if (currentPage === 1) {
          setCache((prev) => ({
            ...prev,
            listNews: [],
            timestamp: Date.now(),
          }));
        }
      } finally {
        setLoading(false);
      }
    };

    getListNews();
  }, [currentPage, activeCategoryId, searchKeyword, userToken, setCache]);

  const handleGroupTypeChange = useCallback(
    (groupValue) => {
      setCache((prev) => ({
        ...prev,
        listNews: [],
        filterSearch: {
          ...prev.filterSearch,
          page: 1,
          groupType: groupValue,
          categoryId: "",
          keyword: "",
        },
        scrollTop: 0,
        timestamp: null,
      }));
      setInputSearchValue("");
      if (searchTimeoutRef.current) {
        clearTimeout(searchTimeoutRef.current);
      }
    },
    [setCache]
  );

  const handleCategoryChange = useCallback(
    (categoryValue) => {
      setCache((prev) => ({
        ...prev,
        listNews: [],
        filterSearch: { ...prev.filterSearch, page: 1, categoryId: categoryValue },
        scrollTop: 0,
        timestamp: null,
      }));
    },
    [setCache]
  );

  const handleSearchChange = useCallback(
    (e) => {
      const value = e.target.value;
      setInputSearchValue(value);

      if (searchTimeoutRef.current) {
        clearTimeout(searchTimeoutRef.current);
      }

      searchTimeoutRef.current = setTimeout(() => {
        setCache((prev) => ({
          ...prev,
          listNews: [],
          filterSearch: { ...prev.filterSearch, page: 1, keyword: value },
          scrollTop: 0,
          timestamp: null,
        }));
      }, 300);
    },
    [setCache]
  );

  // eslint-disable-next-line react-hooks/exhaustive-deps
  const handleScroll = useCallback(
    debounce((e) => {
      if (isRestoringRef.current) {
        isRestoringRef.current = false;
        return;
      }
      const { scrollTop, clientHeight, scrollHeight } = e.target;
      if (scrollTop === 0 && currentPage === 1) return;
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
    [loading, currentPage, totalPages, setCache]
  );

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
      {loading && currentPage === 1 && <Loading />}

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
            {isLoadingMore && (
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
