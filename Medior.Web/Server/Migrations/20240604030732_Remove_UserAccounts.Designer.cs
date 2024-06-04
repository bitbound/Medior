﻿// <auto-generated />
using System;
using Medior.Web.Server.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Medior.Web.Server.Migrations
{
    [DbContext(typeof(AppDb))]
    [Migration("20240604030732_Remove_UserAccounts")]
    partial class Remove_UserAccounts
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.6");

            modelBuilder.Entity("Medior.Shared.Entities.ClipboardSave", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("AccessTokenEdit")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("AccessTokenView")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<byte[]>("Content")
                        .IsRequired()
                        .HasColumnType("BLOB");

                    b.Property<long>("ContentSize")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ContentType")
                        .HasColumnType("INTEGER");

                    b.Property<string>("CreatedAt")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("FriendlyName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("LastAccessed")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("UserAccountId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("ClipboardSaves");
                });

            modelBuilder.Entity("Medior.Shared.Entities.UploadedFile", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("AccessTokenEdit")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("AccessTokenView")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ContentDisposition")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<long>("FileSize")
                        .HasColumnType("INTEGER");

                    b.Property<string>("LastAccessed")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("UploadedAt")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("UploadedFiles");
                });
#pragma warning restore 612, 618
        }
    }
}