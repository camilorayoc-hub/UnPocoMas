# Auditoría aprobada del rediseño visual

No se modificaron archivos de código, estilos ni configuración durante esta auditoría.

## 1. Tabla de los 15 puntos

| Nº | Requisito | Estado | Archivo | Selector o elemento exacto | Evidencia concreta | Riesgo | Corrección mínima propuesta |
|---:|---|---|---|---|---|---|---|
| 1 | Ancho máximo de 520 px | **Cumple** | `UnPocoMas.Maui/wwwroot/app.css` | `.upm-frame`, `.upm-bottom-nav` | `width: min(100%, 520px)` | Ninguno relevante. | Ninguna. |
| 2 | Ocultar navegación en `/round` | **Cumple** | `UnPocoMas.Maui/Components/Layout/MainLayout.razor` | `@if (!IsRoundPage)` | `NavMenu` sólo se renderiza si la ruta no es `round`. | Riesgo bajo. | Ninguna. |
| 3 | Altura mínima de `RETIRAR` de 180 px | **Cumple** | `UnPocoMas.Maui/wwwroot/app.css` | `.upm-retire` | `min-height: 180px; width: 100%` | Puede quedar recortado con viewport pequeño o texto ampliado. | Revisar conjuntamente con `100dvh` y overflow. |
| 4 | Sin scroll vertical en ronda a 360×800 | **Parcial** | `app.css`, `Round.razor` | `.upm-frame--round`, `.upm-round` | `.upm-frame--round { overflow: hidden; }`; filas `15% 40% 45%` y `gap: 8px`. | El contenido puede quedar recortado en vez de desplazarse. | Migrar a `100dvh`, usar `minmax(0, ...)` y recalcular gaps/padding. |
| 5 | La navegación no cubre contenido | **Parcial** | `app.css` | `.upm-bottom-nav`, `.upm-frame` | Navegación fija de `68px`; frame con `padding-bottom: 104px`. | No garantiza `68px + safe-area-inset-bottom` en todos los dispositivos. | Usar una variable dinámica compartida para navegación y contenido. |
| 6 | `safe-area-inset-bottom` se suma realmente | **Parcial** | `app.css` | `.upm-shell`, `.upm-bottom-nav` | Ambos usan `env(safe-area-inset-bottom)`. | El frame reserva `104px` fijos y no el valor real calculado. | Compartir una variable basada en `env(safe-area-inset-bottom)`. |
| 7 | Bootstrap dejó de cargarse desde `index.html` | **Parcial** | `UnPocoMas.Maui/wwwroot/index.html` | `<link rel="stylesheet">` | No se carga `lib/bootstrap/dist/css/bootstrap.min.css`. | Los archivos siguen existiendo; además hay `</html></html>` duplicado. | Mantener una sola etiqueta `</html>` y eliminar archivos sólo tras confirmar referencias. |
| 8 | No quedan clases Bootstrap huérfanas | **No cumple** | `UnPocoMas.Maui/Components/Layout/NavMenu.razor` | Bloque heredado de líneas 1–27 | Persisten `navbar`, `container-fluid`, `nav`, `nav-link`, `bi-*`, etc. | HTML antiguo, JavaScript heredado y posible reaparición de estilos. | Eliminar el bloque antiguo y conservar `upm-bottom-nav`. |
| 9 | Multiplicador con números tabulares | **Cumple** | `app.css` | `.upm-multiplier` | `font-variant-numeric: tabular-nums`. | Ninguno relevante. | Ninguna. |
| 10 | `:focus-visible` en botones y navegación | **Cumple** | `app.css` | `button:focus-visible, a:focus-visible, input:focus-visible` | Outline de `3px` con color de acento. | Bajo. | Ninguna. |
| 11 | Resultado de ronda con `aria-live="polite"` | **Cumple** | `UnPocoMas.UI/Pages/Round.razor` | `.upm-result` | `<section aria-live="polite" aria-label="Resultado de la ronda">`. | Bajo. | Ninguna imprescindible. |
| 12 | Reducir movimiento desactiva animación | **Parcial** | `app.css` | `@media (prefers-reduced-motion: reduce)` | La animación queda en `.01ms`, pero no se establece `animation: none`. | El motor puede seguir procesando el ciclo de animación. | Añadir `.upm-countdown { animation: none; }`. |
| 13 | Utilizable con escalado de texto al 200 % | **No cumple** | `index.html`, `app.css`, `Round.razor` | Meta viewport y `.upm-frame--round` | `maximum-scale=1.0, user-scalable=no`; textos inferiores a 14 px; `overflow: hidden`. | Se impide el zoom y la ronda puede recortar contenido. | Retirar restricciones de zoom, subir tamaños y revisar overflow. |
| 14 | Usar `100dvh` para Android | **No cumple** | `app.css` | `.upm-shell`, `.upm-frame`, `.upm-frame--round` | Sólo se usa `100vh`. | Android WebView puede calcular incorrectamente el alto visible. | Añadir `100dvh` después de `100vh`. |
| 15 | `RETIRAR` conserva el manejador funcional | **Cumple** | `UnPocoMas.UI/Pages/Round.razor` | `<button @onclick="RetireAsync">` | `RetireAsync()` sigue llamando a `Runtime.RetireAsync()`. | Ninguno. | Ninguna. |

