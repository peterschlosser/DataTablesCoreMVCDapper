using System.ComponentModel.DataAnnotations.Schema;

namespace DataTablesCoreMVCDapper.Models
{
    [Table("Customers", Schema = "dbo")]
    public class Customer
    {
        public static string Table => "[dbo].[Customers]";

        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CompanyName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string County { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
        public string Email { get; set; }
        public string Web { get; set; }
    }
}
