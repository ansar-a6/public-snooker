# Snooker Club Management System

## 1. Project Overview

### Project name

Snooker Club Management System

### Platform

Desktop application.

### Main purpose

Replace the club's paper-based records with a digital system for:

-   Snooker session management
-   Time-based and frame-based billing
-   Canteen sales
-   Inventory and stock tracking
-   Payments
-   Dispute recording
-   Expenses
-   Revenue and profit reports

### Initial architecture

The initial version should be a local-first desktop application.

Recommended technology:

-   Python
-   PySide6 for the desktop GUI
-   SQLite for local data storage
-   Git for version control
-   Private GitHub repository for source-code backup

The initial version should not require:

-   Cloud hosting
-   A VPS
-   A domain
-   A cloud database
-   Paid APIs
-   Online customer accounts
-   Mobile applications
-   Multi-branch synchronization

------------------------------------------------------------------------

## 2. Users and Roles

The system has two functional roles:

1.  Cashier
2.  Manager

In practice, the owner may perform both roles.

### User-account requirements

-   No separate login system is required for the initial version.
-   No approval workflow is required.
-   No manager approval is required before corrections or adjustments.
-   The application should still clearly separate operational areas such
    as cashier work, inventory, expenses, and reports.

------------------------------------------------------------------------

## 3. Snooker Session Requirements

### 3.1 Customer information

Each session should record:

-   Customer name
-   Table number or table name
-   Session start time
-   Session end time when completed
-   Billing mode
-   Session status
-   Final amount

Only the customer's name is required.

### 3.2 Billing modes

A session uses exactly one billing mode:

-   Time billing
-   Frame billing

Time and frame charges must not be combined in one session.

### 3.3 Time billing

Time billing requirements:

-   Use exact minutes.
-   Record the session start time.
-   Calculate elapsed time using actual clock time.
-   Do not pause the timer.
-   Do not provide a pause feature.
-   The cashier can end the session and proceed to checkout.
-   The final charge is calculated using the configured time rate.

Example:

If a customer plays for 90 minutes, the system must calculate the charge
using 90 minutes.

### 3.4 Frame billing

Frame billing requirements:

-   The cashier records the number of frames played.
-   The system displays the current frame count.
-   The cashier can increase or correct the frame count before payment.
-   The final charge is calculated using the configured frame rate.
-   Frame billing does not use the time-based charge.

### 3.5 Table rules

-   A table can have only one active session.
-   A table with an active session cannot be assigned to another
    session.
-   Available and occupied tables should be visually distinguishable.
-   Ending a session makes the table available again.

### 3.6 Session statuses

Suggested statuses:

-   Available
-   Active
-   Awaiting checkout
-   Completed
-   Cancelled

### 3.7 Draft session editing

Before payment:

-   The cashier can correct the customer name.
-   The cashier can correct the frame count.
-   The cashier can review the billing mode.
-   The cashier can add or edit canteen orders.
-   The bill total must recalculate after changes.

After payment:

-   The completed bill must be locked.
-   The original completed bill must never be directly edited.
-   Any later correction must use a separate adjustment record.

------------------------------------------------------------------------

## 4. Canteen POS Requirements

### 4.1 Canteen purpose

The canteen is inside the club and is managed by the cashier.

Canteen orders should be associated with the relevant customer/session
whenever applicable.

### 4.2 Product information

Each product should support:

-   Product name
-   Selling price
-   Current stock quantity
-   Low-stock threshold
-   Availability status

### 4.3 Canteen order requirements

The cashier should be able to:

-   Search or select products.
-   Add products to an order.
-   Increase or decrease quantities.
-   Remove products from an order.
-   Review the order total.
-   Associate the order with a session/customer.
-   Confirm or cancel the order.

### 4.4 Stock prevention policy

The system must prevent sales when stock is insufficient.

Rules:

-   A product's stock must never become negative.
-   A quantity control must not allow a quantity greater than available
    stock.
-   A product with zero stock cannot be confirmed for sale.
-   If any item in a multi-product order has insufficient stock, the
    entire order must be blocked.
-   The cashier must correct the order or restock the product before
    confirmation.
