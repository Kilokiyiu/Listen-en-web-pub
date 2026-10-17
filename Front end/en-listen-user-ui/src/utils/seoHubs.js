/**
 * SEO 栏目页配置（路径型 URL，便于搜索引擎按关键词收录多条结果）
 * 暂不含雅思 / 托福（内容未上线）
 */

export const SEO_HUBS = {
  cet4: {
    path: '/cet4',
    code: 'cet4',
    kind: 'listening',
    badge: 'CET-4',
    h1: '英语四级听力真题在线练习',
    subtitle: '历年大学英语四级听力真题，在线播放、看原文、下载试卷与答案',
    title: '英语四级听力真题在线练习 - ListenEase',
    description:
      'ListenEase 提供大学英语四级（CET-4）历年听力真题在线练习：音频播放、听力原文、试卷与答案 PDF 下载，助力四级听力备考。',
    intro: [
      '英语四级听力是四六级考试的重要部分。本站按套卷整理历年真题，支持在线听音频、对照原文、下载试卷与答案。',
      '选择下方任意一套开始练习；也可进入「四六级听力」专题同时查看六级资源。',
    ],
    related: [
      { label: '英语六级听力', to: '/cet6' },
      { label: '四六级听力专题', to: '/si-liu-ji' },
      { label: '官方词本', to: '/word-packs' },
    ],
  },
  cet6: {
    path: '/cet6',
    code: 'cet6',
    kind: 'listening',
    badge: 'CET-6',
    h1: '英语六级听力真题在线练习',
    subtitle: '历年大学英语六级听力真题，长对话、篇章与讲座在线练',
    title: '英语六级听力真题在线练习 - ListenEase',
    description:
      'ListenEase 提供大学英语六级（CET-6）历年听力真题在线练习：长对话、听力篇章、讲座讲话，含音频、原文与 PDF 下载。',
    intro: [
      '六级听力题型包含长对话、听力篇章与讲座讲话。本站按套卷整理，方便按年份刷题与复盘原文。',
      '需要同时备考四级时，可前往四级听力或四六级专题页。',
    ],
    related: [
      { label: '英语四级听力', to: '/cet4' },
      { label: '四六级听力专题', to: '/si-liu-ji' },
      { label: '官方词本', to: '/word-packs' },
    ],
  },
  siLiuJi: {
    path: '/si-liu-ji',
    code: 'si-liu-ji',
    kind: 'combo',
    badge: 'CET',
    h1: '四六级听力真题专题',
    subtitle: '大学英语四级 + 六级听力真题一站练习',
    title: '四六级听力真题在线练习专题 - ListenEase',
    description:
      '四六级听力真题专题：英语四级、英语六级历年听力在线练，含音频、原文、试卷与答案下载，服务大学英语四六级备考。',
    intro: [
      '备考四六级时，听力需要按级别系统刷题。本专题汇总四级与六级听力入口，便于收藏与分享。',
      '也可直接进入「四级听力」或「六级听力」查看完整套卷列表。',
    ],
    comboCodes: ['cet4', 'cet6'],
    related: [
      { label: '四级听力', to: '/cet4' },
      { label: '六级听力', to: '/cet6' },
      { label: '考研英语', to: '/kaoyan' },
      { label: '每日短文', to: '/daily' },
    ],
  },
  listening: {
    path: '/listening',
    code: 'listening',
    kind: 'hub',
    badge: '听力',
    h1: '英语听力练习与真题资料',
    subtitle: '四级、六级听力真题在线练，配套原文与学习资料',
    title: '英语听力练习与真题资料 - ListenEase',
    description:
      'ListenEase 英语听力练习入口：大学英语四级、六级历年听力真题在线练习，听力原文与试卷下载，持续更新备考资料。',
    intro: [
      '在这里选择你的目标考试，进入对应听力真题列表开始练习。',
      '除真题外，还可配合每日短文、BBC 新闻与词本巩固语感与词汇。',
    ],
    hubLinks: [
      { title: '英语四级听力', desc: 'CET-4 历年真题', to: '/cet4', tag: 'CET-4' },
      { title: '英语六级听力', desc: 'CET-6 历年真题', to: '/cet6', tag: 'CET-6' },
      { title: '四六级专题', desc: '四级 + 六级入口汇总', to: '/si-liu-ji', tag: 'CET' },
      { title: '考研英语', desc: '完形与阅读在线练', to: '/kaoyan', tag: '考研' },
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
    title: '考研英语完形填空与阅读在线练习 - ListenEase',
    description:
      '考研英语一 / 英语二完形填空与阅读理解在线练习，提交后查看正确答案与解析，服务考研英语备考。',
  },
  daily: {
    path: '/daily',
    code: 'daily',
    kind: 'route-meta-only',
    title: '每日英语短文阅读与学习资料 - ListenEase',
    description:
      '每天一篇精选英语短文，英汉对照阅读，配合听力练习保持语感，适合四六级与考研日常积累。',
  },
  bbcNews: {
    path: '/bbc-news',
    code: 'bbc-news',
    kind: 'route-meta-only',
    title: 'BBC 英语新闻阅读学习资料 - ListenEase',
    description:
      '精选 BBC 英语新闻阅读与学习资料，提升语感、词汇与阅读理解，适合四六级及进阶英语学习。',
  },
  wordRoots: {
    path: '/word-roots',
    code: 'word-roots',
    kind: 'route-meta-only',
    title: '英语词根词缀学习资料 - ListenEase',
    description:
      '系统学习英语词根词缀与例词，扩大词汇量，辅助四六级、考研英语单词记忆。',
  },
  wordPacks: {
    path: '/word-packs',
    code: 'word-packs',
    kind: 'route-meta-only',
    title: '四六级考研官方高频词本 - ListenEase',
    description:
      '领取英语四级、六级、考研官方高频词本，加入个人单词本，用间隔复习高效背单词。',
  },
  reading: {
    path: '/reading',
    code: 'reading',
    kind: 'route-meta-only',
    title: '英语阅读练习入口 - 短文新闻考研 - ListenEase',
    description:
      '英语阅读入口：每日短文、BBC 新闻与考研阅读相关练习，配合听力备考提升综合能力。',
  },
  exams: {
    path: '/exams',
    code: 'exams',
    kind: 'route-meta-only',
    title: '英语听力真题列表 - 四级六级 - ListenEase',
    description:
      '浏览大学英语四级、六级听力历年真题列表，在线练习并下载试卷与答案 PDF。',
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
