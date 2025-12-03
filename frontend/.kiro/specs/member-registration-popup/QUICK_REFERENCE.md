# Quick Reference: TabNavigation Component

## What Changed?

The tab navigation in `GroupRegisterPage.tsx` has been enhanced with:

- ✅ Smooth scroll-to-center functionality
- ✅ Reusable component architecture
- ✅ Hidden scrollbar for cleaner UI
- ✅ Better performance with memoization
- ✅ Improved visual design

## Component Location

**File**: `src/pagesGiba/GroupRegisterPage.tsx`
**Component**: `TabNavigation` (lines 13-73)

## How to Use

### Basic Usage

```typescript
<TabNavigation
  tabs={[
    { id: "tab1", name: "Tab 1" },
    { id: "tab2", name: "Tab 2" },
  ]}
  activeTabId={activeTabId}
  onTabChange={setActiveTabId}
/>
```

### In GroupRegisterPage

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

## Component Props

```typescript
interface TabItem {
  id: string; // Unique identifier for the tab
  name: string; // Display name of the tab
}

interface TabNavigationProps {
  tabs: TabItem[]; // Array of tabs to display
  activeTabId: string; // Currently active tab ID
  onTabChange: (tabId: string) => void; // Callback when tab changes
}
```

## Features

### 1. Smooth Scroll to Center

When you click a tab, it automatically scrolls to the center of the viewport with smooth animation.

```typescript
// Automatically triggered when activeTabId changes
itemRefs.current[activeTabId]?.scrollIntoView({
  behavior: "smooth",
  inline: "center",
  block: "nearest",
});
```

### 2. Hidden Scrollbar

The horizontal scrollbar is hidden but scrolling still works (especially on touch devices).

```typescript
className = "hidden-scrollbar-y"; // CSS class that hides scrollbar
```

### 3. Active Tab Indicator

The active tab shows a 3px solid border in the primary color (#003d82).

```typescript
borderBottom: activeTabId === tab.id ? "3px solid #003d82" : "none";
```

### 4. Performance Optimization

The component is memoized to prevent unnecessary re-renders.

```typescript
const TabNavigation = memo<TabNavigationProps>(...)
```

## Styling

### Container

- Display: Flex (horizontal)
- Gap: 8px between tabs
- Border: 1px solid #e5e7eb at bottom
- Overflow: Auto with hidden scrollbar
- Scroll behavior: Smooth

### Tab Button

- Padding: 10px 16px
- Font size: 14px
- Transition: all 0.3s ease
- Cursor: pointer
- White space: nowrap (no wrapping)

### Active Tab

- Font weight: 600
- Color: #000
- Border bottom: 3px solid #003d82

### Inactive Tab

- Font weight: 400
- Color: #6b7280
- Border bottom: none

## Common Tasks

### Change Active Tab

```typescript
setActiveTabId("tab2");
```

### Add New Tab

```typescript
const newTabs = [...tabs, { id: "tab3", name: "New Tab" }];
```

### Customize Colors

Modify the style object in the component:

```typescript
borderBottom: activeTabId === tab.id ? "3px solid #YOUR_COLOR" : "none";
```

### Disable Smooth Scroll

Change `behavior: "smooth"` to `behavior: "auto"`:

```typescript
itemRefs.current[activeTabId]?.scrollIntoView({
  behavior: "auto", // Changed from "smooth"
  inline: "center",
  block: "nearest",
});
```

## Testing

### Test Smooth Scroll

1. Click on a tab that's not visible
2. Verify it scrolls to center with animation

### Test Active Indicator

1. Click different tabs
2. Verify the active tab shows the blue border
3. Verify inactive tabs have no border

### Test Responsive

1. Test on desktop (wide screen)
2. Test on tablet (medium screen)
3. Test on mobile (narrow screen)
4. Verify tabs scroll horizontally when needed

### Test Performance

1. Open DevTools Performance tab
2. Click tabs multiple times
3. Verify no unnecessary re-renders

## Troubleshooting

### Tabs Not Scrolling to Center

- Check if `activeTabId` is being updated correctly
- Verify the ref is properly assigned to the button
- Check browser console for errors

### Scrollbar Still Visible

- Ensure the `hidden-scrollbar-y` CSS class is available
- Check if CSS is properly loaded
- Try adding inline styles as fallback

### Tabs Not Responding to Clicks

- Verify `onTabChange` callback is properly connected
- Check if `activeTabId` state is being updated
- Ensure no event propagation issues

### Performance Issues

- Verify component is memoized
- Check if parent component is re-rendering unnecessarily
- Use React DevTools Profiler to identify bottlenecks

## Related Files

- **Component**: `src/pagesGiba/GroupRegisterPage.tsx`
- **CSS**: Uses `hidden-scrollbar-y` class (defined in global styles)
- **Similar Component**: `src/components/Category/index.jsx` (reference implementation)

## Future Enhancements

Potential improvements:

- [ ] Keyboard navigation (arrow keys)
- [ ] Swipe support for mobile
- [ ] Lazy loading of tab content
- [ ] Customizable styling props
- [ ] Icon support in tabs
- [ ] Notification badges
- [ ] Disabled state for tabs

## Support

For questions or issues:

1. Check the IMPLEMENTATION_NOTES.md file
2. Review the TAB_COMPONENT_GUIDE.md file
3. Compare with BEFORE_AFTER_COMPARISON.md
4. Check the Category component for reference
