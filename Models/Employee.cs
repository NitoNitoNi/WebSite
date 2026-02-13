using System.Data.SqlTypes;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace MyMvcApp.Models;

public partial class Employee
{
    public int? emp_id { get; set; }

    [StringLength(150)]
    public string? emp_name { get; set; } = null!;

    [StringLength(50)]
    public string position { get; set; } = null!;

    [StringLength(500)]
    public string? description { get; set; }
    
    [DefaultValue(true)]
    public bool? IsActive { get; set; }

    public List<Deparment> deparments { get; set; } = null!;
}
