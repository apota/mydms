using System;
using System.Collections.Generic;
using DMS.CRM.Core.Models;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DMS.CRM.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Campaigns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Budget = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TargetAudience = table.Column<TargetAudience>(type: "jsonb", nullable: false),
                    Content = table.Column<CampaignContent>(type: "jsonb", nullable: false),
                    Metrics = table.Column<CampaignMetrics>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campaigns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContactType = table.Column<int>(type: "integer", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BusinessName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    PhoneNumbers = table.Column<List<PhoneNumber>>(type: "jsonb", nullable: false),
                    Addresses = table.Column<List<Address>>(type: "jsonb", nullable: false),
                    CommunicationPreferences = table.Column<CommunicationPreferences>(type: "jsonb", nullable: false),
                    DemographicInfo = table.Column<DemographicInfo>(type: "jsonb", nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceType = table.Column<int>(type: "integer", nullable: false),
                    LeadScore = table.Column<int>(type: "integer", nullable: false),
                    LoyaltyTier = table.Column<int>(type: "integer", nullable: false),
                    LoyaltyPoints = table.Column<int>(type: "integer", nullable: false),
                    LifetimeValue = table.Column<decimal>(type: "numeric", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Tags = table.Column<List<string>>(type: "jsonb", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomerSegments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Criteria = table.Column<string>(type: "jsonb", nullable: true),
                    MemberCount = table.Column<int>(type: "integer", nullable: false),
                    LastRefreshed = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Tags = table.Column<List<string>>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerSegments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LoyaltyRewards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    PointsCost = table.Column<int>(type: "integer", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    RewardType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MonetaryValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    EligibleTiers = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    QuantityAvailable = table.Column<int>(type: "integer", nullable: true),
                    QuantityRedeemed = table.Column<int>(type: "integer", nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Terms = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ExpirationDays = table.Column<int>(type: "integer", nullable: false),
                    RequiresApproval = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyaltyRewards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LoyaltyTierConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Tier = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MinimumPoints = table.Column<int>(type: "integer", nullable: false),
                    PointsMultiplier = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    Benefits = table.Column<string>(type: "jsonb", nullable: false),
                    Color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyaltyTierConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomerInteractions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Direction = table.Column<int>(type: "integer", nullable: false),
                    ChannelId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    InteractionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Duration = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Subject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Content = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Outcome = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Sentiment = table.Column<int>(type: "integer", nullable: false),
                    Tags = table.Column<List<string>>(type: "jsonb", nullable: false),
                    RelatedToType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RelatedToId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequiresFollowUp = table.Column<bool>(type: "boolean", nullable: false),
                    FollowUpDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FollowUpAssignedToId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    FollowUpNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerInteractions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerInteractions_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomerJourneys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Stage = table.Column<int>(type: "integer", nullable: false),
                    Substage = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CurrentMilestone = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    NextMilestone = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    JourneyStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EstimatedCompletionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AssignedToId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastActivityDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextScheduledActivity = table.Column<ScheduledActivity>(type: "jsonb", nullable: true),
                    JourneyScore = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerJourneys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerJourneys_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomerLoyalties",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentTier = table.Column<int>(type: "integer", nullable: false),
                    CurrentPoints = table.Column<int>(type: "integer", nullable: false),
                    LifetimePointsEarned = table.Column<int>(type: "integer", nullable: false),
                    LifetimePointsRedeemed = table.Column<int>(type: "integer", nullable: false),
                    EnrollmentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TierAchievedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TierExpirationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastActivityDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerLoyalties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerLoyalties_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomerSurveys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    SurveyType = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    RelatedToType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RelatedToId = table.Column<Guid>(type: "uuid", nullable: true),
                    SentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OverallScore = table.Column<int>(type: "integer", nullable: true),
                    Comments = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    FollowUpRequired = table.Column<bool>(type: "boolean", nullable: false),
                    FollowUpAssignedToId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FollowUpStatus = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerSurveys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerSurveys_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomerVehicles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    RelationshipType = table.Column<int>(type: "integer", nullable: false),
                    PurchaseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PurchaseLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    PurchaseType = table.Column<int>(type: "integer", nullable: false),
                    FinanceType = table.Column<int>(type: "integer", nullable: false),
                    FinanceCompany = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    EstimatedPayoffDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsCurrentVehicle = table.Column<bool>(type: "boolean", nullable: false),
                    IsServicedHere = table.Column<bool>(type: "boolean", nullable: false),
                    PrimaryDriver = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AnnualMileage = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerVehicles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerVehicles_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CampaignSegments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    SegmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignSegments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignSegments_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignSegments_CustomerSegments_SegmentId",
                        column: x => x.SegmentId,
                        principalTable: "CustomerSegments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SegmentMember",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SegmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    AddedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SegmentMember", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SegmentMember_CustomerSegments_SegmentId",
                        column: x => x.SegmentId,
                        principalTable: "CustomerSegments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SegmentMember_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LoyaltyRedemptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerLoyaltyId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    RewardId = table.Column<Guid>(type: "uuid", nullable: false),
                    PointsRedeemed = table.Column<int>(type: "integer", nullable: false),
                    RedemptionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RedemptionCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    UsedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ApprovalStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ApprovedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyaltyRedemptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoyaltyRedemptions_CustomerLoyalties_CustomerLoyaltyId",
                        column: x => x.CustomerLoyaltyId,
                        principalTable: "CustomerLoyalties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LoyaltyRedemptions_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LoyaltyRedemptions_LoyaltyRewards_RewardId",
                        column: x => x.RewardId,
                        principalTable: "LoyaltyRewards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LoyaltyTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerLoyaltyId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionType = table.Column<int>(type: "integer", nullable: false),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    PointsBalance = table.Column<int>(type: "integer", nullable: false),
                    Source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ReferenceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TransactionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RewardId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProcessedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyaltyTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoyaltyTransactions_CustomerLoyalties_CustomerLoyaltyId",
                        column: x => x.CustomerLoyaltyId,
                        principalTable: "CustomerLoyalties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LoyaltyTransactions_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LoyaltyTransactions_LoyaltyRewards_RewardId",
                        column: x => x.RewardId,
                        principalTable: "LoyaltyRewards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CustomerSurveyResponses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SurveyId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Question = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ResponseType = table.Column<int>(type: "integer", nullable: false),
                    Response = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Score = table.Column<int>(type: "integer", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerSurveyResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerSurveyResponses_CustomerSurveys_SurveyId",
                        column: x => x.SurveyId,
                        principalTable: "CustomerSurveys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerSurveyResponses_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_Name",
                table: "Campaigns",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_StartDate",
                table: "Campaigns",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_Status",
                table: "Campaigns",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_Type",
                table: "Campaigns",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignSegments_CampaignId",
                table: "CampaignSegments",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignSegments_CampaignId_SegmentId",
                table: "CampaignSegments",
                columns: new[] { "CampaignId", "SegmentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CampaignSegments_SegmentId",
                table: "CampaignSegments",
                column: "SegmentId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerInteractions_CustomerId",
                table: "CustomerInteractions",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerInteractions_Direction",
                table: "CustomerInteractions",
                column: "Direction");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerInteractions_FollowUpDate",
                table: "CustomerInteractions",
                column: "FollowUpDate");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerInteractions_InteractionDate",
                table: "CustomerInteractions",
                column: "InteractionDate");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerInteractions_RequiresFollowUp",
                table: "CustomerInteractions",
                column: "RequiresFollowUp");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerInteractions_Status",
                table: "CustomerInteractions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerInteractions_Type",
                table: "CustomerInteractions",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerJourneys_CustomerId",
                table: "CustomerJourneys",
                column: "CustomerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerJourneys_JourneyStartDate",
                table: "CustomerJourneys",
                column: "JourneyStartDate");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerJourneys_Stage",
                table: "CustomerJourneys",
                column: "Stage");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerLoyalties_CurrentTier",
                table: "CustomerLoyalties",
                column: "CurrentTier");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerLoyalties_CustomerId",
                table: "CustomerLoyalties",
                column: "CustomerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerLoyalties_EnrollmentDate",
                table: "CustomerLoyalties",
                column: "EnrollmentDate");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerLoyalties_IsActive",
                table: "CustomerLoyalties",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_BusinessName",
                table: "Customers",
                column: "BusinessName");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_ContactType",
                table: "Customers",
                column: "ContactType");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Email",
                table: "Customers",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Status",
                table: "Customers",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerSegments_Name",
                table: "CustomerSegments",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerSegments_Type",
                table: "CustomerSegments",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerSurveyResponses_CustomerId",
                table: "CustomerSurveyResponses",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerSurveyResponses_QuestionId",
                table: "CustomerSurveyResponses",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerSurveyResponses_SubmittedAt",
                table: "CustomerSurveyResponses",
                column: "SubmittedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerSurveyResponses_SurveyId",
                table: "CustomerSurveyResponses",
                column: "SurveyId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerSurveys_CompletedDate",
                table: "CustomerSurveys",
                column: "CompletedDate");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerSurveys_CustomerId",
                table: "CustomerSurveys",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerSurveys_SentDate",
                table: "CustomerSurveys",
                column: "SentDate");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerSurveys_SurveyType",
                table: "CustomerSurveys",
                column: "SurveyType");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerVehicles_CustomerId",
                table: "CustomerVehicles",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerVehicles_PurchaseDate",
                table: "CustomerVehicles",
                column: "PurchaseDate");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerVehicles_Status",
                table: "CustomerVehicles",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerVehicles_VehicleId",
                table: "CustomerVehicles",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyRedemptions_CustomerId",
                table: "LoyaltyRedemptions",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyRedemptions_CustomerLoyaltyId",
                table: "LoyaltyRedemptions",
                column: "CustomerLoyaltyId");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyRedemptions_ExpirationDate",
                table: "LoyaltyRedemptions",
                column: "ExpirationDate");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyRedemptions_RedemptionCode",
                table: "LoyaltyRedemptions",
                column: "RedemptionCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyRedemptions_RedemptionDate",
                table: "LoyaltyRedemptions",
                column: "RedemptionDate");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyRedemptions_RewardId",
                table: "LoyaltyRedemptions",
                column: "RewardId");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyRedemptions_Status",
                table: "LoyaltyRedemptions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyRewards_Category",
                table: "LoyaltyRewards",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyRewards_DisplayOrder",
                table: "LoyaltyRewards",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyRewards_EndDate",
                table: "LoyaltyRewards",
                column: "EndDate");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyRewards_PointsCost",
                table: "LoyaltyRewards",
                column: "PointsCost");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyRewards_StartDate",
                table: "LoyaltyRewards",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyRewards_Status",
                table: "LoyaltyRewards",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyTierConfigs_DisplayOrder",
                table: "LoyaltyTierConfigs",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyTierConfigs_IsActive",
                table: "LoyaltyTierConfigs",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyTierConfigs_MinimumPoints",
                table: "LoyaltyTierConfigs",
                column: "MinimumPoints");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyTierConfigs_Tier",
                table: "LoyaltyTierConfigs",
                column: "Tier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyTransactions_CustomerId",
                table: "LoyaltyTransactions",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyTransactions_CustomerLoyaltyId",
                table: "LoyaltyTransactions",
                column: "CustomerLoyaltyId");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyTransactions_ExpirationDate",
                table: "LoyaltyTransactions",
                column: "ExpirationDate");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyTransactions_RewardId",
                table: "LoyaltyTransactions",
                column: "RewardId");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyTransactions_TransactionDate",
                table: "LoyaltyTransactions",
                column: "TransactionDate");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyTransactions_TransactionType",
                table: "LoyaltyTransactions",
                column: "TransactionType");

            migrationBuilder.CreateIndex(
                name: "IX_SegmentMember_CustomerId",
                table: "SegmentMember",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_SegmentMember_SegmentId_CustomerId",
                table: "SegmentMember",
                columns: new[] { "SegmentId", "CustomerId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CampaignSegments");

            migrationBuilder.DropTable(
                name: "CustomerInteractions");

            migrationBuilder.DropTable(
                name: "CustomerJourneys");

            migrationBuilder.DropTable(
                name: "CustomerSurveyResponses");

            migrationBuilder.DropTable(
                name: "CustomerVehicles");

            migrationBuilder.DropTable(
                name: "LoyaltyRedemptions");

            migrationBuilder.DropTable(
                name: "LoyaltyTierConfigs");

            migrationBuilder.DropTable(
                name: "LoyaltyTransactions");

            migrationBuilder.DropTable(
                name: "SegmentMember");

            migrationBuilder.DropTable(
                name: "Campaigns");

            migrationBuilder.DropTable(
                name: "CustomerSurveys");

            migrationBuilder.DropTable(
                name: "CustomerLoyalties");

            migrationBuilder.DropTable(
                name: "LoyaltyRewards");

            migrationBuilder.DropTable(
                name: "CustomerSegments");

            migrationBuilder.DropTable(
                name: "Customers");
        }
    }
}
