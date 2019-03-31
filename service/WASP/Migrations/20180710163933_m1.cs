using Microsoft.EntityFrameworkCore.Migrations;

namespace WASP.Migrations
{
    public partial class m1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"CREATE VIRTUAL TABLE Descriptions USING fts5(Content);");
            /*
            migrationBuilder.CreateTable(
                name: "Descriptions",
                columns: table => new
                {
                    rowid = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Content = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Descriptions", x => x.rowid);
                });
            */
            migrationBuilder.CreateTable(
                name: "Attacks",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Password = table.Column<string>(nullable: true),
                    Location = table.Column<string>(nullable: true),
                    AttackDate = table.Column<string>(nullable: true),
                    Contentrowid = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attacks_Descriptions_Contentrowid",
                        column: x => x.Contentrowid,
                        principalTable: "Descriptions",
                        principalColumn: "rowid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attacks_Contentrowid",
                table: "Attacks",
                column: "Contentrowid");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attacks");

            migrationBuilder.DropTable(
                name: "Descriptions");
        }
    }
}
