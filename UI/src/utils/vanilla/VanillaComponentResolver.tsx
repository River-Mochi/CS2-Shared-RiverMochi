// File: UI/src/utils/vanilla/VanillaComponentResolver.tsx
// Purpose: Typed resolver for vanilla CS2 UI modules loaded from the game registry.

import type { FocusKey, UniqueFocusKey } from "cs2/bindings";
import type { ModuleRegistry } from "cs2/modding";
import type { FC } from "react";
import { VANILLA_COMPONENT_MODULES, type VanillaComponentModuleName } from "./components";
import { VANILLA_FOCUS_MODULES, type VanillaFocusModuleName } from "./focus";
import { VANILLA_THEME_MODULES, type VanillaThemeModuleName } from "./themes";
import type {
    VanillaCheckboxProps,
    VanillaColorPickerProps,
    VanillaColorPickerSliderMode,
    VanillaColorFieldProps,
    VanillaDescriptionTooltipProps,
    VanillaDropdownItemComponentProps,
    VanillaInfoLinkProps,
    VanillaInfoRowComponentProps,
    VanillaInfoSectionComponentProps,
    VanillaPanelProps,
    VanillaPageSelectorProps,
    VanillaPageSwitcherProps,
    VanillaScrollableComponentProps,
    VanillaSectionProps,
    VanillaSliderProps,
    VanillaStepToolButtonProps,
    VanillaTextInputProps,
    VanillaThemeValue,
    VanillaToolButtonProps,
    VanillaUseUniqueFocusKey,
} from "./types";

// Shared vanilla-module library for HoverColors and future CS2 mods.
//
// This combines two good patterns from community:
// - yenyang's "how to in the UI" comments
// - Luca's scalable grouped registries for components/themes/focus helpers
// - River-Mochi notes and tweaks
//
// Practical notes for future:
// 1. Open the in-game UI dev tools at http://localhost:9444/
// 2. In Sources -> index.js, pretty-print if needed
// 3. Search for the TSX/SCSS path wanted
// 4. Add it to one of the maps below and expose a typed getter
//
// Note: `UI/types/bindings.d.ts` is helpful for binding/widget payloads,
// but the vanilla React module props can differ from the binding model.

type VanillaModuleName =
    | VanillaComponentModuleName
    | VanillaThemeModuleName
    | VanillaFocusModuleName;

export class VanillaComponentResolver {
    public static get instance(): VanillaComponentResolver {
        return this._instance!!;
    }

    private static _instance?: VanillaComponentResolver;

    public static setRegistry(moduleRegistry: ModuleRegistry) {
        this._instance = new VanillaComponentResolver(moduleRegistry);
    }

    private readonly registryData: ModuleRegistry;
    private readonly cache: Partial<Record<VanillaModuleName, unknown>> = {};

    private constructor(moduleRegistry: ModuleRegistry) {
        this.registryData = moduleRegistry;
    }

    private resolve<T>(entry: VanillaModuleName, path: string, exportName: string): T {
        const cached = this.cache[entry];
        if (cached !== undefined) {
            return cached as T;
        }

        const moduleValue = this.registryData.registry.get(path);
        if (moduleValue == null || !(exportName in moduleValue)) {
            throw new Error(
                `[VanillaComponentResolver] Failed to resolve ${entry} from ${path} -> ${exportName}.`,
            );
        }

        const resolved = (moduleValue as Record<string, unknown>)[exportName] as T;
        this.cache[entry] = resolved;
        return resolved;
    }

    public resolveComponent<T = FC<any>>(entry: VanillaComponentModuleName): T {
        const [path, exportName] = VANILLA_COMPONENT_MODULES[entry];
        return this.resolve<T>(entry, path, exportName);
    }

    public resolveTheme<T = VanillaThemeValue>(entry: VanillaThemeModuleName): T {
        const [path, exportName] = VANILLA_THEME_MODULES[entry];
        return this.resolve<T>(entry, path, exportName);
    }

    public resolveFocus<T = unknown>(entry: VanillaFocusModuleName): T {
        const [path, exportName] = VANILLA_FOCUS_MODULES[entry];
        return this.resolve<T>(entry, path, exportName);
    }

    public get Panel(): FC<VanillaPanelProps> {
        return this.resolveComponent("Panel");
    }

    public get Section(): FC<VanillaSectionProps> {
        return this.resolveComponent("Section");
    }

    public get ToolButton(): FC<VanillaToolButtonProps> {
        return this.resolveComponent("ToolButton");
    }

    public get StepToolButton(): FC<VanillaStepToolButtonProps> {
        return this.resolveComponent("StepToolButton");
    }

    public get Checkbox(): FC<VanillaCheckboxProps> {
        return this.resolveComponent("Checkbox");
    }

    public get DescriptionTooltip(): FC<VanillaDescriptionTooltipProps> {
        return this.resolveComponent("DescriptionTooltip");
    }

    public get ColorField(): FC<VanillaColorFieldProps> {
        return this.resolveComponent("ColorField");
    }

