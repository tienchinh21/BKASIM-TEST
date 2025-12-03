import React, { forwardRef, useImperativeHandle, useState } from "react";
import { Sheet, Box, useNavigate } from "zmp-ui";
import { Form, Input, Button } from "antd";
import { useRecoilValue } from "recoil";
import { infoShare, token } from "../../../recoil/RecoilState";
import axios from "axios";
import dfData from "../../../common/DefaultConfig.json";
import { toast } from "react-toastify";

const PopUpJoinEvent = forwardRef((props, ref) => {
  const { id } = props;
  const navigate = useNavigate();
  const shareInfo = useRecoilValue(infoShare);
  const tokenAuth = useRecoilValue(token);

  const [visible, setVisible] = useState(false);
  const [form] = Form.useForm();

  useImperativeHandle(ref, () => ({
    open: () => {
      setVisible(true);

      form.setFieldsValue({
        name: shareInfo?.displayName || "",
        phone: shareInfo?.phoneNumber || "",
        email: "",
      });
    },
    close: () => setVisible(false),
  }));

  const handleSubmit = (values) => {
    const { name, phone, email } = values;

    if (email && !/^[\w-.]+@([\w-]+\.)+[a-zA-Z]{2,}$/.test(email)) {
      toast.error("Email không hợp lệ");
      return;
    }

    axios
      .post(
        `${dfData.domain}/api/EventRegistrations/Register/${id}`,
        {
          name,
          phone,
          email,
        },
        {
          headers: { Authorization: `Bearer ${tokenAuth}` },
        }
      )
      .then((res) => {
        if (res.data.code === 0) {
          toast.success("Đăng ký sự kiện thành công!");
          navigate("/success-event");
          setVisible(false);
        } else {
          toast.error(res.data.message);
        }
      })
      .catch((err) => {
        console.error("Đăng ký thất bại:", err);
        toast.error("Đăng ký thất bại. Vui lòng thử lại!");
      });
  };

  return (
    <Sheet
      visible={visible}
      onClose={() => setVisible(false)}
      style={{ padding: "0 20px" }}
      title={<strong>Đăng ký tham gia</strong>}
    >
      <Form
        form={form}
        layout="vertical"
        onFinish={handleSubmit}
        style={{ marginTop: 16 }}
      >
        <Form.Item
          style={{ marginBottom: "10px" }}
          label="Tên người nhận"
          name="name"
          rules={[{ required: true, message: "Vui lòng nhập họ tên" }]}
        >
          <Input placeholder="Nhập họ và tên" style={{ height: 40 }} />
        </Form.Item>

        <Form.Item
          style={{ marginBottom: "10px" }}
          label="Số điện thoại"
          name="phone"
          rules={[
            { required: true, message: "Vui lòng nhập số điện thoại" },
            {
              pattern: /^(0\d{9,10}|84\d{9,10})$/,
              message: "Số điện thoại không hợp lệ",
            },
          ]}
        >
          <Input placeholder="Nhập số điện thoại" style={{ height: 40 }} />
        </Form.Item>

        <Form.Item
          label="Email"
          name="email"
          rules={[{ type: "email", message: "Email không hợp lệ" }]}
        >
          <Input placeholder="Nhập email" style={{ height: 40 }} />
        </Form.Item>

        <Form.Item>
          <Button
            type="primary"
            htmlType="submit"
            block
            style={{
              background: "var(--background-button-color)",
              border: "none",
              fontWeight: "bold",
            }}
          >
            Đăng ký
          </Button>
        </Form.Item>
      </Form>
    </Sheet>
  );
});

export default PopUpJoinEvent;
