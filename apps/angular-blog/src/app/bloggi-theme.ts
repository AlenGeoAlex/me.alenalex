/**
 * blog-theme.ts
 *
 * PrimeNG 21 custom preset — mapped 1-to-1 from angular-design-system.md.
 *
 * Usage in app.config.ts:
 *
 *   import { BlogTheme } from './blog-theme';
 *
 *   providePrimeNG({
 *     ripple: true,
 *     theme: {
 *       preset: BlogTheme,
 *       options: {
 *         prefix: 'p',
 *         darkModeSelector: '.dark',   // matches the design system's .dark on <html>
 *         cssLayer: false,
 *       },
 *     },
 *   })
 *
 * Dark mode is toggled by adding/removing the `.dark` class on <html>.
 * This matches the ThemeService described in angular-design-system.md which
 * writes to localStorage under the key `theme` and toggles the class before
 * first render to prevent a flash.
 */

import { definePreset } from '@primeuix/themes';
import Aura from '@primeuix/themes/aura';

// ---------------------------------------------------------------------------
// Primitive palette — warm stone scale used as the surface ramp.
// PrimeNG expects a palette object with keys 50–950.
// These are derived from the design system background/text tokens so that
// PrimeNG's surface.*  semantic tokens resolve to the same stones.
// ---------------------------------------------------------------------------
const stonePalette = {
  50:  '#FAFAF9',
  100: '#F5F5F4',
  200: '#F5F2ED',   // --bg  light
  300: '#EDE9E3',   // --bg-2 light
  400: '#E5E1DA',   // --bg-3 light
  500: '#78716C',   // --ink-3 / muted
  600: '#57534E',
  700: '#44403C',   // --ink-2 light
  800: '#292524',
  900: '#1C1917',   // --ink light / --bg dark
  950: '#0F0E0D',   // --bg dark (deepest)
};

// Amber palette — maps to --accent tokens.
const amberPalette = {
  50:  '#FFFBEB',
  100: '#FEF3C7',
  200: '#FDE68A',
  300: '#FCD34D',
  400: '#FBBF24',   // --accent-hover dark
  500: '#F59E0B',   // --accent dark
  600: '#D97706',   // --accent light
  700: '#B45309',   // --accent-hover light
  800: '#92400E',
  900: '#78350F',
  950: '#451A03',
};

