using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fcg.Catalog.Infrastructure._Shared.Context.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "games",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    genre = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    price = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    release_date = table.Column<DateOnly>(type: "date", nullable: true),
                    active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_games", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "promotions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    discount_percent = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    starts_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ends_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_promotions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "library_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    game_id = table.Column<Guid>(type: "uuid", nullable: false),
                    price_paid = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    acquired_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_library_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_library_items_games_game_id",
                        column: x => x.game_id,
                        principalTable: "games",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "promotion_games",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    promotion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    game_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_promotion_games", x => x.id);
                    table.ForeignKey(
                        name: "fk_promotion_games_games_game_id",
                        column: x => x.game_id,
                        principalTable: "games",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_promotion_games_promotions_promotion_id",
                        column: x => x.promotion_id,
                        principalTable: "promotions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_games_genre",
                table: "games",
                column: "genre");

            migrationBuilder.CreateIndex(
                name: "ix_games_title",
                table: "games",
                column: "title");

            migrationBuilder.CreateIndex(
                name: "ix_library_items_game_id",
                table: "library_items",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "ix_library_items_user_id_game_id",
                table: "library_items",
                columns: new[] { "user_id", "game_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_promotion_games_game_id",
                table: "promotion_games",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "ix_promotion_games_promotion_id_game_id",
                table: "promotion_games",
                columns: new[] { "promotion_id", "game_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "library_items");

            migrationBuilder.DropTable(
                name: "promotion_games");

            migrationBuilder.DropTable(
                name: "games");

            migrationBuilder.DropTable(
                name: "promotions");
        }
    }
}
