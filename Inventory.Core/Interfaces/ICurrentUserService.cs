using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Core.Interfaces
{
    public interface ICurrentUserService
    {
        int? UserId { get; }
        string? UserName { get; }
    }
}
