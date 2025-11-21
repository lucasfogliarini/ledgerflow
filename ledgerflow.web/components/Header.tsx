'use client'

import { useKeycloak } from './KeycloakProvider'
import Link from 'next/link'
import { usePathname } from 'next/navigation'

export default function Header() {
    const { authenticated, logout, getUserInfo } = useKeycloak()
    const pathname = usePathname()
    const userInfo = getUserInfo()

    return (
        <>
            <div className="header">
                <h1>💰 LedgerFlow</h1>
                {authenticated && userInfo && (
                    <div className="user-badge">
                        <span className="user-name">{userInfo.preferred_username}</span>
                        <button onClick={logout} className="logout-button">
                            🚪 Sair
                        </button>
                    </div>
                )}
            </div>

            {authenticated && (
                <div className="tabs">
                    <Link
                        href="/transactions"
                        className={`tab ${pathname === '/transactions' ? 'active' : ''}`}
                    >
                        💳 Transações
                    </Link>
                    <Link
                        href="/ledger_summaries"
                        className={`tab ${pathname === '/ledger_summaries' ? 'active' : ''}`}
                    >
                        📊 Saldo consolidado
                    </Link>
                </div>
            )}
        </>
    )
}
