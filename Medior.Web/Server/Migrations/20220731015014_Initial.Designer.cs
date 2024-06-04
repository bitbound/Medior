﻿// <auto-generated />
using System;
using Medior.Web.Server.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Medior.Web.Server.Migrations;

[DbContext(typeof(AppDb))]
[Migration("20220731015014_Initial")]
partial class Initial
{
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder.HasAnnotation("ProductVersion", "6.0.7");

        modelBuilder.Entity("Medior.Shared.Entities.UploadedFile", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("TEXT");

                b.Property<string>("ContentDisposition")
                    .IsRequired()
                    .HasColumnType("TEXT");

                b.Property<string>("FileName")
                    .IsRequired()
                    .HasColumnType("TEXT");

                b.Property<long>("FileSize")
                    .HasColumnType("INTEGER");

                b.Property<string>("UploadedAt")
                    .IsRequired()
                    .HasColumnType("TEXT");

                b.HasKey("Id");

                b.ToTable("UploadedFiles");
            });
#pragma warning restore 612, 618
    }
}
