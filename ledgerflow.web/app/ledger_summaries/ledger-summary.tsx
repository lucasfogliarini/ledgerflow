'use client'

import { useState, useEffect } from 'react'
import { LedgerSummary, getLedgerSummaries, consolidateLedger, formatCurrency, formatDate } from '../../lib/ledgerSummariesApi'
import '../transactions/transactions.css'

export default function LedgerSummaryView() {
    const [summaries, setSummaries] = useState<LedgerSummary[]>([])
    const [loading, setLoading] = useState(true)
    const [error, setError] = useState<string | null>(null)
    const [consolidating, setConsolidating] = useState(false)
    const [success, setSuccess] = useState<string | null>(null)

    useEffect(() => {
        loadSummaries()
    }, [])

    const loadSummaries = async () => {
        setLoading(true)
        setError(null)

        try {
            const today = new Date().toISOString().split('T')[0]
            const result = await getLedgerSummaries(today);
            if (result.error) {
                setError(result.error)
            } else if (result.data) {
                const sortedData = result.data.sort((a, b) =>
                    new Date(b.referenceDate).getTime() - new Date(a.referenceDate).getTime()
                );
                setSummaries(sortedData.slice(0, 1))
            }
        } catch (err) {
            setError('Erro ao carregar saldo consolidado')
        } finally {
            setLoading(false)
        }
    }

    const handleConsolidate = async () => {
        setConsolidating(true)
        setError(null)
        setSuccess(null)

        try {
            const now = new Date().toISOString();
            const result = await consolidateLedger(now)

            if (result.error) {
                setError(result.error)
            } else {
                setSuccess('Saldo consolidado com sucesso!')
                await loadSummaries()
            }
        } catch (err) {
            setError('Erro ao consolidar o saldo')
        } finally {
            setConsolidating(false)
        }
    }

    if (loading) {
        return (
            <div className="ledger-summary-container">
                <h2>📊 Saldo consolidado</h2>
                <div className="loading">Carregando saldo consolidado...</div>
            </div>
        )
    }

    return (
        <div className="ledger-summary-container">
            <div className="header-with-action">
                <h2>📊 Saldo consolidado</h2>
                <button
                    onClick={handleConsolidate}
                    disabled={consolidating}
                    className="consolidate-button"
                >
                    {consolidating ? '⏳ Consolidando...' : '🔄 Consolidar Saldo'}
                </button>
            </div>

            {error && (
                <div className="message error">
                    ⚠️ {error}
                </div>
            )}

            {success && (
                <div className="message success">
                    ✅ {success}
                </div>
            )}

            {summaries.length === 0 ? (
                <div className="empty-state">
                    <p>Nenhum saldo consolidado disponível ainda.</p>
                    <p className="hint">Clique em &quot;Consolidar Saldo&quot; para gerar o primeiro saldo consolidado.</p>
                </div>
            ) : (
                <div className="summary-grid">
                    {summaries.map((summary, index) => (
                        <div key={index} className="summary-card">
                            <div className="summary-header">
                                <h3>📅 {formatDate(summary.referenceDate)}</h3>
                            </div>

                            <div className="summary-stats">
                                <div className="stat-item balance">
                                    <span className="stat-label">Saldo</span>
                                    <span className={`stat-value ${summary.balance >= 0 ? 'positive' : 'negative'}`}>
                                        {formatCurrency(summary.balance)}
                                    </span>
                                </div>

                                <div className="stat-item credit">
                                    <span className="stat-label">Total de Créditos</span>
                                    <span className="stat-value">
                                        {formatCurrency(summary.totalCredits)}
                                    </span>
                                </div>

                                <div className="stat-item debit">
                                    <span className="stat-label">Total de Débitos</span>
                                    <span className="stat-value">
                                        {formatCurrency(summary.totalDebits)}
                                    </span>
                                </div>
                            </div>
                        </div>
                    ))}
                </div>
            )}

            <button
                onClick={loadSummaries}
                className="refresh-button"
                disabled={loading}
            >
                🔄 Atualizar
            </button>
        </div>
    )
}
