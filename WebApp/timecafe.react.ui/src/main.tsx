import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import App from "@app/App";
import { FluentProvider, webDarkTheme, webLightTheme } from "@fluentui/react-components";
import { Provider } from "react-redux";
import { persistor, store } from "@store";
import { PersistGate } from "redux-persist/integration/react";
import { useAppSelector } from "@store/hooks";
import "./index.css"

const ThemeWrapper = ({ children }: { children: React.ReactNode }) => {
    const theme = useAppSelector((state) => state.ui.theme);
    const selectedTheme = theme === "dark" ? webDarkTheme : webLightTheme;
    
    if (theme === "dark") {
        document.documentElement.classList.add("dark");
    } else {
        document.documentElement.classList.remove("dark");
    }

    return (
        <FluentProvider theme={selectedTheme} className="FluentProvider">
            {children}
        </FluentProvider>
    );
};

createRoot(document.getElementById('root')!).render(
    <StrictMode>
        <Provider store={store}>
            <PersistGate loading={null} persistor={persistor}>
                <ThemeWrapper>
                    <App />
                </ThemeWrapper>
            </PersistGate>
        </Provider>
    </StrictMode>
)
