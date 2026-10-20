using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UserProfileModel;

public class UserModelView
{
    [Key]
    public Guid Id { get; set; }

    public Guid UserProfileId { get; set; }

    [ForeignKey(nameof(UserProfileId))]
    public UserProfile UserProfile { get; set; }

    public Guid ThreeDModelId { get; set; }

    [ForeignKey(nameof(ThreeDModelId))]
    public ThreeDModel ThreeDModel { get; set; }

    public DateTime ViewedAt { get; set; }
}