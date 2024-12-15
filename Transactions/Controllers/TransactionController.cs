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

        public TransactionsController(ITransactionsService transactionsService, IUsersService usersService)
        {
            _transactionsService = transactionsService;
            _usersService = usersService;
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
                return NotFound(new StatusOutDto("error", "Transaction not found"));
            }
            if (transaction.Status != "new")
            {
                return BadRequest(new StatusOutDto("error", $"Cannot delete a transaction with status '{transaction.Status}'"));
            }
            var success = await _transactionsService.DeleteTransaction(id);
            if (!success)
            {
                return NotFound(new StatusOutDto("error", "Transaction not found"));
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
                return NotFound(new StatusOutDto("error", "Transaction not found"));
            }
            if (transaction.Status != "new")
            {
                return BadRequest(new StatusOutDto("error", $"Cannot pay a transaction with status '{transaction.Status}'"));
            }
            var success = await _transactionsService.UpdateTransactionStatus(id, "paid");
            if (!success)
            {
                return NotFound(new StatusOutDto("error", "Transaction could not be paid"));
            }

            return Ok(new StatusOutDto("ok"));
        }
    }
}