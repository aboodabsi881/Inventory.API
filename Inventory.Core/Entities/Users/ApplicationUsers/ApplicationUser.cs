using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Text;

namespace Inventory.Core.Entities.Users.ApplicationUsers
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string NameAr { get; set; }
        public string NameEn { get; set; }

        public Byte[] PasswordByte { get; set; }


        [NotMapped]
        public string DisplayName
        {
            get
            {
                var isArabic = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft;
                return isArabic ? NameAr : NameEn ?? string.Empty;
            }
        }

        public string? Img { get; set; }

        public virtual ICollection<IdentityUserRole<int>> UserRoles { get; set; } = new List<IdentityUserRole<int>>();
    }
}
