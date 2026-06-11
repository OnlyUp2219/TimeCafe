import { Field } from "@fluentui/react-components";
import { useEffect, useMemo, useRef } from "react";
import { DatePicker, defaultDatePickerStrings } from "@fluentui/react-datepicker-compat";
import type { CalendarStrings } from "@fluentui/react-datepicker-compat";

interface DateInputProps {
    value?: Date;
    onChange: (value: Date | undefined) => void;
    disabled?: boolean;
    label?: string;
    placeholder?: string;
    validate?: (value: Date | undefined) => string;
    onValidationChange?: (error: string) => void;
    shouldValidate?: boolean;
    required?: boolean;
    maxDate?: Date;
    maxDateMessage?: string;
    size?: "small" | "medium" | "large";
}

const localizedStrings: CalendarStrings = {
    ...defaultDatePickerStrings,
    days: [
        "Воскресенье",
        "Понедельник",
        "Вторник",
        "Среда",
        "Четверг",
        "Пятница",
        "Суббота"
    ],
    shortDays: ["Вс", "Пн", "Вт", "Ср", "Чт", "Пт", "Сб"],
    months: [
        "Январь",
        "Февраль",
        "Март",
        "Апрель",
        "Май",
        "Июнь",
        "Июль",
        "Август",
        "Сентябрь",
        "Октябрь",
        "Ноябрь",
        "Декабрь"
    ],
    shortMonths: [
        "Янв",
        "Фев",
        "Мар",
        "Апр",
        "Май",
        "Июн",
        "Июл",
        "Авг",
        "Сен",
        "Окт",
        "Ноя",
        "Дек"
    ],
    goToToday: "Перейти к сегодня"
};

const onFormatDate = (date?: Date): string => {
    return !date
        ? ""
        : `${String(date.getDate()).padStart(2, "0")}.${String(date.getMonth() + 1).padStart(2, "0")}.${date.getFullYear()}`;
};

const onParseDateFromString = (dateStr: string): Date | null => {
    const parts = (dateStr || "").trim().split(".");
    if (parts.length === 3) {
        const day = parseInt(parts[0], 10);
        const month = parseInt(parts[1], 10) - 1;
        const year = parseInt(parts[2], 10);
        if (!isNaN(day) && !isNaN(month) && !isNaN(year)) {
            return new Date(year, month, day);
        }
    }
    return null;
};

export const DateInput = ({
                              value,
                              onChange,
                              disabled = false,
                              label = "Дата",
                              placeholder,
                              validate,
                              onValidationChange,
                              shouldValidate = true,
                              required = false,
                              maxDate,
                              maxDateMessage = "Дата не может быть в будущем.",
                              size,
                          }: DateInputProps) => {

    const normalizedMaxDate = useMemo(() => {
        if (!maxDate) return undefined;
        return new Date(maxDate.getFullYear(), maxDate.getMonth(), maxDate.getDate());
    }, [maxDate]);

    const maxDateError = useMemo(() => {
        if (!normalizedMaxDate || !value || Number.isNaN(value.getTime())) return "";
        const normalizedValue = new Date(value.getFullYear(), value.getMonth(), value.getDate());
        return normalizedValue > normalizedMaxDate ? maxDateMessage : "";
    }, [maxDateMessage, normalizedMaxDate, value]);

    const errorMsg = useMemo(() => {
        const customError = validate ? validate(value) : "";
        return customError || maxDateError;
    }, [validate, value, maxDateError]);

    const onValidationChangeRef = useRef(onValidationChange);
    useEffect(() => {
        onValidationChangeRef.current = onValidationChange;
    }, [onValidationChange]);

    useEffect(() => {
        onValidationChangeRef.current?.(errorMsg);
    }, [errorMsg]);

    return (
        <Field
            label={label}
            required={required}
            validationState={shouldValidate && errorMsg ? "error" : undefined}
            validationMessage={shouldValidate ? errorMsg : undefined}
            size={size}
        >
            <DatePicker
                value={value ?? null}
                onSelectDate={(date) => onChange(date ?? undefined)}
                placeholder={placeholder || "ДД.ММ.ГГГГ"}
                disabled={disabled}
                maxDate={maxDate}
                allowTextInput
                formatDate={onFormatDate}
                parseDateFromString={onParseDateFromString}
                firstDayOfWeek={1}
                strings={localizedStrings}
                className="w-full"
                size={size}
            />
        </Field>
    );
};
