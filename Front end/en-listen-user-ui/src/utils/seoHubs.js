/**
 * SEO 栏目页配置（路径型 URL，便于搜索引擎按关键词收录多条结果）
 * 暂不含雅思 / 托福（内容未上线）
 *
 * 全站定位参考搜索意图（竞品词持续迭代）：
 * - 四六级备考网站 / 大学四六级练习网站 / 英语刷题网站
 * - 免费四级刷题软件、四级往年试卷、英语四级真题汇总
 * - 六级听力真题、英语六级听力、大学英语六级考试卷
 * - 考研英语二阅读原文；六级作文真题（整卷 PDF 查阅）
 */

export const SEO_HUBS = {
  cet4: {
    path: '/cet4',
    code: 'cet4',
    kind: 'listening',
    badge: 'CET-4',
    h1: '英语四级真题汇总_免费听力刷题',
    subtitle: '四级往年试卷听力汇总在线练，看原文、下载试卷答案',
    title: '英语四级真题汇总_四级往年试卷免费刷题 - ListenEase',
    description:
      '英语四级真题汇总专区：按套整理四级往年试卷听力，免费在线刷题（网页版），含音频、听力原文、试卷与答案 PDF，服务四六级备考。',
    keywords:
      '英语四级真题汇总,四级往年试卷,免费四级刷题软件,四级真题,英语四级听力,四级试卷下载,CET-4,四六级备考,ListenEase',
    intro: [
      '这里汇总大学英语四级真题听力套卷，相当于英语四级真题汇总入口：打开即可免费刷题，无需安装软件。',
      '支持听音频、对照听力原文、下载四级往年试卷与答案；六级备考可前往六级听力真题或四六级备考专题。',
    ],
    related: [
      { label: '六级听力真题', to: '/cet6' },
      { label: '四六级备考专题', to: '/si-liu-ji' },
      { label: '往年试卷列表', to: '/exams' },
      { label: '官方词本', to: '/word-packs' },
    ],
  },
  cet6: {
    path: '/cet6',
    code: 'cet6',
    kind: 'listening',
    badge: 'CET-6',
    h1: '六级听力真题_大学英语六级考试卷练习',
    subtitle: '六级听力真题在线刷；整卷考试卷 PDF 可查阅作文题',
    title: '六级听力真题在线练习_大学英语六级考试卷下载 - ListenEase',
    description:
      '六级听力真题专区：大学英语六级考试卷听力部分免费在线练习，含长对话、篇章与讲座；整卷考试卷 PDF 可下载查阅六级作文真题等笔试题型。',
    keywords:
      '六级听力真题,大学英语六级考试卷,英语六级听力,六级作文真题,六级刷题,CET-6,六级试卷下载,四六级备考,ListenEase',
    intro: [
      '备考六级需要系统刷六级听力真题。本站按年份汇总大学英语六级考试卷听力套题，免费在线练并复盘原文。',
      '下载整份大学英语六级考试卷 PDF，还可查阅作文等笔试部分；四级可前往英语四级真题汇总页。',
    ],
    related: [
      { label: '英语四级真题汇总', to: '/cet4' },
      { label: '四六级备考专题', to: '/si-liu-ji' },
      { label: '官方词本', to: '/word-packs' },
    ],
  },
  siLiuJi: {
    path: '/si-liu-ji',
    code: 'si-liu-ji',
    kind: 'combo',
    badge: 'CET',
    h1: '四六级备考网站专题',
    subtitle: '英语四级真题汇总 + 六级听力真题一站入口',
    title: '四六级备考网站_四级真题汇总与六级听力真题 - ListenEase',
    description:
      '四六级备考网站专题：汇总英语四级真题、四级往年试卷与六级听力真题、大学英语六级考试卷在线练习，免费刷题并下载试卷答案。',
    keywords:
      '四六级备考网站,大学四六级练习网站,英语四级真题汇总,六级听力真题,大学英语六级考试卷,四级往年试卷,免费刷题,ListenEase',
    intro: [
      'ListenEase 定位为四六级备考网站：把英语四级真题汇总与六级听力真题入口放在同一页，方便收藏和分享。',
      '可分别进入四级往年试卷或大学英语六级考试卷听力列表，全程免费在线练习。',
    ],
    comboCodes: ['cet4', 'cet6'],
    related: [
      { label: '英语四级真题汇总', to: '/cet4' },
      { label: '六级听力真题', to: '/cet6' },
      { label: '考研英语刷题', to: '/kaoyan' },
      { label: '每日短文', to: '/daily' },
    ],
  },
  listening: {
    path: '/listening',
    code: 'listening',
    kind: 'hub',
    badge: '听力',
    h1: '四六级备考_听力真题免费刷题入口',
    subtitle: '英语四级真题汇总、六级听力真题与考试卷在线练',
    title: '四六级备考听力入口_四级真题与六级听力真题 - ListenEase',
    description:
      '四六级备考听力入口：英语四级真题汇总、六级听力真题与大学英语六级考试卷免费在线刷题，含原文与试卷下载。',
    keywords:
      '四六级备考,英语四级真题汇总,六级听力真题,大学英语六级考试卷,英语刷题网站,四级往年试卷,ListenEase',
    intro: [
      '选择考试类型进入真题列表，免费刷四级往年试卷或六级听力真题；网页打开即用。',
      '除听力外，还可配合考研阅读、每日短文、BBC 新闻与词本继续备考。',
    ],
    hubLinks: [
      { title: '英语四级真题汇总', desc: '四级往年试卷 · 免费刷', to: '/cet4', tag: 'CET-4' },
      { title: '六级听力真题', desc: '大学英语六级考试卷', to: '/cet6', tag: 'CET-6' },
      { title: '四六级备考专题', desc: '四级 + 六级入口', to: '/si-liu-ji', tag: 'CET' },
      { title: '考研英语刷题', desc: '英语二阅读原文', to: '/kaoyan', tag: '考研' },
    ],
    related: [
      { label: '每日短文', to: '/daily' },
      { label: 'BBC 新闻', to: '/bbc-news' },
      { label: '官方词本', to: '/word-packs' },
    ],
  },
  kaoyan: {
    path: '/kaoyan',
    code: 'kaoyan',
    kind: 'route-meta-only',
    title: '考研英语刷题_英语二阅读原文免费在线练习 - ListenEase',
    description:
      '四六级备考之余可练考研：英语二阅读原文与英语一/二历年真题免费在线刷题，完形与阅读提交后看解析；整卷 PDF 可查阅作文题。',
    keywords:
      '考研英语刷题,考研英语二阅读原文,考研英语2阅读原文,英语二历年真题,英语二作文,考研阅读,免费刷题,ListenEase',
  },
  daily: {
    path: '/daily',
    code: 'daily',
    kind: 'route-meta-only',
    title: '每日英语短文_四六级备考阅读补充 - ListenEase',
    description:
      '每天一篇精选英语短文免费阅读，英汉对照，配合四六级备考与听力真题刷题保持语感。',
    keywords: '每日英语短文,四六级备考,英语阅读,英语刷题,ListenEase',
  },
  bbcNews: {
    path: '/bbc-news',
    code: 'bbc-news',
    kind: 'route-meta-only',
    title: 'BBC英语新闻_四六级备考语感阅读 - ListenEase',
    description:
      '精选 BBC 英语新闻免费阅读，作为四六级备考网站的阅读补充，提升语感与理解力。',
    keywords: 'BBC英语新闻,四六级备考,英语新闻阅读,ListenEase',
  },
  wordRoots: {
    path: '/word-roots',
    code: 'word-roots',
    kind: 'route-meta-only',
    title: '英语词根词缀_辅助四级真题与六级听力备考 - ListenEase',
    description:
      '免费学习词根词缀，扩大词汇量，辅助英语四级真题汇总刷题与六级听力真题备考。',
    keywords: '词根词缀,四级真题,六级听力真题,四六级备考,ListenEase',
  },
  wordPacks: {
    path: '/word-packs',
    code: 'word-packs',
    kind: 'route-meta-only',
    title: '四六级考研词本_备考刷题记单词 - ListenEase',
    description:
      '免费领取四级、六级、考研高频词本，配合英语四级真题与六级听力真题刷题记单词。',
    keywords: '四六级词本,四级词汇,六级词汇,四六级备考,ListenEase',
  },
  reading: {
    path: '/reading',
    code: 'reading',
    kind: 'route-meta-only',
    title: '英语阅读_四六级备考与考研阅读原文 - ListenEase',
    description:
      '阅读入口：每日短文、BBC 新闻与考研英语二阅读原文，配合四级真题汇总与六级听力真题备考。',
    keywords: '英语阅读,四六级备考,考研英语二阅读原文,ListenEase',
  },
  exams: {
    path: '/exams',
    code: 'exams',
    kind: 'route-meta-only',
    title: '英语四级真题汇总与六级考试卷列表_免费刷题 - ListenEase',
    description:
      '浏览英语四级真题汇总、四级往年试卷与六级听力真题、大学英语六级考试卷列表，免费在线刷题并下载 PDF。',
    keywords:
      '英语四级真题汇总,四级往年试卷,六级听力真题,大学英语六级考试卷,四六级备考网站,试卷下载,ListenEase',
  },
}

/** 需要独立落地页组件的 hub（listening / combo / 单级听力） */
export const SEO_LANDING_HUBS = Object.values(SEO_HUBS).filter((h) =>
  ['listening', 'combo', 'hub'].includes(h.kind)
)

export function getSeoHubByPath (path) {
  return Object.values(SEO_HUBS).find((h) => h.path === path) || null
}

export function getSeoHubByCode (code) {
  return Object.values(SEO_HUBS).find((h) => h.code === code) || null
}

/** 听力分类 code → 栏目路径（首页「查看全部」用） */
export const LISTENING_CODE_TO_PATH = {
  cet4: '/cet4',
  cet6: '/cet6',
}
