# QT: Termination — mod para RimWorld

Inspirado en el mod "QT" de Friday Night Funkin' (uno de los charts más brutales de esa comunidad).

## Qué hace
1. **Incidente `QT_Visit`**: cada ~15 días (a partir del día 6) puede aparecer QT como visitante amistosa.
2. **`Hediff_QTCore`**: vigila si QT entra en combate o pierde salud, y va escalando fases automáticamente:
   - **Carefree** (cosmético, sin peligro)
   - **Careless** (+velocidad, +precisión cuerpo a cuerpo)
   - **Sensory Overload** (pulso de área cada ~4s que aturde a los colonos cercanos, -precisión de disparo)
   - **Termination** (carga final)
3. **`Hediff_TerminationCharge`**: durante la fase final, una barra sube durante ~7s. En el último ~1.5s aparece
   un **Gizmo global "¡ESQUIVAR!"** (visible en la barra de comandos de cualquier colono, sin necesitar tener
   seleccionada a QT) — si lo pulsas dentro de esa ventana, la carga se reinicia; si no, detona una explosión
   grande centrada en QT.

Esto traduce la idea de "juego de ritmo con timing exigente" a una mecánica de RimWorld sin necesitar un
sistema de notas completo: la tensión viene de vigilar la barra y reaccionar en el momento justo.

## Sobre los errores del log

Si ves errores tipo `Could not find a type named QTTermination.Hediff_QTCore` es **normal y esperado**
hasta que compiles el `.dll` (paso siguiente) y lo coloques en `1.5/Assemblies/`. RimWorld carga primero
los XML y falla al no encontrar las clases porque el ensamblado aún no existe.

## Compilar

1. Instala el SDK de .NET (o usa Visual Studio / Rider).
2. Abre `Source/QTTermination/QTTermination.csproj` y ajusta `RimWorldInstallDir` a tu ruta real de RimWorld
   (o pásalo por línea de comandos: `dotnet build -p:RimWorldInstallDir="D:\SteamLibrary\...\RimWorld"`).
3. Verifica dónde tienes `0Harmony.dll` — normalmente viene con el mod "Harmony" de brrainz (Steam Workshop)
   en `Mods\Harmony\Current\Assemblies\0Harmony.dll` en vez de `Mods\Core\Assemblies`. Ajusta el `HintPath`
   si hace falta.
4. Compila. El `.dll` resultante se coloca automáticamente en `1.6/Assemblies/QTTermination.dll` gracias al
   `OutputPath` del csproj.
5. Copia toda la carpeta `QTTermination/` (About, Defs, Languages, 1.6, Textures) a tu carpeta de Mods de RimWorld.
6. Activa "Harmony" y luego "QT: Termination" en el gestor de mods, en ese orden.

## Pendiente / ideas para pulir
- Falta textura propia de QT (usa el sprite humano genérico + apparel por ahora — puedes crear un tag de
  apparel `QT_Outfit` con una ropa a medida para darle su look).
- El sonido del "warmup" ahora mismo es solo visual (fogonazo); se puede añadir un `SoundDef` propio con
  algo tipo sirena creciente.
- Se podría añadir Mod Settings para activar/desactivar el incidente o ajustar la dificultad (ventana de
  esquive más generosa, etc.).
- Las cifras exactas de la API de `GenExplosion.DoExplosion` y `HediffCompProperties_*` pueden variar
  ligeramente entre 1.5 y 1.6 — si el compilador se queja de algún parámetro, es lo primero a revisar.
