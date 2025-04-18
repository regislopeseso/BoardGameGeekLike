﻿// <auto-generated />
using System;
using BoardGameGeekLike.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace BoardGameGeekLike.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
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

                    b.Property<decimal>("AverageRating")
                        .HasColumnType("decimal(2,1)");

                    b.Property<int?>("CategoryId")
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .IsRequired()
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

                    b.Property<int>("RatingsCount")
                        .HasColumnType("int");

                    b.Property<int>("SessionsCount")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.ToTable("boardgames");
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

            modelBuilder.Entity("BoardGameGeekLike.Models.Entities.Rating", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("BoardGameId")
                        .HasColumnType("int");

                    b.Property<decimal>("Rate")
                        .HasColumnType("decimal(2,1)");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("BoardGameId");

                    b.HasIndex("UserId");

                    b.ToTable("ratings");
                });

            modelBuilder.Entity("BoardGameGeekLike.Models.Entities.Session", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("BoardGameId")
                        .HasColumnType("int");

                    b.Property<DateOnly?>("Date")
                        .HasColumnType("date");

                    b.Property<int>("Duration_minutes")
                        .HasColumnType("int");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("tinyint(1)");

                    b.Property<int>("PlayersCount")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("BoardGameId");

                    b.HasIndex("UserId");

                    b.ToTable("sessions");
                });

            modelBuilder.Entity("BoardGameGeekLike.Models.Entities.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateOnly>("BirthDate")
                        .HasColumnType("date");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Nickname")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("users");
                });

            modelBuilder.Entity("BoardGameMechanic", b =>
                {
                    b.Property<int>("BoardGamesId")
                        .HasColumnType("int");

                    b.Property<int>("MechanicsId")
                        .HasColumnType("int");

                    b.HasKey("BoardGamesId", "MechanicsId");

                    b.HasIndex("MechanicsId");

                    b.ToTable("BoardGameMechanic");
                });

            modelBuilder.Entity("BoardGameGeekLike.Models.Entities.BoardGame", b =>
                {
                    b.HasOne("BoardGameGeekLike.Models.Entities.Category", "Category")
                        .WithMany("BoardGames")
                        .HasForeignKey("CategoryId");

                    b.Navigation("Category");
                });

            modelBuilder.Entity("BoardGameGeekLike.Models.Entities.Rating", b =>
                {
                    b.HasOne("BoardGameGeekLike.Models.Entities.BoardGame", "BoardGame")
                        .WithMany()
                        .HasForeignKey("BoardGameId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BoardGameGeekLike.Models.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("BoardGame");

                    b.Navigation("User");
                });

            modelBuilder.Entity("BoardGameGeekLike.Models.Entities.Session", b =>
                {
                    b.HasOne("BoardGameGeekLike.Models.Entities.BoardGame", "BoardGame")
                        .WithMany("Sessions")
                        .HasForeignKey("BoardGameId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BoardGameGeekLike.Models.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("BoardGame");

                    b.Navigation("User");
                });

            modelBuilder.Entity("BoardGameMechanic", b =>
                {
                    b.HasOne("BoardGameGeekLike.Models.Entities.BoardGame", null)
                        .WithMany()
                        .HasForeignKey("BoardGamesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BoardGameGeekLike.Models.Entities.Mechanic", null)
                        .WithMany()
                        .HasForeignKey("MechanicsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("BoardGameGeekLike.Models.Entities.BoardGame", b =>
                {
                    b.Navigation("Sessions");
                });

            modelBuilder.Entity("BoardGameGeekLike.Models.Entities.Category", b =>
                {
                    b.Navigation("BoardGames");
                });
#pragma warning restore 612, 618
        }
    }
}
