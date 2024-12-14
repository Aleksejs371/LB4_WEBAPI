using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Lab2.DataAccess;
using LB4_WEBAPI.DTO;
using Microsoft.EntityFrameworkCore;

namespace LB4_WEBAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BankController : ControllerBase
    {
        private readonly BankDbContext _db;

        public BankController(BankDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public ActionResult<IEnumerable<BankDto>> GetBanksList()
        {
            var banks = _db.Banks
                .Include(bank => bank.Accounts) 
                .Select(bank => new BankDto
                {
                    Id = bank.Id,
                    Name = bank.Name,
                    Description = bank.Description,
                    Specialization = bank.Specialization,
                    Account = bank.Accounts.Select(account => new AccountsDto
                    {
                        Name = account.Name,
                        Enail = account.Email,
                        Balance = account.Balance
                    }).ToList()
                })
                .ToList();

            return Ok(banks);
        }

        [HttpGet("{id}")]
        public ActionResult<BankDto> GetBankById(int id)
        {
            var bank = _db.Banks
                .Include(bank => bank.Accounts) 
                .FirstOrDefault(bank => bank.Id == id);

            if (bank == null)
                return NotFound();

            var bankDto = new BankDto
            {
                Id = bank.Id,
                Name = bank.Name,
                Description = bank.Description,
                Specialization = bank.Specialization,
                Account = bank.Accounts.Select(account => new AccountsDto
                {
                    Name = account.Name,
                    Enail = account.Email, 
                    Balance = account.Balance
                }).ToList()
            };

            return Ok(bankDto);
        }

        [HttpPost]
        public IActionResult CreateBank([FromBody] BankDto bankDto)
        {
            if (bankDto == null)
                return BadRequest("Bank data is null.");

            var bank = new Bank
            {
                Name = bankDto.Name,
                Description = bankDto.Description,
                Specialization = bankDto.Specialization
            };

            if (bankDto.Account != null && bankDto.Account.Count > 0)
            {
                bank.Accounts = bankDto.Account.Select(accountDto => new Account
                {
                    Name = accountDto.Name,
                    Email = accountDto.Enail, 
                    Balance = accountDto.Balance
                }).ToList();
            }

            _db.Banks.Add(bank);
            _db.SaveChanges();

            return CreatedAtAction(nameof(GetBankById), new { id = bank.Id }, bankDto);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateBank(int id, [FromBody] BankDto bankDto)
        {
            var existingBank = _db.Banks
                .Include(bank => bank.Accounts)
                .FirstOrDefault(bank => bank.Id == id);

            if (existingBank == null)
                return NotFound($"Bank with ID {id} not found.");

            existingBank.Name = bankDto.Name;
            existingBank.Description = bankDto.Description;
            existingBank.Specialization = bankDto.Specialization;

            if (bankDto.Account != null)
            {
                _db.Accounts.RemoveRange(existingBank.Accounts);

                existingBank.Accounts = bankDto.Account.Select(accountDto => new Account
                {
                    Name = accountDto.Name,
                    Email = accountDto.Enail, 
                    Balance = accountDto.Balance
                }).ToList();
            }

            _db.SaveChanges();

            var updatedBankDto = new BankDto
            {
                Id = existingBank.Id,
                Name = existingBank.Name,
                Description = existingBank.Description,
                Specialization = existingBank.Specialization,
                Account = existingBank.Accounts.Select(account => new AccountsDto
                {
                    Name = account.Name,
                    Enail = account.Email, 
                    Balance = account.Balance
                }).ToList()
            };

            return Ok(updatedBankDto);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteBank(int id)
        {
            var bank = _db.Banks.FirstOrDefault(bank => bank.Id == id);

            if (bank == null)
                return NotFound($"Bank with ID {id} not found.");

            _db.Banks.Remove(bank);
            _db.SaveChanges();

            return NoContent();
        }

        [HttpGet("Search")]
        public ActionResult<IEnumerable<BankDto>> SearchBanks(string? name, string? description, string? specialization)
        {
            var query = _db.Banks.AsQueryable();

            if (!string.IsNullOrEmpty(name))
                query = query.Where(bank => bank.Name.Contains(name));

            if (!string.IsNullOrEmpty(description))
                query = query.Where(bank => bank.Description.Contains(description));

            if (!string.IsNullOrEmpty(specialization))
                query = query.Where(bank => bank.Specialization.Contains(specialization));

            var banks = query
                .Select(bank => new BankDto
                {
                    Id = bank.Id,
                    Name = bank.Name,
                    Description = bank.Description,
                    Specialization = bank.Specialization,
                    Account = bank.Accounts.Select(account => new AccountsDto
                    {
                        Name = account.Name,
                        Enail = account.Email, 
                        Balance = account.Balance
                    }).ToList()
                })
                .ToList();

            if (!banks.Any())
                return NotFound("No banks found.");

            return Ok(banks);
        }


    }
}
