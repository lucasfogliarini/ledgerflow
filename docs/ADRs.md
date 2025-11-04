# Decisões Arquiteturais – LedgerFlow

Este documento registra as principais decisões arquiteturais tomadas durante o design e implementação do **LedgerFlow**, destacando o racional, os benefícios esperados e os trade-offs considerados.

---

## 1. Padrão Arquitetural: Microsserviços Independentes
**Decisão:** Adotar dois serviços independentes – `Transactions API` e `LedgerSummaries API`.

**Motivação:**  
A separação dos contextos permite maior isolamento de responsabilidades e garante resiliência. Assim, o serviço de lançamentos permanece disponível mesmo que o de consolidação esteja fora do ar.

**Trade-offs:**  
Aumenta a complexidade operacional e de integração, exigindo observabilidade, rastreamento distribuído e monitoramento entre serviços.

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
**Decisão:** Estruturar o domínio com **Aggregates**, **Value Objects**, **Entities** e **Domain Events**.

**Motivação:**  
Organizar a lógica de negócio de forma expressiva e sustentável, facilitando evolução do código e entendimento do domínio financeiro.

**Trade-offs:**  
Requer um investimento inicial maior em modelagem e alinhamento com especialistas de domínio — mitigado por sessões de **EventStorming** para descoberta colaborativa dos eventos e comandos do sistema.

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

## 9. Estratégia de Testes
**Decisão:** Adotar uma pirâmide de testes composta por:
- Testes unitários (domínio);
- Testes de integração (entre APIs e banco);
- Testes funcionais (Postman);
- Testes de carga (k6).

**Motivação:**  
Assegurar qualidade, confiabilidade e performance, com verificação contínua em diferentes níveis da aplicação.

---

## 10. Escalabilidade e Resiliência
**Decisão:** Planejar suporte a até **50 requisições/segundo** no serviço de consolidação, com **tolerância de 5% de perda**.

**Motivação:**  
Atender ao requisito não funcional do desafio, assegurando comportamento estável sob carga.

**Evolução futura:**  
Implementar cache de resultados, filas assíncronas e estratégias de retry para maximizar throughput e disponibilidade.

---

📚 **Resumo:**  
O **LedgerFlow** foi concebido para ser modular, escalável e resiliente. As decisões priorizam clareza, segurança e capacidade de evolução — com espaço aberto para incrementos em observabilidade, mensageria e processamento assíncrono conforme o sistema amadurece.
