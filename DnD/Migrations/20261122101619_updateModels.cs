using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DnD.Migrations
{
    /// <inheritdoc />
    public partial class updateModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Alignment",
                table: "Characters",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ArmorId",
                table: "Characters",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "ArmorTraining",
                table: "Characters",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "BackgroundCharacter",
                table: "Characters",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BackgroundId",
                table: "Characters",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ClassId",
                table: "Characters",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Darkvision",
                table: "Characters",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DeathSaveFailures",
                table: "Characters",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DeathSaveSuccesses",
                table: "Characters",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<List<string>>(
                name: "Feats",
                table: "Characters",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasShield",
                table: "Characters",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HeroicInspiration",
                table: "Characters",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "HitDiceSpent",
                table: "Characters",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RaceId",
                table: "Characters",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<List<string>>(
                name: "SaveProficiencies",
                table: "Characters",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "Size",
                table: "Characters",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<List<string>>(
                name: "SkillExpertise",
                table: "Characters",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<List<string>>(
                name: "SkillProficiencies",
                table: "Characters",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<int>(
                name: "Speed",
                table: "Characters",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TempHp",
                table: "Characters",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<List<string>>(
                name: "ToolProficiencies",
                table: "Characters",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<List<string>>(
                name: "WeaponProficiencies",
                table: "Characters",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<int>(
                name: "Xp",
                table: "Characters",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Alignment",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "ArmorId",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "ArmorTraining",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "BackgroundCharacter",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "BackgroundId",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "ClassId",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "Darkvision",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "DeathSaveFailures",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "DeathSaveSuccesses",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "Feats",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "HasShield",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "HeroicInspiration",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "HitDiceSpent",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "RaceId",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "SaveProficiencies",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "SkillExpertise",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "SkillProficiencies",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "Speed",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "TempHp",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "ToolProficiencies",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "WeaponProficiencies",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "Xp",
                table: "Characters");
        }
    }
}
