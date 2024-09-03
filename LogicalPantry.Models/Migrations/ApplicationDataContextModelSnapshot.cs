﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace LogicalPantry.Models.Migrations
{
    [DbContext(typeof(ApplicationDataContext))]
    partial class ApplicationDataContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.16")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("LogicalPantry.Models.Models.Role", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("RoleName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("LogicalPantry.Models.Models.Tenant", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("AdminEmail")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Logo")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PageName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PaypalId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TenantDisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TenantName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Timezone")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Tenants");
                });

            modelBuilder.Entity("LogicalPantry.Models.Models.TimeSlot", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("EndTime")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("StartTime")
                        .HasColumnType("datetime2");

                    b.Property<int>("TenantId")
                        .HasColumnType("int");

                    b.Property<string>("TimeSlotName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("TenantId");

                    b.HasIndex("UserId");

                    b.ToTable("TimeSlots");
                });

            modelBuilder.Entity("LogicalPantry.Models.Models.TimeSlotSignup", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<bool>("Attended")
                        .HasColumnType("bit");

                    b.Property<int>("TimeSlotId")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("TimeSlotId");

                    b.HasIndex("UserId");

                    b.ToTable("TimeSlotSignups");
                });

            modelBuilder.Entity("LogicalPantry.Models.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsAllow")
                        .HasColumnType("bit");

                    b.Property<bool>("IsRegistered")
                        .HasColumnType("bit");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("TenantId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("TenantId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("LogicalPantry.Models.Models.UserRole", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<int>("RoleId")
                        .HasColumnType("int");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("UserRoles");
                });

            modelBuilder.Entity("LogicalPantry.Models.Models.TimeSlot", b =>
                {
                    b.HasOne("LogicalPantry.Models.Models.Tenant", "Tenant")
                        .WithMany("TimeSlots")
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("LogicalPantry.Models.Models.User", "User")
                        .WithMany("TimeSlots")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Tenant");

                    b.Navigation("User");
                });

            modelBuilder.Entity("LogicalPantry.Models.Models.TimeSlotSignup", b =>
                {
                    b.HasOne("LogicalPantry.Models.Models.TimeSlot", "TimeSlot")
                        .WithMany("TimeSlotSignups")
                        .HasForeignKey("TimeSlotId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("LogicalPantry.Models.Models.User", "User")
                        .WithMany("TimeSlotSignups")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("TimeSlot");

                    b.Navigation("User");
                });

            modelBuilder.Entity("LogicalPantry.Models.Models.User", b =>
                {
                    b.HasOne("LogicalPantry.Models.Models.Tenant", "Tenant")
                        .WithMany("Users")
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Tenant");
                });

            modelBuilder.Entity("LogicalPantry.Models.Models.UserRole", b =>
                {
                    b.HasOne("LogicalPantry.Models.Models.Role", "Role")
                        .WithMany("UserRoles")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("LogicalPantry.Models.Models.User", "User")
                        .WithMany("UserRoles")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Role");

                    b.Navigation("User");
                });

            modelBuilder.Entity("LogicalPantry.Models.Models.Role", b =>
                {
                    b.Navigation("UserRoles");
                });

            modelBuilder.Entity("LogicalPantry.Models.Models.Tenant", b =>
                {
                    b.Navigation("TimeSlots");

                    b.Navigation("Users");
                });

            modelBuilder.Entity("LogicalPantry.Models.Models.TimeSlot", b =>
                {
                    b.Navigation("TimeSlotSignups");
                });

            modelBuilder.Entity("LogicalPantry.Models.Models.User", b =>
                {
                    b.Navigation("TimeSlotSignups");

                    b.Navigation("TimeSlots");

                    b.Navigation("UserRoles");
                });
#pragma warning restore 612, 618
        }
    }
}