-   Cancelled orders must not reduce stock.
-   Confirmed orders must reduce stock exactly once.
-   Repeated confirmation must not reduce stock more than once.

### 4.5 Stock warnings

-   Zero stock: show an out-of-stock status.
-   Stock above zero but at or below the low-stock threshold: show a
    low-stock warning.
-   Low stock is a warning only; it does not automatically block a sale
    if enough stock remains.

### 4.6 Order statuses

Suggested statuses:

-   Draft
-   Confirmed
-   Cancelled

------------------------------------------------------------------------

## 5. Inventory Requirements

Inventory is a required part of the system.

### 5.1 Product management

The cashier/manager should be able to:

-   Add a product.
-   Edit product name.
-   Edit selling price.
-   Set the low-stock threshold.
-   View current stock.
-   View product availability.
-   Mark a product inactive if it should no longer be sold.

### 5.2 Stock purchases

A stock purchase should record:

-   Product
-   Quantity purchased
-   Purchase date
-   Purchase cost if tracked
-   Supplier or note if needed

Rules:

-   A purchase increases available stock.
-   The stock increase must be recorded correctly.
-   The purchase should appear in stock history.
-   The purchase should be visible in inventory-related reports.

### 5.3 Stock adjustments

Stock adjustments may be used for:

-   Damaged items
-   Expired items
-   Missing items
-   Counting corrections
-   Manual stock corrections

Each adjustment should record:

-   Product
-   Increase or decrease
-   Quantity
-   Reason
-   Date/time

Rules:

-   A reason is required.
-   Stock cannot be reduced below zero.
-   Every adjustment must appear in stock history.

### 5.4 Stock history

The system should show:

-   Previous stock quantity
-   Change amount
-   New stock quantity
-   Change type
-   Reason or reference
-   Date/time

------------------------------------------------------------------------

## 6. Bills and Checkout

### 6.1 Checkout information

The checkout screen should display:

-   Customer name
-   Table
-   Session billing mode
-   Snooker charge
-   Canteen charge
-   Adjustments, if any
-   Final total
-   Payment method
-   Bank transaction reference, if applicable
-   Dispute option

### 6.2 Payment methods

Supported payment methods:

-   Cash
-   Bank transfer

For bank transfer:

-   Transaction reference or note is optional.
-   The reference should be stored when provided.

### 6.3 Payment rules

-   A bill can be paid after the session ends or is sent to checkout.
-   Payment confirms the bill.
-   Paid bills remain viewable in bill history.
-   No receipt printing is required.
-   A paid bill cannot be directly edited.

### 6.4 Bill statuses

Suggested statuses:

-   Draft
-   Awaiting payment
-   Paid
-   Cancelled
-   Adjusted

------------------------------------------------------------------------

## 7. Dispute Requirements

### 7.1 Dispute purpose

Disputes mainly concern:

-   Number of frames played
-   Session duration
-   Canteen items
-   Amount charged

### 7.2 Dispute behavior

The cashier should be able to record a dispute while allowing payment.

A dispute may include:

-   Dispute reason
-   Description
-   Related bill/session
-   Date/time
-   Optional supporting note

### 7.3 Automatic resolution

When the bill is paid:

-   The dispute is automatically marked as resolved.
-   The original bill remains locked after payment.
-   If a later correction is required, create a separate adjustment.

CCTV may be used as supporting evidence outside the application. CCTV
integration is not required in the initial version.

------------------------------------------------------------------------

## 8. Corrections and Adjustments

### 8.1 Before payment

Before payment, the cashier can:

-   Edit the draft bill.
-   Correct frame quantity.
-   Correct canteen quantities.
-   Remove incorrect items.
-   Add missing items.
-   Review the recalculated total.

### 8.2 After payment

After payment:

-   The original bill is locked.
-   The system must prevent direct editing.
-   Corrections must be recorded separately.
-   The adjustment must include a reason.
-   The original amount and corrected amount should remain visible in
    history.

No approval workflow is required for the initial version.

------------------------------------------------------------------------

## 9. Expenses

Both cashier and manager functions may enter expenses because the owner
may perform both roles.

An expense should record:

-   Date
-   Category
-   Amount
-   Description or note

