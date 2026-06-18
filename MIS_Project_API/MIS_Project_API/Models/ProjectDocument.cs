using System;
using System.Collections.Generic;

namespace MIS_Project_API.Models;

public partial class ProjectDocument
{
    public int DocumentId { get; set; }

    public int ProjectId { get; set; }

    public string FileName { get; set; } = null!;

    public string FilePath { get; set; } = null!;

    public string? Type { get; set; }

    public virtual Project Project { get; set; } = null!;
}
