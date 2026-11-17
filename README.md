# Campus App — ASP.NET MVC 5 with Entity Framework (Lab 7)

A basic ASP.NET MVC website built with Entity Framework Code First for **Lab 7 / Topic 8** of the Web Development course, extended in **Lab 8** with student search, campus filtering and a strongly typed view model, and in **Lab 9** with EF migrations, seed data, data annotations, sorting and paging. It contains two parts: the tutorial project (Categories and Products) and the lab task (University Campuses and Students in a one-to-many relationship).

## Requirements

- Visual Studio 2022 with the **ASP.NET and web development** workload
- .NET Framework 4.7.2
- SQL Server LocalDB (`(localdb)\MSSQLLocalDB`), installed with Visual Studio
- NuGet packages are restored automatically from `packages.config` on the first build

## Running the site

1. Open `CampusApp.sln` in Visual Studio 2022.
2. Press `Ctrl+F5` (Debug → Start Without Debugging). `Application_Start` applies the two migrations, so the database `CampusContext.mdf` is created in `App_Data` with the three campuses and nine students already in it.
3. To rebuild the database from scratch, delete `App_Data/CampusContext.mdf` and `CampusContext_log.ldf` and run the site again (or run `Update-Database` in the Package Manager Console).

## What is inside

| Area | Contents |
| --- | --- |
| `Models` | `Product`, `Category` (tutorial), `Student`, `UniversityCampus` (lab) |
| `Data` | `CampusContext` — one `DbContext` with four `DbSet`s and its connection string in `Web.config` |
| `Controllers` | `HomeController`, `ProductsController`, `CategoriesController`, `StudentsController`, `UniversityCampusesController` |
| `Views` | Scaffolded CRUD pages for Products, Categories, Students and Campuses |

## The one-to-many relationship

```csharp
public class Student
{
    [Key] public int ID { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }

    public int? CampusID { get; set; }                     // foreign key, nullable
    public virtual UniversityCampus Campus { get; set; }    // reference navigation property
}

public class UniversityCampus
{
    [Key] public int ID { get; set; }
    public string Name { get; set; }

    public virtual ICollection<Student> Students { get; set; }  // collection navigation property
}
```

The foreign key `CampusID` sits on the `Student` table, so many students can point at the same campus while each student points at most one campus. Making it `int?` keeps the relationship optional: a student may not belong to a campus at all. Entity Framework Code First creates the column and the foreign key constraint by convention (the `<navigation property>ID` naming pattern), so no Fluent API configuration is needed.

The navigation properties let the application walk the relationship through objects instead of joins: `student.Campus.Name` returns the campus of a student, `campus.Students` enumerates the students of a campus, `db.Students.Include(s => s.Campus)` eager-loads the related campus for a list page, and the MVC scaffolding uses the collection to render the campus drop-down on the Student create and edit forms.

## Lab 8 — searching, filtering and view models

`StudentsController.Index` now takes two query-string parameters and does three things in one request:

```csharp
public ActionResult Index(string campus, string search)
{
    StudentIndexViewModel viewModel = new StudentIndexViewModel();
    var students = db.Students.Include(s => s.Campus);

    if (!String.IsNullOrEmpty(search))
    {
        students = students.Where(s => s.Name.Contains(search) ||
                                       s.Address.Contains(search) ||
                                       s.Campus.Name.Contains(search));
        viewModel.Search = search;
    }

    viewModel.CampusesWithCount = from s in students
                                  where s.CampusID != null
                                  group s by s.Campus.Name into campusGroup
                                  select new CampusWithCount
                                  {
                                      CampusName = campusGroup.Key,
                                      StudentCount = campusGroup.Count()
                                  };

    if (!String.IsNullOrEmpty(campus))
    {
        students = students.Where(s => s.Campus.Name == campus);
    }

    viewModel.Students = students.OrderBy(s => s.Campus.Name).ThenBy(s => s.Name).ToList();
    viewModel.Campus = campus;

    return View(viewModel);
}
```