    public get ColorPicker(): FC<VanillaColorPickerProps> {
        return this.resolveComponent("ColorPicker");
    }

    public get ColorPickerSliderMode(): VanillaColorPickerSliderMode {
        return this.resolveComponent("ColorPickerSliderMode");
    }

    public get Slider(): FC<VanillaSliderProps> {
        return this.resolveComponent("Slider");
    }

    public get InfoSection(): FC<VanillaInfoSectionComponentProps> {
        return this.resolveComponent("InfoSection");
    }

    public get InfoRow(): FC<VanillaInfoRowComponentProps> {
        return this.resolveComponent("InfoRow");
    }

    public get InfoLink(): FC<VanillaInfoLinkProps> {
        return this.resolveComponent("InfoLink");
    }

    public get PageSelector(): FC<VanillaPageSelectorProps> {
        return this.resolveComponent("PageSelector");
    }

    public get PageSwitcher(): FC<VanillaPageSwitcherProps> {
        return this.resolveComponent("PageSwitcher");
    }

    public get Page(): FC<any> {
        return this.resolveComponent("Page");
    }

    public get Scrollable(): FC<VanillaScrollableComponentProps> {
        return this.resolveComponent("Scrollable");
    }

    public get TextInput(): FC<VanillaTextInputProps> {
        return this.resolveComponent("TextInput");
    }

    public get DropdownItem(): FC<VanillaDropdownItemComponentProps> {
        return this.resolveComponent("DropdownItem");
    }

    public get toolButtonTheme(): VanillaThemeValue {
        return this.resolveTheme("toolButtonTheme");
    }

    public get mouseToolOptionsTheme(): VanillaThemeValue {
        return this.resolveTheme("mouseToolOptionsTheme");
    }

    public get descriptionTooltipTheme(): VanillaThemeValue {
        return this.resolveTheme("descriptionTooltipTheme");
    }

    public get colorFieldTheme(): VanillaThemeValue {
        return this.resolveTheme("colorFieldTheme");
    }

    public get commonDropdownTheme(): VanillaThemeValue {
        return this.resolveTheme("commonDropdownTheme");
    }

    public get menuDropdownTheme(): VanillaThemeValue {
        return this.resolveTheme("menuDropdownTheme");
    }

    public get checkboxTheme(): VanillaThemeValue {
        return this.resolveTheme("checkboxTheme");
    }

    public get statisticsCheckboxTheme(): VanillaThemeValue {
        return this.resolveTheme("statisticsCheckboxTheme");
    }

    public get toolbarFeatureButtonTheme(): VanillaThemeValue {
        return this.resolveTheme("toolbarFeatureButtonTheme");
    }

    public get roundHighlightButtonTheme(): VanillaThemeValue {
        return this.resolveTheme("roundHighlightButtonTheme");
    }

    public get infoRowTheme(): VanillaThemeValue {
        return this.resolveTheme("infoRowTheme");
    }

    public get panelBaseTheme(): VanillaThemeValue {
        return this.resolveTheme("panelBaseTheme");
    }

    public get panelTheme(): VanillaThemeValue {
        return this.resolveTheme("panelTheme");
    }

    public get infoviewMenuTheme(): VanillaThemeValue {
        return this.resolveTheme("infoviewMenuTheme");
    }

    public get pageSelectorTheme(): VanillaThemeValue {
        return this.resolveTheme("pageSelectorTheme");
    }

    public get whatsNewPageTheme(): VanillaThemeValue {
        return this.resolveTheme("whatsNewPageTheme");
    }

    public get horizontalTransitionTheme(): VanillaThemeValue {
        return this.resolveTheme("horizontalTransitionTheme");
    }

    public get whatsNewTabTheme(): VanillaThemeValue {
        return this.resolveTheme("whatsNewTabTheme");
    }

    public get toolOptionsPanelTheme(): VanillaThemeValue {
        return this.resolveTheme("toolOptionsPanelTheme");
    }

    public get assetCategoryTabItemTheme(): VanillaThemeValue {
        return this.resolveTheme("assetCategoryTabItemTheme");
    }

    public get assetCategoryTabBarTheme(): VanillaThemeValue {
        return this.resolveTheme("assetCategoryTabBarTheme");
    }

    public get itemGridTheme(): VanillaThemeValue {
        return this.resolveTheme("itemGridTheme");
    }

    public get textInputTheme(): VanillaThemeValue {
        return this.resolveTheme("textInputTheme");
    }

    public get FOCUS_DISABLED(): UniqueFocusKey {
        return this.resolveFocus("FOCUS_DISABLED");
    }

    public get FOCUS_AUTO(): UniqueFocusKey {
        return this.resolveFocus("FOCUS_AUTO");
    }

    public get useUniqueFocusKey(): VanillaUseUniqueFocusKey {
        return this.resolveFocus("useUniqueFocusKey");
    }
}

// Short alias kept because a lot of CS2 UI examples use this name.
export const VanillaResolver = VanillaComponentResolver;
