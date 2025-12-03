import React from "react";
import { Modal } from "antd";
import { User, Users } from "lucide-react";

interface RefOptionModalProps {
  visible: boolean;
  onClose: () => void;
  onSelectOption: (option: "option1" | "option2") => void;
}

const RefOptionModal: React.FC<RefOptionModalProps> = ({
  visible,
  onClose,
  onSelectOption,
}) => {
  return (
    <Modal
      open={visible}
      onCancel={onClose}
      footer={null}
      title="Chọn loại Referral"
      centered
      width={400}
      closeIcon={
        <span className="text-gray-500 hover:text-gray-700 text-xl">×</span>
      }
    >
      <div className="space-y-3 py-4">
        <button
          onClick={() => onSelectOption("option1")}
          className="w-full p-4 rounded-lg border-2 border-gray-200 hover:border-yellow-400 hover:bg-yellow-50 transition-all duration-200 flex items-center gap-4 group"
        >
          <div className="w-12 h-12 bg-yellow-100 rounded-full flex items-center justify-center group-hover:bg-yellow-200 transition-all">
            <User size={24} className="text-yellow-600" />
          </div>
          <div className="flex-1 text-left">
            <div className="font-semibold text-gray-900 mb-1">
              Trao thông tin Referral cho thành viên
            </div>
            <div className="text-xs text-gray-500">
              Gửi thông tin Referral đến thành viên trong nhóm
            </div>
          </div>
        </button>

        <button
          onClick={() => onSelectOption("option2")}
          className="w-full p-4 rounded-lg border-2 border-gray-200 hover:border-blue-400 hover:bg-blue-50 transition-all duration-200 flex items-center gap-4 group"
        >
          <div className="w-12 h-12 bg-blue-100 rounded-full flex items-center justify-center group-hover:bg-blue-200 transition-all">
            <Users size={24} className="text-blue-600" />
          </div>
          <div className="flex-1 text-left">
            <div className="font-semibold text-gray-900 mb-1">
              Trao thông tin thành viên cho người nhận bên ngoài
            </div>
            <div className="text-xs text-gray-500">
              Gửi thông tin thành viên đến người bên ngoài
            </div>
          </div>
        </button>
      </div>
    </Modal>
  );
};

export default RefOptionModal;
