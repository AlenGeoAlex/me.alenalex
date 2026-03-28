# Angular App Design System

This document is the single source of truth for the visual design of the Angular app.
All components, colours, spacing, and effects are defined here. Angular components
reference CSS variables only — never hardcode hex values inside component styles.

---

## Fonts

Three fonts, each with a distinct role. Load all three from Google Fonts.

```
https://fonts.googleapis.com/css2?family=Instrument+Serif:ital@0;1&family=Geist:wght@300;400;500&family=Geist+Mono:wght@400;500&display=swap
```

| Font | CSS family | Role |
|---|---|---|
| Instrument Serif | `font-serif` | Display headings, post titles, logo |
| Geist | `font-sans` | Body text, UI labels, navigation |
| Geist Mono | `font-mono` | Tags, badges, inline code, meta info |

In Tailwind config:

```javascript
fontFamily: {
  serif: ['Instrument Serif', 'Georgia', 'serif'],
  sans:  ['Geist', 'sans-serif'],
  mono:  ['Geist Mono', 'monospace'],
}
```

---

## Colour Tokens

All values defined as CSS variables in `:root`. The `.dark` class on `<html>` overrides
each token. Swap a variable once and every component that references it updates.

### Backgrounds

| Variable | Light | Dark | Used for |
|---|---|---|---|
| `--bg` | `#F5F2ED` | `#0F0E0D` | Page canvas |
| `--bg-2` | `#EDE9E3` | `#1A1917` | Elevated surfaces, input fields |
| `--bg-3` | `#E5E1DA` | `#242220` | Pressed states, deepest surface |
| `--bg-glass` | `rgba(245,242,237,0.72)` | `rgba(15,14,13,0.72)` | Glass cards, nav, dropdowns |
| `--bg-glass-heavy` | `rgba(245,242,237,0.88)` | `rgba(15,14,13,0.90)` | Modals, sidebars |

### Text

| Variable | Light | Dark | Used for |
|---|---|---|---|
| `--ink` | `#1C1917` | `#F5F2ED` | Headings, primary labels |
| `--ink-2` | `#44403C` | `#D6D3D1` | Body text, descriptions |
| `--ink-3` | `#78716C` | `#78716C` | Muted text, placeholders, captions |

### Accent

Single accent colour. Change `--accent` to retheme the entire app.

| Variable | Light | Dark | Used for |
|---|---|---|---|
| `--accent` | `#D97706` | `#F59E0B` | Links, active states, highlights |
| `--accent-hover` | `#B45309` | `#FBBF24` | Hover state of accent elements |
| `--accent-dim` | `rgba(217,119,6,0.12)` | `rgba(245,158,11,0.12)` | Subtle accent tint on surfaces |
| `--accent-dim-2` | `rgba(217,119,6,0.20)` | `rgba(245,158,11,0.20)` | Stronger accent tint, badge bg |

### Semantic

| Variable | Colour | Background | Used for |
|---|---|---|---|
| `--success` | `#16A34A` | `rgba(22,163,74,0.08)` | Published status, success toast |
| `--warning` | `#D97706` | `rgba(217,119,6,0.08)` | Draft status, warning callout |
| `--danger` | `#DC2626` | `rgba(220,38,38,0.08)` | Delete actions, error states |
| `--info` | `#2563EB` | `rgba(37,99,235,0.08)` | Info callout, neutral status |

### Borders

| Variable | Light | Dark | Used for |
|---|---|---|---|
| `--border` | `rgba(28,25,23,0.10)` | `rgba(245,242,237,0.08)` | Default border on all surfaces |
| `--border-2` | `rgba(28,25,23,0.18)` | `rgba(245,242,237,0.14)` | Hover state border |
| `--border-focus` | `rgba(217,119,6,0.50)` | `rgba(245,158,11,0.45)` | Input focus ring |

---

## Effects

### Glass surface

The core visual language of the app. Every card, nav, modal, and dropdown uses this.
Glass only works when there is depth behind it — the background orbs provide that depth.