// ---------------------------------------------------------------------------
// The preset
// ---------------------------------------------------------------------------
// @ts-ignore
export const BlogTheme = definePreset(Aura, {

  // ─── Primitive tokens ────────────────────────────────────────────────────
  primitive: {
    borderRadius: {
      none: '0',
      xs:   '4px',
      sm:   '6px',    // --r-sm
      md:   '10px',   // --r-md
      lg:   '14px',   // --r-lg
      xl:   '20px',   // --r-xl
      '2xl':'24px',
    },

    // Surface (neutral) palette — warm stone
    stone: stonePalette,

    // Primary (accent) palette — amber
    amber: amberPalette,

    // Keep these so Aura's internal references don't break
    emerald: {
      50: '#ecfdf5', 100: '#d1fae5', 200: '#a7f3d0', 300: '#6ee7b7',
      400: '#34d399', 500: '#10b981', 600: '#059669', 700: '#047857',
      800: '#065f46', 900: '#064e3b', 950: '#022c22',
    },

    // Semantic colour primitives (used by callout / status tokens below)
    green: {
      50:  '#f0fdf4', 100: '#dcfce7', 200: '#bbf7d0', 300: '#86efac',
      400: '#4ade80', 500: '#22c55e', 600: '#16a34a',  // --success
      700: '#15803d', 800: '#166534', 900: '#14532d', 950: '#052e16',
    },
    red: {
      50:  '#fef2f2', 100: '#fee2e2', 200: '#fecaca', 300: '#fca5a5',
      400: '#f87171', 500: '#ef4444', 600: '#dc2626',  // --danger
      700: '#b91c1c', 800: '#991b1b', 900: '#7f1d1d', 950: '#450a0a',
    },
    blue: {
      50:  '#eff6ff', 100: '#dbeafe', 200: '#bfdbfe', 300: '#93c5fd',
      400: '#60a5fa', 500: '#3b82f6', 600: '#2563eb',  // --info
      700: '#1d4ed8', 800: '#1e40af', 900: '#1e3a8a', 950: '#172554',
    },
  },

  // ─── Semantic tokens ─────────────────────────────────────────────────────
  semantic: {

    // Map primary → amber (the design-system accent)
    primary: {
      50:  '{amber.50}',
      100: '{amber.100}',
      200: '{amber.200}',
      300: '{amber.300}',
      400: '{amber.400}',
      500: '{amber.500}',
      600: '{amber.600}',
      700: '{amber.700}',
      800: '{amber.800}',
      900: '{amber.900}',
      950: '{amber.950}',
    },

    // Focus ring → amber tint (--border-focus)
    focusRing: {
      width:  '3px',
      style:  'solid',
      color:  '{primary.color}',
      offset: '0',
      shadow: 'none',
    },

    // Surface palette → stone (controls backgrounds, borders, text)
    colorScheme: {

      // ── Light ────────────────────────────────────────────────────────────
      light: {
        primary: {
          color:           '{amber.600}',   // --accent
          contrastColor:   '#ffffff',
          hoverColor:      '{amber.700}',   // --accent-hover
          activeColor:     '{amber.800}',
        },

        highlight: {
          background:      'rgba(217,119,6,0.12)',  // --accent-dim
          focusBackground: 'rgba(217,119,6,0.20)',  // --accent-dim-2
          color:           '{amber.600}',
          focusColor:      '{amber.700}',
        },

        surface: {
          0:    '#ffffff',
          50:   '{stone.50}',
          100:  '{stone.100}',
          200:  '{stone.200}',  // --bg
          300:  '{stone.300}',  // --bg-2
          400:  '{stone.400}',  // --bg-3
          500:  '{stone.500}',  // --ink-3
          600:  '{stone.600}',
          700:  '{stone.700}',  // --ink-2
          800:  '{stone.800}',
          900:  '{stone.900}',  // --ink
          950:  '{stone.950}',
        },

        mask: {
          background: 'rgba(0,0,0,0.40)',
          color:      '{surface.200}',
        },

        formField: {
          background:           '{stone.300}',       // --bg-2
          disabledBackground:   '{stone.400}',
          filledBackground:     '{stone.300}',
          filledFocusBackground:'{stone.300}',
          borderColor:          'rgba(28,25,23,0.10)',  // --border
          hoverBorderColor:     'rgba(28,25,23,0.18)',  // --border-2
          focusBorderColor:     'rgba(217,119,6,0.50)', // --border-focus
          invalidBorderColor:   '{red.600}',
          color:                '{stone.900}',       // --ink
          disabledColor:        '{stone.500}',
          placeholderColor:     '{stone.500}',       // --ink-3
          invalidPlaceholderColor: '{red.500}',
          floatLabelColor:      '{stone.500}',
          floatLabelFocusColor: '{amber.600}',
          floatLabelInvalidColor: '{red.600}',
          iconColor:            '{stone.500}',
          shadow:               '0 0 0 3px rgba(217,119,6,0.12)', // accent-dim ring on focus
        },

        text: {
          color:           '{stone.900}',  // --ink
          hoverColor:      '{stone.800}',
          mutedColor:      '{stone.500}',  // --ink-3
          hoverMutedColor: '{stone.600}',
        },

        content: {
          background:       '{stone.200}',   // --bg
          hoverBackground:  '{stone.300}',   // --bg-2
          borderColor:      'rgba(28,25,23,0.10)',
          color:            '{stone.700}',   // --ink-2
          hoverColor:       '{stone.900}',
        },

        overlay: {
          select: {
            background:  'rgba(245,242,237,0.88)',  // --bg-glass-heavy
            borderColor: 'rgba(28,25,23,0.10)',
            color:       '{stone.900}',
          },
          popover: {
            background:  'rgba(245,242,237,0.88)',
            borderColor: 'rgba(28,25,23,0.10)',
            color:       '{stone.900}',
          },
          modal: {
            background:  'rgba(245,242,237,0.88)',
            borderColor: 'rgba(28,25,23,0.10)',
            color:       '{stone.900}',
          },
        },

        list: {
          option: {
            focusBackground:  'rgba(217,119,6,0.12)',  // --accent-dim
            selectedBackground: 'rgba(217,119,6,0.20)',
            selectedFocusBackground: 'rgba(217,119,6,0.28)',
            color:            '{stone.700}',
            focusColor:       '{stone.900}',
            selectedColor:    '{amber.700}',
            selectedFocusColor: '{amber.800}',
            icon:             {
              color:      '{stone.500}',
              focusColor: '{stone.700}',
            },
          },
          optionGroup: {
            background:  'transparent',
            color:       '{stone.500}',
          },
        },

        navigation: {
          item: {
            focusBackground:    'rgba(217,119,6,0.12)',
            activeBackground:   'rgba(217,119,6,0.12)',
            color:              '{stone.700}',
            focusColor:         '{stone.900}',
            activeColor:        '{amber.600}',
            icon: {
              color:       '{stone.500}',
              focusColor:  '{stone.700}',
              activeColor: '{amber.600}',
            },
          },
          submenuLabel: {
            background: 'transparent',
            color:      '{stone.500}',
          },
          submenuIcon: {
            color:      '{stone.500}',
            focusColor: '{stone.700}',
            activeColor:'{amber.600}',
          },
        },
      }, // end light

      // ── Dark ─────────────────────────────────────────────────────────────
      dark: {
        primary: {
          color:           '{amber.500}',   // --accent dark
          contrastColor:   '{stone.950}',
          hoverColor:      '{amber.400}',   // --accent-hover dark
          activeColor:     '{amber.300}',
        },

        highlight: {
          background:      'rgba(245,158,11,0.12)',  // --accent-dim dark
          focusBackground: 'rgba(245,158,11,0.20)',  // --accent-dim-2 dark
          color:           '{amber.500}',
          focusColor:      '{amber.400}',
        },

        surface: {
          0:    '#ffffff',
          50:   '{stone.50}',
          100:  '{stone.100}',
          200:  '{stone.950}',  // --bg dark  (#0F0E0D)
          300:  '{stone.900}',  // --bg-2 dark (#1A1917) — mapped via 900
          400:  '{stone.800}',  // --bg-3 dark (#242220)
          500:  '{stone.500}',  // --ink-3
          600:  '{stone.400}',
          700:  '{stone.300}',  // --ink-2 dark (#D6D3D1)
          800:  '{stone.200}',
          900:  '{stone.100}',  // --ink dark (#F5F2ED)
          950:  '{stone.50}',
        },

        mask: {
          background: 'rgba(0,0,0,0.60)',
          color:      '{surface.200}',
        },

        formField: {
          background:           'rgba(26,25,23,1)',     // #1A1917 --bg-2 dark
          disabledBackground:   'rgba(36,34,32,1)',     // --bg-3 dark
          filledBackground:     'rgba(26,25,23,1)',
          filledFocusBackground:'rgba(26,25,23,1)',
          borderColor:          'rgba(245,242,237,0.08)',   // --border dark
          hoverBorderColor:     'rgba(245,242,237,0.14)',   // --border-2 dark
          focusBorderColor:     'rgba(245,158,11,0.45)',    // --border-focus dark
          invalidBorderColor:   '{red.500}',
          color:                '#F5F2ED',              // --ink dark
          disabledColor:        '{stone.500}',
          placeholderColor:     '{stone.500}',          // --ink-3
          invalidPlaceholderColor: '{red.400}',
          floatLabelColor:      '{stone.500}',
          floatLabelFocusColor: '{amber.500}',
          floatLabelInvalidColor: '{red.500}',
          iconColor:            '{stone.500}',
          shadow:               '0 0 0 3px rgba(245,158,11,0.12)',
        },

        text: {
          color:           '#F5F2ED',     // --ink dark
          hoverColor:      '{stone.200}',
          mutedColor:      '{stone.500}', // --ink-3 (same both modes)
          hoverMutedColor: '{stone.400}',
        },

        content: {
          background:       '#0F0E0D',    // --bg dark
          hoverBackground:  '#1A1917',    // --bg-2 dark
          borderColor:      'rgba(245,242,237,0.08)',
          color:            '#D6D3D1',    // --ink-2 dark
          hoverColor:       '#F5F2ED',
        },

        overlay: {
          select: {
            background:  'rgba(15,14,13,0.90)',   // --bg-glass-heavy dark
            borderColor: 'rgba(245,242,237,0.08)',
            color:       '#F5F2ED',
          },
          popover: {
            background:  'rgba(15,14,13,0.90)',
            borderColor: 'rgba(245,242,237,0.08)',
            color:       '#F5F2ED',
          },
          modal: {
            background:  'rgba(15,14,13,0.90)',
            borderColor: 'rgba(245,242,237,0.08)',
            color:       '#F5F2ED',
          },
        },

        list: {
          option: {
            focusBackground:       'rgba(245,158,11,0.12)',
            selectedBackground:    'rgba(245,158,11,0.20)',
            selectedFocusBackground:'rgba(245,158,11,0.28)',
            color:                 '#D6D3D1',
            focusColor:            '#F5F2ED',
            selectedColor:         '{amber.500}',
            selectedFocusColor:    '{amber.400}',
            icon: {
              color:      '{stone.500}',
              focusColor: '#D6D3D1',
            },
          },
          optionGroup: {
            background: 'transparent',
            color:      '{stone.500}',
          },
        },

        navigation: {
          item: {
            focusBackground:  'rgba(245,158,11,0.12)',
            activeBackground: 'rgba(245,158,11,0.12)',
            color:            '#D6D3D1',
            focusColor:       '#F5F2ED',
            activeColor:      '{amber.500}',
            icon: {
              color:       '{stone.500}',
              focusColor:  '#D6D3D1',
              activeColor: '{amber.500}',
            },
          },
          submenuLabel: {
            background: 'transparent',
            color:      '{stone.500}',
          },
          submenuIcon: {
            color:       '{stone.500}',
            focusColor:  '#D6D3D1',
            activeColor: '{amber.500}',
          },
        },
      }, // end dark
    }, // end colorScheme
  }, // end semantic

  // ─── Component tokens ────────────────────────────────────────────────────
  // Only the components that need values the semantic layer can't reach.
  components: {

    // Button — maps to design system button variants
    button: {
      // @ts-ignore
      borderRadius: '{borderRadius.md}',    // --r-md = 10px
      sm: { fontSize: '0.8125rem', paddingX: '0.75rem',  paddingY: '0.3125rem' },
      md: { fontSize: '0.875rem',  paddingX: '1.125rem', paddingY: '0.5rem'    },
      lg: { fontSize: '0.9375rem', paddingX: '1.5rem',   paddingY: '0.6875rem' },
      // Raised (primary) button shadow on hover is handled by global hover
      // transition. PrimeNG adds --shadow-md equivalent via raised modifier.
    },

    // InputText — glass surface input style
    inputtext: {
      // @ts-ignore
      borderRadius:    '{borderRadius.md}',
      paddingX:        '0.875rem',
      paddingY:        '0.5625rem',
      fontSize:        '0.9375rem',
      transitionDuration: '0.15s',
    },

    // Textarea
    textarea: {
      // @ts-ignore
      borderRadius: '{borderRadius.md}',
      paddingX:     '0.875rem',
      paddingY:     '0.5625rem',
      fontSize:     '0.9375rem',
    },

    // Select / Dropdown
    select: {
      // @ts-ignore
      borderRadius: '{borderRadius.md}',
      paddingX:     '0.875rem',
      paddingY:     '0.5625rem',
      fontSize:     '0.9375rem',
      overlay: {
        borderRadius: '{borderRadius.lg}',  // --r-lg = 14px (glass dropdown)
      },
    },

    // Card — glass card (--r-xl, border, glass bg)
    card: {
      // @ts-ignore
      borderRadius: '{borderRadius.xl}',    // --r-xl = 20px
      shadow:       '0 4px 16px rgba(0,0,0,0.08)',  // --shadow-md
      background:   '{content.background}',
      color:        '{text.color}',
    },

    // Panel
    panel: {
      // @ts-ignore
      borderRadius: '{borderRadius.xl}',
    },

    // Dialog / Modal — glass-heavy, --r-xl, --shadow-lg
    dialog: {
      // @ts-ignore
      borderRadius: '{borderRadius.xl}',
      background:   '{overlay.modal.background}',
      color:        '{text.color}',
      shadow:       '0 12px 40px rgba(0,0,0,0.10)',
    },

    // Tooltip
    tooltip: {
      // @ts-ignore
      background:   '{overlay.popover.background}',
      color:        '{text.color}',
      borderRadius: '{borderRadius.md}',
      shadow:       '0 4px 16px rgba(0,0,0,0.08)',
      padding:      '0.5rem 0.75rem',
    },

    // Badge / Tag — glass pill, Geist Mono
    badge: {
      // @ts-ignore
      borderRadius:  '{borderRadius.full}',
      fontSize:      '0.75rem',
      fontWeight:    '400',
      // primary severity maps to amber accent
    },

    tag: {
      // @ts-ignore
      borderRadius: '{borderRadius.full}',
      fontSize:     '0.75rem',
      fontWeight:   '400',
    },

    // Message (Toast body) — --r-lg, glass-heavy
    message: {
      // @ts-ignore
      borderRadius: '{borderRadius.lg}',
    },

    // Toast — bottom-right, glass-heavy, --shadow-lg
    toast: {
      // @ts-ignore
      borderRadius: '{borderRadius.lg}',
      shadow:       '0 12px 40px rgba(0,0,0,0.10)',
      // Severity colours map to semantic vars set in colorScheme above
    },

    // DataTable
    datatable: {
      // @ts-ignore
      headerCellBackground:         'transparent',
      headerCellColor:              '{text.mutedColor}',
      headerCellBorderColor:        '{content.borderColor}',
      headerCellHoverBackground:    '{content.hoverBackground}',
      bodyCellBackground:           'transparent',
      bodyCellColor:                '{content.color}',
      bodyRowHoverBackground:       '{highlight.background}',
      bodyRowSelectedBackground:    '{highlight.focusBackground}',
      borderColor:                  '{content.borderColor}',
      footerCellBackground:         'transparent',
      footerCellColor:              '{text.mutedColor}',
    },

    // Tabs
    tabs: {
      // @ts-ignore
      tabBorderRadius:        '{borderRadius.md}',
      activeTabBackground:    '{highlight.background}',
      activeTabColor:         '{primary.color}',
      tabColor:               '{text.mutedColor}',
      tabHoverColor:          '{text.color}',
    },

    // ToggleSwitch — maps to design system Toggle spec
    toggleswitch: {
      // @ts-ignore
      width:          '2rem',
      height:         '1.125rem',
      borderRadius:   '{borderRadius.full}',
      // off state background comes from surface.400 (--bg-3)
      // on  state comes from primary.color (amber)
      handleBackground: '#ffffff',
      transitionDuration: '0.2s',
    },

    // Avatar — glass circle with accent text
    avatar: {
      // @ts-ignore
      background:   '{content.hoverBackground}',
      color:        '{primary.color}',
      borderRadius: '{borderRadius.full}',
      sm: { width: '1.5rem',  height: '1.5rem',  fontSize: '0.65rem'  },
      md: { width: '2rem',    height: '2rem',    fontSize: '0.75rem'  },
      lg: { width: '2.75rem', height: '2.75rem', fontSize: '0.875rem' },
    },

    // Chip (same as badge)
    chip: {
      // @ts-ignore
      borderRadius: '{borderRadius.full}',
      fontSize:     '0.75rem',
      fontWeight:   '400',
    },

    // Menu / ContextMenu / TieredMenu
    menu: {
      // @ts-ignore
      background:   '{overlay.popover.background}',
      borderRadius: '{borderRadius.lg}',
      shadow:       '0 4px 16px rgba(0,0,0,0.08)',
      color:        '{text.color}',
      item: {
        borderRadius: '{borderRadius.md}',
      },
    },

    // Sidebar / Drawer — glass-heavy, 320px
    drawer: {
      // @ts-ignore
      background: '{overlay.modal.background}',
      color:      '{text.color}',
      shadow:     '0 12px 40px rgba(0,0,0,0.10)',
    },

    // Popover
    popover: {
      // @ts-ignore
      background:   '{overlay.popover.background}',
      borderRadius: '{borderRadius.lg}',
      shadow:       '0 4px 16px rgba(0,0,0,0.08)',
      color:        '{text.color}',
    },

    // InputGroup
    inputgroup: {
      // @ts-ignore
      addonBackground: '{content.hoverBackground}',
      addonColor:      '{text.mutedColor}',
      addonBorderColor:'{content.borderColor}',
    },

    // Breadcrumb
    breadcrumb: {
      // @ts-ignore
      background:   'transparent',
      color:        '{text.mutedColor}',
      itemColor:    '{text.mutedColor}',
      activeItemColor: '{primary.color}',
    },

    // Paginator
    paginator: {
      // @ts-ignore
      background:          'transparent',
      color:               '{text.mutedColor}',
      activePageBackground:'{highlight.background}',
      activePageColor:     '{primary.color}',
      borderRadius:        '{borderRadius.md}',
    },

    // Accordion
    accordion: {
      // @ts-ignore
      headerBackground:         'transparent',
      headerHoverBackground:    '{content.hoverBackground}',
      headerActiveBackground:   '{highlight.background}',
      headerColor:              '{text.color}',
      headerActiveColor:        '{primary.color}',
      contentBackground:        'transparent',
      contentColor:             '{content.color}',
      borderRadius:             '{borderRadius.lg}',
    },

    // Stepper
    stepper: {
      // @ts-ignore
      stepActiveColor:        '{primary.color}',
      stepActiveBackground:   '{primary.color}',
      stepInactiveColor:      '{text.mutedColor}',
    },

    // ProgressBar
    progressbar: {
      // @ts-ignore
      background:      '{content.hoverBackground}',
      valueBackground: '{primary.color}',
      borderRadius:    '{borderRadius.full}',
    },

    // Slider
    slider: {
      // @ts-ignore
      trackBackground:     '{content.hoverBackground}',
      rangeBackground:     '{primary.color}',
      handleBackground:    '{primary.color}',
      handleBorderColor:   '{primary.color}',
      handleBorderRadius:  '{borderRadius.full}',
    },

    // Rating
    rating: {
      // @ts-ignore
      iconColor:       '{content.borderColor}',
      iconActiveColor: '{primary.color}',
    },

    // Divider
    divider: {
      // @ts-ignore
      borderColor: '{content.borderColor}',
      color:       '{text.mutedColor}',
    },

    // Skeleton
    skeleton: {
      // @ts-ignore
      background:         '{content.hoverBackground}',
      animationBackground:'{content.background}',
    },

    // Splitter
    splitter: {
      // @ts-ignore
      gutterBackground: '{content.borderColor}',
    },
  }, // end components
});
