# Hyperboliq
## Take control of your queries

Hyperboliq is a .NET based ORM with the following main goals

1. **Predictability**  
    It should be possible to intuit what SQL statements is going to be generated by your code. No magic allowed.
2. **Control**  
    Hyperboliq will by design not contain any magic or attempt to second-guess you by performing clever query rewrites behind your back. 
    What you see is what you get. No magic allowed.
3. **Expressability**   
    Hyperboliq will aim to support as much of the SQL standard as possible (within reason).

# Building Hyperboliq

Hyperboliq targets the .NET framework version 4.5.1 and F# 4.3.1.
Currently there are no plans to support older versions, though this may change.
Apart from this, Hyperboliq does not require anything special to build. Simply restore nuget packages and build the solution with Visual Studio.

# License
Hyperboliq is distributed under the terms of the Apache License v.2.

# Current state of Hyperboliq
Hyperboliq is still considered to be in pre-alpha and can still change drastically from one day to another. 
The API is to be considered unstable and you are not recommended to use this in production yet. 
Suffice to say **you should probably wait for the first official release if you want to use this library**.

# Getting Started
For now, if you wish to use Hyperboliq you will need to download and build it manually. 
When Hyperboliq has reached a state that is stable there will be nuget packages.
Do be aware of the fact that Hyperboliq is currently in a pre-alpha state and as thus may still have breaking API changes etc.

## Selecting data
Hyperboliq aims to use POCOs for accessing data in order to be able to provide as much intellisense/type safety as possible. 
Hyperboliq will always generate table aliases for all tables in your query. 
You can provide your own aliases if necessary, though as long as you are not referencing the same table more than once in your query you should not have to do this. 

### Basic selects

````csharp
Select<Person>(p => p.Name).From<Person>().Where<Person>(p => p.Age > 42)
```

```sql
SELECT PersonRef.Name 
FROM Person PersonRef 
WHERE PersonRef.Age > 42
```

### Basic joins

```csharp
Select.Star<Person>().Star<Car>().From<Person>().InnerJoin<Person, Car>((p, c) => c.DriverId == p.Id)
```

```sql
SELECT PersonRef.*, CarRef.* 
FROM Person PersonRef
INNER JOIN Car CarRef ON CarRef.DriverId = PersonRef.Id
```

### Self-joins
```csharp
var child = Table<Person>.WithReferenceName("child");
var parent = Table<Person>.WithReferenceName("parent");
Select.Column(child, p => p.Name).Column(parent, p => p.Name).From(child).InnerJoin(child, parent, (c, p) => c.ParentId == p.Id)
```

```sql
SELECT child.Name, parent.Name 
FROM Person child 
INNER JOIN Person parent ON child.ParentId = parent.Id
```

### Windowing functions with OVER/PARTITION BY/ORDER BY
```csharp
Select.Column<Person>(p => p.Name)
      .Column<Person>(
          p => Sql.Sum(p.Age), 
          Over.PartitionBy<Person>(p => p.Name).OrderBy<Person>(p => p.Age, Direction.Ascending))
      .From<Person>();
```

```sql
SELECT PersonRef.Name, SUM(PersonRef.Age) OVER (PARTITION BY PersonRef.Name ORDER BY PersonRef.Age ASC)
FROM Person PersonRef
```

### Subquery support
Hyperboliq allows you to create queries that contain subqueries. However due to the nature of C#'s type system these kind of queries are not as type safe. This may let you write queries that translate into SQL that your database will not accept (and thus throw a `SQLException` during runtime). If you use subqueries, be extra careful to match the correct types, number of expected results and number of columns to avoid these exceptions during runtime.

#### Subqueries returning a single value
```csharp
// In this case we need to use the utility method Sql.SubExpr<T> in order to help the C# Type system.
Select.Star<Person>()
      .From<Person>()
      .Where<Person>(p => p.Age > Sql.SubExpr<int>(Select.Column<Car>(c => c.Age)
                                                         .From<Car>()
                                                         .Where<Car>(c => c.Id == 42)));
```

```sql
SELECT PersonRef.* FROM Person PersonRef WHERE PersonRef.Age > (SELECT CarRef.Age FROM Car CarRef WHERE CarRef.Id = 42)
```

#### Subqueries with IN
```csharp
// In the case of subqueries with IN we do not need to use the Sql.SubExpr<T> utility method and can just write the subquery inline.
Select.Star<Person>()
      .From<Person>()
      .Where<Person>(p => Sql.In(p.Id, Select.Column<Car>(c => c.DriverId).From<Car>()));
```

```sql
SELECT PersonRef.* FROM Person PersonRef WHERE PersonRef.Id IN (SELECT CarRef.DriverId FROM Car CarRef)
```

### Common Table Expression support
Hyperboliq lets you use common table expressions in your select queries. 
These do however require an extra class in order to specify how the result set for this CTE looks.

#### Simple example

```csharp
// Class for specifying how the result set of our CTE looks
public class PersonLite
{
    public string Name { get; set; }
    public int Age { get; set; }
}

