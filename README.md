# 🖥️ ComputerStore - E-Commerce Platform

**Интернет-магазин компьютерной техники на ASP.NET Core 10.0 с Clean Architecture**

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat&logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-12.0-239120?style=flat&logo=csharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC2927?style=flat&logo=microsoft-sql-server)](https://www.microsoft.com/sql-server)
[![Bootstrap](https://img.shields.io/badge/Bootstrap-5.3-7952B3?style=flat&logo=bootstrap)](https://getbootstrap.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

---

## 📋 Содержание

- [О проекте](#-о-проекте)
- [Технологический стек](#️-технологический-стек)
- [Архитектура](#️-архитектура)
- [Функциональность](#-функциональность)
- [Установка и запуск](#-установка-и-запуск)
- [Конфигурация](#️-конфигурация)
- [Структура проекта](#-структура-проекта)
- [База данных](#-база-данных)
- [Пользователи по умолчанию](#-пользователи-по-умолчанию)
- [API и сервисы](#-api-и-сервисы)
- [Разработка](#-разработка)
- [Лицензия](#-лицензия)

---

## 🎯 О проекте

**ComputerStore** - это современная платформа электронной коммерции для продажи компьютерной техники и комплектующих. Проект реализован с использованием принципов **Clean Architecture** и лучших практик разработки на ASP.NET Core 10.0.

### ✨ Ключевые особенности

- ✅ **Clean Architecture** - четкое разделение на слои (Domain, Application, Infrastructure, Web)
- ✅ **SOLID принципы** - поддерживаемый и расширяемый код
- ✅ **Repository Pattern + Unit of Work** - абстракция доступа к данным
- ✅ **ASP.NET Core Identity** - полная система аутентификации и авторизации
- ✅ **Entity Framework Core 10** - ORM с Code-First подходом
- ✅ **AutoMapper** - автоматический маппинг объектов
- ✅ **FluentValidation** - декларативная валидация
- ✅ **Responsive Design** - адаптивный интерфейс на Bootstrap 5
- ✅ **Docker Support** - контейнеризация приложения

---

## 🛠️ Технологический стек

### Backend Framework

```
┌─────────────────────────────────────────┐
│   ASP.NET Core 10.0 (.NET 10.0)        │
│   C# 12.0                               │
└─────────────────────────────────────────┘
```

### Зависимости проектов

#### 📦 ComputerStore.Domain
| Пакет | Версия | Назначение |
|-------|--------|------------|
| Microsoft.AspNetCore.Identity.EntityFrameworkCore | 10.0.3 | Identity интеграция |
| Microsoft.EntityFrameworkCore.Tools | 10.0.3 | EF Core инструменты |

#### 📦 ComputerStore.Application
| Пакет | Версия | Назначение |
|-------|--------|------------|
| AutoMapper | 12.0.0 | Маппинг объектов |
| AutoMapper.Extensions.Microsoft.DependencyInjection | 12.0.0 | DI интеграция AutoMapper |
| FluentValidation | 12.1.1 | Валидация данных |
| Microsoft.EntityFrameworkCore | 10.0.3 | EF Core |
| Microsoft.EntityFrameworkCore.Tools | 10.0.3 | EF Core инструменты |

#### 📦 ComputerStore.Infrastructure
| Пакет | Версия | Назначение |
|-------|--------|------------|
| Microsoft.AspNetCore.Identity.EntityFrameworkCore | 10.0.3 | Identity + EF Core |
| Microsoft.EntityFrameworkCore | 10.0.3 | EF Core |
| Microsoft.EntityFrameworkCore.Design | 10.0.3 | Design-time компоненты |
| Microsoft.EntityFrameworkCore.SqlServer | 10.0.3 | SQL Server провайдер |
| Microsoft.EntityFrameworkCore.Tools | 10.0.3 | EF Core инструменты |

#### 📦 ComputerStore.Shared
| Пакет | Версия | Назначение |
|-------|--------|------------|
| Microsoft.EntityFrameworkCore.Tools | 10.0.3 | EF Core инструменты |

#### 📦 ComputerStore.Web
| Пакет | Версия | Назначение |
|-------|--------|------------|
| EPPlus | 8.4.2 | Работа с Excel |
| Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore | 10.0.3 | Диагностика EF Core |
| Microsoft.AspNetCore.Identity.EntityFrameworkCore | 10.0.3 | Identity + EF Core |
| Microsoft.AspNetCore.Identity.UI | 10.0.3 | Identity UI |
| Microsoft.EntityFrameworkCore.Design | 10.0.3 | Design-time компоненты |
| Microsoft.EntityFrameworkCore.SqlServer | 10.0.3 | SQL Server провайдер |
| Microsoft.EntityFrameworkCore.Tools | 10.0.3 | EF Core инструменты |
| Microsoft.VisualStudio.Azure.Containers.Tools.Targets | 1.23.0 | Docker поддержка |

### Frontend

| Технология | Версия | Назначение |
|-----------|--------|------------|
| Bootstrap | 5.3 | CSS Framework |
| Bootstrap Icons | 1.11 | Иконки |
| JavaScript | ES6+ | Клиентская логика |
| Razor Pages | - | Server-side rendering |

### Database

| Технология | Назначение |
|-----------|------------|
| Microsoft SQL Server 2022 | Реляционная БД |
| Entity Framework Core 10.0.3 | ORM |

---

## 🏗️ Архитектура

Проект следует принципам **Clean Architecture** с четким разделением ответственности:

```
┌───────────────────────────────────────────────────────────┐
│                    Presentation Layer                     │
│                  (ComputerStore.Web)                      │
│  ┌─────────────────────────────────────────────────────┐  │
│  │  Controllers │ Views │ ViewModels │ Areas/Admin     │  │
│  └─────────────────────────────────────────────────────┘  │
└──────────────────────┬────────────────────────────────────┘
                       │ зависит от
┌──────────────────────▼────────────────────────────────────┐
│                  Application Layer                        │
│              (ComputerStore.Application)                  │
│  ┌─────────────────────────────────────────────────────┐  │
│  │   Services │ DTOs │ Interfaces │ Validators         │  │
│  │   AutoMapper Profiles │ Business Logic              │  │
│  └─────────────────────────────────────────────────────┘  │
└──────────────────────┬────────────────────────────────────┘
                       │ зависит от
┌──────────────────────▼────────────────────────────────────┐
│                Infrastructure Layer                       │
│             (ComputerStore.Infrastructure)                │
│  ┌─────────────────────────────────────────────────────┐  │
│  │  DbContext │ Repositories │ Unit of Work            │  │
│  │  EF Configurations │ Data Seeding                   │  │
│  └─────────────────────────────────────────────────────┘  │
└──────────────────────┬────────────────────────────────────┘
                       │ зависит от
┌──────────────────────▼────────────────────────────────────┐
│                    Domain Layer                           │
│                (ComputerStore.Domain)                     │
│  ┌─────────────────────────────────────────────────────┐  │
│  │  Entities │ Enums │ Interfaces                       │  │
│  │  Domain Logic │ Value Objects                        │  │
│  └─────────────────────────────────────────────────────┘  │
└───────────────────────────────────────────────────────────┘

          ┌────────────────────────────────┐
          │    Shared Layer (DTOs)         │
          │   (ComputerStore.Shared)       │
          └────────────────────────────────┘
                   ▲
                   │ используется всеми слоями
```

### Принципы архитектуры

- **Dependency Inversion** - внешние слои зависят от внутренних
- **Separation of Concerns** - каждый слой имеет свою ответственность
- **Testability** - бизнес-логика независима от инфраструктуры
- **Maintainability** - легко поддерживать и расширять

---

## ⚡ Функциональность

### 🛒 Клиентская часть

#### Каталог товаров
- 📱 Просмотр товаров по категориям (иерархическая структура)
- 🔍 Полнотекстовый поиск по названию, описанию, производителю
- 🎚️ Фильтрация по цене, наличию, рейтингу
- 📊 Сортировка (по цене, названию, рейтингу)
- 🖼️ Галерея изображений товара
- 📋 Детальные характеристики (спецификации)
- ⭐ Рейтинг и отзывы покупателей

#### Корзина и оформление заказа
- 🛒 Добавление товаров в корзину
- ➕➖ Изменение количества товаров
- 💰 Автоматический расчет стоимости
- 🚚 Расчет доставки
- 🎟️ Применение промокодов и скидок
- 📝 Оформление заказа с адресом доставки
- 📧 Email уведомления о статусе заказа

#### Система оплаты
- 💳 Оплата банковской картой (симуляция платежного шлюза)
- 💵 Оплата наличными при получении
- 🔒 Безопасная обработка платежей
- 🆔 Генерация уникальных Transaction ID
- 📄 Страница подтверждения оплаты

#### Отзывы и рейтинги
- ⭐ Рейтинг от 1 до 5 звёзд
- ✍️ Заголовок и текст отзыва
- ✅ Отметка "Подтверждённая покупка" (для доставленных заказов)
- 👍👎 Голосование за полезность отзывов
- ✏️ Редактирование своих отзывов
- 🗑️ Удаление своих отзывов
- 🏆 Страница топ-товаров по рейтингу

#### Личный кабинет
- 👤 Управление профилем
- 📦 История заказов
- 📊 Статистика (всего заказов, сумма покупок)
- ⭐ Мои отзывы
- 📍 Управление адресами доставки

### 👨‍💼 Административная панель

#### Управление товарами
- ➕ Создание товаров (CRUD)
- ✏️ Редактирование товаров
- 🗑️ Удаление товаров
- 📊 Управление характеристиками
- 🖼️ Множественные изображения товаров
- 📥 Массовый импорт из JSON/Excel
- 📤 Экспорт в Excel
- 🏷️ Управление статусами (In Stock, Out of Stock, Discontinued)

#### Управление категориями
- 🗂️ Создание категорий и подкатегорий
- 🌳 Иерархическая структура (неограниченная вложенность)
- 🖼️ Изображения категорий
- 📊 Счётчик товаров в категории
- 🔄 Изменение родительской категории

#### Управление заказами
- 📋 Просмотр всех заказов
- 🎚️ Фильтрация по статусу (Pending, Processing, Shipped, Delivered, Cancelled)
- 🔍 Поиск по номеру заказа
- 📝 Детальная информация о заказе
- ✏️ Изменение статуса заказа
- 📦 Добавление трек-номера для отслеживания
- 💰 Просмотр статуса оплаты

#### Модерация отзывов ⭐ NEW
- 📋 Просмотр всех отзывов
- 🎚️ Фильтры (Все, На модерации, Одобренные, Отклонённые, Удалённые)
- ☑️ **Массовые действия:**
  - ✅ Одобрить выбранные (показывает на сайте)
  - ❌ Отклонить выбранные (скрывает с сайта)
  - 🗑️ Удалить выбранные (мягкое удаление)
  - ♻️ Восстановить выбранные (из корзины)
  - 🔥 Удалить навсегда (полное удаление из БД)
- 📄 Детальная страница отзыва
- 👤 Информация о покупателе и заказе
- 👍👎 Статистика полезности отзыва

#### Аналитика (Dashboard)
- 📊 Общая статистика (заказы, продажи, клиенты)
- 📈 Графики продаж
- 🏆 Топ товары
- 💰 Финансовая отчётность

---

## 🚀 Установка и запуск

### 📋 Предварительные требования

- ✅ [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- ✅ [SQL Server 2022](https://www.microsoft.com/sql-server/sql-server-downloads) или LocalDB
- ✅ [Visual Studio 2022](https://visualstudio.microsoft.com/) или [VS Code](https://code.visualstudio.com/)
- ⚙️ [Git](https://git-scm.com/)

### 🔧 Шаги установки

#### 1️⃣ Клонирование репозитория

```bash
git clone https://github.com/your-username/ComputerStore.git
cd ComputerStore
```

#### 2️⃣ Настройка строки подключения

Откройте `ComputerStore.Web/appsettings.json`:

**Для LocalDB:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ComputerStoreDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

**Для SQL Server:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ComputerStoreDb;User Id=sa;Password=YourPassword123;TrustServerCertificate=True"
  }
}
```

**Для Docker SQL Server:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=ComputerStoreDb;User Id=sa;Password=YourStrong@Password;TrustServerCertificate=True"
  }
}
```

#### 3️⃣ Восстановление пакетов

```bash
dotnet restore
```

#### 4️⃣ Применение миграций

```bash
cd ComputerStore.Web
dotnet ef database update --project ../ComputerStore.Infrastructure
```

**Или через Package Manager Console (Visual Studio):**
```powershell
Update-Database
```

#### 5️⃣ Запуск проекта

```bash
dotnet run --project ComputerStore.Web
```

Приложение будет доступно по адресу:
- 🌐 HTTPS: `https://localhost:5001`
- 🌐 HTTP: `http://localhost:5000`

### 🐳 Запуск через Docker

```bash
# Сборка образа
docker build -t computerstore:latest .

# Запуск контейнера
docker run -d -p 8080:80 --name computerstore computerstore:latest
```

**Или через Docker Compose:**
```bash
docker-compose up -d
```

---

## ⚙️ Конфигурация

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ComputerStoreDb;Trusted_Connection=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ApplicationSettings": {
    "SiteName": "ComputerStore",
    "DefaultCurrency": "USD",
    "ShippingCost": 10.00
  }
}
```

### Переменные окружения (Production)

```bash
export ASPNETCORE_ENVIRONMENT=Production
export ConnectionStrings__DefaultConnection="Your Production Connection String"
```

---

## 📁 Структура проекта

```
ComputerStore/
│
├── 📁 ComputerStore.Domain/              # Доменная модель
│   ├── 📁 Entities/                      # Сущности
│   │   ├── Product.cs
│   │   ├── Category.cs
│   │   ├── Customer.cs
│   │   ├── Order.cs
│   │   ├── OrderItem.cs
│   │   ├── Review.cs                     # ⭐ Отзывы
│   │   ├── Payment.cs                    # 💳 Платежи
│   │   └── CartItem.cs
│   ├── 📁 Enums/                         # Перечисления
│   │   ├── OrderStatus.cs
│   │   ├── PaymentMethod.cs
│   │   ├── PaymentStatus.cs
│   │   └── ProductStatus.cs
│   └── 📁 Interfaces/                    # Интерфейсы
│
├── 📁 ComputerStore.Application/         # Бизнес-логика
│   ├── 📁 Services/                      # Сервисы
│   │   ├── ProductService.cs
│   │   ├── CategoryService.cs
│   │   ├── OrderService.cs
│   │   ├── ReviewService.cs              # ⭐ Сервис отзывов
│   │   ├── PaymentService.cs             # 💳 Сервис платежей
│   │   └── CustomerService.cs
│   ├── 📁 Abstractions/                  # Интерфейсы сервисов
│   ├── 📁 Mappings/                      # AutoMapper профили
│   │   └── MappingProfile.cs
│   └── 📁 Validators/                    # FluentValidation
│
├── 📁 ComputerStore.Infrastructure/      # Инфраструктура
│   ├── 📁 Data/                          # Контекст БД
│   │   ├── ApplicationDbContext.cs
│   │   ├── 📁 Configurations/           # EF Core конфигурации
│   │   │   ├── ProductConfiguration.cs
│   │   │   ├── ReviewConfiguration.cs    # ⭐
│   │   │   └── PaymentConfiguration.cs   # 💳
│   │   └── DbInitializer.cs             # Seed данных
│   ├── 📁 Repositories/                  # Репозитории
│   │   ├── ProductRepository.cs
│   │   ├── ReviewRepository.cs           # ⭐
│   │   └── PaymentRepository.cs          # 💳
│   └── 📁 Persistence/
│       └── UnitOfWork.cs                 # Unit of Work
│
├── 📁 ComputerStore.Shared/              # Общие компоненты
│   └── 📁 DTOs/                          # Data Transfer Objects
│       ├── ProductDto.cs
│       ├── ReviewDto.cs                  # ⭐
│       └── PaymentDto.cs                 # 💳
│
└── 📁 ComputerStore.Web/                 # Представление
    ├── 📁 Controllers/                   # Контроллеры
    │   ├── ProductsController.cs
    │   ├── OrdersController.cs
    │   ├── ReviewsController.cs          # ⭐ Отзывы
    │   └── PaymentController.cs          # 💳 Оплата
    ├── 📁 Areas/                         # Areas
    │   └── 📁 Admin/                     # Админ панель
    │       ├── 📁 Controllers/
    │       │   ├── ProductsController.cs
    │       │   ├── OrdersController.cs
    │       │   └── ReviewsController.cs  # ⭐ Модерация
    │       └── 📁 Views/
    ├── 📁 Views/                         # Представления
    │   ├── 📁 Products/
    │   ├── 📁 Orders/
    │   ├── 📁 Reviews/                   # ⭐
    │   ├── 📁 Payment/                   # 💳
    │   └── 📁 Shared/
    ├── 📁 Models/                        # ViewModels
    ├── 📁 wwwroot/                       # Статические файлы
    │   ├── 📁 css/
    │   ├── 📁 js/
    │   ├── 📁 images/
    │   └── 📁 lib/
    ├── Program.cs
    ├── appsettings.json
    └── Dockerfile                        # 🐳 Docker
```

---

## 💾 База данных

### Схема базы данных

```sql
┌─────────────────────────────────────────────────────────────────┐
│                         Database Schema                         │
└─────────────────────────────────────────────────────────────────┘

  ┌──────────────┐       ┌──────────────┐       ┌──────────────┐
  │  Categories  │◄──────│   Products   │──────►│ProductImages │
  └──────────────┘       └──────────────┘       └──────────────┘
         │                      │                       
         │                      ▼                       
         │              ┌──────────────┐               
         │              │ProductSpecs  │               
         │              └──────────────┘               
         │                      │                       
         │                      │                       
         ▼                      ▼                       
  ┌──────────────┐       ┌──────────────┐       
  │   Reviews    │◄──────│  OrderItems  │       
  └──────────────┘       └──────────────┘       
         │                      │                       
         │                      ▼                       
         │              ┌──────────────┐               
         │              │    Orders    │               
         │              └──────────────┘               
         │                      │                       
         │                      ▼                       
         │              ┌──────────────┐               
         ▼              │   Payments   │               
  ┌──────────────┐     └──────────────┘               
  │  Customers   │             │                       
  └──────────────┘             │                       
         │                      │                       
         ▼                      ▼                       
  ┌──────────────┐     ┌──────────────┐               
  │  CartItems   │     │AspNetUsers   │               
  └──────────────┘     └──────────────┘               
```

### Основные таблицы

| Таблица | Описание | Ключевые поля |
|---------|----------|---------------|
| **Products** | Товары | Id, Name, Price, Rating, ReviewCount |
| **Categories** | Категории | Id, Name, ParentCategoryId |
| **ProductSpecifications** | Характеристики | ProductId, Name, Value |
| **ProductImages** | Изображения | ProductId, ImageUrl, DisplayOrder |
| **Customers** | Покупатели | Id, UserId, FirstName, LastName |
| **Orders** | Заказы | Id, OrderNumber, Status, IsPaid |
| **OrderItems** | Позиции заказа | OrderId, ProductId, Quantity, Price |
| **Reviews** ⭐ | Отзывы | ProductId, CustomerId, Rating, IsApproved, IsDeleted |
| **Payments** 💳 | Платежи | OrderId, Amount, Status, TransactionId |
| **CartItems** | Корзина | CustomerId, ProductId, Quantity |

### Команды миграций

```bash
# Создание миграции
dotnet ef migrations add MigrationName --project ComputerStore.Infrastructure --startup-project ComputerStore.Web

# Применение миграций
dotnet ef database update --project ComputerStore.Infrastructure --startup-project ComputerStore.Web

# Откат миграции
dotnet ef database update PreviousMigrationName --project ComputerStore.Infrastructure --startup-project ComputerStore.Web

# Удаление последней миграции
dotnet ef migrations remove --project ComputerStore.Infrastructure --startup-project ComputerStore.Web

# Генерация SQL скрипта
dotnet ef migrations script --project ComputerStore.Infrastructure --startup-project ComputerStore.Web
```

---

## 👥 Пользователи по умолчанию

После первого запуска создаются следующие аккаунты:

### 👨‍💼 Администратор
```
Email:    admin@computerstore.com
Пароль:   Admin@123
Роль:     Admin
```

**Доступ:**
- Админ панель: `/Admin`
- Управление товарами, категориями, заказами
- Модерация отзывов
- Просмотр статистики

### 👤 Тестовый пользователь
```
Email:    user@test.com
Пароль:   User@123
Роль:     User
```

**Доступ:**
- Каталог товаров
- Корзина и оформление заказов
- Оставление отзывов
- Личный кабинет

---

## 🔌 API и сервисы

### Основные сервисы

#### ProductService
```csharp
Task<IEnumerable<ProductDto>> GetAllProductsAsync()
Task<ProductDto?> GetProductByIdAsync(int id)
Task<ProductDetailsDto?> GetProductDetailsAsync(int id)
Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm)
Task<ProductDto?> CreateProductAsync(CreateProductDto dto)
Task<ProductDto?> UpdateProductAsync(UpdateProductDto dto)
Task<bool> DeleteProductAsync(int id)
```

#### ReviewService ⭐
```csharp
Task<IEnumerable<ReviewDto>> GetProductReviewsAsync(int productId)
Task<ProductRatingDto> GetProductRatingAsync(int productId)
Task<IEnumerable<TopRatedProductDto>> GetTopRatedProductsAsync(int count)
Task<ReviewDto?> CreateReviewAsync(string userId, CreateReviewDto dto)
Task<ReviewDto?> UpdateReviewAsync(string userId, UpdateReviewDto dto)
Task<bool> DeleteReviewAsync(string userId, int reviewId)
Task<bool> MarkHelpfulAsync(int reviewId, bool isHelpful)
```

#### PaymentService 💳
```csharp
Task<PaymentDto?> CreatePaymentAsync(CreatePaymentDto dto)
Task<PaymentResultDto> ProcessPaymentAsync(ProcessPaymentDto dto)
Task<PaymentResultDto> ProcessCashPaymentAsync(int orderId)
Task<bool> CancelPaymentAsync(int paymentId)
```

---

## 🔧 Разработка

### Добавление новых функций

```bash
# Создание ветки
git checkout -b feature/new-feature

# Внесение изменений
# ... coding ...

# Коммит
git add .
git commit -m "Add: новая функция"

# Push
git push origin feature/new-feature

# Создание Pull Request на GitHub
```

### Стандарты кода

- ✅ Следуйте принципам SOLID
- ✅ Используйте async/await
- ✅ Комментируйте сложную логику
- ✅ Пишите unit тесты
- ✅ Используйте meaningful имена

### Тестирование

```bash
# Запуск тестов (когда будут добавлены)
dotnet test
```

---

## 📝 Лицензия

Этот проект распространяется под лицензией **MIT**. 

См. файл [LICENSE](LICENSE) для подробностей.

---

## 👨‍💻 Автор

**Ваше Имя**

- 🌐 GitHub: [@your-username](https://github.com/your-username)
- 📧 Email: your.email@example.com
- 💼 LinkedIn: [Your Profile](https://linkedin.com/in/your-profile)

---

## 🤝 Вклад в проект

Contributions, issues и feature requests приветствуются!

1. Fork проекта
2. Создайте ветку (`git checkout -b feature/AmazingFeature`)
3. Commit изменения (`git commit -m 'Add some AmazingFeature'`)
4. Push в ветку (`git push origin feature/AmazingFeature`)
5. Откройте Pull Request

---

## 📞 Поддержка

Если у вас есть вопросы:

- 📝 Создайте [Issue](https://github.com/your-username/ComputerStore/issues)
- 📧 Напишите на: support@computerstore.com
- 💬 Telegram: [@your_telegram](https://t.me/your_telegram)

---

## 🙏 Благодарности

- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [AutoMapper](https://automapper.org/)
- [FluentValidation](https://fluentvalidation.net/)
- [Bootstrap](https://getbootstrap.com/)
- [Bootstrap Icons](https://icons.getbootstrap.com/)

---

## 📊 Статистика проекта

```
📂 Всего файлов:     200+
💻 Строк кода:       15,000+
🧩 Компоненты:       50+
⚡ API endpoints:    40+
📱 Страницы:         30+
```

---

**Made with ❤️ using ASP.NET Core 10.0**

*ComputerStore © 2024. All rights reserved.*
