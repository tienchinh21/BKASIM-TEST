# Project Structure

## Root Configuration

- `vite.config.mts` - Vite build configuration
- `tsconfig.json` - TypeScript configuration (strict mode)
- `tailwind.config.js` - Tailwind CSS configuration
- `postcss.config.js` - PostCSS configuration
- `app-config.json` - App metadata and configuration
- `zmp-cli.json` - Zalo Mini App CLI configuration

## Source Directory (`src/`)

### Entry Points

- `app.ts` - Application entry point
- `components/app.tsx` - Main app component with routing

### Pages

**Legacy Pages** (`src/pages/`):

- Older implementation of features
- Includes: event, home, info, news, product, welcome

**Giba Pages** (`src/pagesGiba/`):

- Current/active implementation
- Main pages: HomeGiba, LoginGiba, DashboardGiba
- Feature modules:
  - `event/` - Event management and registration
  - `home/components/` - Home page sections
  - `news/` - News and articles
  - `appointment/` - Appointment scheduling
  - `meetingSchedule/` - Meeting management
  - `showcase/` - Member showcases
  - `ref/` - Referral system
  - `achievements/` - Achievement tracking
  - `managerX/` - Admin management pages (membership, articles, meetings, training, club)

### Components

**Legacy Components** (`src/components/`):

- Shared components: CommonButton, CommonShareModal, Loading
- Feature components: Category, TimePicker, TwoTierTab
- Layout components in `layout/`
- Custom hooks in `hooks/`

**Giba Components** (`src/componentsGiba/`):

- Modern component library
- Naming convention: `ComponentNameGiba.tsx`
- Key components: ButtonGiba, LoadingGiba, SwiperGiba, Badge, MemberCardGiba
- Drawers: EventRegisterDrawer, CreateRefDrawer, GroupRulesDrawer
- Skeletons in `skeletons/`

### State Management

- `src/recoil/RecoilState.js` - Global Recoil atoms and selectors
- `src/state.ts` - Additional state definitions
- Key state: isRegister, infoUser, token, userMembershipInfo, isFollowedOA

### Utilities & Helpers

- `src/common/` - API helpers, token management, default config
- `src/utils/` - Date formatting, validation, storage utilities
- `src/utils/enum/` - TypeScript enums for various entities
- `src/hooks/` - Custom React hooks (useApiCall, useDeepLink, useHasRole, etc.)
- `src/types/` - TypeScript type definitions

### Styling

- `src/css/` - Global styles (app.css, tailwind.css, icons.css)
- `src/constantsGiba/colorsGiba.ts` - Color palette constants
- `src/store/styles/GlobalStyles.ts` - Styled components global styles

### Assets

- `src/assets/` - Images (logos, backgrounds, placeholders)
- `src/static/icons/` - App icons and favicons

## Naming Conventions

- **Giba suffix**: New/active components and pages use "Giba" suffix (e.g., `HomeGiba`, `ButtonGiba`)
- **File extensions**:
  - `.tsx` for TypeScript React components
  - `.jsx` for JavaScript React components
  - `.ts` for TypeScript utilities
  - `.js` for JavaScript utilities
- **Component files**: PascalCase matching component name
- **Style files**: Same name as component with `.css` extension

## Routing

All routes defined in `src/components/app.tsx`:

- Base paths: `/`, `/giba`, `/s/:sessionId/`, `/zapps/:sessionId/`
- Protected routes wrapped with `<PrivateRoute>`
- Fallback redirects to `/giba`

## Build Output

- `dist/` - Development build output
- `www/` - Production build output
