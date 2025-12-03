# Component Architecture: TabNavigation

## Component Hierarchy

```
GroupRegisterPage
├── Header (useSetHeader)
├── Basic Fields Section
│   ├── Name Input
│   ├── Phone Input
│   ├── Email Input
│   ├── Reason Textarea
│   ├── Company Input
│   └── Position Input
├── Custom Fields Section
│   ├── TabNavigation ⭐ NEW
│   │   ├── Tab Button 1 (Active)
│   │   ├── Tab Button 2
│   │   ├── Tab Button 3
│   │   └── Tab Button N
│   └── Tab Content
│       ├── Custom Field 1
│       ├── Custom Field 2
│       └── Custom Field N
└── Submit Buttons
    ├── Cancel Button
    └── Register Button
```

## Data Flow

```
User Interaction
    ↓
Click Tab Button
    ↓
onTabChange(tabId)
    ↓
setActiveTabId(tabId)
    ↓
activeTabId State Updated
    ↓
useEffect Triggered
    ↓
scrollIntoView() Called
    ↓
Tab Scrolls to Center
    ↓
Tab Content Re-renders
```

## Component State Management

```
GroupRegisterPage State
├── customFieldTabs: CustomFieldTab[]
│   └── Contains all tabs and their fields
├── activeTabId: string
│   └── Currently active tab ID
├── customFieldValues: Record<string, any>
│   └── Values for all custom fields
├── formData: RegistrationFormData
│   └── Basic form fields
├── loading: boolean
│   └── Loading state for API calls
└── submitting: boolean
    └── Submission state
```

## TabNavigation Component State

```
TabNavigation Props
├── tabs: TabItem[]
│   ├── id: string
│   └── name: string
├── activeTabId: string
│   └── Currently active tab ID
└── onTabChange: (tabId: string) => void
    └── Callback to parent component

TabNavigation Internal State
└── itemRefs: useRef<Record<string, HTMLButtonElement>>
    └── References to tab buttons for scrolling
```

## Scroll Behavior Flow

```
Tab Clicked
    ↓
onClick Handler Triggered
    ↓
onTabChange(tabId) Called
    ↓
Parent Updates activeTabId
    ↓
TabNavigation Re-renders with New activeTabId
    ↓
useEffect Detects activeTabId Change
    ↓
Get Button Reference from itemRefs
    ↓
Call scrollIntoView({
      behavior: "smooth",
      inline: "center",
      block: "nearest"
    })
    ↓
Browser Animates Scroll
    ↓
Tab Centered in Viewport
```

## Rendering Logic

```
CustomFieldTabs.length > 0?
├── YES
│   ├── CustomFieldTabs.length > 1?
│   │   ├── YES → Render TabNavigation
│   │   └── NO → Skip TabNavigation
│   └── Render Active Tab Content
│       └── Map through fields and render inputs
└── NO → Skip entire section
```

## Event Flow

```
User Interaction
    ↓
┌─────────────────────────────────────┐
│ Tab Button Click                    │
│ onClick={() => onTabChange(tab.id)} │
└─────────────────────────────────────┘
    ↓
┌─────────────────────────────────────┐
│ Parent Component (GroupRegisterPage)│
│ setActiveTabId(tabId)               │
└─────────────────────────────────────┘
    ↓
┌─────────────────────────────────────┐
│ TabNavigation Re-renders            │
│ activeTabId prop updated            │
└─────────────────────────────────────┘
    ↓
┌─────────────────────────────────────┐
│ useEffect Hook Triggered            │
│ Dependency: [activeTabId]           │
└─────────────────────────────────────┘
    ↓
┌─────────────────────────────────────┐
│ scrollIntoView() Called             │
│ Smooth animation to center          │
└─────────────────────────────────────┘
    ↓
┌─────────────────────────────────────┐
│ Tab Content Updates                 │
│ Display fields for active tab       │
└─────────────────────────────────────┘
```

## Performance Optimization

```
Without Memoization
├── Parent Re-renders
├── TabNavigation Re-renders (unnecessary)
├── All Tab Buttons Re-render
└── Performance Impact: HIGH

With Memoization (React.memo)
├── Parent Re-renders
├── TabNavigation Checks Props
│   ├── Props Changed?
│   │   ├── YES → Re-render
│   │   └── NO → Skip Re-render
└── Performance Impact: LOW
```

