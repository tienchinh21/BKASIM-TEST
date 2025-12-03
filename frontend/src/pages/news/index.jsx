import React, {
  useEffect,
  useCallback,
  useRef,
  useLayoutEffect,
  useState,
} from "react";
import { useNavigate } from "react-router-dom";
import { Page, Box, Input } from "zmp-ui";
import axios from "axios";
import dfData from "../../common/DefaultConfig.json";
import useSetHeader from "../../components/hooks/useSetHeader";
import debounce from "lodash.debounce";
import "./news.css";
import Category from "../../components/Category";
import Loading from "../../components/loading/Loading";
import ItemNew from "./ItemNew";
import { GlobalStyles } from "../../store/styles/GlobalStyles";
import { useRecoilState } from "recoil";
import { newsDataCache } from "../../recoil/RecoilState";

export default function News() {
  const setHeader = useSetHeader();
  const navigate = useNavigate();

  const [newsCache, setNewsCache] = useRecoilState(newsDataCache);
  const [loading, setLoading] = useState(false);
  const scrollContainerRef = useRef(null);
  const isRestoringRef = useRef(false);

  // ====== INIT HEADER + CACHE CHECK ======
  useEffect(() => {
    setHeader({
      hasLeftIcon: true,
      type: "secondary",
      title: "BẢN TIN",
      customTitle: false,
    });
  }, []);

  useLayoutEffect(() => {
    const FIVE_MIN = 5 * 60 * 1000;
    const isValidCache =
      newsCache?.timestamp && Date.now() - newsCache.timestamp < FIVE_MIN;

    if (isValidCache) {
      if (newsCache.scrollTop > 0) isRestoringRef.current = true;
    } else {
      isRestoringRef.current = false;
      setNewsCache({
        listNews: [],
        listCategory: [],
        filterSearch: {
          page: 1,
          pageSize: 10,
          keyword: "",
          categoryId: "",
        },
        scrollTop: 0,
        timestamp: null,
        totalPages: 1,
      });
    }
  }, []);

  useEffect(() => {
    if (newsCache.listCategory.length === 0) {
      getCategory();
    }
  }, [newsCache.listCategory]);

  const getCategory = async () => {
    try {
      const response = await axios.get(
        `${dfData.domain}/api/Articles/Category`,
        {
          params: { page: 1, pageSize: 100 },
        }
      );
      setNewsCache((prev) => ({
        ...prev,
        listCategory: [{ id: "", name: "Tất cả" }, ...response.data.data],
      }));
    } catch (error) {
      console.log("Error category:", error);
    }
  };

  useEffect(() => {
    if (isRestoringRef.current) return;
    getListNews();
  }, [newsCache.filterSearch]);

  const getListNews = async () => {
    setLoading(true);
    try {
      const response = await axios.get(`${dfData.domain}/api/articles`, {
        params: newsCache.filterSearch,
      });
      if (response.data.code === 0) {
        const newList = response.data.data;
        const isFirstPage = newsCache.filterSearch.page === 1;
        setNewsCache((prev) => ({
          ...prev,
          listNews: isFirstPage ? newList : [...prev.listNews, ...newList],
          totalPages: response.data.totalPages,
          timestamp: Date.now(),
        }));
      }
    } catch (error) {
      console.log("Error list news:", error);
    } finally {
      setLoading(false);
    }
  };

  useLayoutEffect(() => {
    const scrollTop = newsCache.scrollTop;
    const listLen = newsCache.listNews.length;
    if (!isRestoringRef.current || scrollTop === 0 || listLen === 0) return;

    const el = scrollContainerRef.current;
    if (!el) return;

    const tryScroll = () => {
      el.scrollTop = scrollTop;
      isRestoringRef.current = false;
    };
    requestAnimationFrame(tryScroll);
  }, [newsCache.scrollTop, newsCache.listNews.length]);

  const handleScroll = useCallback(
    debounce((e) => {
      if (isRestoringRef.current) {
        isRestoringRef.current = false;
        return;
      }
      const { scrollTop, clientHeight, scrollHeight } = e.target;
      if (scrollTop === 0 && newsCache.filterSearch.page === 1) {
        return;
      }

      setNewsCache((prev) => {
        const page = prev.filterSearch?.page || 1;
        const totalPages = prev.totalPages || 1;
        if (
          scrollTop + clientHeight >= scrollHeight - 10 &&
          !loading &&
          page < totalPages
        ) {
          return {
            ...prev,
            scrollTop,
            timestamp: Date.now(),
            filterSearch: {
              ...prev.filterSearch,
              page: page + 1,
            },
          };
        } else {
          return {
            ...prev,
            scrollTop,
            timestamp: Date.now(),
          };
        }
      });
    }, 200),
    [loading, newsCache.filterSearch.page, newsCache.totalPages]
  );

  const handleKeywordChange = (e) => {
    const newKeyword = e.target.value;
    if (newKeyword === newsCache.filterSearch.keyword) return;
    scrollToTop();
    setNewsCache((prev) => ({
      ...prev,
      scrollTop: 0,
      filterSearch: {
        ...prev.filterSearch,
        keyword: newKeyword,
        page: 1,
      },
    }));
  };

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
        paddingBottom: "50px",
        overflowY: "hidden",
        height: "100vh",
      }}
    >
      {loading && <Loading />}
      <Category
        list={newsCache.listCategory}
        value={newsCache.filterSearch.categoryId}
        onChange={(value) => {
          scrollToTop();
          setNewsCache((prev) => ({
            ...prev,
            scrollTop: 0,
            filterSearch: {
              ...prev.filterSearch,
              categoryId: value,
              page: 1,
            },
          }));
        }}
      />
      <Box
        className="divCenter"
        style={{
          flexDirection: "column",
          ...GlobalStyles.getMargin(10, "auto"),
        }}
      >
        <Box style={{ padding: "0px 14px", width: "100%" }}>
          <Input.Search
            placeholder="Tìm kiếm nhanh"
            value={newsCache.filterSearch.keyword}
            onChange={handleKeywordChange}
          />
        </Box>

        <Box
          ref={scrollContainerRef}
          style={{
            paddingBottom: 50,
            height: "80vh",
            overflowY: "scroll",
            width: "100%",
          }}
          onScroll={handleScroll}
        >
          {newsCache.listNews.length > 0 ? (
            newsCache.listNews.map((item, index) => (
              <ItemNew key={index} item={item}></ItemNew>
            ))
          ) : (
            <Box mt={3}>Không có tin tức</Box>
          )}
        </Box>
      </Box>
    </Page>
  );
}
