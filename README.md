================================================================================
RESUMO DO PROJETO: pix-integration-hub
Repositorio: https://github.com/gabrielrosas28/pix-integration-hub
Data do resumo: 2026-05-25
================================================================================

VISAO GERAL
-----------
Projeto academico/squad (Squad 8) de uma API de Integracao Bancaria focada em
PIX. Trata-se de um Hub de Integracao PIX construido em .NET (C#) seguindo os
principios de Domain-Driven Design (DDD) e Clean Architecture, com infra de
mensageria, banco relacional e mock de gateways de pagamento para testes
locais.

- Linguagem principal: C# (.NET) - 47.511 bytes
- Auxiliar: PowerShell - 7.553 bytes
- Solucao: Integracao_Bancaria_squad_8.sln
- Visibilidade: publico
- Branch default: main
- Branches existentes: main, Pedro, Pedro_Almeida
- Criado em: 2026-03-09
- Ultimo push: 2026-05-25
- Stars: 2 | Watchers: 2 | Forks: 0
- Issues abertas: 0 | PRs abertos: 0

CONTRIBUIDORES
--------------
1. PedroTorres42 (Pedro Torres)  - 13 commits  (lider de codigo)
2. PedroHAlmeida2007 (Pedro_Henrique_Almeida) - 7 commits
3. ElfeNss (Joao Gabriel) - 2 commits
   (Copilot aparece como co-author em alguns commits)

ARQUITETURA (Clean Architecture / DDD)
--------------------------------------
A solucao esta dividida em 4 projetos C#:

1) Domain/  (Camada de Dominio)
   - Aggregates/Invoice  (agregados de fatura/cobranca)
   - Entities            (entidades de negocio: Conta, ChavePix, Secret, Auditoria)
   - Events              (eventos de dominio)
   - ValueObjects        (objetos de valor)
   - Domain.csproj

2) Application/  (Camada de Aplicacao - CQRS)
   - Commands            (comandos - escrita)
   - Queries             (consultas - leitura)
   - Handlers            (manipuladores das requisicoes)
   - DTOs                (objetos de transferencia)
   - Interfaces          (contratos)
   - MockPayments        (simulacao de pagamentos)
   - Application.csproj

3) Infrastructure/  (Camada de Infraestrutura)
   - Adapters
   - Configuration
   - Data
   - Messaging/RabbitMQ  (producer de mensagens)
   - Migrations          (migrations EF Core)
   - MockPayments
   - Persistence
   - Repositories
   - Services
   - Infrastructure.csproj

4) ApiService/  (Camada de Apresentacao - ASP.NET Core Web API)
   - Controller/
       * ChavePixController.cs   (CRUD de chaves PIX)
       * ContaController.cs      (CRUD de contas)
       * InvoiceController.cs    (Cobrancas / faturas)
       * SecretController.cs     (CRUD de secrets para autenticacao bancaria)
   - Filters
   - Middleware
   - Properties
   - openapi              (especificacao OpenAPI/Swagger)
   - Program.cs
   - appsettings.json / appsettings.Development.json
   - ApiService.http      (testes HTTP)
   - ApiService.csproj

INFRAESTRUTURA LOCAL (docker-compose.yml)
-----------------------------------------
Rede: "pix-net" (bridge). Quatro servicos:

1) PostgreSQL 15 (squad8_postgres)

2) RabbitMQ 3-management
   

3) Mock Server (mockserver/mockserver:latest)

4) MockServer Seed (curlimages/curl)
   - Servico utilitario para popular o mock server com expectativas iniciais

Existe tambem uma pasta /mockserver com a configuracao do mock e uma pasta
/scripts com utilitarios em PowerShell.

FUNCIONALIDADES JA IMPLEMENTADAS
--------------------------------
- Estrutura completa de Clean Architecture/DDD criada conforme orientacao do mentor
- Modelagem inicial do banco de dados
- CRUD de Conta (operacional - implementacoes provisorias)
- CRUD de Secret + integracao Conta -> Secret via FK SecretId
- CRUD de ChavePix
- Entidade Auditoria preenchida
- Criacao de Cobranca (Invoice)
- Refatoracao da Cobranca
- Producer RabbitMQ (envio de objetos para a fila - ainda com ajustes pendentes)
- Mock Server configurado e corrigido
- Endpoints de teste expostos para cada acao CRUD de Conta e Secret
- Ajustes no Postman para testes manuais
- Refatoracao geral para preparar testes

LINHA DO TEMPO (commits mais recentes -> mais antigos)
------------------------------------------------------
2026-05-25  02f30c9  Pedro Torres            Conserto Postman
2026-05-25  3b35bda  Pedro Torres            Refatoracao para Testes
2026-05-25  017c1de  Pedro Torres            Refatoracao Cobranca
2026-05-23  9dd58b9  Joao Gabriel            CRUD ChavePix
2026-05-19  9b17813  Joao Gabriel            Entidade Auditoria preenchida
2026-05-18  09c8eea  Pedro H. Almeida        Merge PR #3 (atualizacao da main)
2026-05-18  03737d5  Pedro Torres            Criacao da Cobranca
2026-05-18  1ece7ec  Pedro Torres            Endpoints CRUD Conta e Secret
2026-05-18  b3ee0bb  Pedro Torres            Integracao Conta -> Secret (FK)
2026-05-18  9a5d683  Pedro Torres            Criacao CRUD Secret
2026-05-18  ab57bc8  Pedryn-A07              CRUD de contas (operacional)
2026-05-09  b68a269  Pedryn-A07              Pastas que faltavam (placeholders)
2026-05-09  ab1a566  Pedryn-A07              Pastas faltantes da arquitetura
2026-05-09  0a66d78  Pedro Torres            Correcao MockServer (com Copilot)
2026-05-04  de95753  Pedryn-A07              Atualizacao versao do Postgres
2026-05-02  ffaf556  Pedryn-A07              Adicao do producer RabbitMQ
2026-04-27  491bfbb  Pedro Torres            Correcao arquivo de teste (Copilot)
2026-04-27  40b7a77  Pedro Torres            Criacao do Mock Server (Copilot)
2026-04-27  2aba164  Pedro Torres            Correcao .gitignore
2026-04-27  e34674d  Pedro Torres            Modelagem do banco
2026-04-27  804a008  Pedro Torres            Remocao de build artifacts do repo
2026-04-24  1eab42d  Pedryn-A07              Criacao do docker-compose + Postgres

EM ANDAMENTO / PROXIMOS PASSOS (inferidos dos commits e mensagens)
------------------------------------------------------------------
- Finalizar a modelagem definitiva do banco de dados
- Substituir implementacoes provisorias do CRUD de Conta
- Ajustar a "gambiarra" do producer RabbitMQ para uma solucao definitiva
- Cobertura de testes (a refatoracao recente foi feita "para testes")
- Estabilizar a colecao do Postman para testes manuais

SEGURANCA / CONFIGURACAO DO REPO
--------------------------------
- Secret scanning: ATIVADO
- Secret scanning push protection: ATIVADO
- Dependabot security updates: DESATIVADO

================================================================================
Fim do resumo - gerado automaticamente a partir da API publica do GitHub.
================================================================================
