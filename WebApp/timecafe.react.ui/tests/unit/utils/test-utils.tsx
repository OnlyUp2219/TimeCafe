import { render } from "@testing-library/react";
import { BrowserRouter } from "react-router-dom";
import { FluentProvider, webLightTheme } from "@fluentui/react-components";
import { Provider } from "react-redux";
import { configureStore } from "@reduxjs/toolkit";

const createMockStore = () => configureStore({
    reducer: {
        auth: (state = { userId: "user-123", token: null }, action) => state,
        permissions: (state = { permissions: [], isLoaded: true }, action) => state
    }
});

export const renderWithProviders = (ui: React.ReactElement) => {
    return render(
        <Provider store={createMockStore()}>
            <FluentProvider theme={webLightTheme}>
                <BrowserRouter>
                    {ui}
                </BrowserRouter>
            </FluentProvider>
        </Provider>
    );
};
