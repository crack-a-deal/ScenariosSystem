using System;
using System.ComponentModel.DataAnnotations;

namespace UserAccountModel
{
    public class UserAccount
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Login { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public UserProfileModel.UserProfile? Profile { get; set; }
    }
}