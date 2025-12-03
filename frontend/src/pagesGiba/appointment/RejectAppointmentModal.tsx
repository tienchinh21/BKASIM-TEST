import React, { useState } from "react";
import { Modal, Input } from "antd";
import { X } from "lucide-react";
import { toast } from "react-toastify";

interface RejectAppointmentModalProps {
  visible: boolean;
  onClose: () => void;
  onConfirm: (reason: string) => Promise<void>;
  appointmentId?: string;
  isSender?: boolean;
}

const RejectAppointmentModal: React.FC<RejectAppointmentModalProps> = ({
  visible,
  onClose,
  onConfirm,
  appointmentId,
  isSender = false,
}) => {
  const [rejectReason, setRejectReason] = useState("");
  const [loading, setLoading] = useState(false);

  const handleClose = () => {
    setRejectReason("");
    onClose();
  };

  const modalTitle = isSender ? "Huỷ lịch hẹn" : "Từ chối lịch hẹn";
  const reasonLabel = isSender ? "Lý do huỷ" : "Lý do từ chối";
  const reasonPlaceholder = isSender
    ? "Vui lòng nhập lý do huỷ lịch hẹn này..."
    : "Vui lòng nhập lý do từ chối lịch hẹn này...";
  const confirmButtonText = isSender ? "Xác nhận huỷ" : "Xác nhận từ chối";
  const hintText = isSender
    ? "💡 Lý do huỷ sẽ được gửi đến người nhận lịch hẹn"
    : "💡 Lý do từ chối sẽ được gửi đến người tạo lịch hẹn";

  const handleSubmit = async () => {
    if (!rejectReason.trim()) {
      toast.error(`Vui lòng nhập lý do ${isSender ? "huỷ" : "từ chối"}`);
      return;
    }

    try {
      setLoading(true);
      await onConfirm(rejectReason);
      setRejectReason("");
    } catch (error) {
      console.error("Error rejecting appointment:", error);
      toast.error("Có lỗi xảy ra khi từ chối");
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal
      open={visible}
      onCancel={handleClose}
      title={
        <div className="flex items-center gap-2">
          <div className="w-10 h-10 bg-red-100 rounded-full flex items-center justify-center">
            <X size={20} className="text-red-600" />
          </div>
          <span className="text-lg font-semibold">{modalTitle}</span>
        </div>
      }
      width={500}
      centered
      footer={null}
      closeIcon={
        <span className="text-gray-500 hover:text-gray-700 text-xl">×</span>
      }
    >
      <div className="py-4">
        <div className="mb-4">
          <label className="block text-sm font-semibold mb-2 text-gray-700">
            {reasonLabel} <span className="text-red-500">*</span>
          </label>
          <Input.TextArea
            placeholder={reasonPlaceholder}
            value={rejectReason}
            onChange={(e) => setRejectReason(e.target.value)}
            rows={4}
            maxLength={300}
            showCount
            disabled={loading}
            autoFocus
          />
          {/* <div className="text-xs text-gray-500 mt-2">{hintText}</div> */}
        </div>

        <div className="flex gap-2 mt-4">
          <button
            onClick={handleClose}
            className="flex-1 px-4 py-2.5 bg-gray-100 hover:bg-gray-200 text-gray-700 rounded-lg font-medium transition-all"
            disabled={loading}
          >
            Hủy
          </button>
          <button
            onClick={handleSubmit}
            className="flex-1 px-4 py-2.5 bg-red-500 hover:bg-red-600 text-white rounded-lg font-semibold transition-all disabled:opacity-50 disabled:cursor-not-allowed"
            disabled={loading}
          >
            {loading ? "Đang xử lý..." : confirmButtonText}
          </button>
        </div>
      </div>
    </Modal>
  );
};

export default RejectAppointmentModal;
