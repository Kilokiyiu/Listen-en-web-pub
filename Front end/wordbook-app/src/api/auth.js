import { identityApi } from './http'

export function loginByEmail(data) {
  return identityApi.post('/Login/LoginByEmailAndPwd', data)
}

export function loginByUserName(data) {
  return identityApi.post('/Login/LoginByUserNameAndPwd', data)
}

export function register(data) {
  return identityApi.post('/Login/Register', data)
}

export function getUserInfo() {
  return identityApi.get('/Login/GetUserInfo')
}
