# Resumen actual del proyecto

## 1. Identidad del proyecto

- **Nombre:** Un Poco Más
- **Tipo:** aplicación móvil/juego diario con .NET MAUI y Blazor Hybrid.
- **Framework:** .NET 10.
- **Proyecto MAUI:** `UnPocoMas.Maui/UnPocoMas.Maui.csproj`.
- **Proyecto de interfaz:** `UnPocoMas.UI/UnPocoMas.UI.csproj`.
- **Proyecto de lógica:** `UnPocoMas.Core/UnPocoMas.Core.csproj`.
- **Proyecto de pruebas:** `UnPocoMas.Core.Tests/UnPocoMas.Core.Tests.csproj`.
- **Solución:** `UnPocoMas.slnx`.

## 2. Estado actual

El proyecto compila correctamente para:

- Windows: `net10.0-windows10.0.19041.0`.
- Android: `net10.0-android`.
- UI: `net10.0`.

Las pruebas actuales están en verde:

- **25 pruebas totales.**
- **25 correctas.**
- **0 fallidas.**
- **0 omitidas.**

La aplicación Windows se ha iniciado correctamente mediante el ejecutable generado:

- Proceso: `UnPocoMas.Maui`.
- Ventana: `Un Poco Más`.
- Estado: respondiendo.

## 3. Dependencias de desarrollo Android

Se han instalado y verificado:

- Android SDK: `C:\Android\Sdk`.
- Java JDK: `C:\Android\Jdk`.
- Java: OpenJDK `17.0.14 LTS`.
- Plataforma Android: `android-36`.
- Build-tools: `36.0.0`.

Variables de usuario configuradas:

```text
ANDROID_HOME=C:\Android\Sdk
ANDROID_SDK_ROOT=C:\Android\Sdk
JAVA_HOME=C:\Android\Jdk
```

El build Android Debug funciona correctamente. Todavía no se ha configurado:

- un emulador Android;
- un dispositivo físico;
- firma de producción;
- keystore;
- generación de `.aab` firmado.

## 4. Arquitectura

### `UnPocoMas.Core`

Contiene la lógica independiente de la interfaz:

- generación determinista del reto;
- cálculo de multiplicadores;
- curvas de caída;
- cálculo de puntuación;
- modelos de rondas y resultados;
- configuración del juego;
- reglas de éxito y fallo.

La lógica se ha mantenido separada de MAUI y de Razor.

### `UnPocoMas.UI`

Contiene la interfaz Blazor Hybrid y los servicios de presentación:

- páginas Razor;
- estado del juego diario;
- carga y persistencia del estado;
- estadísticas;
- construcción del texto para compartir;
- abstracciones de compartir, vibración y sonido;
- integración con el ciclo de vida mediante interfaces.

### `UnPocoMas.Maui`

Contiene el host nativo:

- arranque MAUI;
- ventana principal;
- `BlazorWebView`;
- almacenamiento mediante `Preferences`;
- integración del ciclo de vida Windows/Android;
- implementaciones temporales de compartir, vibración y sonido.

## 5. Juego diario

El juego está organizado como un reto diario de cinco rondas.

Flujo actual:

1. Se carga el reto actual.
2. El usuario inicia una ronda.
3. Se muestra una cuenta atrás.
4. Comienza el multiplicador dinámico.
5. El jugador pulsa **RETIRAR** antes de la caída.
6. Se calcula el resultado y la puntuación.
7. Se guarda la ronda.
8. Se inicia la siguiente ronda automáticamente.
9. Tras cinco rondas se muestra el resumen diario.

El botón de retirada conserva el manejador Razor existente:

```razor
@onclick="RetireAsync"
```

No se ha cambiado la lógica de retirada.

## 6. Modo temporal de pruebas

Actualmente el cambio de reto está configurado temporalmente para producirse cada minuto en vez de cada día.

La fecha lógica se deriva de los minutos UTC. Esto permite probar rápidamente el flujo completo:

- completar el reto;
- ver el resumen;
- esperar el siguiente minuto;
- cargar un nuevo reto;
- reiniciar el estado diario;
- volver a jugar.

El modo está implementado en:

- `UnPocoMas.UI/Services/IUtcDateProvider.cs`.
- `UnPocoMas.UI/Services/DailyGameService.cs`.
- `UnPocoMas.UI/Pages/Summary.razor` para la cuenta atrás.

Este comportamiento es temporal y debe revertirse antes de producción para volver a un reto por día natural UTC.

## 7. Persistencia

Se persisten los siguientes elementos:

- configuración;
- estadísticas;
- reto activo;
- resultado diario.

La persistencia valida que los datos correspondan al reto y periodo actual. Si los datos no son válidos, el estado se reinicia de forma segura.

También se controla:

- restauración de una partida parcial;
- bloqueo de duplicación de estadísticas;
- continuidad y reinicio de racha;
- mejor puntuación;
- eficiencia media;
- resultados de rondas.

## 8. Puntuación y rondas

Cada ronda tiene:

- perfil de curva;
- multiplicador de caída;
- tiempo de caída;
- señales reales o falsas cuando corresponda;
- puntos base;
- resultado de retirada.

El jugador debe retirar antes de la caída. El objetivo visual mostrado actualmente es:

```text
FRENA ANTES DE [90 % del multiplicador de caída]×
```

La referencia del 90 % coincide con la clasificación de resultado utilizada por la lógica de puntuación.

## 9. Diseño visual actual

El diseño se ha rediseñado con una estética arcade móvil:

- fondo oscuro;
- superficies elevadas;
- acento violeta;
- estados de éxito, advertencia y peligro;
- tarjetas redondeadas;
- botones grandes;
- navegación inferior;
- diseño centrado para pantallas móviles;
- tipografía clara y contrastada;
- indicadores de progreso;
- multiplicadores grandes;
- botón RETIRAR destacado.

