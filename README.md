# Find That Book 📚

A .NET 8 book discovery application that uses AI + Open Library API to find books from messy user queries.

---

## 🎯 Overview

Given messy blobs like “tolkien hobbit illustrated deluxe 1937” or “mark huckleberry”, this app:

1) Uses AI (Gemini) to extract fields → `{ title?, author?, keywords[], year? }`  
2) Queries Open Library for candidates  
3) Applies a matching hierarchy with normalization (lowercase, diacritics, partials)  
4) Returns the top 5 results with a concise explanation (“why it matched”)  

Tech Stack: .NET 8, ASP.NET Core Web API, Gemini AI, Open Library API, xUnit, Tailwind (CDN) for Web UI

---

## 🏗️ Architecture (Clean Architecture)

```text
┌─────────────────────────────────────────────────────────────┐
│                        API Layer                            │
│        Controllers, HTTP concerns, Swagger & static files   │
├─────────────────────────────────────────────────────────────┤
│                    Infrastructure Layer                     │
│   GeminiAIProvider, OpenLibraryClient, Matching & Dedup     │
├─────────────────────────────────────────────────────────────┤
│                    Application Layer                        │
│            Use Cases, DTOs, Interfaces                      │
├───────────────────────────────────────────���─────────────────┤
│                      Domain Layer                           │
│   Entities (Book), Value Objects (Author), Enums/Records    │
└─────────────────────────────────────────────────────────────┘
```

Why this structure:
- Testable without external APIs
- Easy to swap AI provider or add new book sources
- Maintainable, professional code layout
- Clear separation of concerns (Domain/Application/Infrastructure/API)

---

## 🚀 Quick Start

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Gemini API Key](https://aistudio.google.com/app/apikey)

### Setup

```powershell
# Clone the repository
git clone https://github.com/PaulMec/FindThatBook.git
cd FindThatBook

# Restore dependencies
dotnet restore

# Configure your Gemini API key (stored securely via User Secrets)
dotnet user-secrets init --project src/FindThatBook.Api
dotnet user-secrets set "Gemini:ApiKey" "YOUR_GEMINI_API_KEY_HERE" --project src/FindThatBook.Api

# Build
dotnet build

# Run (API + Web UI)
dotnet run --project src/FindThatBook.Api
```

- Web UI: `http://localhost:5200/`  
- Swagger UI: `http://localhost:5200/swagger`

### Tests

```powershell
dotnet test
```

Current status: 30/30 unit tests green ✅

---

## 📡 API Usage

### POST /api/books/search

Request:
```json
{
  "query": "tolkien hobbit"
}
```

Response (example):
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
      "explanation": "Exact title match; J.R.R. Tolkien is primary author."
    }
  ],
  "message": null
}
```

### GET /api/books/health

```json
{
  "status": "healthy",
  "timestamp": "2026-02-02T00:00:00Z"
}
```

---

## 🎯 Features by Phases

### ✅ Phase 1: Core Domain
- `Book`:
  - `HasAuthor()` with flexible partial matching (word-based, bidirectional contains)
  - `GetOpenLibraryUrl()`
  - `GetNormalizedTitle()` → lowercase + diacritics removal (e.g., “años” → “anos”)
- `Author`:
  - `GetNormalizedName()` → lowercase + diacritics removal (e.g., “García Márquez” → “garcia marquez”)
- `BookMatch` (record) with confidence factories
- `MatchStrength` enum: Strongest → Strong → Medium → Weak → VeryWeak

### ✅ Phase 2: Application
- `SearchBooksUseCase` orchestrates: AI → OpenLibrary → Matcher → Ranker → Response
- Interfaces: `IAIFieldExtractor`, `IOpenLibraryClient`, `IBookMatcher`, `IBookRanker`
- DTOs: `SearchBooksRequest` (validated), `SearchBooksResponse`, `AIExtractionResult`

### ✅ Phase 3: Infrastructure (AI, Open Library, Matching)
- `GeminiAIProvider` (gemini-2.5-flash) for field extraction
- `OpenLibraryClient`:
  - `/search.json` for candidates
- `BookMatcher`:
  - Strategies: Title+Author, Title-only, Author-only, Keywords
  - Input normalization and partial matching
- `BookRanker`:
  - Orders by MatchStrength → Score, returns Top N (default: 5)

### ✅ Phase 4: API
- `BooksController` (search + health)
- Dependency Injection setup, Swagger/OpenAPI
- User Secrets for secure API keys

### ✅ Phase 5: Improvements, Error Handling, Matching Enhancements
- Explanations now in English and cite concrete fields:
  - “Exact title match; J.R.R. Tolkien is primary author.”
  - “Author match only; showing top works by Gabriel García Márquez.”
  - “Partial title match (95% similarity); author not confirmed.”
  - “Keyword match: magic school found in title/description.”
- Error handling:
  - 429 rate limit → friendly message (no 500)
  - AI service unavailable → graceful degradation
  - No results / no exact matches → informative suggestions
  - `SearchBooksResponse.message` added for user-facing feedback
- Title matching:
  - Diacritics removal, basic article handling (“The”), basic subtitle handling (“Book: Subtitle” → “Book”)
  - Jaccard similarity for partials + percentage score
- Author matching:
  - Diacritics normalization, multi-word partials, bidirectional contains
- Open Library enhancements:
  - `/works/{work_id}.json` → work details/description
  - `/authors/{author_id}.json` → author info
  - `/authors/{author_id}/works.json` → top works by author
- De-duplication:
  - `BookDeduplicator` integrated into ranking pipeline:
    - Prefers highest strength, has cover, oldest first publish year, has description

### ✅ Phase 6: FrontEnd — Modern Web UI (served by API)

- Tailwind CSS via CDN, gradient background (purple/slate), glassmorphism, animations
- Large search input, example query buttons, loading spinner
- Error banner and friendly messages
- Result cards: cover, title, author, year, explanation; links to Open Library
- Shows AI extraction info (what the AI understood)
- Responsive layout (mobile/desktop)
- Security:
  - Backend validation: `[Required]`, `[StringLength(500)]` on `SearchBooksRequest`
  - Frontend validation: max 500 chars prior to API call
  - XSS prevention: `escapeHtml()` for all dynamic DOM content
  - API key remains server-side only
- Files:
  - `src/FindThatBook.Api/wwwroot/index.html`
  - `src/FindThatBook.Api/wwwroot/js/app.js`
  - `src/FindThatBook.Api/wwwroot/css/styles.css`
- Program:
  - `UseStaticFiles()` and root redirect in `Program.cs`

AI assistance note (FrontEnd only):
- Due to time constraints, the Phase 6 FrontEnd implementation (UI scaffolding, Tailwind class composition, glassmorphism styles, animations, and DOM wiring in `app.js`) was largely produced with GitHub Copilot prompts.

### ✅ Phase 7: Unit Tests & Normalization
- 30 unit tests green covering:
  - Author and title normalization (diacritics)
  - Author partial matching (multi-word)
  - GetOpenLibraryUrl and entity validations
  - Ranking: strength ordering → score, Top 5 limit
- IntegrationTests project present (no test cases required for submission)

---

## 🧪 Testing Strategy

- Unit tests: Domain + Infrastructure (fast, isolated)
- xUnit + FluentAssertions
- No external network dependencies in unit tests

```powershell
dotnet test
```

---

## 🛡️ Error Handling & Edge Cases

- AI (Gemini):
  - Rate limit (429): friendly message in `SearchBooksResponse.message`, no crash
  - Service unavailable / timeout: graceful degradation with logging
  - Minor typos in author: recovered by normalization + partials
- Open Library:
  - Prioritize canonical `works.authors` (primary author); contributors treated as lower signal
  - No results: safe empty response and message
- API responses:
  - Consistent payloads: details + “why it matched”
- De-duplication:
  - Removes duplicates and selects best candidate based on strength, cover, year, description

---

## 📁 Project Structure

```
FindThatBook/
├── src/
│   ├── FindThatBook.Api/             # Controllers, Program.cs, wwwroot (Web UI), Swagger
│   ├── FindThatBook.Application/     # Use Cases, DTOs, Interfaces
│   ├── FindThatBook.Domain/          # Entities, Value Objects, Enums/Records
│   └── FindThatBook.Infrastructure/  # AI, OpenLibrary, Matching, Dedup
└── tests/
    ├── FindThatBook.UnitTests/
    └── FindThatBook.IntegrationTests/
