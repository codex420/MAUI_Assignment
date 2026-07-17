# MAUI Dashboard — Project Presentation & Structure

> A cross-platform admin **Dashboard** built with **.NET MAUI**, targeting **Android** and **Windows**.
> Follows the **MVVM** pattern with dependency injection, compiled bindings, and a component-based UI.

---

## 1. Overview

| Property | Value |
|---|---|
| **App name** | MAUI Dashboard |
| **App ID** | `com.fiftyfivetech.mauidashboard` |
| **Framework** | .NET MAUI (`net9.0`) |
| **Targets** | `net9.0-android`, `net9.0-windows10.0.19041.0` |
| **Version** | 1.0 (build 1) |
| **Architecture** | MVVM (Model – View – ViewModel) |
| **Min Android** | API 21 (Android 5.0) |
| **Min Windows** | 10.0.17763.0 |

### Key libraries
| Package | Purpose |
|---|---|
| `Microsoft.Maui.Controls` | Core MAUI UI framework |
| `CommunityToolkit.Mvvm` (8.4.0) | `[ObservableProperty]` / `[RelayCommand]` source generators |
| `Microsoft.Extensions.Logging.Debug` | Debug logging |

---

## 2. Architecture at a Glance

```
┌─────────────────────────────────────────────────────────────┐
│                          VIEW (XAML)                          │
│   MainPage.xaml  +  Components/*.xaml   (UI only, no logic)   │
└───────────────────────────┬─────────────────────────────────┘
                            │  Data Binding (x:DataType, compiled)
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                    VIEWMODEL (State + Logic)                  │
│   DashboardViewModel.cs   ([ObservableProperty], commands)    │
└───────────────────────────┬─────────────────────────────────┘
                            │  Interface call
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                     SERVICE (Data Source)                     │
│   IDashboardService → DashboardService   (in-memory data)     │
└───────────────────────────┬─────────────────────────────────┘
                            │  returns
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                       MODELS (POCOs)                          │
│   StatCard, OrderItem, ActivityItem, NavItem, ...            │
└─────────────────────────────────────────────────────────────┘
```

