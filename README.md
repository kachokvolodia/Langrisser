# Langrisser-Like SRPG

Прототип 2D SRPG в духе **Langrisser** на Unity. Проект развивается пошагово и предназначен для обучения.

## Минимальная версия Unity

Согласно `ProjectSettings/ProjectVersion.txt` используется Unity **6000.1.4f1**. Открывайте проект этой или более новой версией.

## Запуск
1. Склонируйте репозиторий и откройте его через Unity Hub.
2. После импорта выберите сцену `Scenes/LangrisserScene.unity` и запустите её.

## Структура
- `Assets/` – игровые ресурсы и код.
  - `Scripts/` – C# скрипты.
  - `Prefabs/`, `Sprites/`, `Tiles/` – графика и префабы.
  - `Data/` – ScriptableObject с данными биомов, фракций и юнитов.
- `Packages/` – зависимости проекта.
- `ProjectSettings/` – настройки Unity.

## Тесты
Тесты расположены в `Assets/Tests`. Запускайте их через **Unity Test Runner**:
1. В Unity откройте **Window → General → Test Runner**.
2. Вкладка *Edit Mode* содержит примеры тестов. Нажмите *Run All*, чтобы выполнить их.
