---
trigger: always_on
---

# Payroll Application - Code Style Instructions

## 1. Application Context

This is an existing ASP.NET MVC Payroll application.

The application contains business-critical payroll calculations and database-driven workflows.

Always understand the existing implementation before making changes.

Follow the existing project architecture, coding style, naming conventions, database patterns, and technology stack.

Do not introduce a new architecture or framework unless explicitly requested.

---

## 2. Core Development Rules

- Make the smallest safe change required.
- Preserve existing functionality.
- Do not perform unrelated refactoring.
- Reuse existing code and project patterns.
- Do not create duplicate functionality.
- Do not rewrite working code without a clear reason.
- Do not modify unrelated files.
- Do not change behavior merely for code-style improvement.

Before creating a new class, method, service, repository, helper, JavaScript function, CSS class, or SQL procedure, search the project for existing implementations.

---

## 3. ASP.NET MVC Architecture

Maintain the existing MVC separation:

- Models
- Views
- Controllers
- Services
- Business Logic
- Repositories
- Data Access

Keep business logic out of Razor Views.

Do not put complex business logic directly into Controllers when an existing service/business layer is available.

Follow the project's existing dependency and data-flow pattern.

---

## 4. Controllers

Preserve existing:

- Controller names
- Action names
- Routes
- HTTP methods
- Action parameters
- Model binding
- Authorization attributes
- Redirect behavior
- ViewBag
- ViewData
- TempData
- AJAX endpoints

Do not rename controller actions or parameters unnecessarily.

Do not change an existing action's behavior when making a UI-only change.

Keep Controllers focused on request handling and orchestration.

---

## 5. Models

Preserve existing model properties and their names.

Do not rename properties simply for code-style reasons because they may be used by:

- Razor Views
- JavaScript
- AJAX
- Model binding
- Database mapping
- Reports
- APIs

Use appropriate validation attributes where the existing project pattern supports them.

---

## 6. Razor Views

Keep Razor Views focused on presentation.

Do not place payroll calculation logic inside Views.

For UI changes:

- Preserve form field names.
- Preserve element IDs.
- Preserve model binding.
- Preserve validation.
- Preserve AJAX calls.
- Preserve existing URLs/routes.
- Preserve existing JavaScript dependencies.
- Preserve existing form submission behavior.

Improve UI through HTML/Razor markup and CSS without changing business behavior.

---

## 7. Payroll Business Logic & Calculation Rules (STRICT PROHIBITION)

Payroll calculation and CTC logic are business-critical and sensitive.

**CRITICAL RULE: DO NOT CHANGE ANY CALCULATION, FORMULA, MATH, OR BUSINESS LOGIC WITHOUT EXPLICIT WRITTEN INSTRUCTION AND CONFIRMATION FROM THE USER.**

- **Never** modify, "optimize", refactor, rewrite, or adjust existing payroll calculation logic during UI, property mapping, database, or bug-fix tasks.
- **Never** change how values are computed or derived (e.g. Basic, Gross, Deductions, Contributions, Net Pay, Monthly CTC, Annual CTC, Statutory PF/ESIC/PTAX/TDS).
- **Never** change formulas, rounding methods, formula evaluation order, caps, eligibility checks, or rate multipliers on your own assumption.
- If a task involves adding a new field (such as `IsTaxable`, flags, IDs, etc.):
  - Only map, bind, pass, and save the field.
  - **DO NOT** change the calculation logic or equations to incorporate the new field into salary computation unless the user gives exact formulas and explicit instructions to do so.
- If an existing calculation appears incorrect, incomplete, or ambiguous:
  - **STOP immediately**.
  - Document the exact observation and ask the user for confirmation/instructions before touching the calculation code.

Preserve all existing rules for:

- Salary calculation formulas and evaluation
- Gross salary and Monthly/Annual CTC calculation
- Basic salary calculations and percentages
- Salary components and allowances
- Deductions and recoveries
- PF (EE & ER) limits, caps, and rounding
- ESI/ESIC (EE & ER) limits, caps, and rounding
- Tax / PTAX slabs and TDS calculations
- LWP and paid days arithmetic
- Attendance and overtime multipliers
- Arrears, increments, and adjustments
- Loans and loan installments
- Full & Final settlement logic
- Payroll processing and finalization workflows
- Payroll periods and cycles
- Rounding rules (MidpointRounding, Ceiling, Floor)
- Employee eligibility logic

---

## 8. Payroll Data Safety

Payroll data must be treated as business-critical.

Never modify real payroll data simply to test code.

Do not unnecessarily change:

- Employee salary
- Employee IDs
- Payroll IDs
- Company IDs
- Tenant IDs
- Salary component IDs
- Payroll period IDs
- Payroll transaction records
- Finalized payroll records

Use development/test data for testing.

---

## 9. Database / SQL

Follow the existing project's database-access pattern.

If the application uses:

