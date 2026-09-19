export type Priority = 0 | 1 | 2

export const PRIORITY_LABELS: Record<Priority, string> = {
  0: 'Low',
  1: 'Medium',
  2: 'High',
}

export interface Task {
  id: number
  title: string
  description: string | null
  isCompleted: boolean
  priority: Priority
  dueDate: string | null
  createdAt: string
}

export interface TaskFields {
  title: string
  description: string
  priority: Priority
  /** `YYYY-MM-DD` from a date input, or empty for no due date. */
  dueDate: string
}

export function toFields(task?: Task): TaskFields {
  return {
    title: task?.title ?? '',
    description: task?.description ?? '',
    priority: task?.priority ?? 1,
    // The API returns e.g. 2026-09-30T00:00:00, the date input wants just the date part.
    dueDate: task?.dueDate?.slice(0, 10) ?? '',
  }
}

export interface Page<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

interface Session {
  accessToken: string
  refreshToken: string
}

const BASE = '/api/v1'
const STORAGE_KEY = 'taskapi.session'

export class ApiError extends Error {
  status: number

  constructor(status: number, message: string) {
    super(message)
    this.status = status
  }
}

function loadSession(): Session | null {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    return raw ? (JSON.parse(raw) as Session) : null
  } catch {
    return null
  }
}

let session = loadSession()
let onSessionEnded: () => void = () => {}

function saveSession(next: Session | null) {
  session = next
  try {
    if (next) localStorage.setItem(STORAGE_KEY, JSON.stringify(next))
    else localStorage.removeItem(STORAGE_KEY)
  } catch {
    // Storage can be unavailable (private mode); the session then lasts until reload.
  }
}

export function hasSession() {
  return session !== null
}

/** Called when the refresh token is rejected and the user has to sign in again. */
export function setSessionEndedHandler(handler: () => void) {
  onSessionEnded = handler
}

async function readError(res: Response): Promise<string> {
  const text = await res.text()
  try {
    const body: unknown = JSON.parse(text)
    if (typeof body === 'string') return body
    if (body && typeof body === 'object') {
      const { errors, detail, title } = body as {
        errors?: Record<string, string[]>
        detail?: string
        title?: string
      }
      if (errors) return Object.values(errors).flat().join(' ')
      if (detail ?? title) return (detail ?? title)!
    }
  } catch {
    // Not JSON, the API returns plain text for auth errors.
  }
  return text || res.statusText
}

let refreshing: Promise<boolean> | null = null

// One shared refresh: the API rotates refresh tokens, so parallel refreshes would invalidate each other.
function refresh(): Promise<boolean> {
  refreshing ??= doRefresh().finally(() => {
    refreshing = null
  })
  return refreshing
}

async function doRefresh(): Promise<boolean> {
  if (!session) return false
  const res = await fetch(`${BASE}/auth/refresh`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ refreshToken: session.refreshToken }),
  })
  if (!res.ok) return false
  saveSession(await res.json())
  return true
}

interface RequestOptions {
  auth?: boolean
  retried?: boolean
}

async function request<T>(
  path: string,
  init: RequestInit = {},
  { auth = true, retried = false }: RequestOptions = {},
): Promise<T> {
  const headers = new Headers(init.headers)
  if (init.body) headers.set('Content-Type', 'application/json')
  if (auth && session) headers.set('Authorization', `Bearer ${session.accessToken}`)

  const res = await fetch(BASE + path, { ...init, headers })

  if (res.status === 401 && auth && !retried) {
    if (await refresh()) return request<T>(path, init, { auth, retried: true })
    saveSession(null)
    onSessionEnded()
  }

  if (!res.ok) throw new ApiError(res.status, await readError(res))
  return (res.status === 204 ? undefined : await res.json()) as T
}

function credentials(username: string, password: string): RequestInit {
  return { method: 'POST', body: JSON.stringify({ username, password }) }
}

export async function login(username: string, password: string) {
  saveSession(await request('/auth/login', credentials(username, password), { auth: false }))
}

export async function register(username: string, password: string) {
  saveSession(await request('/auth/register', credentials(username, password), { auth: false }))
}

export async function logout() {
  const token = session?.refreshToken
  saveSession(null)
  if (!token) return
  try {
    await request('/auth/logout', { method: 'POST', body: JSON.stringify({ refreshToken: token }) }, { auth: false })
  } catch {
    // Signing out locally already happened; a failed revoke is not worth surfacing.
  }
}

function toBody(fields: TaskFields) {
  return {
    title: fields.title,
    description: fields.description || null,
    priority: fields.priority,
    // No zone suffix on purpose: the date stays exactly what the user picked.
    dueDate: fields.dueDate ? `${fields.dueDate}T00:00:00` : null,
  }
}

export function listTasks(page: number, pageSize = 10) {
  // The API marks this response cacheable for 30 seconds, so ask the browser not to reuse it.
  return request<Page<Task>>(`/tasks?page=${page}&pageSize=${pageSize}`, { cache: 'no-store' })
}

export function createTask(fields: TaskFields) {
  return request<Task>('/tasks', { method: 'POST', body: JSON.stringify(toBody(fields)) })
}

export function updateTask(id: number, fields: TaskFields, isCompleted: boolean) {
  return request<Task>(`/tasks/${id}`, {
    method: 'PUT',
    body: JSON.stringify({ ...toBody(fields), isCompleted }),
  })
}

export function deleteTask(id: number) {
  return request<void>(`/tasks/${id}`, { method: 'DELETE' })
}
