using Lab2.DataAccess;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LB4_WEBAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly BankDbContext _db;

        public AccountController(BankDbContext db)
        {
            _db = db;

        }

        [HttpGet]
        public ActionResult<IEnumerable<Account>> GetAccountsList()
        {
            var accounts = _db.Accounts.ToList();
            return Ok(accounts);
        }

        [HttpGet("{id}")]
        public ActionResult<Account> GetAccountById(int id)
        {
            var account = _db.Accounts.FirstOrDefault(account => account.Id == id);

            if (account == null)
                return NotFound();

            return Ok(account);
        }

        [HttpPost]
        public IActionResult CreateAccount([FromBody] Account account)
        {
            if (account == null)
                return BadRequest("Account data is null");

            _db.Accounts.Add(account);
            _db.SaveChanges();

            return CreatedAtAction(nameof(GetAccountById), new { id = account.Id }, account);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateAccount(int id, [FromBody] Account updatedAccount)
        {
            if (updatedAccount == null)
                return BadRequest("Account data is null");

            var existingAccount = _db.Accounts.FirstOrDefault(account => account.Id == id);

            if (existingAccount == null)
                return NotFound();

            existingAccount.Name = updatedAccount.Name;
            existingAccount.Email = updatedAccount.Email;
            existingAccount.Balance = updatedAccount.Balance;
            existingAccount.BankId = updatedAccount.BankId;

            _db.SaveChanges();

            return Ok(existingAccount);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteAccount(int id)
        {
            var account = _db.Accounts.FirstOrDefault(account => account.Id == id);

            if (account == null)
                return NotFound();

            _db.Accounts.Remove(account);
            _db.SaveChanges();

            return NoContent();
        }

        [HttpGet("Search")]
        public ActionResult<IEnumerable<Account>> SearchAccounts(string? name, string? email, int? balance)
        {
            var query = _db.Accounts.AsQueryable();

            if (!string.IsNullOrEmpty(name))
                query = query.Where(account => account.Name.Contains(name));

            if (!string.IsNullOrEmpty(email))
                query = query.Where(account => account.Email.Contains(email));

            if (balance.HasValue)
                query = query.Where(account => account.Balance >= balance.Value - 500 && account.Balance <= balance.Value + 500);

            var accounts = query.ToList();

            if (!accounts.Any())
                return NotFound($"No accounts found.");

            return Ok(accounts);
        }


    }
}
