// Find That Book - Frontend Application

const API_URL = '/api/books/search';

// DOM Elements
const searchForm = document.getElementById('searchForm');
const searchInput = document.getElementById('searchInput');
const searchButton = document.getElementById('searchButton');
const buttonText = document.getElementById('buttonText');
const buttonLoader = document.getElementById('buttonLoader');
const resultsContainer = document.getElementById('results');
const noResults = document.getElementById('noResults');
const errorMessage = document.getElementById('errorMessage');
const errorText = document.getElementById('errorText');
const extractionInfo = document.getElementById('extractionInfo');
const extractionDetails = document.getElementById('extractionDetails');
const messageBanner = document.getElementById('messageBanner');
const messageText = document.getElementById('messageText');

// URL validation helper to prevent XSS
function isValidHttpUrl(string) {
    if (!string) return false;
    try {
        const url = new URL(string);
        return url.protocol === 'http:' || url.protocol === 'https:';
    } catch {
        return false;
    }
}

// Example query buttons
document.querySelectorAll('.example-query').forEach(button => {
    button.addEventListener('click', () => {
        searchInput.value = button.dataset.query;
        searchForm.dispatchEvent(new Event('submit'));
    });
});

// Search form submission
searchForm.addEventListener('submit', async (e) => {
    e.preventDefault();

    const query = searchInput.value.trim();
    await performSearch(query);
});

async function performSearch(query) {
    // Validación: campo vacío
    if (!query || query.trim().length === 0) {
        showError('Please enter a search query (title, author, or keywords).');
        return;
    }

    // Validación de seguridad
    if (query.length > 500) {
        showError('Query is too long. Maximum 500 characters allowed.');
        return;
    }

    // Reset UI
    hideAllMessages();
    showLoading(true);
    resultsContainer.innerHTML = '';

    try {
        const response = await fetch(API_URL, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ query }),
        });

        if (!response.ok) {
            let errorMessage = 'Search failed';
            try {
                const error = await response.json();
                errorMessage = error.detail || error.title || errorMessage;
            } catch {
                // Response wasn't JSON, use status text
                errorMessage = response.statusText || errorMessage;
            }
            throw new Error(errorMessage);
        }

        const data = await response.json();
        displayResults(data);

    } catch (error) {
        showError(error.message);
    } finally {
        showLoading(false);
    }
}
function displayResults(data) {
    // Show AI extraction info
    if (data.extraction) {
        showExtractionInfo(data.extraction);
    }

    // Show message if present
    if (data.message) {
        showMessage(data.message);
    }

    // Check for results
    if (!data.results || data.results.length === 0) {
        noResults.classList.remove('hidden');
        return;
    }

    noResults.classList.add('hidden');

    // Render book cards
    data.results.forEach((book, index) => {
        const card = createBookCard(book, index);
        resultsContainer.appendChild(card);
    });
}

function createBookCard(book, index) {
    const card = document.createElement('div');
    card.className = 'book-card bg-white/10 backdrop-blur-lg rounded-2xl overflow-hidden border border-white/20 flex flex-col sm:flex-row fade-in opacity-0';
    card.style.animationDelay = `${index * 0.1}s`;

    // Cover image - validate URL
    const coverHtml = isValidHttpUrl(book.coverUrl)
        ? `<img src="${book.coverUrl}" alt="${escapeHtml(book.title)}" class="w-full h-full object-cover">`
        : `<div class="cover-placeholder w-full h-full">📖</div>`;

    // Year display
    const yearHtml = book.firstPublishYear
        ? `<span class="text-slate-400">First published: ${book.firstPublishYear}</span>`
        : '';

    // Validate Open Library URL
    const openLibraryHref = isValidHttpUrl(book.openLibraryUrl) ? book.openLibraryUrl : '#';

    card.innerHTML = `
        <div class="cover-container w-full sm:w-48 h-48 sm:h-auto flex-shrink-0 bg-slate-800">
            ${coverHtml}
        </div>
        <div class="flex-1 p-6">
            <div class="flex flex-wrap items-start justify-between gap-2 mb-3">
                <h2 class="text-xl font-bold text-white">${escapeHtml(book.title)}</h2>
            </div>
            <p class="text-purple-300 font-medium mb-2">${escapeHtml(book.author)}</p>
            <div class="text-sm mb-4">
                ${yearHtml}
            </div>
            <div class="bg-white/5 rounded-lg p-3 mb-4">
                <p class="text-slate-300 text-sm italic">"${escapeHtml(book.explanation)}"</p>
            </div>
            <div class="flex flex-wrap gap-2">
                <a href="${openLibraryHref}" target="_blank" rel="noopener noreferrer"
                   class="inline-flex items-center gap-1 px-3 py-1.5 bg-purple-600/30 hover:bg-purple-600/50 text-purple-300 rounded-lg text-sm transition-colors">
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h6m0 0v6m0-6L10 14"></path>
                    </svg>
                    Open Library
                </a>
            </div>
        </div>
    `;

    return card;
}

function showExtractionInfo(extraction) {
    const parts = [];

    if (extraction.title) {
        parts.push(`<strong>Title:</strong> "${escapeHtml(extraction.title)}"`);
    }
    if (extraction.author) {
        parts.push(`<strong>Author:</strong> "${escapeHtml(extraction.author)}"`);
    }
    if (extraction.keywords && extraction.keywords.length > 0) {
        parts.push(`<strong>Keywords:</strong> ${extraction.keywords.map(k => escapeHtml(k)).join(', ')}`);
    }

    if (parts.length > 0) {
        extractionDetails.innerHTML = parts.join(' &nbsp;•&nbsp; ');
        extractionInfo.classList.remove('hidden');
    }
}
function showMessage(message) {
    messageText.textContent = message;
    messageBanner.classList.remove('hidden');
}

function showError(message) {
    errorText.textContent = message;
    errorMessage.classList.remove('hidden');
}

function hideAllMessages() {
    errorMessage.classList.add('hidden');
    noResults.classList.add('hidden');
    extractionInfo.classList.add('hidden');
    messageBanner.classList.add('hidden');
}

function showLoading(isLoading) {
    if (isLoading) {
        buttonText.classList.add('hidden');
        buttonLoader.classList.remove('hidden');
        searchButton.disabled = true;
        searchButton.classList.add('opacity-70', 'cursor-not-allowed');
    } else {
        buttonText.classList.remove('hidden');
        buttonLoader.classList.add('hidden');
        searchButton.disabled = false;
        searchButton.classList.remove('opacity-70', 'cursor-not-allowed');
    }
}

function escapeHtml(text) {
    if (!text) return '';
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

// Focus search input on page load
searchInput.focus();