# Find That Book 📚

A .NET 8 book discovery application that uses AI + Open Library API to find books from messy user queries.

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
API Layer          → Controllers, HTTP concerns
Infrastructure     → Gemini client, Open Library client
Application        → Use cases, matching logic, interfaces
Domain             → Entities (Book), Value Objects (Author), Business rules
```

**Why?**
- Testable without external APIs
- Easy to swap AI providers or add new book sources
- Professional, maintainable code structure

**What we avoided:** CQRS, Event Sourcing, Mediator (over-engineering for 4-6 hours)

---

## 🚀 Quick Start

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Gemini API Key](https://makersuite.google.com/app/apikey)

### Setup

```bash
git clone https://github.com/PaulMec/FindThatBook.git
cd FindThatBook
dotnet restore

# Configure API key
cd src/FindThatBook.Api
dotnet user-secrets set "AIProvider:ApiKey" "YOUR_KEY_HERE"

# Run
dotnet run --project src/FindThatBook.Api
```

Open: `https://localhost:7001/swagger`

### Run Tests

```bash
dotnet test
```

---

## 📡 API Example

**POST** `/api/books/search`

```json
{
  "query": "tolkien hobbit illustrated deluxe 1937"
}
```

**Response:**

```json
{
  "results": [
    {
      "title": "The Hobbit",
      "author": "J.R.R. Tolkien",
      "firstPublishYear": 1937,
      "openLibraryId": "/works/OL27516W",
      "openLibraryUrl": "https://openlibrary.org/works/OL27516W",
      "coverUrl": "https://covers.openlibrary.org/b/id/12345-L.jpg",
      "explanation": "Exact title match; Tolkien is primary author"
    }
  ],
  "query": "tolkien hobbit illustrated deluxe 1937",
  "extraction": {
    "title": "The Hobbit",
    "author": "Tolkien",
    "keywords": ["illustrated", "deluxe"],
    "year": 1937
  }
}
```

---

## 🎯 What's Implemented

### ✅ Phase 1: Domain Layer (Completed)

**Core entities and business logic:**
- `Book` entity with methods like `HasAuthor()`, `GetNormalizedTitle()`
- `Author` and `SearchQuery` value objects (immutable, self-normalizing)
- `BookMatch` record with factory methods for different confidence levels
- `MatchStrength` enum (Strongest → VeryWeak)
- Custom exception hierarchy for better error handling

**Design decisions:**
- Records for immutability (prevents bugs)
- Validation in constructors (fail fast)
- Zero dependencies (pure business logic)

### 🔄 Phase 2: Application Layer (In Progress)

Will be updated after completion.

---

## 🎨 Key Design Decisions

| Decision | Why |
| -------- | --- |
| **Clean Architecture** | Testability, flexibility, demonstrates senior thinking |
| **Records for Value Objects** | Immutability + equality by value |
| **Factory methods in BookMatch** | Self-documenting, prevents inconsistencies |
| **Custom exceptions with metadata** | Better debugging (includes query, status code, etc.) |
| **AI fields are optional** | Handles "only author" or "only title" queries |

---

## 🧪 Testing Strategy

- **Unit tests:** Domain logic, matching algorithms (fast, isolated)
- **Integration tests:** Full API flow with mocked external services
- **Framework:** xUnit + Moq + FluentAssertions

---

## 📖 Assumptions

- English-only queries
- Open Library API is available (no fallback for MVP)
- Gemini AI returns parseable JSON
- Single-user application (no auth)

---

## 👤 Author

**Paul Mec**  
Technical Assessment for InfoTrack

[github.com/PaulMec/FindThatBook](https://github.com/PaulMec/FindThatBook)