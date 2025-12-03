# Implementation Notes: Dynamic Tab Navigation

## Changes Made

### Enhanced Tab Navigation Component

**File**: `src/pagesGiba/GroupRegisterPage.tsx`

#### New TabNavigation Component

A new `TabNavigation` component has been added that provides:

1. **Smooth Scrolling to Center**: When a tab is clicked, it automatically scrolls to the center of the viewport using `scrollIntoView()` with `inline: "center"` option
2. **Dynamic Tab Rendering**: Accepts an array of tab items and renders them dynamically
3. **Memoization**: Uses `React.memo()` to prevent unnecessary re-renders
4. **Responsive Design**: Tabs are flexible and adapt to content

#### Component Props

```typescript
interface TabItem {
  id: string;
  name: string;
}

interface TabNavigationProps {
  tabs: TabItem[];
  activeTabId: string;
  onTabChange: (tabId: string) => void;
}
```

#### Features

- **Smooth Scroll Behavior**: Uses `scrollIntoView()` with `behavior: "smooth"` and `inline: "center"`
- **Hidden Scrollbar**: Uses the `hidden-scrollbar-y` CSS class to hide the horizontal scrollbar
- **Active Tab Indicator**: Shows a 3px solid border at the bottom in the primary color (#003d82)
- **Keyboard Accessible**: Buttons are properly semantic HTML elements
- **Touch Friendly**: Includes `-webkit-overflow-scrolling: touch` for smooth scrolling on mobile

#### Visual Styling

- **Active Tab**:
  - Font weight: 600
  - Color: #000
  - Bottom border: 3px solid #003d82
- **Inactive Tab**:

  - Font weight: 400
  - Color: #6b7280
  - No bottom border

- **Hover Effect**: Smooth transition on all properties (0.3s ease)

### Integration in GroupRegisterPage

The TabNavigation component is used in the custom fields section:

```typescript
{
  customFieldTabs.length > 1 && (
    <TabNavigation
      tabs={customFieldTabs.map((tab) => ({
        id: tab.id,
        name: tab.tabName,
      }))}
      activeTabId={activeTabId}
      onTabChange={setActiveTabId}
    />
  );
}
```

## Benefits

1. **Better UX**: Tabs automatically scroll to center when clicked, similar to the Category component
2. **Code Reusability**: The TabNavigation component can be reused in other parts of the application
3. **Dynamic Rendering**: Tabs are rendered dynamically based on the data from the API
4. **Performance**: Memoization prevents unnecessary re-renders
5. **Consistency**: Follows the same pattern as the existing Category component

## CSS Requirements

The component uses the `hidden-scrollbar-y` CSS class which should be available in your global styles. If not, add this to your CSS:

```css
.hidden-scrollbar-y::-webkit-scrollbar {
  display: none;
}

.hidden-scrollbar-y {
  -ms-overflow-style: none;
  scrollbar-width: none;
}
```

## Testing Considerations

When testing the tab navigation:

1. Verify tabs scroll to center when clicked
2. Verify the active tab indicator is displayed correctly
3. Verify tab content switches when a different tab is clicked
4. Verify the component works with multiple tabs
5. Verify the component works with a single tab (should not display navigation)
6. Test on mobile devices to ensure smooth scrolling behavior
