# Decisões Arquiteturais – LedgerFlow

Este documento registra as principais decisões arquiteturais tomadas durante o design e implementação do **LedgerFlow**, destacando o racional, os benefícios esperados e os trade-offs considerados.

---

## 1. Monolito Modular com múltiplos artefatos de implantação

**Decisão:**
Adotar um monolito modular com dois artefatos de implantação independentes — LedgerFlow.Transactions.WebApi e LedgerFlow.LedgerSummaries.WebApi — compartilhando a mesma base de código de aplicação e infraestrutura.

**Motivação:**  
A estrutura modular possibilita separar claramente os contextos de domínio (Transactions e LedgerSummaries), mantendo a coesão e a integridade transacional de um monolito.
A existência de dois artefatos de implantação distintos permite independência operacional — cada API pode ser versionada, escalada e publicada separadamente — sem romper a unidade lógica da aplicação.
Essa abordagem é especialmente útil para times que desejam preparar o sistema para uma futura transição gradual para microsserviços, preservando a simplicidade do desenvolvimento monolítico no estágio inicial.

Referência:  
A abordagem de monolito modular é amplamente discutida como meio-termo entre monolito tradicional e microsserviços.
Por exemplo, [Blank et al. (202x) argumentam](https://binaryigor.com/modular-monolith-and-microservices-modularity-is-what-truly-matters.html) que, embora a implantação seja comumente um único artefato, é viável construir múltiplas unidades de implantação mesmo dentro do monolito modular — “with one, two or three units of deployment”. 

**Trade-offs:**  
A manutenção de múltiplos pontos de deploy dentro de um mesmo monolito exige atenção redobrada à gestão de dependências internas e à compatibilidade entre módulos.
Embora compartilhem o mesmo domínio e infraestrutura, é necessário garantir isolamento lógico entre camadas e observabilidade consistente para monitorar o comportamento dos módulos de forma independente.
A modularização adiciona uma camada de complexidade, mas oferece benefícios claros em termos de evolução arquitetural e escalabilidade progressiva.

---

## 2. Comunicação entre Serviços: HTTP REST
**Decisão:** Utilizar comunicação síncrona via **HTTP REST**, com contratos bem definidos.

**Motivação:**  
Simplicidade e compatibilidade com ferramentas de mercado, facilitando testes e integração com o front-end.  
O design atual prioriza legibilidade sobre latência, considerando que a consolidação não é uma operação crítica em tempo real.

**Evolução futura:**  
Poderá evoluir para uma abordagem assíncrona com mensageria (ex: Kafka ou RabbitMQ), garantindo desacoplamento e processamento eventual de eventos.

---

## 3. Padrão de Domínio: DDD (Domain-Driven Design)
**Decisão:**  
Estruturar o domínio com **Aggregates**, **Value Objects**, **Entities** e **Domain Events**.

**Motivação:**  
Essa abordagem organiza a lógica de negócio de maneira expressiva, consistente e centrada no domínio, tornando o código mais próximo da linguagem utilizada pelos especialistas financeiros e facilitando sua manutenção e evolução ao longo do tempo.

O design do LedgerFlow já está preparado para emissão e manipulação de eventos de domínio, ainda que nenhum caso de uso concreto tenha sido implementado até o momento. Essa adaptação permite evoluir facilmente para um modelo mais reativo e orientado a eventos, promovendo integração entre partes da aplicação sem acoplamento direto.

---

## 4. Autenticação e Autorização: Keycloak (OAuth2 + JWT)
**Decisão:** Utilizar o **Keycloak** como Identity Provider, implementando o fluxo **Authorization Code** com emissão de **JWTs**.

**Motivação:**  
Centralizar a autenticação, facilitar o controle de usuários e papéis, e garantir segurança via padrões amplamente adotados.

**Trade-offs:**  
Adiciona complexidade operacional (configuração e manutenção do realm), mas fornece um ganho substancial em segurança e interoperabilidade.

---

## 5. Persistência: SQL Server
**Decisão:** Utilizar o **SQL Server** como banco relacional padrão para ambos os serviços.

**Motivação:**  
Oferece consistência transacional, suporte nativo ao EF Core e boa compatibilidade com ambientes corporativos.

**Evolução futura:**  
Poderá ser substituído ou complementado por soluções orientadas a eventos ou bancos NoSQL para cenários de alto volume de leitura.

---

## 6. Front-end: Next.js (LedgerFlow Web)
**Decisão:** Implementar uma interface moderna em **Next.js**, consumindo as APIs autenticadas.

**Motivação:**  
Proporcionar uma experiência fluida e responsiva, com SSR (Server-Side Rendering) e integração nativa com APIs REST e OAuth2.

**Trade-offs:**  
Requer configuração cuidadosa de variáveis de ambiente e integração com o Keycloak, mas oferece excelente performance e experiência de uso.

---

## 7. Observabilidade: OpenTelemetry
**Decisão:** Adotar **OpenTelemetry** para coleta de métricas, logs estruturados e traces distribuídos.

**Motivação:**  
A observabilidade é essencial para monitorar a saúde e o desempenho dos serviços, além de fornecer rastreabilidade entre APIs e suporte à análise de falhas.
O OpenTelemetry foi escolhido por ser um padrão aberto e extensível, compatível com a maioria das plataformas de observabilidade do mercado.
Essa característica garante flexibilidade para integração tanto com soluções de baixo custo (como Prometheus, Zipkin ou Grafana Tempo), quanto com plataformas corporativas e completas, como Dynatrace, Datadog ou New Relic.

**Evolução futura:**  
Integração com **Dynatrace** para visualização de métricas operacionais e SLAs.

---

## 8. Deploy e Infraestrutura: Docker + Kubernetes
**Decisão:** Containerizar os serviços e orquestrar via **Kubernetes (K8s)**.

**Motivação:**  
Garantir portabilidade, escalabilidade e isolamento entre componentes, permitindo deploy consistente em qualquer ambiente.

**Trade-offs:**  
Demanda infraestrutura e conhecimento operacional de K8s, mas viabiliza escalabilidade horizontal e alta disponibilidade.

---

## 9. Caching: Redis para endpoints de alta demanda

**Decisão:** Implementar cache distribuído com Microsoft.Extensions.Caching.StackExchangeRedis.

**Motivação:**
Determinados endpoints do sistema, especialmente aqueles expostos a um alto volume de requisições (como consultas de consolidação), demandavam uma estratégia de cache eficiente para reduzir a carga no banco de dados e melhorar o tempo de resposta.

Embora o Output Caching Middleware nativo do ASP.NET Core ofereça uma solução simples e performática, ele não suporta requisições que incluam o cabeçalho Authorization, o que inviabilizou seu uso em endpoints protegidos por autenticação JWT.

Por isso, optou-se por uma implementação direta de cache distribuído utilizando o Redis por meio do pacote Microsoft.Extensions.Caching.StackExchangeRedis, que permite controle programático sobre o ciclo de vida dos dados armazenados e compatibilidade com APIs autenticadas.

**Trade-offs:**
A abordagem manual de caching aumenta a complexidade do código, exigindo políticas explícitas de invalidação e definição de chaves únicas por usuário ou contexto. Em contrapartida, garante maior flexibilidade e compatibilidade com cenários de autenticação, mantendo a escalabilidade e a performance desejadas.

**Evolução futura:**
Poderá evoluir para uma estratégia híbrida, combinando Output Cache para endpoints públicos e Redis distribuído para endpoints autenticados, ou integrar mecanismos reativos de invalidação baseados em eventos de domínio.

---

## 10. Estratégia de Testes
**Decisão:** Adotar uma pirâmide de testes composta por:
- Testes unitários (domínio);
- Testes de integração (entre APIs e banco);
- Testes funcionais (Postman);
- Testes de carga (k6).

**Motivação:**  
Assegurar qualidade, confiabilidade e performance, com verificação contínua em diferentes níveis da aplicação.

---

📚 **Resumo:**  
O **LedgerFlow** foi concebido para ser modular, escalável e resiliente. As decisões priorizam clareza, segurança e capacidade de evolução — com espaço aberto para incrementos em observabilidade, mensageria e processamento assíncrono conforme o sistema amadurece.
