using System.Data.SqlTypes;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace MyMvcApp.Models;

public partial class Resources
{
    public int? res_id { get; set; }

    [StringLength(150)]
    public string? res_name { get; set; } = null!;

    [Url]
    [StringLength(2048)]
    public string? url { get; set; }

    [StringLength(500)]
    public string? description { get; set; }
    
    [DefaultValue(true)]
    public bool? IsActive { get; set; }
}
