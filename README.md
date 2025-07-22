# DynamicFormBuilderAppQIA

# 📝 Dynamic Form Builder - Requirements

## 📌 Detailed Steps

### 1. Create a Form Title
- The user should be able to enter a **title** for their form.

---

### 2. Add More Dropdown Fields
- The user should see an **"Add More"** button.
- When clicked, it should add a **new dropdown field**.
- For each dropdown, the user should be able to:
  - Set a **label** (e.g., "Select Country").
  - Choose if the field is **required** (via checkbox).
  - Select options from a **fixed list** (e.g., Option 1, Option 2, Option 3).

---

### 3. Save the Form
- When the user finishes creating the form, they should click the **Submit** button.
- The following information should be saved to the **database**:
  - Form Title
  - Each dropdown field:
    - Label
    - Options
    - Required status

---

### 4. Display Forms in a Grid
- After saving, each form should be displayed in a **grid view** (table layout).
- Each row in the grid should include a **Preview** button that leads to the preview page.

---

### 5. Preview the Form
- When the user clicks **Preview**, they should be taken to a new page where:
  - Dropdown fields are shown with their **selected options**.
  - If a dropdown field is **required**, a **red asterisk** (`*`) should appear next to the label.

---

## 🛠 Technologies You Will Use
- **ASP.NET Core MVC** (for server-side logic)
- **HTML/CSS/JavaScript** (for building the form and displaying the grid)
- **MS SQL Database** (for storing form data)

