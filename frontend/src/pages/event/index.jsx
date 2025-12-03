import React, { useEffect, useState, useCallback, useRef } from "react";
import axios from "axios";
import { useRecoilValue } from "recoil";
import { Box, Page } from "zmp-ui";
import dfData from "../../common/DefaultConfig.json";
import { token } from "../../recoil/RecoilState";
import useSetHeader from "../../components/hooks/useSetHeader";
import Loading from "../../components/loading/Loading";
import EventItem from "./EventItem";
import Category from "../../components/Category";
import debounce from "lodash.debounce";
import { useLocation } from "react-router-dom";
import { GlobalStyles } from "../../store/styles/GlobalStyles";

const Event = () => {
  const setHeader = useSetHeader();
  const tokenAuth = useRecoilValue(token);
  const containerRef = useRef();
  const location = useLocation();
  const query = new URLSearchParams(location.search);
  const defaultScreenParam = query.get("default");

  const [listEvent, setListEvent] = useState([]);
  const [activeTab, setActiveTab] = useState("0");
  const [loading, setLoading] = useState(false);
  const [page, setPage] = useState(1);
  const [totalPage, setTotalPage] = useState(1);

  const pageSize = 10;

  const tabs = [
    { id: "0", name: "Tất cả" },
    { id: "1", name: "Sắp diễn ra" },
    { id: "2", name: "Đang diễn ra" },
    { id: "3", name: "Đã kết thúc" },
  ];

  useEffect(() => {
    setHeader({
      hasLeftIcon: true,
      type: "secondary",
      title: "DANH SÁCH SỰ KIỆN",
      customTitle: false,
      route: defaultScreenParam ? "/home" : null,
      hasLeftIcon: true,
    });
  }, [defaultScreenParam]);

  useEffect(() => {
    setPage(1);
    setListEvent([]);
  }, [activeTab]);

  useEffect(() => {
    getAllEvent();
  }, [page, activeTab]);

  const getAllEvent = async () => {
    setLoading(true);
    try {
      const resp = await axios.get(`${dfData.domain}/api/events`, {
        params: { status: activeTab, page, pageSize },
        headers: { Authorization: `Bearer ${tokenAuth}` },
      });

      const newData = resp.data.data || [];
      const newTotalPage = resp.data.totalPages || 1;

      setListEvent((prev) => (page === 1 ? newData : [...prev, ...newData]));
      setTotalPage(newTotalPage);
    } catch (error) {
      console.log("Error fetching event:", error);
    } finally {
      setLoading(false);
    }
  };

  const handleScroll = useCallback(
    debounce((e) => {
      const { scrollTop, clientHeight, scrollHeight } = e.target;
      if (
        scrollTop + clientHeight >= scrollHeight - 10 &&
        !loading &&
        page < totalPage
      ) {
        setPage((prev) => prev + 1);
      }
    }, 200),
    [loading, page, totalPage]
  );

  return (
    <Page
      style={{ paddingTop: 50, background: "var(--second-background-color)" }}
    >
      {loading && <Loading />}
      <Category
        list={tabs}
        value={activeTab}
        onChange={(val) => {
          setActiveTab(val);
        }}
        containerStyle={{ justifyContent: "space-around" }}
      />

      {listEvent.length > 0 ? (
        <Box
          flex
          flexDirection="column"
          ref={containerRef}
          onScroll={handleScroll}
          style={{
            padding: "20px 14px 110px",
            overflowY: "auto",
            gap: GlobalStyles.spacing.xs,
          }}
        >
          {listEvent.map((item, index) => (
            <Box
              key={item.id}
              style={{ marginBottom: index < listEvent.length - 1 ? 10 : 0 }}
            >
              <EventItem item={item} />
            </Box>
          ))}
        </Box>
      ) : (
        <Box style={{ marginTop: "46px", textAlign: "center" }}>
          Hiện tại chưa có sự kiện khả dụng
        </Box>
      )}
    </Page>
  );
};

export default Event;
