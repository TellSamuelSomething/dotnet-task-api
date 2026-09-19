import { useState, type SubmitEvent } from 'react'
import { login, register } from './api'

interface Props {
  onSignedIn: () => void
}

export default function AuthForm({ onSignedIn }: Props) {
  const [mode, setMode] = useState<'login' | 'register'>('login')
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [busy, setBusy] = useState(false)

  const isLogin = mode === 'login'

  async function handleSubmit(e: SubmitEvent<HTMLFormElement>) {
    e.preventDefault()
    setBusy(true)
    setError(null)
    try {
      await (isLogin ? login : register)(username, password)
      onSignedIn()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Something went wrong.')
    } finally {
      setBusy(false)
    }
  }

  function switchMode() {
    setMode(isLogin ? 'register' : 'login')
    setError(null)
  }

  return (
    <form className="card auth-card" onSubmit={handleSubmit}>
      <h2>{isLogin ? 'Sign in' : 'Create account'}</h2>

      <label>
        Username
        <input
          value={username}
          onChange={(e) => setUsername(e.target.value)}
          autoComplete="username"
          minLength={isLogin ? undefined : 3}
          maxLength={50}
          required
          autoFocus
        />
      </label>

      <label>
        Password
        <input
          type="password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          autoComplete={isLogin ? 'current-password' : 'new-password'}
          minLength={isLogin ? undefined : 6}
          required
        />
        {!isLogin && <span className="hint">At least 6 characters.</span>}
      </label>

      {error && <p className="error" role="alert">{error}</p>}

      <button type="submit" className="btn btn-primary" disabled={busy}>
        {busy ? 'Please wait…' : isLogin ? 'Sign in' : 'Create account'}
      </button>

      <button type="button" className="btn btn-link" onClick={switchMode}>
        {isLogin ? 'No account? Create one' : 'Already have an account? Sign in'}
      </button>
    </form>
  )
}
