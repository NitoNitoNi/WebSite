using System.Data.SqlTypes;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;


namespace MyMvcApp.Models;

public partial class Deparment
{
    public int dep_id { get; set; }

    [StringLength(250)]
    public string dep_name { get; set; }  = null!;

    
}