```css
.glass {
  background: var(--bg-glass);
  backdrop-filter: saturate(180%) blur(20px);
  -webkit-backdrop-filter: saturate(180%) blur(20px);
  border: 1px solid var(--border);
  border-radius: 14px;
}
```

Heavy variant for modals and sidebars that need more opacity:

```css
.glass-heavy {
  background: var(--bg-glass-heavy);
  backdrop-filter: saturate(200%) blur(32px);
  -webkit-backdrop-filter: saturate(200%) blur(32px);
  border: 1px solid var(--border);
}
```

### Background orbs

Three fixed positioned blurred radial gradients that sit behind all content at z-index 0.
They give the glass surfaces something to blur and refract, creating the ambient glow.
Place them in the root app shell component, `aria-hidden="true"`.

```
Orb 1 — top right,  700×700px, accent-dim colour,  opacity 1.0
Orb 2 — bottom left, 500×500px, stone gray,         opacity 1.0
Orb 3 — mid right,   400×400px, accent-dim colour,  opacity 0.5
```

### Shadows

| Variable | Value | Used for |
|---|---|---|
| `--shadow-sm` | `0 1px 3px rgba(0,0,0,0.06)` | Subtle lift on small elements |
| `--shadow-md` | `0 4px 16px rgba(0,0,0,0.08)` | Card hover, dropdowns |
| `--shadow-lg` | `0 12px 40px rgba(0,0,0,0.10)` | Modals, command palette |

Dark mode shadows use higher opacity — `0.20`, `0.30`, `0.40` respectively.

### Border radius

| Variable | Value | Used for |
|---|---|---|
| `--r-sm` | `6px` | Small elements, badges |
| `--r-md` | `10px` | Buttons, inputs, small cards |
| `--r-lg` | `14px` | Cards, glass surfaces |
| `--r-xl` | `20px` | Large cards, modals |
| `--r-full` | `999px` | Pills, tag badges, avatars |

---

## Typography Scale

| Class | Font | Size | Weight | Colour | Notes |
|---|---|---|---|---|---|
| `t-display` | Instrument Serif | clamp(2rem, 5vw, 3.5rem) | 400 | `--ink` | Page hero, post title |
| `t-title` | Instrument Serif | clamp(1.5rem, 3vw, 2rem) | 400 | `--ink` | Section heading |
| `t-heading` | Geist | 1.125rem | 500 | `--ink` | Card heading, modal title |
| `t-subheading` | Geist | 0.8125rem | 500 | `--accent` | Uppercase label, section tag |
| `t-body` | Geist | 1rem | 300 | `--ink-2` | Descriptions, body copy |
| `t-small` | Geist | 0.875rem | 400 | `--ink-3` | Captions, meta, timestamps |
| `t-mono` | Geist Mono | 0.875rem | 400 | `--accent` | Tags, code, IDs |

`t-subheading` is always uppercase with `letter-spacing: 0.06em`. Use it for section
labels, table column headers, and status indicators.

---

## Navigation

Sticky glass nav at the top of every page.

Structure: logo left, nav items centre, action buttons right.

```
[ ● Logo ]    [ Writing ]  [ About ]    [ New Post  ▾ Profile ]
```

- Height: `3.5rem`
- Background: `--bg-glass-heavy` with heavy blur
- Bottom border: `1px solid var(--border)`
- Logo: Instrument Serif, amber dot indicator
- Nav items: `0.875rem` Geist, `--ink-3` default, amber on active, glass tint on hover
- Active item: `color: var(--accent)`, `background: var(--accent-dim)`
- Transition: `color 0.15s`, `background 0.15s`

---

## Buttons

All buttons use Geist, `0.875rem`, and a `0.15s` transition on all properties.

| Variant | Background | Text | Border | Use for |
|---|---|---|---|---|
| Primary | `--accent` | white | none | Main CTA — publish, save, create |
| Secondary | `--bg-glass` | `--ink` | `--border` | Secondary actions |
| Ghost | transparent | `--ink-2` | transparent | Tertiary actions, nav items |
| Danger | `--danger-bg` | `--danger` | danger tint | Delete, destructive actions |

