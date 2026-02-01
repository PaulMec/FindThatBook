# Find That Book 📚

A .NET 8 book discovery application that uses AI + Open Library API to find books from messy user queries.

> 🤖 **Note:** This project was built with AI assistance (GitHub Copilot) for code generation, documentation, and best practices guidance.

## 🎯 Overview

Given messy queries like "tolkien hobbit illustrated deluxe 1937" or "mark huckleberry", this app:

1. Uses **AI (Gemini)** to extract fields → `{ title?, author?, keywords[], year? }`
2. Searches **Open Library API** for candidates
3. Matches and ranks results using a hierarchy
4. Returns top 5 books with explanations

**Tech Stack:** .NET 8, ASP.NET Core Web API, Gemini AI, Open Library API, xUnit

---

## 🏗️ Architecture: Clean Architecture

```text
┌─────────────────────────────────────────────────────────────┐
│                        API Layer                            │
│              Controllers, HTTP concerns, Swagger            │
├─────────────────────────────────────────────────────────────┤
│                    Infrastructure Layer                     │
│         GeminiAIProvider, OpenLibraryClient, Matching       │
├─────────────────────────────────────────────────────────────┤
│                    Application Layer                        │
│            Use Cases, DTOs, Interfaces                      │
├─────────────────────────────────────────────────────────────┤
│                      Domain Layer                           │
│      Entities (Book), Value Objects (Author), Enums         │
└─────────────────────────────────────────────────────────────┘
```

**Why Clean Architecture?**
- ✅ Testable without external APIs
- ✅ Easy to swap AI providers or add new book sources
- ✅ Professional, maintainable code structure
- ✅ Clear separation of concerns

---

## 🚀 Quick Start

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Gemini API Key](https://aistudio.google.com/app/apikey)

### Setup

```bash
# Clone the repository
git clone https://github.com/PaulMec/FindThatBook.git
cd FindThatBook

# Restore dependencies
dotnet restore

# Configure your Gemini API key (stored securely in User Secrets)
cd src/FindThatBook.Api
dotnet user-secrets init
dotnet user-secrets set "Gemini:ApiKey" "YOUR_GEMINI_API_KEY_HERE"
cd ../..

# Build
dotnet build

# Run
dotnet run --project src/FindThatBook.Api
```

Open Swagger UI: `https://localhost:5200/swagger`

### Run Tests

```bash
dotnet test
```

---

## 📡 API Usage

### Search Books

**POST** `/api/books/search`

**Request:**
```json
{
  "query": "tolkien hobbit"
}
```

**Response:**
```json
{
  "query": "tolkien hobbit",
  "extraction": {
    "title": "hobbit",
    "author": "tolkien",
    "year": null,
    "keywords": [],
    "hasTitle": true,
    "hasAuthor": true,
    "hasAnyField": true
  },
  "results": [
    {
      "title": "The Hobbit",
      "author": "J.R.R. Tolkien",
      "firstPublishYear": 1937,
      "openLibraryId": "/works/OL27482W",
      "openLibraryUrl": "https://openlibrary.org/works/OL27482W",
      "coverUrl": "https://covers.openlibrary.org/b/id/14627509-L.jpg",
      "explanation": "Coincidencia exacta del título; tolkien es el autor principal."
    }
  ]
}
```

### Health Check

**GET** `/api/books/health`

```json
{
  "status": "healthy",
  "timestamp": "2026-02-01T04:59:46Z"
}
```

---

## 🎯 Features Implemented

### ✅ Phase 1: Domain Layer
- `Book` entity with methods like `HasAuthor()`, `GetNormalizedTitle()`
- `Author` and `SearchQuery` value objects (immutable, self-normalizing)
- `BookMatch` record with factory methods for confidence levels
- `MatchStrength` enum (Strongest → VeryWeak)
- Custom exception hierarchy

### ✅ Phase 2: Application Layer
- `SearchBooksUseCase` orchestrating the search flow
- `IAIFieldExtractor` interface for AI abstraction
- `IOpenLibraryClient` interface for book source abstraction
- `IBookMatcher` and `IBookRanker` interfaces
- DTOs for requests and responses

### ✅ Phase 3: Infrastructure Layer
- `GeminiAIProvider` - Gemini AI integration for field extraction
- `OpenLibraryClient` - Open Library API integration
- `BookMatcher` - 4 matching strategies (Title+Author, Title-only, Author-only, Keywords)
- `BookRanker` - Orders by MatchStrength, returns top N

### ✅ Phase 4: API Layer
- `BooksController` with search and health endpoints
- Full Dependency Injection configuration
- Swagger/OpenAPI documentation
- User Secrets for secure API key storage

---

## 🎨 Key Design Decisions

| Decision | Why |
| -------- | --- |
| **Clean Architecture** | Testability, flexibility, demonstrates senior thinking |
| **Records for DTOs** | Immutability + cleaner code |
| **Factory methods in BookMatch** | Self-documenting, prevents inconsistencies |
| **Custom exceptions with metadata** | Better debugging (includes query, status code) |
| **User Secrets for API Key** | Security best practice - keys never in source code |
| **Options Pattern** | Type-safe configuration with validation |

---

## 🧪 Testing Strategy

- **Unit tests:** Domain logic, value objects (fast, isolated)
- **Integration tests:** Full API flow testing
- **Framework:** xUnit + FluentAssertions

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

---

## 📁 Project Structure

```
FindThatBook/
├── src/
│   ├── FindThatBook.Api/           # Controllers, Program.cs, Configuration
│   ├── FindThatBook.Application/   # Use Cases, DTOs, Interfaces
│   ├── FindThatBook.Domain/        # Entities, Value Objects, Enums
│   └── FindThatBook.Infrastructure/# External APIs, Matching Logic
├── tests/
│   ├── FindThatBook.UnitTests/
│   └── FindThatBook.IntegrationTests/
└── README.md
```

---

## 🔧 Configuration

Configuration is managed through `appsettings.json` and User Secrets:

```json
{
  "Gemini": {
    "ApiKey": "",  // Set via User Secrets
    "Model": "gemini-2.5-flash",
    "BaseUrl": "https://generativelanguage.googleapis.com/v1beta/models"
  },
  "OpenLibrary": {
    "BaseUrl": "https://openlibrary.org"
  }
}
```

---

## 📖 Assumptions & Limitations

- English-only queries (MVP)
- Open Library API availability (no fallback)
- Gemini AI returns parseable JSON
- Single-user application (no authentication)
- Top 5 results returned by default

---

## 👤 Author

**PaulMec**

[github.com/PaulMec/FindThatBook](https://github.com/PaulMec/FindThatBook)

---

## 🤖 AI Assistance

This project was developed with assistance from **GitHub Copilot** for:
- Code review focusing on SOLID principles and best practices
- Documentation writing and PR descriptions
- Design pattern recommendations
- Identifying potential bugs and improvement opportunities

Code reviews from **CodeRabbit** were also considered to improve the implementation, with the developer making final decisions on how to address each recommendation.

**Developer Ownership:** All architecture decisions, business logic design, code implementation, and execution choices were made by the developer. AI tools served as reviewers and advisors.