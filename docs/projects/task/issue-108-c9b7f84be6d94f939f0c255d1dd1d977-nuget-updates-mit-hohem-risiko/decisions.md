# Architektur- und Prozessentscheidungen – Projekt Issue #108

Dieses Dokument sammelt die im Projektverlauf getroffenen Entscheidungen mit Begründung. Jede Entscheidung wird im zugehörigen Entwicklungsschritt ergänzt.

## F6 – ASP.NET-Core-Bezug in `Schnittstellenzentrale.Infrastructure` (Schritt 2)

**Status:** Entschieden und umgesetzt (Schritt-Branch `…-schritt-2-frameworkref`).

**Entscheidung:** Volle `FrameworkReference`-Variante. `Schnittstellenzentrale.Infrastructure.csproj` erhält `<FrameworkReference Include="Microsoft.AspNetCore.App" />`; die PackageReferences `Microsoft.AspNetCore.Http.Abstractions 2.3.0` und `Microsoft.AspNetCore.SignalR.Core 1.2.10` entfallen. Zusätzlich – unabhängig von der Variante verbindlich – wird `Microsoft.AspNetCore.SignalR.Client 11.0.*` aus `Infrastructure` ins Web-Projekt `src/Schnittstellenzentrale/Schnittstellenzentrale.csproj` verschoben.

**Begründung:**

- Die in `Infrastructure` tatsächlich genutzten ASP.NET-Core-APIs liegen vollständig im Shared Framework: `Hub` und `IHubContext<THub>` (`SignalRNotificationService<THub>`, einzige `Microsoft.AspNetCore.SignalR`-Nutzung) sowie `IHttpContextAccessor` (`WindowsCurrentUserService`, einzige `Microsoft.AspNetCore.Http`-Nutzung). Per Volltextsuche verifiziert: keine weiteren `Microsoft.AspNetCore.*`-usings in `Infrastructure`, keine in `Core`.
- Zwei Legacy-Paketlinien entfallen: `Http.Abstractions 2.3.0` (.NET-Core-2.x-Linie, wird nicht mehr separat gepflegt) und `SignalR.Core 1.2.10` (1.x-Linie). Der ohnehin fällige faktische Major-Sprung `SignalR.Core` 1.2.10 → Shared-Framework-Version (11.0.x) wird sauber auf die Framework-Version vollzogen statt als verwaiste Paketlinie weiterverwaltet.
- `Microsoft.AspNetCore.SignalR.Client` ist **nicht** Teil des Shared Frameworks und wird produktiv ausschließlich im Web-Projekt genutzt (`Components/Layout/AppShell.razor`, `Components/Shared/ApplicationGroupTree.razor`). Die Referenz gehört daher dorthin, wo sie gebraucht wird — `Infrastructure` hatte sie nur historisch mitgeschleppt.
- Das Testprojekt (`Microsoft.NET.Sdk`, bunit) erhält `Microsoft.AspNetCore.App` transitiv über die `ProjectReference`n (FrameworkReference ist transitiv; das Web-SDK-Projekt liefert sie ohnehin implizit mit) und `SignalR.Client` transitiv über die Web-Referenz — bunit-Verträglichkeit wurde per Build und kompletter regulärer Testlane verifiziert.

**Betrachtete Alternative (verworfen):** Minimalvariante — nur `SignalR.Client` verschieben und die beiden Legacy-Pakete in `Infrastructure` belassen. Verworfen, weil sie die verwaisten Paketlinien (`Http.Abstractions 2.3.0`, `SignalR.Core 1.2.10`) konserviert und den fälligen SignalR-Major-Sprung weiter aufschiebt; das Restrisiko der vollen Variante (bunit-/Testprojekt-Verträglichkeit) ist gering und wurde im Schritt verifiziert.

**Konsequenzen:**

- `Infrastructure` hängt zur Laufzeit an der ASP.NET-Core-Shared-Framework-Installation (bei Blazor-Server-Einsatz ohnehin immer gegeben); als class library ändert sich am Kompilat nichts.
- Die SignalR-Server-Version in `Infrastructure` folgt künftig der Runtime statt einer pinnenbaren Paketversion — gewollt, da das Hosting-Projekt dieselbe Runtime vorgibt.
- Security-Update-Pfad für diese APIs läuft über Runtime-/SDK-Patches statt über NuGet-Paketupdates — passt zur Floating-/Patch-Strategie der übrigen Framework-Pakete.
- **Folgebereinigung (NU1510):** Mit der `FrameworkReference` wurden außerdem die bis dahin direkt referenzierten, nun vollständig im Shared Framework enthaltenen Pakete `Microsoft.Extensions.Configuration.Abstractions`, `Microsoft.Extensions.Configuration.Binder`, `Microsoft.Extensions.DependencyInjection.Abstractions`, `Microsoft.Extensions.Http`, `Microsoft.JSInterop` und `System.Security.Cryptography.Pkcs` als redundant gemeldet und entfernt — sonst blieben dauerhaft NU1510-Warnungen im Build. Zur Pkcs-Sicherheitslage (GHSA-846f-f764-76mx, GHSA-2fmv-rv2c-59x7): Die ursprüngliche Pinning-Begründung entfällt — Pkcs kommt jetzt aus der Runtime-Installation und bleibt über deren Patches auf der gepatchten Linie; die transitive `Microsoft.EntityFrameworkCore.SqlServer`-Anforderung wird auf das Framework-Asset gekürzt.
