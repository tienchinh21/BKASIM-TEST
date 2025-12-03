import React, { useEffect, useState, useRef } from "react";
import { Page, Box } from "zmp-ui";
import { useNavigate } from "zmp-ui";
import useSetHeader from "../components/hooks/useSetHeader";
import { useRecoilValue } from "recoil";
import { token } from "../recoil/RecoilState";
import axios from "axios";
import LoadingGiba from "../componentsGiba/LoadingGiba";
import { Form, Input, DatePicker, Spin, Avatar, Select, Button } from "antd";
import { toast } from "react-toastify";
import dayjs from "dayjs";
import CommonButton from "../components/CommonButton";
import dfData from "../common/DefaultConfig.json";
import { EditOutlined } from "@ant-design/icons";
const { TextArea } = Input;
const { Option } = Select;

// Custom CSS for form components
const customStyles = `
  .giba-datepicker .ant-picker {
    background-color: #1f2937 !important;
    border-color: #374151 !important;
    color: white !important;
  }

  .giba-datepicker .ant-picker:hover,
  .giba-datepicker .ant-picker:focus {
    background-color: #1f2937 !important;
    border-color: white !important;
    color: white !important;
  }

  .giba-datepicker .ant-picker-input input {
    color: white !important;
    background-color: transparent !important;
  }

  .giba-datepicker .ant-picker-input input::placeholder {
    color: #9ca3af !important;
  }

  .giba-select .ant-select-selector {
    background-color: #1f2937 !important;
    border-color: #374151 !important;
    color: white !important;
  }

  .giba-select .ant-select-selector:hover,
  .giba-select .ant-select-selector:focus {
    background-color: #1f2937 !important;
    border-color: white !important;
    color: white !important;
  }

  .giba-select .ant-select-selection-item {
    color: white !important;
  }

  .giba-select .ant-select-selection-placeholder {
    color: #9ca3af !important;
  }

  .giba-select-multiple .ant-select-selector {
    background-color: #1f2937 !important;
    border-color: #374151 !important;
    color: white !important;
  }

  .giba-select-multiple .ant-select-selector:hover,
  .giba-select-multiple .ant-select-selector:focus {
    background-color: #1f2937 !important;
    border-color: white !important;
    color: white !important;
  }

  .giba-select-multiple .ant-select-selection-item {
    background-color: #C0002C !important;
    color: white !important;
    border-color: #C0002C !important;
  }

  .giba-select-multiple .ant-select-selection-placeholder {
    color: #9ca3af !important;
  }

  /* TextArea focus styles */
  .ant-input:focus {
    background-color: #1f2937 !important;
    border-color: white !important;
    color: white !important;
    box-shadow: 0 0 0 2px rgba(255, 255, 255, 0.2) !important;
  }

  .ant-input:hover {
    background-color: #1f2937 !important;
    border-color: #6b7280 !important;
    color: white !important;
  }

  /* Disabled input styles */
  .ant-input[disabled] {
    background-color: #1f2937 !important;
    border-color: #374151 !important;
    color: white !important;
    opacity: 1 !important;
  }

  .ant-input[disabled]:hover {
    background-color: #1f2937 !important;
    border-color: #374151 !important;
    color: white !important;
  }
`;

interface Field {
  id: string;
  fieldName?: string;
  name?: string; // Some APIs might use 'name' instead of 'fieldName'
}

interface ProfileData {
  id: string;
  userZaloId: string;
  userZaloName: string;
  fullname: string;
  slug: string;
  phoneNumber: string;
  email: string;
  zaloAvatar: string;
  profile: string;
  dayOfBirth: string;
  address: string;
  company: string;
  position: string;
  appPosition: string | null;
  term: string | null;
  fieldIds: string;
  averageRating: number;
  totalRatings: number;
  approvalStatus: number;
  approvalReason: string | null;
  approvedDate: string | null;
  approvedBy: string | null;
  // Thông tin pháp nhân
  companyFullName: string | null;
  companyBrandName: string | null;
  taxCode: string | null;
  businessField: string | null;
  businessType: string | null;
  headquartersAddress: string | null;
  companyWebsite: string | null;
  companyPhoneNumber: string | null;
  companyEmail: string | null;
  legalRepresentative: string | null;
  legalRepresentativePosition: string | null;
  companyLogo: string | null;
  businessRegistrationNumber: string | null;
  businessRegistrationDate: string | null;
  businessRegistrationPlace: string | null;
  createdDate: string;
  updatedDate: string;
}

