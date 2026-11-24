# Architektur- und Prozessentscheidungen – Projekt Issue #108

Dieses Dokument sammelt die im Projektverlauf getroffenen Entscheidungen mit Begründung. Jede Entscheidung wird im zugehörigen Entwicklungsschritt ergänzt.

## F5 – OData-Stack: Edm 9.0.0 + AspNetCore.OData-Strategie (Schritt 3)

**Status:** Entschieden und umgesetzt (Schritt-Branch `…-schritt-3-odata-edm`).

**Entscheidung:**

1. **Microsoft.AspNetCore.OData: auf `10.0.0-preview.2` verharren** (dokumentierte Annahme).
   - **Begründung:** Es existiert kein stabiler 10.x-/11.x-Release von `Microsoft.AspNetCore.OData` (neueste Version zum Entscheidungszeitpunkt: `10.0.0-preview.2`; letzter stabiler Strang ist 9.x für net8.0). `10.0.0-preview.2` läuft unter net11 bereits seit Schritt 1 verifiziert (78/78 `ODataControllerIntegrationTests` grün). Ein Preview-Tracking auf neue Preview-Stände ohne Zielrelease (unbekannter Stabilisierungszeitpunkt) würde das Risiko nur erhöhen, ohne einen Zielzustand zu erreichen.
   - **Exit-Kriterium:** Umstieg auf die stabile Linie, sobald ein stabiler 10.x- oder 11.x-Release von `Microsoft.AspNetCore.OData` verfügbar ist — dann als eigener Schritt mit voller OData-Testverifikation.

2. **Microsoft.OData.Edm 8.4.3 → 9.0.0** (direkte Referenz in `Schnittstellenzentrale.Infrastructure.csproj`). Damit ist die Paketanforderung von `Microsoft.AspNetCore.OData 10.0.0-preview.2` (`>= 9.0.0-preview.4`) erfüllt und der binärfragile Versions-Split aufgehoben (vorher: kompiliert gegen 8.4.3, gelaufen transitiv gegen 9.0.0-preview.4). `dotnet list package --include-transitive` zeigt `Microsoft.OData.Edm` in allen Projekten einheitlich `9.0.0`.

3. **Ergänzend (dokumentierte Annahme, technisch erforderlich):** direkte Referenzen `Microsoft.OData.Core 9.0.0` und `Microsoft.OData.ModelBuilder 3.0.0` in `Schnittstellenzentrale.csproj`.
   - **Begründung:** `Microsoft.OData.Core 9.0.0-preview.4` pinnt `Microsoft.OData.Edm` **exakt** auf `= 9.0.0-preview.4` — ohne die Core-Hebung erzeugte die Edm-9.0.0-Referenz NU1608-Warnungen (Web- und Testprojekt) und einen Rest-Versions-Split im ODL-Stack. Die stabilen Releases erfüllen die von `AspNetCore.OData 10.0.0-preview.2` deklarierten Mindestbereiche (`Core`/`Edm`/`Spatial` `[9.0.0-preview.4, 10.0.0)`, `ModelBuilder` `[3.0.0-preview.1, 4.0.0)`). `ModelBuilder` wird außerdem direkt im Code genutzt (`OData/ODataEdmModelBuilder.cs`) — eine direkte Referenz ist ohnehin semantisch korrekt. Ergebnis: kompletter ODL-Stack (`Edm`, `Core`, `Spatial` 9.0.0, `ModelBuilder` 3.0.0) auf der stabilen Linie; einzig `AspNetCore.OData` bleibt Preview (Strategie siehe Punkt 1).

**Betrachtete Alternativen (verworfen):**

- Edm auf `9.0.0-preview.4` pinnen (Deck-Gleichheit mit der Core-Anforderung): würde das Issue-Ziel „stabile 9.0.0" verfehlen und den Preview-Status in den direkt referenzierten Code tragen.
- Fallback `8.4.4`: nur vorgesehen, wenn 9.0.0 konkrete Probleme verursacht — nicht eingetreten (Build sauber, Edm-9-API kompatibel: `CsdlReader.Parse`, `IEdmModel`/`IEdmEntitySet`/`IEdmOperation`/`IEdmAction`, Vocabulary-Annotations-API unverändert nutzbar).
- AspNetCore.OData-Downgrade auf stabilen 9.x-Strang (net8.0): verworfen — würde den seit Schritt 1 verifizierten net11-Pfad verlassen und OData-API-Downgrades riskieren.

**Konsequenzen:**

