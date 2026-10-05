# OnlineDrawing

Пет-проект: онлайн-доска для совместного рисования. Пользователь регистрируется, создаёт комнату, и все, кто в неё зашёл, рисуют на одном холсте одновременно и видят друг друга в списке участников комнаты.

Делал я его в первую очередь, чтобы попробовать микросервисную архитектуру на практике: несколько сервисов на ASP.NET, постоянное подключение клиента и сервера через SignalR, брокер сообщений RabbitMQ, базы PostgreSQL и Redis, Docker для деплоя. Клиент написан на Vue 3.

## Чему я тут научился

- Разбил бэкенд на отдельные сервисы (Auth, Rooms, RealtimeBoard) и поставил перед ними API Gateway на YARP.
- Сделал аутентификацию на JWT, все сервисы проверяют подпись токена общим секретным ключом.
- Работал с SignalR: совместное рисование, группы комнат, передача токена через query string для WebSocket.
- Связал сервисы Rooms.Api и RealtimeBoard.Api через RabbitMQ: когда комнату удаляют, сервис комнат шлёт событие, а сервис доски закрывает её у всех участников.
- Поместил список онлайн-пользователей в Redis.
- Использовал PostgreSQL и Entity Framework Core с миграциями (по своей базе на сервис).
- Написал фронтенд на Vue 3 + TypeScript (Pinia, Vue Router, Tailwind, shadcn-vue).
- Упаковал всё в Docker Compose, чтобы проект поднимался одной командой.
- Написал модульные и интеграционные тесты на xUnit, для этого выделил интерфейсы у сервисов и репозиториев.

## Возможности

- Регистрация и вход по логину и паролю (пароли хранятся в виде BCrypt-хэша).
- Создание, просмотр и удаление своих комнат.
- Совместное рисование на общем холсте.
- Список пользователей, которые сейчас в комнате.
- Один пользователь может быть подключён только с одной вкладки: при входе с новой старая отключается.
- Если владелец удалил комнату, все, кто в ней сидел, получают уведомление.

## Технологии

| Часть | Что использовал |
|---|---|
| Бэкенд | C#, .NET 10, ASP.NET Core Web API |
| Реальное время | SignalR |
| API Gateway | YARP (Yarp.ReverseProxy) |
| Базы данных | PostgreSQL 16 (ORM - Entity Framework Core) для аккаунтов и комнат, Redis для списка участников комнаты |
| Брокер сообщений | RabbitMQ |
| Аутентификация | JWT (HS256), BCrypt для хэширования паролей |
| Фронтенд | Vue 3, TypeScript, Vite, Pinia, Vue Router, Tailwind CSS, shadcn-vue |
| Инфраструктура | Docker, Docker Compose, nginx |
| Тесты | xUnit, NSubstitute, SQLite |

## Архитектура

<img width="1259" height="938" alt="изображение" src="https://github.com/user-attachments/assets/887c1b47-b9b0-462a-99b4-d75249412d42" />

Браузер ходит только в Gateway, а он уже раскидывает запросы по сервисам:

- /api/auth/... идёт в Auth.Api
- /api/rooms/... идёт в Rooms.Api
- /boardhub/... идёт в RealtimeBoard.Api (SignalR)

## Логика работы

**Вход и токен.** Auth.Api проверяет логин и пароль и выдаёт JWT. В токене лежат id, имя и почта пользователя. Остальные сервисы сами проверяют подпись токена общим ключом.

**Комнаты.** Rooms.Api хранит комнаты в своей базе PostgreSQL. Создать комнату может любой авторизованный пользователь, а удалить только владелец. Название комнаты должно быть уникальным.

**Рисование.** Фронтенд подключается к хабу в RealtimeBoard.Api, заходит в комнату (JoinRoom) и отправляет события штриха: StartStroke, AddPoint, EndStroke. Хаб рассылает их всем остальным в группе комнаты, а отправителю не возвращает. Перед входом пользователя в комнату RealtimeBoard.Api через HTTP спрашивает у Rooms.Api, существует ли комната.

**Кто онлайн.** Список подключённых пользователей лежит в Redis. При входе, выходе или отключении всем в комнате отправляется обновлённый список.

**Удаление комнаты.** Rooms.Api удаляет комнату из базы и кладёт id в очередь RabbitMQ room_deleted. RealtimeBoard.Api читает очередь, шлёт участникам событие RoomDeleted и чистит данные о них в Redis. Если RabbitMQ недоступен, комната всё равно удаляется, просто уведомление не придёт.

## Демо


https://github.com/user-attachments/assets/13890f98-3280-4758-83ad-c57270aee3bb


видео обрезано и ускорено*

## Системные требования

Для запуска через Docker:

- Docker и Docker Compose
- свободные порты 5173, 5215, 5123, 5204, 5238 и 15672

