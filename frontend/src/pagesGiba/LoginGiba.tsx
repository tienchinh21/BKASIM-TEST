import React, { useState, useEffect, useRef } from "react";
import { useNavigate, Page } from "zmp-ui";
import { useRecoilState, useRecoilValue } from "recoil";
import {
  isRegister,
  infoUser,
  phoneNumberUser,
  token,
} from "../recoil/RecoilState";
import {
  getUserInfo,
  getPhoneNumber,
  getAccessToken,
  followOA,
} from "zmp-sdk/apis";
import { Select, DatePicker } from "antd";
import dayjs from "dayjs";
import LoadingGiba from "../componentsGiba/LoadingGiba";
import useSetHeader from "../components/hooks/useSetHeader";
import dfData from "../common/DefaultConfig.json";
import axios from "axios";
import { toast } from "react-toastify";

const LoginGiba: React.FC = () => {
  const navigate = useNavigate();
  const setHeader = useSetHeader();
  const hasInitialized = useRef(false); // Track if component has been initialized
  const [loading, setLoading] = useState(false);
  const [isLoadingZaloInfo, setIsLoadingZaloInfo] = useState(false);
  const [isLoggedIn, setIsLoggedIn] = useRecoilState(isRegister);
  const [infoZalo, setInfoZalo] = useRecoilState(infoUser);
  const [phoneUser, setPhoneUser] = useRecoilState(phoneNumberUser);
  const [isFollowedOA, setIsFollowedOA] = useState(false);
  const [activeTab, setActiveTab] = useState<
    "personal" | "company" | "message"
  >("personal");
  const [validationErrors, setValidationErrors] = useState<
    Record<string, string>
  >({});
  const [isExistingUser, setIsExistingUser] = useState(false);

  const [formData, setFormData] = useState({
    // === THÔNG TIN CÁ NHÂN ===
    fullname: "",
    position: "",
    dayOfBirth: "", // Năm sinh
    gender: "",
    address: "",
    phone: "",
    email: "",
    userZaloIdByOA: "",

    // Removed fields (commented for reference)
    // profile: "",
    // fieldIds: [] as string[],
    // company: "",

    // === THÔNG TIN DOANH NGHIỆP ===
    companyLogoFile: null as File | null,
    companyFullName: "", // Tên doanh nghiệp
    taxCode: "", // Mã số thuế
    businessField: "", // Ngành nghề kinh doanh chính
    headquartersAddress: "", // Địa chỉ doanh nghiệp
    companyPhoneNumber: "", // Điện thoại doanh nghiệp
    companyEmail: "", // Email doanh nghiệp
    companyWebsite: "", // Website doanh nghiệp

    // Removed fields (commented for reference)
    // companyBrandName: "",
    // businessType: "",
    // legalRepresentative: "",
    // legalRepresentativePosition: "",
    // businessRegistrationNumber: "",
    // businessRegistrationDate: "",
    // businessRegistrationPlace: "",

    // === THÔNG ĐIỆP MUỐN TRUYỀN TẢI ===
    message: "", // Thông điệp muốn truyền tải
    reason: "", // Lý do muốn gia nhập GIBA
    object: "", // Mục tiêu muốn đạt được từ GIBA
    contribute: "", // Sản phẩm, dịch vụ và kinh nghiệm bạn có thể đóng góp
    careAbout: "", // Các dự án hợp tác, đầu tư hoặc hỗ trợ cộng đồng mà bạn quan tâm
    otherContribute: "", // Bạn có thể đóng góp gì khác cho GIBA?
  });

  useEffect(() => {
    if (hasInitialized.current) return;
    hasInitialized.current = true;

    setHeader({
      title: "ĐĂNG KÝ THÀNH VIÊN",
      hasLeftIcon: true,
      rightIcon: null,
      showUserInfo: false,
      showMenuButton: false,
      showCloseButton: false,
      backTo: false,
    });

    const savedData = sessionStorage.getItem("gibaRegistrationData");
    if (savedData) {
      try {
        const {
          formData: savedFormData,
          infoZalo: savedInfoZalo,
          phoneUser: savedPhoneUser,
          activeTab: savedActiveTab,
        } = JSON.parse(savedData);
        if (savedFormData) {
          setFormData(savedFormData);
          if (savedFormData.phone) {
            checkExistingUser(savedFormData.phone);
          }
        }
        if (savedInfoZalo && Object.keys(savedInfoZalo).length > 0) {
          setInfoZalo(savedInfoZalo);
        }
        if (savedPhoneUser) {
          setPhoneUser(savedPhoneUser);
          if (savedPhoneUser) {
            checkExistingUser(savedPhoneUser);
          }
        }
        if (savedActiveTab) {
          setActiveTab(savedActiveTab);
        }
      } catch (error) {
        console.error("Error restoring data from sessionStorage:", error);
      }
    } else {
      if (infoZalo && Object.keys(infoZalo).length > 0 && phoneUser) {
        console.log("Auto filling from Zalo info:", infoZalo, phoneUser);
        setFormData((prev) => ({
          ...prev,
          fullname: (infoZalo as any)?.name || prev.fullname,
          phone: phoneUser || prev.phone,
          userZaloIdByOA: (infoZalo as any)?.idByOA || prev.userZaloIdByOA,
        }));
        if (phoneUser) {
          checkExistingUser(phoneUser);
        }
      } else {
        authorizeUser();
      }
    }
  }, []);

  const getInfoUserZalo = async (): Promise<any> => {
    const infoZaloPromise = await new Promise<any>((resolve) => {
      getUserInfo({
        autoRequestPermission: true,
        success: (data) => {
          const { userInfo } = data;
          resolve(userInfo);
        },
        fail: (error) => {
          resolve({});
        },
      });
    });
    return infoZaloPromise;
  };

  const getInfoPhonenumber = async () => {
    try {
      const accessToken = await getAccessToken({});

      if (!accessToken) {
        return "";
      }

      const phoneNumberPromise = await new Promise((resolve) => {
        getPhoneNumber({
          success: async (data) => {
            try {
              const { token } = data;
              const response = await axios.post(
                `${dfData.domain}/api/ZaloHelperApi/GetPhoneNumber`,
                {
                  accessToken: accessToken,
                  tokenNumber: token,
                  secretKey: dfData.secretKey,
                }
              );

              let phoneNumber = response.data.data.number;

              if (phoneNumber.startsWith("84")) {
                phoneNumber = "0" + phoneNumber.slice(2);
              }
              resolve(phoneNumber);
            } catch (error) {
              console.log("Error fetching phone number:", error);
              resolve("");
            }
          },
          fail: (error) => {
            console.log("Error getting phone number:", error);
            resolve("");
          },
        });
      });

      return phoneNumberPromise;
    } catch (error) {
      console.log("Error in getInfoPhonenumber:", error);
      return "";
    }
  };

  const checkExistingUser = async (phoneNumber: string) => {
    try {
      let formattedPhone = phoneNumber.replace(/\s/g, "");
      if (formattedPhone.startsWith("0")) {
        formattedPhone = "84" + formattedPhone.slice(1);
      } else if (!formattedPhone.startsWith("84")) {
        formattedPhone = "84" + formattedPhone;
      }

      const response = await axios.get(
        `${dfData.domain}/api/Memberships/check-user?phoneNumber=${formattedPhone}&type=user`
      );

      if (response.data.success && response.data.data?.userInfo) {
        const userInfo = response.data.data.userInfo;
        setIsExistingUser(true);

        let formattedPhoneNumber = userInfo.phoneNumber || "";

        if (formattedPhoneNumber) {
          formattedPhoneNumber = String(formattedPhoneNumber).trim();
          formattedPhoneNumber = formattedPhoneNumber.replace(/\s/g, "");

          if (
            formattedPhoneNumber.startsWith("84") &&
            formattedPhoneNumber.length === 11
          ) {
            formattedPhoneNumber = "0" + formattedPhoneNumber.slice(2);
            console.log("Converted 84xxx to 0xxx:", formattedPhoneNumber);
          }
          // Convert 84xxx to 0xxx (10 digits starting with 84)
          else if (
            formattedPhoneNumber.startsWith("84") &&
            formattedPhoneNumber.length === 10
          ) {
            formattedPhoneNumber = "0" + formattedPhoneNumber.slice(2);
            console.log(
              "Converted 84xxx (10 digits) to 0xxx:",
              formattedPhoneNumber
            );
          }
          // If it starts with +84, convert to 0xxx
          else if (formattedPhoneNumber.startsWith("+84")) {
            formattedPhoneNumber = "0" + formattedPhoneNumber.slice(3);
            console.log("Converted +84xxx to 0xxx:", formattedPhoneNumber);
          }
          // If it starts with 84 but has different length, still try to convert
          else if (
            formattedPhoneNumber.startsWith("84") &&
            formattedPhoneNumber.length >= 10
          ) {
            formattedPhoneNumber = "0" + formattedPhoneNumber.slice(2);
            console.log(
              "Converted 84xxx (>=10 digits) to 0xxx:",
              formattedPhoneNumber
            );
          }
        }

        console.log("Final formatted phone:", formattedPhoneNumber);
        console.log("Fullname from API:", userInfo.fullname);

        setFormData((prev) => {
          const newFormData = {
            ...prev,
            fullname: userInfo.fullname || prev.fullname,
            position: userInfo.position || prev.position,
            dayOfBirth: userInfo.birthDate || prev.dayOfBirth,
            address: userInfo.address || prev.address,
            phone: formattedPhoneNumber || prev.phone,
            email: userInfo.email || prev.email,
            companyFullName: userInfo.companyFullName || prev.companyFullName,
            taxCode: userInfo.taxCode || prev.taxCode,
            businessField: userInfo.businessField || prev.businessField,
            headquartersAddress:
              userInfo.headquartersAddress || prev.headquartersAddress,
            companyWebsite: userInfo.companyWebsite || prev.companyWebsite,
            companyPhoneNumber:
              userInfo.companyPhoneNumber || prev.companyPhoneNumber,
            companyEmail: userInfo.companyEmail || prev.companyEmail,
          };
          console.log("New formData after API fill:", newFormData);
          return newFormData;
        });
      } else {
        setIsExistingUser(false);
      }
    } catch (error) {
      console.log("Error checking existing user:", error);
      setIsExistingUser(false);
    }
  };

  const authorizeUser = async () => {
    if (isLoadingZaloInfo) return;

    if (formData.fullname && formData.phone) return;

    try {
      setIsLoadingZaloInfo(true);
      const zaloInfo: any = await getInfoUserZalo();
      const phonenumber = (await getInfoPhonenumber()) as string;

      console.log("phonenumber: ", phonenumber);
      console.log("zaloInfo: ", zaloInfo);

      if (zaloInfo?.name || phonenumber) {
        setFormData((prev) => ({
          ...prev,
          fullname: (zaloInfo?.name as string) || prev.fullname,
          phone: phonenumber || prev.phone,
          userZaloIdByOA: (zaloInfo?.idByOA as string) || prev.userZaloIdByOA,
        }));

        if (zaloInfo) {
          setInfoZalo(zaloInfo);
        }
        if (phonenumber) {
          setPhoneUser(phonenumber);
          await checkExistingUser(phonenumber);
        }
      }

      setIsLoadingZaloInfo(false);
    } catch (error) {
      console.log("Error authorizing user:", error);
      setIsLoadingZaloInfo(false);
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
          console.log("err: ", err);
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

    setIsFollowedOA(isFollow);

    if (isFollow) {
      navigate("/giba/behavior-rules", {
        state: {
          groupId: null,
          returnToDrawer: null,
          drawerKey: null,
        },
      });
    }

    return isFollow;
  };

  const isFormComplete = () => {
    // Debug log
    console.log("isFormComplete check:", {
      fullname: formData.fullname,
      phone: formData.phone,
      fullnameTrimmed: formData.fullname?.trim(),
      phoneTrimmed: formData.phone?.trim(),
    });

    if (!formData.fullname || formData.fullname.trim() === "") {
      console.log("Failed: fullname is empty");
      return false;
    }
    if (!formData.phone || formData.phone.trim() === "") {
      console.log("Failed: phone is empty");
      return false;
    }
    const phoneRegex = /^(0|\+84)[0-9]{9,10}$/;
    const phoneWithoutSpaces = formData.phone.replace(/\s/g, "");
    const phoneTest = phoneRegex.test(phoneWithoutSpaces);
    console.log("Phone validation:", {
      phone: formData.phone,
      phoneWithoutSpaces,
      phoneTest,
    });
    if (!phoneTest) {
      console.log("Failed: phone format invalid");
      return false;
    }
    console.log("Form is complete!");
    return true;
  };

  const validatePersonalInfo = () => {
    const errors: Record<string, string> = {};

    if (!formData.fullname || formData.fullname.trim() === "") {
      errors.fullname = "Họ tên là bắt buộc";
    }

    // Validate phone
    if (!formData.phone || formData.phone.trim() === "") {
      errors.phone = "Số điện thoại là bắt buộc";
    } else {
      const phoneRegex = /^(0|\+84)[0-9]{9,10}$/;
      if (!phoneRegex.test(formData.phone.replace(/\s/g, ""))) {
        errors.phone = "Số điện thoại không hợp lệ";
      }
    }

    setValidationErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleNext = async () => {
    if (!validatePersonalInfo()) {
      toast.error("Vui lòng điền đầy đủ thông tin bắt buộc");
      return;
    }

    sessionStorage.setItem(
      "gibaRegistrationData",
      JSON.stringify({
        formData,
        infoZalo,
        phoneUser,
        activeTab,
        isExistingUser,
      })
    );

    navigate("/giba/behavior-rules", {
      state: {
        groupId: null,
        returnToDrawer: null,
        drawerKey: null,
      },
    });
  };

  return (
    <Page className="bg-black min-h-screen">
      <div className="max-w-2xl mx-auto px-4 py-6 mt-[60px] pb-32">
        {loading ? (
          <div className="flex justify-center items-center min-h-screen">
            <LoadingGiba size="lg" text="Đang đăng ký..." />
          </div>
        ) : (
          <>
            <div className="flex bg-gray-900 rounded-lg p-1 mb-6">
              <button
                onClick={() => setActiveTab("personal")}
                className={`flex-1 py-3 px-4 text-sm font-medium rounded-md transition-all duration-200 ${
                  activeTab === "personal"
                    ? "bg-white text-black"
                    : "text-gray-400 hover:text-white"
                }`}
              >
                Thông tin cá nhân
              </button>
              <button
                onClick={() => setActiveTab("company")}
                className={`flex-1 py-3 px-4 text-sm font-medium rounded-md transition-all duration-200 ${
                  activeTab === "company"
                    ? "bg-white text-black"
                    : "text-gray-400 hover:text-white"
                }`}
              >
                Thông tin doanh nghiệp
              </button>
              <button
                onClick={() => setActiveTab("message")}
                className={`flex-1 py-3 px-4 text-sm font-medium rounded-md transition-all duration-200 ${
                  activeTab === "message"
                    ? "bg-white text-black"
                    : "text-gray-400 hover:text-white"
                }`}
              >
                Thông điệp
              </button>
            </div>

            {/* Registration Form */}
            {activeTab === "personal" ? (
              <div className="space-y-4">
                {/* === THÔNG TIN CÁ NHÂN === */}

                {/* Họ tên */}
                <div>
                  <label className="block text-sm font-medium text-gray-300 mb-2">
                    Họ tên <span className="text-red-500">*</span>
                  </label>
                  <div className="relative">
                    <input
                      type="text"
                      value={formData.fullname}
                      onChange={(e) =>
                        setFormData({ ...formData, fullname: e.target.value })
                      }
                      onFocus={authorizeUser}
                      disabled={isLoadingZaloInfo}
                      className="w-full px-4 py-3 bg-gray-900 border border-gray-700 rounded-lg text-white placeholder-gray-500 focus:ring-2 focus:ring-white focus:border-transparent outline-none transition-all disabled:opacity-50 disabled:cursor-not-allowed"
                      placeholder={
                        isLoadingZaloInfo
                          ? "Đang lấy thông tin..."
                          : formData.fullname
                          ? ""
                          : "Nhấn để tự động điền từ Zalo"
                      }
                    />
                    {isLoadingZaloInfo && (
                      <div className="absolute right-3 top-1/2 transform -translate-y-1/2">
                        <svg
                          className="animate-spin h-5 w-5 text-white"
                          xmlns="http://www.w3.org/2000/svg"
                          fill="none"
                          viewBox="0 0 24 24"
                        >
                          <circle
                            className="opacity-25"
                            cx="12"
                            cy="12"
                            r="10"
                            stroke="currentColor"
                            strokeWidth="4"
                          ></circle>
                          <path
                            className="opacity-75"
                            fill="currentColor"
                            d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
                          ></path>
                        </svg>
                      </div>
                    )}
                  </div>
                </div>

                {/* Chức vụ */}
                <div>
                  <label className="block text-sm font-medium text-gray-300 mb-2">
                    Chức vụ
                  </label>
                  <input
                    type="text"
                    value={formData.position}
                    onChange={(e) =>
                      setFormData({ ...formData, position: e.target.value })
                    }
                    className="w-full px-4 py-3 bg-gray-900 border border-gray-700 rounded-lg text-white placeholder-gray-500 focus:ring-2 focus:ring-white focus:border-transparent outline-none transition-all"
                    placeholder="Nhập chức vụ"
                  />
                </div>

                {/* Năm sinh */}
                <div>
                  <label className="block text-sm font-medium text-gray-300 mb-2">
                    Năm sinh
                  </label>
                  <DatePicker
                    value={
                      formData.dayOfBirth ? dayjs(formData.dayOfBirth) : null
                    }
                    onChange={(date) =>
                      setFormData({
                        ...formData,
                        dayOfBirth: date ? date.format("YYYY-MM-DD") : "",
                      })
                    }
                    placeholder="Chọn năm sinh"
                    format="DD/MM/YYYY"
                    style={{ width: "100%", height: "48px" }}
                    className="giba-datepicker"
                  />
                </div>

                {/* Giới tính */}
                <div>
                  <label className="block text-sm font-medium text-gray-300 mb-2">
                    Giới tính
                  </label>
                  <Select
                    value={formData.gender || undefined}
                    onChange={(value) =>
                      setFormData({ ...formData, gender: value })
                    }
                    placeholder="Chọn giới tính"
                    style={{ width: "100%", height: "48px" }}
                    className="giba-select"
                    options={[
                      { value: "Nam", label: "Nam" },
                      { value: "Nữ", label: "Nữ" },
                      { value: "Khác", label: "Khác" },
                    ]}
                  />
                </div>

                {/* Địa chỉ */}
                <div>
                  <label className="block text-sm font-medium text-gray-300 mb-2">
                    Địa chỉ
                  </label>
                  <input
                    type="text"
                    value={formData.address}
                    onChange={(e) =>
                      setFormData({ ...formData, address: e.target.value })
                    }
                    className="w-full px-4 py-3 bg-gray-900 border border-gray-700 rounded-lg text-white placeholder-gray-500 focus:ring-2 focus:ring-white focus:border-transparent outline-none transition-all"
                    placeholder="Nhập địa chỉ"
                  />
                </div>

                {/* Điện thoại */}
                <div>
                  <label className="block text-sm font-medium text-gray-300 mb-2">
                    Điện thoại <span className="text-red-500">*</span>
                  </label>
                  <div className="relative">
                    <input
                      type="tel"
                      value={formData.phone}
                      onChange={(e) =>
                        setFormData({ ...formData, phone: e.target.value })
                      }
                      onFocus={authorizeUser}
                      disabled={isLoadingZaloInfo}
                      className="w-full px-4 py-3 bg-gray-900 border border-gray-700 rounded-lg text-white placeholder-gray-500 focus:ring-2 focus:ring-white focus:border-transparent outline-none transition-all disabled:opacity-50 disabled:cursor-not-allowed"
                      placeholder={
                        isLoadingZaloInfo
                          ? "Đang lấy thông tin..."
                          : formData.phone
                          ? ""
                          : "Nhấn để tự động điền từ Zalo"
                      }
                    />
                    {isLoadingZaloInfo && (
                      <div className="absolute right-3 top-1/2 transform -translate-y-1/2">
                        <svg
                          className="animate-spin h-5 w-5 text-white"
                          xmlns="http://www.w3.org/2000/svg"
                          fill="none"
                          viewBox="0 0 24 24"
                        >
                          <circle
                            className="opacity-25"
                            cx="12"
                            cy="12"
                            r="10"
                            stroke="currentColor"
                            strokeWidth="4"
                          ></circle>
                          <path
                            className="opacity-75"
                            fill="currentColor"
                            d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
                          ></path>
                        </svg>
                      </div>
                    )}
                  </div>
                </div>

                {/* Email */}
                <div>
                  <label className="block text-sm font-medium text-gray-300 mb-2">
                    Email
                  </label>
                  <input
                    type="email"
                    value={formData.email}
                    onChange={(e) =>
                      setFormData({ ...formData, email: e.target.value })
                    }
                    className="w-full px-4 py-3 bg-gray-900 border border-gray-700 rounded-lg text-white placeholder-gray-500 focus:ring-2 focus:ring-white focus:border-transparent outline-none transition-all"
                    placeholder="Nhập email"
                  />
                </div>
              </div>
            ) : activeTab === "company" ? (
              <div className="space-y-4">
                {/* === THÔNG TIN DOANH NGHIỆP === */}

                {/* Logo doanh nghiệp */}
                <div>
                  <label className="block text-sm font-medium text-gray-300 mb-2">
                    Logo doanh nghiệp
                  </label>
                  <input
                    type="file"
                    accept="image/*"
                    onChange={(e) =>
                      setFormData({
                        ...formData,
                        companyLogoFile: e.target.files?.[0] || null,
                      })
                    }
                    className="w-full px-4 py-3 bg-gray-900 border border-gray-700 rounded-lg text-white file:mr-4 file:py-2 file:px-4 file:rounded-full file:border-0 file:text-sm file:font-semibold file:bg-white file:text-black hover:file:bg-gray-200 transition-all"
                  />
                </div>

                {/* Tên doanh nghiệp */}
                <div>
                  <label className="block text-sm font-medium text-gray-300 mb-2">
                    Tên doanh nghiệp
                  </label>
                  <input
                    type="text"
                    value={formData.companyFullName}
                    onChange={(e) =>
                      setFormData({
                        ...formData,
                        companyFullName: e.target.value,
                      })
                    }
                    className="w-full px-4 py-3 bg-gray-900 border border-gray-700 rounded-lg text-white placeholder-gray-500 focus:ring-2 focus:ring-white focus:border-transparent outline-none transition-all"
                    placeholder="Nhập tên doanh nghiệp"
                  />
                </div>

                {/* Mã số thuế */}
                <div>
                  <label className="block text-sm font-medium text-gray-300 mb-2">
                    Mã số thuế
                  </label>
                  <input
                    type="text"
                    value={formData.taxCode}
                    onChange={(e) =>
                      setFormData({ ...formData, taxCode: e.target.value })
                    }
                    className="w-full px-4 py-3 bg-gray-900 border border-gray-700 rounded-lg text-white placeholder-gray-500 focus:ring-2 focus:ring-white focus:border-transparent outline-none transition-all"
                    placeholder="Nhập mã số thuế"
                  />
                </div>

                {/* Ngành nghề kinh doanh chính */}
                <div>
                  <label className="block text-sm font-medium text-gray-300 mb-2">
                    Ngành nghề kinh doanh chính
                  </label>
                  <input
                    type="text"
                    value={formData.businessField}
                    onChange={(e) =>
                      setFormData({
                        ...formData,
                        businessField: e.target.value,
                      })
                    }
                    className="w-full px-4 py-3 bg-gray-900 border border-gray-700 rounded-lg text-white placeholder-gray-500 focus:ring-2 focus:ring-white focus:border-transparent outline-none transition-all"
                    placeholder="Nhập ngành nghề kinh doanh chính"
                  />
                </div>

                {/* Địa chỉ doanh nghiệp */}
                <div>
                  <label className="block text-sm font-medium text-gray-300 mb-2">
                    Địa chỉ doanh nghiệp
                  </label>
                  <input
                    type="text"
                    value={formData.headquartersAddress}
                    onChange={(e) =>
                      setFormData({
                        ...formData,
                        headquartersAddress: e.target.value,
                      })
                    }
                    className="w-full px-4 py-3 bg-gray-900 border border-gray-700 rounded-lg text-white placeholder-gray-500 focus:ring-2 focus:ring-white focus:border-transparent outline-none transition-all"
                    placeholder="Nhập địa chỉ doanh nghiệp"
                  />
                </div>

                {/* Điện thoại doanh nghiệp */}
                <div>
                  <label className="block text-sm font-medium text-gray-300 mb-2">
                    Điện thoại doanh nghiệp
                  </label>
                  <input
                    type="tel"
                    value={formData.companyPhoneNumber}
                    onChange={(e) =>
                      setFormData({
                        ...formData,
                        companyPhoneNumber: e.target.value,
                      })
                    }
                    className="w-full px-4 py-3 bg-gray-900 border border-gray-700 rounded-lg text-white placeholder-gray-500 focus:ring-2 focus:ring-white focus:border-transparent outline-none transition-all"
                    placeholder="Nhập số điện thoại doanh nghiệp"
                  />
                </div>

                {/* Email doanh nghiệp */}
                <div>
                  <label className="block text-sm font-medium text-gray-300 mb-2">
                    Email doanh nghiệp
                  </label>
                  <input
                    type="email"
                    value={formData.companyEmail}
                    onChange={(e) =>
                      setFormData({ ...formData, companyEmail: e.target.value })
                    }
                    className="w-full px-4 py-3 bg-gray-900 border border-gray-700 rounded-lg text-white placeholder-gray-500 focus:ring-2 focus:ring-white focus:border-transparent outline-none transition-all"
                    placeholder="Nhập email doanh nghiệp"
                  />
                </div>

                {/* Website doanh nghiệp */}
                <div>
                  <label className="block text-sm font-medium text-gray-300 mb-2">
                    Website doanh nghiệp
                  </label>
                  <input
                    type="url"
                    value={formData.companyWebsite}
                    onChange={(e) =>
                      setFormData({
                        ...formData,
                        companyWebsite: e.target.value,
                      })
                    }
                    className="w-full px-4 py-3 bg-gray-900 border border-gray-700 rounded-lg text-white placeholder-gray-500 focus:ring-2 focus:ring-white focus:border-transparent outline-none transition-all"
                    placeholder="Nhập website doanh nghiệp"
                  />
                </div>

                {/* Removed fields - commented for reference */}
                {/* Tên thương hiệu */}
                {/* <div>
                  <label className="block text-sm font-medium text-gray-300 mb-2">
                    Tên thương hiệu:
                  </label>
                  <input
                    type="text"
                    value={formData.companyBrandName}
                    onChange={(e) =>
                      setFormData({
                        ...formData,
                        companyBrandName: e.target.value,
                      })
                    }
                    className="w-full px-4 py-3 bg-gray-900 border border-gray-700 rounded-lg text-white placeholder-gray-500 focus:ring-2 focus:ring-white focus:border-transparent outline-none transition-all"
                    placeholder="Nhập tên thương hiệu"
                  />
                </div> */}

                {/* Loại hình doanh nghiệp */}
                {/* <div>
                  <label className="block text-sm font-medium text-gray-300 mb-2">
                    Loại hình doanh nghiệp
                  </label>
                  <Select
                    value={formData.businessType || undefined}
                    onChange={(value) =>
                      setFormData({ ...formData, businessType: value })
                    }
                    placeholder="Chọn loại hình doanh nghiệp"
                    style={{ width: "100%", height: "48px" }}
                    className="giba-select"
                    options={[
                      { value: "Công ty TNHH", label: "Công ty TNHH" },
                      { value: "Công ty Cổ phần", label: "Công ty Cổ phần" },
                      {
                        value: "Doanh nghiệp tư nhân",
                        label: "Doanh nghiệp tư nhân",
                      },
                      { value: "Hộ kinh doanh", label: "Hộ kinh doanh" },
                    ]}
                  />
                </div> */}

                {/* Người đại diện pháp lý */}
                {/* <div>
                  <label className="block text-sm font-medium text-gray-300 mb-2">
                    Người đại diện pháp lý
                  </label>
                  <input
                    type="text"
                    value={formData.legalRepresentative}
                    onChange={(e) =>
                      setFormData({
                        ...formData,
                        legalRepresentative: e.target.value,
                      })
                    }
                    className="w-full px-4 py-3 bg-gray-900 border border-gray-700 rounded-lg text-white placeholder-gray-500 focus:ring-2 focus:ring-white focus:border-transparent outline-none transition-all"
                    placeholder="Nhập tên người đại diện pháp lý"
                  />
                </div> */}

                {/* Chức vụ của người đại diện */}
                {/* <div>
                  <label className="block text-sm font-medium text-gray-300 mb-2">
                    Chức vụ của người đại diện
                  </label>
                  <input
                    type="text"
                    value={formData.legalRepresentativePosition}
                    onChange={(e) =>
                      setFormData({
                        ...formData,
                        legalRepresentativePosition: e.target.value,
                      })
                    }
                    className="w-full px-4 py-3 bg-gray-900 border border-gray-700 rounded-lg text-white placeholder-gray-500 focus:ring-2 focus:ring-white focus:border-transparent outline-none transition-all"
                    placeholder="Nhập chức vụ của người đại diện"
                  />
                </div> */}

                {/* Removed fields - Giấy chứng nhận đăng ký kinh doanh */}
                {/* <div className="space-y-3">
                  <h4 className="text-lg font-medium text-white">
                    Giấy chứng nhận đăng ký kinh doanh
                  </h4>
                  <div>
                    <label className="block text-sm font-medium text-gray-300 mb-2">Số</label>
                    <input type="text" value={formData.businessRegistrationNumber} />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-300 mb-2">Ngày cấp</label>
                    <DatePicker value={formData.businessRegistrationDate} />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-300 mb-2">Nơi cấp</label>
                    <input type="text" value={formData.businessRegistrationPlace} />
                  </div>
                </div> */}
              </div>
            ) : activeTab === "message" ? (
              <div className="space-y-4">
                {/* === THÔNG ĐIỆP MUỐN TRUYỀN TẢI === */}

                {/* Thông điệp muốn truyền tải */}
                <div>
                  <label className="block text-sm font-medium text-gray-300 mb-2">
                    Thông điệp muốn truyền tải
                  </label>
                  <textarea
                    value={formData.message}
                    onChange={(e) =>
                      setFormData({ ...formData, message: e.target.value })
                    }
                    rows={4}
                    className="w-full px-4 py-3 bg-gray-900 border border-gray-700 rounded-lg text-white placeholder-gray-500 focus:ring-2 focus:ring-white focus:border-transparent outline-none transition-all resize-none"
                    placeholder="Nhập thông điệp muốn truyền tải"
                  />
                </div>

                {/* MỤC ĐÍCH THAM GIA CỘNG ĐỒNG */}
                <div className="mt-6">
                  <h4 className="text-lg font-medium text-white mb-4">
                    MỤC ĐÍCH THAM GIA CỘNG ĐỒNG
                  </h4>

                  {/* Lý do muốn gia nhập GIBA */}
                  <div className="mb-4">
                    <label className="block text-sm font-medium text-gray-300 mb-2">
                      Lý do muốn gia nhập GIBA
                    </label>
                    <textarea
                      value={formData.reason}
                      onChange={(e) =>
                        setFormData({ ...formData, reason: e.target.value })
                      }
                      rows={4}
                      className="w-full px-4 py-3 bg-gray-900 border border-gray-700 rounded-lg text-white placeholder-gray-500 focus:ring-2 focus:ring-white focus:border-transparent outline-none transition-all resize-none"
                      placeholder="Nhập lý do muốn gia nhập GIBA"
                    />
                  </div>

                  {/* Mục tiêu muốn đạt được từ GIBA */}
                  <div>
                    <label className="block text-sm font-medium text-gray-300 mb-2">
                      Mục tiêu muốn đạt được từ GIBA
                    </label>
                    <textarea
                      value={formData.object}
                      onChange={(e) =>
                        setFormData({ ...formData, object: e.target.value })
                      }
                      rows={4}
                      className="w-full px-4 py-3 bg-gray-900 border border-gray-700 rounded-lg text-white placeholder-gray-500 focus:ring-2 focus:ring-white focus:border-transparent outline-none transition-all resize-none"
                      placeholder="Nhập mục tiêu muốn đạt được từ GIBA"
                    />
                  </div>
                </div>

                {/* KINH NGHIỆM VÀ ĐÓNG GÓP SỰ KIỆN */}
                <div className="mt-6">
                  <h4 className="text-lg font-medium text-white mb-4">
                    KINH NGHIỆM VÀ ĐÓNG GÓP SỰ KIỆN
                  </h4>

                  {/* Sản phẩm, dịch vụ và kinh nghiệm bạn có thể đóng góp */}
                  <div className="mb-4">
                    <label className="block text-sm font-medium text-gray-300 mb-2">
                      Sản phẩm, dịch vụ và kinh nghiệm bạn có thể đóng góp
                    </label>
                    <textarea
                      value={formData.contribute}
                      onChange={(e) =>
                        setFormData({ ...formData, contribute: e.target.value })
                      }
                      rows={4}
                      className="w-full px-4 py-3 bg-gray-900 border border-gray-700 rounded-lg text-white placeholder-gray-500 focus:ring-2 focus:ring-white focus:border-transparent outline-none transition-all resize-none"
                      placeholder="Nhập sản phẩm, dịch vụ và kinh nghiệm bạn có thể đóng góp"
                    />
                  </div>

                  {/* Các dự án hợp tác, đầu tư hoặc hỗ trợ cộng đồng mà bạn quan tâm */}
                  <div className="mb-4">
                    <label className="block text-sm font-medium text-gray-300 mb-2">
                      Các dự án hợp tác, đầu tư hoặc hỗ trợ cộng đồng mà bạn
                      quan tâm
                    </label>
                    <textarea
                      value={formData.careAbout}
                      onChange={(e) =>
                        setFormData({ ...formData, careAbout: e.target.value })
                      }
                      rows={4}
                      className="w-full px-4 py-3 bg-gray-900 border border-gray-700 rounded-lg text-white placeholder-gray-500 focus:ring-2 focus:ring-white focus:border-transparent outline-none transition-all resize-none"
                      placeholder="Nhập các dự án hợp tác, đầu tư hoặc hỗ trợ cộng đồng mà bạn quan tâm"
                    />
                  </div>

                  {/* Bạn có thể đóng góp gì khác cho GIBA? */}
                  <div>
                    <label className="block text-sm font-medium text-gray-300 mb-2">
                      Bạn có thể đóng góp gì khác cho GIBA?
                    </label>
                    <textarea
                      value={formData.otherContribute}
                      onChange={(e) =>
                        setFormData({
                          ...formData,
                          otherContribute: e.target.value,
                        })
                      }
                      rows={4}
                      className="w-full px-4 py-3 bg-gray-900 border border-gray-700 rounded-lg text-white placeholder-gray-500 focus:ring-2 focus:ring-white focus:border-transparent outline-none transition-all resize-none"
                      placeholder="Nhập những gì bạn có thể đóng góp khác cho GIBA"
                    />
                  </div>
                </div>
              </div>
            ) : null}

            {/* Submit Button - Fixed Bottom */}
            <div className="fixed bottom-0 left-0 right-0 px-4 py-4 bg-black border-t border-gray-800 z-50">
              <button
                onClick={handleNext}
                disabled={!isFormComplete()}
                className={`w-full mb-2 py-3 px-6 rounded-lg font-semibold text-base transition-all duration-200 ${
                  isFormComplete()
                    ? "bg-white text-black hover:bg-gray-200 shadow-lg"
                    : "bg-gray-700 text-gray-400 cursor-not-allowed"
                }`}
              >
                Tiếp theo
              </button>
            </div>
          </>
        )}
      </div>
    </Page>
  );
};

export default LoginGiba;
