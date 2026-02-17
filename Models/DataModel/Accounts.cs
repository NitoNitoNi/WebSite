using System.Data.SqlTypes;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyMvcApp.Models.DataModel;
public partial class Accounts
{
    public int? acc_id { get; set; }

    [StringLength(20)]
    public string login { get; set; } = null!;

    [StringLength(20)]
    public string pass { get; set; } = null!;

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime data { get; set; }
    
    [DefaultValue(true)]
    public bool? IsActive { get; set; }

    public List<Resources> resources { get; set; } = null!;
    public List<Employee> employees { get; set; } = null!;
}
