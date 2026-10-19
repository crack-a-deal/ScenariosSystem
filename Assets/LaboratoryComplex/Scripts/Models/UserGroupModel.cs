using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using UserProfileModel;

namespace GroupModel
{
    public class UserGroup
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        public int Number { get; set; }

        public ICollection<UserProfile> Users { get; set; }
            = new List<UserProfile>();
    }
}