Hover on primary: `--accent-hover`, `translateY(-1px)`, `--shadow-md`.
Hover on secondary: `--bg-2`, `--border-2`, `translateY(-1px)`.
Disabled: `opacity: 0.45`, `cursor: not-allowed`.

Size variants:

| Size | Padding | Font size | Radius |
|---|---|---|---|
| Small | `0.3125rem 0.75rem` | `0.8125rem` | `--r-sm` |
| Default | `0.5rem 1.125rem` | `0.875rem` | `--r-md` |
| Large | `0.6875rem 1.5rem` | `0.9375rem` | `--r-lg` |
| Icon | `0.5rem` | — | `--r-md` |

---

## Badges and Tags

Glass pill shape. Geist Mono font. Used for post tags, status indicators, and counts.

```
Default  — glass bg, --border, --ink-3 text
Accent   — --accent-dim bg, accent border, --accent text
Success  — --success-bg, green border, --success text
Danger   — --danger-bg, red border, --danger text
Warning  — --warning-bg, amber border, --warning text
```

All badges use `border-radius: var(--r-full)` and `font-size: 0.75rem`.
Add a `::before` dot using `currentColor` for status badges with a live indicator.

---

## Cards

### Standard card

Glass surface with hover lift. Used for post list items, stat panels, and settings sections.

```
Background:    --bg-glass
Border:        1px solid --border → --border-2 on hover
Border radius: --r-xl (20px)
Padding:       1.5rem
Transition:    border-color 0.15s, box-shadow 0.15s
Hover:         --shadow-md
```

### Post card

Used on the home page and dashboard post list.

```
Cover image area:  10rem tall, --r-lg, gradient placeholder if no image
Tags row:          badge pills, flex wrap, gap 0.375rem
Title:             Instrument Serif, 1.25rem, --ink
Excerpt:           0.9rem, --ink-3, weight 300, 2-line clamp
Meta row:          avatar + author + dot + date, 0.8125rem, --ink-3
                   top border: 1px solid --border, margin-top: auto
```

### Stat card

Used on the admin dashboard for counts and quick metrics.

```
Value:   Instrument Serif, 2rem, --ink
Label:   0.8125rem, --ink-3, Geist
Trend:   0.8125rem Geist Mono, --success or --danger
```

---

## Form Inputs

All inputs share a consistent glass-surface style.

```
Background:    --bg-2
Border:        1px solid --border
Border radius: --r-md
Padding:       0.5625rem 0.875rem
Font:          Geist 0.9375rem, --ink
Placeholder:   --ink-3

Focus:
  outline: none
  border-color: --border-focus
  box-shadow: 0 0 0 3px var(--accent-dim)

Transition: border-color 0.15s, box-shadow 0.15s
```

Label: `0.875rem`, Geist 400, `--ink-2`, `margin-bottom: 0.375rem`.
Error text: `0.8125rem`, `--danger`, `margin-top: 0.375rem`.
Helper text: `0.8125rem`, `--ink-3`, `margin-top: 0.375rem`.

### Select

Same as input. Custom dropdown arrow using a CSS SVG background-image in `--ink-3`.

### Textarea

Same as input. `resize: vertical`. `min-height: 8rem`.

### Toggle / Switch

Track: `2rem × 1.125rem`, `--r-full`. Off: `--bg-3` background. On: `--accent` background.
Thumb: white circle, `1px solid var(--border)`, transitions `transform 0.2s`.

---

## Avatar

Circular element showing author initials or profile photo.

| Size | Diameter | Font |
|---|---|---|
| Small | 1.5rem | Geist Mono 0.65rem |
| Default | 2rem | Geist Mono 0.75rem |
| Large | 2.75rem | Geist Mono 0.875rem |

Background: `--bg-glass`. Border: `1px solid --border`. Text: `--accent`.
If photo present: `object-fit: cover`, `border-radius: 50%`.

---

## Status Indicators

Used in the post list dashboard to show draft vs published.

| Status | Badge variant | Text |
|---|---|---|
| Published | Success | published |
| Draft | Warning | draft |
| Scheduled | Info | scheduled |

