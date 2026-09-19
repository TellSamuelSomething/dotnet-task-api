import { useEffect, useState } from 'react'
import { hasSession, logout, setSessionEndedHandler } from './api'
import AuthForm from './AuthForm'
import TasksPage from './TasksPage'

export default function App() {
  const [signedIn, setSignedIn] = useState(hasSession)

  useEffect(() => {
    setSessionEndedHandler(() => setSignedIn(false))
  }, [])

  async function handleSignOut() {
    await logout()
    setSignedIn(false)
  }

  return (
    <div className="app">
      <header className="app-header">
        <h1>Task Manager</h1>
        {signedIn && (
          <button type="button" className="btn btn-ghost" onClick={handleSignOut}>
            Sign out
          </button>
        )}
      </header>
      <main>
        {signedIn ? <TasksPage /> : <AuthForm onSignedIn={() => setSignedIn(true)} />}
      </main>
    </div>
  )
}
