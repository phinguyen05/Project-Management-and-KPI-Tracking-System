using System;
using System.Collections.Generic;

namespace MIS_Project_API.Models;

public partial class Notification
{
    public int NotifId { get; set; }

    public int UserId { get; set; }

    public string Content { get; set; } = null!;

    public bool? IsRead { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