- **Search** is a LINQ `Contains` over the student name, the address and the related campus name. The navigation property is available because `Include(s => s.Campus)` eager-loads it.
- **Campus filter** is a second `Where` on the same query, so search and filter combine.
- **Ordering matters**: the campus list with its counts is built *before* the campus filter is applied. Building it afterwards would leave only the selected campus in the drop-down (and a count of zero for everything else), which is the trap the tutorial warns about.
- **View model instead of ViewBag**: `StudentIndexViewModel` carries `Students`, `Search`, `Campus`, `CampusesWithCount` and the derived `CampusFilterItems`, so the strongly typed view reads `Model.Students` and `Model.CampusFilterItems` instead of `ViewBag`. `CampusWithCount.NameWithCount` produces the `Manawatu Campus (2)` text shown in the drop-down.
- **Search box in the navigation bar** (`Views/Shared/_Layout.cshtml`) is a GET form posting `Search` to `Students/Index`, so the term travels in the URL and survives the campus filter through the hidden field in the filter form.
- **Column headings come from Data Annotations**: `[Display(Name = "Student Name")]` and `[Display(Name = "Address")]` on `Student`, `[Display(Name = "Campus Name")]` on `UniversityCampus`, and the list view uses `@Html.DisplayNameFor(model => model.Students.First().Name)`. `DisplayNameFor` only reads the metadata of the property, it does not execute `First()`, so an empty result set still renders.

The intermediate ViewBag version was exercised first (search plus `ViewBag.Campus` as a `SelectList`); `docs/screenshots/lab8/01`–`04` were captured while that version was live, and `05`–`08` show the final view-model version with the campus counts. The `05`–`08` shots were retaken after the display annotations were added, so their headings read `Campus Name` and `Student Name`.

## Lab 9 — migrations, seeding, validation, sorting and paging

The database is now built and filled by EF migrations instead of letting Code First create it from the model, and the student list is sorted and paged. `Global.asax.cs` sets the initializer so the migrations and the seed run on the first request:

```csharp
Database.SetInitializer(new MigrateDatabaseToLatestVersion<CampusContext, Migrations.Configuration>());
```

Two migrations are committed, in this order:

| Migration | What it does |
| --- | --- |
| `202611170825294_InitialDatabase` | creates `UniversityCampus`, `Students`, `Categories`, `Products` and the `CampusID` foreign key |
| `202611170827176_StudentCampusValidation` | applies the validation annotations: `Students.Name` and `UniversityCampus.Name` become `nvarchar(50)`, `Students.Address` becomes `nvarchar(100)`, all three `NOT NULL` |

`Migrations/Configuration.cs` holds the seed. Campuses are written first because the students need their `ID`s:

```csharp
protected override void Seed(CampusContext context)
{
    var campuses = new[]
    {
        new UniversityCampus { Name = "Albany Campus" },
        new UniversityCampus { Name = "Manawatu Campus" },
        new UniversityCampus { Name = "Wellington Campus" }
    };

    Array.ForEach(campuses, c => context.UniversityCampus.AddOrUpdate(x => x.Name, c));
    context.SaveChanges();

    var albany    = context.UniversityCampus.Single(c => c.Name == "Albany Campus").ID;
    var manawatu  = context.UniversityCampus.Single(c => c.Name == "Manawatu Campus").ID;
    var wellington = context.UniversityCampus.Single(c => c.Name == "Wellington Campus").ID;

    context.Students.AddOrUpdate(s => s.Name,
        new Student { Name = "Ben Carter",    Address = "5 Massey Rd, Albany",              CampusID = albany },
        new Student { Name = "Emma Wilson",   Address = "18 Oteha Valley Rd, Albany",       CampusID = albany },
        // ... nine students in total, three per campus
        new Student { Name = "Chen Wei",      Address = "9 Cuba St, Wellington",            CampusID = wellington });
    context.SaveChanges();
}
```

