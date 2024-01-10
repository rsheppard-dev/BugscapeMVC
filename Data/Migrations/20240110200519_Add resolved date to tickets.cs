﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BugscapeMVC.Data.Migrations
{
    /// <inheritdoc />
    public partial class Addresolveddatetotickets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ResolvedDate",
                table: "Tickets",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResolvedDate",
                table: "Tickets");
        }
    }
}
