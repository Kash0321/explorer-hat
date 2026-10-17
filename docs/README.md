# Documentos de apoyo (`docs/`)

Documentos que no son lecciones ni código del robot: manuales de piezas, medidas hechas, fotos y el prompt para retomar
el trabajo. Las lecciones están en [`lessons/`](../lessons/README.md), el estado del proyecto en
[`PLAN.md`](../PLAN.md) y las normas de trabajo en [`AGENTS.md`](../AGENTS.md).

| Documento | Qué contiene | Cuándo mirarlo |
|---|---|---|
| [`retomar-sesion.md`](retomar-sesion.md) | El prompt para empezar una sesión nueva con un asistente de IA y recuperar el contexto del proyecto | Al empezar una sesión de trabajo |
| [`multimetro-DT830B.md`](multimetro-DT830B.md) | Manual del multímetro ExtraStar DT830B, **en castellano y en inglés**, con las marcas de la carcasa, los rangos y la precisión de cada uno, y los errores del original corregidos | Al medir resistencias, tensiones o continuidad |
| [`fotorresistencias-LDR.md`](fotorresistencias-LDR.md) | Todas las medidas de las fotorresistencias (LDR), el divisor de tensión con las tensiones esperadas, cómo conectarlas, ideas de uso y el texto de lección que se quitó de la lección 08. El programa para medir otras LDR con el robot está en [`src/ExplorerHat.LdrCheck`](../src/ExplorerHat.LdrCheck/README.md) | Si se vuelve a usar una LDR (una lección nueva, un vehículo de Braitenberg, un sensor de luz) |
| [`img/pantalla-conexion.jpg`](img/pantalla-conexion.jpg) | Foto de los cuatro cables de la pantalla LCD y su placa | La usa el [README principal](../README.md) (cables de la pantalla) |

## Cómo añadir un documento

1. Guárdalo en `docs/` (las fotos, en `docs/img/`) con un nombre que diga qué es.
2. Añade una fila a esta tabla: qué contiene y **cuándo mirarlo**.
3. Si lo usa una lección o el README, enlázalo desde allí.
4. Si es una medida o una decisión, anota en `PLAN.md` qué se decidió y enlaza el documento.
