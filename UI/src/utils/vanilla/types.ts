// File: UI/src/utils/vanilla/types.ts
// Purpose: Shared TypeScript prop types for vanilla CS2 UI resolver entries.

import type { Color, FocusKey, Theme, UniqueFocusKey } from "cs2/bindings";
import type {
    BalloonDirection,
    DropdownItemProps,
    InfoRowProps,
    InfoSectionProps,
    InputAction,
    ScrollableProps,
} from "cs2/ui";
import type { CSSProperties, HTMLAttributes, PropsWithChildren, ReactNode } from "react";

// Note: UI/types/bindings.d.ts exposes the widget-binding model for ColorField
// (`value`, `hdr`, `showAlpha`). The vanilla React `color-field.tsx` module has
// a different prop surface, so this file keeps a separate component prop type.
export type VanillaColorFieldProps = {
    focusKey?: FocusKey | null;
    disabled?: boolean;
    value?: Color;
    className?: string;
    selectAction?: InputAction | string;
    alpha?: boolean;
    popupDirection?: BalloonDirection;
    hideHint?: boolean;
    colorWheel?: boolean;
    hexInput?: boolean;
    onChange?: (value: Color) => void;
    onClick?: (event: any) => void;
    onMouseEnter?: (event: any) => void;
    onMouseLeave?: (event: any) => void;
    onOpenPicker?: () => void;
    onClosePicker?: (event?: any) => void;
};

export type VanillaHsvaColor = {
    h: number;
    s: number;
    v: number;
    a: number;
};

export type VanillaColorPickerSliderMode = {
    Hsv: "Hsv";
    RgbFloat: "RgbFloat";
    RgbByte: "RgbByte";
};

export type VanillaColorPickerProps = {
    focusKey?: FocusKey | null;
    color: VanillaHsvaColor;
    alpha?: boolean;
    colorWheel?: boolean;
    sliderTextInput?: boolean;
    preview?: unknown;
    mode?: VanillaColorPickerSliderMode[keyof VanillaColorPickerSliderMode];
    hexInput?: boolean;
    allowFocusExit?: boolean;
    onChange?: (value: VanillaHsvaColor) => void;
};

export type VanillaPanelProps = {
    focusKey?: FocusKey | null;
    header?: ReactNode;
    footer?: ReactNode;
    theme?: Theme | Record<string, unknown>;
    className?: string;
    contentClassName?: string;
    children?: ReactNode;
    [key: string]: any;
};

export type VanillaSliderProps = {
    focusKey?: FocusKey | null;
    value: number;
    start: number;
    end: number;
    gamepadStep?: number;
    disabled?: boolean;
    vertical?: boolean;
    sounds?: boolean;
    thumb?: any;
    theme?: Theme;
    className?: string;
    style?: CSSProperties;
    children?: ReactNode;
    noFill?: boolean;
    valueTransformer?: (value: number, start: number, end: number) => number;
    onChange?: (value: number) => void;
    onDragStart?: () => void;
    onDragEnd?: () => void;
    onMouseOver?: () => void;
    onMouseLeave?: () => void;
};

export type VanillaToolButtonProps = {
    focusKey?: UniqueFocusKey | null;
    src?: string;
    selected?: boolean;
    multiSelect?: boolean;
    disabled?: boolean;
    tooltip?: ReactNode | null;
    selectSound?: any;
    uiTag?: string;
    className?: string;
    children?: ReactNode;
    onSelect?: (value: any) => any;
} & HTMLAttributes<any>;

export type VanillaStepToolButtonProps = {
    focusKey?: UniqueFocusKey | null;
    selectedValue: number;
    values: number[];
    tooltip?: string | null;
    uiTag?: string;
    onSelect?: (value: any) => any;
} & HTMLAttributes<any>;

export type VanillaSectionProps = {
    title?: string | null;
    uiTag?: string;
    focusKey?: UniqueFocusKey | null;
    children: ReactNode;
};

export type VanillaDescriptionTooltipProps = {
    title?: string | null;
    description?: string | null;
    content?: ReactNode | null;
    direction?: any;
    children: ReactNode;
};

export type VanillaCheckboxProps = {
    checked?: boolean;
    disabled?: boolean;
    theme?: any;
    className?: string;
    [key: string]: any;
};

export type VanillaInfoLinkProps = {
    icon?: string;
    tooltip?: ReactNode;
    uppercase?: boolean;
    onSelect?: (value: any) => any;
    children?: ReactNode;
};

export type VanillaPageSelectorProps = {
    pages: number;
    selected: number;
    onSelect?: (value: number) => any;
};

export type VanillaPageSwitcherProps = {
    activePage: number;
    transitionStyles?: any;
    className?: any;
    children?: ReactNode;
};

export type VanillaDropdownItemComponentProps = PropsWithChildren<DropdownItemProps<any>>;
export type VanillaInfoSectionComponentProps = PropsWithChildren<InfoSectionProps>;
export type VanillaInfoRowComponentProps = PropsWithChildren<InfoRowProps>;
export type VanillaScrollableComponentProps = PropsWithChildren<ScrollableProps>;
export type VanillaTextInputProps = Record<string, any>;

export type VanillaUseUniqueFocusKey = (
    focusKey: FocusKey,
    debugName: string,
) => UniqueFocusKey | null;

export type VanillaThemeValue = Theme & Record<string, string>;
