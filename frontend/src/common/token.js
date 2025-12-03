import { getStorage, setStorage } from "zmp-sdk/apis";
import dataDf from "./DefaultConfig.json";
import axios from "axios";

const getDataToken = async () => {
  let tokenData;
  try {
    const { token } = await getStorage({
      keys: ["token"],
    });
    if (token) {
      tokenData = token;
    }
  } catch (error) {
    // xử lý khi gọi api thất bại
    console.log(error);
  }
  return tokenData;
};

const handleLogin = async () => {
  let data = JSON.stringify({
    UserName: dataDf.UserName,
    Password: dataDf.Password,
  });
  let config = {
    method: "post",
    maxBodyLength: Infinity,
    url: `${dataDf.url}/api/Authenticate/LoginAdmin`,
    headers: {
      "Content-Type": "application/json",
    },
    data: data,
  };
  var rs = "";
  var response = await axios.request(config);

  if (response.data.code === 1) {
    rs = await setDataToStorage(response.data.token);
    // console.log("Token:", response.data);
  }
  return rs;
};
const setDataToStorage = async (token) => {
  try {
    const { errorKeys } = await setStorage({
      data: {
        token: token,
      },
    });
    return "ok";
  } catch (error) {
    return "error";
  }
};

export { getDataToken, handleLogin };
