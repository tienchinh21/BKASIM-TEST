import React, { useState, useEffect } from "react";
import { Row, Col, Drawer, Button, Input, Checkbox } from "antd";
import { toast } from "react-toastify";
import { useRecoilValue } from "recoil";
import { isRegister, token } from "../recoil/RecoilState";
import dfData from "../common/DefaultConfig.json";
import axios from "axios";
import GroupRulesDrawer from "./GroupRulesDrawer";

interface RegisterDrawerProps {
  visible: boolean;
  onClose: () => void;
  groupId: string;
  groupName: string;
  onSuccess?: () => void;
}

const RegisterDrawer: React.FC<RegisterDrawerProps> = ({
  visible,
  onClose,
  groupId,
  groupName,
  onSuccess,
}) => {
  const userToken = useRecoilValue(token);
  const [loading, setLoading] = useState(false);
  const [formData, setFormData] = useState({
    reason: "",
  });
  const [isAgreedRules, setIsAgreedRules] = useState(false);
  const [rulesDrawerVisible, setRulesDrawerVisible] = useState(false);

  useEffect(() => {
    if (visible && groupId) {
      const savedData = sessionStorage.getItem(`registerDrawer_${groupId}`);
      if (savedData) {
        try {
          const parsed = JSON.parse(savedData);
          setFormData(parsed.formData || { reason: "" });
          setIsAgreedRules(parsed.isAgreedRules || false);
        } catch (error) {
          console.error("Error parsing saved drawer data:", error);
        }
      }
    }
  }, [visible, groupId]);

  useEffect(() => {
    if (visible && groupId) {
      const dataToSave = {
        formData,
        isAgreedRules,
        timestamp: Date.now(),
      };
      sessionStorage.setItem(
        `registerDrawer_${groupId}`,
        JSON.stringify(dataToSave)
      );
    }
  }, [formData, isAgreedRules, visible, groupId]);

  const clearSavedData = () => {
    if (groupId) {
      sessionStorage.removeItem(`registerDrawer_${groupId}`);
    }
  };

  const handleSubmit = async () => {
    if (!formData.reason.trim()) {
      toast.error("Vui lòng nhập lý do tham gia");
      return;
    }
    if (!isAgreedRules) {
      toast.error("Vui lòng đồng ý với quy tắc ứng xử của nhóm");
      return;
    }
    if (!isRegister) {
      toast.error("Vui lòng đăng nhập để sử dụng tính năng này!");
      return;
    }

    let loadingToastId: any = null;

    try {
      setLoading(true);

      const payload = {
        GroupId: groupId,
        reason: formData.reason.trim(),
        company: formData.company.trim() || "",
        position: formData.position.trim() || "",
      };

      const response = await axios.post(
        `${dfData.domain}/api/groups/${groupId}/join`,
        payload,
        {
          headers: {
            Authorization: `Bearer ${userToken}`,
          },
        }
      );

      // Close loading message
      if (loadingToastId) {
        toast.dismiss(loadingToastId);
      }

      if (
        response.data.success ||
        response.data.code === 0 ||
        response.data.code === 1
      ) {
        // Handle both success (code 0) and pending approval (code 1)
        const isPending = response.data.code === 1;
        const successMessage = isPending
          ? response.data.message ||
            "Đã gửi đơn xin tham gia nhóm! Đang chờ xét duyệt..."
          : response.data.message || "Đã gửi đơn xin tham gia nhóm thành công!";

        toast.success(successMessage);
        clearSavedData(); // Clear saved data on success
        onSuccess?.();
        onClose();
        // Reset form
        setFormData({
          reason: "",
          company: "",
          position: "",
        });
        setIsAgreedRules(false);
      } else {
        toast.error(
          response.data.message || "Có lỗi xảy ra khi gửi đơn xin tham gia"
        );
      }
    } catch (error) {
      console.error("Error joining group:", error);
      // Close loading message if it exists
      if (loadingToastId) {
        toast.dismiss(loadingToastId);
      }

      // Extract error message from axios error response
      let errorMessage =
        "Có lỗi xảy ra khi gửi đơn xin tham gia. Vui lòng thử lại!";

      if ((error as any).response?.data?.message) {
        errorMessage = (error as any).response.data.message;
      } else if ((error as any).message) {
        errorMessage = (error as any).message;
      }

      toast.error(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  // Kiểm tra form có đầy đủ thông tin và đã tick checkbox chưa
  const isFormValid = () => {
    return formData.reason.trim() && isAgreedRules;
  };

  const handleInputChange = (field: string, value: string) => {
    setFormData((prev) => ({
      ...prev,
      [field]: value,
    }));
  };

  // Hàm mở tài liệu quy tắc nhóm - mở drawer thay vì navigate
  const openGroupRules = () => {
    setRulesDrawerVisible(true);
  };

  return (
    <Drawer
      title={`Tham gia nhóm: ${groupName}`}
      placement="bottom"
      height={500}
      open={visible}
      style={{ borderRadius: "15px 15px 0 0" }}
      onClose={onClose}
      footer={
        <div className="flex gap-3 p-4">
          <Button
            onClick={onClose}
            className="flex-1 h-10 text-sm font-medium"
            size="middle"
          >
            Hủy
          </Button>
          <Button
            type="primary"
            onClick={handleSubmit}
            loading={loading}
            disabled={!isFormValid()}
            className={`flex-1 h-10 text-sm font-medium ${
              isFormValid()
                ? "bg-black hover:bg-gray-800"
                : "bg-gray-400 cursor-not-allowed"
            }`}
            size="middle"
          >
            {loading ? "Đang gửi..." : "Gửi đơn xin tham gia"}
          </Button>
        </div>
      }
    >
      <div className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Lý do tham gia <span className="text-red-500">*</span>
          </label>
          <Input.TextArea
            value={formData.reason}
            onChange={(e) => handleInputChange("reason", e.target.value)}
            placeholder="Hãy chia sẻ lý do bạn muốn tham gia nhóm này..."
            rows={4}
            maxLength={500}
            showCount
          />
        </div>

        <Row gutter={16}>
          <Col span={12}>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Công ty
              </label>
              <Input
                style={{
                  height: 40,
                }}
                value={formData.company}
                onChange={(e) => handleInputChange("company", e.target.value)}
                placeholder="Tên công ty (tùy chọn)"
                maxLength={100}
              />
            </div>
          </Col>
          <Col span={12}>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Vị trí
              </label>
              <Input
                style={{
                  height: 40,
                }}
                value={formData.position}
                onChange={(e) => handleInputChange("position", e.target.value)}
                placeholder="Vị trí công việc (tùy chọn)"
                maxLength={100}
              />
            </div>
          </Col>
        </Row>

        {/* Phần hiển thị tài liệu quy tắc nhóm */}
        <div className="bg-gray-50 border border-gray-200 p-3 rounded-lg">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 bg-blue-100 rounded-lg flex items-center justify-center">
                <svg
                  className="w-5 h-5 text-blue-600"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
                  />
                </svg>
              </div>
              <div>
                <p className="text-sm font-medium text-gray-900">
                  Quy tắc ứng xử của nhóm
                </p>
                <p className="text-xs text-gray-500">Xem tài liệu quy định</p>
              </div>
            </div>
            <button
              onClick={openGroupRules}
              className="px-3 py-1.5 bg-blue-600 text-white text-xs font-medium rounded-md hover:bg-blue-700 transition-colors"
            >
              Xem tài liệu
            </button>
          </div>
        </div>

        {/* Checkbox đồng ý quy tắc */}
        <div className="bg-yellow-50 border border-yellow-200 p-3 rounded-lg">
          <Checkbox
            checked={isAgreedRules}
            onChange={(e) => setIsAgreedRules(e.target.checked)}
            className="w-full"
          >
            <div className="flex-1">
              <p className="text-sm text-gray-700 leading-relaxed">
                Tôi đã đọc, hiểu và đồng ý tuân thủ{" "}
                <span className="text-blue-600 font-semibold">
                  Quy tắc ứng xử của nhóm
                </span>{" "}
                <span className="text-red-500 font-bold">*</span>
              </p>
            </div>
          </Checkbox>
        </div>

        <div className="bg-blue-50 border border-blue-200 p-3 rounded-lg">
          <p className="text-sm text-blue-800">
            <strong>Quy trình xét duyệt:</strong>
          </p>
          <ul className="text-xs text-blue-700 mt-2 space-y-1">
            <li>• Đơn xin tham gia sẽ được gửi đến quản trị viên nhóm</li>
            <li>• Quản trị viên sẽ xem xét và phê duyệt trong 24-48h</li>
            <li>• Bạn sẽ nhận được thông báo khi có kết quả</li>
            <li>• Có thể kiểm tra trạng thái trong mục "Nhóm của tôi"</li>
          </ul>

          {/* Helper text khi form chưa hợp lệ */}
          {!isFormValid() && (
            <div className="mt-3 p-2 bg-yellow-50 border border-yellow-200 rounded-md">
              <p className="text-xs text-yellow-700">
                <strong>Để gửi đơn xin tham gia:</strong>
                <br />
                • Vui lòng nhập lý do tham gia
                <br />• Vui lòng đồng ý với quy tắc ứng xử của nhóm
              </p>
            </div>
          )}
        </div>
      </div>

      {/* Group Rules Drawer */}
      <GroupRulesDrawer
        visible={rulesDrawerVisible}
        onClose={() => setRulesDrawerVisible(false)}
        groupId={groupId}
      />
    </Drawer>
  );
};

export default RegisterDrawer;
