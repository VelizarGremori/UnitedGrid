# UnitedGred


Веб-чат на ASP.NET Core MVC (.NET 8) с поддержкой приватных и групповых сообщений.  
Используется SignalR для реального времени и Redis для масштабируемости.

## Возможности

- Приватные и групповые чаты  
- Статус прочтения сообщений  
- Онлайн-статусы пользователей через Redis  
- Уведомление пользователя

## Запуск проекта

1. Установить зависимости:

```bash
dotnet restore
```

2. Примени миграции и создай базу данных:

```bash
dotnet ef database update
```
3. Запусти Redis локально:

Если Redis не установлен — можно поднять его через WSL:

```bash
sudo apt update
sudo apt install redis-server
sudo service redis-server start
```
Запусти приложение:

```bash
dotnet run
```
4. Конфигурация
Файл appsettings.json должен содержать строки подключения:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=YourDatabaseName;Trusted_Connection=True;",
  "Redis": "localhost:6379"
}
```
