using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartySchoolApi.Migrations
{
    /// <inheritdoc />
    public partial class ExtendedFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_content_tags_learning_contents_content_id",
                table: "content_tags");

            migrationBuilder.DropForeignKey(
                name: "FK_exam_tests_exam_papers_paper_id",
                table: "exam_tests");

            migrationBuilder.DropForeignKey(
                name: "FK_exam_tests_organizations_target_org_id",
                table: "exam_tests");

            migrationBuilder.DropForeignKey(
                name: "FK_exam_tests_party_members_publisher_id",
                table: "exam_tests");

            migrationBuilder.DropForeignKey(
                name: "FK_learning_contents_content_categories_category_id",
                table: "learning_contents");

            migrationBuilder.DropForeignKey(
                name: "FK_member_learning_progress_learning_contents_content_id",
                table: "member_learning_progress");

            migrationBuilder.DropForeignKey(
                name: "FK_member_learning_progress_party_members_member_id",
                table: "member_learning_progress");

            migrationBuilder.DropForeignKey(
                name: "FK_member_test_records_exam_tests_test_id",
                table: "member_test_records");

            migrationBuilder.DropForeignKey(
                name: "FK_member_test_records_party_members_member_id",
                table: "member_test_records");

            migrationBuilder.DropForeignKey(
                name: "FK_party_members_organizations_organization_id",
                table: "party_members");

            migrationBuilder.DropForeignKey(
                name: "FK_task_contents_learning_contents_content_id",
                table: "task_contents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_party_members",
                table: "party_members");

            migrationBuilder.DropPrimaryKey(
                name: "PK_learning_contents",
                table: "learning_contents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_exam_tests",
                table: "exam_tests");

            migrationBuilder.RenameTable(
                name: "party_members",
                newName: "PartyMembers");

            migrationBuilder.RenameTable(
                name: "learning_contents",
                newName: "LearningContents");

            migrationBuilder.RenameTable(
                name: "exam_tests",
                newName: "ExamTests");

            migrationBuilder.RenameColumn(
                name: "role",
                table: "PartyMembers",
                newName: "Role");

            migrationBuilder.RenameColumn(
                name: "phone",
                table: "PartyMembers",
                newName: "Phone");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "PartyMembers",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "PartyMembers",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "refresh_token_expiry",
                table: "PartyMembers",
                newName: "RefreshTokenExpiry");

            migrationBuilder.RenameColumn(
                name: "refresh_token",
                table: "PartyMembers",
                newName: "RefreshToken");

            migrationBuilder.RenameColumn(
                name: "password_hash",
                table: "PartyMembers",
                newName: "PasswordHash");

            migrationBuilder.RenameColumn(
                name: "organization_id",
                table: "PartyMembers",
                newName: "OrganizationId");

            migrationBuilder.RenameColumn(
                name: "is_enabled",
                table: "PartyMembers",
                newName: "IsEnabled");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "PartyMembers",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_party_members_role",
                table: "PartyMembers",
                newName: "IX_PartyMembers_Role");

            migrationBuilder.RenameIndex(
                name: "IX_party_members_phone",
                table: "PartyMembers",
                newName: "IX_PartyMembers_Phone");

            migrationBuilder.RenameIndex(
                name: "IX_party_members_organization_id",
                table: "PartyMembers",
                newName: "IX_PartyMembers_OrganizationId");

            migrationBuilder.RenameColumn(
                name: "title",
                table: "LearningContents",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "body",
                table: "LearningContents",
                newName: "Body");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "LearningContents",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "video_url",
                table: "LearningContents",
                newName: "VideoUrl");

            migrationBuilder.RenameColumn(
                name: "is_public",
                table: "LearningContents",
                newName: "IsPublic");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "LearningContents",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "content_type",
                table: "LearningContents",
                newName: "ContentType");

            migrationBuilder.RenameColumn(
                name: "category_id",
                table: "LearningContents",
                newName: "CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_learning_contents_title",
                table: "LearningContents",
                newName: "IX_LearningContents_Title");

            migrationBuilder.RenameIndex(
                name: "IX_learning_contents_is_public",
                table: "LearningContents",
                newName: "IX_LearningContents_IsPublic");

            migrationBuilder.RenameIndex(
                name: "IX_learning_contents_content_type",
                table: "LearningContents",
                newName: "IX_LearningContents_ContentType");

            migrationBuilder.RenameIndex(
                name: "IX_learning_contents_category_id",
                table: "LearningContents",
                newName: "IX_LearningContents_CategoryId");

            migrationBuilder.RenameColumn(
                name: "deadline",
                table: "ExamTests",
                newName: "Deadline");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "ExamTests",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "time_limit_minutes",
                table: "ExamTests",
                newName: "TimeLimitMinutes");

            migrationBuilder.RenameColumn(
                name: "target_org_id",
                table: "ExamTests",
                newName: "TargetOrgId");

            migrationBuilder.RenameColumn(
                name: "publisher_id",
                table: "ExamTests",
                newName: "PublisherId");

            migrationBuilder.RenameColumn(
                name: "paper_id",
                table: "ExamTests",
                newName: "PaperId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "ExamTests",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_exam_tests_target_org_id",
                table: "ExamTests",
                newName: "IX_ExamTests_TargetOrgId");

            migrationBuilder.RenameIndex(
                name: "IX_exam_tests_publisher_id",
                table: "ExamTests",
                newName: "IX_ExamTests_PublisherId");

            migrationBuilder.RenameIndex(
                name: "IX_exam_tests_paper_id",
                table: "ExamTests",
                newName: "IX_ExamTests_PaperId");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "PartyMembers",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "PointTotal",
                table: "PartyMembers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Body",
                table: "LearningContents",
                type: "varchar(8000)",
                maxLength: 8000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "RelatedDocumentUrl",
                table: "LearningContents",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "SourceType",
                table: "LearningContents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsAiGenerated",
                table: "ExamTests",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TargetWeaknessTags",
                table: "ExamTests",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PartyMembers",
                table: "PartyMembers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LearningContents",
                table: "LearningContents",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExamTests",
                table: "ExamTests",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "BattleRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ChallengerId = table.Column<int>(type: "int", nullable: false),
                    OpponentId = table.Column<int>(type: "int", nullable: false),
                    ResultJson = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BattleTime = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BattleRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BattleRecords_PartyMembers_ChallengerId",
                        column: x => x.ChallengerId,
                        principalTable: "PartyMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BattleRecords_PartyMembers_OpponentId",
                        column: x => x.OpponentId,
                        principalTable: "PartyMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CheckInRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PartyMemberId = table.Column<int>(type: "int", nullable: false),
                    LocationName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CheckInTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Note = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AiBackgroundInterpretation = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PointsEarned = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckInRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CheckInRecords_PartyMembers_PartyMemberId",
                        column: x => x.PartyMemberId,
                        principalTable: "PartyMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LearningPoints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PartyMemberId = table.Column<int>(type: "int", nullable: false),
                    SourceType = table.Column<int>(type: "int", nullable: false),
                    SourceId = table.Column<int>(type: "int", nullable: true),
                    Points = table.Column<int>(type: "int", nullable: false),
                    EarnedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningPoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LearningPoints_PartyMembers_PartyMemberId",
                        column: x => x.PartyMemberId,
                        principalTable: "PartyMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MeetingActivities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ActivityTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsAiSummaryGenerated = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AiSummaryContent = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetingActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeetingActivities_organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MemberLearningReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PartyMemberId = table.Column<int>(type: "int", nullable: false),
                    ReportJson = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberLearningReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MemberLearningReports_PartyMembers_PartyMemberId",
                        column: x => x.PartyMemberId,
                        principalTable: "PartyMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MessageNotifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PartyMemberId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Content = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsRead = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessageNotifications_PartyMembers_PartyMemberId",
                        column: x => x.PartyMemberId,
                        principalTable: "PartyMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OrganizationQuarterlyReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    Quarter = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReportJson = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationQuarterlyReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationQuarterlyReports_organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PairHelpRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    HelperId = table.Column<int>(type: "int", nullable: false),
                    HelpReceiverId = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    HelpContentJson = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OutcomeSummary = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PairHelpRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PairHelpRecords_PartyMembers_HelpReceiverId",
                        column: x => x.HelpReceiverId,
                        principalTable: "PartyMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PairHelpRecords_PartyMembers_HelperId",
                        column: x => x.HelperId,
                        principalTable: "PartyMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PartyDevelopmentProcesses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PartyMemberId = table.Column<int>(type: "int", nullable: false),
                    Stage = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    MaterialsJson = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReportContent = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SubmittedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ReviewComment = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReviewedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsReminderSent = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartyDevelopmentProcesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartyDevelopmentProcesses_PartyMembers_PartyMemberId",
                        column: x => x.PartyMemberId,
                        principalTable: "PartyMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ActivityHearts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MeetingActivityId = table.Column<int>(type: "int", nullable: false),
                    PartyMemberId = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SubmittedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    AiPolishSuggestion = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityHearts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityHearts_MeetingActivities_MeetingActivityId",
                        column: x => x.MeetingActivityId,
                        principalTable: "MeetingActivities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActivityHearts_PartyMembers_PartyMemberId",
                        column: x => x.PartyMemberId,
                        principalTable: "PartyMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityHearts_MeetingActivityId",
                table: "ActivityHearts",
                column: "MeetingActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityHearts_PartyMemberId",
                table: "ActivityHearts",
                column: "PartyMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_BattleRecords_ChallengerId",
                table: "BattleRecords",
                column: "ChallengerId");

            migrationBuilder.CreateIndex(
                name: "IX_BattleRecords_OpponentId",
                table: "BattleRecords",
                column: "OpponentId");

            migrationBuilder.CreateIndex(
                name: "IX_CheckInRecords_LocationName",
                table: "CheckInRecords",
                column: "LocationName");

            migrationBuilder.CreateIndex(
                name: "IX_CheckInRecords_PartyMemberId",
                table: "CheckInRecords",
                column: "PartyMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningPoints_PartyMemberId",
                table: "LearningPoints",
                column: "PartyMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningPoints_SourceType",
                table: "LearningPoints",
                column: "SourceType");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingActivities_ActivityTime",
                table: "MeetingActivities",
                column: "ActivityTime");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingActivities_OrganizationId",
                table: "MeetingActivities",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingActivities_Type",
                table: "MeetingActivities",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_MemberLearningReports_PartyMemberId",
                table: "MemberLearningReports",
                column: "PartyMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageNotifications_IsRead",
                table: "MessageNotifications",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "IX_MessageNotifications_PartyMemberId",
                table: "MessageNotifications",
                column: "PartyMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationQuarterlyReports_OrganizationId_Quarter",
                table: "OrganizationQuarterlyReports",
                columns: new[] { "OrganizationId", "Quarter" });

            migrationBuilder.CreateIndex(
                name: "IX_PairHelpRecords_HelperId",
                table: "PairHelpRecords",
                column: "HelperId");

            migrationBuilder.CreateIndex(
                name: "IX_PairHelpRecords_HelpReceiverId",
                table: "PairHelpRecords",
                column: "HelpReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_PartyDevelopmentProcesses_PartyMemberId",
                table: "PartyDevelopmentProcesses",
                column: "PartyMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_PartyDevelopmentProcesses_Stage",
                table: "PartyDevelopmentProcesses",
                column: "Stage");

            migrationBuilder.CreateIndex(
                name: "IX_PartyDevelopmentProcesses_Status",
                table: "PartyDevelopmentProcesses",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_content_tags_LearningContents_content_id",
                table: "content_tags",
                column: "content_id",
                principalTable: "LearningContents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamTests_PartyMembers_PublisherId",
                table: "ExamTests",
                column: "PublisherId",
                principalTable: "PartyMembers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamTests_exam_papers_PaperId",
                table: "ExamTests",
                column: "PaperId",
                principalTable: "exam_papers",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamTests_organizations_TargetOrgId",
                table: "ExamTests",
                column: "TargetOrgId",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LearningContents_content_categories_CategoryId",
                table: "LearningContents",
                column: "CategoryId",
                principalTable: "content_categories",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_member_learning_progress_LearningContents_content_id",
                table: "member_learning_progress",
                column: "content_id",
                principalTable: "LearningContents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_member_learning_progress_PartyMembers_member_id",
                table: "member_learning_progress",
                column: "member_id",
                principalTable: "PartyMembers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_member_test_records_ExamTests_test_id",
                table: "member_test_records",
                column: "test_id",
                principalTable: "ExamTests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_member_test_records_PartyMembers_member_id",
                table: "member_test_records",
                column: "member_id",
                principalTable: "PartyMembers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PartyMembers_organizations_OrganizationId",
                table: "PartyMembers",
                column: "OrganizationId",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_task_contents_LearningContents_content_id",
                table: "task_contents",
                column: "content_id",
                principalTable: "LearningContents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_content_tags_LearningContents_content_id",
                table: "content_tags");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamTests_PartyMembers_PublisherId",
                table: "ExamTests");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamTests_exam_papers_PaperId",
                table: "ExamTests");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamTests_organizations_TargetOrgId",
                table: "ExamTests");

            migrationBuilder.DropForeignKey(
                name: "FK_LearningContents_content_categories_CategoryId",
                table: "LearningContents");

            migrationBuilder.DropForeignKey(
                name: "FK_member_learning_progress_LearningContents_content_id",
                table: "member_learning_progress");

            migrationBuilder.DropForeignKey(
                name: "FK_member_learning_progress_PartyMembers_member_id",
                table: "member_learning_progress");

            migrationBuilder.DropForeignKey(
                name: "FK_member_test_records_ExamTests_test_id",
                table: "member_test_records");

            migrationBuilder.DropForeignKey(
                name: "FK_member_test_records_PartyMembers_member_id",
                table: "member_test_records");

            migrationBuilder.DropForeignKey(
                name: "FK_PartyMembers_organizations_OrganizationId",
                table: "PartyMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_task_contents_LearningContents_content_id",
                table: "task_contents");

            migrationBuilder.DropTable(
                name: "ActivityHearts");

            migrationBuilder.DropTable(
                name: "BattleRecords");

            migrationBuilder.DropTable(
                name: "CheckInRecords");

            migrationBuilder.DropTable(
                name: "LearningPoints");

            migrationBuilder.DropTable(
                name: "MemberLearningReports");

            migrationBuilder.DropTable(
                name: "MessageNotifications");

            migrationBuilder.DropTable(
                name: "OrganizationQuarterlyReports");

            migrationBuilder.DropTable(
                name: "PairHelpRecords");

            migrationBuilder.DropTable(
                name: "PartyDevelopmentProcesses");

            migrationBuilder.DropTable(
                name: "MeetingActivities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PartyMembers",
                table: "PartyMembers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LearningContents",
                table: "LearningContents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExamTests",
                table: "ExamTests");

            migrationBuilder.DropColumn(
                name: "PointTotal",
                table: "PartyMembers");

            migrationBuilder.DropColumn(
                name: "RelatedDocumentUrl",
                table: "LearningContents");

            migrationBuilder.DropColumn(
                name: "SourceType",
                table: "LearningContents");

            migrationBuilder.DropColumn(
                name: "IsAiGenerated",
                table: "ExamTests");

            migrationBuilder.DropColumn(
                name: "TargetWeaknessTags",
                table: "ExamTests");

            migrationBuilder.RenameTable(
                name: "PartyMembers",
                newName: "party_members");

            migrationBuilder.RenameTable(
                name: "LearningContents",
                newName: "learning_contents");

            migrationBuilder.RenameTable(
                name: "ExamTests",
                newName: "exam_tests");

            migrationBuilder.RenameColumn(
                name: "Role",
                table: "party_members",
                newName: "role");

            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "party_members",
                newName: "phone");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "party_members",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "party_members",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "RefreshTokenExpiry",
                table: "party_members",
                newName: "refresh_token_expiry");

            migrationBuilder.RenameColumn(
                name: "RefreshToken",
                table: "party_members",
                newName: "refresh_token");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "party_members",
                newName: "password_hash");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "party_members",
                newName: "organization_id");

            migrationBuilder.RenameColumn(
                name: "IsEnabled",
                table: "party_members",
                newName: "is_enabled");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "party_members",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_PartyMembers_Role",
                table: "party_members",
                newName: "IX_party_members_role");

            migrationBuilder.RenameIndex(
                name: "IX_PartyMembers_Phone",
                table: "party_members",
                newName: "IX_party_members_phone");

            migrationBuilder.RenameIndex(
                name: "IX_PartyMembers_OrganizationId",
                table: "party_members",
                newName: "IX_party_members_organization_id");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "learning_contents",
                newName: "title");

            migrationBuilder.RenameColumn(
                name: "Body",
                table: "learning_contents",
                newName: "body");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "learning_contents",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "VideoUrl",
                table: "learning_contents",
                newName: "video_url");

            migrationBuilder.RenameColumn(
                name: "IsPublic",
                table: "learning_contents",
                newName: "is_public");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "learning_contents",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "ContentType",
                table: "learning_contents",
                newName: "content_type");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "learning_contents",
                newName: "category_id");

            migrationBuilder.RenameIndex(
                name: "IX_LearningContents_Title",
                table: "learning_contents",
                newName: "IX_learning_contents_title");

            migrationBuilder.RenameIndex(
                name: "IX_LearningContents_IsPublic",
                table: "learning_contents",
                newName: "IX_learning_contents_is_public");

            migrationBuilder.RenameIndex(
                name: "IX_LearningContents_ContentType",
                table: "learning_contents",
                newName: "IX_learning_contents_content_type");

            migrationBuilder.RenameIndex(
                name: "IX_LearningContents_CategoryId",
                table: "learning_contents",
                newName: "IX_learning_contents_category_id");

            migrationBuilder.RenameColumn(
                name: "Deadline",
                table: "exam_tests",
                newName: "deadline");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "exam_tests",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "TimeLimitMinutes",
                table: "exam_tests",
                newName: "time_limit_minutes");

            migrationBuilder.RenameColumn(
                name: "TargetOrgId",
                table: "exam_tests",
                newName: "target_org_id");

            migrationBuilder.RenameColumn(
                name: "PublisherId",
                table: "exam_tests",
                newName: "publisher_id");

            migrationBuilder.RenameColumn(
                name: "PaperId",
                table: "exam_tests",
                newName: "paper_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "exam_tests",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_ExamTests_TargetOrgId",
                table: "exam_tests",
                newName: "IX_exam_tests_target_org_id");

            migrationBuilder.RenameIndex(
                name: "IX_ExamTests_PublisherId",
                table: "exam_tests",
                newName: "IX_exam_tests_publisher_id");

            migrationBuilder.RenameIndex(
                name: "IX_ExamTests_PaperId",
                table: "exam_tests",
                newName: "IX_exam_tests_paper_id");

            migrationBuilder.AlterColumn<string>(
                name: "password_hash",
                table: "party_members",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldMaxLength: 200)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "body",
                table: "learning_contents",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(8000)",
                oldMaxLength: 8000,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_party_members",
                table: "party_members",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_learning_contents",
                table: "learning_contents",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_exam_tests",
                table: "exam_tests",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_content_tags_learning_contents_content_id",
                table: "content_tags",
                column: "content_id",
                principalTable: "learning_contents",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_exam_tests_exam_papers_paper_id",
                table: "exam_tests",
                column: "paper_id",
                principalTable: "exam_papers",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_exam_tests_organizations_target_org_id",
                table: "exam_tests",
                column: "target_org_id",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_exam_tests_party_members_publisher_id",
                table: "exam_tests",
                column: "publisher_id",
                principalTable: "party_members",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_learning_contents_content_categories_category_id",
                table: "learning_contents",
                column: "category_id",
                principalTable: "content_categories",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_member_learning_progress_learning_contents_content_id",
                table: "member_learning_progress",
                column: "content_id",
                principalTable: "learning_contents",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_member_learning_progress_party_members_member_id",
                table: "member_learning_progress",
                column: "member_id",
                principalTable: "party_members",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_member_test_records_exam_tests_test_id",
                table: "member_test_records",
                column: "test_id",
                principalTable: "exam_tests",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_member_test_records_party_members_member_id",
                table: "member_test_records",
                column: "member_id",
                principalTable: "party_members",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_party_members_organizations_organization_id",
                table: "party_members",
                column: "organization_id",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_task_contents_learning_contents_content_id",
                table: "task_contents",
                column: "content_id",
                principalTable: "learning_contents",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
