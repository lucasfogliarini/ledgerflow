import type { Metadata } from 'next'
import './globals.css'
import './page.css'
import { KeycloakProvider } from '../components/KeycloakProvider'
import Header from '../components/Header'

export const metadata: Metadata = {
  title: 'LedgerFlow Web',
  description: 'LedgerFlow Web Application',
}

export default function RootLayout({
  children,
}: {
  children: React.ReactNode
}) {
  return (
    <html lang="en">
      <body>
        <KeycloakProvider>
          <main>
            <Header />
            {children}
          </main>
        </KeycloakProvider>
      </body>
    </html>
  )
}