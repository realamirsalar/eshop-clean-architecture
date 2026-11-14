# E-Shop Modular API & Testing Architecture 🛒⚡

[Persian (فارسی)](#-فارسی) | [English](#-english)

---

## 🇬🇧 English

A high-performance, resilient, and strictly decoupled E-Commerce RESTful Backend built with **ASP.NET Core Web API** and **Clean Architecture**. This project serves as an architectural showcase demonstrating enterprise-grade patterns including **CQRS**, automated **MediatR Cross-Cutting Pipelines** (Distributed Redis Caching, Cache Invalidation, and Validation), the **Result Pattern**, and a production-like **Testing Trophy Strategy** powered by **Docker Testcontainers** and **Respawner**.

### 🚀 Key Engineering Features
- **Automated Distributed Caching Pipeline**: Transparent query caching using **Redis** (`IDistributedCache`) via `CachePipelineBehavior` and the `ICacheable` contract, featuring configurable sliding/absolute expirations, cache-bypass capabilities, and graceful fallback to PostgreSQL if the Redis cluster is unreachable.
- **Automated Cache Invalidation Pipeline**: Eventual consistency and zero stale data guaranteed via `CacheInvalidationPipelineBehavior` and `ICacheInvalidator`. State-changing commands automatically purge target keys (e.g., list and entity-specific keys) only upon successful database commits.
- **Fail-Fast Validation Pipeline**: Incoming requests are intercepted by `ValidationPipelineBehavior` (`IValidatable`) and evaluated against **FluentValidation** rules before ever reaching application handlers.
- **Result Pattern Over Exceptions**: Replaces costly exception-driven control flow with **FluentResults** (`Result<T>`), using strongly-typed domain errors (`NotFoundError`, `ConflictError`, `ValidationError`) mapped cleanly to standard HTTP status codes in `ApiBaseController`.
- **Containerized End-to-End Integration Testing**: Real-world API integration tests (`WebApplicationFactory<IApiMarker>`) spinning up isolated **PostgreSQL 16** and **Redis 7** Docker containers via **Testcontainers**, combined with **Respawner** for lightning-fast database state resets and Redis `FlushAllDatabasesAsync` between test runs.
- **Isolated Unit Testing Suite**: Comprehensive, millisecond-fast unit tests covering pipeline behaviors (resiliency, cache hits/misses, failure short-circuiting) and isolated property-level validator tests using **xUnit**, **NSubstitute**, **FluentAssertions**, and **Bogus**.
- **Global Exception Handling**: Centralized fallback error management (`GlobalExceptionHandler`) formatting unhandled runtime exceptions into standard RFC ProblemDetails responses.

### 🏗️ Architecture & Project Structure
The solution strictly adheres to **Clean Architecture** and **Separation of Concerns (SoC)** across four core source layers and two dedicated test suites:

1. **Domain Layer (`E-Shop.Domain`)**: Contains core enterprise entities (`Category`) free of any external framework dependencies.
2. **Application Layer (`E-Shop.Application`)**: Orchestrates business use cases via **CQRS** (`Commands` & `Queries`), **MediatR Pipeline Behaviors** (`CachePipelineBehavior`, `CacheInvalidationPipelineBehavior`, `ValidationPipelineBehavior`), **FluentValidation** rules, **AutoMapper** profiles, and repository contracts (`ICategoryRepository`, `IUnitOfWork`).
3. **Infrastructure Layer (`E-Shop.Infrastructure`)**: Implements data persistence using **Entity Framework Core** with **PostgreSQL**, Fluent API entity configurations (`CategoryConfiguration`), migrations, and concrete `Repository` & `UnitOfWork` implementations.
4. **API Layer (`E-Shop.Api`)**: Presentation entry point featuring versioned RESTful controllers (`v1/CategoryController`), request contracts (`CreateCategoryRequest`, `UpdateCategoryRequest`), `ApiBaseController` result-to-HTTP mapping, and DI composition.

#### 🧪 Testing Architecture (The Testing Trophy)
- **Integration Tests (`E-Shop.Api.Tests.Integration`)**:
  - `ApplicationApiFactory`: Manages lifecycle of `PostgreSqlContainer` (`postgres:16-alpine`) and `RedisContainer` (`redis:7-alpine`), applies EF Core migrations on startup, and exposes `ResetDatabaseAsync()` using `Respawner`.
  - `SharedTestCollection` & `BaseIntegrationTest`: Shares container instances across test classes via xUnit `ICollectionFixture` to maximize CI/CD execution speed while guaranteeing 100% test isolation before every `[Fact]`.
  - **Covered Scenarios**: Full CRUD happy paths, database persistence verification, out-of-band DB updates proving Redis cache hits, automatic cache invalidation after `PUT`/`POST`/`DELETE`, and HTTP 400/404 failure mappings.
- **Unit Tests (`E-Shop.Application.Tests.Unit`)**:
  - `PipelineBehaviorTests`: Mocks `IDistributedCache` and `RequestHandlerDelegate` via `NSubstitute` to verify cache bypass, cache hit short-circuiting, non-caching of failed `Result<T>` responses, and Redis connection failure resiliency.
  - `ValidatorTests`: Utilizes `FluentValidation.TestHelper` (`TestValidate`) to test boundary conditions on every command/query property in complete isolation.

### 💻 Tech Stack & Libraries
- **Framework**: C#, .NET, ASP.NET Core Web API
- **Architecture & Patterns**: Clean Architecture, CQRS (MediatR), Repository & Unit of Work, Result Pattern (FluentResults)
- **Database & Caching**: PostgreSQL, Entity Framework Core (Code-First), Redis (`IDistributedCache`, `StackExchange.Redis`)
- **Validation & Mapping**: FluentValidation, AutoMapper
- **Testing Ecosystem**: xUnit, Testcontainers (`PostgreSql`, `Redis`), Respawner, NSubstitute, FluentAssertions, Bogus (Faker)

---

## 🇮🇷 فارسی

یک بک‌اند فروشگاهی (E-Commerce RESTful API) با کارایی بالا، تاب‌آور (Resilient) و کاملاً ماژولار که با استفاده از **ASP.NET Core Web API** و اصول سخت‌گیرانه **Clean Architecture (معماری تمیز)** طراحی و پیاده‌سازی شده است. این پروژه به عنوان یک نمونه‌کار مهندسی پیشرفته، الگوهای سطح Enterprise از جمله **CQRS**، **پایپ‌لاین‌های خودکار MediatR** (کش توزیع‌شده Redis، ابطال خودکار کش و اعتبارسنجی)، **الگوی Result** و استراتژی تست‌نویسی مدرن (**Testing Trophy**) به کمک **Docker Testcontainers** و **Respawner** را به نمایش می‌گذارد.

### 🚀 ویژگی‌های کلیدی و مهندسی
- **پایپ‌لاین کش توزیع‌شده خودکار (Distributed Caching Pipeline)**: کش شدن شفاف کوئری‌ها در **Redis** از طریق `CachePipelineBehavior` و قرارداد `ICacheable`؛ همراه با قابلیت تنظیم زمان انقضا (Sliding/Absolute)، امکان دور زدن کش (`BypassCache`) و مکانیزم **Fallback خودکار به دیتابیس** در صورت قطع بودن سرور ردیس (Resiliency) بدون کرش کردن API.
- **پایپ‌لاین ابطال خودکار کش (Cache Invalidation Pipeline)**: تضمین یکپارچگی داده‌ها و جلوگیری از خواندن دیتای کهنه (Stale Data) به کمک `CacheInvalidationPipelineBehavior` و اینترفیس `ICacheInvalidator`. کامندهای تغییر وضعیت (Create, Update, Delete) بدون درگیر کردن هندلر به منطق کش (رعایت اصل SRP)، تنها در صورت موفقیت عملیات دیتابیس، کلیدهای مرتبط را به صورت غیرهمزمان از Redis پاک می‌کنند.
- **پایپ‌لاین اعتبارسنجی (Fail-Fast Validation Pipeline)**: رهگیری تمام درخواست‌های ورودی توسط `ValidationPipelineBehavior` (`IValidatable`) و بررسی قوانین **FluentValidation** پیش از رسیدن درخواست به لایه هندلرها و دیتابیس.
- **مدیریت خطا با الگوی Result (بدون استفاده از Exceptionهای پرهزینه)**: جایگزینی Exceptionهای کنترلی با کتابخانه **FluentResults** (`Result<T>`) و خطاهای تایپ‌شده (`NotFoundError`، `ConflictError`، `ValidationError`) که در `ApiBaseController` به صورت تمیز به کدهای وضعیت استاندارد HTTP (مانند 200, 201, 400, 404) نگاشت می‌شوند.
- **تست‌های یکپارچه‌سازی واقعی با کانتینرهای داکر (Integration Testing)**: اجرای تست‌های End-to-End واقعی به کمک `WebApplicationFactory` که در آن نمونه‌های واقعی دیتابیس **PostgreSQL 16** و **Redis 7** توسط **Testcontainers** در داکر بالا می‌آیند و وضعیت دیتابیس و کش بین هر تست توسط **Respawner** و `FlushAllDatabasesAsync` در کسری از میلی‌ثانیه ریست می‌شود.
- **مجموعه تست‌های واحد ایزوله (Unit Testing Suite)**: پوشش کامل رفتار پایپ‌لاین‌ها (تست تاب‌آوری در برابر قطعی ردیس، عدم کش شدن خطاها، تست Cache Hit/Miss) و تست تک‌تک پراپرتی‌های ولیدیتورها با استفاده از **xUnit**، **NSubstitute**، **FluentAssertions** و **Bogus**.
- **مدیریت متمرکز خطاها (Global Exception Handling)**: پیاده‌سازی `GlobalExceptionHandler` برای مدیریت خطاهای پیش‌بینی‌نشده زمان اجرا و بازگرداندن خروجی استاندارد.

### 🏗️ معماری و ساختار لایه‌ها
این پروژه با رعایت کامل اصل جداسازی وظایف (Separation of Concerns) در ۴ لایه اصلی و ۲ پروژه تست مجزا ساختاردهی شده است:

1. **لایه Domain (`E-Shop.Domain`)**: شامل موجودیت‌های اصلی سیستم (`Category`) بدون هیچ‌گونه وابستگی به فریم‌ورک‌های خارجی.
2. **لایه Application (`E-Shop.Application`)**: قلب منطق برنامه شامل هندلرهای **CQRS** (`Commands` و `Queries`)، پایپ‌لاین‌های میان‌بر **MediatR**، قوانین **FluentValidation**، پروفایل‌های **AutoMapper** و قراردادهای دسترسی به داده (`ICategoryRepository` و `IUnitOfWork`).
3. **لایه Infrastructure (`E-Shop.Infrastructure`)**: پیاده‌سازی دسترسی به دیتابیس **PostgreSQL** با استفاده از **EF Core**، تنظیمات جداول با Fluent API (`CategoryConfiguration`)، مایگریشن‌ها و پیاده‌سازی `Repository` و `UnitOfWork`.
4. **لایه API (`E-Shop.Api`)**: نقطه ورود برنامه شامل کنترلرهای نسخه‌بندی‌شده (`v1/CategoryController`)، مدل‌های Request، کلاس پایه `ApiBaseController` و تنظیمات Dependency Injection.

#### 🧪 معماری تست‌ها (استراتژی Testing Trophy)
- **پروژه Integration Tests (`E-Shop.Api.Tests.Integration`)**:
  - `ApplicationApiFactory`: مدیریت چرخه حیات کانتینرهای `postgres:16-alpine` و `redis:7-alpine`، اجرای خودکار مایگریشن‌ها و پاکسازی جداول و کش بین تست‌ها.
  - `SharedTestCollection` و `BaseIntegrationTest`: اشتراک‌گذاری کانتینرها بین کلاس‌های تست با استفاده از `ICollectionFixture` برای به حداکثر رساندن سرعت اجرای تست‌ها همراه با ایزولاسیون ۱۰۰٪ داده‌ها پیش از هر تست.
- **پروژه Unit Tests (`E-Shop.Application.Tests.Unit`)**:
  - `PipelineBehaviorTests`: شبیه‌سازی (Mock) سرویس کش و هندلرها با `NSubstitute` برای اثبات صحت عملکرد پایپ‌لاین‌ها در سناریوهای مرزی و خطا.
  - `ValidatorTests`: اعتبارسنجی ایزوله تک‌تک قوانین ورودی به کمک `FluentValidation.TestHelper`.

### 💻 تکنولوژی‌ها و ابزارهای استفاده‌شده
- **بک‌اند**: C#, .NET, ASP.NET Core Web API
- **معماری و الگوها**: Clean Architecture, CQRS (MediatR), Repository & Unit of Work, Result Pattern (FluentResults)
- **دیتابیس و کش**: PostgreSQL, Entity Framework Core (Code-First), Redis (`IDistributedCache`, `StackExchange.Redis`)
- **اعتبارسنجی و مپینگ**: FluentValidation, AutoMapper
- **اکوسیستم تست**: xUnit, Testcontainers (`PostgreSql`, `Redis`), Respawner, NSubstitute, FluentAssertions, Bogus
