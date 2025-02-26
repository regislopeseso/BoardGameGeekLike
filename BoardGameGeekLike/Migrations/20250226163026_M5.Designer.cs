﻿// <auto-generated />
using BoardGameGeekLike.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace BoardGameGeekLike.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20250226163026_M5")]
    partial class M5
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("BoardGameGeekLike.Models.Entities.BoardGame", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("CategoryId")
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .HasColumnType("longtext");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("tinyint(1)");

                    b.Property<int>("MaxPlayersCount")
                        .HasColumnType("int");

                    b.Property<int>("MinAge")
                        .HasColumnType("int");

                    b.Property<int>("MinPlayersCount")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.ToTable("boardgames");
                });

            modelBuilder.Entity("BoardGameGeekLike.Models.Entities.BoardGameMechanics", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("BoardGameId")
                        .HasColumnType("int");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("tinyint(1)");

                    b.Property<int>("MechanicId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("BoardGameId");

                    b.HasIndex("MechanicId");

                    b.ToTable("boardgamemechanics");
                });

            modelBuilder.Entity("BoardGameGeekLike.Models.Entities.Category", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("categories");
                });

            modelBuilder.Entity("BoardGameGeekLike.Models.Entities.Mechanic", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("mechanics");
                });

            modelBuilder.Entity("BoardGameGeekLike.Models.Entities.BoardGame", b =>
                {
                    b.HasOne("BoardGameGeekLike.Models.Entities.Category", "Category")
                        .WithMany("BoardGames")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Category");
                });

            modelBuilder.Entity("BoardGameGeekLike.Models.Entities.BoardGameMechanics", b =>
                {
                    b.HasOne("BoardGameGeekLike.Models.Entities.BoardGame", "BoardGame")
                        .WithMany("BoardGameMechanics")
                        .HasForeignKey("BoardGameId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BoardGameGeekLike.Models.Entities.Mechanic", "Mechanic")
                        .WithMany()
                        .HasForeignKey("MechanicId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("BoardGame");

                    b.Navigation("Mechanic");
                });

            modelBuilder.Entity("BoardGameGeekLike.Models.Entities.BoardGame", b =>
                {
                    b.Navigation("BoardGameMechanics");
                });

            modelBuilder.Entity("BoardGameGeekLike.Models.Entities.Category", b =>
                {
                    b.Navigation("BoardGames");
                });
#pragma warning restore 612, 618
        }
    }
}
