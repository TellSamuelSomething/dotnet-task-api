import { useState, type SubmitEvent } from 'react'
import { PRIORITY_LABELS, toFields, type Priority, type Task, type TaskFields } from './api'

interface Props {
  initial?: Task
  submitLabel: string
  onSubmit: (fields: TaskFields) => Promise<void>
  onCancel?: () => void
}

export default function TaskForm({ initial, submitLabel, onSubmit, onCancel }: Props) {
  const [fields, setFields] = useState(() => toFields(initial))
  const [error, setError] = useState<string | null>(null)
  const [busy, setBusy] = useState(false)

  function set<K extends keyof TaskFields>(key: K, value: TaskFields[K]) {
    setFields((prev) => ({ ...prev, [key]: value }))
  }

  async function handleSubmit(e: SubmitEvent<HTMLFormElement>) {
    e.preventDefault()
    setBusy(true)
    setError(null)
    try {
      await onSubmit(fields)
      if (!initial) setFields(toFields())
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Something went wrong.')
    } finally {
      setBusy(false)
    }
  }

  return (
    <form className="task-form" onSubmit={handleSubmit}>
      <label>
        Title
        <input
          value={fields.title}
          onChange={(e) => set('title', e.target.value)}
          maxLength={100}
          required
        />
      </label>

      <label>
        Description
        <textarea
          value={fields.description}
          onChange={(e) => set('description', e.target.value)}
          maxLength={500}
          rows={2}
        />
      </label>

      <div className="row">
        <label>
          Priority
          <select
            value={fields.priority}
            onChange={(e) => set('priority', Number(e.target.value) as Priority)}
          >
            {([0, 1, 2] as const).map((p) => (
              <option key={p} value={p}>
                {PRIORITY_LABELS[p]}
              </option>
            ))}
          </select>
        </label>

        <label>
          Due date
          <input
            type="date"
            value={fields.dueDate}
            onChange={(e) => set('dueDate', e.target.value)}
          />
        </label>
      </div>

      {error && <p className="error" role="alert">{error}</p>}

      <div className="actions">
        <button type="submit" className="btn btn-primary" disabled={busy}>
          {busy ? 'Saving…' : submitLabel}
        </button>
        {onCancel && (
          <button type="button" className="btn btn-ghost" onClick={onCancel} disabled={busy}>
            Cancel
          </button>
        )}
      </div>
    </form>
  )
}
