using System.Data.SqlTypes;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyMvcApp.Models.DataModel;

public partial class Resources
{
    [Key]
    public int ResourcesId { get; set; }

    [StringLength(150)]
    public string? Name { get; set; } = null!;

    [Url]
    [StringLength(2048)]
    public string? Url { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }
    
    [DefaultValue(true)]
    public bool? IsActive { get; set; }
    [NotMapped]
    public List<Accounts> Accounts { get; set; } = new List<Accounts>();
}
