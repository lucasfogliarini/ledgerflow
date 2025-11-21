'use client'

import { useState } from 'react'
import { createCredit, createDebit, CreateTransactionRequest } from '../../lib/transactionsApi'
import './transactions.css'

export type TransactionType = 'credit' | 'debit'

interface TransactionFormProps {
    onSuccess?: () => void
}

export default function TransactionForm({ onSuccess }: TransactionFormProps) {
    const [type, setType] = useState<TransactionType>('credit')
    const [value, setValue] = useState('500.00')
    const [description, setDescription] = useState('Pagamento de conta de luz')
    const [loading, setLoading] = useState(false)
    const [error, setError] = useState<string | null>(null)
    const [success, setSuccess] = useState<string | null>(null)

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault()
        setError(null)
        setSuccess(null)

        // Validação
        const numericValue = parseFloat(value)
        if (isNaN(numericValue) || numericValue <= 0) {
            setError('O valor deve ser um número positivo maior que zero')
            return
        }

        if (!description.trim()) {
            setError('A descrição é obrigatória')
            return
        }

        const request: CreateTransactionRequest = {
            value: numericValue,
            description: description.trim()
        }

        setLoading(true)

        try {
            const result = type === 'credit'
                ? await createCredit(request)
                : await createDebit(request)

            if (result.error) {
                setError(result.error)
            } else {
                setSuccess(`${type === 'credit' ? 'Crédito' : 'Débito'} criado com sucesso!`)
                setValue('')
                setDescription('')
                if (onSuccess) {
                    onSuccess()
                }
            }
        } catch (err) {
            setError('Erro ao processar transação')
        } finally {
            setLoading(false)
        }
    }

    return (
        <div className="transaction-form-container">
            <h2>💰 Nova Transação</h2>

            <form onSubmit={handleSubmit} className="transaction-form">
                <div className="form-group type-selector">
                    <label>Tipo de Transação:</label>
                    <div className="type-buttons">
                        <button
                            type="button"
                            className={`type-button ${type === 'credit' ? 'active credit' : ''}`}
                            onClick={() => setType('credit')}
                        >
                            ➕ Crédito
                        </button>
                        <button
                            type="button"
                            className={`type-button ${type === 'debit' ? 'active debit' : ''}`}
                            onClick={() => setType('debit')}
                        >
                            ➖ Débito
                        </button>
                    </div>
                </div>

                <div className="form-group">
                    <label htmlFor="value">Valor (R$):</label>
                    <input
                        id="value"
                        type="number"
                        step="1.00"
                        min="1.00"
                        value={value}
                        onChange={(e) => setValue(e.target.value)}
                        placeholder="500.00"
                        required
                        disabled={loading}
                    />
                </div>

                <div className="form-group">
                    <label htmlFor="description">Descrição:</label>
                    <input
                        id="description"
                        type="text"
                        value={description}
                        onChange={(e) => setDescription(e.target.value)}
                        placeholder="Descreva a transação..."
                        required
                        disabled={loading}
                        maxLength={200}
                    />
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

                <button
                    type="submit"
                    className={`submit-button ${type}`}
                    disabled={loading}
                >
                    {loading ? '⏳ Processando...' : `Criar ${type === 'credit' ? 'Crédito' : 'Débito'}`}
                </button>
            </form>
        </div>
    )
}
