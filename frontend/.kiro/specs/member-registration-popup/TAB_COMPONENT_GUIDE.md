# Tab Navigation Component Guide

## Overview

The new `TabNavigation` component provides a dynamic, scrollable tab interface for the GroupRegisterPage. It's designed to work similarly to the existing Category component but with enhanced functionality for form field organization.

## Component Structure

```
TabNavigation
├── Container (Box with flex layout)
│   ├── Tab Button 1
│   ├── Tab Button 2
│   ├── Tab Button 3
│   └── Tab Button N
```

## Usage Example

```typescript
import { TabNavigation } from "./GroupRegisterPage";

// In your component
<TabNavigation
  tabs={[
    { id: "tab1", name: "Thông tin cơ bản" },
    { id: "tab2", name: "Thông tin công ty" },
    { id: "tab3", name: "Thông tin khác" },
  ]}
  activeTabId="tab1"
  onTabChange={(tabId) => setActiveTabId(tabId)}
/>;
```

## Styling Details

### Container Styles

- **Display**: Flex with horizontal layout
- **Overflow**: Auto with hidden scrollbar
- **Border**: 1px solid #e5e7eb at bottom
- **Gap**: 8px between tabs
- **Scroll Behavior**: Smooth

### Tab Button Styles

#### Inactive State

```
Padding: 10px 16px
Font Size: 14px
Font Weight: 400
Color: #6b7280
Background: transparent
Border: none
Cursor: pointer
Transition: all 0.3s ease
```

#### Active State

```
Padding: 10px 16px
Font Size: 14px
Font Weight: 600
Color: #000
Background: transparent
Border Bottom: 3px solid #003d82
Cursor: pointer
Transition: all 0.3s ease
```

## Scroll Behavior

When a tab is clicked:

1. The `onTabChange` callback is triggered
2. The `activeTabId` state is updated
3. The `useEffect` hook detects the change
4. The corresponding tab button is scrolled into view with:
   - `behavior: "smooth"` - Smooth animation
   - `inline: "center"` - Scroll to center of viewport
   - `block: "nearest"` - Minimal vertical scrolling

## Responsive Behavior

The component is fully responsive:

- **Desktop**: All tabs visible with horizontal scroll if needed
- **Tablet**: Tabs adapt to screen width
- **Mobile**: Tabs scroll horizontally with smooth scrolling enabled

## Accessibility Features

- Semantic HTML: Uses `<button>` elements
- Keyboard Navigation: Tabs can be focused and activated with keyboard
- Visual Feedback: Clear active state indicator
- Color Contrast: Meets WCAG standards

## Performance Optimization

- **Memoization**: Component is wrapped with `React.memo()` to prevent unnecessary re-renders
- **Ref Management**: Uses `useRef` to store button references efficiently
- **Conditional Rendering**: Only renders when tabs exist

## Integration with GroupRegisterPage

The TabNavigation component is integrated into the custom fields section:

```typescript
{
  customFieldTabs.length > 0 && (
    <Box style={{ marginBottom: "24px" }}>
      {/* Tab Navigation */}
      {customFieldTabs.length > 1 && (
        <TabNavigation
          tabs={customFieldTabs.map((tab) => ({
            id: tab.id,
            name: tab.tabName,
          }))}
          activeTabId={activeTabId}
          onTabChange={setActiveTabId}
        />
      )}

      {/* Tab Content */}
      {customFieldTabs.map((tab) => {
        if (tab.id !== activeTabId) return null;
        // Render tab content
      })}
    </Box>
  );
}
```

## Comparison with Category Component

| Feature          | TabNavigation      | Category             |
| ---------------- | ------------------ | -------------------- |
| Purpose          | Form field tabs    | Category filtering   |
| Scroll Behavior  | Smooth to center   | Smooth to center     |
| Active Indicator | Bottom border      | Background color     |
| Styling          | Minimal            | Colored background   |
| Use Case         | Registration forms | Navigation/filtering |

## Future Enhancements

Potential improvements for future versions:

1. **Keyboard Navigation**: Add arrow key support for tab switching
2. **Lazy Loading**: Load tab content only when tab is active
3. **Animations**: Add fade-in/fade-out animations for tab content
4. **Touch Gestures**: Add swipe support for mobile devices
5. **Customizable Styling**: Accept style props for theming
6. **Icons**: Support for icons in tab labels
7. **Badges**: Support for notification badges on tabs
