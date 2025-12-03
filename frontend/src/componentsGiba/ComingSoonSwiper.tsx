import React, { useState, useEffect, useRef } from "react";
import { useNavigate } from "react-router-dom";
import { useRecoilValue } from "recoil";
import {
  token,
  phoneNumberUser,
  userMembershipInfo,
} from "../recoil/RecoilState";
import axios from "axios";
import dfData from "../common/DefaultConfig.json";
import EventItem from "../pagesGiba/event/EventItem";
import EventGuestItem from "../pagesGiba/event/EventGuestItem";
import ShowcaseItem from "../pagesGiba/showcase/ShowcaseItem";
import MeetingItem from "../pagesGiba/meetingSchedule/MeetingItem";
import { ChevronRight } from "lucide-react";
import Badge from "./Badge";
import DefaultImage from "../assets/no_image.png";
import ComingSoonSkeleton from "./skeletons/ComingSoonSkeleton";

interface ComingSoonData {
  events: any[];
  newsletters: any | any[];
  meetings: any[];
  showcases: any[];
}

const EventCard: React.FC<{ item: any; isConfirmedGuest?: boolean }> = ({
  item,
  isConfirmedGuest = false,
}) => {
  return (
    <div
      style={{
        minWidth: "280px",
        width: "280px",
        flexShrink: 0,
      }}
    >
      {isConfirmedGuest ? (
        <EventGuestItem item={item} titleMaxLines={1} />
      ) : (
        <EventItem
          item={item}
          titleMaxLines={1}
          customRegisterText={undefined}
          onInvitationConfirmSuccess={undefined}
        />
      )}
    </div>
  );
};

const MeetingCard: React.FC<{ item: any }> = ({ item }) => {
  const navigate = useNavigate();

  const handleViewDetail = (meeting: any) => {
    navigate("/giba/meeting-detail", { state: { meeting } });
  };

  const transformedMeeting = {
    ...item,
    startDate: item.startDate || item.time,
    endDate: item.endDate,
    // Map type to isPublic (type: 2 = Công khai)
    isPublic: item.isPublic !== undefined ? item.isPublic : item.type === 2,
  };

  return (
    <div
      style={{
        minWidth: "280px",
        width: "280px",
        flexShrink: 0,
      }}
    >
      <MeetingItem
        meeting={transformedMeeting}
        onViewDetail={handleViewDetail}
      />
    </div>
  );
};

const ShowcaseCard: React.FC<{ item: any }> = ({ item }) => {
  const navigate = useNavigate();

  const handleViewDetail = (showcase: any) => {
    navigate("/giba/showcase-detail", { state: { showcase } });
  };

  const transformedShowcase = {
    ...item,
    startDate: item.startDate || item.time,
    endDate: item.endDate,
    // Map type to isPublic (type: 2 = Công khai)
    isPublic: item.isPublic !== undefined ? item.isPublic : item.type === 2,
  };

  return (
    <div
      style={{
        minWidth: "280px",
        width: "280px",
        flexShrink: 0,
      }}
    >
      <ShowcaseItem
        showcase={transformedShowcase}
        onViewDetail={handleViewDetail}
      />
    </div>
  );
};

