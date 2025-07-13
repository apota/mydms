namespace DMS.CRM.Core.Models
{
    public enum CommunicationChannel
    {
        Email,
        Phone,
        SMS,
        Chat,
        Web,
        Social,
        InPerson,
        Mail
    }

    public enum InteractionStatus
    {
        Pending,
        InProgress,
        Completed,
        Cancelled,
        Failed,
        OnHold
    }

    public enum JourneyStage
    {
        Awareness,
        Consideration,
        Purchase,
        Ownership,
        Service,
        Repurchase
    }

    public enum CampaignType
    {
        Email,
        SMS,
        DirectMail,
        Social,
        MultiChannel
    }

    public enum CampaignStatus
    {
        Draft,
        Scheduled,
        Running,
        Completed,
        Cancelled,
        Inactive
    }

    public enum QuestionType
    {
        Text,
        MultipleChoice,
        SingleChoice,
        Rating,
        YesNo,
        Scale,
        Date,
        Number,
        Checkbox
    }

    public enum SurveyStatus
    {
        Draft,
        Active,
        Paused,
        Completed,
        Cancelled,
        Closed
    }

    public enum CustomerStatus
    {
        Active,
        Inactive,
        Prospect,
        Suspended,
        Pending,
        Blocked
    }

    public enum LoyaltyTier
    {
        Bronze,
        Silver,
        Gold,
        Platinum,
        Diamond
    }

    public enum SegmentType
    {
        Dynamic,
        Static
    }

    public enum ResponseCompletionStatus
    {
        NotStarted,
        InProgress,
        Completed,
        Abandoned
    }

    public enum SurveyQuestionType
    {
        Text,
        MultipleChoice,
        SingleChoice,
        Rating,
        YesNo,
        Scale,
        Date,
        Number,
        Checkbox
    }
}
