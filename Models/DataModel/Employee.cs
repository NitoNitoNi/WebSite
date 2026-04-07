using System.Data.SqlTypes;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyMvcApp.Models.DataModel;
public partial class Employee
{
    [Key]
    public int EmployeeId { get; set; }

    [StringLength(150)]
    public string? Name { get; set; } = null!;

    [StringLength(50)]
    public string Position { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; }
    
    [DefaultValue(true)]
    public bool? IsActive { get; set; }

    public int DeparmentId { get; set; }
    [NotMapped]
    public Deparment Deparment { get; set; } = null!;
    [NotMapped]
    public List<Accounts> Accounts { get; set; } = new List<Accounts>();
}
