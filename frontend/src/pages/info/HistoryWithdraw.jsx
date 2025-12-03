import React, { useState, useEffect, useRef } from "react";
import { Modal as ZmpModal, Box, Text, Button, Tabs } from "zmp-ui";
import axios from "axios";
import dfData from "../../common/DefaultConfig.json";
import { useRecoilValue } from "recoil";
import { infoShare, token } from "../../recoil/RecoilState";
import useSetHeader from "../../components/hooks/useSetHeader";
import { toast } from "react-toastify";

const HistoryWithdraw = () => {
  const setHeader = useSetHeader();
  const [activeTab, setActiveTab] = useState("0"); // Tab mặc định là "Tất cả"
  const [data, setData] = useState([]); // Lưu lịch sử theo trạng thái
  const [loading, setLoading] = useState(false); // Trạng thái tải dữ liệu
  const tokenAuth = useRecoilValue(token);

  useEffect(() => {
    setHeader({
      hasLeftIcon: true,
      type: "secondary",
      title: "Lịch sử rút tiền",
      customTitle: false,
    });
  }, []);

  // Theo dõi tab hiện tại và fetch dữ liệu
  useEffect(() => {
    fetchData(activeTab);
  }, [activeTab]);

  // Các trạng thái tab
  const tabs = [
    { label: "Tất cả", status: "0" },
    { label: "Đang chờ duyệt", status: "1" },
    { label: "Đã xác nhận", status: "3" },
    { label: "Hoàn thành", status: "5" },
    { label: "Đã hủy", status: "4" },
  ];

  const fetchData = (status) => {
    setLoading(true);
    // Xây dựng URL API
    const url = `${dfData.domain}/api/WithdrawRequests/history?RequestStatus=${status}&page=1&pagesize=100`;
    // Gọi API
    axios
      .get(url, {
        headers: {
          Authorization: `Bearer ${tokenAuth}`,
        },
      })
      .then((res) => {
        setData(res.data.data || []);
      })
      .catch((error) => {
        console.error("Error fetching withdraw history:", error);
        setData([]);
      })
      .finally(() => {
        setLoading(false);
      });
  };

  const cancelWithdrawRequest = (id) => {
    setLoading(true);
    axios
      .post(`${dfData.domain}/api/WithdrawRequests/cancel/${id}`, null, {
        headers: {
          Authorization: `Bearer ${tokenAuth}`,
        },
      })
      .then((res) => {
        if (res.data.code === 0) {
          toast.success("Hủy yêu cầu rút tiền thành công!");
          fetchData(activeTab);
        } else {
          toast.error("Lỗi hủy yêu cầu!");
        }
      })
      .catch((error) => {
        console.error("Error canceling withdraw request:", error);
        toast.error("Có lỗi xảy ra khi hủy yêu cầu.");
      })
      .finally(() => {
        setLoading(false);
      });
  };

  const getBackgroundColorByStatus = (status) => {
    switch (status) {
      case 1:
        return "#747474";
      case 2:
        return "#E73636";
      case 3:
        return "#E68C3D";
      case 4:
        return "#E73636";
      case 5:
        return "#00B106";
      default:
        return "#747474";
    }
  };

  const getStatusNameByStatus = (status) => {
    switch (status) {
      case 1:
        return "Đang chờ duyệt";
      case 2:
        return "Đã hủy";
      case 3:
        return "Đã xác nhận";
      case 4:
        return "Đã hủy";
      case 5:
        return "Đã hoàn thành";
      default:
        return "Không xác định";
    }
  };

  return (
    <Box style={{ marginTop: 36, paddingTop: 16, paddingBottom: 40 }}>
      <Box
        style={{
          marginTop: "3%",
          paddingLeft: 10,
          paddingRight: 10,
          display: "flex",
          fontSize: 12,
          overflowX: "auto",
          whiteSpace: "nowrap",
        }}
        className="hidden-scrollbar"
      >
        {tabs.map((item, index) => (
          <Box
            key={index}
            className="itemService"
            style={{
              cursor: "pointer",
              background: activeTab === item.status ? "#FFE6C0" : "#EFEFEF",
            }}
            onClick={() => setActiveTab(item.status)}
          >
            {item.label}
          </Box>
        ))}
      </Box>

      {loading ? (
        <Box style={{ textAlign: "center", padding: 20 }}>
          Đang tải dữ liệu ...
        </Box>
      ) : (
        <Box>
          {data.length === 0 ? (
            <Box style={{ textAlign: "center", padding: 20 }}>
              Không có dữ liệu
            </Box>
          ) : (
            data.map((item, index) => (
              <Box
                key={index}
                style={{
                  width: "90%",
                  margin: "5% auto",
                  display: "flex",
                  flexDirection: "column",
                  gap: 10,
                  borderRadius: 10,
                }}
                className="border shadow-sm px-2"
              >
                <Box
                  style={{
                    padding: "2%",
                    display: "flex",
                    flexDirection: "column",
                    gap: 10,
                  }}
                >
                  <Box
                    style={{
                      display: "flex",
                      justifyContent: "space-between",
                      alignItems: "center",
                    }}
                  >
                    <Box></Box>
                    <Box
                      style={{
                        color: "#fff",
                        fontSize: 12,
                        fontWeight: 600,
                        borderRadius: 15,
                        width: "32%",
                        textAlign: "center",
                        padding: "2px",
                        backgroundColor: getBackgroundColorByStatus(
                          item.requestStatus
                        ),
                      }}
                    >
                      {getStatusNameByStatus(item.requestStatus)}
                    </Box>
                  </Box>
                  {/* <Box>
                          <Text style={{ fontWeight: "bold" }}>
                            Mã đơn rút:
                          </Text>
                          {item.id.toUpperCase()}
                        </Box> */}
                  <Box
                    style={{
                      display: "flex",
                      justifyContent: "space-between",
                      alignItems: "center",
                    }}
                  >
                    <Text style={{ fontWeight: "bold" }}>Khách hàng:</Text>
                    <span>
                      <Box style={{ display: "flex", gap: 3 }}>
                        {item.cusName}
                      </Box>
                    </span>
                  </Box>
                  <Box
                    style={{
                      display: "flex",
                      justifyContent: "space-between",
                      alignItems: "center",
                    }}
                  >
                    <Text style={{ fontWeight: "bold" }}>Tổng tiền rút:</Text>
                    <span>
                      <Box style={{ display: "flex", gap: 3 }}>
                        {item.amount}
                        <Box>VNĐ</Box>
                      </Box>
                    </span>
                  </Box>
                  <Box
                    style={{
                      display: "flex",
                      justifyContent: "space-between",
                      alignItems: "center",
                    }}
                  >
                    <Text style={{ fontWeight: "bold" }}>
                      Ngày yêu cầu rút:
                    </Text>
                    <span>
                      <Box style={{ display: "flex", gap: 3 }}>
                        {new Intl.DateTimeFormat("en-GB").format(
                          new Date(item.updatedDate)
                        )}
                      </Box>
                    </span>
                  </Box>
                  {item?.requestStatus == 1 && (
                    <Box
                      style={{
                        display: "flex",
                        justifyContent: "center",
                        alignItems: "center",
                      }}
                    >
                      <Button
                        style={{
                          padding: 1,
                          border: "1px solid",
                          borderRadius: 12,
                          background: "#FFBC8F",
                        }}
                        onClick={() => cancelWithdrawRequest(item.id)}
                      >
                        Hủy yêu cầu
                      </Button>
                    </Box>
                  )}
                </Box>
              </Box>
            ))
          )}
        </Box>
      )}
    </Box>
  );
};

export default HistoryWithdraw;