- ADO.NET → continue using ADO.NET.
- Dapper → continue using Dapper.
- Stored Procedures → follow the existing stored procedure pattern.
- DatabaseHelper → reuse the existing helper.

Do not introduce Entity Framework or another ORM unless explicitly requested.

Prefer:

- Parameterized queries
- Explicit column lists
- Existing stored procedures
- Existing database helpers
- Existing repository/data-access patterns

Avoid:

- `SELECT *`
- String-concatenated SQL
- Unnecessary dynamic SQL
- Duplicate database logic

---

## 10. Stored Procedures

Before modifying a payroll stored procedure:

1. Find where it is used.
2. Check dependent procedures/functions/views.
3. Check the related Controller/Service/Repository.
4. Understand the existing input/output behavior.
5. Confirm that the requested change requires modifying the procedure.

Do not modify stored procedure behavior for a UI-only request.

Do not rename procedure parameters or result columns unnecessarily.

---

## 11. Database Structure

Do not casually change:

- Tables
- Columns
- Primary keys
- Foreign keys
- IDs
- Views
- Functions
- Stored procedures

Never drop or rename database objects without explicit approval.

Never modify production data unless explicitly instructed.

---

## 12. UI Modernization

When the user requests UI/design changes:

Change only the presentation layer wherever possible.

Allowed:

- Razor/HTML structure
- CSS
- Typography
- Spacing
- Colors
- Borders
- Border radius
- Shadows
- Tables
- Forms
- Cards
- Modals
- Tabs
- Buttons
- Responsive layout
- Hover/focus states
- Visual hierarchy

Do not change:

- Payroll calculations
- Controllers
- Services
- Repositories
- Database
- Stored procedures
- APIs
- Business rules
- IDs
- Data
- Form submission behavior
- Authentication/authorization

Target:

> Same Payroll application + better professional UI.

---

## 13. Button Styling

Use a consistent enterprise UI hierarchy:

- Save → Primary
- Update → Primary
- Process → Primary
- Finalize → Strong primary/action
- Add/New → Primary
- Search/Filter → Secondary
- Back/Cancel → Neutral
- Delete → Danger

Improve button appearance without changing the underlying Controller action, form submission, or JavaScript behavior.

---

## 14. CSS

Before creating CSS:

1. Check existing styles.
2. Check whether a reusable class already exists.
3. Follow the application's existing naming convention.
4. Prefer reusable classes over inline styles.

Avoid:

- Duplicate CSS
- Excessive `!important`
- Broad global selectors
- Unnecessary animations
- Excessive gradients
- Unrelated theme changes

Keep the existing Payroll application's visual identity unless the user explicitly requests a new theme.

---

## 15. JavaScript

Before modifying JavaScript:

- Search for all usages.
- Check element IDs.
- Check AJAX endpoints.
- Check form submission.
- Check validation.
- Check modal behavior.
- Check dependent pages.

Do not remove or rename JavaScript functions without checking their usage.

Do not change JavaScript behavior during a UI-only request.

---

## 16. Security

Never hardcode or expose:

- Passwords
- API keys
- Access tokens
- Connection-string credentials
- Private keys
- Production secrets

Do not disable authentication, authorization, validation, or security checks merely to solve a development problem.

---

## 17. Dependencies

Before adding a NuGet package or frontend library:

1. Check existing packages.
2. Check whether the functionality already exists.
3. Prefer existing project dependencies.
4. Verify .NET/framework compatibility.
5. Add a dependency only when necessary.

Do not introduce a new framework simply because it is newer.

---

## 18. Refactoring

Do not mix unrelated refactoring with payroll feature development.

For example, if asked:

> Improve Payroll Salary Process UI

do not simultaneously:

- Rewrite payroll calculations.
- Rewrite stored procedures.
- Change database structure.
- Replace repositories.
- Change Controllers.
- Introduce a new ORM.
- Change API contracts.

Keep the requested change isolated.

---

## 19. Validation

After changes:

- Build the affected project when possible.
- Check compilation errors.
- Check runtime errors.
- Check browser console errors for UI changes.
- Test the affected workflow.
- Verify existing payroll calculations remain unchanged.
- Verify existing Save/Update/Process/Finalize behavior.
- Verify reports and exports where affected.

Never claim testing was performed unless it was actually performed.

---

## 20. Most Important Rule (Zero Unapproved Logic Changes)

> This is a business-critical Payroll MVC application. Understand the existing code first, preserve existing payroll calculations and data, make the smallest required change, and **NEVER** change business logic, mathematical formulas, calculation methods, or calculation order during UI, data-binding, or code-style tasks without explicit user instruction and approval.

If the Agent discovers:

> "The requested UI cannot work without changing the salary calculation."
OR
> "A calculation formula or logic needs modification, optimization, or correction."

The Agent must **STOP immediately** and ask:

> "The requested change requires modifying the existing salary calculation/business logic in [location]. This will affect payroll calculation results. Do you want me to proceed with this logic change?"

**No calculation or logic change shall be made until the user explicitly approves it and provides the exact required formula.**