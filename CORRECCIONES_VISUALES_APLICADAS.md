# Correcciones visuales aplicadas

## Alcance

Se aplicaron las correcciones mínimas únicamente en:

- `UnPocoMas.Maui/wwwroot/index.html`
- `UnPocoMas.Maui/Components/Layout/NavMenu.razor`
- `UnPocoMas.Maui/wwwroot/app.css`

No se modificaron Core, servicios, persistencia, ciclo de vida, temporizadores, puntuación, navegación funcional ni manejadores Razor.

## 1. `index.html`

Meta viewport final:

```html
<meta name="viewport" content="width=device-width, initial-scale=1.0, viewport-fit=cover" />
```

También se eliminó la etiqueta `</html>` duplicada.

Bootstrap no se carga desde `index.html`. No se eliminaron archivos físicos de Bootstrap.

## 2. `NavMenu.razor`

Contenido final completo:

```razor
<nav class="upm-bottom-nav" aria-label="Navegación principal">
	<NavLink href="" Match="NavLinkMatch.All" aria-label="Ir a Inicio">
		<span class="upm-nav-icon" aria-hidden="true">⌂</span>
		<span>Inicio</span>
	</NavLink>
	<NavLink href="statistics" aria-label="Ir a Estadísticas">
		<span class="upm-nav-icon" aria-hidden="true">◒</span>
		<span>Estadísticas</span>
	</NavLink>
	<NavLink href="settings" aria-label="Ir a Ajustes">
		<span class="upm-nav-icon" aria-hidden="true">⚙</span>
		<span>Ajustes</span>
	</NavLink>
</nav>
```

Se eliminó todo el markup heredado: `top-row`, `navbar`, `navbar-toggler`, `container-fluid`, `nav-scrollable`, enlaces antiguos, JavaScript y clases `bi-*`.

## 3. Variables y reserva de navegación

Se añadieron:

```css
--upm-nav-height: 68px;
--upm-safe-top: env(safe-area-inset-top, 0px);
--upm-safe-bottom: env(safe-area-inset-bottom, 0px);
--upm-nav-total-height: calc(var(--upm-nav-height) + var(--upm-safe-bottom));
```

La navegación usa:

```css
min-height: var(--upm-nav-height);
padding-bottom: var(--upm-safe-bottom);
```

Las páginas reservan:

```css
padding-bottom: calc(var(--upm-nav-total-height) + 20px);
```

Con una safe area inferior de `48px`:

- navegación total: `68px + 48px = 116px`;
- reserva mínima del contenido: `116px + 20px = 136px`.

No se mantiene el valor fijo `104px`.

## 4. Cálculos verticales

### 360 × 800

Para una safe area superior de `0px` e inferior de `0px`:

- safe area superior: `0px`;
- padding superior de ronda: `16px`;
- encabezado: mínimo `82px`;
- gaps: `8px + 8px = 16px`;
- zona central de referencia: `490px`;
- zona de acción: mínimo `180px`;
- padding inferior: `16px`;
- safe area inferior: `0px`.

Total:

```text
16 + 82 + 8 + 490 + 8 + 180 + 16 = 800px
```

Esta suma usa la zona central de referencia y demuestra que la distribución base ocupa 800px. No demuestra por sí sola el encaje final: `minmax(180px, 38%)` puede hacer que la fila de acción mida aproximadamente el 38% del área interior, unos 292px en este viewport. La fila central `minmax(0, 1fr)` debe absorber la diferencia, siempre que su contenido no imponga una altura mínima superior. La comprobación de ausencia de scroll requiere validación visual o medición efectiva del layout. El overflow es `auto` como fallback accesible y el botón conserva sus `180px` mínimos.

### 412 × 915

Con la misma configuración:

- padding superior/inferior: `16px + 16px`;
- encabezado mínimo: `82px`;
- gaps: `8px + 8px = 16px`;
- acción mínima: `180px`;
- zona central disponible: `915 - 16 - 82 - 16 - 180 - 16 = 605px`.

La altura disponible es suficiente para una ronda normal sin recorte.

### 360 × 640

Se activa `@media (max-height: 700px)`:

- padding vertical de ronda: `8px`;
- encabezado mínimo: `64px`;
- gaps: `4px`;
- acción mínima: `140px`;
- zona central disponible aproximada: `640 - 8 - 64 - 8 - 140 - 8 = 412px`.

Se conserva el objetivo táctil mínimo de 140 px para RETIRAR en esta condición.

### 800 × 360, orientación horizontal

Se activa:

```css
@media (orientation: landscape) and (max-height: 600px)
```

Se aplican:

- padding vertical de `6px`;
- encabezado mínimo de `52px`;
- gaps de `4px`;
- zona de acción mínima de `120px`;
- multiplicador y cuenta atrás reducidos mediante `clamp()`;
- navegación inferior ausente durante `/round` porque `MainLayout.razor` ya la excluye funcionalmente para esa ruta.

### Safe area inferior de 48 px

La navegación ocupa:

```text
68px + 48px = 116px
```

El contenido normal reserva:

```text
116px + 20px = 136px
```

La safe area no se resta de nuevo dentro de la altura de `.upm-frame--round`.

### Texto ampliado al 200 %

Se eliminó el bloqueo de zoom del viewport. Los textos funcionales tienen como mínimo `0.875rem`, y se usan `clamp()` en:

- multiplicador;
- cuenta atrás;
- título;
- puntuación principal.

La ronda usa `overflow-y: auto` como fallback accesible. No se oculta el botón RETIRAR mediante `overflow: hidden`.

## 5. Viewport dinámico

`.upm-shell` y `.upm-frame` usan:

```css
min-height: 100vh;
min-height: 100dvh;
```

La ronda usa:

```css
height: 100vh;
height: 100dvh;
```

El primer valor es el fallback compatible. El segundo adapta la altura al viewport dinámico de navegadores modernos y WebView.

## 6. Grid de ronda

Se reemplazó la suma `15% + 40% + 45%` por:

```css
grid-template-rows: minmax(82px, auto) minmax(0, 1fr) minmax(180px, 38%);
```

También se añadieron `min-height: 0` y `min-width: 0` en la cuadrícula, zona central y zona de acción.

## 7. Reduced motion e interacción

Se añadieron reglas explícitas:

```css
.upm-countdown { animation: none; }
.upm-button, .upm-retire { transition: none; }
html { scroll-behavior: auto; }
```

En `.upm-retire` se conservaron:

- `touch-action: manipulation`;
- `user-select: none`;
- `-webkit-user-select: none`;
- ancho completo;
- `@onclick="RetireAsync"` sin cambios;
- estados `:active` y `:disabled`;
- foco visible global.

## 8. Coincidencias Bootstrap

La búsqueda se realizó en todos los archivos `.razor`, `.html` y `.cshtml`.

Resultado:

- clases Bootstrap reales restantes: ninguna;
- referencias textuales a Bootstrap: ninguna;
- clases propias `upm-*`: permanecen y no son Bootstrap.

El patrón amplio también encontró nombres genéricos contenidos en clases propias, como `upm-round-item`, pero no corresponden a clases Bootstrap.

## 9. Validación técnica

Ejecutada en el orden solicitado:

1. `dotnet build .\UnPocoMas.UI\UnPocoMas.UI.csproj`  
   Resultado: correcto.

2. `dotnet test .\UnPocoMas.Core.Tests\UnPocoMas.Core.Tests.csproj`  
   Resultado exacto: `25` pruebas totales, `25` correctas, `0` errores, `0` omitidas.

3. `dotnet build .\UnPocoMas.Maui\UnPocoMas.Maui.csproj -f net10.0-windows10.0.19041.0`  
   Resultado: correcto.

## 10. Riesgos visuales pendientes en Android WebView

- El soporte de `100dvh` depende de la versión del WebView.
- La interpretación de `env(safe-area-inset-*)` puede variar entre dispositivos y modos de navegación.
- La barra del sistema y los gestos de navegación pueden producir pequeñas diferencias visuales.
- No se ha instalado ni validado Android en dispositivo o emulador.
- No se ha bloqueado la orientación mediante configuración de plataforma, tal como se solicitó.