## 2. Cálculo vertical para 360 × 800 px

### Valores actuales

```css
.upm-frame--round {
	height: calc(100vh - env(safe-area-inset-top) - env(safe-area-inset-bottom));
	padding-top: 24px;
	padding-bottom: 18px;
}

.upm-round {
	height: 100%;
	grid-template-rows: 15% 40% 45%;
	gap: 8px;
}
```

### Estimación

Suposiciones representativas:

- viewport total: `800px`;
- safe area superior: `24px`;
- safe area inferior: `24px`;
- altura calculada: `800 - 24 - 24 = 752px`;
- padding superior: `24px`;
- padding inferior: `18px`;
- espacio interno: `752 - 24 - 18 = 710px`.

| Elemento | Cálculo | Estimación |
|---|---:|---:|
| Encabezado de ronda | `15% × 710` | `106,5px` |
| Gap | fijo | `8px` |
| Multiplicador/cuenta atrás | `40% × 710` | `284px` |
| Gap | fijo | `8px` |
| Zona de acción | `45% × 710` | `319,5px` |
| Botón `RETIRAR` | mínimo | `180px` |
| Padding superior | fijo | `24px` |
| Padding inferior | fijo | `18px` |
| Safe area superior | estimada | `24px` |
| Safe area inferior | estimada | `24px` |

Suma del grid:

```text
106,5 + 8 + 284 + 8 + 319,5 = 726px
```

Espacio interno disponible:

```text
710px
```

Exceso aproximado:

```text
726 - 710 = 16px
```

Suma completa:

```text
24 safe superior
+ 24 padding superior
+ 106,5 encabezado
+ 8 gap
+ 284 centro
+ 8 gap
+ 319,5 acción
+ 18 padding inferior
+ 24 safe inferior
= 816px
```

### Resultado

La ronda **no demuestra que quepa sin exceso** en `360 × 800 px` con esas safe areas. El `overflow: hidden` evita el scroll visible, pero puede recortar contenido.

## 3. Espacio reservado para la navegación inferior

La navegación usa:

```css
.upm-bottom-nav {
	min-height: 68px;
	padding-bottom: calc(6px + env(safe-area-inset-bottom));
}
```

El contenido usa:

```css
.upm-frame {
	padding-bottom: 104px;
}
```

La condición requerida es:

```text
104px >= 68px + env(safe-area-inset-bottom)
```

Esto sólo se cumple si:

```text
env(safe-area-inset-bottom) <= 36px
```

No existe una garantía general de que el safe area inferior sea menor o igual a `36px`.

**Resultado:** no se cumple de forma general.

## 4. Clases Bootstrap huérfanas confirmadas

Archivo:

`UnPocoMas.Maui/Components/Layout/NavMenu.razor`

Clases heredadas:

- `top-row`
- `ps-3`
- `navbar`
- `navbar-dark`
- `container-fluid`
- `navbar-brand`
- `navbar-toggler`
- `nav-scrollable`
- `nav`
- `flex-column`
- `nav-item`
- `px-3`
- `nav-link`
- `bi`
- `bi-house-door-fill-nav-menu`
- `bi-plus-square-fill-nav-menu`
- `bi-list-nested-nav-menu`

También permanece JavaScript heredado:

```razor
onclick="document.querySelector('.navbar-toggler').click()"
```

### Clases propias del proyecto

No son Bootstrap:

- `upm-card`
- `upm-card--padded`
- `upm-badge`
- `upm-list`
- `upm-round-item`
- `upm-nav-icon`
- `upm-button`
- `upm-stat-grid`
- `upm-frame`
- `upm-round`

El prefijo `upm-` indica que son clases propias.

No se encontraron clases activas `row`, `col-*`, `btn`, `btn-*`, `d-*`, `text-*`, `form-*`, `list-group` o `table`.

## 5. Emojis de resultado y accesibilidad

### Resultado de ronda

En `Round.razor`:

```razor
<div class="upm-result-symbol" aria-hidden="true">
	@_result.ShareSymbol
</div>

<h1>@ResultTitle</h1>
```

El emoji se oculta para lectores de pantalla, pero existe texto visible:

- `Retirada segura`
- `¡Al límite!`
- `Te pasaste`

### Resumen

En `Summary.razor`:

```razor
<span aria-label="@RoundStatus(round)">
	@round.ShareSymbol
</span>
```

También aparece el estado textual junto al símbolo.

**Resultado:** la accesibilidad no depende exclusivamente del emoji. Si Windows muestra cuadrados de color, el texto sigue comunicando el estado.

## 6. Riesgos concretos para Android WebView

### `100vh` frente a `100dvh`

**Prioridad alta.** Se usa `100vh` en `.upm-shell`, `.upm-frame` y `.upm-frame--round`. Puede generar recorte al cambiar la visibilidad de barras, espacio vacío, botón parcialmente fuera de pantalla y problemas al rotar.

**Corrección mínima:** fallback con `100vh` y valor posterior con `100dvh`.

