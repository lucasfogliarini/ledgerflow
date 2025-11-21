'use client'

import { useKeycloak } from '../components/KeycloakProvider'
import { useRouter } from 'next/navigation'
import { useEffect } from 'react'

export default function Home() {
  const { authenticated, loading, login } = useKeycloak()
  const router = useRouter()

  useEffect(() => {
    if (!loading && authenticated) {
      router.push('/transactions')
    }
  }, [loading, authenticated, router])

  if (loading) {
    return (
      <div className="card">
        <h1>LedgerFlow</h1>
        <p>Carregando autenticação...</p>
      </div>
    )
  }

  if (authenticated) {
    return null;
  }

  return (
    <div className="card">
      <h2>Bem-vindo ao LedgerFlow</h2>
      <p>Sistema de gerenciamento de transações financeiras</p>
      <button onClick={login} className="login-button">
        🔑 Login com Keycloak
      </button>
    </div>
  )
}