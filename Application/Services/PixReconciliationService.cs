using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using BankingHub.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging; // Importante para logar o que acontece no processo

namespace BankingHub.Application.Services
{
    /// <summary>
    /// Serviço responsável pela conciliação bancária dos pagamentos via Pix.
    /// Ele serve como uma "rede de segurança" para atualizar cobranças pendentes caso o Webhook falhe.
    /// </summary>
    public class PixReconciliationService
    {
        private readonly IBankPixAdapter _bankPixAdapter;
        private readonly IMediator _mediator;
        private readonly INotificationService _notificationService;
        private readonly ILogger<PixReconciliationService> _logger; // Registra logs para auditoria financeira

        // Injeção de dependência das interfaces necessárias
        public PixReconciliationService(
            IBankPixAdapter bankPixAdapter,
            IMediator mediator,
            INotificationService notificationService,
            ILogger<PixReconciliationService> logger)
        {
            _bankPixAdapter = bankPixAdapter;
            _mediator = mediator;
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// Executa a varredura e conciliação de todas as cobranças Pix que estão presas no status "Pendente".
        /// </summary>
        public async Task ReconcilePendingPixChargesAsync()
        {
            _logger.LogInformation("Iniciando o processo de conciliação de Pix...");

            try
            {
                // 1. BUSCA INTERNA:
                // Dispara uma query (CQRS) para buscar no seu banco de dados as cobranças que ainda constam como pendentes.
                // ADAPTAÇÃO: Se você não usa Mediator para queries, substitua por uma chamada direta ao seu repositório.
                // Ex: var pendingCharges = await _pixRepository.GetPendingChargesAsync();
                var pendingCharges = await _mediator.Send(new GetPendingPixChargesQuery());

                if (pendingCharges == null || !pendingCharges.Any())
                {
                    _logger.LogInformation("Nenhuma cobrança Pix pendente encontrada para conciliação.");
                    return;
                }

                // 2. PROCESSAMENTO INDIVIDUAL:
                // Iteramos sobre cada cobrança pendente para verificar o status real na API do Banco
                foreach (var charge in pendingCharges)
                {
                    try
                    {
                        _logger.LogInformation("Verificando status do Pix Id: {ChargeId} (TxId: {TxId}) no banco...", charge.Id, charge.TxId);

                        // 3. CONSULTA EXTERNA (API DO BANCO):
                        // O Adapter traduz a chamada para o banco específico (Itaú, Inter, BB, etc) usando o TxId (Identificador do Pix)
                        var bankStatus = await _bankPixAdapter.ConsultPixStatusAsync(charge.TxId);

                        // 4. TOMADA DE DECISÃO (REGRAS DE NEGÓCIO):
                        
                        // Cenário A: O cliente pagou, mas o nosso sistema ainda não sabia
                        if (bankStatus.IsPaid)
                        {
                            _logger.LogWarning("Inconsistência encontrada! Pix {ChargeId} foi PAGO no banco, mas constava como pendente. Conciliando...", charge.Id);

                            // Atualiza o status no nosso banco de dados para "Pago" (via Command