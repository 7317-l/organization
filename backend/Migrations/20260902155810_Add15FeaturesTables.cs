using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartySchoolApi.Migrations
{
    /// <inheritdoc />
    public partial class Add15FeaturesTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SiteId",
                table: "CheckInRecords",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "anticheat_records",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    party_member_id = table.Column<int>(type: "int", nullable: false),
                    content_id = table.Column<int>(type: "int", nullable: true),
                    question_id = table.Column<int>(type: "int", nullable: true),
                    challenge_id = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_pass = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    verified_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_anticheat_records", x => x.id);
                    table.ForeignKey(
                        name: "FK_anticheat_records_PartyMembers_party_member_id",
                        column: x => x.party_member_id,
                        principalTable: "PartyMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "battle_games",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    challenger_id = table.Column<int>(type: "int", nullable: false),
                    opponent_id = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    question_ids = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    challenger_score = table.Column<int>(type: "int", nullable: false),
                    opponent_score = table.Column<int>(type: "int", nullable: false),
                    current_question_index = table.Column<int>(type: "int", nullable: false),
                    timeout_minutes = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    started_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    finished_at = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_battle_games", x => x.id);
                    table.ForeignKey(
                        name: "FK_battle_games_PartyMembers_challenger_id",
                        column: x => x.challenger_id,
                        principalTable: "PartyMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_battle_games_PartyMembers_opponent_id",
                        column: x => x.opponent_id,
                        principalTable: "PartyMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "education_sites",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    address = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    historical_facts = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ai_interpretation = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cover_url = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    latitude = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    longitude = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_education_sites", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "nl2sql_sessions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    session_id = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    member_id = table.Column<int>(type: "int", nullable: false),
                    question = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    rewritten = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    sql_text = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    explanation = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    result_summary = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nl2sql_sessions", x => x.id);
                    table.ForeignKey(
                        name: "FK_nl2sql_sessions_PartyMembers_member_id",
                        column: x => x.member_id,
                        principalTable: "PartyMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "org_rectifications",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    organization_id = table.Column<int>(type: "int", nullable: false),
                    quarter = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    issue = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    suggestion = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<int>(type: "int", nullable: false),
                    remark = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    completed_at = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_org_rectifications", x => x.id);
                    table.ForeignKey(
                        name: "FK_org_rectifications_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "organization_quarterly_ratings",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    organization_id = table.Column<int>(type: "int", nullable: false),
                    quarter = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    rating = table.Column<string>(type: "varchar(1)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    rating_score = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    detail_json = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organization_quarterly_ratings", x => x.id);
                    table.ForeignKey(
                        name: "FK_organization_quarterly_ratings_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "pair_help_requests",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    helper_id = table.Column<int>(type: "int", nullable: false),
                    help_receiver_id = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    match_reason = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pair_help_requests", x => x.id);
                    table.ForeignKey(
                        name: "FK_pair_help_requests_PartyMembers_help_receiver_id",
                        column: x => x.help_receiver_id,
                        principalTable: "PartyMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pair_help_requests_PartyMembers_helper_id",
                        column: x => x.helper_id,
                        principalTable: "PartyMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "party_development_reminders",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    process_id = table.Column<int>(type: "int", nullable: false),
                    party_member_id = table.Column<int>(type: "int", nullable: false),
                    reminder_type = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    due_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    message = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    sent_at = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_party_development_reminders", x => x.id);
                    table.ForeignKey(
                        name: "FK_party_development_reminders_PartyDevelopmentProcesses_proces~",
                        column: x => x.process_id,
                        principalTable: "PartyDevelopmentProcesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_party_development_reminders_PartyMembers_party_member_id",
                        column: x => x.party_member_id,
                        principalTable: "PartyMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CheckInRecords_SiteId",
                table: "CheckInRecords",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_anticheat_records_party_member_id_verified_at",
                table: "anticheat_records",
                columns: new[] { "party_member_id", "verified_at" });

            migrationBuilder.CreateIndex(
                name: "IX_battle_games_challenger_id",
                table: "battle_games",
                column: "challenger_id");

            migrationBuilder.CreateIndex(
                name: "IX_battle_games_opponent_id",
                table: "battle_games",
                column: "opponent_id");

            migrationBuilder.CreateIndex(
                name: "IX_education_sites_name",
                table: "education_sites",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_nl2sql_sessions_member_id",
                table: "nl2sql_sessions",
                column: "member_id");

            migrationBuilder.CreateIndex(
                name: "IX_nl2sql_sessions_session_id",
                table: "nl2sql_sessions",
                column: "session_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_org_rectifications_organization_id_quarter",
                table: "org_rectifications",
                columns: new[] { "organization_id", "quarter" });

            migrationBuilder.CreateIndex(
                name: "IX_organization_quarterly_ratings_organization_id_quarter",
                table: "organization_quarterly_ratings",
                columns: new[] { "organization_id", "quarter" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pair_help_requests_help_receiver_id",
                table: "pair_help_requests",
                column: "help_receiver_id");

            migrationBuilder.CreateIndex(
                name: "IX_pair_help_requests_helper_id",
                table: "pair_help_requests",
                column: "helper_id");

            migrationBuilder.CreateIndex(
                name: "IX_party_development_reminders_party_member_id",
                table: "party_development_reminders",
                column: "party_member_id");

            migrationBuilder.CreateIndex(
                name: "IX_party_development_reminders_process_id",
                table: "party_development_reminders",
                column: "process_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "anticheat_records");

            migrationBuilder.DropTable(
                name: "battle_games");

            migrationBuilder.DropTable(
                name: "education_sites");

            migrationBuilder.DropTable(
                name: "nl2sql_sessions");

            migrationBuilder.DropTable(
                name: "org_rectifications");

            migrationBuilder.DropTable(
                name: "organization_quarterly_ratings");

            migrationBuilder.DropTable(
                name: "pair_help_requests");

            migrationBuilder.DropTable(
                name: "party_development_reminders");

            migrationBuilder.DropIndex(
                name: "IX_CheckInRecords_SiteId",
                table: "CheckInRecords");

            migrationBuilder.DropColumn(
                name: "SiteId",
                table: "CheckInRecords");
        }
    }
}
