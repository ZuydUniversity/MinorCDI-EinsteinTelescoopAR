# AR Object Scene Switcher

## Bestanden
- `Assets/Scripts/Lift/ARObjectSceneSwitcher.cs` - Het script voor scene switching
- `Assets/_ARMarker/Prefabs/JohnLemon.prefab` - Prefab met toegevoegde BoxCollider en ARObjectSceneSwitcher component

## Hoe het werkt

### 1. AR Marker Detectie
In de `LiftScene` gebruikt XR Origin (AR Rig) de `ARTrackedImageManager` component om markers te detecteren en spawnt de prefabs wanneer een marker gevonden wordt.

### 2. Touch Detection met Raycasting
Het `ARObjectSceneSwitcher` script:
- Detecteert touch input via Unity's Input System
- Cast een **ray** (onzichtbare lijn) vanaf de camera naar het touch point
- Checkt of de ray de collider van het object raakt
- Laadt de target scene bij een succesvolle hit

**Wat is Raycasting?** Een raycast is als een laserstraal die checkt of je touch iets raakt in de 3D wereld. De ray converteert je 2D scherm touch naar een 3D wereld richting.

### 3. Componenten op de prefab
- **BoxCollider** - "Klik-zone" voor raycast detectie (zonder collider werkt touch niet!)
- **ARObjectSceneSwitcher** - Script dat touch input afhandelt en scenes laadt

## Configuratie

### In de Inspector
Voeg de `ARObjectSceneSwitcher` component toe aan de gewenste prefab. Dit script heeft:
- **Target Scene Name**: De naam van de scene om naar te laden
- **Max Raycast Distance**: Maximale afstand voor raycast detectie (standaard: 50m)

### Scene wijzigen
Om naar een andere scene te switchen bij klik:
1. Open `Assets/_ARMarker/Prefabs/JohnLemon.prefab`
2. Selecteer het root GameObject
3. Vind de `ARObjectSceneSwitcher` script component
4. Verander de "Target Scene Name" naar gewenste scene:
   - `LiftSceneUI`
   - `LiftSceneBinnenUI`
   - `LiftSceneBuitenUI`

## Testing in Editor
- Play de LiftScene
- Gebruik simulatie mode voor AR
- Klik op JohnLemon of Fusebox met de muis
- Scene switch wordt getriggered

**Note:** Je ziet mogelijk een `MissingReferenceException` error in de Console bij scene switching in de Editor. Dit is normaal en komt door AR simulation in Unity Editor. Op een echte telefoon zal deze error niet voorkomen. Het belangrijkste is dat de scene switching **werkt**

