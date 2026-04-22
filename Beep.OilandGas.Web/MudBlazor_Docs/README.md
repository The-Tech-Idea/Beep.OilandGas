# MudBlazor Local Reference

Purpose: make the local MudBlazor reference set the first stop before changing `Beep.OilandGas.Web` UI markup, layout, navigation, dialogs, grids, or theme behavior.

## Required Workflow

1. Start here before editing `.razor`, layout, theme, dialog, grid, navigation, tab, stepper, or shared-component files.
2. Read the matching local component file before changing markup or component parameters.
3. Do not guess MudBlazor parameter names or provider behavior when a local doc exists.
4. Keep the repo UI rules aligned with these docs: use `Color.*`, MudBlazor spacing utilities, shared components, and typed-client-backed page flows.

## Read First For Shell Or Theme Changes

- `Layouts.txt`
- `Theme.txt`
- `Services.txt`
- `AppBar.txt`
- `Drawer.txt`
- `Container.txt`
- `Grid.txt`
- `PopOver.txt`

## Read By UI Need

### Navigation and workflow

- `NavMenu.txt`
- `Menus.txt`
- `Tabs.txt`
- `Stepper.txt`
- `Tree.txt`

### Forms and actions

- `Button.txt`
- `Dialog.txt`
- `MessageBox.txt`
- `Alert.txt`
- `Progress.txt`

### Data-heavy pages

- `DataGrid.txt`
- `DropZone.txt`
- `Chips.txt`
- `ChipSet.txt`
- `Badge.txt`

### Theme and infrastructure

- `Theme.txt`
- `Services.txt`
- `Localization.txt`
- `ParameterState.txt`

## Planning Use

When implementing the web modernization plan:

- Phase 6: use these docs before changing shell, drawer, nav, shared layout, or page/component ownership.
- Phase 7: use these docs before creating or normalizing workbenches, dialogs, result cards, grids, tabs, and steppers.
- Later phases: use these docs before adding workflow pages, admin pages, dashboards, or engineer workbenches.
