# Pracownia alchemika (MageWorkshop)

VR-игра для HTC Vive Pro на Unity 6 + XR Interaction Toolkit 3.4 + OpenXR (SteamVR).

## Первый запуск

1. Открыть проект в Unity 6000.0.85f1.
2. Открыть сцену `Assets/MageWorkshop/Scenes/Workshop.unity` (она единственная в Build Settings).
3. Нажать Play.

OpenXR для Windows уже настроен: включены профили контроллеров HTC Vive и Meta Quest, режим Single Pass Instanced
(**Edit → Project Settings → XR Plug-in Management → OpenXR**).

## Шлем или симулятор — переключать ничего не нужно

`Simulator Bootstrap` в сцене через ~1,5 с после старта проверяет, есть ли шлем:

- **Есть HTC Vive (лаборатория):** запустите SteamVR (он должен быть OpenXR runtime по умолчанию), затем Play — картинка идёт в шлем, контроллеры Vive управляют руками.
- **Шлема нет (дома):** автоматически запускается **XR Interaction Simulator** (подсказки по клавишам — на экране в окне Game).

## Управление

| Действие | HTC Vive | Симулятор |
|---|---|---|
| Взять / отпустить предмет | grip (боковая кнопка) | **G** |
| Отдалить / приблизить взятый предмет | провести по тачпаду вверх / вниз | **I** / **K** |
| Выбрать руку или голову | — | **Tab** |
| Подсказки симулятора | — | **Y** |

## Тестовый рецепт — Eliksir zdrowia

1. Бросить в котёл **2× Utłuczony korzeń mandragory** (корешки с листьями на нижней полке стеллажа справа).
2. Налить **Woda źródlana** (голубая бутылка на столе справа) до золотой метки (300 ± 40 мл) — наклонить бутылку над котлом.
3. Взять черпак (висит слева) и сделать **3 оборота по часовой стрелке** (если смотреть сверху).

Ход варки и ошибки показаны на табличке справа под маркизой. Ошибка (не тот ингредиент, порядок, слишком много / мало, не то направление) — взрыв котла; через 3 с котёл очищается.

## Структура

Всё своё лежит в `Assets/MageWorkshop`: `Scenes` — сцена, `Scripts` — код, `Data` — ингредиенты и рецепт, `Materials` и `Meshes` — материалы и модели.

| Файл | Что делает |
|---|---|
| `Scripts/Brewing/Cauldron.cs` | Котёл: учёт ингредиентов/жидкостей/оборотов (F7, F8), уровень и цвет жидкости, проверка рецепта, взрыв |
| `Scripts/Brewing/Recipe.cs` | Рецепт (ScriptableObject) и его проверка |
| `Scripts/Interaction/LiquidSource.cs` | Наливание при наклоне бутылки (F6) |
| `Scripts/Interaction/Ladle.cs` | Черпак (кончик, по которому считаются обороты) |
| `Scripts/Interaction/ReturnHome.cs` | Возврат упавших / недоступных предметов на место (F3) |
| `Scripts/Interaction/Haptics.cs` | Вибрация контроллеров (F21) |
| `Scripts/Interaction/GrabDistanceControl.cs` | Приближение / отдаление взятого предмета |
| `Scripts/Interaction/PlayerCollision.cs` | Тело игрока: не даёт пройти сквозь стены, прилавок, стол и котёл |
| `Scripts/Core/SimulatorBootstrap.cs` | Симулятор, только когда нет шлема |
| `Scripts/Core/LightFlicker.cs` | Мерцание огня, фонарей и свечей |

Новые ингредиенты и рецепты: ПКМ в Project → **Create → Mage Workshop → Ingredient / Recipe**.
