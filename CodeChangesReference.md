# Code Changes Reference Index

This document maps all the changes made for the layout overlap fix, Swipe-to-Edit, and custom Info modal features. Use the search tags (e.g. `SEARCH_...`) to easily locate code blocks in your editor.

---

## 1. Project Configuration
**File**: [MAUI_Assignment.csproj](file:///c:/Users/fiftyfive/source/repos/MAUI_Assignment/MAUI_Assignment.csproj)

<!-- SEARCH_CSPROJ_BYPASS -->
### Bypass Java Interface Compile Error (XA4212)
Added the `AndroidErrorOnCustomJavaObject` option under the main `<PropertyGroup>` to prevent native ConstraintLayout/SwipeView interface casting failures:

```xml
		<TargetPlatformMinVersion Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'windows'">10.0.17763.0</TargetPlatformMinVersion>
		<!-- SEARCH_CSPROJ_BYPASS -->
		<AndroidErrorOnCustomJavaObject>false</AndroidErrorOnCustomJavaObject>
	</PropertyGroup>
```

---

## 2. View Model (DashboardViewModel.cs)
**File**: [DashboardViewModel.cs](file:///c:/Users/fiftyfive/source/repos/MAUI_Assignment/ViewModels/DashboardViewModel.cs)

<!-- SEARCH_VIEWMODEL_PROPERTIES -->
### Info & Summary Modal Properties
Added state variables to track the Info and Summary modal visibilities and text contents:

```csharp
    // SEARCH_VIEWMODEL_PROPERTIES
    [ObservableProperty]
    private bool _isInfoModalVisible = false;

    [ObservableProperty]
    private string _infoModalMessage = string.Empty;

    [ObservableProperty]
    private bool _isSummaryModalVisible = false;

    [ObservableProperty]
    private string _summaryModalDetails = string.Empty;
```

<!-- SEARCH_VIEWMODEL_COMMANDS -->
### Info & Summary Modal Commands
Added commands to show the static Info text, toggle the Last Month Summary modal, and close them:

```csharp
    // SEARCH_VIEWMODEL_COMMANDS
    [RelayCommand]
    private void ShowInfo()
    {
        InfoModalMessage = "These are the order statuses of all the people inside the dashboard.";
        IsInfoModalVisible = true;
    }

    [RelayCommand]
    private void CloseInfoModal()
    {
        IsInfoModalVisible = false;
    }

    [RelayCommand]
    private void LastMonthSummary()
    {
        SummaryModalDetails = "Last Month Performance Summary:\n\n" +
                              "• Total Earnings: $24,890 (+12% growth)\n" +
                              "• Total Completed Orders: 1,420\n" +
                              "• Top Customer Location: Brazil (450 orders)\n" +
                              "• Active Platform Users: 890\n" +
                              "• Resolution Rate: 98.4%\n" +
                              "• Average Delivery Time: 2.3 days";
        IsSummaryModalVisible = true;
    }

    [RelayCommand]
    private void CloseSummaryModal()
    {
        IsSummaryModalVisible = false;
    }
```

---

## 3. Order Status Component (OrderStatusView.xaml)
**File**: [OrderStatusView.xaml](file:///c:/Users/fiftyfive/source/repos/MAUI_Assignment/Components/OrderStatusView.xaml)

<!-- SEARCH_ORDER_TOOLBAR -->
### Grid Toolbar Overlap & Info Button Binding
Changed columns to `Auto,Auto,Auto,Auto,*,Auto`. Removed the fixed `WidthRequest="150"` from the search box so it fits responsive screen widths. Added a `TapGestureRecognizer` to the Info button:

```xml
            <!-- SEARCH_ORDER_TOOLBAR -->
            <Grid ColumnDefinitions="Auto,Auto,Auto,Auto,*,Auto" ColumnSpacing="8">
                <Button Grid.Column="0" Text="+ Add" Style="{StaticResource AccentButton}" HeightRequest="34" Padding="12,4" FontSize="12" Command="{Binding ShowAddOrderModalCommand}" />
                <Border Grid.Column="1" WidthRequest="34" HeightRequest="34" StrokeThickness="1" Stroke="{StaticResource BorderColor}">
                    <Border.StrokeShape><RoundRectangle CornerRadius="6" /></Border.StrokeShape>
                    <!-- SEARCH_INFO_GESTURE -->
                    <Border.GestureRecognizers>
                        <TapGestureRecognizer Command="{Binding ShowInfoCommand}" />
                    </Border.GestureRecognizers>
                    <Label Text="&#x2139;" HorizontalOptions="Center" VerticalOptions="Center" TextColor="{StaticResource TextSecondary}" />
                </Border>
                <Border Grid.Column="2" WidthRequest="34" HeightRequest="34" StrokeThickness="1" Stroke="{StaticResource BorderColor}">
                    <Border.StrokeShape><RoundRectangle CornerRadius="6" /></Border.StrokeShape>
                    <Label Text="&#x1F5D1;" HorizontalOptions="Center" VerticalOptions="Center" TextColor="{StaticResource TextSecondary}" />
                </Border>
                <Border Grid.Column="3" WidthRequest="34" HeightRequest="34" StrokeThickness="1" Stroke="{StaticResource BorderColor}">
                    <Border.StrokeShape><RoundRectangle CornerRadius="6" /></Border.StrokeShape>
                    <Label Text="&#x1F5A8;" HorizontalOptions="Center" VerticalOptions="Center" TextColor="{StaticResource TextSecondary}" />
                </Border>

                <!-- SEARCH_DYNAMIC_SEARCHBAR (replaces Grid.Column 5 with 150 WidthRequest) -->
                <Border Grid.Column="4" HeightRequest="34" StrokeThickness="1" Stroke="{StaticResource BorderColor}" Padding="0">
                    <Border.StrokeShape><RoundRectangle CornerRadius="6" /></Border.StrokeShape>
                    <Entry Text="{Binding SearchQuery}" Placeholder="Search..." FontSize="12" VerticalOptions="Center" BackgroundColor="Transparent" TextColor="{StaticResource TextPrimary}" Margin="10,0" />
                </Border>
                <Border Grid.Column="5" WidthRequest="34" HeightRequest="34" StrokeThickness="1" Stroke="{StaticResource BorderColor}">
                    <Border.StrokeShape><RoundRectangle CornerRadius="6" /></Border.StrokeShape>
                    <Label Text="&#x1F50D;" HorizontalOptions="Center" VerticalOptions="Center" TextColor="{StaticResource TextSecondary}" />
                </Border>
            </Grid>
```

---

## 4. Main Page View (MainPage.xaml)
**File**: [MainPage.xaml](file:///c:/Users/fiftyfive/source/repos/MAUI_Assignment/Views/MainPage.xaml)

<!-- SEARCH_INFO_MODAL_GRID -->
### Info Modal UI Overlay
Added the styled Info dialog Grid overlay:

```xml
        <!-- ============ SEARCH_INFO_MODAL_GRID ============ -->
        <Grid Grid.ColumnSpan="2"
              IsVisible="{Binding IsInfoModalVisible}"
              BackgroundColor="#aa000000"
              HorizontalOptions="Fill" VerticalOptions="Fill">
            
            <Border VerticalOptions="Center" HorizontalOptions="Center"
                    WidthRequest="350" Padding="24"
                    BackgroundColor="{StaticResource CardBackground}"
                    StrokeThickness="1" Stroke="{StaticResource BorderColor}">
                <Border.StrokeShape>
                    <RoundRectangle CornerRadius="12" />
                </Border.StrokeShape>
                
                <VerticalStackLayout Spacing="16">
                    <Label Text="Information" FontSize="18" FontFamily="OpenSansSemibold" TextColor="{StaticResource TextPrimary}" />
                    
                    <Label Text="{Binding InfoModalMessage}" FontSize="14" TextColor="{StaticResource TextSecondary}" LineBreakMode="WordWrap" />
                    
                    <Button Text="Close"
                            Style="{StaticResource AccentButton}"
                            Command="{Binding CloseInfoModalCommand}"
                            Margin="0,10,0,0" />
                </VerticalStackLayout>
            </Border>
        </Grid>
```

<!-- SEARCH_SUMMARY_MODAL_GRID -->
### Last Month Summary Modal UI Overlay
Added the styled Performance Summary dialog Grid overlay:

```xml
        <!-- ============ SEARCH_SUMMARY_MODAL_GRID ============ -->
        <Grid Grid.ColumnSpan="2"
              IsVisible="{Binding IsSummaryModalVisible}"
              BackgroundColor="#aa000000"
              HorizontalOptions="Fill" VerticalOptions="Fill">
            
            <Border VerticalOptions="Center" HorizontalOptions="Center"
                    WidthRequest="400" Padding="24"
                    BackgroundColor="{StaticResource CardBackground}"
                    StrokeThickness="1" Stroke="{StaticResource BorderColor}">
                <Border.StrokeShape>
                    <RoundRectangle CornerRadius="12" />
                </Border.StrokeShape>
                
                <VerticalStackLayout Spacing="16">
                    <Label Text="Last Month Summary" FontSize="18" FontFamily="OpenSansSemibold" TextColor="{StaticResource TextPrimary}" />
                    
                    <Label Text="{Binding SummaryModalDetails}" FontSize="14" TextColor="{StaticResource TextSecondary}" LineBreakMode="WordWrap" />
                    
                    <Button Text="Close"
                            Style="{StaticResource AccentButton}"
                            Command="{Binding CloseSummaryModalCommand}"
                            Margin="0,10,0,0" />
                </VerticalStackLayout>
            </Border>
        </Grid>
```