With.Table<PersonLite>(
    Select.Column<Person>(p => new { p.Name, p.Age })
          .From<Person>()
          .Where<Person>(p => p.Age > 15))
    .Query(
        Select.Column<PersonLite>(p => p.Name)
              .From<PersonLite>()
              .Where<PersonLite>(p => p.Age == 42));
```

```sql
WITH PersonLite AS (SELECT PersonRef.Name, PersonRef.Age FROM Person PersonRef WHERE PersonRef.Age > 15) 
SELECT PersonLiteRef.Name FROM PersonLite PersonLiteRef WHERE PersonLiteRef.Age = 42
```

#### Multiple CTE's with the same result set

```csharp
// This example uses the same PersonLite class as the example above

// Define two aliases for the same result set in order to be able to refer to them separately
var oldies = Table<PersonLite>.WithTableAlias("Oldies");
var younglings = Table<PersonLite>.WithTableAlias("YoungOnes");

With.Table(
        oldies,
        Select.Column<Person>(p => new { p.Name, p.Age, })
              .From<Person>()
              .Where<Person>(p => p.Age > 40))
    .Table(
        younglings,
        Select.Column<Person>(p => new { p.Name, p.Age, })
              .From<Person>()
              .Where<Person>(p => p.Age <= 15))
    .Query(
        Select.Column(oldies, p => p.Name)
              .Column(younglings, p => p.Name)
              .From(oldies)
              .InnerJoin(oldies, younglings, (old, young) => old.Age - 30 == young.Age));
```

```sql
WITH Oldies AS (SELECT PersonRef.Name, PersonRef.Age FROM Person PersonRef WHERE PersonRef.Age > 40), 
     YoungOnes AS (SELECT PersonRef.Name, PersonRef.Age FROM Person PersonRef WHERE PersonRef.Age <= 15)
SELECT OldiesRef.Name, YoungOnesRef.Name FROM Oldies OldiesRef INNER JOIN YoungOnes YoungOnesRef ON OldiesRef.Age - 30 = YoungOnesRef.Age
```

### Basic deletes
```csharp
Delete.From<Person>().Where<Person>(p => p.Age > 42)
```

```sql
DELETE FROM Person PersonRef WHERE PersonRef.Age > 42
```

### Basic inserts
Hyperboliq lets you perform basic inserts both specifying the columns to use, and just specifying a generic "all columns" selector as seen in these examples

```csharp
var val = new Person { Id = 2, Name = "Kalle", Age = 42, LivesAtHouseId = 5, ParentId = 0 };
Insert.Into<Person>().Columns(p => new { p.Name, p.Age }).Value(val);
```

```sql
INSERT INTO Person (Name, Age) VALUES ('Kalle', 42)
```

```csharp
var val = new Person { Id = 2, Name = "Kalle", Age = 42, LivesAtHouseId = 5, ParentId = 0 };
Insert.Into<Person>().AllColumns.Value(val);
// If you use the AllColumns selector all columns will be used and will be added in alphabetical order    
```

```sql
INSERT INTO Person (Age, Id, LivesAtHouseId, Name, ParentId) VALUES (42, 2, 5, 'Kalle', 0)
```

```csharp
// It is also possible to perform multi-value inserts using Hyperboliq
var val1 = new Person { Id = 2, Name = "Kalle", Age = 42, LivesAtHouseId = 5, ParentId = 0 };
var val2 = new Person { Id = 3, Name = "Pelle", Age = 12, LivesAtHouseId = 3, ParentId = 2 };
Insert.Into<Person>()
      .AllColumns
      .Values(val1, val2);
```

```sql
INSERT INTO Person (Age, Id, LivesAtHouseId, Name, ParentId) VALUES (42, 2, 5, 'Kalle', 0), (12, 3, 3, 'Pelle', 2)
```

### Basic updates
Hyperboliq aims to allow you to create UPDATE statements that lets you update data without first selecting it (as is necessary in many other ORMs).

#### Updating to a certain static value
```csharp
Update<Person>.Set(p => new { p.Name, p.Age }, 
                   new { Name = "Kalle", Age = 42})
              .Where(p => p.Id == 5);
```

```sql
UPDATE Person SET Age = 42, Name = 'Kalle' WHERE Id = 5
```

#### Updating "in place"
```csharp
Update<Person>.Set(p => p.Age, p => p.Age + 1).Where(p => p.Name == "Kalle");
```

```sql
UPDATE Person SET Age = Age + 1 WHERE Name = 'Kalle'
```

#### Updating with subqueries
```csharp
Update<Person>.Set(p => p.Age, Select.Column<Car>(c => Sql.Max(c.Age)).From<Car>()).Where(p => p.Name == "Kalle");
```

```sql
    UPDATE Person SET Age = (SELECT MAX(CarRef.Age) FROM Car CarRef) WHERE Name = 'Kalle'
```

# Further documentation

Further documentation will be available on the github wiki pages as the API stabilizes.

Hyperboliq also maintains a large battery of unit tests testing query generation in different ways.
Until the API stabilizes enough to write decent docs, you are encouraged to look at the unit tests to see different ways to use Hyperboliq.

# Versioning

Hyperboliq aims to use semantic versioning.

# Features that will not be implemented

* Support for Linq queries.
