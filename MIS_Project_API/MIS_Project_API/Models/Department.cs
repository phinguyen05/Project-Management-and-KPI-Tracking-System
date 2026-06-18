using System;
using System.Collections.Generic;

namespace MIS_Project_API.Models;

public partial class Department
{
    public int DepartmentId { get; set; }

    public string Name { get; set; } = null!;

    public int? ManagerId { get; set; }

    public virtual User? Manager { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
