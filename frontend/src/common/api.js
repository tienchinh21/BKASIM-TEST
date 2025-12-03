import axios from "axios";
import dfData from "./DefaultConfig.json";
import { getDataToken, handleLogin } from "./token";

const api = axios.create({
  baseURL: dfData.domain + "/api",
  // baseURL: dfData.url ,
  timeout: 15000,
  headers: {
    "Content-Type": "application/json",
    "ngrok-skip-browser-warning": "true",
  },
});

api.interceptors.request.use(async function (config) {
  var tokenData = await getDataToken();
  let isLogged = Boolean(tokenData);
  if (isLogged) {
    let token = tokenData;
    config.headers["Authorization"] = `Bearer ${token}`;
  }
  // else {
  //   await handleLogin();
  //   tokenData = await getDataToken();
  //   config.headers["Authorization"] = `Bearer ${tokenData}`;
  // }
  return config;
});

api.interceptors.response.use(
  (response) => {
    return response.data;
  },
  async (error) => {
    const status = error.response.status;
    if (status === 401) {
      // Gọi hàm login và sau đó tiếp tục gọi lại API
      await handleLogin();
      const tokenData = await getDataToken();
      error.config.headers["Authorization"] = `Bearer ${tokenData}`;
      const retryResponse = await axios.request(error.config);
      return retryResponse.data;
    } else {
      return Promise.reject(error);
    }
    //return Promise.reject(error);
  }
);

export default api;
