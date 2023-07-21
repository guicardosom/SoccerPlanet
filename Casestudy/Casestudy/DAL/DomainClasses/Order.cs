using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Casestudy.DAL.DomainClasses
{
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public DateTime OrderDate { get; set; }

        [Column(TypeName = "money")]
        public decimal OrderAmount { get; set; }

        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }
        [Required]
        public int CustomerId { get; set; } // needs to be a FK
    }
}
