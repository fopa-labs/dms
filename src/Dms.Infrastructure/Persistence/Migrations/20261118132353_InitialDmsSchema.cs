using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dms.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialDmsSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.CreateTable(
                name: "audit_log",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    action = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    entity_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_log", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "collection",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    slug = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    lang = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    ts_config = table.Column<string>(type: "character varying(63)", maxLength: 63, nullable: false),
                    domain_pack = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    settings = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_collection", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tag",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tag", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "document",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    collection_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    active_version_id = table.Column<Guid>(type: "uuid", nullable: true),
                    metadata = table.Column<string>(type: "jsonb", nullable: false),
                    lang = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_document", x => x.id);
                    table.ForeignKey(
                        name: "fk_document_collection_collection_id",
                        column: x => x.collection_id,
                        principalTable: "collection",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "document_tag",
                columns: table => new
                {
                    document_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_document_tag", x => new { x.document_id, x.tag_id });
                    table.ForeignKey(
                        name: "fk_document_tag_document_document_id",
                        column: x => x.document_id,
                        principalTable: "document",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_document_tag_tags_tag_id",
                        column: x => x.tag_id,
                        principalTable: "tag",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "document_version",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version_no = table.Column<int>(type: "integer", nullable: false),
                    blob_sha256 = table.Column<string>(type: "character(64)", fixedLength: true, maxLength: 64, nullable: false),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    mime_type = table.Column<string>(type: "character varying(127)", maxLength: 127, nullable: false),
                    size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    page_count = table.Column<int>(type: "integer", nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    error = table.Column<string>(type: "text", nullable: true),
                    uploaded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_document_version", x => x.id);
                    table.CheckConstraint("ck_document_version_status", "status IN ('queued', 'processing', 'indexed', 'failed')");
                    table.ForeignKey(
                        name: "fk_document_version_document_document_id",
                        column: x => x.document_id,
                        principalTable: "document",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_audit_log_entity_type_entity_id_at",
                table: "audit_log",
                columns: new[] { "entity_type", "entity_id", "at" });

            migrationBuilder.CreateIndex(
                name: "ix_collection_slug",
                table: "collection",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_document_active_version_id",
                table: "document",
                column: "active_version_id");

            migrationBuilder.CreateIndex(
                name: "ix_document_collection_id_updated_at_id",
                table: "document",
                columns: new[] { "collection_id", "updated_at", "id" });

            migrationBuilder.CreateIndex(
                name: "ix_document_metadata",
                table: "document",
                column: "metadata")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "ix_document_tag_tag_id",
                table: "document_tag",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "ix_document_version_blob_sha256",
                table: "document_version",
                column: "blob_sha256");

            migrationBuilder.CreateIndex(
                name: "ix_document_version_document_id_version_no",
                table: "document_version",
                columns: new[] { "document_id", "version_no" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tag_name",
                table: "tag",
                column: "name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_document_document_versions_active_version_id",
                table: "document",
                column: "active_version_id",
                principalTable: "document_version",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_document_collection_collection_id",
                table: "document");

            migrationBuilder.DropForeignKey(
                name: "fk_document_document_versions_active_version_id",
                table: "document");

            migrationBuilder.DropTable(
                name: "audit_log");

            migrationBuilder.DropTable(
                name: "document_tag");

            migrationBuilder.DropTable(
                name: "tag");

            migrationBuilder.DropTable(
                name: "collection");

            migrationBuilder.DropTable(
                name: "document_version");

            migrationBuilder.DropTable(
                name: "document");
        }
    }
}
