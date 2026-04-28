// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener('DOMContentLoaded', () => {
    const searchForm = document.getElementById('navbarSearchForm');
    const searchInput = document.getElementById('navbarSearchInput');
    const searchButton = document.querySelector('.nav-search-button');

    if (!searchForm || !searchInput) {
        return;
    }

    if (searchButton) {
        searchButton.addEventListener('click', (event) => {
            const isCollapsed = searchInput.offsetWidth === 0 || window.getComputedStyle(searchInput).maxWidth === '0px';
            if (searchInput.value.trim() === '' && isCollapsed) {
                event.preventDefault();
                searchInput.focus();
            }
        });
    }

    searchForm.addEventListener('submit', (event) => {
        if (searchInput.value.trim() === '') {
            event.preventDefault();
        }
        else {
            searchInput.value = searchInput.value.trim();
        }
    });

    searchInput.addEventListener('keydown', (event) => {
        if (event.key === 'Escape') {
            searchInput.blur();
            searchInput.value = '';
        }
    });
});
