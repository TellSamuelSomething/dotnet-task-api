# Task Manager frontend

React 19, TypeScript and Vite client for the task API in the parent folder.

It covers registration, sign in and task create, edit, complete and delete, with pagination. Access tokens refresh automatically and the session is kept in `localStorage` until you sign out.

## Run

Start the API first (see the [main README](../README.md)), then:

```bash
npm install
npm run dev
```

Open `http://localhost:5173`. Vite proxies `/api` to `http://localhost:5120`, so no CORS setup is needed in development.

## Scripts

| Command | Description |
|---------|-------------|
| `npm run dev` | Start the dev server |
| `npm run build` | Type-check and build for production |
| `npm run lint` | Lint with Oxlint |
| `npm run preview` | Preview the production build |
