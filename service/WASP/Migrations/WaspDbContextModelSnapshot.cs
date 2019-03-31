﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WASP.Storage;

namespace WASP.Migrations
{
    [DbContext(typeof(WaspDbContext))]
    partial class WaspDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.1-rtm-30846");

            modelBuilder.Entity("WASP.Models.Attack", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AttackDate");

                    b.Property<long?>("Contentrowid");

                    b.Property<string>("Location");

                    b.Property<string>("Password");

                    b.HasKey("Id");

                    b.HasIndex("Contentrowid");

                    b.ToTable("Attacks");
                });

            modelBuilder.Entity("WASP.Models.AttackDescriptionContent", b =>
                {
                    b.Property<long>("rowid")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Content");

                    b.HasKey("rowid");

                    b.ToTable("Descriptions");
                });

            modelBuilder.Entity("WASP.Models.Attack", b =>
                {
                    b.HasOne("WASP.Models.AttackDescriptionContent", "Content")
                        .WithMany()
                        .HasForeignKey("Contentrowid");
                });
#pragma warning restore 612, 618
        }
    }
}