Possible categories:

-   Electricity
-   Rent
-   Salaries
-   Repairs
-   Cleaning
-   Supplies
-   Maintenance
-   Other

The system should allow:

-   Adding expenses
-   Viewing expenses
-   Filtering expenses by date
-   Editing draft or incorrect expense entries according to the chosen
    correction policy
-   Including expenses in reports

------------------------------------------------------------------------

## 10. Reports

The system should provide reports for a selected date or date range.

### 10.1 Revenue reports

Show:

-   Total revenue
-   Snooker revenue
-   Canteen revenue
-   Number of completed bills
-   Cash payments
-   Bank-transfer payments

### 10.2 Expense reports

Show:

-   Total expenses
-   Expenses by category
-   Expenses by date range

### 10.3 Inventory reports

Show:

-   Current stock
-   Low-stock products
-   Out-of-stock products
-   Stock purchases
-   Stock adjustments
-   Stock movement history

### 10.4 Profit report

The exact profit formula must be confirmed before final implementation.

Possible initial approach:

> Profit = total paid revenue − recorded operating expenses

Inventory purchase costs should be shown separately unless the business
decides that they must be included in the profit calculation.

This decision should be recorded in `docs/decisions.md`.

------------------------------------------------------------------------

## 11. Settings

The Settings area should allow configuration of:

-   Table names/numbers
-   Time billing rate
-   Frame billing rate
-   Currency display
-   Low-stock defaults
-   Product settings
-   Business name
-   Report preferences

Exact rates must not be hardcoded.

------------------------------------------------------------------------

## 12. Dashboard

The dashboard should provide a quick overview of the club.

Suggested dashboard information:

-   Active sessions
-   Available tables
-   Occupied tables
-   Today's revenue
-   Today's expenses
-   Low-stock product count
-   Out-of-stock product count
-   Recent bills
-   Quick actions

Suggested quick actions:

-   Start session
-   Open canteen POS
-   View active sessions
-   Add expense
-   View today's report

------------------------------------------------------------------------

## 13. GUI Blueprint

### 13.1 Main navigation

Suggested navigation:

-   Dashboard
-   Sessions
-   Canteen POS
-   Bills
-   Inventory
-   Expenses
-   Reports
-   Settings

### 13.2 Dashboard screen

Display:

-   Summary cards
-   Table overview
-   Active sessions
-   Low-stock alerts
-   Recent activity
-   Quick-action buttons

### 13.3 Sessions screen

Display:

-   Table cards
-   Table status
-   Customer name
-   Billing mode
-   Timer or frame count
-   Current estimated amount
-   Actions

Actions:

-   Start session
-   Open session
-   Add frames
-   Add canteen order
-   End session
-   Checkout

### 13.4 Start Session screen

Fields:

-   Customer name
-   Table
-   Billing mode
-   Rate display
-   Start button
-   Cancel button

Validation:

-   Customer name is required.
-   Table is required.
-   Table must be available.
-   Billing mode is required.

### 13.5 Active Session screen

For time billing:

-   Customer name
-   Table
-   Start time
-   Current exact elapsed minutes
-   Current estimated charge
-   Add canteen order
-   End session

For frame billing:

-   Customer name
-   Table
-   Frame count
-   Current estimated charge
-   Add frame
-   Correct frame count
-   Add canteen order
-   End session

There must be no pause button.

### 13.6 Canteen POS screen

Display:

-   Product search
-   Product cards/list
-   Selling price
-   Available stock
-   Quantity controls
-   Low-stock status
-   Out-of-stock status
-   Current order
-   Order total
-   Confirm order
-   Cancel order

Validation:

-   Quantity cannot exceed stock.
-   Out-of-stock items cannot be confirmed.
-   Multi-product orders are blocked if any item is insufficient.
-   Confirming an order updates stock exactly once.

### 13.7 Inventory screen

Display:

-   Product list
-   Product name
-   Selling price
-   Current stock
-   Low-stock threshold
-   Status
-   Add product
-   Edit product
-   Purchase stock
-   Adjust stock
-   View stock history

### 13.8 Checkout screen

Display:

