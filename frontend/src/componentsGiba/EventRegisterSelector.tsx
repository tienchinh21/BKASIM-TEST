import React from "react";
import { Drawer, Button } from "antd";
import { UserOutlined, TeamOutlined } from "@ant-design/icons";
import { useNavigate } from "react-router-dom";

interface EventRegisterSelectorProps {
  visible: boolean;
  onClose: () => void;
  eventId: string;
  onSuccess?: () => void;
  isPersonalRegistered?: boolean; // Thêm prop để biết đã đăng ký cá nhân chưa
}

const EventRegisterSelector: React.FC<EventRegisterSelectorProps> = ({
  visible,
  onClose,
  eventId,
  onSuccess,
  isPersonalRegistered = false,
}) => {
  const navigate = useNavigate();

  const handlePersonalRegister = (e: React.MouseEvent) => {
    e.stopPropagation();
    onClose();
    navigate("/giba/event-register", { state: { eventId } });
  };

  const handleGuestRegister = (e: React.MouseEvent) => {
    e.stopPropagation();
    onClose();
    navigate("/giba/event-guest-register", { state: { eventId } });
  };

  const handleClose = (e?: React.MouseEvent) => {
    if (e) {
      e.stopPropagation();
    }
    onClose();
  };

  return (
    <div onClick={(e) => e.stopPropagation()}>
      <Drawer
        title="Chọn hình thức đăng ký"
        placement="bottom"
        height={300}
        open={visible}
        onClose={() => handleClose()}
        style={{ borderRadius: "15px 15px 0 0" }}
        styles={{
          body: { padding: "20px" },
        }}
        maskClosable={false}
        closable={true}
        closeIcon={
          <span
            onClick={(e) => {
              e.stopPropagation();
              handleClose();
            }}
          >
            ×
          </span>
        }
      >
        <div className="space-y-4">
          <div className="text-center mb-6">
            <p className="text-gray-600 text-sm">
              Bạn muốn đăng ký cho hội nhóm như thế nào?
            </p>
          </div>

          <div className="space-y-3">
            <Button
              onClick={
                isPersonalRegistered ? undefined : handlePersonalRegister
              }
              disabled={isPersonalRegistered}
              className="w-full h-16 flex items-center justify-between text-left"
              style={{
                background: isPersonalRegistered ? "#f5f5f5" : "white",
                border: isPersonalRegistered
                  ? "2px solid #ccc"
                  : "2px solid #000",
                color: isPersonalRegistered ? "#999" : "#000",
                borderRadius: "12px",
                cursor: isPersonalRegistered ? "not-allowed" : "pointer",
              }}
              onMouseDown={(e) => e.stopPropagation()}
            >
              <div className="flex items-center space-x-4">
                <UserOutlined style={{ fontSize: "24px" }} />
                <div>
                  <div className="font-semibold text-base">
                    {isPersonalRegistered
                      ? "Đã đăng ký cá nhân"
                      : "Đăng ký cá nhân"}
                  </div>
                  <div className="text-sm text-gray-500">
                    {isPersonalRegistered
                      ? "Bạn đã đăng ký cá nhân"
                      : "Đăng ký cho bản thân"}
                  </div>
                </div>
              </div>
            </Button>

            <Button
              onClick={handleGuestRegister}
              className="w-full h-16 flex items-center justify-between text-left"
              style={{
                background: "white",
                border: "2px solid #000",
                color: "#000",
                borderRadius: "12px",
              }}
              onMouseDown={(e) => e.stopPropagation()}
            >
              <div className="flex items-center space-x-4">
                <TeamOutlined style={{ fontSize: "24px" }} />
                <div>
                  <div className="font-semibold text-base">
                    Đăng ký danh sách khách mời
                  </div>
                  <div className="text-sm text-gray-500">
                    Đăng ký cho nhiều người
                  </div>
                </div>
              </div>
            </Button>
          </div>

          <div className="pt-4">
            <Button
              onClick={(e) => handleClose(e)}
              className="w-full h-10 text-sm font-medium"
              size="middle"
              onMouseDown={(e) => e.stopPropagation()}
            >
              Hủy
            </Button>
          </div>
        </div>
      </Drawer>
    </div>
  );
};

export default EventRegisterSelector;
