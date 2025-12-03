import React, { useState, useEffect } from "react";
import { Page } from "zmp-ui";
import { useNavigate } from "react-router-dom";
import { useRecoilValue } from "recoil";
import { token, phoneNumberUser, userMembershipInfo } from "../../recoil/RecoilState";
import axios from "axios";
import dfData from "../../common/DefaultConfig.json";
import useSetHeader from "../../components/hooks/useSetHeader";
import { ChevronRight } from "lucide-react";
import EventItem from "../event/EventItem";
import EventGuestItem from "../event/EventGuestItem";
import ShowcaseItem from "../showcase/ShowcaseItem";
import MeetingItem from "../meetingSchedule/MeetingItem";
import ItemNew from "../news/ItemNew";
import LoadingGiba from "../../componentsGiba/LoadingGiba";

interface ComingSoonData {
  events: any[];
  newsletter: any;
  meetings: any[];
  showcases: any[];
}

interface SectionProps {
  title: string;
  items: any[];
  onViewMore: () => void;
  renderItem: (item: any) => React.ReactNode;
  emptyMessage?: string;
  showViewMore?: boolean;
}

const Section: React.FC<SectionProps> = ({
  title,
  items,
  onViewMore,
  renderItem,
  emptyMessage = "Chưa có dữ liệu",
  showViewMore = true,
}) => {
  return (
    <div className="mb-8">
      <div className="flex items-center justify-between mb-4 px-4">
        <h3 className="text-lg font-bold text-black">{title}</h3>
        {showViewMore && (
          <button
            onClick={onViewMore}
            className="flex items-center gap-1 text-black text-sm font-medium hover:text-gray-600 transition-colors"
          >
            Xem thêm
            <ChevronRight size={14} />
          </button>
        )}
      </div>
      {items && items.length > 0 ? (
        <div className="flex gap-4 overflow-x-auto px-4 pb-2">
          {items.map((item) => (
            <div
              key={item.id}
              style={{
                minWidth: "280px",
                width: "280px",
                flexShrink: 0,
              }}
            >
              {renderItem(item)}
            </div>
          ))}
        </div>
      ) : (
        <div className="px-4">
          <div className="text-center py-8 text-gray-500 bg-gray-50 rounded-lg text-sm">
            {emptyMessage}
          </div>
        </div>
      )}
    </div>
  );
};