const NewsletterCard: React.FC<{ item: any }> = ({ item }) => {
  const navigate = useNavigate();

  console.log("item", item);

  // Helper function to strip HTML tags and get plain text
  const stripHtml = (html: string) => {
    if (!html) return "";
    const tmp = document.createElement("div");
    tmp.innerHTML = html;
    return tmp.textContent || tmp.innerText || "";
  };

  const contentText = stripHtml(item.content);

  // Badge Công khai/Nội bộ dựa trên type (1 = Công khai)
  const getVisibilityBadge = () => {
    if (item.type === 1) {
      return {
        label: "Công khai",
        bgColor: "#E8F5E9",
        textColor: "#2E7D32",
        borderColor: "#81C784",
      };
    } else {
      return {
        label: "Nội bộ",
        bgColor: "#FFF3E0",
        textColor: "#E65100",
        borderColor: "#FFB74D",
      };
    }
  };

  const visibilityBadge = getVisibilityBadge();

  return (
    <div
      className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden min-w-[280px] max-w-[280px] cursor-pointer hover:shadow-md transition-shadow flex flex-col"
      onClick={() => navigate(`/giba/news-detail/${item.id}`)}
      style={{ position: "relative", minHeight: "380px" }}
    >
      <div
        style={{ position: "absolute", top: "8px", right: "8px", zIndex: 10 }}
        className="flex flex-col gap-1 items-end"
      >
        <div
          style={{
            backgroundColor: "#FFF3E0",
            color: "#E65100",
            border: "1px solid #FFB74D",
            padding: "4px 12px",
            borderRadius: "12px",
            fontSize: "11px",
            fontWeight: "600",
          }}
        >
          Bản tin
        </div>
        <div
          style={{
            backgroundColor: visibilityBadge.bgColor,
            color: visibilityBadge.textColor,
            border: `1px solid ${visibilityBadge.borderColor}`,
            padding: "4px 12px",
            borderRadius: "12px",
            fontSize: "11px",
            fontWeight: "600",
          }}
        >
          {visibilityBadge.label}
        </div>
      </div>

      {/* Banner Image */}
      <div className="relative w-full" style={{ paddingTop: "56.25%" }}>
        <img
          src={item.bannerImage || DefaultImage}
          alt={item.title}
          className="absolute top-0 left-0 w-full h-full object-cover"
          onError={(e) => {
            e.currentTarget.src = DefaultImage;
          }}
        />
      </div>

      <div className="p-4 flex-1 flex flex-col overflow-hidden">
        <h3 className="text-xl font-bold text-gray-900 mb-2 line-clamp-2">
          {item.title}
        </h3>

        {item.description && (
          <p className="text-md text-gray-600 mb-2 line-clamp-8">
            {item.description}
          </p>
        )}

        {contentText && (
          <p
            className="text-xs text-gray-500 flex-1"
            style={{
              overflow: "hidden",
              textOverflow: "ellipsis",
              display: "-webkit-box",
              WebkitLineClamp: 5,
              WebkitBoxOrient: "vertical",
            }}
          >
            {contentText}
          </p>
        )}
      </div>
    </div>
  );
};

interface ComingSoonSwiperProps {
  refreshTrigger?: number;
}

