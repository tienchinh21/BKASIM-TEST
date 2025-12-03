import React, { useState } from "react";
import { Modal, Input } from "antd";
import { AlertCircle } from "lucide-react";

const { TextArea } = Input;

interface RejectModalProps {
  visible: boolean;
  memberName: string;
  onConfirm: (reason: string) => void;
  onCancel: () => void;
  loading?: boolean;
}

const RejectModal: React.FC<RejectModalProps> = ({
  visible,
  memberName,
  onConfirm,
  onCancel,
  loading = false,
}) => {
  const [reason, setReason] = useState("");

  const handleConfirm = () => {
    if (!reason.trim()) {
      return;
    }
    onConfirm(reason);
    setReason("");
  };

  const handleCancel = () => {
    setReason("");
    onCancel();
  };

  return (
    <Modal
      open={visible}
      title={
        <div className="flex items-center gap-2">
          <AlertCircle className="w-5 h-5 text-red-600" />
          <span className="font-bold text-lg">Từ chối thành viên</span>
        </div>
      }
      onOk={handleConfirm}
      onCancel={handleCancel}
      okText="Xác nhận từ chối"
      cancelText="Hủy"
      confirmLoading={loading}
      okButtonProps={{
        danger: true,
        disabled: !reason.trim(),
      }}
      width={500}
    >
      <div className="py-4">
        <p className="text-gray-700 mb-4">
          Bạn có chắc chắn muốn từ chối thành viên{" "}
          <span className="font-bold">{memberName}</span>?
        </p>

        <div className="mb-2">
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Lý do từ chối <span className="text-red-500">*</span>
          </label>
          <TextArea
            value={reason}
            onChange={(e) => setReason(e.target.value)}
            placeholder="Nhập lý do từ chối thành viên..."
            rows={4}
            maxLength={500}
            showCount
            className="w-full"
          />
        </div>

        <p className="text-xs text-gray-500 mt-2">
          Lý do từ chối sẽ được gửi đến thành viên qua email hoặc thông báo.
        </p>
      </div>
    </Modal>
  );
};

export default RejectModal;

