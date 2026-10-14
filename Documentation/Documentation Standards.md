# Documentation Standards

**Purpose:** Define a consistent header for repository documentation.  
**Version:** 1.0  
**Created:** 14 October 2026  
**Updated:** 14 October 2026  
**Status:** Active  
**Related documents:** [Documentation index](README.md)

## Document header

Repository documentation starts with a title followed by the metadata below, in this order. Include each field when it provides useful information for the document; omit fields that do not apply.

```markdown
# Document title

**Purpose:** Brief description of what the document covers.  
**Version:** Document version.  
**Created:** Original creation date.  
**Updated:** Most recent revision date.  
**Status:** Current document status.  
**Related documents:** Links to relevant documents.

Document content begins here.
```

Use a blank line between the title and metadata, and between the metadata and body. In Markdown, end metadata lines with two spaces to keep each field on its own rendered line.

## Field conventions

| Field | Content |
|---|---|
| Purpose | A short statement of the document's scope or intended use. |
| Version | The document's version, distinct from the game's release version. Useful for design documents and specifications; optional for indexes and READMEs. |
| Created | The original creation date, retained across revisions. Omit if unknown or not useful. |
| Updated | The date of the most recent content revision. |
| Status | The document's current state, such as Draft, Active, Deprecated or Archived. Add a qualifier where useful, such as Pre-production design draft. Optional when status adds little value. |
| Related documents | Descriptive links to relevant documentation, using relative paths within the repository. Omit when there are no useful related documents. |

Use dates in the form **14 October 2026**. Keep the field names and order consistent when omitting fields. Remove unused fields rather than leaving empty labels or placeholder values. Update metadata as the document changes; do not invent unknown dates or versions.
