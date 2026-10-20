using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class ThreeDModel
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; }

    public ICollection<UserModelView> Views { get; set; }
        = new List<UserModelView>();
}