**Why this matters:** The View never talks to data directly. The ViewModel holds all state and exposes commands; the Service is swappable (today it's in-memory; tomorrow it could be an HTTP API — nothing else changes).

---

## 3. Folder & File Structure

```
MAUI_Assignment/
│
├── App.xaml / App.xaml.cs          # App entry; creates the Window + AppShell
├── AppShell.xaml / .cs             # Single-route shell (Dashboard → MainPage)
├── MauiProgram.cs                  # DI container, fonts, platform tweaks
├── MAUI_Assignment.csproj          # Targets, packages, app metadata
│
├── Views/                          # Full-page screens
│   ├── MainPage.xaml               # The dashboard layout (responsive grid)
│   └── MainPage.xaml.cs            # Code-behind (minimal)
│
├── ViewModels/                     # Presentation logic & state
│   └── DashboardViewModel.cs       # 451 lines — the "brain" of the app
│
├── Components/                     # Reusable UI building blocks (ContentViews)
│   ├── SidebarView                 # Left navigation menu
│   ├── HeaderView                  # Top summary (earnings / sales)
│   ├── CardView                    # Individual stat card
│   ├── TrafficView                 # Traffic breakdown widget
│   ├── RecentActivitiesView        # Activity feed list
│   ├── OrderStatusView             # Orders table + pagination
│   └── ListItemView                # Generic list row
│
├── Models/                         # Plain data objects (POCOs)
│   ├── StatCard.cs                 # Dashboard stat card (+ StatCardKind enum)
│   ├── OrderItem.cs                # A single order row
│   ├── ActivityItem.cs             # A recent-activity entry
│   ├── NavItem.cs                  # A sidebar navigation item
│   ├── SummaryStat.cs              # Header summary metric
│   └── TrafficSlice.cs             # A traffic-source slice
│
├── Services/                       # Data + helpers
│   ├── IDashboardService.cs        # Data contract (interface)
│   ├── DashboardService.cs         # In-memory implementation
│   ├── Converters.cs               # XAML value converters
│   └── NotEmptyConverter.cs        # "is string non-empty?" converter
│
├── Platforms/                      # Platform-specific entry points
│   ├── Android/  (MainActivity, MainApplication)
│   └── Windows/  (App.xaml, App.xaml.cs)
│
└── Resources/                      # Fonts, images, icons, styles, colors
```

---

## 4. Layer-by-Layer Walkthrough

### 4.1 Startup — `MauiProgram.cs`
The composition root. It:
1. Registers **fonts** (OpenSans + Segoe MDL2 icon font).
2. Wires up **Dependency Injection**:
   ```csharp
   builder.Services.AddSingleton<IDashboardService, DashboardService>();
   builder.Services.AddSingleton<DashboardViewModel>();
   builder.Services.AddSingleton<MainPage>();
   ```
3. Adds a **Windows-only** tweak to style the `Picker` dropdown (white background, black text).

> **Talking point:** Everything is a singleton because it's a single-page dashboard — one shared state object for the app's lifetime.

### 4.2 Shell & Window — `App.xaml.cs` + `AppShell.xaml`
- `App.CreateWindow` builds the window, titles it *"MAUI Dashboard"*, and sets a fixed **1280×820** size on desktop.
- `AppShell` defines a single route (`MainPage`) with the flyout disabled and the nav bar hidden — the app draws its own custom sidebar instead.

### 4.3 The View — `Views/MainPage.xaml`
A **responsive** dashboard that reflows for wide / medium / compact (Android) screens:
- **Sidebar** — docked on wide screens, overlay + scrim + hamburger button on compact.
- **Row 1** — Header summary + Traffic widget.
- **Row 2** — Stat cards in a `FlexLayout` that wraps 4 → 2 → 1 across.
- **Row 3** — Recent Activities + Order Status table.
- **Modals** — Add Order, Info, and Summary dialogs.
- **WebView** — non-dashboard sidebar tabs load a URL instead.

> **Note:** Uses **`FlexLayout` + `BindableLayout`** (not a nested `CollectionView`) so the stat-card row measures correctly inside a `ScrollView` on Android — a known MAUI Android quirk.

### 4.4 The ViewModel — `ViewModels/DashboardViewModel.cs`
The heart of the app (451 lines). Responsibilities:
- Holds all **observable state** (`[ObservableProperty]` generates the boilerplate).
- Drives the **responsive layout** — computes grid rows/columns/spans and card basis per screen size.
- Manages **modals** (Add Order, Info, Summary) and **commands** (toggle sidebar, submit order, pagination…).
- Handles **pagination** for the orders table (page size 5, with computed `CanGoNext` / `CanGoPrevious` / status text).
- Switches between the **dashboard** and an in-app **WebView** for other tabs.

### 4.5 Services — data layer
- **`IDashboardService`** — the contract: `GetNavItems()`, `GetStatCards()`, `GetOrders()`, `GetTraffic()`, `GetHeaderTotals()`, etc.
- **`DashboardService`** — returns hardcoded, in-memory data. Colors are pulled from the app's `ResourceDictionary` so data and theme stay in sync.

> **Talking point:** Because the ViewModel depends only on the **interface**, you could replace `DashboardService` with a real REST client and the UI wouldn't change a line.

### 4.6 Models — the data shapes
Simple POCOs (Plain Old CLR Objects), no logic:

| Model | Represents | Key fields |
|---|---|---|
| `StatCard` | A KPI card | `Title, Value, Note, Background, Accent, Kind` |
| `OrderItem` | An order row | `Invoice, Customer, From, Price, Status, StatusColor` |
| `ActivityItem` | Activity feed entry | `TimeAgo, Title, Author, Detail, Icon` |
| `NavItem` | Sidebar menu item | `Icon, Title, IsActive, Url` |
| `SummaryStat` | Header metric | `Icon, Title, Value, IconColor` |
| `TrafficSlice` | Traffic source | `Label, Percent, Color` |

---

## 5. Data Flow Example — "Add an Order"

```
User taps "Add Order"
      │
      ▼
OrderStatusView  ──Command──►  DashboardViewModel.OpenAddOrderModalCommand
      │                                   │
      │                          IsAddOrderModalVisible = true
      ▼                                   │
Modal appears (bound to VM)               │
User fills form + taps Submit             │
      │                                   ▼
      └──Command──►  SubmitNewOrderCommand ──► creates OrderItem ──► adds to list
                                              └──► refreshes pagination + closes modal
```

The View only raises commands and binds to properties — **all logic lives in the ViewModel**.

---

## 6. Cross-Platform Notes

| Concern | How it's handled |
|---|---|
| **Responsive layout** | ViewModel recomputes grid spans / card basis per width breakpoint |
| **Android ScrollView bug** | `FlexLayout` + `BindableLayout` instead of nested `CollectionView` |
| **Windows Picker styling** | Handler mapper override in `MauiProgram.cs` |
| **Desktop window size** | Fixed 1280×820 on Windows / Mac Catalyst |
| **Icons** | Segoe MDL2 (`segmdl2.ttf`) glyph font |

---

## 7. How to Build & Run

**Run on Android emulator:**
```bash
dotnet build -t:Run -f net9.0-android -c Debug
```

**Run on Windows:**
```bash
dotnet build -t:Run -f net9.0-windows10.0.19041.0 -c Debug
```

**Produce a release APK:**
```bash
dotnet publish -f net9.0-android -c Release -p:AndroidPackageFormat=apk
# Output: bin/Release/net9.0-android/publish/com.fiftyfivetech.mauidashboard-Signed.apk
```

---

## 8. Presentation Talking Points (Quick Script)

1. **"It's an MVVM dashboard"** — clean separation: XAML views, C# view-model, swappable service.
2. **"Dependency injection from the start"** — services and view-models registered in `MauiProgram`.
3. **"Component-driven UI"** — each dashboard widget is its own reusable `ContentView`.
4. **"Truly responsive"** — the same page reflows from a wide desktop layout down to a compact Android phone with an overlay sidebar.
5. **"Compiled bindings"** — `x:DataType` everywhere for compile-time safety and runtime speed.
6. **"Data is abstracted"** — swapping the in-memory service for a live API needs zero UI changes.
7. **"Cross-platform"** — one codebase → Android APK + Windows app.

---

*Generated as a structural overview of the MAUI_Assignment project.*
