import React from "react";
import {
  BrowserRouter as Router,
  Route,
  Navigate,
  Routes,
} from "react-router-dom";
import { App, ZMPRouter, AnimationRoutes, SnackbarProvider } from "zmp-ui";
import { RecoilRoot } from "recoil";
import { ToastContainer } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";
import useDeepLink from "../hooks/useDeepLink";

import HomeGiba from "../pagesGiba/home/HomeGiba";
import GroupDetailGiba from "../pagesGiba/GroupDetailGiba";
import GroupsListGiba from "../pagesGiba/GroupsListGiba";
import LoginGiba from "../pagesGiba/LoginGiba";
import AppBriefGiba from "../pagesGiba/AppBriefGiba";
import BehaviorRulesGiba from "../pagesGiba/BehaviorRulesGiba";
import MemberGiba from "../pagesGiba/MemberGiba";
import MemberDetailGiba from "../pagesGiba/MemberDetailGiba";
import PublicProfileGiba from "../pagesGiba/PublicProfileGiba";
import EventRegistrationHistory from "../pagesGiba/EventRegistrationHistory";
import EventRegistrationDetail from "../pagesGiba/EventRegistrationDetail";
import RefListGiba from "../pagesGiba/RefListGiba";
import RefDetailGiba from "../pagesGiba/RefDetailGiba";
import EditInfoGiba from "../pagesGiba/EditInfoGiba";
import GuestListHistory from "../pagesGiba/GuestListHistory";
import GuestListDetail from "../pagesGiba/GuestListDetail";
import GroupJoinRequestHistory from "../pagesGiba/GroupJoinRequestHistory";
import GroupJoinRequestDetail from "../pagesGiba/GroupJoinRequestDetail";
import DashboardGiba from "../pagesGiba/DashboardGiba";
import ProfileIntroGiba from "../pagesGiba/ProfileIntroGiba";
import ProfileViewGiba from "../pagesGiba/ProfileViewGiba";
import Contact from "../pagesGiba/contact/Contact";
import ManagerArticles from "../pagesGiba/managerArticles/managerArticles";
import ManagerMeetings from "../pagesGiba/managerMeetings/managerMeetings";
import ManagerTraining from "../pagesGiba/managerTraining/managerTraining";
import ManagerClub from "../pagesGiba/managerClub/managerClub";
import BottomNavigationPage from "./BottomNavigate";

import HeaderCustom from "./layout/Header";

import PrivateRoute from "./layout/PrirateRoute";
import EventDetail from "../pagesGiba/event/event-detail";
import EventCheckIn from "../pages/event/EventCheckIn";
import SuccessJoinEvent from "../pages/event/SuccessJoinEvent";
import DetailNew from "../pagesGiba/news/detailNew";
import News from "../pagesGiba/news";
import Train from "../pagesGiba/train/Train";
import MeetingList from "../pagesGiba/meetingSchedule/MeetingList";
import MeetingCreate from "../pagesGiba/meetingSchedule/MeetingCreate";
import MeetingDetail from "../pagesGiba/meetingSchedule/MeetingDetail";
import RefCreate from "../pagesGiba/ref/Ref";
import AppointmentList from "../pagesGiba/appointment/AppointmentList";
import AppointmentCreate from "../pagesGiba/appointment/AppointmentCreate";
import AppointmentDetail from "../pagesGiba/appointment/AppointmentDetail";
import ShowcaseList from "../pagesGiba/showcase/ShowcaseList";
import ShowcaseCreate from "../pagesGiba/showcase/ShowcaseCreate";
import ShowcaseDetail from "../pagesGiba/showcase/ShowcaseDetail";
import Event from "../pagesGiba/event/index";
import EventRegisterPage from "../pagesGiba/event/EventRegisterPage";
import EventGuestRegisterPage from "../pagesGiba/event/EventGuestRegisterPage";
import ManagerMembership from "../pagesGiba/managerMembership/managerMembersip";
import Achievements from "../pagesGiba/achievements/Achievements";
import ProfileMembership from "../pagesGiba/profilemembership/ProfileMemberships";
import ComingSoonPage from "../pagesGiba/comingSoon/ComingSoonPage";
import GroupRegisterPage from "../pagesGiba/GroupRegisterPage";

// Component to handle deep links
const DeepLinkHandler = () => {
  useDeepLink();
  return null;
};

