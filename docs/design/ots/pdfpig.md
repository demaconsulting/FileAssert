## PdfPig OTS Design

PdfPig (UglyToad.PdfPig) is the PDF parsing library used by FileAssert.

### Purpose

PdfPig is chosen to read PDF documents under test for `pdf:` assertions. It provides managed,
dependency-free PDF parsing for .NET, exposing page counts, document metadata, and per-page text
without requiring native libraries.

### Features Used

- Opening a PDF document from a stream and enumerating its pages for page-count assertions.
- Reading document information (metadata) fields such as title and author.
- Extracting page text for `contains`/`matches` content rules.
- Detection of files that are not valid PDF documents, surfaced as exceptions.

### Integration Pattern

PdfPig is referenced as a NuGet package by the main `DemaConsulting.FileAssert` project. The PDF
asserter opens the document under test through PdfPig, reads the required page, metadata, and text
information, and applies the configured rules. Exceptions raised for invalid PDF input are caught at
the asserter boundary and reported through the context.
