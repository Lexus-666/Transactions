using kursah_5semestr.Abstractions;
using kursah_5semestr.Contracts;
using kursah_5semestr;
using Microsoft.AspNetCore.Mvc;
using kursah_5semestr.Controllers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using kursah_5semestr.Services;

namespace kursah_5semestr.Contracts
{

    [Route("/transactions")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TransactionsController : ControllerBase
    {
        private ITransactionsService _transactionsService;
        private IUsersService _usersService;
        private ILogger _logger;

        public TransactionsController(ITransactionsService transactionsService, IUsersService usersService, ILogger<TransactionsController> logger)
        {
            _transactionsService = transactionsService;
            _usersService = usersService;
            _logger = logger;
        }

        private TransactionOutDto ToDto(Transaction transaction)
        {
            var orderDto = new OrderOutDto(
                Id: transaction.Order.Id,
                Date: transaction.Order.Date,
                Status: transaction.Order.Status,
                Amount: transaction.Order.Amount
                );
            return new TransactionOutDto(
                Id: transaction.Id,
                Status: transaction.Status,
                Order: orderDto,
                TransactionDetails: transaction.TransactionDetails,
                CreatedAt: transaction.CreatedAt,
                PaidAt: transaction.PaidAt,
                Amount: transaction.Amount
                );
        }

        [HttpGet]
        public async Task<ActionResult<IList<TransactionOutDto>>> GetTransactions()
        {
            var user = Utils.GetAuthenticatedUser(HttpContext, _usersService);
            if (user == null)
            {
                _logger.LogError("User session information not found");
                return Unauthorized(new StatusOutDto("error"));
            }
            var transactions = await _transactionsService.GetTransactionsByUser(user.Id);
            return Ok(transactions.Select(ToDto));
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<TransactionOutDto>> GetTransactionById(Guid id)
        {
            var transaction = await _transactionsService.GetTransactionById(id);
            if (transaction == null)
            {
                _logger.LogError($"Transaction {id} not found");
                return NotFound(new StatusOutDto("error", "Transaction not found"));
            }
            return Ok(ToDto(transaction));
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<ActionResult> DeleteTransaction(Guid id)
        {
            var transaction = await _transactionsService.GetTransactionById(id);
            if (transaction == null)
            {
                _logger.LogError($"Transaction {id} not found");
                return NotFound(new StatusOutDto("error", "Transaction not found"));
            }
            if (transaction.Status != TransactionStatus.New)
            {
                _logger.LogError($"Could not delete transaction {id} with status '{transaction.Status}'");
                return BadRequest(new StatusOutDto("error", $"Cannot delete a transaction with status other than 'new'"));
            }
            var success = await _transactionsService.DeleteTransaction(id);
            if (!success)
            {
                _logger.LogError($"Could not delete transaction {id}");
                return NotFound(new StatusOutDto("error"));
            }

            return Ok(new StatusOutDto("ok"));
        }

        [HttpPatch]
        [Route("{id}/pay")]
        public async Task<ActionResult> PayTransaction(Guid id)
        {
            var transaction = await _transactionsService.GetTransactionById(id);
            if (transaction == null)
            {
                _logger.LogError($"Transaction {id} not found");
                return NotFound(new StatusOutDto("error", "Transaction not found"));
            }
            if (transaction.Status != TransactionStatus.New)
            {
                _logger.LogError($"Transaction {id} with status '{transaction.Status}' could not be paid");
                return BadRequest(new StatusOutDto("error", $"Cannot pay a transaction with status other than 'new'"));
            }
            var success = await _transactionsService.UpdateTransactionStatus(id, TransactionStatus.Paid);
            if (!success)
            {
                _logger.LogError($"Transaction {id} could not be paid");
                return NotFound(new StatusOutDto("error", "Transaction could not be paid"));
            }

            return Ok(new StatusOutDto("ok"));
        }
    }
}