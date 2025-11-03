# LedgerFlow

LedgerFlow é uma solução desenvolvida em .NET 8 para controle e consolidação de lançamentos financeiros diários (débitos e créditos). O projeto foi criado com foco em escalabilidade, resiliência e boas práticas de arquitetura de software — aplicando princípios de DDD, separação de contextos e testes automatizados.

A arquitetura contempla dois principais serviços:

* **Serviço de Lançamentos (Transactions API):** responsável pelos débitos e créditos do fluxo de caixa.
* **Serviço de Consolidação (LedgerSummaries API):** gera o saldo consolidado diário(ou do momento) com base nas transações registradas.

---

## Resumo
- [🧩 Setup](#setup)
- [⚙️ Funcionalidades](#funcionalidades)
- [🧪 Testes](#testes)
- [🧱 Design da solução](#design-da-solução)

## 🧩 Setup

Abaixo estão as etapas para configurar o ambiente local.

### 1. Subir a infraestrutura com Docker Compose

Certifique-se de ter **Docker** e **Docker Compose** instalados.

No diretório raiz do projeto, execute:

```bash
docker-compose up -d
```

Esse comando inicializa todos os containers necessários (banco de dados SQL Server, APIs, Keycloak, etc).

### 2. Aplicar as migrações do banco de dados

Após os containers estarem rodando, aplique as migrações executando o comando a seguir no terminal:

Ainda no diretório raiz (LedgerFlow), execute:
```
 dotnet tool install --global dotnet-ef
 dotnet ef database update --startup-project LedgerFlow.Transactions.WebApi/LedgerFlow.Transactions.WebApi.csproj
```

Isso criará o schema e as tabelas necessárias no banco de dados configurado via `appsettings.Development.json`.

### 3. Importar realm e clients do Keycloak

O sistema utiliza o **Keycloak** como provedor de identidade.

1. Acesse a interface administrativa do Keycloak [Master Admin Console](http://localhost:2000/admin).
2. Vá até **Manage realms → Create Realm → Browse Resource file**.
3. Faça upload do arquivo `ledgerflow-realm-export.json` fornecido com o projeto.
4. Entre no [Ledger Admin Console](http://localhost:2000/admin/ledgerflow/console) com usuário usuário **admin** e senha **admin** para testar ou customizar algo a mais.
5. Entre em [clients](http://localhost:2000/admin/ledgerflow/console/#/ledgerflow/clients) e confirme a criação do client público (legderflow).

---

## ⚙️ Funcionalidades

```gherkin
  Cenário: Criar uma transação de crédito com valores válidos
    Dado que o usuário informa um valor maior que zero
    Quando o sistema cria uma transação de crédito
    Então a transação deve ser registrada com sucesso
    E o tipo deve ser "Credit"
    E a data de criação deve ser registrada automaticamente
```

```gherkin
 Cenário: Criar uma transação de débito com valores válidos
    Dado que o usuário informa um valor maior que zero
    Quando o sistema cria uma transação de débito
    Então a transação deve ser registrada com sucesso
    E o tipo deve ser "Debit"
    E a data de criação deve ser registrada automaticamente
```

```gherkin
 Cenário: Consolidação e lançamentos (saldo, créditos e débitos)
    Dado que existe uma lista de transações válidas (créditos e débitos)
    Quando o usuário solicitar a consolidação de lançamentos
    Então o sistema deve calcular o total de créditos, débitos e saldo.
```

```gherkin
Cenário: Obter relatórios consolidados de uma data específica
    Dado que o usuário informa uma data de referência válida
    E existam relatórios consolidados cadastrados para essa data
    Quando o sistema processa a requisição de consulta
    Então o sistema retornar a lista de relatórios com seus respectivos saldos, totais de créditos e débitos e data e hora de referência
```

---

## 🧪 Testes

### Testes unitários

Os testes unitários cobrem a lógica de domínio e regras de negócio.

Para executá-los:

```bash
dotnet test LedgerFlow.Tests.Unit
```

Os resultados detalham quais casos de uso e entidades foram validados.

### Testes de performance

Para medir o desempenho das APIs (principalmente em cenários de alta carga no consolidado), utilize o script configurado em `k6.js` na raiz do projeto.:

```bash
cd LedgerFlow
k6 run k6.js
```

Os resultados indicam latência média, throughput e taxa de erros — essenciais para avaliar se o sistema se mantém dentro do limite de 5% de perda de requisições.

---

## 🧱 Design da solução

A solução foi desenhada seguindo princípios de **Domain-Driven Design (DDD)** e **Clean Architecture**, com clara separação entre camadas:

* **LedgerFlow** — contém entidades, agregados, eventos de domínio e regras de negócio.
* **LedgerFlow.Infrastructure** — abstrações de persistência, mapeamentos e contexto EF Core.
* **LedgerFlow.Application** — implementa os casos de uso da aplicação, comandos, consultas e orquestração das regras de negócio.
* **LedgerFlow.Transactions.WebApi** — expõe os endpoints responsáveis pelo registro e consulta de transações (créditos e débitos).
* **LedgerFlow.LedgerSummaries.WebApi** — expõe os endpoints responsáveis pela consolidação e consulta dos saldos diários.


A arquitetura também contempla:

* **Observabilidade:** instrumentação com OpenTelemetry.
* **Segurança:** autenticação via Keycloak (OpenID Connect).

---
