using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartySchoolApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "content_categories",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    parent_id = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_content_categories", x => x.id);
                    table.ForeignKey(
                        name: "FK_content_categories_content_categories_parent_id",
                        column: x => x.parent_id,
                        principalTable: "content_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "exam_papers",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    question_ids = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    total_score = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_papers", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "organizations",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    parent_id = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organizations", x => x.id);
                    table.ForeignKey(
                        name: "FK_organizations_organizations_parent_id",
                        column: x => x.parent_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "question_categories",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_question_categories", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "tags",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tags", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "learning_contents",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    body = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    video_url = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    content_type = table.Column<int>(type: "int", nullable: false),
                    category_id = table.Column<int>(type: "int", nullable: true),
                    is_public = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_learning_contents", x => x.id);
                    table.ForeignKey(
                        name: "FK_learning_contents_content_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "content_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "learning_tasks",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    task_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    target_org_id = table.Column<int>(type: "int", nullable: false),
                    deadline = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_learning_tasks", x => x.id);
                    table.ForeignKey(
                        name: "FK_learning_tasks_organizations_target_org_id",
                        column: x => x.target_org_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "party_members",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    phone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    password_hash = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    role = table.Column<int>(type: "int", nullable: false),
                    organization_id = table.Column<int>(type: "int", nullable: false),
                    is_enabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    refresh_token = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    refresh_token_expiry = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_party_members", x => x.id);
                    table.ForeignKey(
                        name: "FK_party_members_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "questions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    question_type = table.Column<int>(type: "int", nullable: false),
                    stem = table.Column<string>(type: "text", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    options = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    correct_answer = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    score = table.Column<int>(type: "int", nullable: false),
                    category_id = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_questions", x => x.id);
                    table.ForeignKey(
                        name: "FK_questions_question_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "question_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "content_tags",
                columns: table => new
                {
                    content_id = table.Column<int>(type: "int", nullable: false),
                    tag_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_content_tags", x => new { x.content_id, x.tag_id });
                    table.ForeignKey(
                        name: "FK_content_tags_learning_contents_content_id",
                        column: x => x.content_id,
                        principalTable: "learning_contents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_content_tags_tags_tag_id",
                        column: x => x.tag_id,
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "task_contents",
                columns: table => new
                {
                    task_id = table.Column<int>(type: "int", nullable: false),
                    content_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_task_contents", x => new { x.task_id, x.content_id });
                    table.ForeignKey(
                        name: "FK_task_contents_learning_contents_content_id",
                        column: x => x.content_id,
                        principalTable: "learning_contents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_task_contents_learning_tasks_task_id",
                        column: x => x.task_id,
                        principalTable: "learning_tasks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "exam_tests",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    paper_id = table.Column<int>(type: "int", nullable: false),
                    publisher_id = table.Column<int>(type: "int", nullable: false),
                    target_org_id = table.Column<int>(type: "int", nullable: false),
                    time_limit_minutes = table.Column<int>(type: "int", nullable: false),
                    deadline = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_tests", x => x.id);
                    table.ForeignKey(
                        name: "FK_exam_tests_exam_papers_paper_id",
                        column: x => x.paper_id,
                        principalTable: "exam_papers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exam_tests_organizations_target_org_id",
                        column: x => x.target_org_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exam_tests_party_members_publisher_id",
                        column: x => x.publisher_id,
                        principalTable: "party_members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "member_learning_progress",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    member_id = table.Column<int>(type: "int", nullable: false),
                    content_id = table.Column<int>(type: "int", nullable: false),
                    task_id = table.Column<int>(type: "int", nullable: true),
                    duration_seconds = table.Column<int>(type: "int", nullable: false),
                    is_completed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    completed_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_member_learning_progress", x => x.id);
                    table.ForeignKey(
                        name: "FK_member_learning_progress_learning_contents_content_id",
                        column: x => x.content_id,
                        principalTable: "learning_contents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_member_learning_progress_learning_tasks_task_id",
                        column: x => x.task_id,
                        principalTable: "learning_tasks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_member_learning_progress_party_members_member_id",
                        column: x => x.member_id,
                        principalTable: "party_members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "member_test_records",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    member_id = table.Column<int>(type: "int", nullable: false),
                    test_id = table.Column<int>(type: "int", nullable: false),
                    answers = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    score = table.Column<int>(type: "int", nullable: false),
                    submitted_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_member_test_records", x => x.id);
                    table.ForeignKey(
                        name: "FK_member_test_records_exam_tests_test_id",
                        column: x => x.test_id,
                        principalTable: "exam_tests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_member_test_records_party_members_member_id",
                        column: x => x.member_id,
                        principalTable: "party_members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_content_categories_parent_id",
                table: "content_categories",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "IX_content_tags_tag_id",
                table: "content_tags",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "IX_exam_papers_name",
                table: "exam_papers",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_exam_tests_paper_id",
                table: "exam_tests",
                column: "paper_id");

            migrationBuilder.CreateIndex(
                name: "IX_exam_tests_publisher_id",
                table: "exam_tests",
                column: "publisher_id");

            migrationBuilder.CreateIndex(
                name: "IX_exam_tests_target_org_id",
                table: "exam_tests",
                column: "target_org_id");

            migrationBuilder.CreateIndex(
                name: "IX_learning_contents_category_id",
                table: "learning_contents",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_learning_contents_content_type",
                table: "learning_contents",
                column: "content_type");

            migrationBuilder.CreateIndex(
                name: "IX_learning_contents_is_public",
                table: "learning_contents",
                column: "is_public");

            migrationBuilder.CreateIndex(
                name: "IX_learning_contents_title",
                table: "learning_contents",
                column: "title");

            migrationBuilder.CreateIndex(
                name: "IX_learning_tasks_target_org_id",
                table: "learning_tasks",
                column: "target_org_id");

            migrationBuilder.CreateIndex(
                name: "IX_member_learning_progress_content_id",
                table: "member_learning_progress",
                column: "content_id");

            migrationBuilder.CreateIndex(
                name: "IX_member_learning_progress_member_id_content_id_task_id",
                table: "member_learning_progress",
                columns: new[] { "member_id", "content_id", "task_id" });

            migrationBuilder.CreateIndex(
                name: "IX_member_learning_progress_task_id",
                table: "member_learning_progress",
                column: "task_id");

            migrationBuilder.CreateIndex(
                name: "IX_member_test_records_member_id",
                table: "member_test_records",
                column: "member_id");

            migrationBuilder.CreateIndex(
                name: "IX_member_test_records_member_id_test_id",
                table: "member_test_records",
                columns: new[] { "member_id", "test_id" });

            migrationBuilder.CreateIndex(
                name: "IX_member_test_records_test_id",
                table: "member_test_records",
                column: "test_id");

            migrationBuilder.CreateIndex(
                name: "IX_organizations_name",
                table: "organizations",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_organizations_parent_id",
                table: "organizations",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "IX_party_members_organization_id",
                table: "party_members",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_party_members_phone",
                table: "party_members",
                column: "phone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_party_members_role",
                table: "party_members",
                column: "role");

            migrationBuilder.CreateIndex(
                name: "IX_question_categories_name",
                table: "question_categories",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_questions_category_id",
                table: "questions",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_questions_question_type",
                table: "questions",
                column: "question_type");

            migrationBuilder.CreateIndex(
                name: "IX_tags_name",
                table: "tags",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_task_contents_content_id",
                table: "task_contents",
                column: "content_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "content_tags");

            migrationBuilder.DropTable(
                name: "member_learning_progress");

            migrationBuilder.DropTable(
                name: "member_test_records");

            migrationBuilder.DropTable(
                name: "questions");

            migrationBuilder.DropTable(
                name: "task_contents");

            migrationBuilder.DropTable(
                name: "tags");

            migrationBuilder.DropTable(
                name: "exam_tests");

            migrationBuilder.DropTable(
                name: "question_categories");

            migrationBuilder.DropTable(
                name: "learning_contents");

            migrationBuilder.DropTable(
                name: "learning_tasks");

            migrationBuilder.DropTable(
                name: "exam_papers");

            migrationBuilder.DropTable(
                name: "party_members");

            migrationBuilder.DropTable(
                name: "content_categories");

            migrationBuilder.DropTable(
                name: "organizations");
        }
    }
}
