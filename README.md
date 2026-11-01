# Pracownia alchemika

Gra VR na gogle HTC Vive (Unity 6, XR Interaction Toolkit, OpenXR). Gracz prowadzi pracownię alchemiczną w wozie na rynku i warzy mikstury.

## Uruchomienie

1. Otwórz projekt w Unity 6000.0.85f1.
2. Otwórz scenę `Assets/MageWorkshop/Scenes/Workshop.unity`.
3. Naciśnij Play.

## Sterowanie

| Akcja | HTC Vive | Symulator |
| --- | --- | --- |
| Chwycenie / puszczenie przedmiotu | grip | G |
| Oddalenie / przybliżenie trzymanego przedmiotu | touchpad w górę / w dół | I / K |

## Struktura

Wszystkie pliki projektu są w `Assets/MageWorkshop`: `Scenes` — scena, `Scripts` — kod, `Data` — składniki i receptura, `Materials` i `Meshes` — materiały i modele.

| Plik | Opis |
| --- | --- |
| `Scripts/Brewing/Cauldron.cs` | Kocioł: rejestrowanie składników, płynów i mieszania chochlą, poziom i kolor cieczy, ocena receptury, wybuch |
| `Scripts/Brewing/Recipe.cs` | Receptura (ScriptableObject) i jej sprawdzanie |
| `Scripts/Brewing/BrewAction.cs` | Pojedyncza czynność zarejestrowana przez kocioł |
| `Scripts/Brewing/IngredientDefinition.cs` | Rodzaj składnika (stały lub płynny) |
| `Scripts/Interaction/Ingredient.cs` | Składnik, który można wrzucić do kotła |
| `Scripts/Interaction/LiquidSource.cs` | Nalewanie płynu przez przechylenie butelki |
| `Scripts/Interaction/Ladle.cs` | Chochla do mieszania |
| `Scripts/Interaction/ReturnHome.cs` | Powrót upuszczonych przedmiotów na miejsce |
| `Scripts/Interaction/GrabDistanceControl.cs` | Przybliżanie i oddalanie trzymanego przedmiotu |
| `Scripts/Interaction/PlayerCollision.cs` | Kolizje gracza ze ścianami i meblami |
| `Scripts/Interaction/Haptics.cs` | Wibracje kontrolerów |
| `Scripts/Core/SimulatorBootstrap.cs` | Włączanie symulatora, gdy nie ma gogli |
| `Scripts/Core/LightFlicker.cs` | Migotanie ognia, latarni i świec |

Nowe składniki i receptury: prawy przycisk w oknie Project -\> **Create -\> Mage Workshop -\> Ingredient / Recipe**.
