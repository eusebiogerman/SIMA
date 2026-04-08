# SIMA
Project: Simple Inventory Management App
Task:
1. Develop a basic WPF desktop application to manage an inventory list.
2. Each inventory item should include Name, Category, and Quantity.
3. The application will allow the user to add, edit, delete, and filter items.
Suggested UI Layout:
1. Top Section: Search bar for filtering items by Name or Category.
2. Main Section: Data grid or list showing all inventory items, with columns for Name, Category,
and Quantity.
3. Bottom Section: Input fields and buttons for adding, editing, and deleting items.



## 📌 References
1. [Developer Manual — Windows Management Module](#Developer)
2. [Introduction ](#1.).
3. [General Architecture](##2.). 	
    * 3.1 [Involved Components](###2.1)
    * 3.2 [Module Initialization](##3.) 	
    * 3.3 [Window_Initialized](###3.1) 
    * 3.4 [Window_Loaded](###3.2) 
4. [Data Loading and Filtering](##4.) 	
    *4.1 [FillGrid Method](###4.1) 
5. [Record Editing](##5.) 	
    * 5.1 [EditControl Method](###5.1) 
6. [Data Saving](##6.) 	
    * 6.1 [btnsSaveStock_Click](###6.1) 
    * 6.2 [Record Deletion](##7.) 	
    * 6.3 [btnRemove_Click](###7.1) 
7. [Navigation and Pagination](##8.) 	
    * 7.1 [Navigation](###8.1) 
8. [LOV Synchronization (Product → Brand)](##9.) 	
    * 8.1 [cmbProduct_SelectionChanged](###9.1) 
9. [Search with Debounce](##10) 	
    * 9.1 [txtSearch_TextChanged](###10.1) 
10. [Form Clearing](##11.) 	
11. [Extending the Module](##12.) 	
    * 11.1 [Adding New Filters](###12.1) 
    * 11.2 [Integrating a New Child Service](###12.2) 
12. [Best Practices](##13.) 	
13. [Technical Documentation — Every Window Management Module](##Technical) 	
 1. [Module Purpose](#Purpose)
 2. [Window Initialization](#Initialization)
	* 2.1 [Initialize Window](#Initialize)
 3. [Data Loading and Updating](#Loading)
 	* 3.1 [FillGrid Method](#FillGrid)
 4. [Record Editing](#Editing)
 	* 4.1 [EditControl Method](#EditControl)
 5. [Main Events](#Events)
 	* 5.1 [Save Stock](#Save)
 	* 5.2 [Form Clearing](#Clearing) 
 	* 5.3 [Page Navigation](#Navigation)
 	* 5.4 [PopupLov TextBox Change](#Change)
 	* 5.5 [Search with Debounce](#Debounce)
 	* 5.6 [Deletion](#Deletion)
 	* 5.7 [Update](#Update)
 6. [Window Lifecycle](#Lifecycle)
 	* 6.1 [Window Initialized]
 	* 6.2 [Window Loaded]
 	* 6.3 [Window Content Rendered]
 	* 6.4 [Window Responsive]
 7. [Full Edit]


## 1. Introduction 

This module implements the interaction logic between the WPF UI and the domain services to manage product stock.

**It includes:**
* Environment initialization.
* Data loading and filtering.
* Record editing and deletion.
* Paged navigation.
* LOV Synchronization (Product → Brand).
* Event handling and form state management.

The module utilizes a centralized component: `WindowServices<TModel, TView, TParam>`, which standardizes common behaviors across windows.

---

## 2. General Architecture
### 2.1 Involved Components

| Component | Role |
| :--- | :--- |
| **StockProductServices** | Stock CRUD operations. |
| **BrandServices** | Retrieves brands filtered by product. |
| **WindowServices** | UI control, pagination, LOVs, editing, and filters. |
| **StockViewModel** | Data and event exposure for the model. |
| **WPF Controls** | TextBox, ComboBox, Grid, PageControl. |

---

## 3. Module Initialization

### 3.1 `Window_Initialized`
Responsible for constructing all services and the ViewModel.

**Workflow:**
1. Create utilities and configuration.
2. Instantiate the ViewModel.
3. Instantiate domain services.
4. Configure `WindowServices`.
5. Register child services (`BrandServices`).
6. Suppress ComboBox events to prevent premature triggers.

**Relevant Code:**
```csharp
_windowservices = new WindowServices<StockProduct, StockProductView, StockProductParam>(_config, _page, service);
```

### 3.2 `Window_Loaded`
Configures visual controls and prepares pagination.

**Responsibilities:**
* Assign controls to `WindowServices`.
* Configure Parent and Child LOVs.
* Initialize pagination.
* Prepare the form in a closed state.

**Key Code:**
```csharp
_windowservices.Page.Offset = (int?)windowservices.PageControl.GetSelectedItemsPerPage();
```

---

## 4. Data Loading and Filtering

### 4.1 `FillGrid` Method
Loads the grid by applying active filters or an explicit parameter.

**Workflow:**
1. Obtain active filters.
2. Call the `GetByFilter` service.
3. Populate the grid via `WindowServices`.

**Usage Example:**
```csharp
await FillGrid(new StockProductParam { idBrand = 5 });
```

---

## 5. Record Editing

### 5.1 `EditControl` Method
Generates an action that populates form controls with the data from the selected record.

**Responsibilities:**
* Loading numerical values.
* Synchronizing Product LOV.
* Synchronizing Brand LOV.
* Handling null `idProduct` cases.

**Invocation Example:**
```csharp
await _windowservices.Edit("40%", EditControl(param));
```
---
## 6. Data Saving

### 6.1 `btnsSaveStock_Click`
Controls the complete save cycle.

**Workflow:**
1. Read UI values.
2. Validate fields.
3. Show progress.
4. Call `_service.Set()`.
5. Clear form.
6. Refresh grid.

**Key Code:**
```csharp
bool isset = await service.Set(new StockProduct { ... });
```

---

## 7. Record Deletion

### 7.1 `btnRemove_Click`
Deletes a selected record from the grid.

**Workflow:**
1. User confirmation.
2. Obtain `rowData` from the button.
3. Call `_service.Delete()`.
4. Refresh grid and show message.

**Key Code:**
```csharp
bool valid = await service.Delete(rowData.IdStock) > 0;
```

---

## 8. Navigation and Pagination

### 8.1 Navigation
Methods `PagePrevious_Click` and `PageNext_Click` delegate to `WindowServices`:
```csharp
windowservices.NavigationGrid(DIRECTION.next);
```

### 8.2 Page Size Change
`PageNavigation_SelectionChanged` updates the limit and reloads data.

---

## 9. LOV Synchronization (Product → Brand)

### 9.1 `cmbProduct_SelectionChanged`
When the product changes, associated brands are loaded.

**Workflow:**
1. Get selected product.
2. Determine if a dummy value (-1) is used.
3. Call `BrandServices`.
4. Populate Child LOV.

**Key Code:**
```csharp
var cat = await brandservices.GetByFilter(new BrandParam { idProduct = sendobj.Id });
```

---

## 10. Search with Debounce

### 10.1 `txtSearch_TextChanged`
Prevents excessive server calls using a 300ms delay and `CancellationTokenSource`.

**Key Code:**
```csharp
await Task.Delay(300, cts.Token);
```

---

## 11. Form Clearing
Methods like `btnNewBrand_Click`, `btnsClear_Click`, and `btnsClose_Click` utilize:
```csharp
await _windowservices.ClearFilters("40%", () => {
    txtIdStock.Text = string.Empty;
    txtStock.Text = string.Empty;
});
```

---

## 12. Extending the Module

### 12.1 Adding New Filters
1. Create new properties in `StockProductParam`.
2. Modify `FillGrid` to include them.
3. Add UI controls and update `activeFilters` in `WindowServices`.

### 12.2 Integrating a New Child Service
```csharp
_windowservices.ServiceChildCombobox.Add(
    typeof(IContextservices<NewEntity, NewView, NewParam>),
    new NewService(_config)
);
```

---

## 13. Best Practices
* **Decoupling:** Avoid business logic inside UI events.
* **Validation:** Extract validations to separate methods to improve maintainability.
* **MVVM:** Migrate toward using Commands and `IDataErrorInfo`.
* **Safety:** Avoid direct `int.Parse` conversions without prior validation.

---

Would you like me to generate a technical flowchart, an interaction diagram, or a more formal version for architectural documentation next?


## 📘 Technical Documentation — Every Window Management Module
*(Based on the provided file)*

### 1. Module Purpose
The module manages the complete maintenance cycle of product stock, including:

* Window initialization and context setup.
* Data loading and filtering.
* Record editing and updating.
* Paged navigation.
* Synchronization between LOV (List of Values) controls (Product / Brand).
* UI event handling.
* Error handling and visual feedback for the user.

This module integrates with domain services such as `StockProductServices` and `BrandServices`, and utilizes a centralized `WindowServices<T>` component to standardize UI behaviors.

### 2. Window Initialization
#### 2.1 `InitializeWstock` Method
Sets the edit mode and determines if the window was opened from the main form.

```csharp
_windowservices.EditMode = rowData != null;`  
_windowservices.FromMain = windowservices.EditMode;`
```

**Usage Example:** When the window is opened via an "Edit" button, a `StockProductView` is passed, and the window enters edit mode.

### 3. Data Loading and Updating
#### 3.1 `FillGrid` Method
Loads grid data by applying active filters or an explicit parameter.

```csharp
var prod = await _service.GetByFilter(param ?? windowservices.activeFilters("Name"));
```

**Workflow:**
1.  Retrieves active filters.
2.  Calls `StockProductServices.GetByFilter`.
3.  Sends results to the grid via `_windowservices.Fill()`.

**Example:**  

```csharp 
await FillGrid(new StockProductParam { idBrand = 10 }); 
```
### 4. Record Editing
#### 4.1 `EditControl` Method
Generates an action that populates the form controls with the values of the selected record.

```csharp
txtIdStock.Text = rowData.idStock.ToString();  
ParentLovtextbox.SelectedItem = itemProd;
```
**Responsibilities:**
* Synchronizing Product and Brand LOVs.
* Loading numerical values.
* Handling cases where `idProduct` is null.

**Invoke Example:** 
```csharp
await _windowservices.Edit("40%", EditControl(param));
```
### 5. Main Events
#### 5.1 Save Stock — `btnsSaveStock_Click`
Performs validations, displays progress, saves the record, and refreshes the grid.

```csharp
bool isset = await service.Set(new StockProduct { ... });
```

**Key Behaviors:**
* Handling of SQL and general exceptions.
* Form clearing after saving.
* User feedback messages.

#### 5.2 Form Clearing — `btnNewBrand_Click`, `btnsClear_Click`
Both methods clear the fields and reset filters.

**Example:** 
```csharp
await _windowservices.ClearFilters("40%", () => {
    txtIdStock.Text = string.Empty;
    txtStock.Text = string.Empty;
});
```
#### 5.3 Navigation — `PagePrevious_Click`, `PageNext_Click`
Controls grid pagination.

```csharp
windowservices.NavigationGrid(DIRECTION.next);
```

#### 5.4 Product Change — `cmbProduct_SelectionChanged`
Dynamically updates the brand list based on the selected product.

```csharp
var cat = await brandservices.GetByFilter(new BrandParam { idProduct = sendobj.Id });
```
#### 5.5 Search with Debounce — `txtSearch_TextChanged`
Implements a 300 ms debounce using `CancellationTokenSource`.

```csharp
await Task.Delay(300, cts.Token);
```

#### 5.6 Deletion — `btnRemove_Click`
Confirms, deletes, and updates the grid.

```csharp
bool valid = await service.Delete(rowData.IdStock) > 0;
```
#### 5.7 Update — `btnUpdate_Click`
Loads the record in edit mode.

### 6. Window Lifecycle
#### 6.1 `Window_Initialized`
Configures:
* ViewModel
* Services
* WindowServices
* Child ComboBox: `windowservices.ServiceChildCombobox = new Dictionary<Type, object> { ... }`

#### 6.2 `Window_Loaded`
Assigns controls, initializes pagination, and prepares the form.

#### 6.3 `Window_ContentRendered`
If the window was opened from the main form, it loads the record and filters by brand.

#### 6.4 `Window_SizeChanged`
Dynamically adjusts the grid based on the window size.

### 7. Full Edit Flow Example
1.  User clicks **"Edit"**.
2.  `EditControl` is called with the record data.
3.  `WindowServices.Edit` opens the form.
4.  User modifies values.
5.  Click on **"Save"**.
6.  `btnsSaveStock_Click` validates and saves.
7.  The grid is refreshed.

### 8. Improvement Recommendations (Optional)
If you wish, I can generate a section for recommendations such as:
* Applying pure **MVVM** to remove logic from the code-behind.
* Replacing `MessageBox` with a decoupled **notification service**.
* Implementing validations with `IDataErrorInfo`.
* Extracting repeated logic into **utility methods**.
* Full **XML Documentation** for each method.
