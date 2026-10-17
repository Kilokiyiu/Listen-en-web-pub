/** 手机端「阅读 / 单词」栏目内切换 */

export const readingTabs = [
  { label: '每日短文', path: '/daily', names: ['dailyArticle'] },
  { label: 'BBC 新闻', path: '/bbc-news', names: ['bbcNews'] },
  { label: '全部', path: '/reading', names: ['readingHub'], secondary: true },
]

export const wordsTabs = [
  { label: '单词本', path: '/my-words', names: ['myWords'] },
  { label: '词根', path: '/word-roots', names: ['wordRoots', 'wordRootDetail'], matchPrefix: '/word-roots', secondary: true },
  { label: '复习', path: '/word-review', names: ['wordReview'], secondary: true },
  { label: '全部', path: '/words', names: ['wordsHub'], secondary: true },
]
