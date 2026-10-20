using System;

public class UserModelViewDto
{
    public Guid Id { get; }
    public Guid UserProfileId { get; }
    public Guid ThreeDModelId { get; }
    public string ModelName { get; }
    public DateTime ViewedAt { get; }

    public UserModelViewDto(
        Guid id,
        Guid userProfileId,
        Guid threeDModelId,
        string modelName,
        DateTime viewedAt)
    {
        Id = id;
        UserProfileId = userProfileId;
        ThreeDModelId = threeDModelId;
        ModelName = modelName;
        ViewedAt = viewedAt;
    }
}