All use the `badge-dot` pattern with a leading coloured dot.

---

## Toast Notifications

Appear bottom-right. Stack upward. Auto-dismiss after 4 seconds.
Entry: `translateY(8px)` → `translateY(0)`, `opacity 0` → `1`, `0.3s var(--ease)`.
Exit: reverse, `0.25s`.

```
Container:  glass-heavy, --r-lg, --shadow-lg
Min width:  260px, max width: 380px
Padding:    0.875rem 1.125rem
Layout:     flex, icon + message + dismiss button
Icon:       coloured dot matching semantic colour
Message:    0.9rem Geist, --ink-2
Dismiss:    ghost icon button, --ink-3
```

| Type | Icon colour | Border tint |
|---|---|---|
| Success | `--success` | `rgba(22,163,74,0.20)` |
| Error | `--danger` | `rgba(220,38,38,0.20)` |
| Warning | `--warning` | `rgba(217,119,6,0.20)` |
| Info | `--info` | `rgba(37,99,235,0.20)` |

---

## Modal / Dialog

```
Backdrop:   rgba(0,0,0,0.40) in light, rgba(0,0,0,0.60) in dark
            fadeIn 0.2s
Container:  glass-heavy, --r-xl, --shadow-lg
            scaleIn 0.3s var(--ease) — from scale(0.96) to scale(1)
Max width:  480px default, 640px for wide variant
Padding:    1.75rem
Header:     t-heading + optional t-small subtitle + close button top-right
Footer:     flex, gap 0.5rem, justify-content flex-end
            primary action right, cancel left
```

---

## Sidebar / Drawer

Used for the admin editor settings panel.

```
Width:        320px
Background:   glass-heavy, full height
Border:       left border 1px solid --border (right drawer: right border)
Entry:        translateX(100%) → translateX(0), 0.3s var(--ease)
Backdrop:     same as modal
```

---

## Table

Used in the admin post list.

```
Container:    glass card, card-flush (no padding, overflow hidden)
Header row:   0.8rem Geist Mono uppercase, --ink-3, letter-spacing 0.05em
              padding: 0.625rem 1.25rem, border-bottom: 1px solid --border
Data cells:   0.9375rem Geist, --ink-2, padding: 0.875rem 1.25rem
Row hover:    background: --accent-dim, transition 0.15s
Last row:     no bottom border
```

Column widths are determined by content. Title column is always widest.

---

## Empty State

Shown when a section has no data — no posts, no tags, no results.

```
Container:  card, text-centre, padding 3rem 1.5rem
Icon area:  64px circle, --bg-2, --border, centred glyph in --ink-3
Heading:    t-heading, --ink, margin-top 1rem
Body:       t-body, --ink-3, max-width 280px, margin auto
Action:     btn-primary, margin-top 1.25rem
```

---

## Search

```
Input:      glass surface, full width, Geist Mono placeholder
            magnifier icon left-padded, clear button appears when value present
Dropdown:   glass-heavy, --r-lg, --shadow-lg, appears below input
Result item: padding 0.75rem 1rem, hover --accent-dim
             title in --ink, excerpt snippet in --ink-3 0.875rem
             tag badges right-aligned
No results:  t-small centred, --ink-3, padding 1.5rem
```

---

## Page Animations

Apply these classes to top-level sections on route entry. Angular route animations
trigger them on navigation. Stagger by 80ms per element for a cascading reveal.

| Class | Animation | Delay |
|---|---|---|
| `anim-fade-up` | `translateY(16px)` → 0, `opacity` 0 → 1 | 0ms |
| `anim-fade-up-2` | same | 80ms |
| `anim-fade-up-3` | same | 160ms |
| `anim-fade-up-4` | same | 240ms |
| `anim-scale-in` | `scale(0.96)` → 1, `opacity` 0 → 1 | 0ms |

Duration: `0.5s`. Easing: `cubic-bezier(0.16, 1, 0.3, 1)` — fast out, slight spring.

---

## Dark Mode

