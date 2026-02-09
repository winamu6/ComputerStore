# Компьютерный Магазин - ASP.NET Core

## Структура проекта

### Проекты Solution

1. **ComputerStore.Domain** - Доменный слой
   - Entities (сущности базы данных)
   - Enums (перечисления)
   - Interfaces (интерфейсы репозиториев)

2. **ComputerStore.Application** - Слой бизнес-логики
   - Services (сервисы)
   - DTOs (объекты передачи данных)
   - Validators (валидация)
   - Mappings (AutoMapper профили)

3. **ComputerStore.Infrastructure** - Инфраструктурный слой
   - DbContext (контекст EF Core)
   - Repositories (реализация репозиториев)
   - Configurations (конфигурация EF Core)
   - Migrations (миграции БД)

4. **ComputerStore.Shared** - Общие утилиты
   - Constants
   - Helpers
   - Extensions

5. **ComputerStore.Web** - Веб-приложение (MVC)
   - Controllers
   - Views
   - ViewModels
   - Areas (Identity)

## Модели данных

### Основные сущности:

- **Product** - Товары (компьютерные комплектующие)
- **Category** - Категории товаров (с поддержкой подкатегорий)
- **Order** - Заказы покупателей
- **OrderItem** - Позиции в заказе
- **Customer** - Профили покупателей
- **CartItem** - Корзина покупок
- **Review** - Отзывы на товары
- **ProductSpecification** - Технические характеристики
- **ProductImage** - Изображения товаров

### Перечисления:

- **OrderStatus** - Статусы заказа (Pending, Processing, Shipped, Delivered, Cancelled, Refunded)
- **PaymentMethod** - Способы оплаты (CreditCard, DebitCard, PayPal, BankTransfer, Cash)

## Установка и настройка

### Предварительные требования:

- Visual Studio 2022
- .NET 8.0 SDK
- Docker Desktop
- SQL Server (через Docker)

### Зависимости проектов:

```
ComputerStore.Web
    └── ComputerStore.Application
    └── ComputerStore.Infrastructure
    └── ComputerStore.Shared

ComputerStore.Infrastructure
    └── ComputerStore.Domain
    └── ComputerStore.Application

ComputerStore.Application
    └── ComputerStore.Domain
    └── ComputerStore.Shared
```

### NuGet пакеты:

**ComputerStore.Infrastructure:**
```bash
Install-Package Microsoft.EntityFrameworkCore
Install-Package Microsoft.EntityFrameworkCore.SqlServer
Install-Package Microsoft.EntityFrameworkCore.Tools
Install-Package Microsoft.EntityFrameworkCore.Design
```

**ComputerStore.Application:**
```bash
Install-Package AutoMapper
Install-Package AutoMapper.Extensions.Microsoft.DependencyInjection
Install-Package FluentValidation
Install-Package FluentValidation.DependencyInjectionExtensions
```

**ComputerStore.Web:**
```bash
Install-Package Microsoft.EntityFrameworkCore.Design
Install-Package Microsoft.AspNetCore.Identity.EntityFrameworkCore
```

## Запуск с Docker

### 1. Запуск базы данных и приложения:

```bash
docker-compose up -d
```

### 2. Применение миграций:

```bash
dotnet ef database update --project ComputerStore.Infrastructure --startup-project ComputerStore.Web
```

### 3. Доступ к приложению:

- Web: http://localhost:8080
- HTTPS: https://localhost:8081
- SQL Server: localhost:1433

## Следующие шаги разработки:

1. ✅ Создание структуры проектов
2. ✅ Разработка моделей данных
3. ✅ Настройка DbContext и конфигурации EF Core
4. ✅ Создание миграций базы данных
5. ✅ Реализация репозиториев
6. ✅ Разработка сервисов бизнес-логики
7. ⏭️ Создание контроллеров и представлений
8. ⏭️ Реализация корзины покупок
9. ⏭️ Настройка аутентификации и авторизации
10. ⏭️ Добавление функционала поиска и фильтрации
11. ⏭️ Интеграция платёжных систем

## Функционал приложения:

### Для покупателей:
- Просмотр каталога товаров
- Поиск и фильтрация по категориям
- Корзина покупок
- Оформление заказов
- Отслеживание заказов
- Написание отзывов
- Личный кабинет

### Для администраторов:
- Управление товарами
- Управление категориями
- Обработка заказов
- Управление пользователями
- Модерация отзывов
- Аналитика продаж

## Технологии:

- ASP.NET Core 8.0 MVC
- Entity Framework Core 8.0
- SQL Server
- Docker
- Bootstrap 5
- AutoMapper
- FluentValidation
- ASP.NET Core Identity

## Автор

Разработано для обучения и демонстрации навыков разработки на ASP.NET Core