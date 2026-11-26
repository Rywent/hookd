using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hookd.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WebhookTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    EventType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TargetUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    Payload = table.Column<string>(type: "text", nullable: false),
                    ContentType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, defaultValue: "application/json; charset=utf-8"),
                    Headers = table.Column<string>(type: "jsonb", nullable: true),
                    IdempotencyKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    HttpMethod = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "POST"),
                    Status = table.Column<string>(type: "text", nullable: false),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    MaxAttempts = table.Column<int>(type: "integer", nullable: false),
                    LastStatusCode = table.Column<int>(type: "integer", nullable: true),
                    LastError = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    LastResponseBody = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SuccessAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FailureAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextAttemptAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ScheduleTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebhookTasks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WebhookDeliveryAttempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    WebhookTaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttemptNumber = table.Column<int>(type: "integer", nullable: false),
                    StatusCode = table.Column<int>(type: "integer", nullable: true),
                    IsSuccess = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ResponseBody = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FinishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DurationMs = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebhookDeliveryAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebhookDeliveryAttempts_WebhookTasks_WebhookTaskId",
                        column: x => x.WebhookTaskId,
                        principalTable: "WebhookTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WebhookDeliveryAttempts_WebhookTaskId",
                table: "WebhookDeliveryAttempts",
                column: "WebhookTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_WebhookDeliveryAttempts_WebhookTaskId_AttemptNumber",
                table: "WebhookDeliveryAttempts",
                columns: new[] { "WebhookTaskId", "AttemptNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_WebhookTasks_CreatedAt",
                table: "WebhookTasks",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_WebhookTasks_IdempotencyKey",
                table: "WebhookTasks",
                column: "IdempotencyKey",
                unique: true,
                filter: "\"IdempotencyKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_WebhookTasks_Status_NextAttemptAt",
                table: "WebhookTasks",
                columns: new[] { "Status", "NextAttemptAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WebhookDeliveryAttempts");

            migrationBuilder.DropTable(
                name: "WebhookTasks");
        }
    }
}