const EditInfoGiba: React.FC = () => {
  const navigate = useNavigate();
  const setHeader = useSetHeader();
  const userToken = useRecoilValue(token);
  const [form] = Form.useForm();
  const [loading, setLoading] = useState(true);
  const [spinning, setSpinning] = useState(false);
  const [profileData, setProfileData] = useState<ProfileData | null>(null);
  const [fields, setFields] = useState<Field[]>([]);
  const [activeTab, setActiveTab] = useState<"personal" | "legal">("personal");
  const [uploadingLogo, setUploadingLogo] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  // Helper function to merge form values with original data
  const mergeFormData = (formValues: any, originalData: any) => {
    const merged = { ...originalData };

    // Override with form values if they exist and are not empty
    Object.keys(formValues).forEach((key) => {
      if (
        formValues[key] !== undefined &&
        formValues[key] !== null &&
        formValues[key] !== ""
      ) {
        merged[key] = formValues[key];
      }
    });

    return merged;
  };

  // Set header
  React.useEffect(() => {
    setHeader({
      title: "CHỈNH SỬA THÔNG TIN",
      showUserInfo: false,
      showMenuButton: false,
      showCloseButton: false,
      hasLeftIcon: true,
    });
  }, [setHeader]);

  // Fetch profile and fields
  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);

        // Fetch profile
        const profileResponse = await axios.get(
          `${dfData.domain}/api/memberships/profile`,
          {
            headers: {
              Authorization: `Bearer ${userToken}`,
            },
          }
        );
        if (profileResponse.data.success && profileResponse.data.data) {
          setProfileData(profileResponse.data.data);

          // Set form values with proper null checks
          const profileData = profileResponse.data.data;
          console.log("Profile data from API:", profileData);
          form.setFieldsValue({
            fullname: profileData.fullname || "",
            phoneNumber: profileData.phoneNumber || "",
            email: profileData.email || "",
            profile: profileData.profile || "",
            dayOfBirth: profileData.dayOfBirth
              ? dayjs(profileData.dayOfBirth)
              : null,
            address: profileData.address || "",
            company: profileData.company || "",
            position: profileData.position || "",
            appPosition: profileData.appPosition || "",
            term: profileData.term || "",
            fieldIds: profileData.fieldIds
              ? profileData.fieldIds.split(",").filter((id) => id.trim())
              : [],
            // Thông tin pháp nhân
            companyFullName: profileData.companyFullName || "",
            companyBrandName: profileData.companyBrandName || "",
            taxCode: profileData.taxCode || "",
            businessField: profileData.businessField || "",
            businessType: profileData.businessType || "",
            headquartersAddress: profileData.headquartersAddress || "",
            companyWebsite: profileData.companyWebsite || "",
            companyPhoneNumber: profileData.companyPhoneNumber || "",
            companyEmail: profileData.companyEmail || "",
            legalRepresentative: profileData.legalRepresentative || "",
            legalRepresentativePosition:
              profileData.legalRepresentativePosition || "",
            businessRegistrationNumber:
              profileData.businessRegistrationNumber || "",
            businessRegistrationDate: profileData.businessRegistrationDate
              ? dayjs(profileData.businessRegistrationDate)
              : null,
            businessRegistrationPlace:
              profileData.businessRegistrationPlace || "",
          });

          console.log("Form values after setting:", form.getFieldsValue());
        }

        // Fetch fields
        const fieldsResponse = await axios.get(
          `${dfData.domain}/api/fields/active`,
          {
            headers: {
              Authorization: `Bearer ${userToken}`,
            },
          }
        );
        if (
          (fieldsResponse.data.success || fieldsResponse.data.code === 0) &&
          fieldsResponse.data.data
        ) {
          setFields(fieldsResponse.data.data);

          // Update fieldIds in form after fields are loaded
          if (profileData?.fieldIds) {
            const selectedFieldIds = profileData.fieldIds
              .split(",")
              .filter((id: string) => id.trim());
            form.setFieldsValue({
              ...form.getFieldsValue(),
              fieldIds: selectedFieldIds,
            });
          }
        }
      } catch (error) {
        console.error("Error fetching data:", error);
        toast.error("Không thể tải thông tin. Vui lòng thử lại!");
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, []);

  // Hàm xử lý upload logo công ty
  const handleLogoUpload = async (file: File) => {
    setUploadingLogo(true);
    try {
      const formData = new FormData();
      formData.append("companylogofile", file);

      const response = await axios.put(
        `${dfData.domain}/api/memberships/update-company-logo`,
        formData,
        {
          headers: {
            Authorization: `Bearer ${userToken}`,
            "Content-Type": "multipart/form-data",
          },
        }
      );

      if (response.data.success || response.data.code === 0) {
        toast.success("Cập nhật logo công ty thành công!");
        // Cập nhật lại profile data để hiển thị logo mới
        if (profileData) {
          setProfileData({
            ...profileData,
            companyLogo:
              response.data.data?.companyLogo || profileData.companyLogo,
          });
        }
      } else {
        toast.error(response.data.message || "Có lỗi xảy ra khi cập nhật logo");
      }
    } catch (error) {
      console.error("Error uploading logo:", error);
      toast.error("Cập nhật logo thất bại. Vui lòng thử lại!");
    } finally {
      setUploadingLogo(false);
    }
  };

  // Hàm xử lý khi chọn file
  const handleFileSelect = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (file) {
      // Kiểm tra kích thước file (tối đa 5MB)
      if (file.size > 5 * 1024 * 1024) {
        toast.error("Kích thước file không được vượt quá 5MB");
        return;
      }

      // Kiểm tra định dạng file
      if (!file.type.startsWith("image/")) {
        toast.error("Vui lòng chọn file ảnh");
        return;
      }

      handleLogoUpload(file);
    }
    // Reset input để có thể chọn cùng file
    if (event.target) {
      event.target.value = "";
    }
  };

  const onFinish = async (values: any) => {
    setSpinning(true);
    try {
      // Merge dữ liệu cũ với dữ liệu mới - giữ nguyên dữ liệu cũ nếu không thay đổi
      const mergedData = mergeFormData(values, profileData || {});

      // Format payload theo API spec
      const payload = {
        fullname: mergedData.fullname || "",
        email: mergedData.email || "",
        profile: mergedData.profile || "",
        dayOfBirth: mergedData.dayOfBirth
          ? typeof mergedData.dayOfBirth === "string"
            ? mergedData.dayOfBirth
            : mergedData.dayOfBirth.format("YYYY-MM-DD")
          : null,
        address: mergedData.address || "",
        company: mergedData.company || "",
        position: mergedData.position || "",
        appPosition: mergedData.appPosition || "",
        term: mergedData.term || "",
        fieldIds: mergedData.fieldIds
          ? Array.isArray(mergedData.fieldIds)
            ? mergedData.fieldIds
            : mergedData.fieldIds.split(",").filter((id) => id.trim())
          : [],
        // Thông tin pháp nhân
        companyFullName: mergedData.companyFullName || "",
        companyBrandName: mergedData.companyBrandName || "",
        taxCode: mergedData.taxCode || "",
        businessField: mergedData.businessField || "",
        businessType: mergedData.businessType || "",
        headquartersAddress: mergedData.headquartersAddress || "",
        companyWebsite: mergedData.companyWebsite || "",
        companyPhoneNumber: mergedData.companyPhoneNumber || "",
        companyEmail: mergedData.companyEmail || "",
        legalRepresentative: mergedData.legalRepresentative || "",
        legalRepresentativePosition:
          mergedData.legalRepresentativePosition || "",
        businessRegistrationNumber: mergedData.businessRegistrationNumber || "",
        businessRegistrationDate: mergedData.businessRegistrationDate
          ? typeof mergedData.businessRegistrationDate === "string"
            ? mergedData.businessRegistrationDate
            : mergedData.businessRegistrationDate.format("YYYY-MM-DD")
          : null,
        businessRegistrationPlace: mergedData.businessRegistrationPlace || "",
      };

      const response = await axios.put(
        `${dfData.domain}/api/memberships/profile`,
        payload,
        {
          headers: {
            Authorization: `Bearer ${userToken}`,
          },
        }
      );

      if (response.data.success || response.data.code === 0) {
        toast.success(
          response.data.message || "Cập nhật thông tin thành công!"
        );
        navigate(-1);
      } else {
        toast.error(response.data.message || "Có lỗi xảy ra khi cập nhật");
      }
    } catch (error) {
      console.error("Error updating profile:", error);
      const errorMessage =
        (error as any).response?.data?.message ||
        "Chỉnh sửa thông tin thất bại. Vui lòng thử lại!";
      toast.error(errorMessage);
    } finally {
      setSpinning(false);
    }
  };

  if (loading) {
    return (
      <Page className="bg-white min-h-screen mt-[50px]">
        <div className="flex justify-center items-center h-64">
          <LoadingGiba size="lg" text="Đang tải thông tin..." />
        </div>
      </Page>
    );
  }

  return (
    <Page className="bg-black min-h-screen">
      <style>{customStyles}</style>
      <Spin spinning={spinning} fullscreen />
      <div className="max-w-2xl mx-auto px-4 py-6 mt-[40px] pb-32">
        {loading ? (
          <div className="flex justify-center items-center min-h-screen">
            <LoadingGiba size="lg" text="Đang tải thông tin..." />
          </div>
        ) : (
          <>
            {/* Tab Navigation */}
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
                onClick={() => setActiveTab("legal")}
                className={`flex-1 py-3 px-4 text-sm font-medium rounded-md transition-all duration-200 ${
                  activeTab === "legal"
                    ? "bg-white text-black"
                    : "text-gray-400 hover:text-white"
                }`}
              >
                Thông tin pháp nhân
              </button>
            </div>

            {/* Avatar */}
            <div className="flex justify-center mb-6">
              <div className="relative">
                <Avatar
                  size={100}
                  src={profileData?.zaloAvatar}
                  className="border-4 border-white shadow-lg"
                />
              </div>
            </div>

            <Form
              form={form}
              layout="vertical"
              onFinish={onFinish}
              className="space-y-4"
            >
              {/* Personal Info Tab */}
              {activeTab === "personal" ? (
                <div className="space-y-4">
                  {/* Họ tên */}
                  <div>
                    <label className="block text-sm font-medium text-gray-300 mb-2">
                      Họ tên <span className="text-red-500">*</span>
                    </label>
                    <Form.Item
                      name="fullname"
                      rules={[
                        { required: true, message: "Vui lòng điền họ và tên" },
                      ]}
                    >
                      <Input
                        disabled
                        className="w-full px-4 py-3 bg-gray-900 border border-gray-700 rounded-lg text-white placeholder-gray-500 focus:ring-2 focus:ring-white focus:border-transparent focus:bg-gray-900 focus:text-white outline-none transition-all disabled:opacity-100 disabled:cursor-not-allowed disabled:text-white"
                        placeholder="Họ và tên"
                      />
                    </Form.Item>
                  </div>

                  {/* Số điện thoại */}
                  <div>
                    <label className="block text-sm font-medium text-gray-300 mb-2">
                      Số điện thoại <span className="text-red-500">*</span>
                    </label>
                    <Form.Item
                      name="phoneNumber"
                      rules={[
                        {
                          required: true,
                          message: "Vui lòng điền số điện thoại",
                        },
                      ]}
                    >
                      <Input
                        disabled
                        className="w-full px-4 py-3 bg-gray-900 border border-gray-700 rounded-lg text-white placeholder-gray-500 focus:ring-2 focus:ring-white focus:border-transparent focus:bg-gray-900 focus:text-white outline-none transition-all disabled:opacity-100 disabled:cursor-not-allowed disabled:text-white"
                        placeholder="Số điện thoại"
                      />
                    </Form.Item>
                  </div>

                  {/* Email */}
                  <div>
                    <label className="block text-sm font-medium text-gray-300 mb-2">
                      Email
                    </label>
                    <Form.Item
                      name="email"
                      rules={[{ type: "email", message: "Email không hợp lệ" }]}
                    >
                      <Input
                        className="w-full px-4 py-3 bg-gray-900 border border-gray-700 rounded-lg text-white placeholder-gray-500 focus:ring-2 focus:ring-white focus:border-transparent focus:bg-gray-900 focus:text-white outline-none transition-all"
                        placeholder="Email"
                      />
                    </Form.Item>
                  </div>

                  {/* Ngày sinh */}
                  <div>
                    <label className="block text-sm font-medium text-gray-300 mb-2">
                      Ngày sinh
                    </label>
                    <Form.Item name="dayOfBirth">
                      <DatePicker
                        format="DD/MM/YYYY"
                        placeholder="Chọn ngày sinh"
                        style={{ width: "100%", height: "48px" }}
                        className="giba-datepicker"
                      />
                    </Form.Item>
                  </div>

                  {/* Địa chỉ */}
                  <div>
                    <label className="block text-sm font-medium text-gray-300 mb-2">
                      Địa chỉ
                    </label>
                    <Form.Item name="address">
                      <Input
                        className="w-full px-4 py-3 bg-gray-900 border border-gray-700 rounded-lg text-white placeholder-gray-500 focus:ring-2 focus:ring-white focus:border-transparent focus:bg-gray-900 focus:text-white outline-none transition-all"
                        placeholder="Địa chỉ"
                      />
                    </Form.Item>
                  </div>

                  {/* Công ty */}
                  <div>
                    <label className="block text-sm font-medium text-gray-300 mb-2">
                      Công ty
                    </label>
                    <Form.Item name="company">
                      <Input
                        className="w-full px-4 py-3 bg-gray-900 border border-gray-700 rounded-lg text-white placeholder-gray-500 focus:ring-2 focus:ring-white focus:border-transparent focus:bg-gray-900 focus:text-white outline-none transition-all"
                        placeholder="Công ty"
                      />
                    </Form.Item>
                  </div>

                  {/* Chức vụ */}
                  <div>
                    <label className="block text-sm font-medium text-gray-300 mb-2">
                      Chức vụ
                    </label>
                    <Form.Item name="position">
                      <Input
                        className="w-full px-4 py-3 bg-gray-900 border border-gray-700 rounded-lg text-white placeholder-gray-500 focus:ring-2 focus:ring-white focus:border-transparent focus:bg-gray-900 focus:text-white outline-none transition-all"
                        placeholder="Chức vụ"
                      />
                    </Form.Item>
                  </div>

                  {/* Chức danh */}
                  <div>
                    <label className="block text-sm font-medium text-gray-300 mb-2">
                      Chức danh
                    </label>
                    <Form.Item name="appPosition">
                      <Input
                        className="w-full px-4 py-3 bg-gray-900 border border-gray-700 rounded-lg text-white placeholder-gray-500 focus:ring-2 focus:ring-white focus:border-transparent focus:bg-gray-900 focus:text-white outline-none transition-all"
                        placeholder="Chức danh"
                      />
                    </Form.Item>
                  </div>

                  {/* Nhiệm kỳ */}
                  <div>
                    <label className="block text-sm font-medium text-gray-300 mb-2">
                      Nhiệm kỳ
                    </label>
                    <Form.Item name="term">
                      <Input
                        className="w-full px-4 py-3 bg-gray-900 border border-gray-700 rounded-lg text-white placeholder-gray-500 focus:ring-2 focus:ring-white focus:border-transparent focus:bg-gray-900 focus:text-white outline-none transition-all"
                        placeholder="Nhiệm kỳ"
                      />
                    </Form.Item>
                  </div>

                  {/* Mô tả bản thân */}
                  <div>
                    <label className="block text-sm font-medium text-gray-300 mb-2">
                      Mô tả bản thân
                    </label>
                    <Form.Item name="profile">
                      <TextArea
                        rows={4}
                        maxLength={500}
                        showCount
                        className="w-full px-4 py-3 bg-gray-900 border border-gray-700 rounded-lg text-white placeholder-gray-500 focus:ring-2 focus:ring-white focus:border-transparent focus:bg-gray-900 focus:text-white outline-none transition-all"
                        placeholder="Mô tả bản thân"
                        style={{
                          backgroundColor: "#1f2937",
                          borderColor: "#374151",
                          color: "white",
                        }}
                      />
                    </Form.Item>
                  </div>

                  {/* Lĩnh vực */}
                  <div>
                    <label className="block text-sm font-medium text-gray-300 mb-2">
                      Lĩnh vực
                    </label>
                    <Form.Item name="fieldIds">
                      <Select
                        mode="multiple"
                        placeholder="Chọn lĩnh vực"
                        style={{ width: "100%" }}
                        className="giba-select-multiple"
                        showSearch
                        optionFilterProp="children"
                        filterOption={(input, option) => {
                          const children = option?.children;
                          if (typeof children === "string") {
                            return (children as string)
                              .toLowerCase()
                              .includes(input.toLowerCase());
                          }
                          return false;
                        }}
                      >
                        {fields.map((field) => (
                          <Option key={field.id} value={field.id}>
                            {field.fieldName || field.name || field.id}
                          </Option>
                        ))}
                      </Select>
                    </Form.Item>
                  </div>
                </div>
              ) : (
                <div className="space-y-4">
                  {/* Thông tin pháp nhân */}

                  {/* Logo công ty */}
                  <div>
                    <label className="block text-sm font-medium text-gray-300 mb-2">
                      Logo công ty
                    </label>
                    <div className="flex items-center space-x-4">
                      <div className="relative">
                        {profileData?.companyLogo ? (
                          <img
                            src={profileData.companyLogo}
                            alt="Logo công ty"
                            className="w-20 h-20 object-cover rounded-lg border border-gray-600"
                          />
                        ) : (
                          <div className="w-20 h-20 bg-gray-800 border border-gray-600 rounded-lg flex items-center justify-center">
                            <span className="text-gray-500 text-xs">
                              Chưa có logo
                            </span>
                          </div>
                        )}
                        <Button
                          type="primary"
                          size="small"
                          icon={<EditOutlined />}
                          loading={uploadingLogo}
                          onClick={() => fileInputRef.current?.click()}
                          className="absolute -top-2 -right-2 bg-red-600 border-red-600 hover:bg-red-700"
                        />
                      </div>
                      <div className="flex-1">
                        <p className="text-sm text-gray-400">
                          Nhấn vào nút chỉnh sửa để thay đổi logo công ty
                        </p>
                        <p className="text-xs text-gray-500 mt-1">
                          Định dạng: JPG, PNG (tối đa 5MB)
                        </p>
                      </div>
                    </div>
                    <input
                      ref={fileInputRef}
                      type="file"
                      accept="image/*"
                      onChange={handleFileSelect}
                      style={{ display: "none" }}
                    />
                  </div>

                  {/* Tên công ty */}
                  <div>
                    <label className="block text-sm font-medium text-gray-300 mb-2">
                      Tên công ty
                    </label>
                    <Form.Item name="companyFullName">
                      <Input
                        className="w-full px-4 py-3 bg-gray-900 border border-gray-700 rounded-lg text-white placeholder-gray-500 focus:ring-2 focus:ring-white focus:border-transparent focus:bg-gray-900 focus:text-white outline-none transition-all"
                        placeholder="Nhập tên đầy đủ công ty"
                      />
                    </Form.Item>
                  </div>

                  {/* Tên thương hiệu */}
                  <div>
                    <label className="block text-sm font-medium text-gray-300 mb-2">
                      Tên thương hiệu
                    </label>
                    <Form.Item name="companyBrandName">
                      <Input
                        className="w-full px-4 py-3 bg-gray-900 border border-gray-700 rounded-lg text-white placeholder-gray-500 focus:ring-2 focus:ring-white focus:border-transparent focus:bg-gray-900 focus:text-white outline-none transition-all"
                        placeholder="Nhập tên thương hiệu"
                      />
                    </Form.Item>
                  </div>

                  {/* Mã số thuế */}
                  <div>
                    <label className="block text-sm font-medium text-gray-300 mb-2">
                      Mã số thuế
                    </label>
                    <Form.Item name="taxCode">
                      <Input
                        className="w-full px-4 py-3 bg-gray-900 border border-gray-700 rounded-lg text-white placeholder-gray-500 focus:ring-2 focus:ring-white focus:border-transparent focus:bg-gray-900 focus:text-white outline-none transition-all"
                        placeholder="Nhập mã số thuế"
                      />
                    </Form.Item>
                  </div>

                  {/* Ngành nghề kinh doanh */}
                  <div>
                    <label className="block text-sm font-medium text-gray-300 mb-2">
                      Ngành nghề kinh doanh
                    </label>
                    <Form.Item name="businessField">
                      <Input
                        className="w-full px-4 py-3 bg-gray-900 border border-gray-700 rounded-lg text-white placeholder-gray-500 focus:ring-2 focus:ring-white focus:border-transparent focus:bg-gray-900 focus:text-white outline-none transition-all"
                        placeholder="Nhập ngành nghề kinh doanh"
                      />
                    </Form.Item>
                  </div>

                  {/* Loại hình doanh nghiệp */}
                  <div>
                    <label className="block text-sm font-medium text-gray-300 mb-2">
                      Loại hình doanh nghiệp
                    </label>
                    <Form.Item name="businessType">
                      <Select
                        placeholder="Chọn loại hình doanh nghiệp"
                        style={{ width: "100%", height: "48px" }}
                        className="giba-select"
                        options={[
                          { value: "Công ty TNHH", label: "Công ty TNHH" },
                          {
                            value: "Công ty Cổ phần",
                            label: "Công ty Cổ phần",
                          },
                          {
                            value: "Doanh nghiệp tư nhân",
                            label: "Doanh nghiệp tư nhân",
                          },
                          { value: "Hộ kinh doanh", label: "Hộ kinh doanh" },
                        ]}
                      />
                    </Form.Item>
                  </div>

                  {/* Địa chỉ trụ sở chính */}
                  <div>
                    <label className="block text-sm font-medium text-gray-300 mb-2">
                      Địa chỉ trụ sở chính
                    </label>
                    <Form.Item name="headquartersAddress">
                      <Input
                        className="w-full px-4 py-3 bg-gray-900 border border-gray-700 rounded-lg text-white placeholder-gray-500 focus:ring-2 focus:ring-white focus:border-transparent focus:bg-gray-900 focus:text-white outline-none transition-all"
                        placeholder="Nhập địa chỉ trụ sở chính"
                      />
                    </Form.Item>
                  </div>

                  {/* Website doanh nghiệp */}
                  <div>
                    <label className="block text-sm font-medium text-gray-300 mb-2">
                      Website doanh nghiệp
                    </label>
                    <Form.Item name="companyWebsite">
                      <Input
                        className="w-full px-4 py-3 bg-gray-900 border border-gray-700 rounded-lg text-white placeholder-gray-500 focus:ring-2 focus:ring-white focus:border-transparent focus:bg-gray-900 focus:text-white outline-none transition-all"
                        placeholder="Nhập website doanh nghiệp"
                      />
                    </Form.Item>
                  </div>

                  {/* SĐT doanh nghiệp */}
                  <div>
                    <label className="block text-sm font-medium text-gray-300 mb-2">
                      SĐT doanh nghiệp
                    </label>
                    <Form.Item name="companyPhoneNumber">
                      <Input
                        className="w-full px-4 py-3 bg-gray-900 border border-gray-700 rounded-lg text-white placeholder-gray-500 focus:ring-2 focus:ring-white focus:border-transparent focus:bg-gray-900 focus:text-white outline-none transition-all"
                        placeholder="Nhập số điện thoại doanh nghiệp"
                      />
                    </Form.Item>
                  </div>

                  {/* Email doanh nghiệp */}
                  <div>
                    <label className="block text-sm font-medium text-gray-300 mb-2">
                      Email doanh nghiệp
                    </label>
                    <Form.Item name="companyEmail">
                      <Input
                        className="w-full px-4 py-3 bg-gray-900 border border-gray-700 rounded-lg text-white placeholder-gray-500 focus:ring-2 focus:ring-white focus:border-transparent focus:bg-gray-900 focus:text-white outline-none transition-all"
                        placeholder="Nhập email doanh nghiệp"
                      />
                    </Form.Item>
                  </div>

                  {/* Người đại diện pháp lý */}
                  <div>
                    <label className="block text-sm font-medium text-gray-300 mb-2">
                      Người đại diện pháp lý
                    </label>
                    <Form.Item name="legalRepresentative">
                      <Input
                        className="w-full px-4 py-3 bg-gray-900 border border-gray-700 rounded-lg text-white placeholder-gray-500 focus:ring-2 focus:ring-white focus:border-transparent focus:bg-gray-900 focus:text-white outline-none transition-all"
                        placeholder="Nhập tên người đại diện pháp lý"
                      />
                    </Form.Item>
                  </div>

                  {/* Chức vụ của người đại diện */}
                  <div>
                    <label className="block text-sm font-medium text-gray-300 mb-2">
                      Chức vụ của người đại diện
                    </label>
                    <Form.Item name="legalRepresentativePosition">
                      <Input
                        className="w-full px-4 py-3 bg-gray-900 border border-gray-700 rounded-lg text-white placeholder-gray-500 focus:ring-2 focus:ring-white focus:border-transparent focus:bg-gray-900 focus:text-white outline-none transition-all"
                        placeholder="Nhập chức vụ của người đại diện"
                      />
                    </Form.Item>
                  </div>

                  {/* Giấy chứng nhận đăng ký kinh doanh */}
                  <div className="space-y-3">
                    <h4 className="text-lg font-medium text-white">
                      Giấy chứng nhận đăng ký kinh doanh
                    </h4>

                    {/* Số */}
                    <div>
                      <label className="block text-sm font-medium text-gray-300 mb-2">
                        Số
                      </label>
                      <Form.Item name="businessRegistrationNumber">
                        <Input
                          className="w-full px-4 py-3 bg-gray-900 border border-gray-700 rounded-lg text-white placeholder-gray-500 focus:ring-2 focus:ring-white focus:border-transparent focus:bg-gray-900 focus:text-white outline-none transition-all"
                          placeholder="Nhập số giấy chứng nhận"
                        />
                      </Form.Item>
                    </div>

                    {/* Ngày cấp */}
                    <div>
                      <label className="block text-sm font-medium text-gray-300 mb-2">
                        Ngày cấp
                      </label>
                      <Form.Item name="businessRegistrationDate">
                        <DatePicker
                          format="DD/MM/YYYY"
                          placeholder="Chọn ngày cấp"
                          style={{ width: "100%", height: "48px" }}
                          className="giba-datepicker"
                        />
                      </Form.Item>
                    </div>

                    {/* Nơi cấp */}
                    <div>
                      <label className="block text-sm font-medium text-gray-300 mb-2">
                        Nơi cấp
                      </label>
                      <Form.Item name="businessRegistrationPlace">
                        <Input
                          className="w-full px-4 py-3 bg-gray-900 border border-gray-700 rounded-lg text-white placeholder-gray-500 focus:ring-2 focus:ring-white focus:border-transparent focus:bg-gray-900 focus:text-white outline-none transition-all"
                          placeholder="Nhập nơi cấp giấy chứng nhận"
                        />
                      </Form.Item>
                    </div>
                  </div>
                </div>
              )}
            </Form>
          </>
        )}
      </div>

      {/* Submit Button - Fixed Bottom */}
      <div className="fixed bottom-0 left-0 right-0 px-4 py-4 bg-black border-t border-gray-800 z-50">
        <button
          onClick={() => onFinish(form.getFieldsValue())}
          className="w-full mb-2 py-3 px-6 rounded-lg font-semibold text-base transition-all duration-200 bg-white text-black hover:bg-gray-200 shadow-lg"
        >
          Lưu
        </button>
      </div>
    </Page>
  );
};

export default EditInfoGiba;
