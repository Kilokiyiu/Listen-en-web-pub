import axios from "axios";
import { ElMessage } from "element-plus";

//----------这是整个前端请求的入口，所有的API都是基于此----------

//这个是听力服务的入口
const request = axios.create({
    baseURL: "/api/listen",
    timeout: 10000,
})

//这是认证服务的入口
export const identityRequest = axios.create({
    baseURL: "/api/identity",
    timeout: 10000,
})

//这是文章服务的入口
export const articleRequest = axios.create({
    baseURL: "/api/article",
    timeout: 10000,
})

//两个实例共用的token拦截器
const addTokenInterceptor = (instance) => {
    instance.interceptors.request.use(
        config => {
            const token = localStorage.getItem('token');
            if (token) {
                config.headers.Authorization = `Bearer ${token}`
            }
            return config;
        },
        error => Promise.reject(error)
    )
    instance.interceptors.response.use(
        response => response.data,
        error => {
            let msg;
            if (error.response?.status === 401) {
                msg = "请先登录";
            } else if (error.response?.data) {
                msg = error.response.data;
            } else if (error.code === 'ECONNREFUSED') {
                msg = "服务未启动，请稍后重试";
            } else {
                msg = "网络错误，请稍后重试";
            }
            ElMessage.error(msg)
            if(error.response?.status === 401){
                localStorage.removeItem('token');
                localStorage.removeItem('username');
                window.location.href="/#/login"
            }
            return Promise.reject(error)
        }
    )
}

addTokenInterceptor(request)
addTokenInterceptor(articleRequest)
addTokenInterceptor(identityRequest)

export default request;