-   Customer
-   Table
-   Snooker amount
-   Canteen amount
-   Adjustments
-   Total
-   Payment method
-   Bank transfer reference
-   Dispute option
-   Confirm payment

### 13.9 Bills screen

Sections:

-   Draft bills
-   Awaiting payment
-   Paid bills
-   Disputes
-   Adjustments

Rules:

-   Draft bills can be edited.
-   Paid bills are locked.
-   Adjustments are shown separately.

### 13.10 Expenses screen

Fields:

-   Date
-   Category
-   Amount
-   Description
-   Save
-   Cancel

Display:

-   Expense list
-   Total expenses
-   Date filters
-   Category filters

### 13.11 Reports screen

Controls:

-   Start date
-   End date
-   Report type
-   View report
-   Export later if required

Reports:

-   Revenue
-   Expenses
-   Inventory
-   Profit summary

------------------------------------------------------------------------

## 14. Business Rules

-   BR-01: A table can have only one active session.
-   BR-02: A session uses either time billing or frame billing.
-   BR-03: Time billing uses exact minutes.
-   BR-04: Time billing has no pause feature.
-   BR-05: Frame billing is based on cashier-recorded frames.
-   BR-06: Rates are configurable in Settings.
-   BR-07: A draft bill can be edited before payment.
-   BR-08: A paid bill cannot be directly edited.
-   BR-09: Corrections after payment require a separate adjustment.
-   BR-10: Canteen orders are associated with a customer/session when
    applicable.
-   BR-11: Stock must never become negative.
-   BR-12: A sale is blocked when requested quantity exceeds available
    stock.
-   BR-13: A zero-stock product cannot be sold.
-   BR-14: A multi-product order is blocked if any item has insufficient
    stock.
-   BR-15: Cancelled orders do not change stock.
-   BR-16: Confirmed orders reduce stock exactly once.
-   BR-17: Paid bills with disputes are automatically marked resolved.
-   BR-18: No receipt printing is required.
-   BR-19: Cash and bank transfer are supported.
-   BR-20: Bank transfer references are optional.
-   BR-21: Stock adjustments require a reason.
-   BR-22: Stock adjustments cannot reduce stock below zero.
-   BR-23: Both operational roles may enter expenses.
-   BR-24: Exact snooker rates and business pricing must be
    configurable.
-   BR-25: The profit formula must be confirmed before final
    implementation.

------------------------------------------------------------------------

## 15. Acceptance Tests

### Inventory tests

-   INV-01: Add a new product.
-   INV-02: Purchase stock and verify that stock increases correctly.
-   INV-03: Confirm a valid sale and verify that stock decreases
    correctly.
-   INV-04: Attempt a sale exceeding stock and verify that it is
    blocked.
-   INV-05: Verify that quantity cannot exceed available stock.
-   INV-06: Attempt to sell a zero-stock product and verify that it is
    blocked.
-   INV-07: Create a multi-product order with one insufficient item and
    verify that the entire order is blocked.
-   INV-08: Correct the insufficient quantity and verify that
    confirmation becomes possible.
-   INV-09: Verify that stock reaching zero displays out-of-stock.
-   INV-10: Verify that low-stock warnings appear correctly.
-   INV-11: Verify that stock adjustments appear in history.
-   INV-12: Verify that stock cannot be reduced below zero.
-   INV-13: Restock an out-of-stock product and verify that it becomes
    available.
-   INV-14: Cancel an order and verify that stock does not change.
-   INV-15: Confirm an order once and verify that stock changes only
    once.

### Session tests

-   SES-01: Start a time-based session and verify the timer.
-   SES-02: Start a frame-based session and verify the frame counter.
-   SES-03: Verify correct billing for a 90-minute session.
-   SES-04: Verify that no pause feature exists.
-   SES-05: Add and correct frame counts.
-   SES-06: Edit a draft bill and verify that the total recalculates.
-   SES-07: Pay a bill and verify that it becomes locked.
-   SES-08: Attempt to directly edit a paid bill and verify that it is
    prevented.
-   SES-09: Create a post-payment adjustment and verify that history is
    preserved.

### Payment tests

