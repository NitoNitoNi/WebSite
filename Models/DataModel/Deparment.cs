using System.Data.SqlTypes;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;


namespace MyMvcApp.Models.DataModel;
public partial class Deparment
{
    [Key]
    public int DeparmentId { get; set; }

    [StringLength(250)]
    public string Name { get; set; }  = null!;

    public List<Employee> Employees { get; set; } = new List<Employee>();

    [NotMapped]
    public string dep_name 
    { 
        get => Name; 
        set => Name = value; 
    }
}
