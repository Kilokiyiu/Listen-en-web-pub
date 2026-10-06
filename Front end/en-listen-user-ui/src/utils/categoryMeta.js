/** 首页 / 听力列表共用的分类展示配置 */

export const homeCategoryMeta = {
  cet6: {
    title: '英语六级听力练习',
    subtitle: '历年真题，助你轻松过级',
    listTitle: '六级听力真题',
    color: '#a78bfa',
    blessing: '六级加油。把今天这套听完，就已经赢过昨天的自己。',
  },
  cet4: {
    title: '英语四级听力练习',
    subtitle: '历年真题 + 模拟试题',
    listTitle: '四级听力真题',
    color: '#22d3ee',
    blessing: '四级稳过。认真听、仔细选，下一套真题见。',
  },
  ielts: {
    title: '雅思听力练习',
    subtitle: '剑桥雅思真题 + 模拟训练',
    listTitle: '雅思真题',
    color: '#22c55e',
    blessing: 'Keep calm and listen carefully. 细心，就能多拿一分。',
  },
  toefl: {
    title: '托福听力练习',
    subtitle: 'TPO真题 + 专项训练',
    listTitle: '托福真题',
    color: '#fbbf24',
    blessing: '托福听力多练多记，节奏稳住就会顺。',
  },
  kaoyan: {
    title: '考研英语',
    subtitle: '开发中 · 模拟完形 / 阅读练习',
    listTitle: '考研英语',
    color: '#ef4444',
    blessing: '考研路上不孤单。先把长难句啃下来，一步一步来。',
  },
}

export const listCategoryMeta = {
  cet6: { title: '六级听力真题', label: 'CET-6' },
  cet4: { title: '四级听力真题', label: 'CET-4' },
  ielts: { title: '雅思听力真题', label: 'IELTS' },
  toefl: { title: '托福听力真题', label: 'TOEFL' },
}

/**
 * 首页独立入口（非听力、不走文章服务）。
 */
export const homeExtraCategories = [
  {
    id: '__extra_kaoyan',
    code: 'kaoyan',
    name: { chinese: '考研英语', english: 'Kaoyan' },
    kind: 'kaoyan',
    /** 开发中：可体验模拟做题，非正式真题库 */
    comingSoon: true,
    entryRoute: { name: 'kaoyan' },
  },
]

export function mergeHomeCategoriesWithExtras(apiCategories = []) {
  const codes = new Set(apiCategories.map((c) => c.code))
  const extras = homeExtraCategories.filter((c) => !codes.has(c.code))
  return [...apiCategories, ...extras]
}