-   PAY-01: Complete a cash payment.
-   PAY-02: Complete a bank-transfer payment with a reference.
-   PAY-03: Complete a bank-transfer payment without a reference.
-   PAY-04: Record a dispute and pay the bill; verify automatic
    resolution.
-   PAY-05: Verify that no receipt-printing workflow is required.

### Expense and report tests

-   REP-01: Add an expense.
-   REP-02: View expenses by date range.
-   REP-03: View total revenue.
-   REP-04: View snooker revenue separately.
-   REP-05: View canteen revenue separately.
-   REP-06: View inventory purchases and stock movement.
-   REP-07: View the profit summary using the confirmed profit formula.

------------------------------------------------------------------------

## 16. Validation and Error Messages

Suggested messages:

-   `Customer name is required.`
-   `Please select a table.`
-   `This table already has an active session.`
-   `Please select a billing mode.`
-   `Frame count cannot be negative.`
-   `Quantity cannot exceed available stock.`
-   `This product is out of stock.`
-   `This order cannot be confirmed because one or more items have insufficient stock.`
-   `Please correct the order before confirming.`
-   `A reason is required for stock adjustment.`
-   `Stock cannot be reduced below zero.`
-   `Paid bills cannot be edited directly.`
-   `Create an adjustment to correct a paid bill.`
-   `Please select a payment method.`
-   `Please enter a valid amount.`
-   `Please enter a reason for the dispute.`

------------------------------------------------------------------------

## 17. Recommended Development Plan

### Phase 1: Project foundation

Build:

-   Main application window
-   Navigation
-   Placeholder screens
-   Settings screen
-   Basic styling

### Phase 2: Snooker sessions

Build:

-   Table management
-   Start session
-   Time billing
-   Frame billing
-   Active sessions
-   End session
-   Draft bill

### Phase 3: Canteen POS

Build:

-   Product list
-   Product selection
-   Order summary
-   Quantity controls
-   Stock validation
-   Order confirmation
-   Order cancellation

### Phase 4: Checkout and payments

Build:

-   Combined bill
-   Cash payment
-   Bank transfer
-   Transaction reference
-   Dispute recording
-   Paid-bill locking
-   Adjustments

### Phase 5: Inventory

Build:

-   Product management
-   Stock purchases
-   Stock adjustments
-   Low-stock warnings
-   Stock history

### Phase 6: Expenses and reports

Build:

-   Expense entry
-   Revenue reports
-   Expense reports
-   Inventory reports
-   Profit summary

### Phase 7: Testing and packaging

Build:

-   Acceptance tests
-   Error handling
-   Backup and restore
-   Windows executable
-   Installation instructions
-   User guide

------------------------------------------------------------------------

## 18. Codex Development Rules

Codex should follow these rules:

-   Read this blueprint before making changes.
-   Do not implement the entire system in one step.
-   Work in small, testable milestones.
-   Do not change confirmed business rules without asking.
-   Do not design the database unless explicitly requested.
-   Do not add cloud services in the initial version.
-   Do not add paid APIs.
-   Do not add AI features unless explicitly requested.
-   Prefer local-first development.
-   Prefer Python and PySide6.
-   Keep the code beginner-maintainable.
-   Explain files created and how to run the application.
-   Provide manual test steps after each feature.
-   Do not use real customer or financial data during development.
-   Keep the GitHub repository private.

------------------------------------------------------------------------

## 19. Open Decisions

The following decisions should be finalized before the relevant
implementation:

1.  Exact time billing rate.
2.  Exact frame billing rate.
3.  Currency display.
4.  Table names/numbers.
5.  Low-stock default threshold.
6.  Whether purchase cost per item is tracked.
7.  Whether inventory purchases are deducted from profit.
8.  Exact profit formula.
9.  Backup and restore method.
10. Whether reports need export to CSV/PDF in version 1.

------------------------------------------------------------------------

## 20. Recommended First Codex Prompt

Read `docs/requirements.md` and the project `README.md`.

Before writing code:

1.  Summarize the confirmed requirements.
2.  List unresolved decisions.
3.  Propose a clean project structure.
4.  Propose the first implementation milestone.

Do not write code yet. Do not design the database yet. Do not add cloud
services. Do not change confirmed business rules without asking.
