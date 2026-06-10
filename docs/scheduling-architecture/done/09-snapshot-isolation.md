# SNAPSHOT Isolation — Database-Wide

**Feature Type:** Infrastructure — Database  
**Research Reference:** `docs\scheduling-architecture\research-document-base\IMPLEMENTATION-SUMMARY.md:202-203`  
**Phase:** 0 (Pre-existing)  
**Status:** ✅ Implemented

---

## Description

SNAPSHOT isolation level is already enabled database-wide in the project's initial migration script (`Script0000`). This provides:

- **Consistent reads:** Queries see a snapshot of committed data as of the start of the transaction
- **No reader/writer blocking:** Readers don't block writers and writers don't block readers
- **TOCTOU race mitigation:** While the conflict check-and-insert is not wrapped in a single `SERIALIZABLE` transaction, SNAPSHOT isolation reduces the window for dirty reads

## Design Note

The research doc (implementation guide §8) recommends wrapping conflict check + insert in a `SERIALIZABLE` transaction for full TOCTOU protection. This is **not yet implemented**. The current design relies on SNAPSHOT isolation + application-layer checks, which is acceptable given that university scheduling changes are infrequent admin operations.

## Key Files

| File | Purpose |
|------|---------|
| `Enrollify.DatabaseMigration/Scripts/Script0000__Initial.sql` | Database-wide SNAPSHOT isolation setting |
