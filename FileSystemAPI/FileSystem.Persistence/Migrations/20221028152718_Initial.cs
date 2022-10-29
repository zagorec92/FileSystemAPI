using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileSystem.Persistence.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Content",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Path = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Type = table.Column<byte>(type: "tinyint", nullable: false, comment: "Denotes whether an item is a file or a directory"),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Created = table.Column<long>(type: "bigint", nullable: false),
                    Modified = table.Column<long>(type: "bigint", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Content", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Content_Content_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Content",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Content_ParentId",
                table: "Content",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Content_Path_CustomerId",
                table: "Content",
                columns: new[] { "Path", "CustomerId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Content");
        }
    }
}
