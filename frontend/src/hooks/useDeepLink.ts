import { useEffect } from "react";
import { useNavigate, useLocation } from "react-router-dom";

export const useDeepLink = () => {
  const navigate = useNavigate();
  const location = useLocation();

  useEffect(() => {
    const handleDeepLink = () => {
      try {
        const currentPath = location.pathname;
        const currentSearch = location.search;
          
        const searchParams = new URLSearchParams(currentSearch);
        const profileSlug = searchParams.get("profileSlug");
        const groupId = searchParams.get("groupId");

        // Skip redirect if already on member detail page
        if (currentPath.includes("/giba/member-detail/")) {
          console.log("✅ Already on member detail page - no redirect needed");
          return;
        }

        // Skip redirect if on PublicProfileGiba page (deprecated but keep for safety)
        if (currentPath.includes("/giba/profile/")) {
          console.log("✅ On public profile page - no redirect needed");
          return;
        }

        if (profileSlug && !currentPath.includes("/giba/member-detail/")) {
          const newSearch = new URLSearchParams();
          if (groupId) newSearch.set("groupId", groupId);

          const newUrl = newSearch.toString()
            ? `/giba/member-detail/${profileSlug}?${newSearch.toString()}`
            : `/giba/member-detail/${profileSlug}`;

          navigate(newUrl, { replace: true });
          return;
        }

        // Log if already on correct page
        if (currentPath.includes("/giba/member-detail/")) {
          console.log("✅ Already on member detail page");
        } else if (currentPath.includes("/giba/group-detail/")) {
          console.log("✅ Already on group detail page");
        }
      } catch (error) {
        console.error("❌ Error handling deep link:", error);
      }
    };

    // Handle deep link on component mount and when location changes
    handleDeepLink();
  }, [location.pathname, location.search, navigate]);

  return null;
};

export default useDeepLink;
