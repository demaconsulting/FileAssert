## HtmlAgilityPack OTS Design

HtmlAgilityPack is the HTML parsing library used by FileAssert.

### Purpose

HtmlAgilityPack is chosen to read HTML documents under test for `html:` XPath assertions. It parses
real-world, syntactically imperfect HTML leniently and exposes an XPath-navigable document object
model, which plain XML parsers cannot do reliably.

### Features Used

- Lenient parsing of HTML documents (including malformed markup) into a navigable document.
- XPath query evaluation to select node sets for count and text assertions.
- Access to node inner text for `contains`/exact-text rules.

### Integration Pattern

HtmlAgilityPack is referenced as a NuGet package by the main `DemaConsulting.FileAssert` project.
The HTML asserter loads the document under test through HtmlAgilityPack, evaluates the configured
XPath queries, and applies count and text rules to the matched nodes. Because parsing is lenient,
only IO failures (not parse failures) are reported as errors; IO exceptions are caught at the
asserter boundary and reported through the context.
