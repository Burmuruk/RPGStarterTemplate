# RPG Starter Template

**RPG Starter Template** es una herramienta diseñada para agilizar la creación de un juego tipo RPG en Unity. Incluye una arquitectura básica extensible para el juego y herramientas de editor para configurar personajes, estadísticas y más.

## Características

- Arquitectura base para creación de juego RPG.
- Creación de NavMesh para agentes voladores y no voladores.
- Sistema de guardado por slots, con información de preview editable.
- Creación y configuración de personajes, estadísticas, armas, etc. mediante ventanas.
- Sistema de buffs.
- Soporte para progresión de niveles.
- Prefabs y ScriptableObjects generados automáticamente.
- Sistema de diálogos con editor de nodos.
- Sistema genérico de inventario.
- Sistema de patrullaje.
- Sistema genérico para interacciones.
- Agentes con movimiento base y detección de enemigos. 
- Escena demo.



## Requisitos

   * Unity 2022.3+
     
   * Windows (solo para lectura de NavMesh)

   * Dependencias:

       * TextMeshPro (Demo)

       * Cinemachine (Demo)

       * InputSystem (Demo)

       * Newtonsoft.Json (Sistema de guardado)
    

## Instalación (En desarrollo. Sin funcionar por el momento)

1. En tu proyecto de Unity, abre `Packages/manifest.json`.
2. Agrega:

```json
"com.Burmuruk.RPG-Starter-Template": "https://github.com/Burmuruk/RPGStarterTemplate.git"
```



## Estructura
├── com.Burmuruk.RPG-Starter-Template/

    ├── GameArchitecture/          # Copiará a Assets/ si lo aceptas

    ├── Tool/                      # Herramientas de editor

    └── Samples/                   # Escena de ejemplo
├── Results/                   # ScriptableObjects y prefabs generados

├── StreamingAssets/                   # Archivos de navegación generados



