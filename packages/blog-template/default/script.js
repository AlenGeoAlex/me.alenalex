/* ═══════════════════════════════════════════════════════════════
   SCRIPT.JS
   Progressive enhancement for the blog post template.
   Requires: postId, postSlug, postTitle globals set in template.html
   Depends on: Alpine.js (loaded in <head>), blog.css
═══════════════════════════════════════════════════════════════ */
const API = {
    series:   (id) => `/api/posts/${id}/series`,
    glossary: () => `/api/glossary/terms`,
};


function toggleDark() {
    const html = document.documentElement;
    html.classList.toggle('dark');
    const isDark = html.classList.contains('dark');
    localStorage.setItem('theme', isDark ? 'dark' : 'light');
    const icon = document.getElementById('theme-icon');
    if (icon) icon.textContent = isDark ? '☽' : '☀';
}


document.addEventListener('DOMContentLoaded', () => {
    setThemeIcon();
    initHighlightJs();
    buildToc();
    loadSeries();
});


function setThemeIcon() {
    const icon = document.getElementById('theme-icon');
    if (!icon) return;
    const isDark = document.documentElement.classList.contains('dark');
    icon.textContent = isDark ? '☽' : '☀';
}

function initHighlightJs() {
    if (typeof hljs !== 'undefined') {
        hljs.highlightAll();
    }
}


function buildToc() {
    const content    = document.getElementById('post-content');
    const tocSection = document.getElementById('toc-section');
    const tocList    = document.getElementById('toc-list');

    if (!content || !tocSection || !tocList) return;

    // Query headings in document order
    const headings = Array.from(
        content.querySelectorAll('.post-heading-1, .post-heading-2, .post-heading-3')
    );

    // Hide if too few headings to warrant a TOC
    if (headings.length < 2) return;

    // Ensure each heading has an id for anchor linking
    headings.forEach((heading, i) => {
        if (!heading.id) {
            heading.id = 'heading-' + i;
        }
    });

    // Build the TOC links
    const fragment = document.createDocumentFragment();

    headings.forEach((heading) => {
        const level = getTocLevel(heading);
        const link  = document.createElement('a');

        link.href        = '#' + heading.id;
        link.textContent = heading.textContent;
        link.className   = `toc-item toc-h${level}`;
        link.dataset.id  = heading.id;

        link.addEventListener('click', (e) => {
            e.preventDefault();
            heading.scrollIntoView({ behavior: 'smooth', block: 'start' });
        });

        fragment.appendChild(link);
    });

    tocList.appendChild(fragment);
    tocSection.style.display = '';

    // Highlight active heading on scroll
    initTocObserver(headings);
}

function getTocLevel(heading) {
    if (heading.classList.contains('post-heading-1')) return 1;
    if (heading.classList.contains('post-heading-2')) return 2;
    if (heading.classList.contains('post-heading-3')) return 3;
    return 2;
}

function initTocObserver(headings) {
    const tocLinks = document.querySelectorAll('.toc-item');
    if (!tocLinks.length) return;

    // Track the heading nearest to the top of the viewport
    let activeId = null;

    const observer = new IntersectionObserver(
        (entries) => {
            entries.forEach((entry) => {
                if (entry.isIntersecting) {
                    setActiveTocItem(entry.target.id);
                }
            });
        },
        {
            // Trigger when heading reaches the top 20% of the viewport
            rootMargin: '0px 0px -80% 0px',
            threshold: 0,
        }
    );

    headings.forEach((h) => observer.observe(h));
}

function setActiveTocItem(id) {
    document.querySelectorAll('.toc-item').forEach((link) => {
        if (link.dataset.id === id) {
            link.classList.add('active');
        } else {
            link.classList.remove('active');
        }
    });
}


/* ─────────────────────────────────────────────────────────────
   SERIES
   Calls GET /api/posts/{postId}/series
   Expected response shape on success:
   {
     "seriesTitle": "string",
     "currentEpisode": 1,
     "posts": [
       {
         "title":       "string",
         "slug":        "string",
         "excerpt":     "string",
         "publishedAt": "ISO string | null"  — null means upcoming/not yet published
       }
     ]
   }
   On 404 or any error: all series UI stays hidden.
───────────────────────────────────────────────────────────────  */
async function loadSeries() {
    if (!postId) return;

    let data;

    try {
        const res = await fetch(API.series(postId), { credentials: 'omit' });
        if (!res.ok) return; // 404 = not in a series, silently bail
        data = await res.json();
    } catch {
        return; // network error — silently bail
    }

    if (!data || !Array.isArray(data.posts) || data.posts.length === 0) return;

    renderSeriesSidebar(data);
    renderBreadcrumbSeries(data.seriesTitle);
    renderSeriesNav(data);
}


