# fcg-catalog-api

Microsservico de **Catalogo** da plataforma **FIAP Cloud Games (FCG) - Fase 2**:

- catalogo de jogos (CRUD de administrador, busca publica com preco promocional efetivo e GraphQL);
- promocoes de desconto por conjunto de jogos (vale sempre o **maior desconto ativo**);
- biblioteca do usuario (`library_items`);
- inicio do fluxo de compra: registra o pedido `Pending`, publica `OrderPlacedEvent` e conclui (ou recusa) a compra ao consumir `PaymentProcessedEvent` da PaymentsAPI.

O servico **apenas valida** o JWT emitido pela UsersAPI (nao emite token nem conhece senha). Os contratos de integracao (eventos, filas, JWT, convencoes) sao definidos no repositorio [`fcg-contracts`](https://github.com/sampaiobrenner/fcg-contracts) e a orquestracao (docker-compose e guia de Kubernetes) no [`fcg-orchestration`](https://github.com/sampaiobrenner/fcg-orchestration).

## Fluxo de compra

```
POST /api/v1/orders ──> Order Pending ──(outbox)──> OrderPlacedEvent ──> fcg.order-placed.v1 ──> payments.order-placed (PaymentsAPI)
                                                                                                         │
GET /api/v1/library <── LibraryItem + Completed  <── catalog.payment-processed <── fcg.payment-processed.v1 ┘
                         (ou Rejected + reason)
```

1. `POST /api/v1/orders` valida o jogo (404 inexistente/inativo), a biblioteca e o pedido pendente (409), calcula o preco efetivo e grava o pedido `Pending` + o `OrderPlacedEvent` na mesma transacao (EF Core Outbox). Responde `202 Accepted`.
2. A PaymentsAPI aprova ou recusa e publica `PaymentProcessedEvent`.
3. O consumer da fila `catalog.payment-processed` conclui o pedido: `Approved` -> item na biblioteca + `Completed`; `Rejected` -> `Rejected` com `reason`.

## Mensageria

| Direcao | Evento (`Fcg.Contracts.Events.V1`) | Exchange | Fila |
|---|---|---|---|
| Publica | `OrderPlacedEvent(EventId, OccurredAt, OrderId, UserId, GameId, Price)` | `fcg.order-placed.v1` | consumida pela PaymentsAPI em `payments.order-placed` |
| Consome | `PaymentProcessedEvent(EventId, OccurredAt, OrderId, UserId, GameId, Price, Status, Reason)` | `fcg.payment-processed.v1` | `catalog.payment-processed` (variavel `Messaging__Queues__CatalogPaymentProcessed`, default `QueueNames.CatalogPaymentProcessed`) |

- Payload conforme os samples do contrato: enums como texto (`"status": "Approved"`) e decimais como numero (`"price": 199.90`) - `DecimalNumberJsonConverter` + `JsonStringEnumConverter` no serializer do MassTransit.
- Publicacao via EF Core Outbox; consumo com Inbox (deduplicacao por `MessageId`) e retry 1s/5s/15s/30s (sem retry para `BusinessException`/`ValidationException`). Esgotado o retry, a mensagem vai para `catalog.payment-processed_error`.
- Idempotencia de negocio: pedido que ja saiu de `Pending` ignora novos resultados; indice unico `(user_id, game_id)` em `library_items` impede item duplicado.

## Endpoints

Autenticacao: `Authorization: Bearer <jwt>` emitido pela UsersAPI (claims `sub`, `email`, `name`, `role`). Erros em `ProblemDetails` (400 validacao, 401, 403, 404, 409, 422). JSON camelCase, enums como texto.

| Metodo e rota | Acesso | Resposta |
|---|---|---|
| `GET /api/v1/games?title&genre&minPrice&maxPrice&page=1&pageSize=20` | anonimo | `200` `[{id, title, description, genre, price, discountPercent, effectivePrice, releaseDate}]` (somente jogos ativos; leitura Dapper) |
| `GET /api/v1/games/{id}` | anonimo | `200` `{id, title, description, genre, price, releaseDate, active, createdAt}`; `404` |
| `POST /api/v1/games` | `Administrator` | `201` + `Location`; `400`; `409` titulo duplicado |
| `PUT /api/v1/games/{id}` | `Administrator` | `200`; `400`; `404` |
| `DELETE /api/v1/games/{id}` | `Administrator` | `204` (desativa); `404` |
| `GET /api/v1/promotions` | `Administrator` | `200` `[{id, name, discountPercent, startsAt, endsAt, active, gameIds}]` |
| `POST /api/v1/promotions` | `Administrator` | `{name, discountPercent, startsAt, endsAt, gameIds}` -> `201`; `400`; `404` jogo inexistente |
| `DELETE /api/v1/promotions/{id}` | `Administrator` | `204` (desativa); `404` |
| `GET /api/v1/library` | autenticado | `200` `[{gameId, title, genre, pricePaid, acquiredAt}]` do `sub` do token |
| `POST /api/v1/orders` | autenticado | `{gameId}` -> `202` + `Location: /api/v1/orders/{orderId}` com `{orderId, status: "Pending", price}`; `404` jogo inexistente/inativo; `409` jogo ja na biblioteca ou pedido pendente |
| `GET /api/v1/orders/{id}` | dono ou `Administrator` | `200` `{orderId, userId, gameId, price, status, reason, createdAt, updatedAt}`; `404` (inclusive pedido de outro usuario) |
| `GET /api/v1/orders` | autenticado | `200` pedidos do usuario, mais recentes primeiro |
| `POST /graphql` | anonimo | query `games` com filtro e ordenacao (HotChocolate) |

Exemplo GraphQL:

```graphql
{ games(where: { genre: { eq: "RPG" } }, order: { price: DESC }) { id title genre price releaseDate } }
```

Banco `fcg_catalog`: tabelas `games`, `promotions`, `promotion_games`, `library_items` (unico `user_id, game_id`), `orders` (unico parcial `user_id, game_id WHERE status = 'Pending'`) e as tabelas de outbox/inbox do MassTransit. No startup sao aplicadas as migrations e o seed idempotente dos 4 jogos do monolito (The Witcher 3, Hades, Stardew Valley, Celeste), somente quando `games` esta vazia.

## Stack

.NET 10 · Minimal API · MediatR 12 · FluentValidation · EF Core 10 + PostgreSQL · Dapper (leituras do catalogo e da biblioteca) · HotChocolate (GraphQL) · MassTransit 8 + RabbitMQ (EF Core Outbox/Inbox) · JWT · Serilog · OpenAPI + Scalar · xUnit + Testcontainers.

## Arquitetura

Camadas com dependencia estritamente para dentro: `WebApi -> Infrastructure -> Application -> Domain`.

```
src/
  Fcg.Catalog.Domain/           Regras de negocio. Organizado por agregado.
    _Shared/                    Modules, DomainModelBase/PersistenceModelBase, IDomainEvent, BusinessException
  Fcg.Catalog.Application/      Casos de uso (MediatR). Organizado por <Agregado>/<CasoDeUso>/.
    _Shared/                    ICatalogDbContext (leitura), Parser<TIn,TOut>, IIntegrationEventPublisher, ValidationBehavior
  Fcg.Catalog.Infrastructure/   Portas de saida: DbContext, repositorios, publicacao de eventos. Organizado por agregado.
    _Shared/                    CatalogDbContext (+ tabelas de outbox/inbox), MassTransitIntegrationEventPublisher
  Fcg.Catalog.WebApi/           Portas de entrada: endpoints REST e consumers. Organizado por <Agregado>/<CasoDeUso>/.
    _Shared/                    Endpoints (IEndpoint), erros (ProblemDetails), JWT, mensageria, health checks, migrations
test/
  Fcg.Catalog.UnitTests/        Espelha src/ 1:1
  Fcg.Catalog.IntegrationTests/ WebApplicationFactory + PostgreSQL (Testcontainers) + MassTransit test harness
```

Diretrizes:

- **Um caso de uso = uma pasta** com `Command`/`Query`, `Handler`, `Validator`, `Parser` e, na WebApi, o `Endpoint` (implementa `IEndpoint`) ou o `Consumer`.
- **Escrita:** handler -> servico/modelo de dominio -> repositorio do agregado (recebe e devolve o modelo de dominio).
- **Leitura:** handler projeta direto de `ICatalogDbContext.DataSet<T>()`, sem repositorio.
- **Dominio:** `DomainModel` (comportamento) envolve o `PersistenceModel` (mapeado pelo EF via `IEntityTypeConfiguration<T>`, descoberto automaticamente).
- **Mapeamento:** `Parser<TIn,TOut>` explicito, sem AutoMapper.
- **Mensagens de erro:** sempre em `Properties/*Resources.resx` da propria camada, nunca string literal.
- **DI:** cada camada registra seus servicos no seu `FcgCatalog<Camada>Module`; `Program.cs` apenas compoe os 4 modulos.
- **Eventos de integracao:** publicados via `IIntegrationEventPublisher` antes do `SaveChangesAsync` - o outbox grava na mesma transacao.
- **Consumers:** ficam na WebApi, delegam para um `Command` via MediatR e sao idempotentes (inbox do MassTransit). Cada consumer tem um `ConsumerDefinition` com `EndpointName` lido de `Messaging:Queues:<Fila>` (ConfigMap) e default na constante de `Fcg.Contracts.Messaging.QueueNames`.
- **Contratos:** eventos, filas, claims e papeis vem do pacote `PosTech.Fiap.CloudGames.Contracts`; nunca redefina localmente.
- **Usuario autenticado:** injete `ICurrentUser` (Application/_Shared/Security), que le as claims `sub`, `email`, `name` e `role` do token.
- **Erros HTTP:** `ValidationException` 400, `UnauthorizedException` 401, `ForbiddenException` 403, `NotFoundException` 404, `ConflictException` 409, `BusinessException` 422.
- **JSON:** camelCase e enums como string.

Exemplo de caso de uso:

```
Application/Games/CreateGame/CreateGameCommand.cs
Application/Games/CreateGame/CreateGameCommandHandler.cs
Application/Games/CreateGame/CreateGameCommandValidator.cs
Application/Games/CreateGame/CreateGameResponseParser.cs
WebApi/Games/CreateGame/CreateGameEndpoint.cs
```

## Executando localmente

Pre-requisitos: .NET SDK 10 e PostgreSQL + RabbitMQ da infraestrutura do [`fcg-orchestration`](https://github.com/sampaiobrenner/fcg-orchestration) (`docker compose up -d`). O ambiente `Development` (`appsettings.Development.json`) ja aponta para `localhost` com usuario/senha `fcg` e a chave JWT de desenvolvimento.

```bash
dotnet run --project src/Fcg.Catalog.WebApi
```

- API: http://localhost:5102
- Documentacao (Scalar): http://localhost:5102/scalar/v1
- GraphQL (Nitro): http://localhost:5102/graphql
- Health: `/health/live` (processo) e `/health/ready` (PostgreSQL + bus)

Para testar sem a UsersAPI, gere um token com a mesma chave/issuer/audience (ver `test/Fcg.Catalog.IntegrationTests/_Fixtures/TestJwtFactory.cs`), com `role` `Administrator` para os endpoints de administracao.

Migrations (as pendentes sao aplicadas no startup quando `Database__ApplyMigrationsOnStartup=true`):

```bash
dotnet tool restore
dotnet ef migrations add <Nome> -p src/Fcg.Catalog.Infrastructure -s src/Fcg.Catalog.WebApi -o _Shared/Context/Migrations
```

## Testes

```bash
dotnet test
```

- `Fcg.Catalog.UnitTests`: dominio (`Money`, `Game`, `Promotion`, `PricingService`, `Order`), handlers (Moq / EF Core InMemory), consumer e conversores.
- `Fcg.Catalog.IntegrationTests`: sobe um PostgreSQL real via **Testcontainers** (requer Docker), aplica migrations e seed e troca o RabbitMQ pelo `ITestHarness` do MassTransit. Cobre busca com promocao, autorizacao, pedidos (202/404/409), publicacao do `OrderPlacedEvent`, consumer `Approved`/`Rejected`/duplicado e o payload JSON contra os samples do `fcg-contracts`.

## CI/CD

Workflow unico [`ci.yml`](.github/workflows/ci.yml):

| Job | Quando | O que faz |
|---|---|---|
| `build-test` | PR, push na `main`, tags `v*` | restore, build, testes com cobertura e analise no SonarCloud (projeto `sampaiobrenner_fcg-catalog-api`, requer o secret `SONAR_TOKEN`) |
| `docker` | PR | build da imagem |
| `publish` | push na `main` e tags `v*`, apos os testes | publica `ghcr.io/sampaiobrenner/fcg-catalog-api` (`latest`, `sha-<commit>`, semver) usando o [`docker-publish.yml`](https://github.com/sampaiobrenner/fcg-orchestration/blob/main/.github/workflows/docker-publish.yml) do `fcg-orchestration` |

## Docker

```bash
docker build -t ghcr.io/sampaiobrenner/fcg-catalog-api:latest .
docker run --rm -p 5102:8080 \
  -e ConnectionStrings__Default="Host=host.docker.internal;Port=5432;Database=fcg_catalog;Username=fcg;Password=fcg" \
  -e RabbitMq__Host=host.docker.internal -e RabbitMq__Username=fcg -e RabbitMq__Password=fcg \
  -e Jwt__Key=fcg-dev-only-signing-key-change-me-0123456789 \
  ghcr.io/sampaiobrenner/fcg-catalog-api:latest
```

Imagem multi-stage (`sdk:10.0` restore + publish `-c Release` -> `aspnet:10.0`), usuario non-root (`USER $APP_UID`), porta `8080`. O `.dockerignore` exclui `bin/`, `obj/`, `test/` e `.git/`.

## Kubernetes

Manifests em [`k8s/`](k8s):

| Arquivo | Conteudo |
|---|---|
| `deployment.yaml` | `Deployment` `catalog-api`, 2 replicas, `envFrom` ConfigMap + Secret, `startupProbe`/`livenessProbe` em `/health/live`, `readinessProbe` em `/health/ready`, requests `100m/128Mi`, limits `500m/256Mi` |
| `service.yaml` | `Service` `ClusterIP` `catalog-api:80` -> `http` (8080) |
| `configmap.yaml` | `catalog-api-config`: configuracoes nao sensiveis, incluindo `Messaging__Queues__CatalogPaymentProcessed` |
| `secret.yaml` | `catalog-api-secret` (valores de desenvolvimento): connection string, credenciais do RabbitMQ e chave JWT |

```bash
docker build -t ghcr.io/sampaiobrenner/fcg-catalog-api:latest .
kind load docker-image ghcr.io/sampaiobrenner/fcg-catalog-api:latest   # ou: minikube image load
kubectl apply -f k8s/
kubectl get pods -l app=catalog-api
kubectl port-forward svc/catalog-api 8080:80 && curl localhost:8080/health/ready
```

As migrations rodam no startup (`Database__ApplyMigrationsOnStartup=true`; o EF Core usa lock de migracao e o seed usa advisory lock, entao as 2 replicas podem subir juntas). O `/health/ready` so fica `Healthy` com PostgreSQL e RabbitMQ no cluster (`postgres:5432`, `rabbitmq:5672`).

## Variaveis de ambiente

| Variavel | Origem K8s | Descricao | Exemplo |
|---|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | ConfigMap | Ambiente | `Production` |
| `ASPNETCORE_HTTP_PORTS` | Dockerfile | Porta HTTP | `8080` |
| `ConnectionStrings__Default` | Secret | Conexao PostgreSQL | `Host=postgres;Port=5432;Database=fcg_catalog;Username=fcg;Password=***` |
| `Database__ApplyMigrationsOnStartup` | ConfigMap | Aplica migrations e o seed dos jogos no startup | `true` |
| `RabbitMq__Host` | ConfigMap | Host do RabbitMQ | `rabbitmq` |
| `RabbitMq__VirtualHost` | ConfigMap | Virtual host | `/` |
| `RabbitMq__Username` | Secret | Usuario do RabbitMQ | `fcg` |
| `RabbitMq__Password` | Secret | Senha do RabbitMQ | `***` |
| `Jwt__Issuer` | ConfigMap | Emissor do token | `fcg-users-api` |
| `Jwt__Audience` | ConfigMap | Audiencia do token | `fcg` |
| `Jwt__Key` | Secret | Chave simetrica (min. 32 caracteres, igual em todos os servicos) | `***` |
| `Messaging__Queues__CatalogPaymentProcessed` | ConfigMap | Fila consumida do `PaymentProcessedEvent` (default `QueueNames.CatalogPaymentProcessed`) | `catalog.payment-processed` |
