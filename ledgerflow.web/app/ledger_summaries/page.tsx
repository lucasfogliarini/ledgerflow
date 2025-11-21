'use client'

import { useKeycloak } from '../../components/KeycloakProvider'
import LedgerSummaryView from './ledger-summary'
import { useRouter } from 'next/navigation'
import { useEffect } from 'react'

export default function SummariesPage() {
    const { authenticated, loading } = useKeycloak()
    const router = useRouter()

    useEffect(() => {
        if (!loading && !authenticated) {
            router.push('/')
        }
    }, [loading, authenticated, router])

    if (loading || !authenticated) {
        return <div>Carregando...</div>
    }

    return (
        <div className="summaries-view">
            <LedgerSummaryView />
        </div>
    )
}
