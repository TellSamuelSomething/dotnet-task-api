import { useEffect, useState } from 'react'
import {
  PRIORITY_LABELS,
  createTask,
  deleteTask,
  listTasks,
  toFields,
  updateTask,
  type Page,
  type Task,
  type TaskFields,
} from './api'
import TaskForm from './TaskForm'

function today() {
  const d = new Date()
  const pad = (n: number) => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`
}

export default function TasksPage() {
  const [page, setPage] = useState(1)
  const [data, setData] = useState<Page<Task> | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [editingId, setEditingId] = useState<number | null>(null)
  // Bumped after every change so the list is fetched again.
  const [version, setVersion] = useState(0)

  useEffect(() => {
    let ignore = false
    listTasks(page)
      .then((result) => {
        if (ignore) return
        setData(result)
        setError(null)
      })
      .catch((err: unknown) => {
        if (!ignore) setError(err instanceof Error ? err.message : 'Could not load tasks.')
      })
    return () => {
      ignore = true
    }
  }, [page, version])

  const reload = () => setVersion((v) => v + 1)

  async function handleCreate(fields: TaskFields) {
    await createTask(fields)
    // New tasks can land on any page, the first page is where people look for them.
    setPage(1)
    reload()
  }

  async function handleUpdate(task: Task, fields: TaskFields, isCompleted = task.isCompleted) {
    await updateTask(task.id, fields, isCompleted)
    setEditingId(null)
    reload()
  }

  async function handleToggle(task: Task) {
    try {
      await handleUpdate(task, toFields(task), !task.isCompleted)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Could not update the task.')
    }
  }

  async function handleDelete(task: Task) {
    if (!window.confirm(`Delete "${task.title}"?`)) return
    try {
      await deleteTask(task.id)
      // Deleting the last item of a page would otherwise leave an empty page behind.
      if (data && data.items.length === 1 && page > 1) setPage(page - 1)
      reload()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Could not delete the task.')
    }
  }

  const todayStr = today()

  return (
    <div className="tasks">
      <section className="card">
        <h2>New task</h2>
        <TaskForm submitLabel="Add task" onSubmit={handleCreate} />
      </section>

      <section>
        <h2>Your tasks{data ? ` (${data.totalCount})` : ''}</h2>

        {error && <p className="error" role="alert">{error}</p>}
        {!data && !error && <p className="muted">Loading…</p>}
        {data && data.items.length === 0 && (
          <p className="muted">No tasks yet. Add your first one above.</p>
        )}

        <ul className="task-list">
          {data?.items.map((task) => {
            const due = task.dueDate?.slice(0, 10)
            const overdue = !task.isCompleted && due !== undefined && due < todayStr

            return (
              <li key={task.id} className={`card task ${task.isCompleted ? 'done' : ''}`}>
                {editingId === task.id ? (
                  <TaskForm
                    initial={task}
                    submitLabel="Save"
                    onSubmit={(fields) => handleUpdate(task, fields)}
                    onCancel={() => setEditingId(null)}
                  />
                ) : (
                  <>
                    <input
                      type="checkbox"
                      checked={task.isCompleted}
                      onChange={() => handleToggle(task)}
                      aria-label={`Mark "${task.title}" as ${task.isCompleted ? 'not done' : 'done'}`}
                    />
                    <div className="task-body">
                      <p className="task-title">{task.title}</p>
                      {task.description && <p className="muted">{task.description}</p>}
                      <p className="meta">
                        <span className={`badge priority-${task.priority}`}>
                          {PRIORITY_LABELS[task.priority]}
                        </span>
                        {due && (
                          <span className={overdue ? 'overdue' : undefined}>
                            {overdue ? 'Overdue, was due ' : 'Due '}
                            {due}
                          </span>
                        )}
                      </p>
                    </div>
                    <div className="task-actions">
                      <button type="button" className="btn btn-ghost" onClick={() => setEditingId(task.id)}>
                        Edit
                      </button>
                      <button type="button" className="btn btn-danger" onClick={() => handleDelete(task)}>
                        Delete
                      </button>
                    </div>
                  </>
                )}
              </li>
            )
          })}
        </ul>

        {data && data.totalPages > 1 && (
          <nav className="pager" aria-label="Pagination">
            <button
              type="button"
              className="btn btn-ghost"
              disabled={page <= 1}
              onClick={() => setPage(page - 1)}
            >
              Previous
            </button>
            <span className="muted">
              Page {data.page} of {data.totalPages}
            </span>
            <button
              type="button"
              className="btn btn-ghost"
              disabled={page >= data.totalPages}
              onClick={() => setPage(page + 1)}
            >
              Next
            </button>
          </nav>
        )}
      </section>
    </div>
  )
}
