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



Perfect, Joel. I have the code loaded and I can generate professional, clear documentation with examples, based directly on what appears in your file.

Below is a first version of formal technical documentation, aligned with your preferred style: structured, institutional, and oriented toward architecture and innovation.

📘 Technical Documentation — Stock Management Module
(Based on the provided file)

1. Module Purpose
The module manages the complete maintenance cycle of product stock, including:

Window initialization and context setup.

Data loading and filtering.

Record editing and updating.

Paged navigation.

Synchronization between LOV (List of Values) controls (Product / Brand).

UI event handling.

Error handling and visual feedback for the user.

This module integrates with domain services such as StockProductServices and BrandServices, and utilizes a centralized WindowServices<T> component to standardize UI behaviors.

2. Window Initialization
2.1 InitializeWstock Method
Sets the edit mode and determines if the window was opened from the main form.

_windowservices.EditMode = rowData != null;

_windowservices.FromMain = windowservices.EditMode;

Usage Example: When the window is opened via an "Edit" button, a StockProductView is passed, and the window enters edit mode.

3. Data Loading and Updating
3.1 FillGrid Method
Loads grid data by applying active filters or an explicit parameter.

var prod = await _service.GetByFilter(param ?? windowservices.activeFilters("Name"));

Workflow:

Retrieves active filters.

Calls StockProductServices.GetByFilter.

Sends results to the grid via _windowservices.Fill().

Example: await FillGrid(new StockProductParam { idBrand = 10 });

4. Record Editing
4.1 EditControl Method
Generates an action that populates the form controls with the values of the selected record.

txtIdStock.Text = rowData.idStock.ToString();

ParentLovtextbox.SelectedItem = itemProd;

Responsibilities:

Synchronizing Product and Brand LOVs.

Loading numerical values.

Handling cases where idProduct is null.

Invocación Example: await _windowservices.Edit("40%", EditControl(param));

5. Main Events
5.1 Save Stock — btnsSaveStock_Click
Performs validations, displays progress, saves the record, and refreshes the grid.

bool isset = await service.Set(new StockProduct { ... });

Key Behaviors:

Handling of SQL and general exceptions.

Form clearing after saving.

User feedback messages.

5.2 Form Clearing — btnNewBrand_Click, btnsClear_Click
Both methods clear the fields and reset filters.

Example: ```csharp
await _windowservices.ClearFilters("40%", () => {
txtIdStock.Text = string.Empty;
txtStock.Text = string.Empty;
});


#### 5.3 Navigation — `PagePrevious_Click`, `PageNext_Click`
Controls grid pagination.

> `windowservices.NavigationGrid(DIRECTION.next);`

#### 5.4 Product Change — `cmbProduct_SelectionChanged`
Dynamically updates the brand list based on the selected product.

> `var cat = await brandservices.GetByFilter(new BrandParam { idProduct = sendobj.Id });`

#### 5.5 Search with Debounce — `txtSearch_TextChanged`
Implements a 300 ms debounce using `CancellationTokenSource`.

> `await Task.Delay(300, cts.Token);`

#### 5.6 Deletion — `btnRemove_Click`
Confirms, deletes, and updates the grid.

> `bool valid = await service.Delete(rowData.IdStock) > 0;`

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
