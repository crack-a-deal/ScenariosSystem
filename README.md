# LaboratoryComplex

Виртуальные лабораторные комплексы **«Теплотехническое оборудование»** и **«Электроэнергетика»**. Unity-приложение для учебных классов: студенты на своих ПК проходят интерактивные 3D-сценарии, осматривают 3D-модели и сдают тесты; ПК преподавателя работает сервером, хранит результаты в SQLite и выгружает отчёты в Excel.

## Быстрый старт

1. Клонировать репозиторий, открыть в **Unity 6000.3.10f1**. Для сборки под Linux нужен модуль **Linux Build Support**.
2. Дождаться восстановления NuGet-пакетов (`Assets → NuGet → Restore Packages`, если не запустилось само).
3. Открыть сцену `Assets/LaboratoryComplex/Scenes/Bootstrap.unity` (она же первая в Build Settings) и нажать Play.
4. Вход преподавателем: логин **admin / admin** (учётка создаётся сидом БД).

5. ## Архитектура

**Машина состояний.** `MyApplication` (реализация `IApplication`) переключает `IApplicationState`-состояния, регистрируемые `ApplicationEntryPoint`. Диаграмма переходов:

```
Bootstrap ──лицензии нет──► StudentWaiting (экран ожидания)
    │ лицензия есть
    ▼
RoleSelection ──► TeacherAuthorization ──► TeacherMain (админ-панель, TCP-сервер)
    └─────────► StudentAuthorization ──► StudentMain ──► TestState / InspectionModelState / ScenarioState
```

**DI (Zenject).** Инсталлеры в `Assets/LaboratoryComplex/Scripts/Installers/`:

| Инсталлер | Что делает |
|---|---|
| `ApplicationInstaller` | машина состояний, `UserSession`, точка входа |
| `DatabaseInstaller` | `AppDbContext` (SQLite `Education.db`) |
| `ServicesInstaller` | сервисы, репозитории, реестры Excel-экспорта, выбор OS-провайдера путей |
| `PresentersInstaller` / `ViewsInstaller` | презентеры и сериализованные ссылки на вью (для каждого экрана) |
| `NetworkingInstaller`, `InspectionInstaller` | сетевые сервисы и подсистема осмотра |

**Сеть.** Преподаватель = TCP-сервер (порт 5000) + UDP-broadcast обнаружения (порт 37020, каждые 2 сек). Студенты автоматически находят сервер в LAN и подключаются. Запросы — `IRequest`/`IRequestHandler`, сериализация JSON. Порты настраиваются в `Assets/StreamingAssets/Config/networking.json`.

**База данных.** SQLite-файл `Education.db` рядом с исполняемым файлом (в редакторе — в корне проекта). Схема создаётся `EnsureCreated()` — миграций **нет**: при изменении EF-моделей удалите старый файл БД (или выполните `ALTER TABLE` вручную). Основные таблицы: пользователи/группы, тесты/вопросы/ответы, сессии тестов с ответами, 3D-модели и их просмотры, сценарии и их прохождения.

## Сборка двух комплексов

В репозитории **два продукта в одном проекте**. Для сборки конкретного комплекса переключаются три места:

1. **Build Settings**: сцена `Bootstrap` — первая (индекс 0); далее сцена комплекса (`Теплотехническое оборудование` или `Электроэнергетика`), `MainScene` и нужная сценарная сцена.
2. **`ScenarioState`**: константа `ScenarioSceneName` — `"ScenarioScene 1"` или `"ScenarioScene_2"`.
3. **`ViewsInstaller`**: поля `ScenarioList` и `ModulesList` — ассеты выбранного комплекса (каталоги имён сценариев и моделей: `Assets/Scripts/Export/ScenarioList.asset` — теплотехника, `ScenarioList 1.asset` — электро; аналогично `ModulesList`).

## Структура каталогов

```
Assets/
├── LaboratoryComplex/        # основное приложение
│   ├── Scripts/              # состояния, инсталлеры, сервисы, сеть, БД, тесты, админка, Excel
│   ├── Scenes/               # Bootstrap + сцены двух комплексов
│   ├── ScenarioState.cs, ScenarioSceneRoot.cs, ScenarioCompletionInfo
├── Scripts/                  # движок сценариев (Runner, Step, Conditions, EventBus, режимы)
│   └── Export/               # каталог сценариев + ScenarioRecord (сущности/сервисы прохождений)
├── Scenes/                   # MainScene, ScenarioScene 1, ScenarioScene_2
├── StreamingAssets/          # конфиги (networking.json, tests.json) и банки тестов
├── Content/                  # 3D-контент комплексов
└── NuGet/, packages.config   # зависимости NuGetForUnity
```
