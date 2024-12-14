using Lab2.DataAccess;

namespace LB4_WEBAPI.DTO
{
    public class BankDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Specialization { get; set; }

        public List<AccountsDto> Account { get; set; }

    }
}
