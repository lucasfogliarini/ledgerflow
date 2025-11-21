'use client'

import { useKeycloak } from '../../components/KeycloakProvider'
import TransactionForm from './transaction-form'
import { useRouter } from 'next/navigation'
import { useEffect } from 'react'

export default function TransactionsPage() {
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
        <div className="transactions-view">
            <TransactionForm />
        </div>
    )
}
