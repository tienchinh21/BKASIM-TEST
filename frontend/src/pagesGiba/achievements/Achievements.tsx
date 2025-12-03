import React, { useState, useEffect } from "react";
import { Page } from "zmp-ui";
import { useRecoilValue } from "recoil";
import { token } from "../../recoil/RecoilState";
import useSetHeader from "../../components/hooks/useSetHeader";
import LoadingGiba from "../../componentsGiba/LoadingGiba";
import axios from "axios";
import dfData from "../../common/DefaultConfig.json";
import { Plus } from "lucide-react";

interface UserStats {
  events: {
    checkInCount: number;
  };
  guestInvites: {
    count: number;
    totalGuestNumber: number;
  };
  refs: {
    sent: {
      count: number;
      value: number;
    };
    received: {
      count: number;
      value: number;
    };
  };
  appointments: {
    confirmedCount: number;
  };
}

const Achievements: React.FC = () => {
  const setHeader = useSetHeader();
  const userToken = useRecoilValue(token);
  const [loading, setLoading] = useState(true);
  const [stats, setStats] = useState<UserStats | null>(null);

  React.useEffect(() => {
    setHeader({
      title: "THÀNH TÍCH",
      showUserInfo: false,
      showMenuButton: false,
      showCloseButton: false,
      hasLeftIcon: true,
    });
  }, [setHeader]);

  useEffect(() => {
    const fetchStats = async () => {
      if (!userToken) {
        setLoading(false);
        return;
      }

      try {
        setLoading(true);
        const response = await axios.get(
          `${dfData.domain}/api/statistics/user-stats`,
          {
            headers: {
              Authorization: `Bearer ${userToken}`,
              "Content-Type": "application/json",
            },
          }
        );

        if (response.data.success && response.data.data) {
          setStats(response.data.data);
        }
      } catch (error) {
        console.error("Error fetching user stats:", error);
      } finally {
        setLoading(false);
      }
    };

    fetchStats();
  }, [userToken]);

  if (loading) {
    return (
      <Page className="bg-white min-h-screen mt-[50px]">
        <div className="flex justify-center items-center h-64">
          <LoadingGiba size="lg" text="Đang tải thống kê..." />
        </div>
      </Page>
    );
  }

  if (!stats) {
    return (
      <Page className="bg-white min-h-screen mt-[50px]">
        <div className="text-center py-12 px-4">
          <div className="text-gray-500 text-lg">
            Không thể tải thống kê thành tích
          </div>
        </div>
      </Page>
    );
  }

  const StatCard: React.FC<{
    title: string;
    value: string | number;
    subtitle?: string;
  }> = ({ title, value, subtitle }) => (
    <div className="bg-white border border-gray-200 rounded-xl p-5 shadow-sm">
      <div className="text-sm text-gray-600 mb-2">{title}</div>
      <div className="text-2xl font-bold text-gray-900 mb-1">{value}</div>
      {subtitle && <div className="text-xs text-gray-500 mt-1">{subtitle}</div>}
    </div>
  );

  return (
    <Page className="bg-gray-50 min-h-screen mt-[50px]">
      <div className="px-4 py-6 pb-20">
        {/* Sự kiện */}
        <div className="mb-6">
          <div className="flex items-center justify-between mb-3">
            <h2 className="text-lg font-semibold text-gray-900">Sự kiện</h2>
            <Plus size={18} className="text-gray-400" />
          </div>
          <StatCard title="Hiện diện" value={stats.events.checkInCount} />
        </div>

        {/* Mời khách */}
        <div className="mb-6">
          <div className="flex items-center justify-between mb-3">
            <h2 className="text-lg font-semibold text-gray-900">Khách mời</h2>
            <Plus size={18} className="text-gray-400" />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <StatCard title="Số lần mời" value={stats.guestInvites.count} />
            <StatCard
              title="Tổng số khách"
              value={stats.guestInvites.totalGuestNumber}
            />
          </div>
        </div>

        {/* REF */}
        <div className="mb-6">
          <div className="flex items-center justify-between mb-3">
            <h2 className="text-lg font-semibold text-gray-900">REFERRAL</h2>
            <Plus size={18} className="text-gray-400" />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <StatCard
              title="Đã gửi"
              value={stats.refs.sent.count}
              subtitle={`Giá trị: ${stats.refs.sent.value.toLocaleString()}`}
            />
            <StatCard
              title="Đã nhận"
              value={stats.refs.received.count}
              subtitle={`Giá trị: ${stats.refs.received.value.toLocaleString()}`}
            />
          </div>
        </div>

        {/* Cuộc hẹn */}
        <div className="mb-6">
          <div className="flex items-center justify-between mb-3">
            <h2 className="text-lg font-semibold text-gray-900">Cuộc hẹn</h2>
            <Plus size={18} className="text-gray-400" />
          </div>
          <StatCard
            title="Đã xác nhận"
            value={stats.appointments.confirmedCount}
          />
        </div>
      </div>
    </Page>
  );
};

export default Achievements;
