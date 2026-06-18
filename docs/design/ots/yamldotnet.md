## YamlDotNet OTS Design

YamlDotNet is the YAML parsing and deserialization library used by FileAssert.

### Purpose

YamlDotNet is chosen to deserialize the `.fileassert.yaml` configuration into the Configuration
DTOs and to parse arbitrary YAML documents under test for `yaml:` dot-notation path assertions. It
provides a mature, well-supported YAML 1.1/1.2 implementation for .NET, removing the need for a
hand-written parser.

### Features Used

- Object deserialization of YAML into strongly-typed DTOs via the deserializer with member aliasing.
- Parsing of arbitrary YAML documents into a node graph for dot-notation path evaluation.
- Detection of malformed YAML, surfaced as parse exceptions that FileAssert converts into errors.

### Integration Pattern

YamlDotNet is referenced as a NuGet package by the main `DemaConsulting.FileAssert` project. The
Configuration subsystem constructs a deserializer to map YAML onto DTO types, and the YAML asserter
loads documents under test through the same library. Parse exceptions are caught at the asserter
boundary and reported through the context rather than propagating to the caller.