### Teclado virtual

**Prioridad media.** Actualmente no hay campos editables. Si se añaden, `position: fixed` puede superponerse al teclado y `100vh` puede ignorar la altura visual reducida.

### Barras del sistema

**Prioridad alta.** El viewport usa `viewport-fit=cover`, pero los cálculos se basan en `100vh`. La altura real disponible puede variar con las barras del sistema.

### Safe areas

**Prioridad media-alta.** La navegación respeta `env(safe-area-inset-bottom)`, pero el contenido usa un padding fijo de `104px`.

### `position: fixed`

**Prioridad media.** La navegación puede cubrir contenido si el padding inferior es insuficiente o si cambia el viewport visual.

### Contenido bajo navegación

**Prioridad media.** `padding-bottom: 104px` no garantiza espacio suficiente en todos los dispositivos, especialmente con navegación por gestos.

### Escalado de texto

**Prioridad alta.** El meta viewport incluye `maximum-scale=1.0, user-scalable=no`, y existen textos de `0.78rem`, `0.82rem` y `0.85rem`.

### `overflow`

**Prioridad alta sólo en ronda.** `.upm-frame--round { overflow: hidden; }` puede ocultar contenido en vez de resolver el desbordamiento.

### `touch-action: manipulation`

Está aplicado a `.upm-button` y `.upm-retire`, lo que reduce retrasos de interacción y ciertos gestos de doble toque.

### Selección accidental de texto

`user-select: none` está aplicado a `.upm-button` y `.upm-retire`. No se deshabilita la selección global.

### Doble toque

`touch-action: manipulation` ayuda a evitar la interpretación del doble toque como zoom en los botones. No se detecta lógica que duplique la retirada.

### Orientación horizontal accidental

**Prioridad media-alta.** No se encontraron media queries de landscape, bloqueo de orientación ni layout alternativo horizontal. El grid mantiene `15% 40% 45%`, por lo que el multiplicador y el botón podrían quedar desproporcionados o recortados.

## 7. Incumplimientos por prioridad

### Bloqueantes

1. Uso de `100vh` y `overflow: hidden` en la ronda.
2. Espacio inferior no calculado dinámicamente.
3. Bloqueo del escalado de texto.
4. Markup Bootstrap/plantilla todavía renderizado.

### Importantes

1. Migrar a `100dvh` con fallback.
2. Eliminar el bloque antiguo de `NavMenu.razor`.
3. Revisar la suma vertical real de la ronda a 360×800.
4. Subir los textos menores de 14 px.
5. Añadir tratamiento para orientación horizontal.
6. Eliminar la etiqueta `</html>` duplicada.
7. Usar `animation: none` explícito con reducción de movimiento.

### Cosméticos

1. Emojis variables según la fuente disponible.
2. `backdrop-filter` con soporte variable en WebView.
3. Iconos Unicode dependientes de la fuente.
4. Falta de estado específico para pantallas muy bajas.

## 8. Parche propuesto por archivo, sin aplicarlo

### `UnPocoMas.Maui/wwwroot/app.css`

- Añadir `100dvh` después de `100vh`.
- Crear una variable dinámica para altura de navegación, padding y safe area inferior.
- Usar esa variable en `.upm-frame`.
- Revisar `.upm-round` usando `minmax(0, ...)`.
- Añadir `animation: none` para `.upm-countdown`.
- Subir textos inferiores a 14 px a `0.875rem` como mínimo.
- Añadir media query para orientación horizontal.
- Mantener `touch-action: manipulation` y `user-select: none` en botones.

### `UnPocoMas.Maui/wwwroot/index.html`

- Eliminar `maximum-scale=1.0`.
- Eliminar `user-scalable=no`.
- Mantener `viewport-fit=cover`.
- Eliminar la segunda etiqueta `</html>`.

### `UnPocoMas.Maui/Components/Layout/NavMenu.razor`

- Eliminar el bloque antiguo con `top-row`, `navbar`, `navbar-toggler`, `nav-scrollable`, la navegación antigua y el JavaScript del menú hamburguesa.
- Conservar únicamente `nav.upm-bottom-nav`.

### `UnPocoMas.UI/Pages/Round.razor`

No se propone modificar la lógica funcional. El resultado ya tiene `aria-live="polite"`, texto visible y estado textual independiente del símbolo.

### `UnPocoMas.UI/Pages/Summary.razor`

No se requiere cambio funcional. El resumen ya incluye `aria-label` y texto visible del estado.

## 9. Veredicto

**Requiere correcciones previas antes de probarlo en Android como validación final.**

Las correcciones mínimas prioritarias son:

1. Sustituir `100vh` por `100dvh` con fallback.
2. Corregir el cálculo de espacio inferior de la navegación.
3. Retirar `user-scalable=no` y `maximum-scale=1.0`.
4. Eliminar el markup Bootstrap residual.
5. Revisar el overflow de la ronda para evitar recortes.
6. Eliminar la etiqueta `</html>` duplicada.

En Windows puede realizarse una prueba preliminar, pero no se considera listo para validación Android hasta resolver esos puntos.
