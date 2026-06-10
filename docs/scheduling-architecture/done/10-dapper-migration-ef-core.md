# Dapper Migration — EF Core Owned Entity Workaround

**Feature Type:** Infrastructure — Repository Refactor  
**Research Reference:** `docs\scheduling-architecture\research-document-base\DAPPER-MIGRATION-SUMMARY.md`  
**Phase:** 1  
**Status:** ✅ Implemented

---

## Description

`ClassSchedule` is configured as an `OwnsMany` owned entity type in EF Core, which means it has no top-level `DbSet<ClassSchedule>()`. Attempting to use `_dbContext.Set<ClassSchedule>()` for conflict detection queries fails with:

```
System.InvalidOperationException: Cannot create a DbSet for 'ClassSchedule'
because it is configured as an owned entity type and must be accessed through
its owning entity type 'ClassSectionSubjectOffering'.
```

## Solution

All 5 conflict detection methods were converted to Dapper raw SQL queries, bypassing EF Core's owned entity restrictions entirely.

## Benefits

- ✅ Fixes EF Core owned entity error
- ✅ No EF Core configuration changes needed
- ✅ Explicit SQL is easier to debug and optimize
- ✅ Better performance (no EF Core translation overhead)
- ✅ Follows existing `IDbConnectionFactory` pattern

## Key Files

| File | Purpose |
|------|---------|
| `Enrollify.Infrastructure/Repositories/ClassSectionSubjectOfferingRepository.cs` | Dapper query implementations |
| `Enrollify.Infrastructure/Data/Config/ClassSectionSubjectOfferingConfigs/ClassSectionSubjectOfferingConfiguration.cs` | EF Core `OwnsMany` config (unchanged) |
