> **Disclaimer:** This project is an experimental, work-in-progress prototype built with the help of "vibe coding". Things will break. Features are currently missing, and the build scripts might not work at all. Please be aware that it may not be stable enough for production use now.

# LightMapper

Compile-time object mapper for .NET powered by Roslyn source generators. No reflection on the mapping hot path.

## Install

Reference the `LightMapper` NuGet package (or project-reference this repo). The analyzer is included automatically.

## Quick start

1. Mark your **source** type with `[LightMap(typeof(YourDto))]` (use `Bidirectional = true` for reverse maps).
2. Declare both types as **`partial`** in the same assembly.
3. Call generated APIs from `LightMapper.Generated`.

```csharp
using LightMapper;
using LightMapper.Generated;

[LightMap(typeof(UserDto), Bidirectional = true)]
public sealed partial class User
{
    public int Id { get; set; }
    public string Email { get; set; } = "";
}

public sealed partial class UserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = "";
}

var dto = Maps.Map<User, UserDto>(user);
Maps.MapTo(user, existingDto);
```

See [docs/PROGRAMMING_GUIDE.md](docs/PROGRAMMING_GUIDE.md) for the full guide.

## Attributes

| Attribute | Purpose |
|-----------|---------|
| `[LightMap(typeof(TDest), Bidirectional = true)]` | Declares a mapping from the annotated type to `TDest`. |
| `[LightMapFrom("SourceName")]` | Map a destination member from a differently named source member. |
| `[LightMapIgnore]` | Skip a destination property during mapping. |

## Optional dependency injection

Add the assembly attribute and register mappers:

```csharp
[assembly: GenerateLightMapperServiceRegistrations]

services.AddLightMapperMappers();
// ILightMapper<User, UserDto>
```

## Performance tips

- Prefer **`Maps.Map`** or direct generated `Map_*` methods over large generic dispatch tables when mapping in tight loops.
- Use **`Maps.MapTo`** to reuse destination instances and reduce allocations.
- Implement **`ILightMapperAfterMap<TDestination>`** only when you need custom logic; it is invoked via a type test, not reflection.

## Diagnostics

| Id | Meaning |
|----|---------|
| LM001 | Invalid `[LightMap]` destination type |
| LM002 | Incompatible member types |
| LM003 | `[LightMapFrom]` names a missing source member |

## License

MIT