const ComingSoonSwiper: React.FC<ComingSoonSwiperProps> = ({
  refreshTrigger = 0,
}) => {
  const navigate = useNavigate();
  const userToken = useRecoilValue(token);
  const phoneNumber = useRecoilValue(phoneNumberUser);
  const membershipInfo = useRecoilValue(userMembershipInfo);
  const [data, setData] = useState<ComingSoonData | null>(null);
  const [loading, setLoading] = useState(false);
  const hasFetchedRef = useRef(false);
  const initialTokenRef = useRef<string | null>(null);

  const fetchComingSoonData = async (tokenToUse?: string | null) => {
    try {
      setLoading(true);
      const headers: any = {};
      const token = tokenToUse !== undefined ? tokenToUse : userToken;
      if (token) {
        headers.Authorization = `Bearer ${token}`;
      }

      // Fetch ComingSoon data
      const comingSoonResponse = await axios.get(
        `${dfData.domain}/api/ComingSoon`,
        {
          headers,
        }
      );

      let comingSoonData: ComingSoonData = {
        events: [],
        newsletters: null,
        meetings: [],
        showcases: [],
      };

      if (comingSoonResponse.data.code === 0 && comingSoonResponse.data.data) {
        comingSoonData = comingSoonResponse.data.data;
      }

      // Fetch confirmed events only if user is pending approval (not a full member yet)
      let confirmedEvents: any[] = [];
      const phoneToUse = phoneNumber || membershipInfo?.phoneNumber;
      const isPendingApproval =
        membershipInfo?.approvalStatus === 0 ||
        membershipInfo?.approvalStatus === 2;

      if (isPendingApproval && phoneToUse && phoneToUse.trim() !== "") {
        try {
          const confirmedResponse = await axios.get(
            `${dfData.domain}/api/EventGuests/GetConfirmedEventsByPhone/${phoneToUse}`,
            { headers }
          );

          if (confirmedResponse.data && Array.isArray(confirmedResponse.data)) {
            confirmedEvents = confirmedResponse.data;
          }
        } catch (error) {
          console.error("Error fetching confirmed events:", error);
        }
      } else {
        if (!isPendingApproval) {
          console.log(
            "User is not pending approval, skipping confirmed events API"
          );
        } else {
          console.log(
            "No phone number available, skipping confirmed events API"
          );
        }
      }

      // Merge events: prioritize confirmed events and remove duplicates
      let mergedEvents = [...comingSoonData.events];

      if (confirmedEvents.length > 0) {
        // Create a map of confirmed event IDs for quick lookup
        const confirmedEventIds = new Set(confirmedEvents.map((e) => e.id));

        // Remove events from comingSoon that are already in confirmed list
        mergedEvents = mergedEvents.filter(
          (event) => !confirmedEventIds.has(event.id)
        );

        // Mark confirmed events with a flag
        const markedConfirmedEvents = confirmedEvents.map((event) => ({
          ...event,
          isConfirmedGuest: true,
        }));

        // Add confirmed events at the beginning
        mergedEvents = [...markedConfirmedEvents, ...mergedEvents];
      }

      setData({
        ...comingSoonData,
        events: mergedEvents,
      });
      hasFetchedRef.current = true;
    } catch (error) {
      console.error("Error fetching coming soon data:", error);
      // Set empty data structure on error
      setData({
        events: [],
        newsletters: null,
        meetings: [],
        showcases: [],
      });
      hasFetchedRef.current = true;
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    const currentToken = userToken || null;

    if (!hasFetchedRef.current) {
      initialTokenRef.current = currentToken;
      fetchComingSoonData(userToken);
      return;
    }

    if (initialTokenRef.current !== currentToken) {
      initialTokenRef.current = currentToken;
      fetchComingSoonData(userToken);
    }
  }, [userToken, phoneNumber, membershipInfo?.phoneNumber]);

  useEffect(() => {
    if (refreshTrigger > 0) {
      fetchComingSoonData(userToken);
    }
  }, [refreshTrigger]);

  if (loading) {
    return (
      <div className="w-full">
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-lg font-bold text-black">Sắp diễn ra</h2>
        </div>
        <div className="flex gap-4 overflow-x-auto mt-4">
          <ComingSoonSkeleton />
          <ComingSoonSkeleton />
          <ComingSoonSkeleton />
        </div>
      </div>
    );
  }

  if (!data) return null;

  const hasData =
    (data.events && data.events.length > 0) ||
    (data.meetings && data.meetings.length > 0) ||
    (data.showcases && data.showcases.length > 0) ||
    (data.newsletters &&
      (Array.isArray(data.newsletters) ? data.newsletters.length > 0 : true));

  if (!hasData) return null;

  return (
    <div className="w-full">
      <div className="flex items-center justify-between mb-4">
        <h2 className="text-lg font-bold text-black">Sắp diễn ra</h2>
        <button
          onClick={() => navigate("/giba/coming-soon")}
          className="flex items-center gap-1 text-black text-sm font-medium hover:text-gray-600 transition-colors"
        >
          Xem thêm
          <ChevronRight size={14} />
        </button>
      </div>

      <div className="flex gap-4 overflow-x-auto mt-4">
        {data.events?.map((event) => (
          <EventCard
            key={event.id}
            item={event}
            isConfirmedGuest={event.isConfirmedGuest}
          />
        ))}

        {data.meetings?.map((meeting) => (
          <MeetingCard key={meeting.id} item={meeting} />
        ))}

        {data.showcases?.map((showcase) => (
          <ShowcaseCard key={showcase.id} item={showcase} />
        ))}

        {data.newsletters && Array.isArray(data.newsletters)
          ? data.newsletters.map((newsletter) => (
              <NewsletterCard key={newsletter.id} item={newsletter} />
            ))
          : data.newsletters && <NewsletterCard item={data.newsletters} />}
      </div>
    </div>
  );
};

export default ComingSoonSwiper;
