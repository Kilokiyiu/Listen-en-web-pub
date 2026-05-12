import request, { identityRequest } from "./Request";

//----------这是登录的接口（走IdentityService 5263）----------

//邮箱登录
export function loginByEmail(data){
    return identityRequest.post("/Login/LoginByEmailAndPwd", data)
}

//用户名登录
export function loginByUserName(data){
    return identityRequest.post("/Login/LoginByUserNameAndPwd", data)
}

//注册用户
export function register(data) {
    return identityRequest.post('/Login/Register', data)
}

//获取用户当前信息
export function getUserInfo(){
    return identityRequest.get("/Login/GetUserInfo")
}