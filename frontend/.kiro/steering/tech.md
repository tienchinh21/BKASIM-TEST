# Technology Stack

## Platform

- **Framework**: Zalo Mini App (ZMP) - React-based mini app platform for Zalo ecosystem
- **Build Tool**: Vite 5.x
- **Language**: TypeScript + JavaScript (mixed codebase)

## Core Libraries

- **UI Framework**: React 18.3 with React Router 6.x
- **UI Components**:
  - `zmp-ui` - Zalo Mini App UI components
  - `antd` (Ant Design) - Additional UI components
  - Custom component library in `componentsGiba/`
- **State Management**: Recoil
- **Styling**:
  - Tailwind CSS 3.x
  - PostCSS for CSS processing
  - CSS modules and inline styles
- **HTTP Client**: Axios
- **Notifications**: react-toastify
- **Date Handling**: dayjs
- **Icons**:
  - lucide-react
  - react-icons
  - @iconify/react
- **QR Code**: zmp-qrcode, html5-qrcode
- **Charts**: recharts
- **Other**:
  - Swiper for carousels
  - react-beautiful-dnd for drag-drop
  - html2canvas for screenshots

## Zalo SDK Integration

The app heavily integrates with Zalo SDK (`zmp-sdk`) for:

- User authentication and info retrieval
- Phone number access
- Access token management
- Permission handling
- Official Account (OA) following

## Common Commands

```bash
# Start development server
npm start

# Deploy to production
npm deploy

# Build Tailwind CSS
npm run build:css
```

## Build Configuration

- **Target**: ES6+ (esnext for build)
- **Module System**: ESNext with Node resolution
- **TypeScript**: Strict mode enabled
- **Vite Plugins**:
  - zmp-vite-plugin (Zalo Mini App support)
  - @vitejs/plugin-react (React Fast Refresh)

## API Integration

- Backend API calls use Axios with Bearer token authentication
- API domain configured in `src/common/DefaultConfig.json`
- Token management handled via Recoil state
- FormData used for multipart requests
