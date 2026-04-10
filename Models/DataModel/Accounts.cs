using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
namespace MyMvcApp.Models.DataModel;

public partial class Accounts
{
    [Key]
    public int AccountsId { get; set; }

    [StringLength(20)]
    public string Login { get; set; } = null!;

    [StringLength(255)]
    public string Pass { get; set; } = null!;

    public DateTime Data { get; set; } = DateTime.UtcNow;
    
    [DefaultValue(true)]
    public bool? IsActive { get; set; }
    public int ResourceId { get; set; }
    public int EmployeeId { get; set; }
    [NotMapped]
    public string? ResourceName { get; set; }

    [NotMapped]
    public string? ResourceUrl { get; set; }
}
