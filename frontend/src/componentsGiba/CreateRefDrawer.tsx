import React, { useState } from "react";
import { Drawer, Input, Button, Row, Col, Select } from "antd";
import { toast } from "react-toastify";
import { useRecoilValue } from "recoil";
import { token } from "../recoil/RecoilState";
import axios from "axios";
import dfData from "../common/DefaultConfig.json";
const { TextArea } = Input;
const { Option } = Select;

interface Member {
  membershipId: string;
  membershipName: string;
  phoneNumber: string;
  userZaloId?: string;
}

interface CreateRefDrawerProps {
  visible: boolean;
  onClose: () => void;
  members: Member[];
  groupId: string;
  onSuccess?: () => void;
}

const CreateRefDrawer: React.FC<CreateRefDrawerProps> = ({
  visible,
  onClose,
  members,
  groupId,
  onSuccess,
}) => {
  const userToken = useRecoilValue(token);
  const [loading, setLoading] = useState(false);
  const [formData, setFormData] = useState({
    refTo: "",
    content: "",
    groupId: groupId,
  });

  const handleSubmit = async () => {
    if (!formData.content.trim()) {
      toast.error("Vui lòng nhập nội dung ref");
      return;
    }

    if (!formData.refTo) {
      toast.error("Vui lòng chọn thành viên");
      return;
    }

    if (formData.content.length > 1000) {
      toast.error("Nội dung ref không được vượt quá 1000 ký tự");
      return;
    }

    try {
      setLoading(true);
      const response = await axios.post(
        `${dfData.domain}/api/refs/create`,
        formData,
        {
          headers: {
            Authorization: `Bearer ${userToken}`,
          },
        }
      );

      if (response.data.success || response.data.code === 0) {
        const successMessage =
          response.data.message || "Tạo Referral thành công!";
        toast.success(successMessage);
        onSuccess?.();
        onClose();
        setFormData({ refTo: "", content: "", groupId: groupId });
      } else {
        toast.error(response.data.message || "Có lỗi xảy ra khi tạo Referral");
      }
    } catch (error) {
      console.error("Error creating ref:", error);
      const errorMessage =
        (error as any).response?.data?.message ||
        "Có lỗi xảy ra khi tạo Referral";
      toast.error(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div onClick={(e) => e.stopPropagation()}>
      <Drawer
        title="Tạo Referral"
        placement="bottom"
        height={450}
        open={visible}
        onClose={() => onClose()}
        style={{ borderRadius: "15px 15px 0 0" }}
        styles={{
          body: { padding: "0" },
        }}
        maskClosable={false}
        closable={true}
        closeIcon={
          <span
            onClick={(e) => {
              e.stopPropagation();
              onClose();
            }}
          >
            ×
          </span>
        }
      >
        <div className="flex flex-col h-full">
          <div className="flex-1 overflow-y-auto p-5 pb-20">
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Nội dung ref <span className="text-red-500">*</span>
                </label>
                <TextArea
                  placeholder="Nhập nội dung ref (tối đa 1000 ký tự)"
                  value={formData.content}
                  onChange={(e) =>
                    setFormData({ ...formData, content: e.target.value })
                  }
                  rows={6}
                  maxLength={1000}
                  showCount
                  className="w-full"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Chọn thành viên <span className="text-red-500">*</span>
                </label>
                <Select
                  style={{ height: 40 }}
                  placeholder="Chọn thành viên nhận ref"
                  value={formData.refTo || undefined}
                  onChange={(value) =>
                    setFormData({ ...formData, refTo: value })
                  }
                  className="w-full"
                  showSearch
                  filterOption={(input, option) =>
                    (option?.children as unknown as string)
                      ?.toLowerCase()
                      .includes(input.toLowerCase())
                  }
                >
                  {members.map((member) => (
                    <Option
                      key={member.membershipId}
                      value={member.userZaloId || member.membershipId}
                    >
                      {member.membershipName} - {member.phoneNumber}
                    </Option>
                  ))}
                </Select>
              </div>
            </div>
          </div>

          <div className="fixed bottom-0 left-0 right-0 bg-white border-t border-gray-200 p-4">
            <Row gutter={12}>
              <Col span={12}>
                <Button
                  onClick={onClose}
                  className="w-full h-10 text-sm font-medium"
                  size="middle"
                >
                  Hủy
                </Button>
              </Col>
              <Col span={12}>
                <Button
                  type="primary"
                  onClick={handleSubmit}
                  loading={loading}
                  className="w-full bg-black hover:bg-gray-800 h-10 text-sm font-medium"
                  size="middle"
                >
                  {loading ? "Đang tạo..." : "Tạo Referral"}
                </Button>
              </Col>
            </Row>
          </div>
        </div>
      </Drawer>
    </div>
  );
};

export default CreateRefDrawer;
