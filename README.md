## Setup and Run

1. Install the .NET 10 SDK.
2. Clone the repository and open `Wallet.slnx`.
3. Run the `Wallet.Api` project.

The application uses SQLite, so no separate database server or connection setup is required. On startup, the application automatically creates the SQLite database if it does not already exist.

Swagger is available at:

/swagger

Postman Documentation is available at:

Wallet.Api/Wallet.postman_collection.json


## Performance Notes

The following measures were applied to keep the API responsive under normal local load:

- Async EF Core database operations.
- Unique index on `ReferenceId` for fast idempotency checks.
- Index on `WalletId` and transaction timestamp for transaction-history lookups.
- Pagination for wallet and transaction-history endpoints.
- Read-only queries use `AsNoTracking()`.
- Response projections fetch only required fields.
- Database transactions using `BeginTransactionAsync()` for debit and transfer operations.
- Optimistic concurrency using the `Version` property configured with `IsConcurrencyToken()`.
- No unnecessary joins in balance and transaction queries.

## Scaling Notes

The current implementation is suitable for a single-node deployment. In production, it can be scaled by:

- Moving from SQLite to PostgreSQL or SQL Server.
- Deploying multiple stateless API instances behind a load balancer or API gateway.
- Adding Redis caching for frequently requested balance reads.
- Using read replicas for transaction-history queries.
- Publishing events through RabbitMQ or Kafka for notifications and other asynchronous work.
- Adding an outbox pattern to reliably publish transaction events.
- Implementing centralized logging, metrics, health checks, and distributed tracing.

## Assumptions

- Each user has one wallet.
- Email address and mobile number are unique.
- `ReferenceId` is globally unique across credit, debit, and transfer requests.
- Balances use `decimal(18,2)`.
- The system uses a single fixed currency; multi-currency is out of scope.
- Transaction records are immutable after creation.
- SQLite is used to minimize local setup complexity.
- FluentValidation validates command input.
- Global exception handling returns safe API error responses.
- Debit and transfer operations use EF Core transactions.
- Wallet updates use optimistic concurrency through the internal `Version` concurrency token.
- A Postman collection is included for testing the available endpoints.
- The project follows Clean Architecture and CQRS principles.