const ComingSoonPage: React.FC = () => {
  const navigate = useNavigate();
  const setHeader = useSetHeader();
  const userToken = useRecoilValue(token);
  const phoneNumber = useRecoilValue(phoneNumberUser);
  const membershipInfo = useRecoilValue(userMembershipInfo);
  const [data, setData] = useState<ComingSoonData | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    setHeader({
      hasLeftIcon: true,
      type: "secondary",
      title: "SẮP DIỄN RA",
      customTitle: false,
      showUserInfo: false,
      showMenuButton: false,
      showCloseButton: false,
    });
  }, []);

  useEffect(() => {
    fetchComingSoonData();
  }, [userToken, phoneNumber, membershipInfo?.phoneNumber]);

  const fetchComingSoonData = async () => {
    try {
      setLoading(true);
      const headers: any = {};
      if (userToken) {
        headers.Authorization = `Bearer ${userToken}`;
      }

      // Fetch ComingSoon data
      const comingSoonResponse = await axios.get(`${dfData.domain}/api/ComingSoon`, {
        headers,
      });

      let comingSoonData: ComingSoonData = {
        events: [],
        newsletter: null,
        meetings: [],
        showcases: [],
      };

      if (comingSoonResponse.data.code === 0 && comingSoonResponse.data.data) {
        comingSoonData = comingSoonResponse.data.data;
      }

      // Fetch confirmed events only if user is pending approval
      let confirmedEvents: any[] = [];
      const phoneToUse = phoneNumber || membershipInfo?.phoneNumber;
      const isPendingApproval = membershipInfo?.approvalStatus === 0 || membershipInfo?.approvalStatus === 2;

      console.log("ComingSoonPage - isPendingApproval:", isPendingApproval);
      console.log("ComingSoonPage - phoneToUse:", phoneToUse);

      if (isPendingApproval && phoneToUse && phoneToUse.trim() !== "") {
        try {
          console.log("Fetching confirmed events for pending user");
          const confirmedResponse = await axios.get(
            `${dfData.domain}/api/EventGuests/GetConfirmedEventsByPhone/${phoneToUse}`,
            { headers }
          );

          if (confirmedResponse.data && Array.isArray(confirmedResponse.data)) {
            confirmedEvents = confirmedResponse.data;
            console.log("Confirmed events count:", confirmedEvents.length);
          }
        } catch (error) {
          console.error("Error fetching confirmed events:", error);
        }
      }

      // Merge events: prioritize confirmed events and remove duplicates
      let mergedEvents = [...comingSoonData.events];

      if (confirmedEvents.length > 0) {
        const confirmedEventIds = new Set(confirmedEvents.map((e) => e.id));
        mergedEvents = mergedEvents.filter((event) => !confirmedEventIds.has(event.id));

        const markedConfirmedEvents = confirmedEvents.map((event) => ({
          ...event,
          isConfirmedGuest: true,
        }));

        mergedEvents = [...markedConfirmedEvents, ...mergedEvents];
      }

      setData({
        ...comingSoonData,
        events: mergedEvents,
      });
    } catch (error) {
      console.error("Error fetching coming soon data:", error);
    } finally {
      setLoading(false);
    }
  };

  const handleViewDetailMeeting = (meeting: any) => {
    navigate("/giba/meeting-detail", { state: { meeting } });
  };

  const handleViewDetailShowcase = (showcase: any) => {
    navigate("/giba/showcase-detail", { state: { showcase } });
  };

  // Check if user is a member (logged in and approved)
  const isMember = !!(userToken && membershipInfo?.approvalStatus === 1);

  if (loading) {
    return (
      <Page className="bg-white min-h-screen mt-[50px]">
        <LoadingGiba />
      </Page>
    );
  }

  if (!data) {
    return (
      <Page className="bg-white min-h-screen mt-[50px]">
        <div className="text-center py-16 text-gray-500">Không có dữ liệu</div>
      </Page>
    );
  }

  return (
    <Page className="bg-white min-h-screen mt-[50px] pb-20">
      <div className="py-4">
        {/* Sự kiện Section */}
        <Section
          title="Sự kiện"
          items={data.events || []}
          onViewMore={() => navigate("/giba/event")}
          renderItem={(event) =>
            event.isConfirmedGuest ? (
              <EventGuestItem item={event} />
            ) : (
              <EventItem item={event} />
            )
          }
          emptyMessage="Không có sự kiện sắp tới"
          showViewMore={isMember}
        />

        <Section
          title="Lịch họp"
          items={data.meetings || []}
          onViewMore={() => navigate("/giba/meeting-list")}
          renderItem={(meeting) => (
            <MeetingItem
              meeting={meeting}
              onViewDetail={handleViewDetailMeeting}
            />
          )}
          emptyMessage="Không có lịch họp sắp tới"
        />

        <Section
          title="Lịch showcase"
          items={data.showcases || []}
          onViewMore={() => navigate("/giba/showcase-list")}
          renderItem={(showcase) => (
            <ShowcaseItem
              showcase={showcase}
              onViewDetail={handleViewDetailShowcase}
            />
          )}
          emptyMessage="Không có showcase sắp tới"
        />

        {data.newsletter && (
          <div className="mb-8">
            <div className="flex items-center justify-between mb-4 px-4">
              <h3 className="text-lg font-bold text-black">Bản tin</h3>
              <button
                onClick={() => navigate("/giba/news")}
                className="flex items-center gap-1 text-black text-sm font-medium hover:text-gray-600 transition-colors"
              >
                Xem thêm
                <ChevronRight size={14} />
              </button>
            </div>
            <div className="flex gap-4 overflow-x-auto px-4 pb-2">
              <div
                style={{
                  minWidth: "280px",
                  width: "280px",
                  flexShrink: 0,
                }}
              >
                <ItemNew item={data.newsletter} />
              </div>
            </div>
          </div>
        )}
      </div>
    </Page>
  );
};

export default ComingSoonPage;
