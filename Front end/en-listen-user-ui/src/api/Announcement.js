import request from './Request'

/** 公告板：站点运行状态 + 已发布公告 */
export const getAnnouncementBoard = () => request.get('/Listen/GetAnnouncementBoard')
