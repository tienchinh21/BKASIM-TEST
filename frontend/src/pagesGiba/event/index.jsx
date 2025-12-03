import React, { useEffect, useState, useCallback, useRef } from "react";
import axios from "axios";
import { useRecoilValue } from "recoil";
import { Box, Page, Input } from "zmp-ui";
import { Select } from "antd";
import dfData from "../../common/DefaultConfig.json";
import { token } from "../../recoil/RecoilState";
import useSetHeader from "../../components/hooks/useSetHeader";
import Loading from "../../components/loading/Loading";
import EventItem from "./EventItem";
import debounce from "lodash.debounce";
import { useLocation, useNavigate } from "react-router-dom";
import { GlobalStyles } from "../../store/styles/GlobalStyles";
import TwoTierTab from "../../components/TwoTierTab/TwoTierTab";
import {
  EventTabsData,
  EventType,
  EventStatus,
} from "../../utils/enum/event.enum";
import { toast } from "react-toastify";
import { useHasRole } from "../../hooks/useHasRole";
import { Search } from "lucide-react";

const Event = () => {
  const setHeader = useSetHeader();
  const tokenAuth = useRecoilValue(token);
  const navigate = useNavigate();
  const hasRole = useHasRole();
  const containerRef = useRef();
  const searchTimeoutRef = useRef(null);
  const location = useLocation();
  const query = new URLSearchParams(location.search);

  const [activeType, setActiveType] = useState(EventType.ALL);
  const [activeStatus, setActiveStatus] = useState(EventStatus.ALL);
  const [listEvent, setListEvent] = useState([]);
  const [loading, setLoading] = useState(false);
  const [page, setPage] = useState(1);
  const [totalRecords, setTotalRecords] = useState(0);
  const [inputSearchValue, setInputSearchValue] = useState("");
  const [searchKeyword, setSearchKeyword] = useState("");
  const [selectedGroupId, setSelectedGroupId] = useState("");
  const [groups, setGroups] = useState([]);
  const [loadingGroups, setLoadingGroups] = useState(false);

  const pageSize = 10;

  useEffect(() => {
    setHeader({
      hasLeftIcon: false,
      type: "secondary",
      title: "DANH SÁCH SỰ KIỆN",
      customTitle: false,
      showUserInfo: false,
      showMenuButton: false,
      showCloseButton: false,
      hasLeftIcon: true,
    });
    loadGroups();
  }, []);

  useEffect(() => {
    setPage(1);
    setListEvent([]);
  }, [activeType, activeStatus, searchKeyword, selectedGroupId]);

  useEffect(() => {
    getAllEvent();
  }, [page, activeType, activeStatus, searchKeyword, selectedGroupId]);

  const loadGroups = async () => {
    try {
      setLoadingGroups(true);
      const response = await axios.get(`${dfData.domain}/api/Groups/all`, {
        params: { joinstatus: "approved" },
        headers: { Authorization: `Bearer ${tokenAuth}` },
      });

      if (response.data.code === 0 && response.data.data?.items) {
        setGroups(response.data.data.items);
      }
    } catch (error) {
      console.error("Error loading groups:", error);
    } finally {
      setLoadingGroups(false);
    }
  };

  const getAllEvent = async () => {
    if (loading) return;

    setLoading(true);
    try {
      const params = {
        page,
        pageSize,
        keyword: searchKeyword,
        groupId: selectedGroupId,
      };

      // Only append grouptype if not ALL
      if (activeType && activeType !== EventType.ALL) {
        params.grouptype = activeType;
      }

      // Only append status if not ALL
      if (activeStatus && activeStatus !== EventStatus.ALL) {
        params.status = activeStatus;
      }

      const resp = await axios.get(`${dfData.domain}/Event/GetPage`, {
        params,
        headers: { Authorization: `Bearer ${tokenAuth}` },
      });

      const newData = resp.data.data || [];
      const totalRecords = resp.data.recordsFiltered || 0;

      setListEvent((prev) => (page === 1 ? newData : [...prev, ...newData]));
      setTotalRecords(totalRecords);
    } catch (error) {
      console.error("Error fetching events:", error);
      toast.error("Không thể tải danh sách sự kiện");
    } finally {
      setLoading(false);
    }
  };

  const handleSearchChange = (e) => {
    const value = e.target.value;
    setInputSearchValue(value);

    if (searchTimeoutRef.current) {
      clearTimeout(searchTimeoutRef.current);
    }

    searchTimeoutRef.current = setTimeout(() => {
      setSearchKeyword(value);
    }, 200);
  };

  const handleTypeChange = (typeValue) => {
    setActiveType(typeValue);
    setActiveStatus(EventStatus.ALL);
    setSelectedGroupId("");
    setInputSearchValue("");
    setSearchKeyword("");
    if (searchTimeoutRef.current) {
      clearTimeout(searchTimeoutRef.current);
    }
  };

  const handleStatusChange = (statusValue) => {
    setActiveStatus(statusValue);
  };

  const handleGroupChange = (value) => {
    setSelectedGroupId(value);
    setInputSearchValue("");
    setSearchKeyword("");
    if (searchTimeoutRef.current) {
      clearTimeout(searchTimeoutRef.current);
    }
  };

  const handleScroll = useCallback(
    debounce((e) => {
      const { scrollTop, clientHeight, scrollHeight } = e.target;
      const hasMore = listEvent.length < totalRecords;

      if (
        scrollTop + clientHeight >= scrollHeight - 10 &&
        !loading &&
        hasMore
      ) {
        setPage((prev) => prev + 1);
      }
    }, 200),
    [loading, listEvent.length, totalRecords]
  );

  return (
    <Page style={{ paddingTop: 50, background: "#f8fafc" }}>
      {loading && page === 1 && <Loading />}

      <TwoTierTab
        tabs={EventTabsData}
        activeTab={activeType}
        onTabChange={handleTypeChange}
        activeChildTab={activeStatus}
        onChildTabChange={handleStatusChange}
      />

      <Box style={{ padding: "12px 16px", background: "#fff" }}>
        <Box
          style={{
            display: "flex",
            gap: "8px",
            marginBottom: "8px",
          }}
        >
          <Box style={{ flex: 1, position: "relative" }}>
            <input
              type="text"
              placeholder="Tìm kiếm sự kiện..."
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

          <Select
            placeholder="Chọn nhóm"
            value={selectedGroupId || undefined}
            onChange={handleGroupChange}
            style={{ width: "140px", height: "40px" }}
            loading={loadingGroups}
            allowClear
          >
            {groups.map((group) => (
              <Select.Option key={group.id} value={group.id}>
                {group.groupName}
              </Select.Option>
            ))}
          </Select>
        </Box>
      </Box>

      {listEvent.length > 0 ? (
        <Box
          flex
          flexDirection="column"
          ref={containerRef}
          onScroll={handleScroll}
          style={{
            padding: "12px 16px 110px",
            overflowY: "auto",
            gap: "12px",
          }}
        >
          {listEvent.map((item) => (
            <EventItem key={item.id} item={item} />
          ))}
          {loading && page > 1 && (
            <Box style={{ textAlign: "center", padding: "16px" }}>
              <Loading />
            </Box>
          )}
        </Box>
      ) : !loading ? (
        <Box
          style={{ marginTop: "46px", textAlign: "center", color: "#64748b" }}
        >
          Hiện tại chưa có sự kiện khả dụng
        </Box>
      ) : null}
    </Page>
  );
};

export default Event;