Las clases visuales utilizan principalmente el prefijo propio:

```text
upm-*
```

No se cargan clases Bootstrap desde el markup fuente ni desde `index.html`. Los archivos físicos de Bootstrap todavía existen y no se han eliminado.

## 10. Navegación

La navegación inferior contiene tres destinos:

- Inicio: `/`.
- Estadísticas: `/statistics`.
- Ajustes: `/settings`.

Durante `/round` la navegación inferior no se muestra para evitar distraer o ocupar espacio durante la partida.

## 11. Accesibilidad y responsive

Se han aplicado estas mejoras:

- viewport sin bloqueo de zoom;
- soporte de `viewport-fit=cover`;
- safe areas superior e inferior;
- variables CSS reutilizables para navegación;
- fallback `100vh` y mejora `100dvh`;
- objetivos táctiles grandes;
- foco visible;
- soporte de `prefers-reduced-motion`;
- textos funcionales con tamaño mínimo aproximado de `0.875rem`;
- media query para pantallas de poca altura;
- media query para landscape;
- overflow vertical como fallback accesible;
- grid de ronda adaptable.

Variables principales:

```css
--upm-nav-height: 68px;
--upm-safe-top: env(safe-area-inset-top, 0px);
--upm-safe-bottom: env(safe-area-inset-bottom, 0px);
--upm-nav-total-height: calc(var(--upm-nav-height) + var(--upm-safe-bottom));
```

## 12. Pantalla de ronda actual

La pantalla de ronda muestra:

- número de ronda;
- puntos acumulados;
- progreso de las cinco rondas;
- cuenta atrás inicial;
- objetivo de retirada;
- multiplicador actual;
- botón RETIRAR;
- resultado temporal de cada ronda.

El layout utiliza una cuadrícula adaptable y reduce tamaños en:

- pantallas de poca altura;
- orientación horizontal;
- ventanas compactas.

La navegación inferior no aparece durante la ronda.

## 13. Resultado y resumen

Al terminar cada ronda se muestra temporalmente:

- símbolo del resultado;
- estado de la retirada;
- puntos obtenidos;
- multiplicador al que habría caído;
- aviso de la siguiente ronda.

Al completar las cinco rondas se muestra:

- puntuación total;
- eficiencia;
- detalle de cada ronda;
- próximo reto;
- opción para compartir el resultado;
- opción para volver al inicio.

## 14. Ciclo de vida

La aplicación contempla el ciclo de vida:

- pausa/background;
- reanudación/resume;
- cancelación de cuenta atrás si la aplicación pasa a segundo plano;
- reinicio controlado de la cuenta atrás al volver;
- cancelación y liberación de recursos al salir de la pantalla.

El tiempo oficial de la ronda procede de `Stopwatch.Elapsed`. La finalización se protege para evitar resultados duplicados.

## 15. Compartir, vibración y sonido

Actualmente existen abstracciones para:

- compartir resultados;
- vibración/haptics;
- sonido.

En MAUI todavía hay implementaciones temporales o simuladas para algunas de estas capacidades. La implementación real de compartir y vibración queda pendiente.

## 16. Validaciones realizadas

Validaciones correctas realizadas durante el desarrollo:

```powershell
dotnet build .\UnPocoMas.UI\UnPocoMas.UI.csproj
```

```powershell
dotnet test .\UnPocoMas.Core.Tests\UnPocoMas.Core.Tests.csproj
```

Resultado:

```text
25 pruebas correctas
0 errores
```

```powershell
dotnet build .\UnPocoMas.Maui\UnPocoMas.Maui.csproj -f net10.0-windows10.0.19041.0
```

```powershell
dotnet build .\UnPocoMas.Maui\UnPocoMas.Maui.csproj -f net10.0-android -p:AndroidSdkDirectory=C:\Android\Sdk -p:JavaSdkDirectory=C:\Android\Jdk
```

Ambos builds se han completado correctamente.

## 17. Pendientes conocidos

- Revertir el modo temporal de cambio cada minuto.
- Probar visualmente en un dispositivo Android real o emulador.
- Verificar safe areas en Android WebView.
- Verificar pausas y reanudaciones reales en Android.
- Implementar compartir real.
- Implementar vibración real.
- Implementar sonido real si se mantiene como requisito.
- Definir el `ApplicationId` definitivo antes de publicar.
- Crear y proteger un keystore de producción.
- Generar un `.aab` firmado sólo cuando el flujo esté validado.
- Ejecutar pruebas de accesibilidad y texto ampliado al 200 %.
- Medir visualmente la ronda en `360 × 800`, `360 × 640` y landscape `800 × 360`.

## 18. Próximas direcciones posibles

El siguiente paso puede centrarse en una de estas áreas:

1. **Android:** crear emulador o conectar dispositivo y probar la aplicación.
2. **Juego:** ajustar dificultad, curvas, puntuación y sensación de riesgo.
3. **Diseño:** seguir refinando la ronda y la visibilidad del objetivo.
4. **Persistencia:** probar restauración y cambio de periodo en más escenarios.
5. **Funciones reales:** compartir, vibración y sonido.
6. **Producción:** definir identidad final, ApplicationId, iconos, firma y publicación.
7. **Calidad:** ampliar tests de UI, accesibilidad y ciclo de vida.

## 19. Nota importante

El modo de un reto por minuto está activado únicamente para pruebas. Antes de distribuir la aplicación debe restaurarse el comportamiento oficial de un reto diario basado en UTC y revisarse la cuenta atrás del resumen.
