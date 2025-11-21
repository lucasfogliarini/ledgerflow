import keycloak from './keycloak'

// API Base URLs
const TRANSACTIONS_API_URL = process.env.NEXT_PUBLIC_TRANSACTIONS_API_URL || 'http://localhost:2002'
const LEDGERSUMMARIES_API_URL = process.env.NEXT_PUBLIC_LEDGERSUMMARIES_API_URL || 'http://localhost:2003'

// Interfaces
export interface Transaction {
    id: number
    type: 'Credit' | 'Debit'
    value: number
    description: string
    createdAt: string
    updatedAt?: string
}

export interface LedgerSummary {
    referenceDate: string
    balance: number
    totalCredits: number
    totalDebits: number
}

export interface CreateTransactionRequest {
    value: number
    description: string
}

export interface ApiResponse<T> {
    data?: T
    error?: string
}

// Helper function to get authenticated headers
async function getAuthHeaders(): Promise<HeadersInit> {
    if (!keycloak?.token) {
        throw new Error('Usuário não autenticado')
    }

    return {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${keycloak.token}`
    }
}

// Transaction API functions
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

// LedgerSummary API functions
export async function getLedgerSummaries(
    referenceDate: string
): Promise<ApiResponse<LedgerSummary[]>> {
    try {
        const headers = await getAuthHeaders();
        const url = `${LEDGERSUMMARIES_API_URL}/ledger_summaries?referenceDate=${referenceDate}`;

        const response = await fetch(url, {
            method: 'GET',
            headers
        })

        if (!response.ok) {
            const errorText = await response.text()
            return { error: errorText || `Erro ao buscar sumários: ${response.statusText}` }
        }

        const responseData = await response.json()
        // Extract ledgerSummaries array from the response object
        const data = responseData.ledgerSummaries || []
        return { data }
    } catch (error) {
        return { error: error instanceof Error ? error.message : 'Erro ao buscar sumários' }
    }
}

export async function consolidateLedger(
    referenceDate: string
): Promise<ApiResponse<LedgerSummary>> {
    try {
        const headers = await getAuthHeaders()
        const response = await fetch(`${LEDGERSUMMARIES_API_URL}/ledger_summaries/consolidate`, {
            method: 'POST',
            headers,
            body: JSON.stringify({ referenceDate })
        })

        if (!response.ok) {
            const errorText = await response.text()
            return { error: errorText || `Erro ao consolidar ledger: ${response.statusText}` }
        }

        const data = await response.json()
        return { data }
    } catch (error) {
        return { error: error instanceof Error ? error.message : 'Erro ao consolidar ledger' }
    }
}

// Utility function to format currency
export function formatCurrency(value: number): string {
    return new Intl.NumberFormat('pt-BR', {
        style: 'currency',
        currency: 'BRL'
    }).format(value)
}

// Utility function to format date
export function formatDate(dateString: string): string {
    const date = new Date(dateString)
    return new Intl.DateTimeFormat('pt-BR', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    }).format(date)
}