- `ODataImportService` (Infrastructure) und `ODataEdmModelBuilder` (Web) kompilieren und laufen gegen dieselbe Edm-Version — kein Laufzeit-/Kompilierzeit-Split mehr.
- Der ODL-Stack ist bis auf das Host-Paket vollständig stabil; Versionsfolge von `Core`/`Edm`/`Spatial`/`ModelBuilder` liegt jetzt bei direkten, prüfbaren Referenzen statt versteckt in transitiven Preview-Auflösungen.
- Beim Erreichen des Exit-Kriteriums ist zu prüfen, ob die ODL-Direktreferenzen weiter gepflegt oder dem `AspNetCore.OData`-Release folgen.
- `Microsoft.Spatial` bleibt transitiv (kein direkter Code-Bezug) und löst als `9.0.0` auf.

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

## F4 – Serilog-Stack: `Serilog.AspNetCore` 11.0.0, Sinks unverändert (Schritt 4)

**Status:** Entschieden und umgesetzt (Schritt-Branch `…-schritt-4-serilog`).

**Entscheidung:**

1. **`Serilog.AspNetCore` 10.0.0 → 11.0.0** in `Schnittstellenzentrale.csproj`.
   - **Changelog-Bewertung 11.0.0** (GitHub-Release vom 18.11.2026): Der Release fügt den net11.0-TFM hinzu (net8.0/net9.0 entfallen — irrelevant, das Projekt läuft auf net11.0), hebt den Serilog-Kern auf 4.4.0 und alle `Serilog.*`-Satelliten auf die v11-Linie (`Extensions.Hosting`, `Extensions.Logging`, `Settings.Configuration` je 11.0.0). Die einzige Verhaltensänderung — das Request-Logging (`UseSerilogRequestLogging`) unterdrückt Exceptions bei client-seitig abgebrochenen Requests — ist für diese Anwendung **ohne Wirkung**, weil die Middleware nicht genutzt wird (per Volltextsuche verifiziert). Kein API-Bruch auf den genutzten Pfaden (`LoggerConfiguration().ReadFrom.Configuration`, `builder.Host.UseSerilog()`, statisches `Log.Logger`); der Bootstrap-Logger-Pfad in `Program.cs` und der `Serilog`-Abschnitt in `appsettings.json` bleiben unverändert.

2. **Sinks bleiben unverändert** (dokumentierte Annahme): `Serilog.Sinks.File` 7.0.0 und `Serilog.Sinks.EventLog` 4.0.0.
   - **Begründung:** `Sinks.File` 7.0.0 ist die von `Serilog.AspNetCore` 11.0.0 selbst deklarierte Abhängigkeit (`>= 7.0.0`) — Kompatibilität per Paketdeklaration; kein neuerer stabiler Stand verfügbar (nur Prerelease). `Sinks.EventLog` 4.0.0 ist letzter stabiler Stand, targetet net6.0/netstandard2.0 und löst unter net11.0 via Nearest-TFM auf; die Serilog-4.x-Abhängigkeit ist major-kompatibel zu 4.4.0. Ein Mitheben entfällt — kein technischer Anlass.
   - **Verifiziert:** Reale E2E-Abnahme unter Kestrel — `logs/log-20261124.txt` schreibt Request-/Hub-Events, und der EventLog-Sink schreibt auf diesem Windows-Host tatsächlich Einträge unter Provider `Schnittstellenzentrale` ins Anwendungsprotokoll (Aussage stärker als das geforderte „Konfiguration wird geladen").

**Betrachtete Alternativen (verworfen):**

- Sinks vorsorglich auf Prerelease-Stände heben: verworfen — ohne konkreten Kompatibilitätsbefund würde das Prerelease-Risiko ohne Gegenwert eingeführt.
- `manageEventSource: true` in der EventLog-Konfiguration ergänzen: verworfen als Scope-Erweiterung — seit Sink-v3.0 Default `false` (Source muss auf dem Zielhost registriert sein; auf dem Abnahme-Host ist sie es), das Bestandsverhalten bleibt bewusst unverändert.

**Konsequenzen:**

- Der Serilog-Stack folgt jetzt der net11-Linie des Hostings; künftige Sink-Updates können unabhängig vom Hostpaket erfolgen (Sinks hängen nur am Serilog-Kern).
- Nachweisbarkeit des Loggings ist jetzt Teil der Schritt-Abnahme dokumentiert: File-Sink und EventLog-Sink schreiben real — bei künftigen Serilog-Updates ist die reale Kestrel-Abnahme (Datei- und EventLog-Prüfung) zu wiederholen.
