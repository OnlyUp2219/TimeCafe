import {StrictMode} from 'react'
import {createRoot} from 'react-dom/client'
//import './index.css'
import App from "./App"
import {FluentProvider, webLightTheme} from '@fluentui/react-components'
import {Provider} from "react-redux";
import {persistor, store} from "./store";
import {PersistGate} from "redux-persist/integration/react";

createRoot(document.getElementById('root')!).render(
    <StrictMode>
        <Provider store={store}>
            <PersistGate loading={null} persistor={persistor}>
                <FluentProvider theme={webLightTheme} style={{width: '100%', height: '100%'}}>
                    <App/>
                </FluentProvider>
            </PersistGate>
        </Provider>
    </StrictMode>
)