Dark mode is toggled by adding the `.dark` class to `<html>`. Store preference in
`localStorage` under the key `theme`. On app init, read the stored value and apply
the class before first render to prevent a flash.

Angular service pseudocode:

```
init()
  read localStorage.theme
  if stored === 'dark' or (no stored and system prefers dark)
    add .dark to document.documentElement

toggle()
  toggle .dark on document.documentElement
  write new value to localStorage.theme
  emit isDark$ observable so any component can react
```

The toggle button shows `☀` in dark mode and `☽` in light mode.
Transition on theme toggle: add `transition: background 0.3s, color 0.3s, border-color 0.3s`
to `html` — this smooths the palette swap without animating every element individually.

---

## Scrollbar

Custom thin scrollbar to match the minimal aesthetic.

```css
::-webkit-scrollbar       { width: 5px; height: 5px; }
::-webkit-scrollbar-track { background: transparent; }
::-webkit-scrollbar-thumb { background: var(--border-2); border-radius: 3px; }
```

---

## CSS Variables — Full Reference

Paste this block into `styles.css` or `variables.css` as the global token definition.

```css
:root {
  --bg:              #F5F2ED;
  --bg-2:            #EDE9E3;
  --bg-3:            #E5E1DA;
  --bg-glass:        rgba(245, 242, 237, 0.72);
  --bg-glass-heavy:  rgba(245, 242, 237, 0.88);

  --ink:             #1C1917;
  --ink-2:           #44403C;
  --ink-3:           #78716C;

  --accent:          #D97706;
  --accent-hover:    #B45309;
  --accent-dim:      rgba(217, 119, 6, 0.12);
  --accent-dim-2:    rgba(217, 119, 6, 0.20);

  --success:         #16A34A;
  --success-bg:      rgba(22, 163, 74, 0.08);
  --warning:         #D97706;
  --warning-bg:      rgba(217, 119, 6, 0.08);
  --danger:          #DC2626;
  --danger-bg:       rgba(220, 38, 38, 0.08);
  --info:            #2563EB;
  --info-bg:         rgba(37, 99, 235, 0.08);

  --border:          rgba(28, 25, 23, 0.10);
  --border-2:        rgba(28, 25, 23, 0.18);
  --border-focus:    rgba(217, 119, 6, 0.50);

  --blur:            saturate(180%) blur(20px);
  --blur-heavy:      saturate(200%) blur(32px);
  --shadow-sm:       0 1px 3px rgba(0,0,0,0.06), 0 1px 2px rgba(0,0,0,0.04);
  --shadow-md:       0 4px 16px rgba(0,0,0,0.08), 0 1px 4px rgba(0,0,0,0.04);
  --shadow-lg:       0 12px 40px rgba(0,0,0,0.10), 0 2px 8px rgba(0,0,0,0.05);

  --r-sm:            6px;
  --r-md:            10px;
  --r-lg:            14px;
  --r-xl:            20px;
  --r-full:          999px;

  --ease:            cubic-bezier(0.16, 1, 0.3, 1);
  --fast:            0.15s;
  --mid:             0.25s;
  --slow:            0.4s;
}

.dark {
  --bg:              #0F0E0D;
  --bg-2:            #1A1917;
  --bg-3:            #242220;
  --bg-glass:        rgba(15, 14, 13, 0.72);
  --bg-glass-heavy:  rgba(15, 14, 13, 0.90);

  --ink:             #F5F2ED;
  --ink-2:           #D6D3D1;
  --ink-3:           #78716C;

  --accent:          #F59E0B;
  --accent-hover:    #FBBF24;
  --accent-dim:      rgba(245, 158, 11, 0.12);
  --accent-dim-2:    rgba(245, 158, 11, 0.20);

  --border:          rgba(245, 242, 237, 0.08);
  --border-2:        rgba(245, 242, 237, 0.14);
  --border-focus:    rgba(245, 158, 11, 0.45);

  --shadow-sm:       0 1px 3px rgba(0,0,0,0.20);
  --shadow-md:       0 4px 16px rgba(0,0,0,0.30);
  --shadow-lg:       0 12px 40px rgba(0,0,0,0.40);
}
```
