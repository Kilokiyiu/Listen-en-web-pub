/** 本地演示用听力做题数据（无需后端 / 数据库） */

export const MOCK_ALBUM_ID = 'mock'

const SEC_A1 = 'mock-sec-a1'
const SEC_A2 = 'mock-sec-a2'
const SEC_B1 = 'mock-sec-b1'

/** questionId -> correct option index */
export const MOCK_ANSWER_KEY = {
  'mock-q1': 1,
  'mock-q2': 0,
  'mock-q3': 2,
  'mock-q4': 3,
  'mock-q5': 1,
  'mock-q6': 0,
}

export const mockAlbum = {
  id: MOCK_ALBUM_ID,
  name: { chinese: '【演示】做题模式预览' },
  categoryId: 'mock-cet4',
  /** 本地静态演示 PDF（不走 /api/listen） */
  paperFileUrl: '/demo/mock-paper.pdf',
  answerFileUrl: '/demo/mock-answer.pdf',
  _mockLocalFiles: true,
}

export const mockEpisodes = [
  {
    id: 'mock-ep-1',
    name: { chinese: '演示音频（可跳过）' },
    audioUrl: '',
    subtitle:
      'This is a short demo transcript for the whole paper.\n\nYou can ignore the missing audio and focus on the two practice modes.',
  },
]

export const mockQuizPaper = {
  albumId: MOCK_ALBUM_ID,
  sectionCount: 3,
  questionCount: 6,
  sections: [
    {
      id: SEC_A1,
      groupName: 'Section A',
      title: 'News Report 1',
      transcript:
        'A local library has launched a weekend reading club for teenagers. Organizers say the goal is to help students build reading habits and discuss books with peers.',
      audioUrl: '',
      questions: [
        {
          id: 'mock-q1',
          number: 1,
          stem: 'What is the library launching?',
          options: ['A sports camp', 'A weekend reading club', 'A cooking class', 'A coding contest'],
        },
        {
          id: 'mock-q2',
          number: 2,
          stem: 'Who is the club mainly for?',
          options: ['Teenagers', 'Toddlers', 'Teachers only', 'Tourists'],
        },
      ],
    },
    {
      id: SEC_A2,
      groupName: 'Section A',
      title: 'News Report 2',
      transcript:
        'City officials announced a temporary traffic ban near the river park during the holiday weekend to reduce congestion and keep visitors safe.',
      audioUrl: '',
      questions: [
        {
          id: 'mock-q3',
          number: 3,
          stem: 'Where will the traffic ban take place?',
          options: ['Near the airport', 'In the downtown mall', 'Near the river park', 'On the highway only'],
        },
        {
          id: 'mock-q4',
          number: 4,
          stem: 'What is one purpose of the ban?',
          options: ['Raise parking fees', 'Promote car sales', 'Extend shopping hours', 'Reduce congestion'],
        },
      ],
    },
    {
      id: SEC_B1,
      groupName: 'Section B',
      title: 'Conversation 1',
      transcript:
        'M: Do you want to review the listening notes together tonight?\nW: Sure. Let’s meet at the study room after dinner.',
      audioUrl: '',
      questions: [
        {
          id: 'mock-q5',
          number: 5,
          stem: 'What does the man suggest?',
          options: ['Skipping class', 'Reviewing notes together', 'Going to a concert', 'Changing majors'],
        },
        {
          id: 'mock-q6',
          number: 6,
          stem: 'When will they meet?',
          options: ['After dinner', 'Before breakfast', 'At noon', 'On Sunday morning'],
        },
      ],
    },
  ],
}

const EXPLAIN = {
  'mock-q1': '原文提到 weekend reading club。',
  'mock-q2': '面向 teenagers。',
  'mock-q3': '禁行区域在 river park 附近。',
  'mock-q4': '目的之一是 reduce congestion。',
  'mock-q5': '男士提议一起复习听力笔记。',
  'mock-q6': '晚饭后在自习室见面。',
}

/**
 * 模拟 SubmitQuiz 判分
 * @param {string} albumId
 * @param {{ questionId: string, selectedIndex: number }[]} answers
 * @param {string|null} [sectionId]
 */
export function mockSubmitQuiz(albumId, answers, sectionId = null) {
  const sections = mockQuizPaper.sections.filter(
    (s) => !sectionId || s.id === sectionId
  )
  const questions = sections.flatMap((s) =>
    (s.questions || []).map((q) => ({ ...q, sectionId: s.id }))
  )
  const answerMap = Object.fromEntries(
    (answers || []).map((a) => [a.questionId, a.selectedIndex])
  )

  const results = questions.map((q) => {
    const correctAnswer = MOCK_ANSWER_KEY[q.id] ?? 0
    const hasAnswer = Object.prototype.hasOwnProperty.call(answerMap, q.id)
    const selectedIndex = hasAnswer ? answerMap[q.id] : null
    const isCorrect = hasAnswer && selectedIndex === correctAnswer
    return {
      questionId: q.id,
      sectionId: q.sectionId,
      number: q.number,
      stem: q.stem,
      options: q.options,
      selectedIndex,
      correctAnswer,
      isCorrect,
      explanation: EXPLAIN[q.id] || '',
    }
  })

  const correctCount = results.filter((r) => r.isCorrect).length
  const total = results.length
  return Promise.resolve({
    albumId,
    sectionId: sectionId || null,
    total,
    correctCount,
    scorePercent: total ? Math.round((correctCount * 1000) / total) / 10 : 0,
    results,
  })
}

export const isMockAlbum = (albumId) =>
  String(albumId || '') === MOCK_ALBUM_ID || String(albumId || '') === 'demo'
