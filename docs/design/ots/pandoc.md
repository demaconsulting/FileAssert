## Pandoc OTS Design

DemaConsulting.PandocTool is a .NET dotnet global tool wrapper around the Pandoc document
converter. It converts Markdown source documents into HTML as part of the documentation build
pipeline, producing the intermediate output that WeasyPrint then renders to PDF.

### Purpose

Pandoc converts the ordered set of Markdown content files for each document collection into a
single HTML document using a project-specific HTML template. The HTML output is then rendered
to PDF by WeasyPrint. Pandoc is used for all document collections: Build Notes, Code Quality,
Review Plan, Review Report, Design, Verification, and User Guide.

Pandoc is chosen because it handles multi-file document assembly natively, supports a custom
HTML template for consistent styling, and produces numbered sections and table-of-contents
entries without additional tooling.

### Integration

Pandoc is installed as a .NET local tool via the package `demaconsulting.pandoctool` in
`.config/dotnet-tools.json` and restored with `dotnet tool restore`. The tool is invoked as
`dotnet pandoc` with a `definition.yaml` argument that lists the input Markdown files, template,
and output path. Each document collection provides its own `definition.yaml`.
Version constraint: `3.9.0.2` (pinned in `.config/dotnet-tools.json`).

### Configuration

Each document collection provides a `definition.yaml` file at `docs/{collection}/definition.yaml`
that specifies:

- `resource-path` — directories containing the HTML template and CSS assets
- `input-files` — ordered list of Markdown source files to concatenate
- `template` — the shared `template.html` located in `docs/template/`
- `table-of-contents: true` — generates a navigation table of contents
- `number-sections: true` — produces numbered headings in the output HTML

### Interfaces

The project uses the following Pandoc command-line interface:

| Invocation                                     |
| :--------------------------------------------- |
| `dotnet pandoc --definition <definition.yaml>` |

The generated HTML file is placed in `docs/{collection}/generated/{collection}.html` and is
passed directly to WeasyPrint for PDF conversion.

### Dependencies

The PandocTool wrapper bundles the Pandoc executable internally; no separate Pandoc installation
is required. The project supplies a `docs/template/` directory containing the shared
`template.html` and CSS assets used by all document collections. No additional runtime
dependencies are required beyond the tool installation.
