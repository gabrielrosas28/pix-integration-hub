namespace BankingHub.Application.Commands.CreatePixCharge;

/// 
/// Handler que processa o comando de criação de cobrança Pix.
/// Orquestra a lógica de aplicação sem conter regras de negócio.
/// 

public sealed class CreatePixChargeHandler
: IRequestHandler
{
private readonly IInvoiceRepository _invoiceRepository;
private readonly IPixChargeRepository _chargeRepository;
private readonly IBankAdapterFactory _adapterFactory;
private readonly IUnitOfWork _unitOfWork;
private readonly ILogger _logger;

public CreatePixChargeHandler(
IInvoiceRepository invoiceRepository,
IPixChargeRepository chargeRepository,
IBankAdapterFactory adapterFactory,
IUnitOfWork unitOfWork,
ILogger logger)
{
       _invoiceRepository = invoiceRepository;
       _chargeRepository = chargeRepository;
       _adapterFactory = adapterFactory;
       _unitOfWork = unitOfWork;
       _logger = logger;
}

public async Task Handle(
CreatePixChargeCommand cmd,
CancellationToken ct)
{
// 1. Busca a Invoice
var invoice = await _invoiceRepository.GetByIdAsync(
           InvoiceId.From(cmd.InvoiceId), ct)
?? throw new NotFoundException("Invoice não encontrada");

// 2. Gera TxId único
var txId = TxId.Generate();

// 3. Obtém o adapter do banco configurado na Invoice
var adapter = _adapterFactory.Get(invoice.BankId);

// 4. Determina o tipo de cobrança
var chargeType = cmd.ChargeType.ToUpperInvariant() switch
{
"COB" => PixChargeType.Cob,
"COBV" => PixChargeType.CobV,
           _ => throw new ValidationException("Tipo de cobrança invál
};

// 5. Monta request normalizado
var chargeRequest = new ChargeRequest(
TxId: txId.Value,
Type: chargeType,
Amount: cmd.Amount,
PixKey: cmd.PixKey,
DueDate: cmd.DueDate,
ExpiresInSeconds: cmd.ExpiresInSeconds,
PayerMessage: cmd.PayerMessage);

// 6. Cria cobrança no banco via adapter
ChargeResponse bankResponse;
try
{
           bankResponse = chargeType == PixChargeType.CobV
? await adapter.CreateCobVAsync(chargeRequest, ct)
: await adapter.CreateCobAsync(chargeRequest, ct);
}
catch (Exception ex)
{
           _logger.LogError(ex, "Erro ao criar cobrança no banco {Ban
throw new IntegrationException($"Falha na comunicação com {
}

// 7. Obtém QR Code
var qrCode = await adapter.GetQrCodeAsync(txId.Value, chargeTy

6.1.2 Validator com FluentValidation
// 8. Persiste a cobrança no domínio
var pixCharge = PixCharge.Create(
txId: txId,
invoiceId: invoice.Id,
bankId: invoice.BankId,
chargeType: chargeType,
emv: EmvCode.From(qrCode.Emv),
pixLink: qrCode.PixLink,
rawPayload: bankResponse.Raw);

await _chargeRepository.AddAsync(pixCharge, ct);

// 9. Associa TxId à Invoice
       invoice.AssignTxId(txId);

// 10. Commit da transação
await _unitOfWork.SaveChangesAsync(ct);

       _logger.LogInformation(
"Cobrança Pix criada: TxId={TxId}, Invoice={InvoiceId}, Ba
           txId, invoice.Id, invoice.BankId);

return new CreatePixChargeResult(
TxId: txId.Value,
Status: bankResponse.Status.ToString(),
Emv: qrCode.Emv,
PixLink: qrCode.PixLink);
}
}