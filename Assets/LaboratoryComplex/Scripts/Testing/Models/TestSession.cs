using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using UserProfileModel;

public class TestSession
{
    public Guid Id { get; set; }

    public Guid TestId { get; set; }

    public Test Test { get; set; } = null!;

    public Guid UserProfileId { get; set; }

    [ForeignKey(nameof(UserProfileId))]
    public UserProfile UserProfile { get; set; } = null!;

    public DateTime StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public ICollection<UserAnswer> Answers { get; set; }
        = new List<UserAnswer>();
}
