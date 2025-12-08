import keycloak from './keycloak'

const TRANSACTIONS_API_URL = process.env.TRANSACTIONS_API_URL || 'http://localhost:3000'

export interface Transaction {
    id: number
    type: 'Credit' | 'Debit'
    value: number
    description: string
    createdAt: string
    updatedAt?: string
}

export interface CreateTransactionRequest {
    value: number
    description: string
}

export interface ApiResponse<T> {
    data?: T
    error?: string
}

async function getAuthHeaders(): Promise<HeadersInit> {
    if (!keycloak?.token) {
        throw new Error('Usuário não autenticado')
    }

    return {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${keycloak.token}`
    }
}

export async function createCredit(request: CreateTransactionRequest): Promise<ApiResponse<Transaction>> {
    try {
        const headers = await getAuthHeaders()
        const response = await fetch(`${TRANSACTIONS_API_URL}/transactions/credit`, {
            method: 'POST',
            headers,
            body: JSON.stringify(request)
        })

        if (!response.ok) {
            const errorText = await response.text()
            return { error: errorText || `Erro ao criar crédito: ${response.statusText}` }
        }

        const data = await response.json()
        return { data }
    } catch (error) {
        return { error: error instanceof Error ? error.message : 'Erro ao criar crédito' }
    }
}

export async function createDebit(request: CreateTransactionRequest): Promise<ApiResponse<Transaction>> {
    try {
        const headers = await getAuthHeaders()
        const response = await fetch(`${TRANSACTIONS_API_URL}/transactions/debit`, {
            method: 'POST',
            headers,
            body: JSON.stringify(request)
        })

        if (!response.ok) {
            const errorText = await response.text()
            return { error: errorText || `Erro ao criar débito: ${response.statusText}` }
        }

        const data = await response.json()
        return { data }
    } catch (error) {
        return { error: error instanceof Error ? error.message : 'Erro ao criar débito' }
    }
}