function renderSeriesSidebar(data) {
    const section  = document.getElementById('series-section');
    const titleEl  = document.getElementById('series-title');
    const listEl   = document.getElementById('series-list');

    if (!section || !titleEl || !listEl) return;

    titleEl.textContent = data.seriesTitle || 'Series';

    const fragment = document.createDocumentFragment();

    data.posts.forEach((post, index) => {
        const episodeNum  = index + 1;
        const isCurrent   = episodeNum === data.currentEpisode;
        const isPublished = !!post.publishedAt;

        const li   = document.createElement('li');
        li.className = 'series-list-item';

        const epNum = document.createElement('span');
        epNum.className   = 'series-episode-num';
        epNum.textContent = String(episodeNum).padStart(2, '0');

        const link = document.createElement(isPublished && !isCurrent ? 'a' : 'span');
        link.className   = 'series-list-link' +
            (isCurrent   ? ' current'  : '') +
            (!isPublished ? ' upcoming' : '');
        link.textContent = post.title;
        link.title       = post.excerpt || post.title;

        if (isPublished && !isCurrent && link.tagName === 'A') {
            link.href = '/blog/' + post.slug;
        }

        li.appendChild(epNum);
        li.appendChild(link);
        fragment.appendChild(li);
    });

    listEl.appendChild(fragment);
    section.style.display = '';
}


function renderBreadcrumbSeries(seriesTitle) {
    if (!seriesTitle) return;

    const seriesCrumb    = document.getElementById('breadcrumb-series');
    const seriesCrumbSep = document.getElementById('breadcrumb-series-sep');

    if (!seriesCrumb || !seriesCrumbSep) return;

    seriesCrumb.textContent  = seriesTitle;
    seriesCrumb.style.display = '';
    seriesCrumbSep.style.display = '';
}


function renderSeriesNav(data) {
    const navEl   = document.getElementById('series-nav');
    const prevEl  = document.getElementById('series-nav-prev');
    const nextEl  = document.getElementById('series-nav-next');

    if (!navEl || !prevEl || !nextEl) return;

    const currentIndex = data.currentEpisode - 1; // zero-based
    const prevPost     = currentIndex > 0 ? data.posts[currentIndex - 1] : null;
    const nextPost     = currentIndex < data.posts.length - 1 ? data.posts[currentIndex + 1] : null;

    let hasNav = false;

    if (prevPost && prevPost.publishedAt) {
        prevEl.innerHTML = buildNavLink(prevPost, currentIndex, 'prev');
        hasNav = true;
    }

    if (nextPost) {
        nextEl.innerHTML = buildNavLink(nextPost, currentIndex + 2, 'next');
        hasNav = true;
    }

    if (hasNav) {
        navEl.style.display = 'grid';
    }
}

function buildNavLink(post, episodeNum, direction) {
    const isPublished  = !!post.publishedAt;
    const label        = direction === 'prev' ? '← Previous' : 'Next →';
    const episodeLabel = `Episode ${episodeNum}`;

    if (!isPublished) {
        // upcoming — render as a non-clickable card
        return `
      <div class="series-nav-link" style="opacity:0.45;cursor:default;">
        <span class="series-nav-direction">${label}</span>
        <span class="series-nav-title">${escapeHtml(post.title)}</span>
        <span class="series-nav-episode">${episodeLabel} · Coming soon</span>
      </div>
    `;
    }

    return `
    <a href="/blog/${escapeHtml(post.slug)}" class="series-nav-link">
      <span class="series-nav-direction">${label}</span>
      <span class="series-nav-title">${escapeHtml(post.title)}</span>
      <span class="series-nav-episode">${episodeLabel}</span>
    </a>
  `;
}


function escapeHtml(str) {
    if (!str) return '';
    return str
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#039;');
}