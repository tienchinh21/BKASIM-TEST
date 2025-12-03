import React, { useState, useEffect, useCallback, useRef } from "react";
import { useNavigate, Page } from "zmp-ui";
import { useRecoilState, useRecoilValue } from "recoil";
import { useSearchParams, useLocation } from "react-router-dom";
import {
  isRegister,
  infoUser,
  phoneNumberUser,
  userMembershipInfo,
} from "../recoil/RecoilState";
import { followOA, openWebview } from "zmp-sdk/apis";
import LoadingGiba from "../componentsGiba/LoadingGiba";
import useSetHeader from "../components/hooks/useSetHeader";
import dfData from "../common/DefaultConfig.json";
import axios from "axios";
import { toast } from "react-toastify";
import { token } from "../recoil/RecoilState";
import { fixFormatPhoneNumber } from "../common/Common";
import { isFollowedOA } from "../recoil/RecoilState";

interface PageData {
  id: string;
  title: string;
  contentType: "TEXT" | "FILE";
  type: string;
  content: string;
  groupId: string | null;
  sortOrder: number;
  totalPages: number;
  currentPage: number;
}

const BehaviorRulesGiba: React.FC = () => {
  const navigate = useNavigate();
  const setHeader = useSetHeader();
  const [searchParams] = useSearchParams();
  const location = useLocation();
  const userToken = useRecoilValue(token);
  const [loading, setLoading] = useState(false);
  const [isLoggedIn, setIsLoggedIn] = useRecoilState(isRegister);
  const [infoZalo, setInfoZalo] = useRecoilState(infoUser);
  const [phoneUser] = useRecoilState(phoneNumberUser);
  const [membershipInfo, setMembershipInfo] =
    useRecoilState(userMembershipInfo);
  const [isFollowedOAState, setIsFollowedOAState] =
    useRecoilState(isFollowedOA);
  const [isAgreedTerms, setIsAgreedTerms] = useState(false);
  const [registrationData, setRegistrationData] = useState<any>(null);

  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(0);
  const [pageData, setPageData] = useState<PageData | null>(null);
  const [isLoadingPage, setIsLoadingPage] = useState(false);
  const [pageCache, setPageCache] = useState<Map<number, PageData>>(new Map());
  const fetchingPages = useRef<Set<number>>(new Set());

  const isReadOnlyMode = searchParams.get("readonly") === "true";
  const locationState = location.state as any;
  const groupIdFromState = locationState?.groupId ?? null;
  const isGroupMode = !!groupIdFromState;
  const returnToDrawer = locationState?.returnToDrawer ?? null;
  const drawerKey = locationState?.drawerKey ?? null;

  useEffect(() => {
    setHeader({
      title: isGroupMode ? "QUY TẮC NHÓM" : "QUY TẮC ỨNG XỬ",
      hasLeftIcon: true,
    });

    if (!isReadOnlyMode && !isGroupMode) {
      const savedData = sessionStorage.getItem("gibaRegistrationData");
      if (savedData) {
        setRegistrationData(JSON.parse(savedData));
      } else {
        navigate("/giba/login");
      }
    }
  }, [setHeader, navigate, isReadOnlyMode, isGroupMode]);

  const fetchPageFromAPI = useCallback(
    async (page: number): Promise<PageData | null> => {
      if (fetchingPages.current.has(page)) {
        return null;
      }

      fetchingPages.current.add(page);

      try {
        const params: any = {
          page: page,
        };

        if (isGroupMode && groupIdFromState) {
          params.type = "GROUP";
          params.groupid = groupIdFromState;
        } else {
          params.type = "APP";
        }

        const response = await axios.get(
          `${dfData.domain}/api/BehaviorRuleV2/pageInfo`,
          {
            params,
            headers: {
              Authorization: `Bearer ${userToken}`,
            },
          }
        );

        if (response.data.success && response.data.data) {
          return response.data.data;
        }
        return null;
      } catch (error) {
        console.error("Error fetching page:", error);
        return null;
      } finally {
        fetchingPages.current.delete(page);
      }
    },
    [userToken, isGroupMode, groupIdFromState]
  );

  useEffect(() => {
    const loadPage = async () => {
      if (pageCache.has(currentPage)) {
        const cachedData = pageCache.get(currentPage)!;
        setPageData(cachedData);
        if (cachedData.totalPages) {
          setTotalPages(cachedData.totalPages);
        }
        return;
      }

      try {
        setIsLoadingPage(true);
        const data = await fetchPageFromAPI(currentPage);

        if (data) {
          setPageData(data);
          setTotalPages(data.totalPages);

          setPageCache((prev) => {
            const newCache = new Map(prev);
            newCache.set(currentPage, data);
            return newCache;
          });
        } else if (!fetchingPages.current.has(currentPage)) {
          toast.error("Không có nội dung để hiển thị");
        }
      } catch (error) {
        console.error("Error loading page:", error);
        toast.error("Không thể tải nội dung. Vui lòng thử lại!");
      } finally {
        setIsLoadingPage(false);
      }
    };

    loadPage();
  }, [currentPage, fetchPageFromAPI]);

  useEffect(() => {
    const prefetchNextPages = async () => {
      if (totalPages === 0 || !pageData) return;

      const pagesToPrefetch: number[] = [];
      for (let i = 1; i <= 3; i++) {
        const nextPage = currentPage + i;
        if (
          nextPage <= totalPages &&
          !pageCache.has(nextPage) &&
          !fetchingPages.current.has(nextPage)
        ) {
          pagesToPrefetch.push(nextPage);
        }
      }

      for (const page of pagesToPrefetch) {
        const data = await fetchPageFromAPI(page);
        if (data) {
          setPageCache((prev) => {
            const newCache = new Map(prev);
            newCache.set(page, data);
            return newCache;
          });
        }
      }
    };

    const timeoutId = setTimeout(() => {
      prefetchNextPages();
    }, 300);

    return () => clearTimeout(timeoutId);
  }, [currentPage, totalPages, pageData, pageCache, fetchPageFromAPI]);

  const handleNext = () => {
    if (currentPage < totalPages) {
      setCurrentPage((prev) => prev + 1);
    }
  };

  const handlePrevious = () => {
    if (currentPage > 1) {
      setCurrentPage((prev) => prev - 1);
    }
  };

  const getFollowOA = async () => {
    const isFollow = await new Promise<boolean>((resolve) => {
      followOA({
        id: dfData.oaId,
        success: () => {
          toast.success("Cảm ơn bạn đã quan tâm OA!");
          resolve(true);
        },
        fail: (err: any) => {
          if (err.code === -203) {
            toast.error(
              "Bạn từ chối follow OA quá nhiều lần. Vui lòng tải lại trang!"
            );
          } else {
            toast.error("Không thể follow OA. Vui lòng thử lại!");
          }
          resolve(false);
        },
      });
    });

    setIsFollowedOAState(isFollow);

    return isFollow;
  };

  const handleFinalSubmit = async () => {
    if (!isFollowedOAState) {
      toast.error("Vui lòng quan tâm OA để tiếp tục!");
      return;
    }

    if (!isAgreedTerms) {
      toast.error("Vui lòng đồng ý với các điều khoản để tiếp tục!");
      return;
    }

    if (!registrationData) {
      toast.error("Không tìm thấy thông tin đăng ký. Vui lòng thử lại!");
      navigate("/giba/login");
      return;
    }

    setLoading(true);

    try {
      const { formData, isExistingUser } = registrationData;

      if (isExistingUser) {
        const profilePayload: any = {
          userZaloId:
            registrationData.infoZalo?.id || (infoZalo as any)?.id || "",
          userZaloName:
            registrationData.infoZalo?.name ||
            (infoZalo as any)?.name ||
            formData.fullname,
          userZaloIdByOA:
            formData.userZaloIdByOA || (infoZalo as any)?.idByOA || "",
          fullname: formData.fullname,
          phone: fixFormatPhoneNumber(formData.phone),
          approvalStatus: 0,
        };

        if (formData.email && formData.email.trim()) {
          profilePayload.email = formData.email;
        }

        if (registrationData.infoZalo?.avatar || (infoZalo as any)?.avatar) {
          profilePayload.zaloAvatar =
            registrationData.infoZalo?.avatar || (infoZalo as any)?.avatar;
        }

        if (formData.profile && formData.profile.trim()) {
          profilePayload.profile = formData.profile;
        }

        if (formData.dayOfBirth && formData.dayOfBirth.trim()) {
          try {
            const birthDate = new Date(formData.dayOfBirth);
            if (!isNaN(birthDate.getTime())) {
              profilePayload.dayOfBirth = birthDate.toISOString();
            }
          } catch (error) {}
        }

        if (formData.address && formData.address.trim()) {
          profilePayload.address = formData.address;
        }

        if (formData.company && formData.company.trim()) {
          profilePayload.company = formData.company;
        }

        if (formData.position && formData.position.trim()) {
          profilePayload.position = formData.position;
        }

        if (formData.fieldIds && formData.fieldIds.length > 0) {
          profilePayload.fieldIds = formData.fieldIds;
        }

        if (formData.companyFullName && formData.companyFullName.trim()) {
          profilePayload.companyFullName = formData.companyFullName;
        }

        if (formData.companyBrandName && formData.companyBrandName.trim()) {
          profilePayload.companyBrandName = formData.companyBrandName;
        }

        if (formData.taxCode && formData.taxCode.trim()) {
          profilePayload.taxCode = formData.taxCode;
        }

        if (formData.businessField && formData.businessField.trim()) {
          profilePayload.businessField = formData.businessField;
        }

        if (formData.businessType && formData.businessType.trim()) {
          profilePayload.businessType = formData.businessType;
        }

        if (
          formData.headquartersAddress &&
          formData.headquartersAddress.trim()
        ) {
          profilePayload.headquartersAddress = formData.headquartersAddress;
        }

        if (formData.companyWebsite && formData.companyWebsite.trim()) {
          profilePayload.companyWebsite = formData.companyWebsite;
        }

        if (formData.companyPhoneNumber && formData.companyPhoneNumber.trim()) {
          profilePayload.companyPhoneNumber = formData.companyPhoneNumber;
        }

        if (formData.companyEmail && formData.companyEmail.trim()) {
          profilePayload.companyEmail = formData.companyEmail;
        }

        if (
          formData.legalRepresentative &&
          formData.legalRepresentative.trim()
        ) {
          profilePayload.legalRepresentative = formData.legalRepresentative;
        }

        if (
          formData.legalRepresentativePosition &&
          formData.legalRepresentativePosition.trim()
        ) {
          profilePayload.legalRepresentativePosition =
            formData.legalRepresentativePosition;
        }

        if (formData.companyLogoFile) {
          // For existing user, we might need to handle logo differently
          // For now, skip it as profile API might not accept file upload
        }

        if (
          formData.businessRegistrationNumber &&
          formData.businessRegistrationNumber.trim()
        ) {
          profilePayload.businessRegistrationNumber =
            formData.businessRegistrationNumber;
        }

        if (
          formData.businessRegistrationDate &&
          formData.businessRegistrationDate.trim()
        ) {
          try {
            const regDate = new Date(formData.businessRegistrationDate);
            if (!isNaN(regDate.getTime())) {
              profilePayload.businessRegistrationDate = regDate.toISOString();
            }
          } catch (error) {}
        }

        if (
          formData.businessRegistrationPlace &&
          formData.businessRegistrationPlace.trim()
        ) {
          profilePayload.businessRegistrationPlace =
            formData.businessRegistrationPlace;
        }

        if (formData.message && formData.message.trim()) {
          profilePayload.message = formData.message;
        }

        if (formData.reason && formData.reason.trim()) {
          profilePayload.reason = formData.reason;
        }

        if (formData.object && formData.object.trim()) {
          profilePayload.object = formData.object;
        }

        if (formData.contribute && formData.contribute.trim()) {
          profilePayload.contribute = formData.contribute;
        }

        if (formData.careAbout && formData.careAbout.trim()) {
          profilePayload.careAbout = formData.careAbout;
        }

        if (formData.otherContribute && formData.otherContribute.trim()) {
          profilePayload.otherContribute = formData.otherContribute;
        }

        const response = await axios.put(
          `${dfData.domain}/api/memberships/profile`,
          profilePayload,
          {
            headers: {
              "Content-Type": "application/json",
            },
          }
        );

        if (response.data.success) {
          toast.success(
            response.data.message || "Cập nhật thông tin thành công!"
          );
          setIsLoggedIn(true);

          const membership = response.data.data || {};
          const phoneNumber =
            membership.phoneNumber ||
            fixFormatPhoneNumber(formData.phone) ||
            phoneUser ||
            "";

          console.log(
            "BehaviorRulesGiba: Setting membershipInfo after update profile",
            {
              phoneNumber,
              membershipPhone: membership.phoneNumber,
              formDataPhone: formData.phone,
              phoneUser,
            }
          );

          setMembershipInfo({
            id: membership.id || "",
            userZaloId: membership.userZaloId || (infoZalo as any)?.id || "",
            phoneNumber: phoneNumber,
            fullname: membership.fullname || (infoZalo as any)?.name || "",
            approvalStatus:
              membership.approvalStatus !== undefined
                ? membership.approvalStatus
                : 1,
            idByOA: membership.idByOA || (infoZalo as any)?.idByOA || "",
          });

          const userInfoForDisplay = {
            id: membership.userZaloId || (infoZalo as any)?.id,
            name: membership.fullname || (infoZalo as any)?.name || "User",
            avatar: membership.zaloAvatar || (infoZalo as any)?.avatar || null,
            idByOA: membership.idByOA || (infoZalo as any)?.idByOA || "",
          };
          setInfoZalo(userInfoForDisplay);
          setIsFollowedOAState(true); // Đảm bảo isFollowedOA = true trước khi navigate

          sessionStorage.removeItem("gibaRegistrationData");

          navigate("/giba", { replace: true, state: { refresh: true } });
        } else {
          console.error("Update profile failed:", response.data);
          toast.error(response.data.message || "Cập nhật thông tin thất bại!");
        }
      } else {
        // Original registration flow for new users
        const registrationFormData = new FormData();

        registrationFormData.append(
          "UserZaloId",
          registrationData.infoZalo?.id || (infoZalo as any)?.id || ""
        );
        registrationFormData.append(
          "UserZaloName",
          registrationData.infoZalo?.name ||
            (infoZalo as any)?.name ||
            formData.fullname
        );
        registrationFormData.append("Fullname", formData.fullname);
        registrationFormData.append(
          "PhoneNumber",
          fixFormatPhoneNumber(formData.phone)
        );

        if (formData.email && formData.email.trim()) {
          registrationFormData.append("Email", formData.email);
        }

        if (registrationData.infoZalo?.avatar || (infoZalo as any)?.avatar) {
          registrationFormData.append(
            "ZaloAvatar",
            registrationData.infoZalo?.avatar || (infoZalo as any)?.avatar
          );
        }

        if (formData.profile && formData.profile.trim()) {
          registrationFormData.append("Profile", formData.profile);
        }

        if (formData.dayOfBirth && formData.dayOfBirth.trim()) {
          try {
            const birthDate = new Date(formData.dayOfBirth);
            if (!isNaN(birthDate.getTime())) {
              registrationFormData.append(
                "DayOfBirth",
                birthDate.toISOString()
              );
            }
          } catch (error) {}
        }

        if (formData.address && formData.address.trim()) {
          registrationFormData.append("Address", formData.address);
        }

        if (formData.company && formData.company.trim()) {
          registrationFormData.append("Company", formData.company);
        }

        if (formData.position && formData.position.trim()) {
          registrationFormData.append("Position", formData.position);
        }

        if (formData.gender && formData.gender.trim()) {
          registrationFormData.append("Gender", formData.gender);
        }

        if (formData.fieldIds && formData.fieldIds.length > 0) {
          registrationFormData.append("FieldIds", formData.fieldIds.join(","));
        }

        registrationFormData.append(
          "CompanyFullName",
          formData.companyFullName
        );
        registrationFormData.append("TaxCode", formData.taxCode);
        registrationFormData.append("BusinessField", formData.businessField);
        registrationFormData.append(
          "HeadquartersAddress",
          formData.headquartersAddress
        );
        registrationFormData.append(
          "CompanyPhoneNumber",
          formData.companyPhoneNumber
        );
        registrationFormData.append(
          "LegalRepresentative",
          formData.legalRepresentative
        );
        registrationFormData.append(
          "BusinessRegistrationNumber",
          formData.businessRegistrationNumber
        );

        if (
          formData.businessRegistrationDate &&
          formData.businessRegistrationDate.trim()
        ) {
          try {
            const regDate = new Date(formData.businessRegistrationDate);
            if (!isNaN(regDate.getTime())) {
              registrationFormData.append(
                "BusinessRegistrationDate",
                regDate.toISOString()
              );
            }
          } catch (error) {}
        }

        registrationFormData.append(
          "BusinessRegistrationPlace",
          formData.businessRegistrationPlace
        );

        if (formData.companyBrandName && formData.companyBrandName.trim()) {
          registrationFormData.append(
            "CompanyBrandName",
            formData.companyBrandName
          );
        }

        if (formData.businessType && formData.businessType.trim()) {
          registrationFormData.append("BusinessType", formData.businessType);
        }

        if (formData.companyWebsite && formData.companyWebsite.trim()) {
          registrationFormData.append(
            "CompanyWebsite",
            formData.companyWebsite
          );
        }

        if (formData.companyEmail && formData.companyEmail.trim()) {
          registrationFormData.append("CompanyEmail", formData.companyEmail);
        }

        if (
          formData.legalRepresentativePosition &&
          formData.legalRepresentativePosition.trim()
        ) {
          registrationFormData.append(
            "LegalRepresentativePosition",
            formData.legalRepresentativePosition
          );
        }

        if (formData.companyLogoFile) {
          if (formData.companyLogoFile instanceof File) {
            registrationFormData.append(
              "CompanyLogoFile",
              formData.companyLogoFile
            );
          }
        }

        // Add message fields
        if (formData.message && formData.message.trim()) {
          registrationFormData.append("Message", formData.message);
        }

        if (formData.reason && formData.reason.trim()) {
          registrationFormData.append("Reason", formData.reason);
        }

        if (formData.object && formData.object.trim()) {
          registrationFormData.append("Object", formData.object);
        }

        if (formData.contribute && formData.contribute.trim()) {
          registrationFormData.append("Contribute", formData.contribute);
        }

        if (formData.careAbout && formData.careAbout.trim()) {
          registrationFormData.append("CareAbout", formData.careAbout);
        }

        if (formData.otherContribute && formData.otherContribute.trim()) {
          registrationFormData.append(
            "OtherContribute",
            formData.otherContribute
          );
        }

        // Add userZaloIdByOA if available
        if (formData.userZaloIdByOA && formData.userZaloIdByOA.trim()) {
          registrationFormData.append(
            "UserZaloIdByOA",
            formData.userZaloIdByOA
          );
        }

        const response = await axios.post(
          `${dfData.domain}/api/memberships/register`,
          registrationFormData,
          {
            headers: {},
          }
        );

        if (response.data.success) {
          toast.success(response.data.data.message || "Đăng ký thành công!");
          setIsLoggedIn(true);

          const membership = response.data.data.membership || {};
          const phoneNumber =
            membership.phoneNumber ||
            fixFormatPhoneNumber(formData.phone) ||
            phoneUser ||
            "";

          console.log(
            "BehaviorRulesGiba: Setting membershipInfo after registration",
            {
              phoneNumber,
              membershipPhone: membership.phoneNumber,
              formDataPhone: formData.phone,
              phoneUser,
            }
          );

          setMembershipInfo({
            id: membership.id || "",
            userZaloId: membership.userZaloId || (infoZalo as any)?.id || "",
            phoneNumber: phoneNumber,
            fullname: membership.fullname || (infoZalo as any)?.name || "",
            approvalStatus:
              membership.approvalStatus !== undefined
                ? membership.approvalStatus
                : 0,
            idByOA: membership.idByOA || (infoZalo as any)?.idByOA || "",
          });

          const userInfoForDisplay = {
            id: membership.userZaloId || (infoZalo as any)?.id,
            name: membership.fullname || (infoZalo as any)?.name || "User",
            avatar: membership.zaloAvatar || (infoZalo as any)?.avatar || null,
            idByOA: membership.idByOA || (infoZalo as any)?.idByOA || "",
          };
          setInfoZalo(userInfoForDisplay);
          setIsFollowedOAState(true); // Đảm bảo isFollowedOA = true trước khi navigate

          sessionStorage.removeItem("gibaRegistrationData");

          navigate("/giba", { replace: true, state: { refresh: true } });
        } else {
          console.error("Registration failed:", response.data);
          toast.error(response.data.message || "Đăng ký thất bại!");
        }
      }
    } catch (error: any) {
      const errorMessage =
        error.response?.data?.message ||
        error.message ||
        "Đăng ký thất bại. Vui lòng thử lại!";
      toast.error(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  const renderProgressBar = () => {
    if (totalPages === 0) return null;

    const percentage = Math.round((currentPage / totalPages) * 100);

    return (
      <div className="mb-6">
        <div className="flex justify-between items-center mb-2">
          <span className="text-white text-sm font-medium">Quy tắc ứng xử</span>
          <span className="text-gray-300 text-sm">
            {currentPage}/{totalPages}
          </span>
        </div>
        <div className="w-full bg-gray-700 rounded-full h-2 overflow-hidden">
          <div
            className="bg-white h-2 rounded-full transition-all duration-300"
            style={{ width: `${percentage}%` }}
          />
        </div>
        <div className="text-center mt-2">
          <span className="text-gray-400 text-xs">
            {percentage}% Hoàn thành
          </span>
        </div>
      </div>
    );
  };

  const renderContent = () => {
    if (!pageData) {
      return (
        <div className="text-center py-12 text-gray-400">
          <p>Không có nội dung để hiển thị</p>
        </div>
      );
    }

    if (pageData.contentType === "TEXT") {
      return (
        <div className="bg-white rounded-lg p-6">
          <h2 className="text-xl font-bold mb-4 text-gray-900">
            {pageData.title}
          </h2>
          <div
            className="prose prose-sm max-w-none text-gray-800"
            dangerouslySetInnerHTML={{ __html: pageData.content }}
          />
        </div>
      );
    }

    if (pageData.contentType === "FILE") {
      const fullUrl = pageData.content.startsWith("http")
        ? pageData.content
        : `${dfData.domain}${pageData.content}`;

      const isImage = /\.(jpg|jpeg|png|gif|webp|bmp|svg)$/i.test(fullUrl);
      const isPDF = /\.pdf$/i.test(fullUrl);

      if (isImage) {
        return (
          <div className="bg-white rounded-lg overflow-hidden w-full">
            <div className="bg-gray-100 px-4 py-3 border-b border-gray-200">
              <h2 className="text-lg font-bold text-gray-900">
                {pageData.title}
              </h2>
            </div>
            <div className="relative w-full p-4">
              <img
                src={fullUrl}
                alt={pageData.title}
                className="w-full h-auto rounded"
                loading="lazy"
              />
            </div>
          </div>
        );
      }

      const viewerUrl = `https://docs.google.com/viewer?url=${encodeURIComponent(
        fullUrl
      )}&embedded=true`;

      return (
        <div className="bg-white rounded-lg overflow-hidden w-full">
          <div className="bg-gray-100 px-4 py-3 border-b border-gray-200">
            <h2 className="text-lg font-bold text-gray-900">
              {pageData.title}
            </h2>
          </div>
          <div className="relative w-full" style={{ height: "75vh" }}>
            <iframe
              src={viewerUrl}
              className="w-full h-full border-0"
              title={pageData.title}
              loading="lazy"
            />
            {/* Overlay để chặn nút "Open in new window" của Google Docs Viewer */}
            <div
              className="absolute top-0 right-0 w-16 h-16 z-10"
              style={{ pointerEvents: "auto" }}
              onClick={(e) => e.preventDefault()}
            />
          </div>
        </div>
      );
    }

    return null;
  };

  return (
    <Page className="bg-black min-h-screen">
      <div
        className={`${
          pageData?.contentType === "FILE"
            ? "w-full px-2"
            : "max-w-2xl mx-auto px-4"
        } py-6 mt-[60px] ${isReadOnlyMode || isGroupMode ? "pb-6" : "pb-32"}`}
      >
        {loading ? (
          <div className="flex justify-center items-center min-h-screen">
            <LoadingGiba size="lg" text="Đang đăng ký..." />
          </div>
        ) : (
          <>
            {renderProgressBar()}

            <div className="bg-gray-900 rounded-lg mb-6">
              {isLoadingPage ? (
                <div className="flex justify-center items-center py-12">
                  <LoadingGiba size="md" text="Đang tải nội dung..." />
                </div>
              ) : (
                renderContent()
              )}
            </div>

            {!isReadOnlyMode && !isGroupMode && currentPage === totalPages && (
              <>
                <div className="mb-6">
                  <div
                    className="flex items-start gap-3 p-4 bg-gray-900 border border-gray-700 rounded-lg cursor-pointer hover:bg-gray-800 transition-colors"
                    onClick={() => setIsAgreedTerms(!isAgreedTerms)}
                  >
                    <div className="flex-shrink-0 mt-0.5">
                      <div
                        className={`w-5 h-5 rounded border-2 flex items-center justify-center transition-all ${
                          isAgreedTerms
                            ? "bg-white border-white"
                            : "border-gray-500 bg-transparent"
                        }`}
                      >
                        {isAgreedTerms && (
                          <svg
                            className="w-3 h-3 text-black"
                            fill="none"
                            viewBox="0 0 24 24"
                            stroke="currentColor"
                          >
                            <path
                              strokeLinecap="round"
                              strokeLinejoin="round"
                              strokeWidth={3}
                              d="M5 13l4 4L19 7"
                            />
                          </svg>
                        )}
                      </div>
                    </div>
                    <div className="flex-1">
                      <p className="text-sm text-gray-300 leading-relaxed">
                        Tôi đã đọc, hiểu và đồng ý tuân thủ{" "}
                        <span className="text-white font-semibold">
                          Thông tin & Quy tắc ứng xử
                        </span>{" "}
                        của GIBA{" "}
                        <span className="text-red-500 font-bold">*</span>
                      </p>
                    </div>
                  </div>
                </div>

                <div className="mb-6">
                  <div
                    className="flex items-start gap-3 p-4 bg-gray-900 border border-gray-700 rounded-lg cursor-pointer hover:bg-gray-800 transition-colors"
                    onClick={getFollowOA}
                  >
                    <div className="flex-shrink-0 mt-0.5">
                      <div
                        className={`w-5 h-5 rounded border-2 flex items-center justify-center transition-all ${
                          isFollowedOAState
                            ? "bg-white border-white"
                            : "border-gray-500 bg-transparent"
                        }`}
                      >
                        {isFollowedOAState && (
                          <svg
                            className="w-3 h-3 text-black"
                            fill="none"
                            viewBox="0 0 24 24"
                            stroke="currentColor"
                          >
                            <path
                              strokeLinecap="round"
                              strokeLinejoin="round"
                              strokeWidth={3}
                              d="M5 13l4 4L19 7"
                            />
                          </svg>
                        )}
                      </div>
                    </div>
                    <div className="flex-1">
                      <p className="text-sm text-gray-300 leading-relaxed">
                        Tôi đồng ý{" "}
                        <span className="text-white font-semibold">
                          Theo dõi Zalo OA {dfData.oaName || "GIBA"}
                        </span>{" "}
                        để nhận thông báo và cập nhật mới nhất{" "}
                        <span className="text-red-500 font-bold">*</span>
                      </p>
                    </div>
                  </div>
                </div>
              </>
            )}
          </>
        )}
      </div>
      <div className="fixed bottom-0 left-0 right-0 bg-black border-t border-gray-700 px-4 py-4">
        <div className="flex gap-3">
          <button
            onClick={handlePrevious}
            disabled={currentPage === 1}
            className={`flex items-center justify-center w-14 h-12 rounded-lg transition-all duration-200 shadow-lg ${
              currentPage === 1
                ? "bg-gray-800 text-gray-500 cursor-not-allowed"
                : "bg-gray-800 text-white hover:bg-gray-700"
            }`}
          >
            <svg
              className="w-6 h-6"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M15 19l-7-7 7-7"
              />
            </svg>
          </button>

          {!isReadOnlyMode && !isGroupMode && currentPage === totalPages ? (
            <button
              onClick={handleFinalSubmit}
              disabled={!isAgreedTerms || !isFollowedOAState}
              className={`flex-1 py-3 rounded-lg font-bold text-base transition-all duration-200 shadow-lg ${
                isAgreedTerms && isFollowedOAState
                  ? "bg-white text-black hover:bg-gray-200"
                  : "bg-gray-700 text-gray-400 cursor-not-allowed"
              }`}
            >
              Đăng ký thành viên
            </button>
          ) : (isReadOnlyMode || isGroupMode) && currentPage === totalPages ? (
            <button
              onClick={() => {
                // If returning to drawer, don't remove the state key
                // The drawer will handle it when it opens
                navigate(-1);
              }}
              className="flex-1 py-3 rounded-lg font-bold text-base transition-all duration-200 shadow-lg bg-white text-black hover:bg-gray-200"
            >
              Hoàn thành
            </button>
          ) : (
            <button
              onClick={handleNext}
              disabled={currentPage === totalPages}
              className={`flex-1 py-3 rounded-lg font-bold text-base transition-all duration-200 shadow-lg ${
                currentPage === totalPages
                  ? "bg-gray-700 text-gray-400 cursor-not-allowed"
                  : "bg-white text-black hover:bg-gray-200"
              }`}
            >
              Tiếp theo
            </button>
          )}
        </div>
      </div>
    </Page>
  );
};

export default BehaviorRulesGiba;