const MyApp = () => {
  return (
    <RecoilRoot>
      <App>
        <SnackbarProvider>
          <ZMPRouter>
            <DeepLinkHandler />
            <HeaderCustom></HeaderCustom>
            <Routes>
              {/* ====== GIBA ROUTES - NEW MINI APP ====== */}
              {["/s/:sessionId/", "/", "/zapps/:sessionId/"].map((path) => (
                <Route
                  key={path}
                  path={path}
                  element={
                    <PrivateRoute>
                      <HomeGiba />
                    </PrivateRoute>
                  }
                />
              ))}

              <Route path="/giba" element={<HomeGiba />} />

              <Route path="/giba/news" element={<News />} />
              <Route path="/giba/news-detail/:id" element={<DetailNew />} />

              {/* Giba coming soon page */}
              <Route path="/giba/coming-soon" element={<ComingSoonPage />} />

              <Route
                path="/giba/groups"
                element={
                  <PrivateRoute>
                    <GroupsListGiba />
                  </PrivateRoute>
                }
              />

              {/* Giba meeting list page */}
              <Route
                path="/giba/meeting-list"
                element={
                  <PrivateRoute>
                    <MeetingList />
                  </PrivateRoute>
                }
              />

              {/* Giba meeting create page */}
              <Route
                path="/giba/meeting-create"
                element={
                  <PrivateRoute>
                    <MeetingCreate />
                  </PrivateRoute>
                }
              />

              {/* Giba meeting detail page */}
              <Route
                path="/giba/meeting-detail"
                element={
                  <PrivateRoute>
                    <MeetingDetail />
                  </PrivateRoute>
                }
              />

              {/* Giba profile membership page */}
              <Route
                path="/giba/profile-membership"
                element={
                  <PrivateRoute>
                    <ProfileMembership />
                  </PrivateRoute>
                }
              />

              {/* Giba ref create page */}
              <Route
                path="/giba/ref-create"
                element={
                  <PrivateRoute>
                    <RefCreate />
                  </PrivateRoute>
                }
              />

              {/* Giba appointment list page */}
              <Route
                path="/giba/appointment-list"
                element={
                  <PrivateRoute>
                    <AppointmentList />
                  </PrivateRoute>
                }
              />

              {/* Giba appointment create page */}
              <Route
                path="/giba/appointment-create"
                element={
                  <PrivateRoute>
                    <AppointmentCreate />
                  </PrivateRoute>
                }
              />

              {/* Giba appointment detail page */}
              <Route
                path="/giba/appointment-detail"
                element={
                  <PrivateRoute>
                    <AppointmentDetail />
                  </PrivateRoute>
                }
              />

              {/* Giba showcase list page */}
              <Route
                path="/giba/showcase-list"
                element={
                  <PrivateRoute>
                    <ShowcaseList />
                  </PrivateRoute>
                }
              />

              {/* Giba showcase create page */}
              <Route
                path="/giba/showcase-create"
                element={
                  <PrivateRoute>
                    <ShowcaseCreate />
                  </PrivateRoute>
                }
              />

              {/* Giba showcase detail page */}
              <Route
                path="/giba/showcase-detail"
                element={
                  <PrivateRoute>
                    <ShowcaseDetail />
                  </PrivateRoute>
                }
              />

              {/* Giba contact page */}
              <Route
                path="/giba/contact"
                element={
                  <PrivateRoute>
                    <Contact />
                  </PrivateRoute>
                }
              />
              <Route
                path="/giba/train"
                element={
                  <PrivateRoute>
                    <Train />
                  </PrivateRoute>
                }
              />
              <Route
                path="/giba/event"
                element={
                  <PrivateRoute>
                    <Event />
                  </PrivateRoute>
                }
              />
              {/* Giba group detail */}
              <Route
                path="/giba/group-detail/:id"
                element={
                  <PrivateRoute>
                    <GroupDetailGiba />
                  </PrivateRoute>
                }
              />

              {/* Giba group register page */}
              <Route
                path="/giba/group-register"
                element={
                  <PrivateRoute>
                    <GroupRegisterPage />
                  </PrivateRoute>
                }
              />

              {/* Giba login fallback */}
              <Route path="/giba/login" element={<LoginGiba />} />

              {/* Giba app brief */}
              <Route path="/giba/app-brief" element={<AppBriefGiba />} />

              {/* Giba behavior rules */}
              <Route
                path="/giba/behavior-rules"
                element={<BehaviorRulesGiba />}
              />

              {/* Giba member page */}
              <Route
                path="/giba/member"
                element={
                  <PrivateRoute>
                    <MemberGiba />
                  </PrivateRoute>
                }
              />

              {/* Giba manager club page */}
              <Route
                path="/giba/manager-club"
                element={
                  <PrivateRoute>
                    <ManagerClub />
                  </PrivateRoute>
                }
              />

              {/* Giba member detail page */}
              <Route
                path="/giba/member-detail/:userZaloId"
                element={
                  <PrivateRoute>
                    <MemberDetailGiba />
                  </PrivateRoute>
                }
              />

              {/* Giba public profile page (shared link) */}
              <Route
                path="/giba/profile/:slug"
                element={
                  <PrivateRoute>
                    <PublicProfileGiba />
                  </PrivateRoute>
                }
              />

              {/* Giba event registration history */}
              <Route
                path="/giba/event-registration-history"
                element={
                  <PrivateRoute>
                    <EventRegistrationHistory />
                  </PrivateRoute>
                }
              />

              {/* Giba event registration detail */}
              <Route
                path="/giba/event-registration-detail"
                element={
                  <PrivateRoute>
                    <EventRegistrationDetail />
                  </PrivateRoute>
                }
              />

              {/* Giba ref list */}
              <Route
                path="/giba/ref-list"
                element={
                  <PrivateRoute>
                    <RefListGiba />
                  </PrivateRoute>
                }
              />

              {/* Giba ref detail */}
              <Route
                path="/giba/ref-detail"
                element={
                  <PrivateRoute>
                    <RefDetailGiba />
                  </PrivateRoute>
                }
              />

              {/* Giba edit info */}
              <Route
                path="/giba/edit-info"
                element={
                  <PrivateRoute>
                    <EditInfoGiba />
                  </PrivateRoute>
                }
              />

              {/* Giba guest list history */}
              <Route
                path="/giba/guest-list-history"
                element={
                  <PrivateRoute>
                    <GuestListHistory />
                  </PrivateRoute>
                }
              />

              {/* Giba guest list detail */}
              <Route
                path="/giba/guest-list-detail"
                element={
                  <PrivateRoute>
                    <GuestListDetail />
                  </PrivateRoute>
                }
              />

              {/* Giba group join request history */}
              <Route
                path="/giba/group-join-request-history"
                element={
                  <PrivateRoute>
                    <GroupJoinRequestHistory />
                  </PrivateRoute>
                }
              />

              {/* Giba group join request detail */}
              <Route
                path="/giba/group-join-request-detail/:groupId"
                element={
                  <PrivateRoute>
                    <GroupJoinRequestDetail />
                  </PrivateRoute>
                }
              />

              {/* Giba dashboard page */}
              <Route
                path="/giba/dashboard"
                element={
                  <PrivateRoute>
                    <DashboardGiba />
                  </PrivateRoute>
                }
              />

              {/* Giba profile intro page */}
              <Route
                path="/giba/profile-intro"
                element={
                  <PrivateRoute>
                    <ProfileIntroGiba />
                  </PrivateRoute>
                }
              />

              {/* Giba profile view page (public) */}
              <Route
                path="/giba/profile/:userZaloId"
                element={<ProfileViewGiba />}
              />

              {/* Event detail page */}
              <Route
                path="/giba/event-detail/:id"
                element={
                  <PrivateRoute>
                    <EventDetail />
                  </PrivateRoute>
                }
              />

              {/* Event register page */}
              <Route
                path="/giba/event-register"
                element={
                  <PrivateRoute>
                    <EventRegisterPage />
                  </PrivateRoute>
                }
              />

              {/* Event guest register page */}
              <Route
                path="/giba/event-guest-register"
                element={
                  <PrivateRoute>
                    <EventGuestRegisterPage />
                  </PrivateRoute>
                }
              />

              {/* Manager Membership page */}
              <Route
                path="/giba/manager-membership"
                element={
                  <PrivateRoute>
                    <ManagerMembership />
                  </PrivateRoute>
                }
              />

              {/* Manager Articles page */}
              <Route
                path="/giba/manager-articles"
                element={
                  <PrivateRoute>
                    <ManagerArticles />
                  </PrivateRoute>
                }
              />

              {/* Manager Meetings page */}
              <Route
                path="/giba/manager-meetings"
                element={
                  <PrivateRoute>
                    <MeetingList />
                  </PrivateRoute>
                }
              />

              {/* Manager Training page */}
              <Route
                path="/giba/manager-training"
                element={
                  <PrivateRoute>
                    <ManagerTraining />
                  </PrivateRoute>
                }
              />

              {/* Success join event page */}
              <Route
                path="/event/success-join"
                element={<SuccessJoinEvent />}
              />

              {/* Achievements page */}
              <Route
                path="/giba/achievements"
                element={
                  <PrivateRoute>
                    <Achievements />
                  </PrivateRoute>
                }
              />

              <Route path="/check-in" element={<EventCheckIn />} />
              <Route path="*" element={<Navigate to="/giba" replace />} />
            </Routes>

            <BottomNavigationPage></BottomNavigationPage>
            <ToastContainer
              position="top-right"
              autoClose={2000}
              hideProgressBar={false}
              newestOnTop={false}
              closeOnClick
              rtl={false}
              pauseOnFocusLoss
              draggable
              pauseOnHover
              theme="light"
              limit={1}
            />
          </ZMPRouter>
        </SnackbarProvider>
      </App>
    </RecoilRoot>
  );
};

export default MyApp;
