// SPDX-License-Identifier: LicenseRef-arfinityProprietary
// SPDX-FileCopyrightText: © 2022 arfinity GmbH <contact@arfinity.io>

using Microsoft.EntityFrameworkCore;
using Vogen;

namespace EFCoreTest;

[ValueObject<int>(Conversions.EfCoreValueConverter)]
public partial class TeacherId { }

[ValueObject<int>(Conversions.EfCoreValueConverter)]
public partial class StudentId { }

public class Teacher
{
    public TeacherId? Id { get; set; }
    public string TeacherName { get; init; } = "";
}

public class Student
{
    public StudentId? Id { get; set; }

    public Teacher? FavoriteTeacher { get; set; }
}


public class StudentWithFk
{
    public StudentId? Id { get; set; }
    
    public TeacherId? FavoriteTeacherId { get; set; }

    public Teacher? FavoriteTeacher { get; set; }
}

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Teacher>(teacherBuilder =>
        {
            teacherBuilder.HasKey(x => x.Id);
            teacherBuilder.Property(x => x.Id).HasConversion<TeacherId.EfCoreValueConverter>().ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<Student>(studentBuilder =>
        {
            studentBuilder.HasKey(x => x.Id);
            studentBuilder.Property(x => x.Id).HasConversion<StudentId.EfCoreValueConverter>().ValueGeneratedOnAdd();
            studentBuilder.HasOne(x => x.FavoriteTeacher);
        });
        
        modelBuilder.Entity<StudentWithFk>(studentBuilder =>
        {
            studentBuilder.HasKey(x => x.Id);
            studentBuilder.Property(x => x.Id).HasConversion<StudentId.EfCoreValueConverter>().ValueGeneratedOnAdd();
            studentBuilder.HasOne(x => x.FavoriteTeacher).WithMany().HasForeignKey(x => x.FavoriteTeacherId);
        });
    }
    public DbSet<Student> Students => Set<Student>();
    public DbSet<StudentWithFk> StudentsWithFks => Set<StudentWithFk>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
}