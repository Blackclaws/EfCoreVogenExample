// See https://aka.ms/new-console-template for more information

using System.Xml.XPath;
using EFCoreTest;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddLogging();

var connectionString = "DataSource=:memory:;cache=shared";
var connection = new SqliteConnection(connectionString);
connection.Open();
builder.Services.AddDbContext<AppDbContext>(optionsAction =>
{
    optionsAction.EnableSensitiveDataLogging();
    optionsAction.UseSqlite(connection);
});

TeacherId firstTeacherId;

var app = builder.Build();

{
    var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();

    var teacher = new Teacher()
    {
        Id = null,
        TeacherName = "NormalTeacher"
    };

    var student = new Student()
    {
        Id = null,
        FavoriteTeacher = teacher
    };

    context.Students.Add(student);
    context.Teachers.Add(teacher);
    await context.SaveChangesAsync();
    if (teacher.Id is null) throw new Exception("Error while persisting teacher");
    firstTeacherId = teacher.Id;
}

StudentId idToQueryFor;

{
    var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();

    var student = new Student()
    {
        Id = null,
    };

    context.Students.Add(student);
    await context.SaveChangesAsync();
    if (student.Id is null) throw new Exception("Persistence failed");
    idToQueryFor = student.Id;
}

{
    var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();

    var queriedStudent = await context.Students.FirstOrDefaultAsync(x => x.Id == idToQueryFor);
    if (queriedStudent is null) throw new Exception("Unable to retrieve student");
    if (queriedStudent.FavoriteTeacher is null) Console.WriteLine("No favorite teacher");
}


{
    var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();

    var teacher = new Teacher()
    {
        Id = null,
        TeacherName = "ForeignKeyTeacher"
    };

    var student = new StudentWithFk()
    {
        Id = null,
        FavoriteTeacher = teacher
    };

    context.StudentsWithFks.Add(student);
    context.Teachers.Add(teacher);
    if(student.FavoriteTeacherId is null) Console.WriteLine("StudentWithFk not persisted yet");
    await context.SaveChangesAsync();
    if (student.FavoriteTeacherId is null) throw new Exception("StudentWithFk should have populated this");
    Console.WriteLine(student.FavoriteTeacherId);
    
    // Show that teacher name is not immediately updated if the teacher isn't change tracked
    Console.WriteLine(student.FavoriteTeacher?.TeacherName);
    student.FavoriteTeacherId = firstTeacherId;
    Console.WriteLine(student.FavoriteTeacher?.TeacherName);
    var teachers = await context.Teachers.ToListAsync();
    
    Console.WriteLine(student.FavoriteTeacher?.TeacherName);
    await context.SaveChangesAsync();
    Console.WriteLine(student.FavoriteTeacher?.TeacherName);
    
    student = await context.StudentsWithFks.Include(x => x.FavoriteTeacher).FirstOrDefaultAsync(x => x.Id == student.Id);
    Console.WriteLine(student?.FavoriteTeacher?.TeacherName);
}