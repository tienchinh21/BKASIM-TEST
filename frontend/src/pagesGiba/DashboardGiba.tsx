import React, { useState, useEffect } from "react";
import { Page } from "zmp-ui";
import { useNavigate } from "zmp-ui";
import useSetHeader from "../components/hooks/useSetHeader";
import { useRecoilValue } from "recoil";
import { token, isRegister, infoUser } from "../recoil/RecoilState";
import LoadingGiba from "../componentsGiba/LoadingGiba";
import axios from "axios";
import dfData from "../common/DefaultConfig.json";
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from "recharts";
import {
  Star,
  MessageCircle,
  Send,
  Inbox,
  Calendar,
  DollarSign,
  Users,
} from "lucide-react";

interface DashboardStats {
  totalRefsSent: number;
  totalRefsReceived: number;
  totalRevenue: number;
  totalEvents: number;
  totalRefValue: number;
  totalGroups: number;
  averageRating: number;
  feedbackRate: number;
}

interface ChartData {
  name: string;
  refSent: number;
  refReceived: number;
}

const DashboardGiba: React.FC = () => {
  const navigate = useNavigate();
  const setHeader = useSetHeader();
  const userToken = useRecoilValue(token);
  const isLoggedIn = useRecoilValue(isRegister);
  const userInfo = useRecoilValue(infoUser);
  const [loading, setLoading] = useState(true);
  const [chartLoading, setChartLoading] = useState(false);
  const [chartPeriod, setChartPeriod] = useState<
    "monthly" | "weekly" | "today"
  >("weekly");
  const [stats, setStats] = useState<DashboardStats>({
    totalRefsSent: 0,
    totalRefsReceived: 0,
    totalRevenue: 0,
    totalEvents: 0,
    totalRefValue: 0,
    totalGroups: 0,
    averageRating: 0,
    feedbackRate: 0,
  });

  const [chartData, setChartData] = useState<ChartData[]>([]);

  useEffect(() => {
    setHeader({
      title: "DASHBOARD",
      showUserInfo: false,
      showMenuButton: false,
      showCloseButton: false,
      hasLeftIcon: true,
    });
  }, [setHeader]);

  // Initial load - fetch all data
  useEffect(() => {
    const fetchDashboardStats = async () => {
      try {
        setLoading(true);

        const response = await axios.get(
          `${dfData.domain}/api/dashboard/stats`,
          {
            headers: {
              Authorization: `Bearer ${userToken}`,
            },
            params: {
              period: chartPeriod,
            },
          }
        );

        if (response.data.code === 0 && response.data.data) {
          const { summary, chartData: apiChartData } = response.data.data;

          setStats({
            totalRefsSent: summary.totalRefsSent || 0,
            totalRefsReceived: summary.totalRefsReceived || 0,
            totalRevenue: summary.totalRevenue || 0,
            totalEvents: summary.totalEvents || 0,
            totalRefValue: summary.totalRefValue
              ? summary.totalRefValue / 1000000
              : 0,
            totalGroups: summary.totalGroups || 0,
            averageRating: summary.averageRating || 0,
            feedbackRate: summary.feedbackRate || 0,
          });

          if (apiChartData && apiChartData.length > 0) {
            setChartData(
              apiChartData.map((item: any) => ({
                name: item.day,
                refSent: item.refSent || 0,
                refReceived: item.refReceived || 0,
              }))
            );
          } else {
            setChartData([]);
          }
        }
      } catch (error) {
        console.error("Error fetching dashboard stats:", error);
      } finally {
        setLoading(false);
      }
    };

    if (userToken && isLoggedIn) {
      fetchDashboardStats();
    } else {
      setLoading(false);
    }
  }, [userToken, isLoggedIn]);

  // Fetch chart data only when period changes
  useEffect(() => {
    const fetchChartData = async () => {
      try {
        setChartLoading(true);

        const response = await axios.get(
          `${dfData.domain}/api/dashboard/stats`,
          {
            headers: {
              Authorization: `Bearer ${userToken}`,
            },
            params: {
              period: chartPeriod,
            },
          }
        );

        if (response.data.code === 0 && response.data.data) {
          const { chartData: apiChartData } = response.data.data;

          if (apiChartData && apiChartData.length > 0) {
            setChartData(
              apiChartData.map((item: any) => ({
                name: item.day,
                refSent: item.refSent || 0,
                refReceived: item.refReceived || 0,
              }))
            );
          } else {
            setChartData([]);
          }
        }
      } catch (error) {
        console.error("Error fetching chart data:", error);
      } finally {
        setChartLoading(false);
      }
    };

    // Only fetch if not initial load
    if (userToken && isLoggedIn && !loading) {
      fetchChartData();
    }
  }, [chartPeriod]);

  // Format currency
  const formatCurrency = (value: number): string => {
    return new Intl.NumberFormat("vi-VN", {
      style: "currency",
      currency: "VND",
    }).format(value);
  };

  if (loading) {
    return (
      <Page className="bg-white min-h-screen mt-[50px]">
        <div className="flex justify-center items-center h-64">
          <LoadingGiba size="lg" text="Đang tải thống kê..." />
        </div>
      </Page>
    );
  }

  return (
    <Page className="bg-white min-h-screen pb-20" style={{ marginTop: "50px" }}>
      <div className="px-4 mt-4">
        <div className="grid grid-cols-2 gap-4 mb-6">
          <div
            className="bg-gray-900 rounded-xl p-4 cursor-pointer hover:bg-gray-800 transition-colors"
            onClick={() => navigate("/giba/ref-list")}
          >
            <div className="flex items-center gap-2 mb-2">
              <Send size={20} className="text-white" />
              <div className="text-white text-3xl font-bold">
                {stats.totalRefsSent}
              </div>
            </div>
            <div className="text-gray-400 text-sm mb-3">Referral đã gửi</div>
            <div className="flex items-center justify-between text-xs text-gray-400 mb-1">
              <span>Tháng này</span>
              <span>{Math.round((stats.totalRefsSent / 250) * 100)}%</span>
            </div>
            <div className="w-full bg-gray-700 rounded-full h-1.5">
              <div
                className="bg-white h-1.5 rounded-full"
                style={{
                  width: `${Math.min((stats.totalRefsSent / 250) * 100, 100)}%`,
                }}
              ></div>
            </div>
          </div>

          <div
            className="bg-white border border-gray-200 rounded-xl p-4 cursor-pointer hover:border-gray-300 transition-colors"
            onClick={() => navigate("/giba/ref-list")}
          >
            <div className="flex items-center gap-2 mb-2">
              <Inbox size={20} className="text-blue-500" />
              <div className="text-gray-900 text-3xl font-bold">
                {stats.totalRefsReceived}
              </div>
            </div>
            <div className="text-gray-500 text-sm mb-3">Referral đã nhận</div>
            <div className="flex items-center justify-between text-xs text-gray-400 mb-1">
              <span>Tháng này</span>
              <span>{Math.round((stats.totalRefsReceived / 300) * 100)}%</span>
            </div>
            <div className="w-full bg-gray-200 rounded-full h-1.5">
              <div
                className="bg-blue-500 h-1.5 rounded-full"
                style={{
                  width: `${Math.min(
                    (stats.totalRefsReceived / 300) * 100,
                    100
                  )}%`,
                }}
              ></div>
            </div>
          </div>

          {/* Total Events */}
          <div
            className="bg-white border border-gray-200 rounded-xl p-4 cursor-pointer hover:border-gray-300 transition-colors"
            onClick={() => navigate("/giba/event-registration-history")}
          >
            <div className="flex items-center gap-2 mb-2">
              <Calendar size={20} className="text-green-500" />
              <div className="text-gray-900 text-3xl font-bold">
                {stats.totalEvents}
              </div>
            </div>
            <div className="text-gray-500 text-sm mb-3">Sự kiện tham gia</div>
            <div className="flex items-center justify-between text-xs text-gray-400 mb-1">
              <span>Năm nay</span>
              <span>{Math.round((stats.totalEvents / 200) * 100)}%</span>
            </div>
            <div className="w-full bg-gray-200 rounded-full h-1.5">
              <div
                className="bg-gray-400 h-1.5 rounded-full"
                style={{
                  width: `${Math.min((stats.totalEvents / 200) * 100, 100)}%`,
                }}
              ></div>
            </div>
          </div>

          {/* Revenue/Value */}
          <div
            className="bg-white border border-gray-200 rounded-xl p-4 cursor-pointer hover:border-gray-300 transition-colors"
            onClick={() => navigate("/giba/groups")}
          >
            <div className="flex items-center gap-2 mb-2">
              <DollarSign size={20} className="text-red-500" />
              <div className="text-gray-900 text-3xl font-bold">
                {stats.totalRefValue}M
              </div>
            </div>
            <div className="text-gray-500 text-sm mb-3">
              Giá trị Referral (VNĐ)
            </div>
            <div className="flex items-center justify-between text-xs text-gray-400 mb-1">
              <span>Tháng này</span>
              <span>{Math.round((stats.totalRefValue / 150) * 100)}%</span>
            </div>
            <div className="w-full bg-gray-200 rounded-full h-1.5">
              <div
                className="bg-red-300 h-1.5 rounded-full"
                style={{
                  width: `${Math.min((stats.totalRefValue / 150) * 100, 100)}%`,
                }}
              ></div>
            </div>
          </div>

          {/* Average Rating */}
          <div
            className="bg-white border border-gray-200 rounded-xl p-4 cursor-pointer hover:border-gray-300 transition-colors"
            onClick={() => navigate("/giba/ref-list")}
          >
            <div className="flex items-center gap-2 mb-2">
              <Star size={20} className="text-yellow-500 fill-current" />
              <div className="text-gray-900 text-2xl font-bold">
                {stats.averageRating.toFixed(1)}
              </div>
            </div>
            <div className="text-gray-500 text-sm mb-3">
              Đánh giá trung bình
            </div>
            <div className="flex items-center gap-1 mb-2">
              {[1, 2, 3, 4, 5].map((star) => (
                <div key={star} className="flex items-center justify-center">
                  {star <= Math.round(stats.averageRating) ? (
                    <Star size={16} className="text-yellow-400 fill-current" />
                  ) : (
                    <Star size={16} className="text-gray-300" />
                  )}
                </div>
              ))}
            </div>
            <div className="w-full bg-gray-200 rounded-full h-1.5">
              <div
                className="bg-yellow-400 h-1.5 rounded-full"
                style={{
                  width: `${(stats.averageRating / 5) * 100}%`,
                }}
              ></div>
            </div>
          </div>

          {/* Feedback Rate */}
          <div
            className="bg-white border border-gray-200 rounded-xl p-4 cursor-pointer hover:border-gray-300 transition-colors"
            onClick={() => navigate("/giba/ref-list")}
          >
            <div className="flex items-center gap-2 mb-2">
              <MessageCircle size={20} className="text-blue-500" />
              <div className="text-gray-900 text-2xl font-bold">
                {stats.feedbackRate.toFixed(0)}%
              </div>
            </div>
            <div className="text-gray-500 text-sm mb-3">Tỉ lệ phản hồi</div>
            <div className="flex items-center justify-between text-xs text-gray-400 mb-1">
              <span>Từ ref đã hoàn thành</span>
              <span>{stats.feedbackRate.toFixed(0)}%</span>
            </div>
            <div className="w-full bg-gray-200 rounded-full h-1.5">
              <div
                className="bg-blue-500 h-1.5 rounded-full"
                style={{
                  width: `${Math.min(stats.feedbackRate, 100)}%`,
                }}
              ></div>
            </div>
          </div>
        </div>

        {/* Activity Chart Section */}
        <div className="bg-white border border-gray-200 rounded-xl p-4 mb-4">
          {/* Chart Header */}
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-lg font-bold text-gray-900">
              Thống kê Referral
            </h2>
            <div className="flex gap-1 bg-gray-100 rounded-lg p-1">
              <button
                onClick={() => setChartPeriod("monthly")}
                className={`px-3 py-1 rounded text-xs font-medium transition-colors ${
                  chartPeriod === "monthly"
                    ? "bg-white text-gray-900 shadow-sm"
                    : "text-gray-600"
                }`}
              >
                Monthly
              </button>
              <button
                onClick={() => setChartPeriod("weekly")}
                className={`px-3 py-1 rounded text-xs font-medium transition-colors ${
                  chartPeriod === "weekly"
                    ? "bg-gray-900 text-white"
                    : "text-gray-600"
                }`}
              >
                Weekly
              </button>
              <button
                onClick={() => setChartPeriod("today")}
                className={`px-3 py-1 rounded text-xs font-medium transition-colors ${
                  chartPeriod === "today"
                    ? "bg-white text-gray-900 shadow-sm"
                    : "text-gray-600"
                }`}
              >
                Today
              </button>
            </div>
          </div>

          {/* Chart */}
          <div className="h-64 -mx-2 relative">
            {chartLoading && (
              <div className="absolute inset-0 bg-white bg-opacity-75 flex items-center justify-center z-10">
                <div className="flex flex-col items-center gap-2">
                  <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-gray-900"></div>
                  <p className="text-xs text-gray-500">Đang tải...</p>
                </div>
              </div>
            )}
            {chartData.length > 0 ? (
              <ResponsiveContainer width="100%" height="100%">
                <LineChart
                  data={chartData}
                  margin={{ top: 5, right: 5, left: -20, bottom: 5 }}
                >
                  <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                  <XAxis
                    dataKey="name"
                    tick={{ fontSize: 12 }}
                    stroke="#9ca3af"
                  />
                  <YAxis tick={{ fontSize: 12 }} stroke="#9ca3af" width={40} />
                  <Tooltip
                    contentStyle={{
                      backgroundColor: "#fff",
                      border: "1px solid #e5e7eb",
                      borderRadius: "8px",
                      fontSize: "12px",
                    }}
                  />
                  <Legend wrapperStyle={{ fontSize: "12px" }} />
                  <Line
                    type="monotone"
                    dataKey="refSent"
                    stroke="#000"
                    strokeWidth={2}
                    name="Referral đã gửi"
                    dot={{ fill: "#000", r: 4 }}
                  />
                  <Line
                    type="monotone"
                    dataKey="refReceived"
                    stroke="#0066cc"
                    strokeWidth={2}
                    name="Referral đã nhận"
                    dot={{ fill: "#0066cc", r: 4 }}
                  />
                </LineChart>
              </ResponsiveContainer>
            ) : (
              <div className="flex items-center justify-center h-full">
                <div className="text-center">
                  <svg
                    className="w-16 h-16 text-gray-300 mx-auto mb-2"
                    fill="none"
                    viewBox="0 0 24 24"
                    stroke="currentColor"
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      strokeWidth={1}
                      d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"
                    />
                  </svg>
                  <p className="text-gray-400 text-sm">Chưa có dữ liệu</p>
                </div>
              </div>
            )}
          </div>
        </div>

        {/* Quick Actions */}
        <div className="space-y-2">
          <button
            className="w-full bg-white border border-gray-200 rounded-lg p-4 flex items-center justify-between hover:border-gray-300 transition-colors"
            onClick={() => navigate("/giba/ref-list")}
          >
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 bg-gray-100 rounded-lg flex items-center justify-center">
                <svg
                  className="w-5 h-5 text-gray-700"
                  fill="currentColor"
                  viewBox="0 0 20 20"
                >
                  <path d="M9 2a1 1 0 000 2h2a1 1 0 100-2H9z" />
                  <path
                    fillRule="evenodd"
                    d="M4 5a2 2 0 012-2 3 3 0 003 3h2a3 3 0 003-3 2 2 0 012 2v11a2 2 0 01-2 2H6a2 2 0 01-2-2V5zm3 4a1 1 0 000 2h.01a1 1 0 100-2H7zm3 0a1 1 0 000 2h3a1 1 0 100-2h-3zm-3 4a1 1 0 100 2h.01a1 1 0 100-2H7zm3 0a1 1 0 100 2h3a1 1 0 100-2h-3z"
                    clipRule="evenodd"
                  />
                </svg>
              </div>
              <div className="text-left">
                <div className="text-gray-900 font-semibold text-sm">
                  Danh sách Referral
                </div>
                <div className="text-gray-500 text-xs">
                  Quản lý các đơn Referral
                </div>
              </div>
            </div>
            <svg
              className="w-5 h-5 text-gray-400"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M9 5l7 7-7 7"
              />
            </svg>
          </button>

          <button
            className="w-full bg-white border border-gray-200 rounded-lg p-4 flex items-center justify-between hover:border-gray-300 transition-colors"
            onClick={() => navigate("/giba/event-registration-history")}
          >
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 bg-gray-100 rounded-lg flex items-center justify-center">
                <svg
                  className="w-5 h-5 text-gray-700"
                  fill="currentColor"
                  viewBox="0 0 20 20"
                >
                  <path
                    fillRule="evenodd"
                    d="M6 2a1 1 0 00-1 1v1H4a2 2 0 00-2 2v10a2 2 0 002 2h12a2 2 0 002-2V6a2 2 0 00-2-2h-1V3a1 1 0 10-2 0v1H7V3a1 1 0 00-1-1zm0 5a1 1 0 000 2h8a1 1 0 100-2H6z"
                    clipRule="evenodd"
                  />
                </svg>
              </div>
              <div className="text-left">
                <div className="text-gray-900 font-semibold text-sm">
                  Sự kiện
                </div>
                <div className="text-gray-500 text-xs">Lịch sử tham gia</div>
              </div>
            </div>
            <svg
              className="w-5 h-5 text-gray-400"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M9 5l7 7-7 7"
              />
            </svg>
          </button>

          <button
            className="w-full bg-white border border-gray-200 rounded-lg p-4 flex items-center justify-between hover:border-gray-300 transition-colors"
            onClick={() => navigate("/giba/groups")}
          >
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 bg-gray-100 rounded-lg flex items-center justify-center">
                <svg
                  className="w-5 h-5 text-gray-700"
                  fill="currentColor"
                  viewBox="0 0 20 20"
                >
                  <path d="M13 6a3 3 0 11-6 0 3 3 0 016 0zM18 8a2 2 0 11-4 0 2 2 0 014 0zM14 15a4 4 0 00-8 0v3h8v-3zM6 8a2 2 0 11-4 0 2 2 0 014 0zM16 18v-3a5.972 5.972 0 00-.75-2.906A3.005 3.005 0 0119 15v3h-3zM4.75 12.094A5.973 5.973 0 004 15v3H1v-3a3 3 0 013.75-2.906z" />
                </svg>
              </div>
              <div className="text-left">
                <div className="text-gray-900 font-semibold text-sm">Nhóm</div>
                <div className="text-gray-500 text-xs">Quản lý nhóm</div>
              </div>
            </div>
            <svg
              className="w-5 h-5 text-gray-400"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M9 5l7 7-7 7"
              />
            </svg>
          </button>

          <button
            className="w-full bg-white border border-gray-200 rounded-lg p-4 flex items-center justify-between hover:border-gray-300 transition-colors"
            onClick={() => navigate("/giba/profile-intro")}
          >
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 bg-gray-100 rounded-lg flex items-center justify-center">
                <svg
                  className="w-5 h-5 text-gray-700"
                  fill="currentColor"
                  viewBox="0 0 20 20"
                >
                  <path
                    fillRule="evenodd"
                    d="M10 9a3 3 0 100-6 3 3 0 000 6zm-7 9a7 7 0 1114 0H3z"
                    clipRule="evenodd"
                  />
                </svg>
              </div>
              <div className="text-left">
                <div className="text-gray-900 font-semibold text-sm">
                  Profile thành viên
                </div>
                <div className="text-gray-500 text-xs">
                  Tùy chỉnh profile cá nhân
                </div>
              </div>
            </div>
            <svg
              className="w-5 h-5 text-gray-400"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M9 5l7 7-7 7"
              />
            </svg>
          </button>
        </div>
      </div>
    </Page>
  );
};

export default DashboardGiba;
