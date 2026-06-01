# pix-integration-hub — Fixed

## Changes applied

### Critical bug fixes

1. **`ContaController` — async/await**: All methods now properly `await` the service calls. Previously they returned `Task` objects serialized as JSON.

2. **`ItauPixAdapter.GetChargeStatusAsync` — swapped paths**: `CobV` now correctly maps to `/cobv/{txId}` and `Cob` to `/cob/{txId}`. The original code had them reversed.

3. **`Auditoria.TxId` type mismatch**: The column was `integer` in the migration but `string` in the domain. Entities now use `string TxId` consistently, matching the BACEN standard (up to 35 alphanumeric characters).

4. **`CreatePixChargeHandler.cs` — spec text inside code**: The file had documentation text from the PDF pasted literally inside the C# code, making it invalid. Fully rewritten.

5. **`InvoiceRepository` / `PixChargeRepository` — missing DbContext**: Referenced `BankingDbContext` which did not exist. Repositories now use `ApplicationDbContext` correctly.

6. **`INotificationService` not declared**: Added `Domain/Services/Services.cs` with `INotificationService` and `IPixReconciliationService`.

7. **`TxId.cs` — wrong namespace**: Was `BankingHub.Domain.ValueObjects`. Fixed to `Domain.ValueObjects` to match the rest of the project.

8. **`CobrancaService` — invalid TxId format**: Replaced `Guid.NewGuid().ToString("N")` with the proper BACEN-compliant format (`PIX{yyyyMMddHHmmss}{random10}`[..35]).

9. **`RabbitMqProducer` — hardcoded credentials**: Credentials and host are now read from `appsettings.json` via `IOptions<RabbitMqOptions>`.

10. **`RabbitMQ test code in Program.cs`**: Removed. If the RabbitMQ server was not running, the API would crash on startup.

### Architecture fixes

11. **Standardized namespaces**: All files use consistent namespaces (`Domain.*`, `Application.*`, `Infrastructure.*`, `ApiService.*`).

12. **Renamed Portuguese identifiers to English**:
    - `Conta` → `Account`
    - `ChavePix` → `PixKey`
    - `Cobranca` → `Charge`
    - `Auditoria` → `Audit`
    - `NumeroConta` → `AccountNumber`
    - `Agencia` → `Agency`
    - `Documento` → `Document`
    - `Certificado` → `Certificate`
    - `SenhaCertificado` → `CertificatePassword`
    - `ClienteSecret` → `ClientSecret`
    - Folder `Acconut/` → `Account/` (typo fixed)
    - File `IContaServicec.cs` → `IServices.cs` (typo fixed)

13. **`ItauOptions` deduplication**: Two conflicting options classes (`ItauOptions` and `ItauAdapterOptions` with typo `ProdutionBaseUrl`) were merged into one `ItauOptions` with all required fields.

14. **`TransactionBehavior`**: Removed problematic `System.Transactions.TransactionScope` (known issues with async + PostgreSQL). Transactions are now handled through `IUnitOfWork`.

15. **Controllers registered**: `Program.cs` now calls `AddControllers()` and `MapControllers()`.

16. **Duplicate routes removed**: Minimal API routes for accounts/secrets were removed from `Program.cs` since the controllers already handle them.

17. **`ExceptionMiddleware` implemented**: Returns proper `ProblemDetails` JSON for `DomainException`, `KeyNotFoundException`, `BankIntegrationException`, etc.

18. **Implemented previously empty files**:
    - `CreateInvoice` (Command + Handler + Validator)
    - `ProcessWebhook` (Command + Handler)
    - `GetInvoice` (Query + Handler)
    - `GetPixChargeStatus` (Query + Handler)
    - `WebhooksController`
    - `ChargesController`

19. **`GetQrCodeAsync` implemented**: Queries the Itaú charge endpoint and extracts `pixCopiaECola` and `location` fields.

20. **`ValidateWebhook` and `ParseWebhookEvent` implemented**: Basic Itaú webhook handling with header validation and payload parsing.

21. **Removed `/weatherforecast` scaffold endpoint** from `Program.cs`.

### Database column mapping

The `ApplicationDbContext` now uses `.HasColumnName()` to map English property names to the existing Portuguese column names in the database. This means **no new migrations are needed** — the existing tables continue to work.

## appsettings.json

The `Itau` and `RabbitMq` sections were added. Fill in your credentials:

```json
"Itau": {
  "ClientId": "YOUR_CLIENT_ID",
  "ClientSecret": "YOUR_CLIENT_SECRET",
  "UseSandbox": true
},
"RabbitMq": {
  "HostName": "localhost",
  "UserName": "guest",
  "Password": "guest"
}
```