Windows: скачать и поставить [Docker Desktop](https://www.docker.com/products/docker-desktop/), Compose уже входит в него.

Linux (на примере Ubuntu), ставим Docker Engine с плагином Compose по [официальной инструкции](https://docs.docker.com/engine/install/) или быстрым скриптом:

```
curl -fsSL https://get.docker.com | sh
sudo usermod -aG docker $USER
```

После usermod нужно перелогиниться, чтобы команды docker работали без sudo.

Для запуска тестов (Docker не нужен):

- [.NET SDK 10](https://dotnet.microsoft.com/download/dotnet/10.0): на Windows скачать установщик, на Linux поставить по [инструкции для своего дистрибутива](https://learn.microsoft.com/dotnet/core/install/linux)

Тесты я запускал только на Windows. Они не завязаны на ОС (SQLite в памяти и объекты в памяти вместо внешних сервисов), так что на Linux должны работать, но там я их не проверял.

## Установка и запуск

1. Склонировать репозиторий и перейти в папку проекта:

```
git clone https://github.com/savilove0810-creator/OnlineDrawing_PetProject.git
cd OnlineDrawing_PetProject
```

2. Скопировать файл с настройками и заполнить его своими значениями:

```
cp .env.example .env
```

В .env лежат логин и пароль для PostgreSQL, RabbitMQ и Redis, а также JWT_KEY (секретная строка для подписи токенов, лучше длинная).

Для JWT_KEY можно сгенерировать случайную строку (нужно хотя бы 32 символа).

Linux, macOS или Git Bash:

```
openssl rand -base64 48
```

Windows PowerShell:

```
$b = New-Object byte[] 48; [Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($b); [Convert]::ToBase64String($b)
```

3. Поднять всё одной командой:

```
docker compose up --build
```

Миграции базы применяются автоматически при старте сервисов.

## Использование

1. Открыть http://localhost:5173
2. Зарегистрироваться и войти.
3. Создать и зайти в комнату.
4. Чтобы проверить совместное рисование, откройте ту же комнату по её ссылке в другом браузере под другим пользователем (с одного аккаунта две вкладки не получится, старая отключится).

Полезные адреса:

| Что | Адрес |
|---|---|
| Фронтенд | http://localhost:5173 |
| Gateway (единая точка входа для API) | http://localhost:5215 |
| Панель RabbitMQ | http://localhost:15672 |

## Структура проекта

```
Drawing/
├── apis/
│   ├── BuildingBlocks/Drawing.Shared.Web/   общий код: JWT, CORS, обработка ошибок
│   ├── Auth.Api/                            регистрация и вход, выдача JWT
│   ├── Rooms.Api/                           комнаты, события в RabbitMQ
│   ├── RealtimeBoard.Api/                   SignalR-хаб, онлайн в Redis
│   └── Gateway.Api/                         YARP, единая точка входа
├── DrawingFront/                            фронтенд на Vue 3
│   └── src/                                 устроен по FSD (ссылка в документации)
│       ├── features/                        auth, rooms, board, presence
│       ├── pages/                           страницы
│       ├── widgets/                         крупные блоки интерфейса (например, боковая панель)
│       ├── realtime/                        работа с SignalR
│       └── shared/                          общие утилиты и API-клиент
├── tests/Drawing.Tests/                     модульные и интеграционные тесты
├── docker-compose.yml
├── .env.example
└── Drawing.slnx
```

## Тесты

Тесты лежат в tests/Drawing.Tests, их сейчас 71. Запуск:

```
dotnet test tests/Drawing.Tests
```

Docker для тестов не нужен, внешние сервисы в них заменены:

- PostgreSQL → SQLite в памяти.
- Redis → класс FakePresenceStore, список онлайн хранится в словаре.
- RabbitMQ и запрос из RealtimeBoard в Rooms.Api → объекты из NSubstitute. Это библиотека, которая создаёт объект по интерфейсу: он ничего не делает по-настоящему, а запоминает вызовы и отдаёт заданные ответы. Так тест проверяет, например, что при удалении комнаты вызвали SendEvent("room_deleted", id). Всё это работает, потому что сервисы зависят от интерфейсов.

- **Модульные тесты (Unit/)**: проверяют один класс, всё остальное заменено NSubstitute. JWT, регистрация и вход, валидация DTO, логика комнат, вход и выход из комнаты.
- **Интеграционные тесты (Integration/)**: части работают вместе, с базой SQLite и настоящими HTTP-запросами. Репозитории, API Auth и Rooms, SignalR-хаб. Сервис запускается в тесте через WebApplicationFactory без открытия порта.

Что пока не покрыто: настоящие Redis и RabbitMQ (для них можно взять Testcontainers), конфигурация Gateway и фронтенд.

## Документация

Чем пользовался, когда делал проект:

- [ASP.NET Core SignalR](https://learn.microsoft.com/aspnet/core/signalr/introduction)
- [YARP Reverse Proxy](https://microsoft.github.io/reverse-proxy/)
- [Entity Framework Core](https://learn.microsoft.com/ef/core/)
- [JWT Bearer в ASP.NET Core](https://learn.microsoft.com/aspnet/core/security/authentication/)
- [RabbitMQ .NET client](https://www.rabbitmq.com/client-libraries/dotnet-api-guide)
- [StackExchange.Redis](https://stackexchange.github.io/StackExchange.Redis/)
- [Docker Compose](https://docs.docker.com/compose/)
- [Vue 3](https://vuejs.org/guide/introduction.html), [Pinia](https://pinia.vuejs.org/), [Vue Router](https://router.vuejs.org/), [shadcn-vue](https://www.shadcn-vue.com/)
- [FSD - архитектурой для фронтенда](https://habr.com/ru/companies/piter/articles/744824/)
- [xUnit](https://xunit.net/docs/getting-started/v2/getting-started), [NSubstitute](https://nsubstitute.github.io/)
