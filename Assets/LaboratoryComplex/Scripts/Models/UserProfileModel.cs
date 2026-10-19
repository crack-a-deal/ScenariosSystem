using GroupModel;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UserAccountModel;

namespace UserProfileModel
{
    public class UserProfile
    {
        [Key]
        public Guid Id { get; set; }

        public Guid UserAccountId { get; set; }

        [ForeignKey(nameof(UserAccountId))]
        public UserAccount UserAccount { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public string? Patronymic { get; set; }

        public Guid UserGroupId { get; set; }

        [ForeignKey(nameof(UserGroupId))]
        public UserGroup UserGroup { get; set; }
    }
}