## Styling Architecture

```
TabNavigation Container
├── Display: flex
├── Gap: 8px
├── Border: 1px solid #e5e7eb
├── Overflow: auto
├── Scrollbar: hidden
└── Scroll Behavior: smooth

Tab Button (Inactive)
├── Padding: 10px 16px
├── Font Weight: 400
├── Color: #6b7280
├── Border Bottom: none
└── Transition: all 0.3s ease

Tab Button (Active)
├── Padding: 10px 16px
├── Font Weight: 600
├── Color: #000
├── Border Bottom: 3px solid #003d82
└── Transition: all 0.3s ease
```

## API Integration

```
GroupRegisterPage
    ↓
fetchCustomFields()
    ↓
GET /api/groups/{groupId}/custom-fields
    ↓
Response: CustomFieldsResponse
    ├── success: boolean
    ├── message: string
    └── data: {
        groupId: string
        groupName: string
        tabs: CustomFieldTab[]
    }
    ↓
setCustomFieldTabs(tabs)
    ↓
setActiveTabId(tabs[0].id)
    ↓
TabNavigation Renders with Tabs
```

## Error Handling Flow

```
fetchCustomFields()
    ↓
Try API Call
    ├── Success
    │   ├── Parse Response
    │   ├── setCustomFieldTabs(tabs)
    │   └── setActiveTabId(tabs[0].id)
    └── Error
        ├── Log Error
        ├── toast.error(message)
        └── setLoading(false)
```

## Mobile Responsiveness

```
Desktop (> 768px)
├── All tabs visible
├── Horizontal scroll if needed
└── Smooth scroll to center

Tablet (768px - 480px)
├── Most tabs visible
├── Horizontal scroll if needed
└── Smooth scroll to center

Mobile (< 480px)
├── Few tabs visible
├── Horizontal scroll required
├── Touch-friendly scrolling
└── Smooth scroll to center
```

## Browser Compatibility Matrix

```
Feature                 Chrome  Firefox  Safari  Edge  Mobile
─────────────────────────────────────────────────────────────
scrollIntoView()        ✅      ✅       ✅      ✅    ✅
smooth behavior         ✅      ✅       ✅      ✅    ✅
inline: center          ✅      ✅       ✅      ✅    ✅
overflow: auto          ✅      ✅       ✅      ✅    ✅
scrollbar-width         ✅      ✅       ❌      ✅    ✅
-webkit-scrollbar       ✅      ❌       ✅      ✅    ✅
-ms-overflow-style      ❌      ❌       ❌      ✅    ❌
```

## Memory Management

```
Component Mount
├── Create itemRefs object
├── Create useEffect hook
└── Memory: ~1KB

Component Render
├── Map through tabs
├── Create button elements
└── Memory: ~5KB per tab

Component Unmount
├── Clear itemRefs
├── Clear useEffect
└── Memory: Freed
```

## Accessibility Features

```
Keyboard Navigation
├── Tab Key: Focus on tab buttons
├── Enter/Space: Activate tab
└── Arrow Keys: (Future enhancement)

Screen Reader
├── Semantic HTML: <button> elements
├── ARIA Labels: (Can be added)
└── Focus Management: Proper focus handling

Visual Feedback
├── Active Tab: Clear visual indicator
├── Hover State: Color change
└── Focus State: Browser default
```

## Future Enhancement Points

```
TabNavigation Component
├── Add Keyboard Navigation
│   ├── Arrow keys to switch tabs
│   └── Home/End keys
├── Add Swipe Support
│   ├── Swipe left/right on mobile
│   └── Gesture detection
├── Add Lazy Loading
│   ├── Load tab content on demand
│   └── Reduce initial load
├── Add Customization
│   ├── Custom styling props
│   ├── Icon support
│   └── Badge support
└── Add Animations
    ├── Fade in/out content
    ├── Slide animations
    └── Transition effects
```

## Testing Strategy

```
Unit Tests
├── Component Renders
├── Props Validation
├── Event Handlers
└── State Updates

Integration Tests
├── Tab Switching
├── Content Updates
├── Form Submission
└── API Integration

E2E Tests
├── User Flow
├── Tab Navigation
├── Form Completion
└── Success/Error Scenarios

Performance Tests
├── Re-render Count
├── Memory Usage
├── Scroll Performance
└── Load Time
```