```

---

## ✅ Submission Guidelines Compliance

| Guideline | Status |
|---|---|
| GitHub repository | ✅ |
| README with setup and run instructions | ✅ |
| Implementation overview | ✅ |
| Assumptions and design decisions | ✅ |
| Features implemented | ✅ (phased) |
| Testing strategy | ✅ |
| AI API setup (Gemini) with clear steps | ✅ |
| Live demo (if possible) | Optional (served Web UI at root) |

---

## 🔭 Future Improvements

- Canonical works and advanced author disambiguation
- Subtitle handling beyond basics (“There and Back Again”)
- Robust title similarity (Levenshtein/Jaro–Winkler)
- Caching and controlled retries for external clients
- Observability: correlation IDs, metrics
- End-to-end IntegrationTests with controlled stubs/mocks

---

## 👤 Author

- GitHub: [PaulMec](https://github.com/PaulMec)  
- Repository: [FindThatBook](https://github.com/PaulMec/FindThatBook)

---

## 🤖 AI Assistance

This project was developed with assistance from GitHub Copilot for:
- Code review focusing on SOLID principles and best practices
- Documentation writing and PR descriptions
- Design pattern recommendations
- Identifying potential bugs and improvement opportunities

Additional notes:
- FrontEnd (Phase 6): Due to time constraints, the Web UI (Tailwind class composition, glassmorphism styles, animations, and DOM wiring in `app.js`) was largely produced with Copilot prompts.
- Prompt refinement: The query-to-metadata extraction prompt used by the Gemini provider (and its strict JSON rules) was iteratively refined with AI to improve reliability, reduce hallucinations, and ensure conservative field extraction.
- Backend scope: AI assistance on the backend was limited to reviews, documentation, and design guidance; no business logic was auto-generated wholesale.

Code reviews from CodeRabbit were also considered to improve the implementation, with the developer making final decisions on how to address each recommendation.

Developer Ownership: All architecture decisions, business logic design, code implementation, and execution choices were made by the developer. AI tools served as reviewers and advisors.