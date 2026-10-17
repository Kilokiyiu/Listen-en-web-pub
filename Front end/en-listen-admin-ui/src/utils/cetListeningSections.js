/** 四六级听力 Section 官方题型名称（与 user-ui 保持一致） */

export const GROUP_ORDER = ['Section A', 'Section B', 'Section C']

export function normalizeCetCode(code) {
  const raw = String(code || '').toLowerCase().replace(/[^a-z0-9]/g, '')
  if (raw.includes('cet6') || raw === '6') return 'cet6'
  if (raw.includes('cet4') || raw === '4') return 'cet4'
  if (String(code || '').includes('六')) return 'cet6'
  if (String(code || '').includes('四')) return 'cet4'
  return ''
}

export function sectionTypeLabels(code) {
  if (normalizeCetCode(code) === 'cet6') {
    return {
      'Section A': '长对话',
      'Section B': '听力篇章',
      'Section C': '讲座/讲话',
    }
  }
  return {
    'Section A': '短篇新闻',
    'Section B': '长对话',
    'Section C': '听力篇章',
  }
}

export function formatGroupTabLabel(groupName, code) {
  const type = sectionTypeLabels(code)[groupName]
  return type ? `${groupName} · ${type}` : groupName
}

export function defaultPassageTitle(group, index, code) {
  const n = Number(index) || 1
  if (normalizeCetCode(code) === 'cet6') {
    if (group === 'Section A') return `Long Conversation ${n}`
    if (group === 'Section B') return `Passage ${n}`
    return `Lecture ${n}`
  }
  if (group === 'Section A') return `News Report ${n}`
  if (group === 'Section B') return `Long Conversation ${n}`
  return `Passage ${n}`
}

export function normalizeGroupName(name) {
  const raw = String(name || '').trim()
  if (!raw) return ''
  const hit = GROUP_ORDER.find((g) => g.toLowerCase() === raw.toLowerCase())
  if (hit) return hit
  const compact = raw.replace(/\s+/g, '').toUpperCase()
  if (compact === 'A' || compact === 'SECTIONA') return 'Section A'
  if (compact === 'B' || compact === 'SECTIONB') return 'Section B'
  if (compact === 'C' || compact === 'SECTIONC') return 'Section C'
  return ''
}

export function inferGroupName(groupName, title, code) {
  const fromField = normalizeGroupName(groupName)
  if (fromField) return fromField

  const t = String(title || '')
  const m = t.match(/section\s*([abc])/i)
  if (m) return `Section ${m[1].toUpperCase()}`

  const level = normalizeCetCode(code)

  if (level === 'cet6') {
    if (/lecture|talk|讲座|讲话/i.test(t)) return 'Section C'
    if (/passage|篇章/i.test(t)) return 'Section B'
    if (/long\s*conversation|conversation|dialogue|对话/i.test(t)) return 'Section A'
    if (/news\s*report|short\s*news|新闻/i.test(t)) return 'Section A'
    return 'Section A'
  }

  if (/news\s*report|short\s*news|新闻/i.test(t)) return 'Section A'
  if (/long\s*conversation|conversation|dialogue|对话/i.test(t)) return 'Section B'
  if (/lecture|talk|讲座|讲话|passage|篇章/i.test(t)) return 'Section C'
  return 'Section A'
}