`AddOrUpdate` keys on the campus name and on the student name, so running the seed twice updates the existing rows instead of inserting duplicates.

Validation lives on the model as data annotations, so it is enforced on both the server and the client-side unobtrusive validation:

```csharp
[Required(ErrorMessage = "Student name is required.")]
[StringLength(50, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 50 characters.")]
[RegularExpression(@"^[A-Z][A-Za-z]*(\s[A-Z][A-Za-z]*)*$",
    ErrorMessage = "Name must start with a capital letter; letters and spaces only.")]
public string Name { get; set; }
```

Sorting and paging are one query-string driven action. `StudentIndexViewModel.Students` is an `IPagedList<Student>`, and `Sorts` drives the drop-down:

```csharp
public ActionResult Index(string campus, string search, string sortBy, int? page)
{
    StudentIndexViewModel viewModel = new StudentIndexViewModel();
    var students = db.Students.Include(s => s.Campus);

    // search, campus counts and campus filter as in Lab 8 ...

    viewModel.Sorts = new Dictionary<string, string>
    {
        { "Name A-Z", "name_az" },
        { "Name Z-A", "name_za" },
        { "Campus A-Z", "campus_az" }
    };

    switch (sortBy)
    {
        case "name_az":
            students = students.OrderBy(s => s.Name);
            break;
        case "name_za":
            students = students.OrderByDescending(s => s.Name);
            break;
        default:
            students = students.OrderBy(s => s.Campus.Name).ThenBy(s => s.Name);
            break;
    }

    viewModel.Campus = campus;
    viewModel.SortBy = sortBy;

    const int PageItems = 3;
    viewModel.Students = students.ToPagedList(page ?? 1, PageItems);

    return View(viewModel);
}
```

The view keeps the four pieces of state in the pager links, otherwise turning a page would drop the filter and the sort order:

```html
Page @(Model.Students.PageCount < Model.Students.PageNumber ? 0 : Model.Students.PageNumber)
of @Model.Students.PageCount

@Html.PagedListPager(Model.Students, page => Url.Action("Index",
    new { campus = Model.Campus, Search = Model.Search,
          sortBy = Model.SortBy, page }))
```

- The page size is a constant (`PageItems = 3`) so that nine students produce three pages and every state of the pager is visible in the screenshots.
- `PagedList` and `PagedList.Mvc` are added to `packages.config`, and `~/Content/PagedList.css` is bundled into `~/Content/css` in `BundleConfig` so the pager is styled.
- `campus_az` sorts on the navigation property (`s.Campus.Name`); EF translates it to a join, and `ThenBy(s => s.Name)` keeps the order inside a campus stable.

## Notes

- `packages/`, `bin/`, `obj/` and the generated `.mdf` database are not committed; NuGet restore and the first run recreate them.
- The connection string lives in the root `Web.config`, under the name `CampusContext`, which is the name passed to `base(...)` in `CampusContext`.
- Lab 9 is published at https://github.com/Feng105/Lab9_CampusApp; the earlier Labs 7 and 8 live in https://github.com/Feng105/Lab7_CampusApp.

## Tutorial project (week 10 demo)

`Tutorial/OnlineStore` is the week 10 demo from the tutorial, with the sorting and paging part filled in (`ProductIndexViewModel`, `ProductsController.Index`, `Views/Products/Index.cshtml`). It keeps its own three migrations, so the database has to be built once before the site runs:

1. Open `Tutorial/OnlineStore.sln`.
2. In the Package Manager Console run `Update-Database`. The migrations create `OnlineStoreContext2.mdf` in `App_Data` and the seed writes 11 categories and 9 products.
3. Press `Ctrl+F5` and go to `/Products`: the category filter, the sort drop-down (Default / Price low to high / Price high to low) and `Page 1 of 3` with three products per page.
