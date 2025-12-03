import React, {
  useState,
  useImperativeHandle,
  forwardRef,
  useRef,
} from "react";
import { Modal as ZmpModal, Box, Text, Button, Icon, Input } from "zmp-ui";
import axios from "axios";
import dfData from "../../common/DefaultConfig.json";
import { toast } from "react-toastify";
import { useRecoilValue } from "recoil";
import { infoShare, token } from "../../recoil/RecoilState";

const PopupWithdrawMoney = forwardRef((props, ref) => {
  const [modalVisible, setModalVisible] = useState(false);
  const [amount, setAmount] = useState("");
  const [phone, setPhone] = useState("");
  const [accountDetail, setAccountDetail] = useState("");
  const [amountError, setAmountError] = useState("");
  const [phoneError, setPhoneError] = useState("");
  const [accountDetailError, setAccountDetailError] = useState("");
  const tokenAuth = useRecoilValue(token);

  const amountInputRef = useRef(null);
  const phoneInputRef = useRef(null);
  const accountDetailInputRef = useRef(null);

  useImperativeHandle(ref, () => ({
    open,
  }));

  const open = () => {
    setModalVisible(true);
  };

  const close = () => {
    setModalVisible(false);
    setAmount("");
    setPhone("");
    setAccountDetail("");
    setAmountError("");
    setPhoneError("");
    setAccountDetailError("");
  };

  const handleInputChange = (e) => {
    let value = e.target.value;

    // Loại bỏ tất cả ký tự không phải là số
    value = value.replace(/[^\d]/g, "");

    // Định dạng số với dấu phẩy
    const formattedValue = Number(value)
      .toLocaleString("en-US")
      .replace(/,/g, ",");

    // Cập nhật giá trị số tiền
    setAmount(formattedValue);

    // Xoá lỗi nếu có khi người dùng nhập liệu
    if (amountError) setAmountError("");
  };

  const handleWithdraw = async () => {
    let hasError = false;

    // Reset tất cả các lỗi trước
    setAmountError("");
    setPhoneError("");
    setAccountDetailError("");
    if (amount == "") {
      setAmountError("Vui lòng nhập số tiền.");
      if (!hasError && amountInputRef.current) {
        amountInputRef.current.focus();
      }
      hasError = true;
    } else if (Number(amount.replace(/,/g, "")) < 10000) {
      setAmountError("Số tiền phải lớn hơn 10,000 VNĐ");
      if (!hasError && amountInputRef.current) {
        amountInputRef.current.focus();
      }
      hasError = true;
    }

    if (!phone) {
      setPhoneError("Vui lòng nhập số điện thoại.");
      if (!hasError && phoneInputRef.current) {
        phoneInputRef.current.focus();
      }
      hasError = true;
    } else if (!/^\d{10}$/.test(phone)) {
      setPhoneError("Số điện thoại không hợp lệ. Phải gồm 10 chữ số.");
      if (!hasError && phoneInputRef.current) {
        phoneInputRef.current.focus();
      }
      hasError = true;
    }

    if (!accountDetail) {
      setAccountDetailError("Vui lòng nhập thông tin tài khoản.");
      if (!hasError && accountDetailInputRef.current) {
        accountDetailInputRef.current.focus();
      }
      hasError = true;
    }

    if (hasError) {
      toast.error("Vui lòng điền đầy đủ thông tin.");
      return;
    }
    try {
      const response = await axios.post(
        `${dfData.domain}/api/WithdrawRequests`,
        {
          Amount: Number(amount.replace(/,/g, "")), // Loại bỏ dấu phẩy và chuyển thành số
          CusPhoneNumber: phone,
          AccountDetail: accountDetail,
        },
        {
          headers: {
            Authorization: `Bearer ${tokenAuth}`,
          },
        }
      );
      if (response.data.code === 0) {
        toast.success("Yêu cầu rút tiền đã được ghi nhận!");
        close();
        if (props.reload) {
          props.reload();
        }
      } else {
        toast.error("Rút tiền thất bại");
      }
    } catch (error) {
      toast.error("Có lỗi xảy ra, vui lòng thử lại!");
    }
  };

  return (
    <>
      <ZmpModal
        visible={modalVisible}
        onClose={() => {
          close();
        }}
        modalClassName="custom-referred-list-modal"
        width="95%"
        maskClosable={true}
        title={
          <Text
            style={{ color: "#426D9E", fontWeight: "bold", fontSize: "18px" }}
          >
            Nhập số tiền bạn muốn rút
          </Text>
        }
      >
        <Box mt={4}>
          <Text
            style={{
              fontWeight: "bold",
              fontSize: "16px",
            }}
          >
            Số tiền rút <span style={{ color: "red" }}>*</span>
          </Text>
          <Input
            ref={amountInputRef}
            style={{ marginTop: 8 }}
            placeholder="Nhập số tiền muốn rút..."
            value={amount}
            onChange={handleInputChange}
          />
          {amountError && (
            <Text
              type="danger"
              style={{ marginTop: 4, color: "red", marginLeft: 2 }}
            >
              <Icon
                icon="zi-warning-solid"
                size={16}
                style={{ marginBottom: 3 }}
              />
              {amountError}
            </Text>
          )}
          <Text
            style={{
              fontWeight: "bold",
              fontSize: "16px",
              marginTop: "12px",
            }}
          >
            Số điện thoại <span style={{ color: "red" }}>*</span>
          </Text>
          <Input
            ref={phoneInputRef}
            style={{ marginTop: 8 }}
            placeholder="Nhập số điện thoại của bạn..."
            maxLength={10}
            inputMode="tel"
            pattern="[0-9]*"
            value={phone}
            onChange={(e) => {
              setPhone(e.target.value.replace(/\D/g, ""));
              if (phoneError) setPhoneError("");
            }}
          />
          {phoneError && (
            <Text
              type="danger"
              style={{ marginTop: 4, color: "red", marginLeft: 2 }}
            >
              <Icon
                icon="zi-warning-solid"
                size={16}
                style={{ marginBottom: 3 }}
              />
              {phoneError}
            </Text>
          )}
          <Text
            style={{
              fontWeight: "bold",
              fontSize: "16px",
              marginTop: "12px",
            }}
          >
            Thông tin tài khoản <span style={{ color: "red" }}>*</span>
          </Text>
          <Input.TextArea
            ref={accountDetailInputRef}
            style={{ marginTop: 8 }}
            placeholder="Tên, tài khoản, ngân hàng..."
            value={accountDetail}
            onChange={(e) => {
              setAccountDetail(e.target.value);
              if (accountDetailError) setAccountDetailError("");
            }}
          />
          {accountDetailError && (
            <Text
              type="danger"
              style={{ marginTop: 4, color: "red", marginLeft: 2 }}
            >
              <Icon
                icon="zi-warning-solid"
                size={16}
                style={{ marginBottom: 3 }}
              />
              {accountDetailError}
            </Text>
          )}
          <Box
            style={{
              padding: "6px 20px",
              backgroundColor: "#fff",
              border: "2px solid #6991BF",
              borderRadius: 12,
              color: "#6991BF",
              width: "fit-content",
              margin: "16px auto 0",
              fontWeight: "bold",
            }}
            onClick={() => handleWithdraw()}
          >
            RÚT
          </Box>
        </Box>
      </ZmpModal>
    </>
  );
});

export default PopupWithdrawMoney;
