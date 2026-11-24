# Detail: csproj-Umbaupunkte (Ist → Soll)

## `src/Schnittstellenzentrale.Infrastructure/Schnittstellenzentrale.Infrastructure.csproj`

**Ist (Zeilen 7–38, Auszug):**

```xml
<ItemGroup>
  <PackageReference Include="Jint" Version="4.9.2" />
  <PackageReference Include="Markdig" Version="1.2.0" />
  <PackageReference Include="Microsoft.AspNetCore.Http.Abstractions" Version="2.3.0" />
  <PackageReference Include="Microsoft.AspNetCore.SignalR.Client" Version="11.0.*" />
  <PackageReference Include="Microsoft.AspNetCore.SignalR.Core" Version="1.2.10" />
  <PackageReference Include="Microsoft.EntityFrameworkCore" Version="11.0.*" />
  ...
</ItemGroup>
```

**Soll:**

```xml
<ItemGroup>
  <FrameworkReference Include="Microsoft.AspNetCore.App" />
</ItemGroup>

<ItemGroup>
  <PackageReference Include="Jint" Version="4.9.2" />
  <PackageReference Include="Markdig" Version="1.2.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore" Version="11.0.*" />
  ...
</ItemGroup>
```

- Die drei Zeilen 10–12 entfallen vollständig.
- FrameworkReference in eigener ItemGroup (Konvention: Framework-/Projekt-Referenzen von Paket-Referenzen getrennt, wie bereits bei `ProjectReference` oben gehandhabt).

## `src/Schnittstellenzentrale/Schnittstellenzentrale.csproj`

**Ist (Zeilen 14–26):** PackageReference-Liste beginnt mit `Microsoft.AspNetCore.Authentication.Negotiate`, `Microsoft.AspNetCore.OData`, …

**Soll:** eine Zeile ergänzen, alphabetisch einsortiert:

```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.Negotiate" Version="11.0.*" />
<PackageReference Include="Microsoft.AspNetCore.OData" Version="10.0.0-preview.2" />
<PackageReference Include="Microsoft.AspNetCore.SignalR.Client" Version="11.0.*" />
```

(Hinter `Microsoft.AspNetCore.OData`, vor `Microsoft.EntityFrameworkCore.Design` — exakte alphabetische Position: `Authentication.Negotiate` < `OData` < `SignalR.Client`.)

## Unverändert

- `GenerateDocumentationFile`, `WarningsAsErrors`, `Nullable`, `ImplicitUsings`, `TargetFramework` in allen csproj.
- Alle übrigen PackageReferences.
- Kein Quellcode ändert sich — `using Microsoft.AspNetCore.SignalR;`/`Microsoft.AspNetCore.Http;` bleiben, nur die Herkunft der Assemblies wechselt (NuGet → Shared Framework).

## Verifikationsschritte nach Umbau

1. `dotnet restore` + `dotnet build -p:SkipPlaywrightInstall=true` → 0 Fehler.
2. `dotnet list src/Schnittstellenzentrale.Infrastructure package` → weder `Http.Abstractions` noch `SignalR.Core`/`SignalR.Client` als Referenz.
3. `dotnet list src/Schnittstellenzentrale package` → `Microsoft.AspNetCore.SignalR.Client` aufgeführt.
4. `dotnet test --filter "Category!=OsInterface" -p:SkipPlaywrightInstall=true` → vollständig grün (inkl. `TopBarTests`, `AppShellTests`).
5. E2E: App unter Kestrel starten, `GET /` → 200.
