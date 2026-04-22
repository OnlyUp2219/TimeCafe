import {
    Button,
    Popover,
    PopoverSurface,
    PopoverTrigger,
    makeStyles,
    shorthands,
    tokens,
    Tooltip
} from "@fluentui/react-components";
import { FC, useState } from "react";

const useStyles = makeStyles({
    grid: {
        display: "grid",
        gridTemplateColumns: "repeat(6, 1fr)",
        gap: "4px",
        maxHeight: "200px",
        overflowY: "auto",
        padding: "8px",
        ...shorthands.padding("8px"),
    },
    emojiButton: {
        fontSize: "20px",
        minWidth: "36px",
        height: "36px",
        padding: "0",
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        cursor: "pointer",
        backgroundColor: "transparent",
        ...shorthands.border("none"),
        ...shorthands.borderRadius(tokens.borderRadiusMedium),
        ":hover": {
            backgroundColor: tokens.colorNeutralBackground1Hover,
        }
    },
    sectionTitle: {
        gridColumn: "1 / -1",
        fontSize: "12px",
        fontWeight: "bold",
        color: tokens.colorNeutralForeground3,
        marginTop: "8px",
        marginBottom: "4px",
        paddingLeft: "4px",
    }
});

const EMOJI_LIST = [
    { label: "Популярные", emojis: ["🔥", "⭐", "🚀", "💎", "⚡", "✨", "🎉", "❤️", "🍀", "👑", "🍕", "☕"] },
    { label: "Смайлы", emojis: ["😀", "😎", "🤩", "🥳", "😇", "🤔", "😌", "😍", "😜", "🙃", "🫠", "👽"] },
    { label: "Животные", emojis: ["🐱", "🐶", "🦁", "🦊", "🐼", "🐨", "🐸", "🐙", "🦋", "🦄", "🐲", "🐢"] },
    { label: "Объекты", emojis: ["💻", "🎮", "🎸", "📚", "🎨", "🎬", "🎤", "🎧", "📷", "💡", "🔑", "🎁"] },
];

interface Props {
    selectedEmoji?: string;
    onSelect: (emoji: string) => void;
    size?: "small" | "medium" | "large";
}

export const EmojiPicker: FC<Props> = ({ selectedEmoji, onSelect, size = "medium" }) => {
    const styles = useStyles();
    const [open, setOpen] = useState(false);

    const handleSelect = (emoji: string) => {
        onSelect(emoji);
        setOpen(false);
    };

    return (
        <Popover open={open} onOpenChange={(_, d) => setOpen(d.open)} positioning="below-start">
            <PopoverTrigger disableButtonEnhancement>
                <Button size={size} style={{ fontSize: "20px" }}>
                    {selectedEmoji || "😀"}
                </Button>
            </PopoverTrigger>
            <PopoverSurface className={styles.grid}>
                {EMOJI_LIST.map((section) => (
                    <div key={section.label} style={{ display: "contents" }}>
                        <div className={styles.sectionTitle}>{section.label}</div>
                        {section.emojis.map((emoji) => (
                            <Tooltip content={emoji} relationship="label" key={emoji}>
                                <button
                                    className={styles.emojiButton}
                                    onClick={() => handleSelect(emoji)}
                                    type="button"
                                >
                                    {emoji}
                                </button>
                            </Tooltip>
                        ))}
                    </div>
                ))}
            </PopoverSurface>
        </Popover>
    );
};
